import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { SharedModule } from 'primeng/api';
import { LoadingComponent } from '../../shared/components/loading/loading.component';
import { BaseService } from '../../shared/services/Base/base.service';
import { ActivatedRoute, Router } from '@angular/router';
import { ProductModel } from '../../shared/models/interfaces/Product';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-create',
  imports: [CommonModule,
      ReactiveFormsModule,
      SharedModule,
      LoadingComponent],
  templateUrl: './create.component.html',
  styleUrl: './create.component.scss'
})

export class CreateComponent implements OnInit {
  contactForm!: FormGroup;
  isLoading:boolean=false;
  id: number | null = null;
  constructor(
    private fb: FormBuilder,
    public baseService: BaseService,
    private router: Router,
    private route: ActivatedRoute,
    private toastr: ToastrService
  ) {
    const idParam = this.route.snapshot.paramMap.get('id');
    this.id = idParam ? +idParam : null; // Convert id to number or set to null if not present  
    }
    ngOnInit() {
      this.initializeForm(); // Initialize the form
      const idParam = this.route.snapshot.paramMap.get('id'); // Get the product ID from the route
      this.id = idParam ? +idParam : null; // Convert id to number or set to null if not present  
    
      if (this.id) {
        this.loadProduct(this.id); // Load product data for editing if id is present
      }
    }

  private initializeForm(): void {
    this.contactForm = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(100), Validators.minLength(3)]],
    price: ['', [Validators.required, Validators.pattern(/^\d+(\.\d{1,2})?$/), Validators.min(0)]], // Regex for decimal validation
    description: ['', [Validators.required, Validators.maxLength(500), Validators.minLength(10)]],
    });
  }
  private checkForEdit(): void {
    this.loadProduct(this.id!); // Load product data for editing
  }

  private loadProduct(id: number): void {
    this.baseService.getById<ProductModel>(`Products/${id}`).subscribe({
      next: (product) => {
        // Patch the form with the product data
        this.contactForm.setValue({
          name: product.name,
          price: product.price,
          description: product.description,
        });
      },
      error: (error) => {
        this.toastr.error('Error loading product:', error);
      }
    });
  }
  
  onSubmit(): void {
    this.isLoading=true;
    if (this.contactForm.invalid) {
      this.contactForm.markAllAsTouched();
      this.toastr.success('invalid!'); // Show success notification
      this.isLoading = false;
      return;
    }else{
      const productData: ProductModel = this.contactForm.value;
      if (this.id) {
        // Update existing product
        this.baseService.update(`Products/${this.id}`, productData).subscribe({
          next: () => {
            this.toastr.success('Product updated successfully!'); // Show success notification
            this.router.navigate([this.baseService.currentLanguage, '/products']);
          },
          error: (error) => {
            this.toastr.error('Error updating product:', error); // Show error notification
          },
          complete: () => {
            this.isLoading = false;
          },
        });
      } else {
      this.baseService.create('Products', productData).subscribe({
        next: (res) => {
          this.toastr.success('Product Added successfully!'); // Show success notification
          this.router.navigate([this.baseService.currentLanguage, '/products']);
        },
        error: (error) => {
          this.toastr.success('Error sending message:',error); // Show success notification
        },
        complete: () => {
          this.isLoading = false;
        },
      });
    }
    }
    
  }

}
