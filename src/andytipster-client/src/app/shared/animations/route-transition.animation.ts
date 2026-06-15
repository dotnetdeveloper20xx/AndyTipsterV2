import { trigger, transition, style, animate, query, group } from '@angular/animations';

/**
 * Route transition animation (300ms).
 * Fades out the old view and fades in the new view with a subtle slide.
 */
export const routeTransitionAnimation = trigger('routeAnimation', [
  transition('* <=> *', [
    query(
      ':enter, :leave',
      [
        style({
          position: 'absolute',
          top: 0,
          left: 0,
          width: '100%',
        }),
      ],
      { optional: true }
    ),
    query(':enter', [style({ opacity: 0, transform: 'translateY(10px)' })], { optional: true }),
    group([
      query(':leave', [animate('150ms ease-out', style({ opacity: 0, transform: 'translateY(-10px)' }))], {
        optional: true,
      }),
      query(':enter', [animate('300ms 150ms ease-out', style({ opacity: 1, transform: 'translateY(0)' }))], {
        optional: true,
      }),
    ]),
  ]),
]);
