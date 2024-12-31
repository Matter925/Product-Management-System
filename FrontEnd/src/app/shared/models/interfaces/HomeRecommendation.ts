export interface HomeRecommendationFeatures {
  homeRecommendationId: number; 
  name: string;        
  orderIndex: number; 
}
export interface HomeRecommendationDetails {
  id: number;                           
  title: string;                       
  image: string;                       
  clientId: number | null; 
  typeId: number; 
  gymId:number;
  kitchenId:number;                     
  homeRecommendationFeatures: HomeRecommendationFeatures[];
}