import { ClientViewModel } from "./Client";
import { LookupViewModel } from "./Lookup";
import { LookupDto } from "./Order";

export interface ConsultationViewModel {
    id: number;
    consultationDateTime: string; // ISO 8601 date format
    zoomURL?: string; // Optional property
    currentState: LookupViewModel;
    consultant: ConsultantViewModel;
    paymentMethod: LookupViewModel;
    type:number;
    client:ClientViewModel,
    states: LookupDto[];
  }
  export interface ConsultantViewModel {
    id: number;
    nameEn: string;
    nameAr: string;
    specialty: string;
    image: string;
   
}