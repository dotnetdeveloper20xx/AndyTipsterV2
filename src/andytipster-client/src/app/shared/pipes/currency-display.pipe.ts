import { Pipe, PipeTransform } from '@angular/core';

@Pipe({ name: 'currencyDisplay', standalone: true })
export class CurrencyDisplayPipe implements PipeTransform {
  transform(amount: number | null | undefined, currencyCode: string = 'GBP'): string {
    if (amount === null || amount === undefined) return '—';

    const locale = currencyCode === 'GBP' ? 'en-GB' : currencyCode === 'EUR' ? 'en-IE' : 'en-US';

    return new Intl.NumberFormat(locale, {
      style: 'currency',
      currency: currencyCode,
      minimumFractionDigits: 2,
      maximumFractionDigits: 2,
    }).format(amount);
  }
}
