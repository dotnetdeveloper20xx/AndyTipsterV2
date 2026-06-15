import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface DataExportStatusDto {
  requestId: string;
  status: string;
  requestedAt: string;
  completedAt?: string;
  expiresAt?: string;
  downloadUrl?: string;
}

export interface AccountDeletionStatusDto {
  userId: string;
  status: string;
  scheduledDeletionDate: string;
  cancelledAt?: string;
  canCancel: boolean;
}

export interface ConsentRecordDto {
  id: string;
  consentType: string;
  isGranted: boolean;
  grantedAt: string;
  revokedAt?: string;
  ipAddress?: string;
}

export interface DataProcessingRecordDto {
  id: string;
  userId: string;
  processingType: string;
  purpose: string;
  timestamp: string;
  legalBasis?: string;
}

@Injectable({ providedIn: 'root' })
export class GdprService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/api/gdpr`;

  // Data Export
  requestExport(format: 'json' | 'csv' = 'json'): Observable<DataExportStatusDto> {
    return this.http.post<DataExportStatusDto>(`${this.apiUrl}/export`, { format });
  }

  getExportStatus(requestId: string): Observable<DataExportStatusDto> {
    return this.http.get<DataExportStatusDto>(`${this.apiUrl}/export/${requestId}`);
  }

  downloadExport(requestId: string): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/export/${requestId}/download`, { responseType: 'blob' });
  }

  // Account Deletion
  requestDeletion(password: string, reason: string): Observable<AccountDeletionStatusDto> {
    return this.http.post<AccountDeletionStatusDto>(`${this.apiUrl}/deletion`, { password, reason });
  }

  getDeletionStatus(): Observable<AccountDeletionStatusDto> {
    return this.http.get<AccountDeletionStatusDto>(`${this.apiUrl}/deletion`);
  }

  cancelDeletion(): Observable<AccountDeletionStatusDto> {
    return this.http.post<AccountDeletionStatusDto>(`${this.apiUrl}/deletion/cancel`, {});
  }

  // Right to Rectification
  rectifyData(data: { displayName?: string; email?: string; bio?: string; timezone?: string }): Observable<{ message: string }> {
    return this.http.put<{ message: string }>(`${this.apiUrl}/rectification`, data);
  }

  // Consent Records
  getConsentRecords(): Observable<ConsentRecordDto[]> {
    return this.http.get<ConsentRecordDto[]>(`${this.apiUrl}/consent`);
  }

  recordConsent(consentType: string, isGranted: boolean): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.apiUrl}/consent`, { consentType, isGranted });
  }

  // Processing Records
  getProcessingRecords(): Observable<DataProcessingRecordDto[]> {
    return this.http.get<DataProcessingRecordDto[]>(`${this.apiUrl}/processing-records`);
  }

  // Breach Notification (admin)
  sendBreachNotification(subject: string, message: string, notifyAll: boolean, affectedUserIds?: string[]): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.apiUrl}/breach-notification`, {
      subject,
      message,
      notifyAll,
      affectedUserIds,
    });
  }
}
