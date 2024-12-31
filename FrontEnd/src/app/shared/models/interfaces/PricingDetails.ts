export interface ItemPricingDetail {
    cartItemId: number;
    originalPrice: number;
    newPrice: number;
    discountAmount: number;
    itemType: string;
  }
  
  export interface PricingDetails {
    itemPricingDetails: ItemPricingDetail[];
    totalOriginalPrice: number;
    totalNewPrice: number;
    totalDiscountAmount: number;
  }