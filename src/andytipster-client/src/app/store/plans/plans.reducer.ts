import { createEntityAdapter, EntityAdapter } from '@ngrx/entity';
import { createReducer, on } from '@ngrx/store';
import { PlansActions } from './plans.actions';
import { Plan, PlansState } from './plans.state';

export const plansAdapter: EntityAdapter<Plan> = createEntityAdapter<Plan>({
  selectId: (plan) => plan.id,
  sortComparer: (a, b) => a.name.localeCompare(b.name),
});

export const initialPlansState: PlansState = plansAdapter.getInitialState({
  isLoading: false,
  error: null,
  selectedPlanId: null,
});

export const plansReducer = createReducer(
  initialPlansState,

  on(PlansActions.loadPlans, (state): PlansState => ({
    ...state,
    isLoading: true,
    error: null,
  })),

  on(PlansActions.loadPlansSuccess, (state, { plans }): PlansState =>
    plansAdapter.setAll(plans, { ...state, isLoading: false, error: null })
  ),

  on(PlansActions.loadPlansFailure, (state, { error }): PlansState => ({
    ...state,
    isLoading: false,
    error,
  })),

  on(PlansActions.loadPlanSuccess, (state, { plan }): PlansState =>
    plansAdapter.upsertOne(plan, { ...state, isLoading: false })
  ),

  on(PlansActions.createPlanSuccess, (state, { plan }): PlansState =>
    plansAdapter.addOne(plan, state)
  ),

  on(
    PlansActions.updatePlanSuccess,
    PlansActions.archivePlanSuccess,
    PlansActions.syncPlanToPayPalSuccess,
    (state, { plan }): PlansState => plansAdapter.upsertOne(plan, state)
  ),

  on(
    PlansActions.loadPlanFailure,
    PlansActions.createPlanFailure,
    PlansActions.updatePlanFailure,
    PlansActions.archivePlanFailure,
    PlansActions.syncPlanToPayPalFailure,
    (state, { error }): PlansState => ({
      ...state,
      isLoading: false,
      error,
    })
  ),

  on(PlansActions.selectPlan, (state, { planId }): PlansState => ({
    ...state,
    selectedPlanId: planId,
  })),

  on(PlansActions.clearError, (state): PlansState => ({
    ...state,
    error: null,
  })),
);
