import { LookupViewModel } from "./Lookup";

export interface CosmeticViewModel {
  id: number;
  nameEn: string;
  nameAr: string;
  description: string;
  image: string;
  cosmeticClinic?: LookupViewModel;
  email: string;
  isActive: boolean;
  userId: string;
  scientificDegree: string;
  specialty: string;
  yearsOfExperience: number;
  fee: number; 
  languages?: string;
  atHome : boolean
  }

  export interface FilterQuery {
    cityId: number | null;
    specialtyId:number | null;
    search: string;
    date: string;
    
  }