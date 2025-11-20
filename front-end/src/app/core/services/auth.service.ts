// src/app/core/services/auth.service.ts

import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import {
  BehaviorSubject,
  Observable,
  tap,
  finalize,
  EMPTY
} from 'rxjs';

import { environment } from '../../../environments/environment';
import { AuthResponse, AuthUser } from '../models/auth.models';

interface LoginRequest {
  username: string;
  password: string;
}

interface RegisterRequest {
  username: string;
  email: string;
  password: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly tokenKey = 'chattibus_token';
  private readonly userKey = 'chattibus_user';

  private readonly currentUserSubject = new BehaviorSubject<AuthUser | null>(null);
  readonly currentUser$ = this.currentUserSubject.asObservable();

  private refreshTimeoutId: any;
  private isRefreshing = false;

  constructor(private readonly http: HttpClient) {}

  // LOGIN: backend setta refresh token nel cookie HttpOnly
  login(payload: LoginRequest): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${environment.apiUrl}/auth/login`, payload, {
        withCredentials: true
      })
      .pipe(tap((response) => this.persistSession(response)));
  }

  register(payload: RegisterRequest): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${environment.apiUrl}/auth/register`, payload, {
        withCredentials: true
      })
      .pipe(tap((response) => this.persistSession(response)));
  }

  logout(): void {
    if (this.hasWindow()) {
      localStorage.removeItem(this.tokenKey);
      localStorage.removeItem(this.userKey);
    }

    this.clearRefreshTimer();
    this.currentUserSubject.next(null);
  }

  restoreSession(): void {
    if (!this.hasWindow()) return;
    if (this.isRefreshing) return;

    const token = this.getToken();
    const storedUser = this.getStoredUser();

    // access token ancora valido
    if (token && !this.isTokenExpired(token)) {
      if (storedUser) {
        this.currentUserSubject.next(storedUser);
      }
      this.scheduleTokenRefresh(token);
      return;
    }

    // scaduto → prova refresh via cookie HttpOnly
    this.refreshToken().subscribe({
      next: (res) => {
        if (res.user) this.currentUserSubject.next(res.user);
      },
      error: () => this.logout()
    });
  }

  // Legge user memorizzato
  private getStoredUser(): AuthUser | null {
    const stored = this.hasWindow() ? localStorage.getItem(this.userKey) : null;
    return stored ? JSON.parse(stored) : null;
  }

  getToken(): string | null {
    return this.hasWindow() ? localStorage.getItem(this.tokenKey) : null;
  }

  isAuthenticated(): boolean {
    const token = this.getToken();
    return !!token && !this.isTokenExpired(token);
  }

  // REFRESH TOKEN VIA COOKIE HTTPONLY
  refreshToken(): Observable<AuthResponse> {
    // evita refresh simultanei
    if (this.isRefreshing) {
      return EMPTY;
    }

    this.isRefreshing = true;

    return this.http
      .post<AuthResponse>(`${environment.apiUrl}/auth/refresh-token`, null, {
        withCredentials: true
      })
      .pipe(
        tap((response) => this.persistNewAccessToken(response)),
        finalize(() => (this.isRefreshing = false))
      );
  }

  verifyEmail(token: string): Observable<boolean> {
    return this.http.get<boolean>(
      `${environment.apiUrl}/auth/verify?token=${token}`
    );
  }

  requestPasswordRecovery(email: string): Observable<string> {
    return this.http.post(
      `${environment.apiUrl}/auth/recovery-password`,
      { email },
      { responseType: 'text' }
    ) as Observable<string>;
  }

  validatePasswordResetToken(token: string): Observable<boolean> {
    return this.http.get<boolean>(
      `${environment.apiUrl}/auth/validate-password?token=${token}`
    );
  }

  resetPassword(payload: {
    token: string;
    password: string;
    confirmPassword: string;
  }): Observable<boolean> {
    return this.http.put<boolean>(
      `${environment.apiUrl}/auth/reset-password`,
      payload
    );
  }

  // ============================================
  // PRIVATE
  // ============================================

  private persistSession(response: AuthResponse): void {
    if (this.hasWindow()) {
      localStorage.setItem(this.tokenKey, response.accessToken);
      localStorage.setItem(this.userKey, JSON.stringify(response.user));
    }

    this.currentUserSubject.next(response.user);
    this.clearRefreshTimer();
    this.scheduleTokenRefresh(response.accessToken);
  }

  private persistNewAccessToken(response: AuthResponse): void {
    if (this.hasWindow()) {
      localStorage.setItem(this.tokenKey, response.accessToken);

      if (response.user) {
        localStorage.setItem(this.userKey, JSON.stringify(response.user));
        this.currentUserSubject.next(response.user);
      }
    }

    this.clearRefreshTimer();
    this.scheduleTokenRefresh(response.accessToken);
  }

  private isTokenExpired(token: string): boolean {
    try {
      const payload = this.decodeJwt(token);
      if (!payload?.exp) return false;
      return Date.now() > payload.exp * 1000;
    } catch {
      return true;
    }
  }

  private scheduleTokenRefresh(token: string): void {
    try {
      const payload = this.decodeJwt(token);
      if (!payload?.exp) return;

      const expiresAt = payload.exp * 1000;
      const now = Date.now();
      const refreshAt = expiresAt - 60_000;
      const delay = refreshAt - now;

      if (delay <= 0) {
        if (!this.isRefreshing) {
          this.refreshToken().subscribe({
            error: () => this.logout()
          });
        }
        return;
      }

      this.refreshTimeoutId = setTimeout(() => {
        if (!this.isRefreshing) {
          this.refreshToken().subscribe({
            error: () => this.logout()
          });
        }
      }, delay);
    } catch {}
  }

  private clearRefreshTimer(): void {
    if (this.refreshTimeoutId) {
      clearTimeout(this.refreshTimeoutId);
      this.refreshTimeoutId = null;
    }
  }

  private decodeJwt(token: string): any {
    const parts = token.split('.');
    if (parts.length < 2) {
      throw new Error('Token non valido');
    }

    const payload = parts[1];
    const base64 = payload.replace(/-/g, '+').replace(/_/g, '/');
    const decoded = atob(base64);

    const jsonPayload = decodeURIComponent(
      Array.from(decoded)
        .map((char) =>
          `%${char.charCodeAt(0).toString(16).padStart(2, '0')}`
        )
        .join('')
    );

    return JSON.parse(jsonPayload);
  }

  private hasWindow(): boolean {
    return typeof window !== 'undefined';
  }
}
