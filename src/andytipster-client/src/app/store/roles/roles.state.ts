import { EntityState } from '@ngrx/entity';

export interface Role {
  id: string;
  name: string;
  hierarchyLevel: number;
  isSystem: boolean;
  permissions: string[];
  userCount: number;
}

export interface RolesState extends EntityState<Role> {
  userRoles: string[];  // role IDs assigned to the current user
  isLoading: boolean;
  error: string | null;
}
