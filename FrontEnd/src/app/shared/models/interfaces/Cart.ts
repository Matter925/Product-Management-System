import { PlansViewModel } from './Plans';
import { TestViewModel } from './Test';

export interface CartViewModel {
  id: number;
  test?: TestViewModel;
  plan?: PlansViewModel;
  kitchenProgram?: TestViewModel;
  clinicProgram?: TestViewModel;
}
