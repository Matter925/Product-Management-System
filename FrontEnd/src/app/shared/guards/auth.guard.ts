import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';
import { BaseService } from '../services/Base/base.service';
import { AuthService } from '../services/auth/auth.service';

export const authGuard: CanActivateFn = (route, state) => {
  const baseService = inject(BaseService);
  const authService = inject(AuthService);
  const router = inject(Router);
  if (authService.currentUserSubject.getValue() != null) {
    return true;
  } else {
    router.navigate(['/', baseService.currentLanguage, '/login']);

    return false;
  }
};
