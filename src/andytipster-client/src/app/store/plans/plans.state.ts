import { EntityState } from '@ngrx/entity';

export type BillingCycle = 'Weekly' | 'Monthly' | 'Quarterly' | 'SemiAnnual' | 'Annual';
export type Currency = 'GBP' | 'EUR' | 'USD';
export type PlanSyncStatus = 'Synced' | 'SyncPending' | 'SyncFailed';

export interface Plan {
  id: string;
  name: string;
  price: number;
  currency: Currency;
  billingCycle: BillingCycle;
  features: string[];
  trialDays: number | null;
  setupFee: number;
  gracePeriodDays: number;
  autoRenewal: boolean;
  promoCodeCompatible: boolean;
  isArchived: boolean;
  paypalPlanId: string | null;
  syncStatus: PlanSyncStatus;
  createdAt: string;
  updatedAt: string;
}

export interface PlansState extends EntityState<Plan> {
  isLoading: boolean;
  error: string | null;
  selectedPlanId: string | null;
}
