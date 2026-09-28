# ReportGenerator

Enterprise-grade Report Generation System built with .NET 8, Angular 20, SQL Server,
Azure Service Bus and Azure Blob Storage.

**Project name:** `ReportGenerator`

## Solution Structure

```
ReportGenerator.slnx
database/
  01_CreateTables.sql        -> ReportRequests, Employees, Departments, Salaries
  02_SampleData.sql          -> 60 sample employees + salaries
  03_StoredProcedure.sql     -> usp_GenerateEmployeeReport
src/
  ReportGenerator.Domain          (entities, enums)
  ReportGenerator.Application     (DTOs, interfaces, MediatR commands/queries - CQRS)
  ReportGenerator.Infrastructure  (EF Core, Service Bus publisher, Blob storage, repositories)
  ReportGenerator.API             (Web API controllers, DI, Program.cs)
  ReportGenerator.Worker          (Background worker: Service Bus consumer, Excel/ClosedXML, blob upload)
  ReportGenerator.Angular         (Angular 20 single-page UI)
```

## 1. Database Setup

Run against SQL Server in order:

```
database/01_CreateTables.sql
database/02_SampleData.sql
database/03_StoredProcedure.sql
```

## 2. Azure Prerequisites

- Service Bus namespace with a queue named `report-generation-queue`.
- Storage Account with a Blob container named `reports` (created automatically by the Worker if missing).

Update connection strings in:
- `src/ReportGenerator.API/appsettings.json`
- `src/ReportGenerator.Worker/appsettings.json`

(`ConnectionStrings:DefaultConnection`, `ServiceBus:ConnectionString`, `BlobStorage:ConnectionString`)

`ConnectionStrings:DefaultConnection` is preconfigured for a local SQL Server Express
instance (`Server=SCI-LT-299\SQLEXPRESS;Database=reportgeneratordb;`) using Windows
Authentication. Update the `Server` and `Database` values to match your environment.

## 3. Run the API

```
cd src/ReportGenerator.API
dotnet run
```

Swagger UI available at `/swagger` in Development.

## 4. Run the Worker

```
cd src/ReportGenerator.Worker
dotnet run
```

## 5. Run the Angular UI

```
cd src/ReportGenerator.Angular
npm install
npm start
```

Navigate to `http://localhost:4200`. Update `src/environments/environment.ts`
with the API's base URL if it differs from `https://localhost:5001/api`.

## Flow

1. User submits a report request from the Angular grid.
2. `POST /api/reports/request` inserts a `ReportRequests` row with status `Not Processed`
   and publishes a message (`RequestId`, `ReportName`) to `report-generation-queue`.
3. The Worker consumes the message, sets status `In Progress`, executes
   `usp_GenerateEmployeeReport`, builds an Excel file with ClosedXML, uploads it to
   the `reports` blob container, and updates the row to `Completed` with `BlobUrl`
   and `ProcessedDate` (or `Failed` with `FailureReason` on error).
4. The Angular grid polls `GET /api/reports` every 15 seconds and shows the
   Download Link once a report is `Completed`.

## API Endpoints

- `POST /api/reports/request`
- `GET /api/reports`
- `GET /api/reports/{id}`
- `GET /api/reports/download/{id}`
