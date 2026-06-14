import { inject, Injectable } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { of } from 'rxjs';
import { catchError, exhaustMap, map } from 'rxjs/operators';
import { TipsService } from '../../core/services/tips.service';
import { TipsActions } from './tips.actions';

@Injectable()
export class TipsEffects {
  private readonly actions$ = inject(Actions);
  private readonly tipsService = inject(TipsService);

  loadTips$ = createEffect(() =>
    this.actions$.pipe(
      ofType(TipsActions.loadTips),
      exhaustMap(({ page, pageSize, category }) =>
        this.tipsService.getTips(page, pageSize, category).pipe(
          map((response) => TipsActions.loadTipsSuccess({ tips: response.items, totalCount: response.totalCount })),
          catchError((error) =>
            of(TipsActions.loadTipsFailure({ error: error.message ?? 'Failed to load tips' }))
          )
        )
      )
    )
  );

  loadTip$ = createEffect(() =>
    this.actions$.pipe(
      ofType(TipsActions.loadTip),
      exhaustMap(({ tipId }) =>
        this.tipsService.getTip(tipId).pipe(
          map((tip) => TipsActions.loadTipSuccess({ tip })),
          catchError((error) =>
            of(TipsActions.loadTipFailure({ error: error.message ?? 'Failed to load tip' }))
          )
        )
      )
    )
  );

  createTip$ = createEffect(() =>
    this.actions$.pipe(
      ofType(TipsActions.createTip),
      exhaustMap(({ tip }) =>
        this.tipsService.createTip(tip).pipe(
          map((created) => TipsActions.createTipSuccess({ tip: created })),
          catchError((error) =>
            of(TipsActions.createTipFailure({ error: error.message ?? 'Failed to create tip' }))
          )
        )
      )
    )
  );

  publishTip$ = createEffect(() =>
    this.actions$.pipe(
      ofType(TipsActions.publishTip),
      exhaustMap(({ tipId }) =>
        this.tipsService.publishTip(tipId).pipe(
          map((tip) => TipsActions.publishTipSuccess({ tip })),
          catchError((error) =>
            of(TipsActions.publishTipFailure({ error: error.message ?? 'Failed to publish tip' }))
          )
        )
      )
    )
  );

  recordResult$ = createEffect(() =>
    this.actions$.pipe(
      ofType(TipsActions.recordResult),
      exhaustMap(({ tipId, result }) =>
        this.tipsService.recordResult(tipId, result).pipe(
          map((tip) => TipsActions.recordResultSuccess({ tip })),
          catchError((error) =>
            of(TipsActions.recordResultFailure({ error: error.message ?? 'Failed to record result' }))
          )
        )
      )
    )
  );
}
