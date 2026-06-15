import { EntityState } from '@ngrx/entity';

export type BillingCycle = 'Weekly' | 'Monthly' | 'Quarterly' | 'SemiAnnual' | 'Annual';
export type Currency = 'GBP' | 'EUR' | 'USD';
export type PlanSyncStatus = 'Synced' | 'SyncPending' | 'SyncFailed';

export interface Plan {
  id: string;
  name: string;
  slug: string;
  price: number;
  currency: Currency;
  billingCycle: BillingCycle;
  features: string[];
  trialPeriodDays: number;
  setupFee: number;
  gracePeriodDays: number;
  autoRenew: boolean;
  promoCodeCompatible: boolean;
  isActive: boolean;
  isArchived: boolean;
  payPalPlanId: string | null;
  stripePriceId: string | null;
  syncStatus: PlanSyncStatus;
  upgradePaths: string[];
  downgradePaths: string[];
  createdAt: string;
  updatedAt: string | null;
}

export interface PlansState extends EntityState<Plan> {
  isLoading: boolean;
  error: string | null;
  selectedPlanId: string | null;
}
