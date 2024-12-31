import { Names } from './Gym';
import { PlansViewModel } from './Plans';

export interface CreateQuickOrderDto {
  clientId: number;
  paymentTypeId: number;
  selectedTime?: string;
  selectedDate?: string;
  couponId?: number;
  testId?: number;
  planId?: number;
  addressId?: number;
}

export interface OrderPaymentDto {
  id: number;
  remainingPaymentAmount: number;
  paymentTypeId: number;
}
export interface ConsultationPaymentDto {
  id: number;
  feesWithVAT: number;
  paymentMethodId: number;
}

export interface OrderDto {
  orderId: number;
  id: number;
  currentOrderState: LookupDto;
  image: string;
  appointmentDate?: Date;
  test: TestForOrderDto;
}

export interface OrdersItemPlansDto {
  id: number;
  orderId: number;
  plan?: PlanForOrderDto;
  clinicProgram?: PlanForOrderDto;
  kitchenProgram?: PlanForOrderDto;
  branch?: Names;
  startingDate: Date;
  endingDate: Date;
  status: string;
  classtatus: string;
  isReorderDisabled: boolean;
}

export interface PlanForOrderDto {
  id: number;
  nameEn: string;
  nameAr: string;
  image: string;
  base?: BaseGymDto;
}

export interface OrderResultDto {
  id: number;
  image: string;
  result: string;
  nameAr: string;
  nameEn: string;
}

export interface BaseGymDto {
  id: number;
  nameEn: string;
  nameAr: string;
  image: string;
}

export interface TestForOrderDto {
  nameEn: string;
  nameAr: string;
}

export interface OrdersItemDto {
  id: number;
  startingDate: string;
  endingDate: string;
  priceAfter: number;
  priceBefore: number;
  plan: PlanForOrderDto;
  test: TestForOrderDto;
}

export interface OrderItemOnlyDto {
  id: number;
  ordersItems: OrdersItemDto[];
}

export interface OrderTestDetailsDto {
  orderId: number;
  id: number;
  appointmentDate?: Date;
  name: string;
  currentOrderState: LookupDto;
  paymentType: LookupDto;
  test: TestForOrderDto;
  address: string;
  states: LookupDto[];
}

export interface OrderTestItemsDto {
  id: number;
  priceBefore: number;
  priceAfter: number;
  test: TestForOrderDto;
}

export interface ClientNameDto {
  id: number;
  firstName: string;
  middleName?: string;
  lastName: string;
}

export interface LookupDto {
  id: number;
  name: string;
  orderIndex?: number;
}

export interface TestForOrderDto {
  id: number;
  nameAr: string;
  nameEn: string;
  image: string;
}
