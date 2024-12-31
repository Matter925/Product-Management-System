import { inject } from '@angular/core';
import {
  ActivatedRouteSnapshot,
  CanActivateFn,
  Router,
  RouterStateSnapshot,
} from '@angular/router';

export const redirectGuard: CanActivateFn = (
  route: ActivatedRouteSnapshot,
  state: RouterStateSnapshot
) => {
  const router = inject(Router);

  const url = state.url || '';
  const firstPath = url.split('/')[1];
  if (firstPath === 'en') {
    router.navigate(['/en/home']);
  } else {
    router.navigate(['/ar/home']);
  }

  return false;
};
