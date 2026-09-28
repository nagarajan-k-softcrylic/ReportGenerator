import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Subscription, interval, startWith, switchMap } from 'rxjs';
import { ReportService } from './services/report.service';
import { ReportRequest } from './models/report-request.model';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatTableModule,
    MatButtonModule,
    MatInputModule,
    MatFormFieldModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent implements OnInit, OnDestroy {
  displayedColumns: string[] = [
    'requestId', 'reportName', 'requestedBy', 'requestedDate', 'status', 'failureReason', 'download'
  ];

  reportRequests: ReportRequest[] = [];
  reportName = '';
  requestedBy = '';
  isSubmitting = false;
  errorMessage = '';

  private refreshSubscription?: Subscription;
  private readonly refreshIntervalMs = 15000;

  constructor(private reportService: ReportService) {}

  ngOnInit(): void {
    this.refreshSubscription = interval(this.refreshIntervalMs)
      .pipe(
        startWith(0),
        switchMap(() => this.reportService.getAll())
      )
      .subscribe({
        next: (data) => (this.reportRequests = data),
        error: (err) => (this.errorMessage = 'Failed to load report requests: ' + err.message)
      });
  }

  ngOnDestroy(): void {
    this.refreshSubscription?.unsubscribe();
  }

  submitRequest(): void {
    if (!this.reportName.trim() || !this.requestedBy.trim()) {
      this.errorMessage = 'Report Name and Requested By are required.';
      return;
    }

    this.isSubmitting = true;
    this.errorMessage = '';

    this.reportService.create(this.reportName.trim(), this.requestedBy.trim()).subscribe({
      next: (created) => {
        this.reportRequests = [created, ...this.reportRequests];
        this.reportName = '';
        this.requestedBy = '';
        this.isSubmitting = false;
      },
      error: (err) => {
        this.errorMessage = 'Failed to submit report request: ' + err.message;
        this.isSubmitting = false;
      }
    });
  }

  statusClass(status: string): string {
    return 'status-badge status-' + status.toLowerCase().replace(/ /g, '-');
  }

  downloadUrl(id: string): string {
    return this.reportService.getDownloadUrl(id);
  }
}
