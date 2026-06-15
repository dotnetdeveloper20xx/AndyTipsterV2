import { createFeatureSelector, createSelector } from '@ngrx/store';
import { tipsAdapter } from './tips.reducer';
import { TipsState } from './tips.state';

export const selectTipsState = createFeatureSelector<TipsState>('tips');

const { selectAll, selectEntities } = tipsAdapter.getSelectors();

export const selectAllTips = createSelector(selectTipsState, selectAll);

export const selectTipsEntities = createSelector(selectTipsState, selectEntities);

export const selectTipsIsLoading = createSelector(
  selectTipsState,
  (state) => state.isLoading
);

export const selectTipsError = createSelector(
  selectTipsState,
  (state) => state.error
);

export const selectSelectedTipId = createSelector(
  selectTipsState,
  (state) => state.selectedTipId
);

export const selectSelectedTip = createSelector(
  selectTipsEntities,
  selectSelectedTipId,
  (entities, selectedId) => (selectedId ? entities[selectedId] ?? null : null)
);

export const selectTipsTotalCount = createSelector(
  selectTipsState,
  (state) => state.totalCount
);

export const selectTipsCurrentPage = createSelector(
  selectTipsState,
  (state) => state.currentPage
);

export const selectTipsPageSize = createSelector(
  selectTipsState,
  (state) => state.pageSize
);
