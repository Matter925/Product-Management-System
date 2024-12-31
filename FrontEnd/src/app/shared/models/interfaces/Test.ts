export interface TestViewModel {
  id: number;
  nameEn: string;
  nameAr: string;
  price: number;
  oldPrice: number;
  tat: number;
  image: string;
  descEn: string;
  descAr: string;
  testsFeatures: TestsFeatureViewModel[];
}
export interface TestsFeatureViewModel {
  name: string;
  orderIndex?: number; // Optional property
}
