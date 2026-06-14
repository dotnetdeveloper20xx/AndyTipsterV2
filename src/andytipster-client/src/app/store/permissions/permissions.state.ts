export interface Permission {
  id: string;
  name: string;
  description: string;
  category: string;
}

export interface PermissionsState {
  allPermissions: Permission[];
  userPermissions: string[];  // permission names granted to current user (union of all role permissions)
  isLoading: boolean;
  error: string | null;
}

export const initialPermissionsState: PermissionsState = {
  allPermissions: [],
  userPermissions: [],
  isLoading: false,
  error: null,
};
