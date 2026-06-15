import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface LoginResponse {
  accessToken?: string;
  refreshToken?: string;
  expiresAt?: number;
  requires2FA: boolean;
  twoFactorEmail?: string;
}

export interface TokenResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt: number;
}

export interface RegisterResponse {
  message: string;
}

export interface Enable2FAResponse {
  secret: string;
  qrCodeUrl: string;
}

export interface MessageResponse {
  message: string;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/api/auth`;

  login(email: string, password: string): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/login`, { email, password });
  }

  register(email: string, password: string, displayName: string): Observable<RegisterResponse> {
    return this.http.post<RegisterResponse>(`${this.apiUrl}/register`, { email, password, displayName });
  }

  refreshToken(): Observable<TokenResponse> {
    const refreshToken = this.getStoredRefreshToken();
    return this.http.post<TokenResponse>(`${this.apiUrl}/refresh`, { refreshToken });
  }

  logout(): Observable<void> {
    const refreshToken = this.getStoredRefreshToken();
    return this.http.post<void>(`${this.apiUrl}/logout`, { refreshToken });
  }

  forgotPassword(email: string): Observable<MessageResponse> {
    return this.http.post<MessageResponse>(`${this.apiUrl}/forgot-password`, { email });
  }

  resetPassword(email: string, token: string, newPassword: string): Observable<MessageResponse> {
    return this.http.post<MessageResponse>(`${this.apiUrl}/reset-password`, { email, token, newPassword });
  }

  socialLogin(provider: string, accessToken: string): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/social-login`, { provider, accessToken });
  }

  enable2FA(): Observable<Enable2FAResponse> {
    return this.http.post<Enable2FAResponse>(`${this.apiUrl}/2fa/enable`, {});
  }

  confirm2FA(code: string): Observable<MessageResponse> {
    return this.http.post<MessageResponse>(`${this.apiUrl}/2fa/confirm`, { code });
  }

  verify2FA(email: string, code: string): Observable<TokenResponse> {
    return this.http.post<TokenResponse>(`${this.apiUrl}/2fa/verify`, { email, code });
  }

  verifyRecoveryCode(email: string, code: string): Observable<TokenResponse> {
    return this.http.post<TokenResponse>(`${this.apiUrl}/2fa/recovery`, { email, code });
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
