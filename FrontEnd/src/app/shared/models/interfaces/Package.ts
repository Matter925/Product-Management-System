export interface Package {
  id: number;
  nameEn: string;
  nameAr: string;
  price: number;
  priceBefore: number;
  image: string;
}

export interface PackageDetails {
  id: number;
  nameEn: string;
  nameAr: string;
  price: number;
  priceBefore: number;
  isActive: boolean;
  description: string;
  image: string;
  packageTests: PackageTest[];
  packageProfessionals: PackageProfessional[];
}

export interface PackageTest {
  testId: number;
  nameEn: string;
  nameAr: string;
  image: string;
}

export interface PackageProfessional {
  id: number;
  packageId: number;
  doctorId?: number;
  coachId?: number;
  cosmeticSpecialistId?: number;
  nutritionistId?: number;
  physioTherapistId?: number;
  coach?: ConsultationConsultant;
  cosmeticSpecialist?: ConsultationConsultant;
  doctor?: ConsultationConsultant;
  nutritionist?: ConsultationConsultant;
  physioTherapist?: ConsultationConsultant;
}

export interface ConsultationConsultant {
  id: number;
  nameEn: string;
  nameAr: string;
  specialty: string;
  image: string;
}
