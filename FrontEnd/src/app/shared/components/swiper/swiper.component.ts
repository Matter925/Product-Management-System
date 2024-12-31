import {
  Component,
  CUSTOM_ELEMENTS_SCHEMA,
  ElementRef,
  EventEmitter,
  Input,
  Output,
  TemplateRef,
  ViewChild,
} from '@angular/core';
import { LanguageService } from '../../services/Language/LanguageService';
import { Direction } from '@angular/cdk/bidi';
import { SharedModule } from '../../shared.module';

@Component({
  selector: 'app-swiper',
  standalone: true,
  imports: [SharedModule],
  templateUrl: './swiper.component.html',
  styleUrl: './swiper.component.scss',
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
})
export class SwiperComponent {
  @ViewChild('WmSwiper') swiperContainer!: ElementRef;
  @Input() swiperContent: TemplateRef<any> | null = null;
  @Output() initializeSwiperEvent = new EventEmitter<void>();

  constructor(private languageService: LanguageService) {}

  ngOnInit(): void {
    this.languageService.currentLanguage$.subscribe((lang) => {
      const swiperEl = document.querySelector('swiper-container');
      if (swiperEl?.swiper) {
        const dir: Direction = lang == 'ar' ? 'rtl' : 'ltr';
        swiperEl.swiper.changeLanguageDirection(dir);

        const testSlotElements = document.querySelectorAll(
          '.wm-swiper-slide-slot'
        ) as NodeListOf<HTMLElement>;

        testSlotElements.forEach((element: HTMLElement) => {
          if (element) {
            lang == 'ar'
              ? (element.style.marginRight = '0px')
              : (element.style.marginLeft = '0px');
          }
        });
      }
    });
  }

  public initializeSwiper(): void {
    if (this.swiperContainer?.nativeElement) {
      const swiperElement = this.swiperContainer.nativeElement;
      Object.assign(swiperElement, {
        slidesPerView: 1,
        spaceBetween: 20,
        breakpoints: {
          320: {
            slidesPerView: 1,
            spaceBetween: 10,
          },
          480: {
            slidesPerView: 1.5,
            spaceBetween: 15,
          },
          640: {
            slidesPerView: 2,
            spaceBetween: 20,
          },
          768: {
            slidesPerView: 2.5,
            spaceBetween: 25,
          },
          1024: {
            slidesPerView: 3,
            spaceBetween: 30,
          },
          1280: {
            slidesPerView: 4,
            spaceBetween: 40,
          },
        },
        observer: true,
        navigation: {
          nextEl: '.test-button-next',
          prevEl: '.test-button-prev',
        },
      });
      swiperElement.initialize();
    }
  }
}
