import { BaseService } from './../../../shared/services/Base/base.service';
import {
  AfterContentInit,
  AfterViewChecked,
  AfterViewInit,
  ChangeDetectorRef,
  Component,
  ElementRef,
  HostListener,
  Input,
  OnChanges,
  OnDestroy,
  OnInit,
  ViewChild,
} from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { Router } from '@angular/router';
import { MatBadgeModule } from '@angular/material/badge';
import { SharedModule } from './../../../shared/shared.module';
import { SignalRService } from '../../../shared/services/SignalR/signal-r.service';
import { NotificationViewModel } from '../../../shared/models/interfaces/Notification';
import { AuthService } from '../../../shared/services/auth/auth.service';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { LanguageService } from '../../../shared/services/Language/LanguageService';
import { environment } from '../../../../environments/environment';
import { CompanyInfo } from '../../../shared/models/interfaces/CompanyInfo';
declare var $: any;
@Component({
  selector: 'app-header',
  standalone: true,
  imports: [SharedModule, MatBadgeModule,ButtonModule],
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.scss'],
})
export class HeaderComponent implements OnInit, OnDestroy, AfterViewInit {
  currentUser: any = null;
  imageUrl = environment.imageUrl;
  notifications: NotificationViewModel[] = [];
  unreadNotificationsCount: number = 0;
  isNavbarCollapsed = true;
  isActive: boolean = false;
  cartCount: number = 0;
  companyInfo: CompanyInfo | null = null;
  searchInputValue: string = '';
  @ViewChild('searchInput') searchInput!: ElementRef;
  @Input() wishlistCount: number = 0;
  @Input() currentLanguage: string = 'ar';
  private ngUnsubscribe = new Subject<void>();

  constructor(
    private signalRService: SignalRService,
    private authService: AuthService,
    public languageService: LanguageService,
    public baseService: BaseService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {}
  ngAfterViewInit(): void {
    const menuToggle = document.querySelector('.navbar-trigger');
    const mobileNav = document.getElementById('nav-mobile');
  
    if (menuToggle && mobileNav) {
      menuToggle.addEventListener('click', function (event) {
        event.stopPropagation();
        mobileNav.classList.toggle('mm-menu_opened');
      });
    }
  
    if (mobileNav) {
      // Close menu when clicking outside
      document.addEventListener('click', function (event) {
        const target = event.target as HTMLElement;
        if (!mobileNav.contains(target) && !menuToggle?.contains(target)) {
          mobileNav.classList.remove('mm-menu_opened');
        }
      });
  
      // Close menu on router navigation
      this.router.events.subscribe(() => {
        mobileNav.classList.remove('mm-menu_opened');
      });
    }
  }
  
  ngOnInit(): void {
    this.loadCurrentUser();
    this.fetchCompanyInfo();
  }
  ngOnDestroy(): void {
    this.cleanupSubscriptions();
    this.signalRService.stopConnection();
  }
  logout(): void {
    this.authService.logout();
    this.router.navigate(['/', this.baseService.currentLanguage, '/home']);
  }
  scrollToSection(sectionId: string) {
    const section = document.getElementById(sectionId);
    if (section) {
      section.scrollIntoView({ behavior: 'smooth' });
    }
  }
  fetchCompanyInfo(): void {
    this.baseService.getCompanyInfo().subscribe((info) => {
      this.companyInfo = info;
    });
  }
  loadCurrentUser(): void {
    this.authService.getCurrentUser().subscribe({
      next: (user) => {
        this.currentUser = user;
        if (this.currentUser) {
          this.initializeSignalRConnection();
        }
      },
      error: (error) => console.error('Error loading current user:', error),
    });
  }

  markNotificationAsRead(notification: NotificationViewModel): void {
    this.signalRService.markNotificationAsRead(notification.id);
  }

  search(): void {
    if (this.searchInputValue) {
      this.router.navigate([
        '/',
        this.baseService.currentLanguage,
        'search',
        this.searchInputValue,
      ]);
    }
  }

  toggleSearch(): void {
    if (!this.isActive) {
      this.isActive = true;
      setTimeout(() => this.searchInput.nativeElement.focus(), 0);
    } else {
      this.search();
      this.isActive = false;
    }
  }

  onKey(event: KeyboardEvent): void {
    if (event.key === 'Enter') {
      this.search();
    }
  }
  @HostListener('document:click', ['$event'])
  onClick(event: MouseEvent): void {
    const target = event.target as HTMLElement;

    if (this.isActive && this.searchInput?.nativeElement) {
      const searchContainer =
        this.searchInput.nativeElement.closest('.search-container');
      if (searchContainer && !searchContainer.contains(target)) {
        this.isActive = false;
      }
    }
  }

  private cleanupSubscriptions(): void {
    this.ngUnsubscribe.next();
    this.ngUnsubscribe.complete();
  }

  private initializeSignalRConnection(): void {
    const token = localStorage.getItem('userToken');
    if (token) {
      this.signalRService.startConnection(token);
      this.signalRService.loadNotifications();
      this.signalRService.notifications$
        .pipe(takeUntil(this.ngUnsubscribe))
        .subscribe((notifications) => {
          this.notifications = notifications;
          this.calculateUnreadNotificationsCount();
        });
    }
  }

  private calculateUnreadNotificationsCount(): void {
    this.unreadNotificationsCount = this.notifications.filter(
      (notification) => !notification.seen
    ).length;
  }

  showNotify = false;
}
