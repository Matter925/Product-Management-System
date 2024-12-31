import { LookupViewModel } from './Lookup';

export interface ClientViewModel {
  id: number;
  firstName: string;
  middleName: string | null;
  lastName: string;
  idNumber: string;
  nationality: LookupViewModel;
  language: LookupViewModel;
  relationship: LookupViewModel;
  birthDate: string;
  phoneNumber: string;
  phone :string;
  email: string;
  image: string;
  isMale: boolean;
}

export interface CreateProfileDto {
  firstName: string;
  middleName: string;
  lastName: string;
  idNumber: string;
  isMale: boolean;
  nationalityId: number;
  languageId: number;
  birthDate: string;
}

export interface UpdateClient {
  firstName: string;
  middleName: string;
  lastName: string;
  idNumber: string;
  isMale: boolean;
  nationalityId: number;
  languageId: number;
  birthDate: string;
  email: string;
}

export interface ClientProfileDto {
  id: number;
  firstName: string;
  lastName: string;
  image: string;
  relationship: LookupViewModel;
}

export interface UpdateClientDto {
  firstName: string;
  middleName: string;
  lastName: string;
  idNumber: string;
  isMale: boolean;
  nationalityId: number;
  languageId: number;
  relationshipId: number;
  birthDate: string;
  email: string;
}
