import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { ReportRequest } from '../models/report-request.model';

@Injectable({ providedIn: 'root' })
export class ReportService {
  private readonly baseUrl = `${environment.apiBaseUrl}/reports`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<ReportRequest[]> {
    return this.http.get<ReportRequest[]>(this.baseUrl);
  }

  create(reportName: string, requestedBy: string): Observable<ReportRequest> {
    return this.http.post<ReportRequest>(`${this.baseUrl}/request`, { reportName, requestedBy });
  }

  getDownloadUrl(id: string): string {
    return `${this.baseUrl}/download/${id}`;
  }
}
