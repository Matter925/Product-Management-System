import { ClientViewModel } from "./Client";
import { ClinicsProgramsViewModel } from "./Clinics";
import { ConsultantViewModel } from "./Consultation";
import { KitchensProgramsViewModel } from "./Kitchens";
import { LookupViewModel } from "./Lookup";
import { PlansViewModel } from "./Plans";
import { TestViewModel } from "./Test";

// InvoiceItemDto.ts
export class InvoiceItemViewModel {
    id: number;
    invoiceId: string;
    testId?: number;
    planId?: number;
    kitchenProgramId?: number;
    clinicProgramId?: number;
    priceBefore: number;
    priceAfter: number;
    clinicProgram?: ClinicsProgramsViewModel;
    kitchenProgram?: KitchensProgramsViewModel;
    plan?: PlansViewModel;
    test?: TestViewModel;

    constructor() {
        this.id = 0;
        this.invoiceId = '';
        this.priceBefore = 0;
        this.priceAfter = 0;
    }
}

// InvoiceDto.ts
export class InvoiceViewModel {
    id: string;
    orderId?: number;
    clientId: number;
    price: number;
    vat: number;
    orderIndex?: number;
    date: Date;
    nurseFees?: number;
    totalDiscount: number;
    priceWithVAT: number;
    paymentTypeId?: number;
    doctorId?: number;
    invoiceTypeId?: number;
    parentInvoiceId?: string;
    isActive?: boolean;
    createdAt?: Date;
    consultationId?: number;
    coachId?: number;
    cosmeticSpecialistId?: number;
    nutritionistId?: number;
    physioTherapistId?: number;
    client: ClientViewModel | undefined;
    consultant: ConsultantViewModel | undefined;
    invoiceType?: LookupViewModel;
    invoicesItems: InvoiceItemViewModel[];
    paymentType?: LookupViewModel;

    constructor() {
        this.id = '';
        this.clientId = 0;
        this.price = 0;
        this.vat = 0;
        this.date = new Date();
        this.totalDiscount = 0;
        this.priceWithVAT = 0;
        this.invoicesItems = [];
    }
}

