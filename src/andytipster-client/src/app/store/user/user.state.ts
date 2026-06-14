export interface UserProfile {
  id: string;
  email: string;
  displayName: string;
  bio: string | null;
  avatarUrl: string | null;
  timezone: string;
  emailVerified: boolean;
  twoFactorEnabled: boolean;
  createdAt: string;
}

export interface UserState {
  profile: UserProfile | null;
  isLoading: boolean;
  error: string | null;
}

export const initialUserState: UserState = {
  profile: null,
  isLoading: false,
  error: null,
};
