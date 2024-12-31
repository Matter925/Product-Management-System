export interface RecommendationViewModel {
  plans: RecommendedPlanViewModel[];
  tests: RecommendedServiceViewModel[];
  coaches: RecommendedServiceViewModel[];
  nutritionists: RecommendedServiceViewModel[];
  doctors: RecommendedServiceViewModel[];
  physioTherapists: RecommendedServiceViewModel[];
  cosmeticSpecialists: RecommendedServiceViewModel[];
  recommendationsKitchenPrograms: RecommendedServiceViewModel[];
  recommendationsClinicPrograms: RecommendedServiceViewModel[];
}

export interface RecommendedServiceViewModel {
  id: number;
  nameEn: string;
  nameAr: string;
  description: string;
  image: string;
}
export interface RecommendedPlanViewModel extends RecommendedServiceViewModel {
  gymId?: number;
}
