# ReportGenerator — Technical Design Document

## 1. Overview
`ReportGenerator` is an enterprise-grade, asynchronous report generation system. Users submit a
report request from an Angular UI; the request is queued via Azure Service Bus and processed by a
background Worker service, which queries SQL Server, builds an Excel file (ClosedXML), uploads it
to Azure Blob Storage, and updates the request status. The UI polls for status and exposes a
download link once the report is ready.

## 2. Architecture

```
Angular UI --(HTTP)--> API (.NET 8) --(insert)--> SQL Server (ReportRequests)
                              |
                              +--(publish)--> Azure Service Bus queue
                                                        |
                                                        v
                                          Worker Service (BackgroundService)
                                                        |
                          +----------------------------+----------------------------+
                          v                                                         v
                  SQL Server (stored proc query)                     Azure Blob Storage (upload .xlsx)
                          |                                                         |
                          +---------------------> ReportRequests (Status, BlobUrl) <+
                                                        ^
                                                        |
                                     Angular UI polls GET /api/reports every 15s
```

Pattern: **asynchronous request/response with message queue**, decoupling fast API responses from
slow report-generation work.

## 3. Solution Structure (Clean Architecture)

```
ReportGenerator.slnx
database/
  01_CreateTables.sql         -> ReportRequests, Employees, Departments, Salaries
  02_SampleData.sql           -> sample employees + salaries
  03_StoredProcedure.sql      -> usp_GenerateEmployeeReport
  04_AddDateRangeColumns.sql  -> adds StartDate/EndDate to ReportRequests
src/
  ReportGenerator.Domain          (entities, enums)
  ReportGenerator.Application     (DTOs, interfaces, MediatR commands/queries - CQRS)
  ReportGenerator.Infrastructure  (EF Core, Service Bus publisher, Blob storage, repositories)
  ReportGenerator.API             (Web API controllers, DI, Program.cs)
  ReportGenerator.Worker          (Background worker: Service Bus consumer, Excel/ClosedXML, blob upload)
  ReportGenerator.Angular         (Angular 20 single-page UI)
```

Layer dependency direction: `Domain <- Application <- Infrastructure/API/Worker`. Business logic
in `Application` has no dependency on EF Core, Service Bus SDK, or ASP.NET.

## 4. Components

### 4.1 Angular UI (`ReportGenerator.Angular`)
- Angular 20 standalone app, Angular Material table.
- Form fields: Report Name, Requested By, Start Date, End Date.
- On submit: `POST /api/reports/request`.
- Grid auto-refreshes every 15s via RxJS interval, calling `GET /api/reports`.
- Download link appears only when `Status = Completed`, linking to
  `GET /api/reports/download/{id}`.
- Requires `polyfills: ["zone.js"]` in `angular.json` build options (Angular 17+ `application`
  builder does not auto-include zone.js).

### 4.2 API (`ReportGenerator.API`)
- ASP.NET Core Web API using **MediatR** for CQRS — controllers dispatch Commands/Queries to
  handlers instead of containing business logic.
- `POST /api/reports/request` → `CreateReportRequestCommand`:
  1. Insert row into `ReportRequests` with `Status = "Not Processed"`.
  2. Publish `ReportRequestMessage` (RequestId, ReportName, StartDate, EndDate) to Service Bus.
  3. Return immediately (does not wait for processing).
- `GET /api/reports` / `GET /api/reports/{id}` — read current status for UI polling.
- `GET /api/reports/download/{id}` — redirects to the row's `BlobUrl`.

### 4.3 Azure Service Bus (decoupling layer)
- Namespace + queue `report-generation-queue` (Basic tier is sufficient — queues only).
- API is the **producer** (`ServiceBusPublisher`), Worker is the **consumer**.
- Decouples request submission from processing so the API responds instantly and the Worker can
  process independently (and be temporarily unavailable without losing requests).

### 4.4 Worker Service (`ReportGenerator.Worker`)
- .NET 8 `BackgroundService`, no HTTP endpoints.
- `Worker.cs` creates a `ServiceBusProcessor` (`MaxConcurrentCalls = 1`, `AutoCompleteMessages =
  false`) and listens continuously.
- Per message, `ReportProcessingService.ProcessAsync()`:
  1. Load `ReportRequest` by `RequestId`.
  2. Set `Status = "In Progress"`.
  3. Call `usp_GenerateEmployeeReport` (via ADO.NET) with `@IsActiveOnly`, `@StartDate`, `@EndDate`.
  4. Generate `.xlsx` in-memory using **ClosedXML**.
  5. Upload to Blob Storage via `BlobStorageService.UploadAsync()` (auto-creates `reports`
     container if missing).
  6. Update row: `Status = "Completed"`, `BlobUrl`, `ProcessedDate`.
  7. On exception: `Status = "Failed"`, `FailureReason = ex.Message` (currently swallowed — see
     §6 Known Limitations).

