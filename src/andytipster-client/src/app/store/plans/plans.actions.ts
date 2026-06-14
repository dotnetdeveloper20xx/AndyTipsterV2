import { createActionGroup, emptyProps, props } from '@ngrx/store';
import { Plan } from './plans.state';

export const PlansActions = createActionGroup({
  source: 'Plans',
  events: {
    'Load Plans': emptyProps(),
    'Load Plans Success': props<{ plans: Plan[] }>(),
    'Load Plans Failure': props<{ error: string }>(),

    'Load Plan': props<{ planId: string }>(),
    'Load Plan Success': props<{ plan: Plan }>(),
    'Load Plan Failure': props<{ error: string }>(),

    'Create Plan': props<{ plan: Partial<Plan> }>(),
    'Create Plan Success': props<{ plan: Plan }>(),
    'Create Plan Failure': props<{ error: string }>(),

    'Update Plan': props<{ planId: string; changes: Partial<Plan> }>(),
    'Update Plan Success': props<{ plan: Plan }>(),
    'Update Plan Failure': props<{ error: string }>(),

    'Archive Plan': props<{ planId: string }>(),
    'Archive Plan Success': props<{ plan: Plan }>(),
    'Archive Plan Failure': props<{ error: string }>(),

    'Sync Plan To PayPal': props<{ planId: string }>(),
    'Sync Plan To PayPal Success': props<{ plan: Plan }>(),
    'Sync Plan To PayPal Failure': props<{ error: string }>(),

    'Select Plan': props<{ planId: string | null }>(),
    'Clear Error': emptyProps(),
  },
});
