import { Injectable, Injector } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import * as signalR from '@microsoft/signalr';
import { NotificationViewModel } from '../../models/interfaces/Notification';
import { environment } from '../../../../environments/environment';
import { AuthService } from '../auth/auth.service';
import { BaseService } from '../Base/base.service';
import { MatSidenav } from '@angular/material/sidenav'; // Import MatSidenav
import { IPaginationResult } from '../../models/interfaces/PaginationResult';
import { APIConstant } from '../../constant/APIConstant';
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root',
})
export class SignalRService {
  private readonly apiUrl = environment.API_URL;
  private hubConnection: signalR.HubConnection | undefined;
  private notificationsSubject = new BehaviorSubject<NotificationViewModel[]>(
    []
  );
  private connectionStateSubject =
    new BehaviorSubject<signalR.HubConnectionState>(
      signalR.HubConnectionState.Disconnected
    );

  notifications$ = this.notificationsSubject.asObservable();
  connectionState$ = this.connectionStateSubject.asObservable();

  private notificationsLoaded = false;

  constructor(
    private injector: Injector,
    private baseService: BaseService,
    private router: Router
  ) {}

  private get authService(): AuthService {
    return this.injector.get(AuthService);
  }

  startConnection(token: string): void {
    if (this.hubConnection) return;

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${this.apiUrl}NotificationHub`, {
        accessTokenFactory: () => token,
      })
      .withAutomaticReconnect()
      .build();

    this.registerConnectionHandlers(token);

    this.hubConnection
      .start()
      .then(() => {
        this.connectionStateSubject.next(signalR.HubConnectionState.Connected);
        this.registerSignalHandlers();
      })
      .catch((err) => this.handleConnectionError(err, token));
  }

  private registerConnectionHandlers(token: string): void {
    if (!this.hubConnection) return;

    this.hubConnection.onreconnecting(() =>
      this.connectionStateSubject.next(signalR.HubConnectionState.Reconnecting)
    );

    this.hubConnection.onreconnected(() =>
      this.connectionStateSubject.next(signalR.HubConnectionState.Connected)
    );

    this.hubConnection.onclose(() => {
      this.connectionStateSubject.next(signalR.HubConnectionState.Disconnected);
      this.retryConnection(token);
    });
  }

  private handleConnectionError(err: any, token: string): void {
    this.connectionStateSubject.next(signalR.HubConnectionState.Disconnected);
    const statusCode = this.extractStatusCodeFromError(err);

    if (statusCode === 401) {
      this.authService.logout();
      this.router.navigate(['/', this.baseService.currentLanguage, '/login']);
    } else {
      this.retryConnection(token);
    }
  }

  private extractStatusCodeFromError(err: any): number | undefined {
    try {
      const errorMatch = err.message.match(/"statusCode":(\d+)/);
      return errorMatch && errorMatch[1]
        ? parseInt(errorMatch[1], 10)
        : undefined;
    } catch (parseError) {
      console.error('Failed to parse error message:', parseError);
      return undefined;
    }
  }

  private retryConnection(token: string): void {
    setTimeout(() => this.startConnection(token), 5000);
  }

  private registerSignalHandlers(): void {
    if (!this.hubConnection) return;

    this.hubConnection.on(
      'ReceiveNotification',
      (notification: NotificationViewModel) => {
        const currentNotifications = this.notificationsSubject.value;
        this.notificationsSubject.next([notification, ...currentNotifications]);
      }
    );
  }

  stopConnection(): void {
    if (this.hubConnection) {
      this.hubConnection
        .stop()
        .then(() =>
          this.connectionStateSubject.next(
            signalR.HubConnectionState.Disconnected
          )
        )
        .catch((err) =>
          console.error('Error stopping SignalR connection:', err)
        );
    }
  }

  loadNotifications(): void {
    if (this.notificationsLoaded) return;

    this.baseService
      .getAll<NotificationViewModel>('Notifications')
      .subscribe({
        next: (result: IPaginationResult<NotificationViewModel>) => {
          this.notificationsSubject.next(result.data || []);
          this.notificationsLoaded = true;
        },
        error: (error) => console.error('Error loading notifications:', error),
      });
  }

  markNotificationAsRead(notificationId: number): void {
    this.baseService
      .update(`${this.apiUrl}/notifications/${notificationId}`, { seen: true })
      .subscribe({
        next: () => {
          const updatedNotifications = this.notificationsSubject.value.map(
            (notification) =>
              notification.id === notificationId
                ? { ...notification, seen: true }
                : notification
          );
          this.notificationsSubject.next(updatedNotifications);
        },
        error: (error) =>
          console.error('Error marking notification as read:', error),
      });
  }
}
