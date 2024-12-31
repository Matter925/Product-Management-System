import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { IPostResponse } from '../../models/interfaces/Response';
import { environment } from '../../../../environments/environment';
import { jwtDecode } from 'jwt-decode';
import { SignalRService } from '../SignalR/signal-r.service';
@Injectable({
  providedIn: 'root',
})
export class AuthService {
  public currentUserSubject = new BehaviorSubject<any>(null);
  private readonly apiUrl: string;
  constructor(
    private signalRService: SignalRService,
    private http: HttpClient
  ) {
    this.apiUrl = environment.API_URL;
    const token = this.getToken();
    if (token) {
      const decodedUser = this.getUserInfoFromToken(token);
      this.currentUserSubject.next(decodedUser);
    }
  }

  login(model: any, endPoint: string): Observable<IPostResponse> {
    return this.http.post<IPostResponse>(`${this.apiUrl}${endPoint}`, model);
  }

  Register(model: any, endPoint: string): Observable<IPostResponse> {
    return this.http.post<IPostResponse>(`${this.apiUrl}${endPoint}`, model);
  }

  sendOTP(phone: string, endPoint: string): Observable<IPostResponse> {
    const encodedPhone = encodeURIComponent(phone);
    return this.http.post<IPostResponse>(
      `${this.apiUrl}${endPoint}?phone=${encodedPhone}`,
      {}
    );
  }

  verifyOTP(model: any, endPoint: string): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}${endPoint}`, model);
  }

  getToken(): string | null {
    return localStorage.getItem('userToken');
  }

  getUserInfoFromToken(token: string): any {
    try {
      if (token !== null) {
        const decoded = jwtDecode(token);
        return decoded;
      } else {
        return null;
      }
    } catch (error) {
      return null;
    }
  }
  getCurrentUser() {
    return this.currentUserSubject.asObservable();
  }
  setCurrentUser(user: any): void {
    this.currentUserSubject.next(user);
  }

  logout(): void {
    localStorage.removeItem('userToken');
    this.currentUserSubject.next(null);
    this.signalRService.stopConnection();
  }
}
