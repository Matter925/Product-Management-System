import { RouterModule, Routes } from '@angular/router';
import { authGuard } from './shared/guards/auth.guard';
import { redirectGuard } from './shared/guards/redirect.guard';
import { NgModule } from '@angular/core';
import { QuicklinkStrategy } from 'ngx-quicklink';
import { languageGuard } from './shared/guards/language.guard';
import { HomeComponent } from './pages/home/home.component';

export const routes: Routes = [
  {
    path: ':lang/contact-us',
    canActivate: [languageGuard],
    loadComponent: () =>
      import('./pages/Contact-Us/Contact-Us.component').then(
        (m) => m.ContactUsComponent
      ),
    data: {
      titleEn: 'Contact us - Product Management System',
      titleAr: 'تواصل معنا - نظام إدارة المنتجات',
    },
  },
 
  {
    path: ':lang/create/:id',
    canActivate: [languageGuard],
    loadComponent: () =>
      import('./pages/create/create.component').then(
        (m) => m.CreateComponent
      ),
    data: {
      titleEn: 'create item - Product Management System',
      titleAr: ' إضافة منتج - نظام إدارة المنتجات',
    },
  },
  {
    path: ':lang/create',
    canActivate: [languageGuard],
    loadComponent: () =>
      import('./pages/create/create.component').then(
        (m) => m.CreateComponent
      ),
    data: {
      titleEn: 'create item - Product Management System',
      titleAr: ' إضافة منتج - نظام إدارة المنتجات',
    },
  },
  {
    path: ':lang/products',
    canActivate: [languageGuard],
    loadComponent: () =>
      import('./pages/products/products.component').then(
        (m) => m.ProductsComponent
      ),
    data: {
      titleEn: 'Products - Product Management System',
      titleAr: ' المنتجات  - نظام إدارة المنتجات',
    },
  },
 
 
  {
    path: ':lang/home',
    canActivate: [languageGuard],
    component: HomeComponent,
    data: {
      titleEn: 'Home - Product Management System',
      titleAr: 'الصفحة الرئيسية - نظام إدارة المنتجات',
    },
  },
  {
    path: '',
    canActivate: [redirectGuard, languageGuard],
    component: HomeComponent,
    data: {
      titleEn: 'Home - Product Management System',
      titleAr: 'الصفحة الرئيسية - نظام إدارة المنتجات',
    },
  },
  {
    path: '**',
    canActivate: [redirectGuard, languageGuard],
    component: HomeComponent,
    data: {
      titleEn: 'Home - Product Management System',
      titleAr: 'الصفحة الرئيسية - نظام إدارة المنتجات',
    },
  },
];

@NgModule({
  imports: [
    RouterModule.forRoot(routes, {
      preloadingStrategy: QuicklinkStrategy,
      scrollPositionRestoration: 'enabled',
      anchorScrolling: 'enabled',
      onSameUrlNavigation: 'reload',
    }),
  ],
  exports: [RouterModule],
})
export class AppRoutingModule {}
