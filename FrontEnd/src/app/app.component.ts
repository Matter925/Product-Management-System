import { BaseService } from './shared/services/Base/base.service';
import {
  Component,
  OnInit,
  Renderer2,
  OnDestroy,
  inject,
  ElementRef,
  ViewChild,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  ActivatedRoute,
  NavigationEnd,
  Router,
  RouterModule,
  RouterOutlet,
  
} from '@angular/router';
import { HeaderComponent } from './pages/layout/header/header.component';
import { FooterComponent } from './pages/layout/footer/footer.component';
import { TranslateService } from '@ngx-translate/core';
import { filter, switchMap, map, mergeMap, takeUntil } from 'rxjs';
import { MatSidenavModule } from '@angular/material/sidenav';
import { Meta, Title } from '@angular/platform-browser';
import { LanguageService } from './shared/services/Language/LanguageService';
import { Subject } from 'rxjs';
import { Direction } from '@angular/cdk/bidi';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    RouterOutlet,
    HeaderComponent,
    FooterComponent,
    MatSidenavModule,
    
  ],
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss'],
})
export class AppComponent implements OnInit, OnDestroy {
  isCartOpened: boolean = false;
  currentLanguage: string = 'ar';
  wishlistCount: number = 0;
  showHeaderFooter: boolean = true;

  private readonly destroy$ = new Subject<void>();
  private translateService = inject(TranslateService);

  constructor(
    private renderer: Renderer2,
    private route: ActivatedRoute,
    private router: Router,
    private meta: Meta,
    private titleService: Title,
    private languageService: LanguageService,
    private BaseService: BaseService
  ) {}

