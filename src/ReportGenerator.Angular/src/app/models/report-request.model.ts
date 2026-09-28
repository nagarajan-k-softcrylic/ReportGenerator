export interface ReportRequest {
  id: string;
  reportName: string;
  requestedBy: string;
  requestedDate: string;
  status: 'Not Processed' | 'In Progress' | 'Completed' | 'Failed';
  failureReason?: string | null;
  blobUrl?: string | null;
  processedDate?: string | null;
}
