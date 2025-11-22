import { Injectable } from '@angular/core';
import {
  HttpInterceptor,
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpErrorResponse
} from '@angular/common/http';

import { Observable, throwError } from 'rxjs';
import { catchError, switchMap } from 'rxjs/operators';

import { AuthService } from '../services/auth.service';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {   // <-- DEVE avere export
  private refreshing = false;

  constructor(private readonly auth: AuthService) {}

  intercept(
    req: HttpRequest<any>,
    next: HttpHandler
  ): Observable<HttpEvent<any>> {

    const token = this.auth.getToken();

    const authReq = req.clone({
      setHeaders: token ? { Authorization: `Bearer ${token}` } : {},
      withCredentials: true
    });

    return next.handle(authReq).pipe(
      catchError((error: HttpErrorResponse) => {
        if (error.status === 401 && !this.refreshing) {
          this.refreshing = true;

          return this.auth.refreshToken().pipe(
            switchMap((res) => {
              this.refreshing = false;

              const newReq = req.clone({
                setHeaders: { Authorization: `Bearer ${res.accessToken}` },
                withCredentials: true
              });

              return next.handle(newReq);
            }),
            catchError((err) => {
              this.refreshing = false;
              this.auth.logout();
              return throwError(() => err);
            })
          );
        }

        return throwError(() => error);
      })
    );
  }
}
