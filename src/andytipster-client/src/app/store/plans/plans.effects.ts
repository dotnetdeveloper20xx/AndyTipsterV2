import { inject, Injectable } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { of } from 'rxjs';
import { catchError, exhaustMap, map } from 'rxjs/operators';
import { PlansService } from '../../core/services/plans.service';
import { PlansActions } from './plans.actions';

@Injectable()
export class PlansEffects {
  private readonly actions$ = inject(Actions);
  private readonly plansService = inject(PlansService);

  loadPlans$ = createEffect(() =>
    this.actions$.pipe(
      ofType(PlansActions.loadPlans),
      exhaustMap(() =>
        this.plansService.getPlans().pipe(
          map((plans) => PlansActions.loadPlansSuccess({ plans })),
          catchError((error) =>
            of(PlansActions.loadPlansFailure({ error: error.message ?? 'Failed to load plans' }))
          )
        )
      )
    )
  );

  loadPlan$ = createEffect(() =>
    this.actions$.pipe(
      ofType(PlansActions.loadPlan),
      exhaustMap(({ planId }) =>
        this.plansService.getPlan(planId).pipe(
          map((plan) => PlansActions.loadPlanSuccess({ plan })),
          catchError((error) =>
            of(PlansActions.loadPlanFailure({ error: error.message ?? 'Failed to load plan' }))
          )
        )
      )
    )
  );

  createPlan$ = createEffect(() =>
    this.actions$.pipe(
      ofType(PlansActions.createPlan),
      exhaustMap(({ plan }) =>
        this.plansService.createPlan(plan).pipe(
          map((created) => PlansActions.createPlanSuccess({ plan: created })),
          catchError((error) =>
            of(PlansActions.createPlanFailure({ error: error.message ?? 'Failed to create plan' }))
          )
        )
      )
    )
  );

  archivePlan$ = createEffect(() =>
    this.actions$.pipe(
      ofType(PlansActions.archivePlan),
      exhaustMap(({ planId }) =>
        this.plansService.archivePlan(planId).pipe(
          map((plan) => PlansActions.archivePlanSuccess({ plan })),
          catchError((error) =>
            of(PlansActions.archivePlanFailure({ error: error.message ?? 'Failed to archive plan' }))
          )
        )
      )
    )
  );

  syncToPayPal$ = createEffect(() =>
    this.actions$.pipe(
      ofType(PlansActions.syncPlanToPayPal),
      exhaustMap(({ planId }) =>
        this.plansService.syncToPayPal(planId).pipe(
          map((plan) => PlansActions.syncPlanToPayPalSuccess({ plan })),
          catchError((error) =>
            of(PlansActions.syncPlanToPayPalFailure({ error: error.message ?? 'Failed to sync plan to PayPal' }))
          )
        )
      )
    )
  );
}
