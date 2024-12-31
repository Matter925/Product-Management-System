import { AfterViewInit, Component, Input, OnInit } from '@angular/core';
import { SharedModule } from '../../shared/shared.module';
import { environment } from '../../../environments/environment';
import { BaseService } from '../../shared/services/Base/base.service';
import { GalleryModel } from '../../shared/models/interfaces/Gallery';
import { IMetaData, IPaginationResult } from '../../shared/models/interfaces/PaginationResult';
import { PageEvent } from '@angular/material/paginator';
import { PaginatorComponent } from '../../shared/components/paginator/paginator.component';
import { LoadingComponent } from '../../shared/components/loading/loading.component';
declare var $: any;

@Component({
  selector: 'app-gallery',
  standalone: true,
  imports: [SharedModule,PaginatorComponent,LoadingComponent],
  templateUrl: './gallery.component.html',
  styleUrl: './gallery.component.scss'
})
export class GalleryComponent implements OnInit,AfterViewInit{
  loading: boolean = true;
  imageUrl = environment.imageUrl;
  errorMessage: string | null = null;
  currentPage: number = 1;
  totalPages: number = 1;
  pageSize: number = 6;
  visiblePages: number[] = [];
  paginationMetaData?: IMetaData;
  gallery: GalleryModel[] = [];  
  constructor( public baseService: BaseService) {}
  ngOnInit(): void {
    this.loadGallery();
  }
  ngAfterViewInit(): void {
    if($('.lightbox-image').length) {
      $('.lightbox-image').fancybox({
        openEffect  : 'fade',
        closeEffect : 'fade',
        helpers : {
          media : {}
        }
      });
    }  }

  loadGallery(pageSize?: number, pageNumber?: number) {
    this.baseService
    .getAll<GalleryModel>(
      'Gallery',
      pageSize,
      pageNumber
    )
    .subscribe({
      next: (result: IPaginationResult<GalleryModel>) => {
        this.gallery = result.data || [];
        this.paginationMetaData = {
          TotalPages: result.totalPages,
          TotalCount: result.totalCount,
          CurrentPage: result.currentPage,
          PageSize: result.pageSize,
        };
        this.updateVisiblePages();
      },
      error: (error) => {
        console.error('Error loading Tests:', error);
      },
      complete: () => {
        this.loading = false;
      },
    });
      
  }
  
    handlePageEvent(event: PageEvent) {
      this.loadGallery(event.pageSize, event.pageIndex + 1);
    }
    UpdateClinicsAvailability() {
      throw new Error('Method not implemented.');
    }
    updateVisiblePages(): void {
      const pageRange = 3;
      const startPage = Math.max(1, this.currentPage - pageRange);
      const endPage = Math.min(this.totalPages, this.currentPage + pageRange);
      this.visiblePages = [];
      for (let i = startPage; i <= endPage; i++) {
        this.visiblePages.push(i);
      }
    }
    goToPage(page: number): void {
      this.loadGallery(page);
    }
  
    onPrevious(): void {
      if (this.currentPage > 1) {
        this.goToPage(this.currentPage - 1);
      }
    }
  
    onNext(): void {
      if (this.currentPage < this.totalPages) {
        this.goToPage(this.currentPage + 1);
      }
    }
}
