import { LookupViewModel } from "../classes/LookupViewModel"
import { WorkingTimesViewModel } from "./WorkingTimes"

export interface ClinicsViewModel{

        id: number,
        nameEn: string,
        nameAr: string,
        image?: string,
        description?: string,
        clinicSpecialty: LookupViewModel,
        clinicsPrograms: ClinicsProgramsViewModel [],
        clinicsWorkingTimes:WorkingTimesViewModel [] ,
        images:string[],
        isAvailable : boolean;
        avialability: string,
        minPrice: number,
        city?: LookupViewModel
      
}

export interface ClinicsProgramsViewModel {
        id: number,
        nameEn: string,
        nameAr: string,
        image?: string,
        description?:string,
        price : number,
        priceBefore : number,
        durationInDays: number,
        sessionsCount: number,
        isActive : boolean,
        clinicId: number
}
