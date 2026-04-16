import { HttpClient } from '@angular/common/http';
import { computed, inject, Injectable, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';

interface LoginRequest {
  email: string;
  password: string;
}

interface LoginResponse {
  accessToken: string;
  expiresAtUtc: string;
  userId: string;
  email: string;
}

export interface AuthUser {
  userId: string;
  email: string;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private static readonly TOKEN_STORAGE_KEY = 'mtgauth.accessToken';
  private static readonly USER_STORAGE_KEY = 'mtgauth.user';

  private readonly http = inject(HttpClient);

  readonly token = signal<string | null>(localStorage.getItem(AuthService.TOKEN_STORAGE_KEY));
  readonly user = signal<AuthUser | null>(this.readPersistedUser());
  readonly isLoggedIn = computed(() => !!this.token() && !!this.user());

  login(email: string, password: string): Observable<LoginResponse> {
    const request: LoginRequest = { email, password };

    return this.http.post<LoginResponse>(`${environment.authApiUrl}/api/auth/login`, request).pipe(
      tap((response) => {
        this.persistAuth(response.accessToken, {
          userId: response.userId,
          email: response.email
        });
      })
    );
  }

  initialize(): void {
    if (!this.token()) {
      this.clearAuth();
      return;
    }

    this.http.get<AuthUser>(`${environment.authApiUrl}/api/auth/me`).subscribe({
      next: (currentUser) => {
        this.user.set(currentUser);
        localStorage.setItem(AuthService.USER_STORAGE_KEY, JSON.stringify(currentUser));
      },
      error: () => {
        this.clearAuth();
      }
    });
  }

  clearAuth(): void {
    localStorage.removeItem(AuthService.TOKEN_STORAGE_KEY);
    localStorage.removeItem(AuthService.USER_STORAGE_KEY);
    this.token.set(null);
    this.user.set(null);
  }

  private persistAuth(token: string, user: AuthUser): void {
    localStorage.setItem(AuthService.TOKEN_STORAGE_KEY, token);
    localStorage.setItem(AuthService.USER_STORAGE_KEY, JSON.stringify(user));
    this.token.set(token);
    this.user.set(user);
  }

  private readPersistedUser(): AuthUser | null {
    const value = localStorage.getItem(AuthService.USER_STORAGE_KEY);
    if (!value) {
      return null;
    }

    try {
      return JSON.parse(value) as AuthUser;
    } catch {
      localStorage.removeItem(AuthService.USER_STORAGE_KEY);
      return null;
    }
  }
}
