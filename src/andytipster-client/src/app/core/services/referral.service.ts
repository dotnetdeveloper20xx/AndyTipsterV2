import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface ReferralLinkDto {
  referralCode: string;
  referralUrl: string;
}

export interface ReferralDashboardDto {
  referralCode: string;
  referralUrl: string;
  totalClicks: number;
  totalConversions: number;
  totalRewardsEarned: number;
  referrals: ReferralItemDto[];
}

export interface ReferralItemDto {
  id: string;
  referredEmail?: string;
  isConverted: boolean;
  createdAt: string;
  convertedAt?: string;
}

export interface ReferralConfigDto {
  rewardAmount: number;
  rewardType: string;
  maxReferralsPerUser: number;
  isActive: boolean;
}

export interface UpdateReferralConfigDto {
  rewardAmount?: number;
  rewardType?: string;
  maxReferralsPerUser?: number;
  isActive?: boolean;
}

@Injectable({ providedIn: 'root' })
export class ReferralService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/api/referrals`;

  getReferralLink(): Observable<ReferralLinkDto> {
    return this.http.get<ReferralLinkDto>(`${this.apiUrl}/link`);
  }

  getDashboard(): Observable<ReferralDashboardDto> {
    return this.http.get<ReferralDashboardDto>(`${this.apiUrl}/dashboard`);
  }

  trackClick(referralCode: string): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.apiUrl}/click/${referralCode}`, {});
  }

  getConfig(): Observable<ReferralConfigDto> {
    return this.http.get<ReferralConfigDto>(`${this.apiUrl}/config`);
  }

  updateConfig(dto: UpdateReferralConfigDto): Observable<ReferralConfigDto> {
    return this.http.patch<ReferralConfigDto>(`${this.apiUrl}/config`, dto);
  }
}
