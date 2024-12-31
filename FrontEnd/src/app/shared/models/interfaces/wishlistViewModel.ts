import { ClinicsProgramsViewModel } from './Clinics';
import { KitchensProgramsViewModel } from './Kitchens';
import { PlansViewModel } from './Plans';
import { TestViewModel } from './Test';

export interface whislistViewModel {
  id: number;
  testId?: number;
  clinicProgramId?: number;
  kitchenProgramId?: number;
  gymPlanId?: number;
  clinicProgram?: ClinicsProgramsViewModel;
  gymPlan?: PlansViewModel;
  kitchenProgram?: KitchensProgramsViewModel;
  test?: TestViewModel;
}
