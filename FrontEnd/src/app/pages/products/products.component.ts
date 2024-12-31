import { AfterViewInit, Component, OnInit } from '@angular/core';
import { environment } from '../../../environments/environment';
import { IMetaData, IPaginationResult } from '../../shared/models/interfaces/PaginationResult';
import { ProductModel } from '../../shared/models/interfaces/Product';
import { BaseService } from '../../shared/services/Base/base.service';
import { PageEvent } from '@angular/material/paginator';
import { Table, TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { SharedModule } from 'primeng/api';
import { IconFieldModule } from 'primeng/iconfield';
import { InputIconModule } from 'primeng/inputicon';
import { InputTextModule } from 'primeng/inputtext';
import { FormsModule } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { Router } from '@angular/router';

@Component({
  selector: 'app-products',
  imports: [TableModule,ButtonModule,SharedModule,IconFieldModule, InputIconModule,InputTextModule,FormsModule],
  templateUrl: './products.component.html',
  styleUrl: './products.component.scss',
})

export class ProductsComponent implements OnInit{
  loading: boolean = true;
  errorMessage: string | null = null;
  searchQuery: string = ''; // Stores the search input
  currentPage: number = 1;
  totalPages: number = 1;
  pageSize: number = 10;
  first: number = 0; // Current index of the first item on the page
  paginationMetaData?: IMetaData;
  products: ProductModel[] = [];  
  totalCount: number = 0;
  sortField: string = 'id'; // Default sort field
  isAscending: boolean = true; // Default sort order: true for ascending, false for descending
  constructor( public baseService: BaseService,private toastr: ToastrService, private router: Router) {
  }
  ngOnInit(): void {
    this.loadProducts(this.pageSize, this.currentPage);
  }
 
  handlePageEvent(event: any): void {
    const { rows, page } = event;
    this.pageSize = rows; // Set new page size
    this.currentPage = page + 1; // Convert to 1-based index for the backend
    this.loadProducts(this.pageSize, this.currentPage);
  }
  

loadProducts(pageSize: number = this.pageSize, pageNumber: number = this.currentPage, filterQuery: string = this.searchQuery) {
  const filter = this.searchQuery 
    ? ` 
        (id == ${Number(this.searchQuery) ? this.searchQuery : 'null'}) OR 
        (price == ${Number(this.searchQuery) ? this.searchQuery : 'null'}) OR 
        (name.Contains("${this.searchQuery}") OR 
        description.Contains("${this.searchQuery}")) 
      ` 
    : '';

  this.baseService
    .getAll<ProductModel>(
      'Products',
      pageSize,
      pageNumber,
      filter, // No filter query provided
      this.isAscending,
      this.sortField
    )
    .subscribe({
      next: (result: IPaginationResult<ProductModel>) => {
        this.products = result.data || [];
        this.totalCount = result.totalCount; // Update total records
        this.currentPage = result.currentPage; // Update current page
        this.pageSize = result.pageSize; // Update page size
        this.totalPages=result.totalPages
        this.paginationMetaData = {
          TotalPages: result.totalPages,
          TotalCount: result.totalCount,
          CurrentPage: result.currentPage,
          PageSize: result.pageSize,
        };
      },
      error: (error) => {
        console.error('Error loading products:', error);
        this.errorMessage = 'Failed to load products.';
      },
      complete: () => {
        this.loading = false;
      },
    });
}

  handleSortEvent(event: any): void {
    this.sortField = event.field;
    this.isAscending = event.order === 1; // PrimeNG uses 1 for ascending and -1 for descending
    this.loadProducts(this.pageSize, this.currentPage, this.searchQuery);
  }
  editProduct(product: ProductModel): void {
    console.log('Editing product:', product); // Log the product
  if (product && product.id) {
    console.log('Navigating to edit for product ID:', product.id);
    this.router.navigate([this.baseService.currentLanguage, `create/${product.id}`]).then(success => {
      if (!success) {
        console.error('Navigation failed.');
      }
    });
  } else {
    console.error('Invalid product or product ID.');
  }

  }
  onSearch(event: Event): void {
    const inputElement = event.target as HTMLInputElement; // Cast to HTMLInputElement
    this.searchQuery = inputElement.value; // Get the value from the input
    this.currentPage = 1; // Reset to the first page
    this.loadProducts(); // Load products with the search query
}

  clearSearch(): void {
    this.searchQuery = '';
    this.currentPage = 1; // Reset to the first page
    this.loadProducts();
  }
  deleteProduct(productId: number): void {
    if (confirm('Are you sure you want to delete this product?')) {
      this.baseService.delete(`Products/${productId}`).subscribe({
        next: () => {
          this.toastr.success('Product deleted successfully!'); // Show success notification
          this.loadProducts(this.pageSize, this.currentPage);
        },
        error: (error) => {
          this.toastr.error('Failed to delete product!'); // Show error notification
        },
      });
    }
  }
  addItem() {
    // Logic to add the new item
    this.router.navigate(['/',this.baseService.currentLanguage, 'create']);  
  }
  goToFirstPage(): void {
    this.currentPage = 1; // Set to the first page
    this.loadProducts(this.pageSize, this.currentPage); // Reload products for the first page
}

goToLastPage(): void {
    this.currentPage = this.paginationMetaData?.TotalPages || 1; // Set to the last page
    this.loadProducts(this.pageSize, this.currentPage); // Reload products for the last page
}
}

