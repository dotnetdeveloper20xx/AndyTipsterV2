import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { UserProfile } from '../../store/user/user.state';

@Injectable({ providedIn: 'root' })
export class UserService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/api/users`;

  getProfile(): Observable<UserProfile> {
    return this.http.get<UserProfile>(`${this.apiUrl}/me`);
  }

  updateProfile(changes: { displayName?: string; bio?: string; timezone?: string }): Observable<UserProfile> {
    return this.http.patch<UserProfile>(`${this.apiUrl}/me`, changes);
  }

  uploadAvatar(file: File): Observable<{ avatarUrl: string }> {
    const formData = new FormData();
    formData.append('avatar', file);
    return this.http.post<{ avatarUrl: string }>(`${this.apiUrl}/me/avatar`, formData);
  }

  getActivity(page: number = 1): Observable<{ entries: any[]; totalPages: number }> {
    return this.http.get<{ entries: any[]; totalPages: number }>(`${this.apiUrl}/me/activity`, {
      params: { page: page.toString() },
    });
  }
}
