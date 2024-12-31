import { Component, EventEmitter, Input, OnChanges, OnInit, Output, SimpleChanges } from '@angular/core';
import { IMetaData } from '../../models/interfaces/PaginationResult';
import { PageEvent } from '@angular/material/paginator';
import { CommonModule } from '@angular/common'; 
@Component({
  selector: 'app-paginator',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './paginator.component.html',
  styleUrls: ['./paginator.component.scss']
})
export class PaginatorComponent implements OnInit, OnChanges {

  @Input() PaginationMetaData: IMetaData | any;
  @Input() currentLanguage: string = 'ar';
  @Output() page = new EventEmitter<PageEvent>();

  Pages: number[] = [];

  ngOnInit(): void {
    this.generatePageNumbers();
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes['PaginationMetaData']) {
      this.generatePageNumbers();
    }
  }

  generatePageNumbers() {
    if (this.PaginationMetaData?.TotalPages) {
      this.Pages = Array.from({ length: this.PaginationMetaData.TotalPages }, (_, i) => i + 1);
    }
  }

  hasPreviousPage(): boolean {
    return this.PaginationMetaData?.CurrentPage > 1;
  }

  hasNextPage(): boolean {
    return this.PaginationMetaData?.CurrentPage < this.PaginationMetaData?.TotalPages;
  }

  nextPage() {
    if (this.hasNextPage()) {
      this.onChangePage(this.PaginationMetaData.CurrentPage + 1);
    }
  }

  prevPage() {
    if (this.hasPreviousPage()) {
      this.onChangePage(this.PaginationMetaData.CurrentPage - 1);
    }
  }

  onChangePage(pageNumber: number) {
    this.PaginationMetaData.CurrentPage = pageNumber;

    const pageEvent: PageEvent = {
      pageIndex: pageNumber - 1,
      previousPageIndex: this.PaginationMetaData.CurrentPage - 2,
      pageSize: this.PaginationMetaData.PageSize,
      length: this.PaginationMetaData.TotalCount
    };
    window.scrollTo({ top: 0, behavior: 'smooth' });
    this.page.emit(pageEvent);
  }
}