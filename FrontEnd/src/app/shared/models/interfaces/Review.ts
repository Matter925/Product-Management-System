export interface CreateReviewDto {
  ClientId: number;
  OrderId?: number;
  OrderItemId?: number;
  KitchenId?: number;
  GymId?: number;
  ClinicId?: number;
  ConsultationId?: number;
  CreatedAt: string;
  Rate: number;
  Description?: string;
  DoctorId?: number;
  CoachId?: number;
  NutritionistId?: number;
  PhysioTherapistId?: number;
  CosmeticSpecialistId?: number;
}