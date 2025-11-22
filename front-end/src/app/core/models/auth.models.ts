export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  accessExpiresIn: number;
  refreshExpiresIn: number;
  user: AuthUser;
}


export interface AuthUser {
  id: string;
  username: string;
  name: string;
  lastName: string;
  email?: string;
  emailVerified?: boolean;
}
