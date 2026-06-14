import { createFeatureSelector, createSelector } from '@ngrx/store';
import { plansAdapter } from './plans.reducer';
import { PlansState } from './plans.state';

export const selectPlansState = createFeatureSelector<PlansState>('plans');

const { selectAll, selectEntities } = plansAdapter.getSelectors();

export const selectAllPlans = createSelector(selectPlansState, selectAll);

export const selectPlansEntities = createSelector(selectPlansState, selectEntities);

export const selectActivePlans = createSelector(
  selectAllPlans,
  (plans) => plans.filter((p) => !p.isArchived)
);

export const selectPlansIsLoading = createSelector(
  selectPlansState,
  (state) => state.isLoading
);

export const selectPlansError = createSelector(
  selectPlansState,
  (state) => state.error
);

export const selectSelectedPlanId = createSelector(
  selectPlansState,
  (state) => state.selectedPlanId
);

export const selectSelectedPlan = createSelector(
  selectPlansEntities,
  selectSelectedPlanId,
  (entities, selectedId) => (selectedId ? entities[selectedId] ?? null : null)
);
