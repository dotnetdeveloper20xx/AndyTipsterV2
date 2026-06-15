import { TipDto } from '../../core/services/tips.service';
import { Tip } from './tips.state';

export function mapTipDtoToTip(dto: TipDto): Tip {
  return {
    id: dto.id,
    eventDate: dto.eventDate,
    raceName: dto.raceName,
    selection: dto.selection,
    odds: dto.odds,
    stake: dto.stake,
    categoryId: dto.categoryId,
    categoryName: dto.categoryName,
    commentary: dto.commentary ?? null,
    status: dto.status as Tip['status'],
    result: (dto.result as Tip['result']) ?? null,
    profitLoss: dto.profitLoss ?? null,
    publishedAt: dto.publishedAt ?? null,
    scheduledPublishAt: dto.scheduledPublishAt ?? null,
    createdAt: dto.createdAt,
    createdByUserId: dto.createdByUserId,
  };
}
