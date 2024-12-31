import { LookupViewModel } from "./Lookup";

export interface KitchensViewModel{
        id: number;
        nameEn: string;
        nameAr: string;
        description: string;
        image: string;
        isActive: boolean;
        email: string;
        userId: string;
        images:string[],
        city:LookupViewModel,
        Address:string,
        kitchensPrograms: KitchensProgramsViewModel [],
}
export interface KitchensProgramsViewModel {
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
        kitchenId: number
}

