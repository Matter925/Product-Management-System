import { BaseService } from './../../../shared/services/Base/base.service';
import { Component, Input, OnInit } from '@angular/core';
import { SharedModule } from '../../../shared/shared.module';
import { CompanyInfo } from '../../../shared/models/interfaces/CompanyInfo';
import { DepartmentModel } from '../../../shared/models/interfaces/Department';

@Component({
  selector: 'app-footer',
  standalone: true,
  imports: [SharedModule],
  templateUrl: './footer.component.html',
  styleUrl: './footer.component.scss',
})
export class FooterComponent implements OnInit {
  companyInfo: CompanyInfo | null = null;
  loading: boolean = true;
  error: string | null = null;
  departments: DepartmentModel[] = [];  
  constructor(public BaseService: BaseService) {}

  ngOnInit(): void {
    this.fetchCompanyInfo();
    this.loadDepartments()
  }

  fetchCompanyInfo(): void {
    this.BaseService.getCompanyInfo().subscribe((info) => {
      this.companyInfo = info;
    });
  }

  loadDepartments() {

    this.loading = true; // Start loading
    this.BaseService.getAll<DepartmentModel>('Departments').subscribe({
      next: (response) => {
        // Extract the first four items from the response data
        this.departments =response.data ?? [];
      },
      error: (error) => {
        
      },
      complete: () => {
        this.loading = false; // Stop loading when complete
      },
    });
    }

}
