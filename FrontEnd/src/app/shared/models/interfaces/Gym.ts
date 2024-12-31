import { PlansViewModel } from './Plans';

export interface GymViewModel {
  id: number;
  nameEn: string;
  nameAr: string;
  description: string;
  image: string;
  isActive: boolean;
  images?: string[];
}

export interface GymDetailsViewModel {
  id: number;
  nameEn: string;
  nameAr: string;
  description: string;
  image: string;
  isActive: boolean;
  images?: string[];
  plans: PlansViewModel[];
}

export interface Names {
  id: number;
  nameEn: string;
  nameAr: string;
}

export interface BranchesNames {
  id: number;
  nameEn: string;
  nameAr: string;
  gymId: number;
}
