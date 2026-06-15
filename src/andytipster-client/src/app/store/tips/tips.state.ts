import { EntityState } from '@ngrx/entity';

export type TipStatus = 'Draft' | 'Published' | 'Archived';
export type TipResult = 'Won' | 'Lost' | 'Void' | 'Push';

export interface Tip {
  id: string;
  eventDate: string;
  raceName: string;
  selection: string;
  odds: number;
  stake: number;
  categoryId: string;
  categoryName: string;
  commentary: string | null;
  status: TipStatus;
  result: TipResult | null;
  profitLoss: number | null;
  publishedAt: string | null;
  scheduledPublishAt: string | null;
  createdAt: string;
  createdByUserId: string;
}

export interface TipsState extends EntityState<Tip> {
  isLoading: boolean;
  error: string | null;
  selectedTipId: string | null;
  totalCount: number;
  currentPage: number;
  pageSize: number;
}
