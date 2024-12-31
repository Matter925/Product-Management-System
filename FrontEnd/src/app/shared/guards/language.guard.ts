import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';

export const languageGuard: CanActivateFn = (route, state) => {
  const router = inject(Router);

  // Check if the URL starts with /ar/ or /en/
  const path = state.url;
  if (path.startsWith('/ar/') || path.startsWith('/en/')) {
    return true; // Allow navigation
  }

  // Redirect to /ar/ if no valid language prefix is found
  const defaultLanguage = 'ar';
  const newUrl = `/${defaultLanguage}${path}`;
  router.navigateByUrl(newUrl);
  return false;
};
