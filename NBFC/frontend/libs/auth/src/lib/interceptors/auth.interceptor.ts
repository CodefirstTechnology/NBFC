import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { from, switchMap, catchError, throwError } from 'rxjs';
import { TokenStorageService } from '../services/token-storage.service';
import { AuthService } from '../services/auth.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const tokenStorage = inject(TokenStorageService);
  const authService = inject(AuthService);

  const correlationId = crypto.randomUUID();
  let authReq = req.clone({
    setHeaders: {
      'X-Correlation-Id': correlationId,
    },
  });

  const token = tokenStorage.getAccessToken();
  if (token && !req.url.includes('/auth/login') && !req.url.includes('/auth/refresh')) {
    authReq = authReq.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`,
      },
    });
  }

  if (tokenStorage.isAccessTokenExpired() && tokenStorage.getRefreshToken()) {
    return from(authService.refreshSession()).pipe(
      switchMap((refreshed) => {
        const refreshedToken = tokenStorage.getAccessToken();
        const retryReq =
          refreshed && refreshedToken
            ? authReq.clone({ setHeaders: { Authorization: `Bearer ${refreshedToken}` } })
            : authReq;
        return next(retryReq);
      }),
      catchError((error) => throwError(() => error))
    );
  }

  return next(authReq);
};