  ngOnInit(): void {
    // this.checkPassword();
    this.initLanguageSubscription();
    this.initRouterEventsSubscription();
    this.checkRoute();
    this.router.events
      .pipe(filter((event) => event instanceof NavigationEnd))
      .subscribe(() => {
        window.scrollTo(0, 0);
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private initLanguageSubscription(): void {
    this.languageService.currentLanguage$
      .pipe(
        switchMap((lang) =>
          lang
            ? [lang]
            : this.router.events.pipe(
                filter((event) => event instanceof NavigationEnd),
                map(
                  () =>
                    this.route.snapshot.firstChild?.paramMap.get('lang') || 'ar'
                )
              )
        ),
        takeUntil(this.destroy$)
      )
      .subscribe((lang) => {
        this.initializeLanguage(lang);
        this.intiSwiper(lang);
      });
  }

  private intiSwiper(lang: string) {
    const swiperEls = document.querySelectorAll('swiper-container');
    swiperEls.forEach((swiperEl) => {
      if (swiperEl?.swiper) {
        const dir: Direction = lang === 'ar' ? 'rtl' : 'ltr';
        swiperEl.swiper.changeLanguageDirection(dir);
      }
    });
    const testSlotElements = document.querySelectorAll(
      '.wm-swiper-slot'
    ) as NodeListOf<HTMLElement>;
    testSlotElements.forEach((element: HTMLElement) => {
      if (element) {
        lang === 'ar'
          ? (element.style.marginRight = '0px')
          : (element.style.marginLeft = '0px');
      }
    });
  }

  private initRouterEventsSubscription(): void {
    this.router.events
      .pipe(
        filter((event) => event instanceof NavigationEnd),
        map(() => this.getDeepestChildRoute(this.route)),
        mergeMap((route) => route.data),
        takeUntil(this.destroy$)
      )
      .subscribe((data) => {
        const title = this.getRouteTitleByCurrentLang(data);
        this.titleService.setTitle(title);
        this.updateMetaTags(title);
        this.checkRoute();
      });
  }

  private initializeLanguage(lang: string): void {
    this.currentLanguage = lang;
    this.translateService.setDefaultLang(this.currentLanguage);
    this.translateService.use(this.currentLanguage);
    this.setLanguageAttributes(this.currentLanguage);
    // this.loadBootstrapCss(this.currentLanguage);
    this.updateStructuredData(this.currentLanguage);
  }

  private checkRoute(): void {
    const currentRoute = this.router.url;
    this.showHeaderFooter = !currentRoute
      .split('?')[0]
      .match(/\/(login|signup)$/);
  }

  private getDeepestChildRoute(route: ActivatedRoute): ActivatedRoute {
    while (route.firstChild) {
      route = route.firstChild;
    }
    return route;
  }

  private setLanguageAttributes(language: string): void {
    const dir = language === 'ar' ? 'rtl' : 'ltr';
    this.renderer.setAttribute(document.documentElement, 'dir', dir);
    this.renderer.setAttribute(document.documentElement, 'lang', language);
    this.renderer.setStyle(document.body, 'direction', dir);
  }

  private getRouteTitleByCurrentLang(data: any): string {
    return this.currentLanguage === 'ar' ? data['titleAr'] : data['titleEn'];
  }

  private updateMetaTags(title: string): void {
    const description = this.getDescriptionByLanguage(this.currentLanguage);
    this.meta.updateTag({ name: 'description', content: description });
    this.meta.updateTag({ property: 'og:title', content: title });
    this.meta.updateTag({ property: 'og:description', content: description });
    this.meta.updateTag({
      property: 'og:url',
      content: `https://Alroya.sa/${this.currentLanguage}/`,
    });
    this.meta.updateTag({
      property: 'og:image',
      content: 'https://example.com/assets/your-image.png',
    });
  }

  private getDescriptionByLanguage(language: string): string {
    return language === 'ar'
      ? 'نحن نظامًا متكاملًا للغاية يهدف إلى تغيير نمط حياتك اليومي بناءً على الاختبارات الجينية لإنشاء خطط غذائية وتمارين مخصصة.'
      : 'We are a highly integrated system that aims to change your day-to-day lifestyle based on genetic tests to create personalized diet and exercise plans.';
  }

  private updateStructuredData(language: string): void {
    const scriptTag = document.createElement('script');
    scriptTag.type = 'application/ld+json';
    scriptTag.text = JSON.stringify(this.getStructuredData(language));

    const existingScript = document.querySelector(
      'script[type="application/ld+json"]'
    );
    if (existingScript) {
      this.renderer.removeChild(document.head, existingScript);
    }
    this.renderer.appendChild(document.head, scriptTag);
  }

  private getStructuredData(language: string): object {
    return {
      '@context': 'http://schema.org',
      '@type': 'WebSite',
      name: language === 'ar' ? 'خريطة العافية' : 'Alroya',
      alternateName: language === 'en' ? 'خريطة العافية' : 'Alroya',
      url: `https://example.com/${language}/`,
      description: this.getDescriptionByLanguage(language),
      potentialAction: {
        '@type': 'SearchAction',
        target: `https://example.com/${language}/search?q={search_term_string}`,
        'query-input': 'required name=search_term_string',
      },
      contactPoint: {
        '@type': 'ContactPoint',
        telephone: '+1-800-555-1234',
        contactType: 'Customer Support',
        areaServed: 'Worldwide',
      },
    };
  }

  // private loadBootstrapCss(language: string): void {
  //   const rtlHref = 'assets/css/bootstrap.css';
  //   const ltrHref = 'assets/css/bootstrap.css';
  //   const newHref = language === 'ar' ? rtlHref : ltrHref;
  //   const linkElementId = 'bootstrap-css';
  //   let linkElement = document.getElementById(linkElementId) as HTMLLinkElement;

  //   if (!linkElement) {
  //     linkElement = this.renderer.createElement('link');
  //     this.renderer.setAttribute(linkElement, 'id', linkElementId);
  //     this.renderer.setAttribute(linkElement, 'rel', 'stylesheet');
  //     this.renderer.appendChild(document.head, linkElement);
  //   }

  //   const base = window.location.origin;
  //   const absoluteNewHref = new URL(newHref, base).href;

  //   if (linkElement.href !== absoluteNewHref) {
  //     this.renderer.setAttribute(linkElement, 'href', absoluteNewHref);
  //   }
  // }
  scrollToTop(): void {
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }
}
