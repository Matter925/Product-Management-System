import { Injectable } from '@angular/core';
import { ActivatedRoute, NavigationEnd, Router } from '@angular/router';
import { BehaviorSubject, filter } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class LanguageService {
  private currentLanguage = new BehaviorSubject<string | null>('ar');
  currentLanguage$ = this.currentLanguage.asObservable();

  constructor(private router: Router, private route: ActivatedRoute) {
    this.router.events
      .pipe(filter((event) => event instanceof NavigationEnd))
      .subscribe(() => {
        const lang =
          this.route.root.firstChild?.snapshot.paramMap.get('lang') || 'ar';

        this.currentLanguage.next(lang);
      });
  }

  private getCurrentLanguage(): string {
    return this.route.snapshot.firstChild?.paramMap.get('lang') || 'ar';
  }

  changeLanguage(language: string): string {
    const currentLang = this.getCurrentLanguage();
  
    if (currentLang !== language) {
      const currentUrl = this.router.url;
      return currentUrl.replace(`/${currentLang}/`, `/${language}/`);
    }
    return this.router.url; 
  }
  
}
