import { HttpInterceptorFn, HttpRequest, HttpHandlerFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, switchMap, timer } from 'rxjs';
import { AuthService } from '../services/auth.service';

export const tokenInterceptor: HttpInterceptorFn = (
  request: HttpRequest<unknown>,
  next: HttpHandlerFn
) => {
  const authService = inject(AuthService);
  const token = authService.token;
  
  if (token) {
    request = request.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
  }

  return next(request).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401 && !request.url.includes('/auth/refresh')) {
        return handle401Error(request, next, authService);
      }
      
      if (error.status === 429) {
        return handle429Error(request, next);
      }
      
      throw error;
    })
  );
};

function handle401Error(
  request: HttpRequest<unknown>,
  next: HttpHandlerFn,
  authService: AuthService
) {
  return authService.refresh().pipe(
    switchMap((newToken: string) => {
      request = request.clone({
        setHeaders: {
          Authorization: `Bearer ${newToken}`
        }
      });
      
      return next(request);
    }),
    catchError((refreshError) => {
      authService.logout();
      throw refreshError;
    })
  );
}

function handle429Error(
  request: HttpRequest<unknown>,
  next: HttpHandlerFn
) {
  return timer(500).pipe(
    switchMap(() => next(request))
  );
}
