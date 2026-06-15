import { inject, Injectable } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { of } from 'rxjs';
import { catchError, exhaustMap, map, switchMap } from 'rxjs/operators';
import { TipsService, CreateTipDto, UpdateTipDto } from '../../core/services/tips.service';
import { TipsActions } from './tips.actions';
import { mapTipDtoToTip } from './tips.mappers';

@Injectable()
export class TipsEffects {
  private readonly actions$ = inject(Actions);
  private readonly tipsService = inject(TipsService);

  loadTips$ = createEffect(() =>
    this.actions$.pipe(
      ofType(TipsActions.loadTips),
      switchMap(({ page, pageSize, category }) =>
        this.tipsService.getTips({ page, pageSize, categoryId: category }).pipe(
          map((response) => TipsActions.loadTipsSuccess({ tips: response.items.map(mapTipDtoToTip), totalCount: response.totalCount })),
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
      switchMap(({ tipId }) =>
        this.tipsService.getTip(tipId).pipe(
          map((tip) => TipsActions.loadTipSuccess({ tip: mapTipDtoToTip(tip) })),
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
        this.tipsService.createTip(tip as unknown as CreateTipDto).pipe(
          map((created) => TipsActions.createTipSuccess({ tip: mapTipDtoToTip(created) })),
          catchError((error) =>
            of(TipsActions.createTipFailure({ error: error.message ?? 'Failed to create tip' }))
          )
        )
      )
    )
  );

  updateTip$ = createEffect(() =>
    this.actions$.pipe(
      ofType(TipsActions.updateTip),
      exhaustMap(({ tipId, changes }) =>
        this.tipsService.updateTip(tipId, changes as unknown as UpdateTipDto).pipe(
          map((updated) => TipsActions.updateTipSuccess({ tip: mapTipDtoToTip(updated) })),
          catchError((error) =>
            of(TipsActions.updateTipFailure({ error: error.message ?? 'Failed to update tip' }))
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
          map((tip) => TipsActions.publishTipSuccess({ tip: mapTipDtoToTip(tip) })),
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
          map((tip) => TipsActions.recordResultSuccess({ tip: mapTipDtoToTip(tip) })),
          catchError((error) =>
            of(TipsActions.recordResultFailure({ error: error.message ?? 'Failed to record result' }))
          )
        )
      )
    )
  );
}
