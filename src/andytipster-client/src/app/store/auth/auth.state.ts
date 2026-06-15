export interface AuthUser {
  id: string;
  email: string;
  displayName: string;
  avatarUrl: string | null;
}

export interface AuthState {
  accessToken: string | null;
  refreshToken: string | null;
  user: AuthUser | null;
  roles: string[];
  permissions: string[];
  isAuthenticated: boolean;
  isLoading: boolean;
  error: string | null;
  requires2FA: boolean;
  twoFactorEmail: string | null;
  tokenExpiresAt: string | null;
}

export const initialAuthState: AuthState = {
  accessToken: null,
  refreshToken: null,
  user: null,
  roles: [],
  permissions: [],
  isAuthenticated: false,
  isLoading: false,
  error: null,
  requires2FA: false,
  twoFactorEmail: null,
  tokenExpiresAt: null,
};
