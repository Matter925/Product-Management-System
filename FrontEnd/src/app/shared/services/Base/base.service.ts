import {
  HttpClient,
  HttpHeaders,
  HttpParams,
  HttpResponse,
} from '@angular/common/http';
import { Injectable } from '@angular/core';
import {
  BehaviorSubject,
  Observable,
  catchError,
  map,
  of,
  shareReplay,
  tap,
  throwError,
} from 'rxjs';
import {
  IMetaData,
  IPaginationResult,
} from '../../models/interfaces/PaginationResult';
import { environment } from '../../../../environments/environment';
import { Router } from '@angular/router';
import { LanguageService } from '../Language/LanguageService';
import { CompanyInfo } from '../../models/interfaces/CompanyInfo';

@Injectable({
  providedIn: 'root',
})
export class BaseService {
  public currentLanguage: string = 'ar';
  private readonly apiUrl: string = environment.API_URL;

  constructor(
    private http: HttpClient,
    private router: Router,
    private languageService: LanguageService
  ) {
    this.languageService.currentLanguage$.subscribe((lang) => {
      if (lang != null) {
        this.currentLanguage = lang;
      }
    });
  }

  splitTextByLanguage(text: string): string {
    const [englishText, arabicText] = text
      .split('~')
      .map((part) => part.trim());
    return this.currentLanguage === 'ar'
      ? arabicText || text
      : englishText || text;
  }

  isActive(route: string): boolean {
    const urlTree = this.router.createUrlTree([
      `/${this.currentLanguage}/${route}`,
    ]);

    const isExactMatch = this.router.isActive(urlTree, {
      paths: 'exact',
      queryParams: 'exact',
      fragment: 'ignored',
      matrixParams: 'ignored',
    });

    const containsKeyword = this.router.url.includes(route);

    return isExactMatch || containsKeyword;
  }

  getAll<TModel>(
    endpoint: string,
    pageSize?: number,
    pageNumber?: number,
    filterQuery?: string,
    asc?: boolean,
    orderBy?: string
  ): Observable<IPaginationResult<TModel>> {
    let params = new HttpParams();

    // Add parameters to HttpParams if they are defined
    if (pageSize) {
      params = params.set('PageSize', pageSize.toString());
    }
    if (pageNumber) {
      params = params.set('PageNumber', pageNumber.toString());
    }
    if (orderBy) {
      params = params.set('OrderBy', orderBy);
    }
    if (asc) {
      params = params.set('Asc', asc.toString());
    }
    if (filterQuery) {
      params = params.set('FilterQuery', filterQuery);
    }
    var endpointWithQuery: string = '';
    if (params.toString().length > 0) {
      endpointWithQuery = `${this.apiUrl}${endpoint}?${params.toString()}`;
    } else {
      endpointWithQuery = `${this.apiUrl}${endpoint}`;
    }

    return this.http
      .get<TModel[]>(endpointWithQuery, { observe: 'response' })
      .pipe(
        map((response: HttpResponse<TModel[]>) =>
          this.extractPaginationData(response)
        ),
        catchError(this.handleError.bind(this))
      );
  }

  getLookup<TModel>(endpoint: string): Observable<TModel[]> {
    const endpointWithQuery = `${this.apiUrl}${endpoint}`;

    return this.http
      .get<TModel[]>(endpointWithQuery, { observe: 'response' })
      .pipe(
        map((response: HttpResponse<TModel[]>) => {
          return response.body || []; // Extract data from the response body or return empty array
        }),
        catchError(this.handleError.bind(this))
      );
  }
  getById<TModel>(endPoint: string): Observable<TModel> {
    return this.http.get<TModel>(`${this.apiUrl}${endPoint}`);
  }

  getModel<TModel>(endPoint: string): Observable<TModel> {
    return this.http.get<TModel>(`${this.apiUrl}${endPoint}`);
  }

  create(endPoint: string, model: any): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}${endPoint}`, model);
  }

  update(endPoint: string, model: any): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}${endPoint}`, model);
  }
  delete(endPoint: string): Observable<any> {
    return this.http.delete<any>(`${this.apiUrl}${endPoint}`);
  }
  uploadFiles(Id: number, files: File[], entityName: string): Observable<any> {
    const formData = new FormData();
    files.forEach((file) => formData.append('files', file, file.name));

    return this.http
      .post<any>(
        `${this.apiUrl}Files/UploadMixedFiles?entityName=${entityName}&id=${Id}`,
        formData,
        {
          headers: new HttpHeaders({
            // Add your headers here if needed
          }),
        }
      )
      .pipe(
        catchError((error) => {
          console.error('Error uploading files', error);
          throw error;
        })
      );
  }
  uploadFile(Id: number, file: File, entityName: string): Observable<any> {
    const formData = new FormData();
    formData.append('files', file, file.name);

    return this.http
      .post<any>(
        `${this.apiUrl}Files/UploadFile?entityName=${entityName}&id=${Id}`,
        formData,
        {
          headers: new HttpHeaders({
            // Add your headers here if needed
          }),
        }
      )
      .pipe(
        catchError((error) => {
          console.error('Error uploading files', error);
          throw error;
        })
      );
  }
  private extractPaginationData<TModel>(
    response: HttpResponse<TModel[]>
  ): IPaginationResult<TModel> {
    const paginationHeader = response.headers.get('x-pagination');

    if (paginationHeader) {
      const parsedPagination: IMetaData = JSON.parse(paginationHeader);
      return {
        data: response.body ?? [],
        totalCount: parsedPagination.TotalCount,
        totalPages: parsedPagination.TotalPages,
        currentPage: parsedPagination.CurrentPage,
        pageSize: parsedPagination.PageSize,
      };
    } else {
      return {
        data: response.body ?? [],
        totalCount: 0,
        totalPages: 0,
        currentPage: 0,
        pageSize: 0,
      };
    }
  }

  private handleError(error: any): Observable<never> {
    return throwError(() => new Error(error.message || 'Server error'));
  }
  private companyInfoSubject = new BehaviorSubject<CompanyInfo | null>(null);
  companyInfo$ = this.companyInfoSubject.asObservable();
  private dataLoaded = false;
  private companyInfoRequest$: Observable<CompanyInfo | null> | null = null;

  getCompanyInfo(): Observable<CompanyInfo | null> {
    if (this.dataLoaded) {
      return this.companyInfo$;
    }

    if (!this.companyInfoRequest$) {
      this.companyInfoRequest$ = this.http
        .get<CompanyInfo>(`${this.apiUrl}CompanyInfo`)
        .pipe(
          tap((data) => {
            this.companyInfoSubject.next(data);
            this.dataLoaded = true;
          }),
          catchError((error) => {
            console.error('Error fetching company info:', error);
            return of(null);
          }),
          shareReplay(1)
        );
    }

    return this.companyInfoRequest$;
  }
}
