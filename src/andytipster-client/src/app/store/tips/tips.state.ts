import { EntityState } from '@ngrx/entity';

export type TipStatus = 'Draft' | 'Published' | 'Archived';
export type TipResult = 'Pending' | 'Won' | 'Lost' | 'Void' | 'Push';

export interface Tip {
  id: string;
  eventDate: string;
  raceName: string;
  selection: string;
  odds: number;
  stake: number;
  category: string;
  commentary: string | null;
  status: TipStatus;
  result: TipResult;
  profitLoss: number | null;
  publishedAt: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface TipsState extends EntityState<Tip> {
  isLoading: boolean;
  error: string | null;
  selectedTipId: string | null;
  totalCount: number;
  currentPage: number;
  pageSize: number;
}
