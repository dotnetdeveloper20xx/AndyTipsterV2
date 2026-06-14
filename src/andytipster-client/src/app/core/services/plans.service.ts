import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Plan } from '../../store/plans/plans.state';

@Injectable({ providedIn: 'root' })
export class PlansService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/api/plans`;

  getPlans(): Observable<Plan[]> {
    return this.http.get<Plan[]>(this.apiUrl);
  }

  getPlan(planId: string): Observable<Plan> {
    return this.http.get<Plan>(`${this.apiUrl}/${planId}`);
  }

  createPlan(plan: Partial<Plan>): Observable<Plan> {
    return this.http.post<Plan>(this.apiUrl, plan);
  }

  updatePlan(planId: string, changes: Partial<Plan>): Observable<Plan> {
    return this.http.patch<Plan>(`${this.apiUrl}/${planId}`, changes);
  }

  archivePlan(planId: string): Observable<Plan> {
    return this.http.post<Plan>(`${this.apiUrl}/${planId}/archive`, {});
  }

  syncToPayPal(planId: string): Observable<Plan> {
    return this.http.post<Plan>(`${this.apiUrl}/${planId}/sync-paypal`, {});
  }
}
