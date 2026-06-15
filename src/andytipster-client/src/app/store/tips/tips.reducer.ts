import { createEntityAdapter, EntityAdapter } from '@ngrx/entity';
import { createReducer, on } from '@ngrx/store';
import { TipsActions } from './tips.actions';
import { Tip, TipsState } from './tips.state';

export const tipsAdapter: EntityAdapter<Tip> = createEntityAdapter<Tip>({
  selectId: (tip) => tip.id,
  sortComparer: (a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime(),
});

export const initialTipsState: TipsState = tipsAdapter.getInitialState({
  isLoading: false,
  error: null,
  selectedTipId: null,
  totalCount: 0,
  currentPage: 1,
  pageSize: 25,
});

export const tipsReducer = createReducer(
  initialTipsState,

  on(TipsActions.loadTips, (state, { page, pageSize }): TipsState => ({
    ...state,
    isLoading: true,
    error: null,
    currentPage: page ?? state.currentPage,
    pageSize: pageSize ?? state.pageSize,
  })),

  on(TipsActions.loadTipsSuccess, (state, { tips, totalCount }): TipsState =>
    tipsAdapter.setAll(tips, { ...state, isLoading: false, error: null, totalCount })
  ),

  on(TipsActions.loadTipsFailure, (state, { error }): TipsState => ({
    ...state,
    isLoading: false,
    error,
  })),

  on(TipsActions.loadTip, (state): TipsState => ({
    ...state,
    isLoading: true,
    error: null,
  })),

  on(TipsActions.loadTipSuccess, (state, { tip }): TipsState =>
    tipsAdapter.upsertOne(tip, { ...state, isLoading: false })
  ),

  on(TipsActions.loadTipFailure, (state, { error }): TipsState => ({
    ...state,
    isLoading: false,
    error,
  })),

  on(TipsActions.createTipSuccess, (state, { tip }): TipsState =>
    tipsAdapter.addOne(tip, { ...state, totalCount: state.totalCount + 1 })
  ),

  on(TipsActions.updateTipSuccess, TipsActions.publishTipSuccess, TipsActions.recordResultSuccess, (state, { tip }): TipsState =>
    tipsAdapter.upsertOne(tip, state)
  ),

  on(
    TipsActions.createTipFailure,
    TipsActions.updateTipFailure,
    TipsActions.publishTipFailure,
    TipsActions.recordResultFailure,
    (state, { error }): TipsState => ({
      ...state,
      error,
    })
  ),

  on(TipsActions.selectTip, (state, { tipId }): TipsState => ({
    ...state,
    selectedTipId: tipId,
  })),

  on(TipsActions.clearError, (state): TipsState => ({
    ...state,
    error: null,
  })),
);
