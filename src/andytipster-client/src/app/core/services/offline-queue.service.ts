import { Injectable, OnDestroy } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';

export interface QueuedAction {
  id: string;
  method: 'POST' | 'PUT' | 'PATCH' | 'DELETE';
  url: string;
  body?: unknown;
  timestamp: number;
}

const QUEUE_STORAGE_KEY = 'andytipster-offline-queue';

@Injectable({ providedIn: 'root' })
export class OfflineQueueService implements OnDestroy {
  private readonly http = inject(HttpClient);
  private handleOnline = () => this.flush();

  constructor() {
    window.addEventListener('online', this.handleOnline);
  }

  ngOnDestroy(): void {
    window.removeEventListener('online', this.handleOnline);
  }

  enqueue(action: Omit<QueuedAction, 'id' | 'timestamp'>): void {
    const queue = this.getQueue();
    queue.push({
      ...action,
      id: crypto.randomUUID(),
      timestamp: Date.now(),
    });
    this.saveQueue(queue);
  }

  getQueue(): QueuedAction[] {
    try {
      const raw = localStorage.getItem(QUEUE_STORAGE_KEY);
      return raw ? JSON.parse(raw) : [];
    } catch {
      return [];
    }
  }

  async flush(): Promise<void> {
    const queue = this.getQueue();
    if (queue.length === 0) return;

    const remaining: QueuedAction[] = [];

    for (const action of queue) {
      try {
        const request$ = this.http.request(action.method, action.url, {
          body: action.body,
        });
        await firstValueFrom(request$);
      } catch {
        remaining.push(action);
      }
    }

    this.saveQueue(remaining);
  }

  clear(): void {
    try {
      localStorage.removeItem(QUEUE_STORAGE_KEY);
    } catch {
      // localStorage not available
    }
  }

  get pendingCount(): number {
    return this.getQueue().length;
  }

  private saveQueue(queue: QueuedAction[]): void {
    try {
      localStorage.setItem(QUEUE_STORAGE_KEY, JSON.stringify(queue));
    } catch {
      // localStorage full or unavailable
    }
  }
}
