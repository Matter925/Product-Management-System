import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { BaseService } from '../Base/base.service';
import { whislistViewModel } from '../../models/interfaces/wishlistViewModel';
import { IPaginationResult } from '../../models/interfaces/PaginationResult';
import { AuthService } from '../auth/auth.service';

@Injectable({
  providedIn: 'root',
})
export class WishlistService {
  private wishlistKey = 'wishlist';

  private wishlitsItems = new BehaviorSubject<whislistViewModel[]>(this.getWishlistItems());
  private wishlistCount = new BehaviorSubject<number>(this.wishlitsItems.value.length || 0);

  wishlistCount$ = this.wishlistCount.asObservable();
  wishlitsItems$ = this.wishlitsItems.asObservable();

  constructor(private baseService: BaseService, private authService: AuthService) {}

  private IntiLocalWishlist(Ids: number[]): whislistViewModel[] {
    return Ids.map((testId) => ({
      testId: testId,
      kitchenProgramId: testId,
      clinicProgramId: testId,
      gymPlanId: testId,
      id: 0,
    }));
  }

 
  private getWishlistItems(): whislistViewModel[] {
    if (this.authService.getToken()) {
        this.baseService.getAll<whislistViewModel>('WishLists').subscribe({
            next: (result: IPaginationResult<whislistViewModel>) => {
                this.wishlitsItems.next(result.data || []);
                this.wishlistCount.next(result.data?.length || 0);
            },
            error: (error) => {
                console.error('Error loading wishlist:', error);
            },
        });
    } else {
        const wishlistLocal: number[] = JSON.parse(localStorage.getItem(this.wishlistKey) || '[]');
        return this.IntiLocalWishlist(wishlistLocal);
    }
    return [];
}
  addToWishlist(product: { testId?: number; KitchenProgramId?: number; ClinicProgramId?: number; GymPlanId?: number }): void {
    const productId = product.testId || product.KitchenProgramId || product.ClinicProgramId || product.GymPlanId;

    if (this.authService.getToken()) {
      this.baseService.create('WishLists', product).subscribe({
        next: () => {
          this.getWishlistItems(); // Refresh the wishlist
        },
        error: (error) => {
          console.error('Error adding to wishlist:', error);
        },
      });
    } else {
      const wishlist = JSON.parse(localStorage.getItem(this.wishlistKey) || '[]');

      if (productId && !wishlist.includes(productId)) {
        wishlist.push(productId);
        localStorage.setItem(this.wishlistKey, JSON.stringify(wishlist));
        this.wishlistCount.next(wishlist.length);
        this.wishlitsItems.next(this.IntiLocalWishlist(wishlist));
      }
    }
  }

  removeFromWishlist(product: { testId?: number; KitchenProgramId?: number; ClinicProgramId?: number; GymPlanId?: number }): void {
    const productId = product.testId || product.KitchenProgramId || product.ClinicProgramId || product.GymPlanId;
  
    if (this.authService.getToken()) {
      const wishlistId = this.wishlitsItems.value.find((w) => 
        w.testId === product.testId || 
        w.kitchenProgramId === product.KitchenProgramId || 
        w.clinicProgramId === product.ClinicProgramId || 
        w.gymPlanId === product.GymPlanId)?.id;
  
      if (wishlistId) {
        this.baseService.delete(`WishLists/${wishlistId}`).subscribe({
          next: () => {
            // Update wishlist locally after successful deletion
            this.wishlitsItems.next(this.wishlitsItems.value.filter((w) => w.id !== wishlistId));
            this.wishlistCount.next(this.wishlitsItems.value.length);
          },
          error: (error) => {
            console.error('Error removing from wishlist:', error);
          },
        });
      }
    } else {
      let wishlist = JSON.parse(localStorage.getItem(this.wishlistKey) || '[]');
      wishlist = wishlist.filter((id: number) => id !== productId);
      localStorage.setItem(this.wishlistKey, JSON.stringify(wishlist));
      this.wishlistCount.next(wishlist.length);
      this.wishlitsItems.next(this.IntiLocalWishlist(wishlist));
    }
  }
  updateServerWishlist() {
    let wishlistLocal = JSON.parse(
      localStorage.getItem(this.wishlistKey) || '[]'
    );

    for (let index = 0; index < wishlistLocal.length; index++) {
      this.baseService
        .create('WishLists', { testId: wishlistLocal[index] })
        .subscribe({
          next: () => {
            wishlistLocal = wishlistLocal.filter(
              (id: number) => id == wishlistLocal[index]
            );
            localStorage.setItem(
              this.wishlistKey,
              JSON.stringify(wishlistLocal)
            );
          },
          error: (error) => {
            console.error('Error loading wishlist:', error);
          },
        });
    }
  }

  intiWhislist() {
    this.wishlitsItems.next(this.getWishlistItems());
    this.wishlistCount.next(this.wishlitsItems.value.length);
  }
}
