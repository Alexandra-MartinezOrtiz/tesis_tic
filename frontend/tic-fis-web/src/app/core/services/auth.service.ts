import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { tap } from 'rxjs';
import { ApiEndpoints } from '../constants/api-endpoints';
import { LoginRequest, TokenResponse, DecodedToken } from '../models/auth.models';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private http = inject(HttpClient);
  private router = inject(Router);

  private readonly ACCESS_KEY = 'ticfis_token';
  private readonly REFRESH_KEY = 'ticfis_refresh';

  isAuthenticated = signal(this.hasValidToken());
  currentUserEmail = signal(this.decodeEmail());

  login(request: LoginRequest) {
    return this.http.post<TokenResponse>(ApiEndpoints.auth.login, request).pipe(
      tap((res) => {
        localStorage.setItem(this.ACCESS_KEY, res.accessToken);
        localStorage.setItem(this.REFRESH_KEY, res.refreshToken);
        this.isAuthenticated.set(true);
        this.currentUserEmail.set(this.decodeEmail());
      })
    );
  }

  logout() {
    localStorage.removeItem(this.ACCESS_KEY);
    localStorage.removeItem(this.REFRESH_KEY);
    this.isAuthenticated.set(false);
    this.currentUserEmail.set(null);
    this.router.navigate(['/auth/login']);
  }

  getAccessToken(): string | null {
    return localStorage.getItem(this.ACCESS_KEY);
  }

  getRoles(): string[] {
    const payload = this.decodePayload();
    if (!payload) return [];
    const roles = payload.roles;
    return Array.isArray(roles) ? roles : roles ? [roles] : [];
  }

  private hasValidToken(): boolean {
    const payload = this.decodePayload();
    return !!payload && payload.exp * 1000 > Date.now();
  }

  private decodeEmail(): string | null {
    const payload = this.decodePayload();
    return payload?.email ?? null;
  }

  private decodePayload(): DecodedToken | null {
    const token = localStorage.getItem(this.ACCESS_KEY);
    if (!token) return null;
    try {
      return JSON.parse(atob(token.split('.')[1])) as DecodedToken;
    } catch {
      return null;
    }
  }
}
