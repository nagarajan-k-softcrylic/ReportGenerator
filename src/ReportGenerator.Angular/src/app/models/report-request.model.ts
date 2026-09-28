export interface ReportRequest {
  id: string;
  reportName: string;
  requestedBy: string;
  requestedDate: string;
  status: 'Not Processed' | 'In Progress' | 'Completed' | 'Failed';
  startDate?: string | null;
  endDate?: string | null;
  failureReason?: string | null;
  blobUrl?: string | null;
  processedDate?: string | null;
}
