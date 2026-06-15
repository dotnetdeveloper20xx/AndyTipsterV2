export interface DecodedJwt {
  sub: string;
  email: string;
  display_name: string;
  'http://schemas.microsoft.com/ws/2008/06/identity/claims/role': string | string[];
  permission: string | string[];
  exp: number;
}

export function decodeJwt(token: string): DecodedJwt | null {
  try {
    const base64Url = token.split('.')[1];
    const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
    const jsonPayload = decodeURIComponent(
      atob(base64)
        .split('')
        .map((c) => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
        .join('')
    );
    return JSON.parse(jsonPayload);
  } catch {
    return null;
  }
}

export function extractUserFromJwt(token: string): {
  user: { id: string; email: string; displayName: string; avatarUrl: string | null };
  roles: string[];
  permissions: string[];
} | null {
  const decoded = decodeJwt(token);
  if (!decoded) return null;

  const roleClaim = decoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
  const roles = Array.isArray(roleClaim) ? roleClaim : roleClaim ? [roleClaim] : [];

  const permClaim = decoded.permission;
  const permissions = Array.isArray(permClaim) ? permClaim : permClaim ? [permClaim] : [];

  return {
    user: {
      id: decoded.sub,
      email: decoded.email,
      displayName: decoded.display_name,
      avatarUrl: null,
    },
    roles,
    permissions,
  };
}
