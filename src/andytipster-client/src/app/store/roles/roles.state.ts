/**
 * Roles store slice — holds SYSTEM-WIDE role definitions for admin management.
 * For the CURRENT USER's roles, use selectAuthRoles from the auth slice.
 */
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
