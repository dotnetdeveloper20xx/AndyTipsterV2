import { Injectable, signal } from '@angular/core';

export interface ToastItem {
  id: string;
  message: string;
  type: 'success' | 'error' | 'info' | 'warning';
  createdAt: number;
}

@Injectable({ providedIn: 'root' })
export class ToastService {
  private readonly AUTO_DISMISS_MS = 5000;

  readonly toasts = signal<ToastItem[]>([]);

  success(message: string): void {
    this.addToast(message, 'success');
  }

  error(message: string): void {
    this.addToast(message, 'error');
  }

  info(message: string): void {
    this.addToast(message, 'info');
  }

  warning(message: string): void {
    this.addToast(message, 'warning');
  }

  dismiss(id: string): void {
    this.toasts.update((items) => items.filter((t) => t.id !== id));
  }

  private addToast(message: string, type: ToastItem['type']): void {
    const id = crypto.randomUUID();
    const toast: ToastItem = { id, message, type, createdAt: Date.now() };

    this.toasts.update((items) => [...items, toast]);

    setTimeout(() => this.dismiss(id), this.AUTO_DISMISS_MS);
  }
}
