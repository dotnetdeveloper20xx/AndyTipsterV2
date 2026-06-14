import { createActionGroup, emptyProps, props } from '@ngrx/store';
import { Tip } from './tips.state';

export const TipsActions = createActionGroup({
  source: 'Tips',
  events: {
    'Load Tips': props<{ page?: number; pageSize?: number; category?: string }>(),
    'Load Tips Success': props<{ tips: Tip[]; totalCount: number }>(),
    'Load Tips Failure': props<{ error: string }>(),

    'Load Tip': props<{ tipId: string }>(),
    'Load Tip Success': props<{ tip: Tip }>(),
    'Load Tip Failure': props<{ error: string }>(),

    'Create Tip': props<{ tip: Partial<Tip> }>(),
    'Create Tip Success': props<{ tip: Tip }>(),
    'Create Tip Failure': props<{ error: string }>(),

    'Update Tip': props<{ tipId: string; changes: Partial<Tip> }>(),
    'Update Tip Success': props<{ tip: Tip }>(),
    'Update Tip Failure': props<{ error: string }>(),

    'Publish Tip': props<{ tipId: string }>(),
    'Publish Tip Success': props<{ tip: Tip }>(),
    'Publish Tip Failure': props<{ error: string }>(),

    'Record Result': props<{ tipId: string; result: 'Won' | 'Lost' | 'Void' | 'Push' }>(),
    'Record Result Success': props<{ tip: Tip }>(),
    'Record Result Failure': props<{ error: string }>(),

    'Select Tip': props<{ tipId: string | null }>(),
    'Clear Error': emptyProps(),
  },
});
