import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface LoginResponse {
  accessToken?: string;
  refreshToken?: string;
  expiresAt?: number;
  requires2FA: boolean;
}

export interface TokenResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt: number;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/api/auth`;

  login(email: string, password: string): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/login`, { email, password });
  }

  verify2FA(code: string): Observable<TokenResponse> {
    return this.http.post<TokenResponse>(`${this.apiUrl}/2fa/verify`, { code });
  }

  register(email: string, password: string, displayName: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/register`, { email, password, displayName });
  }

  logout(): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/logout`, {});
  }

  refreshToken(): Observable<TokenResponse> {
    const refreshToken = this.getStoredRefreshToken();
    return this.http.post<TokenResponse>(`${this.apiUrl}/refresh`, { refreshToken });
  }

  getStoredAccessToken(): string | null {
    return localStorage.getItem('accessToken');
  }

  getStoredRefreshToken(): string | null {
    return localStorage.getItem('refreshToken');
  }

  storeTokens(accessToken: string, refreshToken: string): void {
    localStorage.setItem('accessToken', accessToken);
    localStorage.setItem('refreshToken', refreshToken);
  }

  clearTokens(): void {
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
  }
}
