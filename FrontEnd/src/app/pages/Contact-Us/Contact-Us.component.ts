import { AfterViewInit, Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { BaseService } from '../../shared/services/Base/base.service';
import { Router } from '@angular/router';
import { ContactUs } from '../../shared/models/interfaces/Contact-Us';
import { CompanyInfo } from '../../shared/models/interfaces/CompanyInfo';
import { ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { SharedModule } from '../../shared/shared.module';
import { LoadingComponent } from '../../shared/components/loading/loading.component';
declare var $: any;

@Component({
  selector: 'app-contact-us',
  templateUrl: './Contact-Us.component.html',
  standalone: true,
  styleUrls: ['./Contact-Us.component.css'],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    SharedModule,
    LoadingComponent
  ],
})
export class ContactUsComponent implements OnInit,AfterViewInit {
  contactForm!: FormGroup;
  isLoading = false;
  companyInfo: CompanyInfo | null = null;
  error: string | null = null;
  constructor(
    private fb: FormBuilder,
    public baseService: BaseService,
    private router: Router
  ) {}
  ngAfterViewInit(): void {
    // Initialize the clients carousel using Owl Carousel
    if ($('.clients-carousel').length) {
      $('.clients-carousel').owlCarousel({
        loop: true,
        margin: 30,
        nav: true,
        smartSpeed: 400,
        autoplay: true,
        navText: ['<span class="flaticon-left"></span>', '<span class="flaticon-right"></span>'],
        responsive: {
          0: {
            items: 1
          },
          480: {
            items: 2
          },
          600: {
            items: 3
          },
          768: {
            items: 4
          },
          1280: {
            items: 5
          }
        }
      });
    }
  }

  ngOnInit() {
    this.initializeForm();
    this.fetchCompanyInfo();
  }

  private initializeForm(): void {
    this.contactForm = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(255)]],
      email: [
        '',
        [Validators.required, Validators.email, Validators.maxLength(255)],
      ],
      phoneNumber: ['', [Validators.required, Validators.maxLength(50)]],
      subject: ['', [Validators.required, Validators.maxLength(255)]],
      message: ['', [Validators.required]],
    });
  }

  onSubmit(): void {
    this.isLoading=true;
    if (this.contactForm.invalid) {
      this.contactForm.markAllAsTouched();
      console.log("invalid")
      this.isLoading = false;
      return;
    }else{
      const contactUsData: ContactUs = this.contactForm.value;
      this.baseService.create('ContactUsMessages', contactUsData).subscribe({
        next: (res) => {
          console.log('Message sent successfully:', res);
          this.router.navigate(['/', this.baseService.currentLanguage, '/home']);
        },
        error: (error) => {
          console.error('Error sending message:', error);
        },
        complete: () => {
          this.isLoading = false;
        },
      });
    }
    
  }


  fetchCompanyInfo(): void {
    this.baseService.getCompanyInfo().subscribe((info) => {
      this.companyInfo = info;
    });
  }
}
