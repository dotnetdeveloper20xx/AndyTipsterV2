import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface MediaAssetDto {
  id: string;
  fileName: string;
  contentType: string;
  blobUrl: string;
  cdnUrl?: string;
  fileSizeBytes: number;
  width?: number;
  height?: number;
  altText?: string;
  folderId?: string;
  folderName?: string;
  tags: string[];
  createdAt: string;
  uploadedByUserName: string;
}

export interface BatchUploadResult {
  succeeded: MediaAssetDto[];
  failed: { fileName: string; reason: string }[];
}

export interface MediaSearchParams {
  query?: string;
  folderId?: string;
  contentType?: string;
  tags?: string[];
  page?: number;
  pageSize?: number;
}

@Injectable({ providedIn: 'root' })
export class MediaService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/api/media`;

  upload(file: File, altText: string, folderId?: string, tags?: string[]): Observable<MediaAssetDto> {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('altText', altText);
    if (folderId) formData.append('folderId', folderId);
    if (tags?.length) formData.append('tags', tags.join(','));
    return this.http.post<MediaAssetDto>(`${this.apiUrl}/upload`, formData);
  }

  batchUpload(files: File[], altText: string, folderId?: string): Observable<BatchUploadResult> {
    const formData = new FormData();
    files.forEach(f => formData.append('files', f));
    formData.append('altText', altText);
    if (folderId) formData.append('folderId', folderId);
    return this.http.post<BatchUploadResult>(`${this.apiUrl}/upload/batch`, formData);
  }

  getAsset(assetId: string): Observable<MediaAssetDto> {
    return this.http.get<MediaAssetDto>(`${this.apiUrl}/${assetId}`);
  }

  search(params: MediaSearchParams): Observable<MediaAssetDto[]> {
    let httpParams = new HttpParams();
    if (params.query) httpParams = httpParams.set('query', params.query);
    if (params.folderId) httpParams = httpParams.set('folderId', params.folderId);
    if (params.contentType) httpParams = httpParams.set('contentType', params.contentType);
    if (params.page) httpParams = httpParams.set('page', params.page);
    if (params.pageSize) httpParams = httpParams.set('pageSize', params.pageSize);
    return this.http.get<MediaAssetDto[]>(`${this.apiUrl}/search`, { params: httpParams });
  }

  updateAsset(assetId: string, changes: { altText?: string; fileName?: string; tags?: string[] }): Observable<MediaAssetDto> {
    return this.http.patch<MediaAssetDto>(`${this.apiUrl}/${assetId}`, changes);
  }

  deleteAsset(assetId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${assetId}`);
  }

  transformImage(assetId: string, transform: {
    cropX?: number; cropY?: number; cropWidth?: number; cropHeight?: number;
    resizeWidth?: number; resizeHeight?: number; rotateDegrees?: number;
  }): Observable<MediaAssetDto> {
    return this.http.post<MediaAssetDto>(`${this.apiUrl}/${assetId}/transform`, transform);
  }

  checkInUse(assetId: string): Observable<{ inUse: boolean; pages: string[] }> {
    return this.http.get<{ inUse: boolean; pages: string[] }>(`${this.apiUrl}/${assetId}/in-use`);
  }
}
