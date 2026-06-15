import { Injectable } from '@angular/core';

interface CacheEntry<T> {
  data: T;
  expiresAt: number;
}

/**
 * Simple in-memory and localStorage caching service with TTL support.
 * Used for plans, published tips, and CMS pages to reduce API calls.
 */
@Injectable({ providedIn: 'root' })
export class CacheService {
  private memoryCache = new Map<string, CacheEntry<unknown>>();

  /**
   * Get a value from cache (memory first, then localStorage).
   * Returns null if expired or not found.
   */
  get<T>(key: string): T | null {
    // Check memory cache first
    const memEntry = this.memoryCache.get(key) as CacheEntry<T> | undefined;
    if (memEntry) {
      if (Date.now() < memEntry.expiresAt) {
        return memEntry.data;
      }
      this.memoryCache.delete(key);
    }

    // Check localStorage
    try {
      const raw = localStorage.getItem(`cache:${key}`);
      if (raw) {
        const entry: CacheEntry<T> = JSON.parse(raw);
        if (Date.now() < entry.expiresAt) {
          // Promote back to memory cache
          this.memoryCache.set(key, entry);
          return entry.data;
        }
        localStorage.removeItem(`cache:${key}`);
      }
    } catch {
      // localStorage unavailable or parse error
    }

    return null;
  }

  /**
   * Store a value in both memory and localStorage cache with a TTL.
   * @param key Cache key
   * @param data Data to cache
   * @param ttlMs Time to live in milliseconds
   */
  set<T>(key: string, data: T, ttlMs: number): void {
    const entry: CacheEntry<T> = {
      data,
      expiresAt: Date.now() + ttlMs,
    };

    this.memoryCache.set(key, entry);

    try {
      localStorage.setItem(`cache:${key}`, JSON.stringify(entry));
    } catch {
      // localStorage full or unavailable — memory cache still works
    }
  }

  /**
   * Invalidate a specific cache entry.
   */
  invalidate(key: string): void {
    this.memoryCache.delete(key);
    try {
      localStorage.removeItem(`cache:${key}`);
    } catch {
      // ignore
    }
  }

  /**
   * Invalidate all entries matching a prefix.
   */
  invalidateByPrefix(prefix: string): void {
    // Memory cache
    for (const key of this.memoryCache.keys()) {
      if (key.startsWith(prefix)) {
        this.memoryCache.delete(key);
      }
    }

    // localStorage
    try {
      const keysToRemove: string[] = [];
      for (let i = 0; i < localStorage.length; i++) {
        const storageKey = localStorage.key(i);
        if (storageKey?.startsWith(`cache:${prefix}`)) {
          keysToRemove.push(storageKey);
        }
      }
      keysToRemove.forEach((k) => localStorage.removeItem(k));
    } catch {
      // ignore
    }
  }

  /**
   * Clear all cached data.
   */
  clear(): void {
    this.memoryCache.clear();
    try {
      const keysToRemove: string[] = [];
      for (let i = 0; i < localStorage.length; i++) {
        const storageKey = localStorage.key(i);
        if (storageKey?.startsWith('cache:')) {
          keysToRemove.push(storageKey);
        }
      }
      keysToRemove.forEach((k) => localStorage.removeItem(k));
    } catch {
      // ignore
    }
  }
}
