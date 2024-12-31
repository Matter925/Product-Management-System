import { PlansFeatureViewModel } from "./Feature";
import { GymViewModel } from "./Gym";

export interface PlansViewModel{
    id: number,
    nameEn : string;
    nameAr  : string;
    description : string;
    price :number,
    oldPrice  :number,
    priceBefore  :number,
    durationInDays  :number,
    sessionsCount :number,
    isActive  :boolean,
    image  : string,
    gymId:number,
    plansFeatures : PlansFeatureViewModel[];
    gym : GymViewModel
}