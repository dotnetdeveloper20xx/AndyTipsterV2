import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Permission } from '../../store/permissions/permissions.state';

@Injectable({ providedIn: 'root' })
export class PermissionsService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/api/permissions`;

  getAllPermissions(): Observable<Permission[]> {
    return this.http.get<Permission[]>(this.apiUrl);
  }

  getUserPermissions(): Observable<string[]> {
    return this.http.get<string[]>(`${this.apiUrl}/me`);
  }
}