### 4.5 Azure Blob Storage
- General-purpose v2 storage account, container `reports`.
- Chosen over SQL storage for large binary files: cheaper, durable, and lets clients download
  directly via URL without routing large payloads through the API.

### 4.6 SQL Server
Tables: `Employees`, `Departments`, `Salaries` (source data) and `ReportRequests` (state machine:
`Id, ReportName, RequestedBy, RequestedDate, Status, StartDate, EndDate, FileName, BlobUrl,
FailureReason, ProcessedDate, CreatedDate`).

Stored procedure `usp_GenerateEmployeeReport(@IsActiveOnly, @StartDate, @EndDate)` joins
Employees + Departments + latest Salary per employee (via `ROW_NUMBER()` on `EffectiveDate`),
filtered by active flag and joining-date range.

## 5. Design Rationale

| Problem | Solution |
|---|---|
| API must respond fast, not block on report generation | Service Bus queue decouples submission from processing |
| Report generation is slow/resource-heavy | Runs in a separate Worker process |
| Large binary files need storage | Blob Storage, not SQL |
| UI needs async progress visibility | Polling `GET /api/reports` every 15s |
| Worker crash mid-processing | Message lock (`AutoCompleteMessages=false`) expires, Service Bus redelivers |
| Maintainability/testability | Clean Architecture + MediatR CQRS separates business logic from I/O |

## 6. Failure Handling & Known Limitations

Current retry behavior (see `Worker.cs`, `ServiceBusOptions`, `ReportProcessingService.cs`):

| Failure type | Retries? | End state |
|---|---|---|
| Report generation logic fails (SQL/Blob/Excel error) | No — exception is caught and swallowed inside `ReportProcessingService` | `Status = "Failed"`, message completed and removed from queue |
| Malformed message (JSON parse error) | No | Immediately dead-lettered (`DeadLetterMessageAsync`) |
| Worker process crashes mid-processing | Yes — Service Bus redelivers after lock expiry (default 1 min) | Retried up to queue's Max Delivery Count (default 10), then dead-lettered |

**Gap:** the most common failure case (transient SQL/Blob errors) currently gets zero automatic
retries because it's caught and marked `Failed` rather than rethrown.

Recommended improvements (not yet implemented):
1. Distinguish transient vs. permanent exceptions; rethrow transient ones so Service Bus retry
   applies.
2. Add a `RetryCount` column to `ReportRequests`; only mark `Failed` after N attempts.
3. Confirm/tune Max Delivery Count on the queue (Azure Portal → Queue → Max delivery count).
4. Monitor the Dead-Letter Queue via Azure Monitor alert on `DeadletteredMessages > 0`.
5. Add a manual "Retry" action in the UI for `Failed` rows (re-publish the same message).
6. Add exponential backoff for known-transient exceptions.

## 7. Configuration & Secrets
- `ConnectionStrings:DefaultConnection` (SQL Server) — in `appsettings.json` (local Windows Auth,
  no password needed).
- `ServiceBus:ConnectionString` and `BlobStorage:ConnectionString` — stored in **.NET User
  Secrets** (`dotnet user-secrets`), not committed to source control. Both API and Worker read the
  same values since both interact with Service Bus/Blob Storage.
- `QueueName` (`report-generation-queue`) and `ContainerName` (`reports`) remain in
  `appsettings.json` — not secrets, just configuration.

## 8. API Endpoints

| Method | Route | Purpose |
|---|---|---|
| POST | `/api/reports/request` | Create a report request, publish to queue |
| GET | `/api/reports` | List all requests (used for UI polling) |
| GET | `/api/reports/{id}` | Get a single request's status |
| GET | `/api/reports/download/{id}` | Redirect to the completed report's Blob URL |

## 9. Local Development
1. Run `database/01_CreateTables.sql`, `02_SampleData.sql`, `03_StoredProcedure.sql`,
   `04_AddDateRangeColumns.sql` against SQL Server.
2. Set Service Bus and Blob Storage secrets via `dotnet user-secrets` in both `API` and `Worker`
   projects.
3. Run via VS Code compound launch config **"frontend + backend + worker"** (F5), or manually:
   `dotnet run` in `API` and `Worker`, `npm start` in `Angular`.
4. Navigate to `http://localhost:4200`, submit a request, and watch status progress from
   `Not Processed` → `In Progress` → `Completed`.
