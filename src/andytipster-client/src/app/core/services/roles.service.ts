import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Role } from '../../store/roles/roles.state';

@Injectable({ providedIn: 'root' })
export class RolesService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/api/roles`;

  getRoles(): Observable<Role[]> {
    return this.http.get<Role[]>(this.apiUrl);
  }

  getUserRoles(): Observable<string[]> {
    return this.http.get<string[]>(`${this.apiUrl}/me`);
  }

  createRole(name: string, hierarchyLevel: number, permissions: string[]): Observable<Role> {
    return this.http.post<Role>(this.apiUrl, { name, hierarchyLevel, permissions });
  }

  deleteRole(roleId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${roleId}`);
  }
}
