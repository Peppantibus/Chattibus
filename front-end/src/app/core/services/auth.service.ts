import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';

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

  constructor(private readonly http: HttpClient) {}

  login(payload: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${environment.apiUrl}/auth/login`, payload).pipe(
      tap((response) => this.persistSession(response))
    );
  }

  register(payload: RegisterRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${environment.apiUrl}/auth/register`, payload).pipe(
      tap((response) => this.persistSession(response))
    );
  }

  logout(): void {
    if (this.hasWindow()) {
      localStorage.removeItem(this.tokenKey);
      localStorage.removeItem(this.userKey);
    }
    this.currentUserSubject.next(null);
  }

  restoreSession(): void {
    if (!this.hasWindow()) {
      return;
    }
    const token = localStorage.getItem(this.tokenKey);
    const storedUser = localStorage.getItem(this.userKey);

    if (token && !this.isTokenExpired(token)) {
      if (storedUser) {
        this.currentUserSubject.next(JSON.parse(storedUser));
      }
    } else {
      this.logout();
    }
  }

  getToken(): string | null {
    if (!this.hasWindow()) {
      return null;
    }
    return localStorage.getItem(this.tokenKey);
  }

  isAuthenticated(): boolean {
    const token = this.getToken();
    return !!token && !this.isTokenExpired(token);
  }

  private persistSession(response: AuthResponse): void {
    if (this.hasWindow()) {
      localStorage.setItem(this.tokenKey, response.token);
      localStorage.setItem(this.userKey, JSON.stringify(response.user));
    }
    this.currentUserSubject.next(response.user);
  }

  private isTokenExpired(token: string): boolean {
    try {
      const payload = this.decodeJwt(token);
      if (!payload?.exp) {
        return false;
      }
      const expiresAt = payload.exp * 1000;
      return Date.now() > expiresAt;
    } catch {
      return true;
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
        .map((char) => `%${char.charCodeAt(0).toString(16).padStart(2, '0')}`)
        .join('')
    );
    return JSON.parse(jsonPayload);
  }

  private hasWindow(): boolean {
    return typeof window !== 'undefined';
  }
}
