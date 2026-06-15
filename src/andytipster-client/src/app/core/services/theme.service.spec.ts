import { TestBed } from '@angular/core/testing';
import { PLATFORM_ID } from '@angular/core';
import { ThemeService, Theme } from './theme.service';

describe('ThemeService', () => {
  let service: ThemeService;

  beforeEach(() => {
    localStorage.clear();
    document.documentElement.removeAttribute('data-theme');
  });

  function createService(platformId: string = 'browser') {
    TestBed.configureTestingModule({
      providers: [ThemeService, { provide: PLATFORM_ID, useValue: platformId }],
    });
    service = TestBed.inject(ThemeService);
  }

  describe('initialization', () => {
    it('should default to light theme when no preference stored and system is light', () => {
      // Mock matchMedia to return light preference
      spyOn(window, 'matchMedia').and.returnValue({
        matches: false,
        addEventListener: () => {},
      } as unknown as MediaQueryList);

      createService();

      expect(service.theme()).toBe('andytipster-light');
      expect(service.isDarkMode()).toBe(false);
    });

    it('should use system dark preference when no localStorage value', () => {
      spyOn(window, 'matchMedia').and.returnValue({
        matches: true,
        addEventListener: () => {},
      } as unknown as MediaQueryList);

      createService();

      expect(service.theme()).toBe('andytipster-dark');
      expect(service.isDarkMode()).toBe(true);
    });

    it('should read stored theme from localStorage', () => {
      localStorage.setItem('andytipster-theme', 'andytipster-dark');
      spyOn(window, 'matchMedia').and.returnValue({
        matches: false,
        addEventListener: () => {},
      } as unknown as MediaQueryList);

      createService();

      expect(service.theme()).toBe('andytipster-dark');
    });

    it('should set data-theme attribute on document element', () => {
      spyOn(window, 'matchMedia').and.returnValue({
        matches: false,
        addEventListener: () => {},
      } as unknown as MediaQueryList);

      createService();

      expect(document.documentElement.getAttribute('data-theme')).toBe('andytipster-light');
    });
  });

  describe('toggleTheme', () => {
    it('should switch from light to dark', () => {
      spyOn(window, 'matchMedia').and.returnValue({
        matches: false,
        addEventListener: () => {},
      } as unknown as MediaQueryList);

      createService();
      service.toggleTheme();

      expect(service.theme()).toBe('andytipster-dark');
      expect(service.isDarkMode()).toBe(true);
      expect(document.documentElement.getAttribute('data-theme')).toBe('andytipster-dark');
    });

    it('should switch from dark to light', () => {
      localStorage.setItem('andytipster-theme', 'andytipster-dark');
      spyOn(window, 'matchMedia').and.returnValue({
        matches: true,
        addEventListener: () => {},
      } as unknown as MediaQueryList);

      createService();
      service.toggleTheme();

      expect(service.theme()).toBe('andytipster-light');
      expect(service.isDarkMode()).toBe(false);
    });

    it('should persist theme choice to localStorage', () => {
      spyOn(window, 'matchMedia').and.returnValue({
        matches: false,
        addEventListener: () => {},
      } as unknown as MediaQueryList);

      createService();
      service.toggleTheme();

      expect(localStorage.getItem('andytipster-theme')).toBe('andytipster-dark');
    });
  });

  describe('setTheme', () => {
    it('should set a specific theme', () => {
      spyOn(window, 'matchMedia').and.returnValue({
        matches: false,
        addEventListener: () => {},
      } as unknown as MediaQueryList);

      createService();
      service.setTheme('andytipster-dark');

      expect(service.theme()).toBe('andytipster-dark');
      expect(localStorage.getItem('andytipster-theme')).toBe('andytipster-dark');
      expect(document.documentElement.getAttribute('data-theme')).toBe('andytipster-dark');
    });
  });

  describe('localStorage error handling', () => {
    it('should continue without error when localStorage throws', () => {
      spyOn(localStorage, 'getItem').and.throwError('Storage access denied');
      spyOn(window, 'matchMedia').and.returnValue({
        matches: false,
        addEventListener: () => {},
      } as unknown as MediaQueryList);

      expect(() => createService()).not.toThrow();
      expect(service.theme()).toBe('andytipster-light');
    });

    it('should continue without error when localStorage.setItem throws', () => {
      spyOn(window, 'matchMedia').and.returnValue({
        matches: false,
        addEventListener: () => {},
      } as unknown as MediaQueryList);

      createService();

      spyOn(localStorage, 'setItem').and.throwError('Storage full');
      expect(() => service.toggleTheme()).not.toThrow();
      expect(service.theme()).toBe('andytipster-dark');
    });
  });

  describe('system preference listener', () => {
    it('should respond to system preference changes when no stored preference', () => {
      let callback: ((event: MediaQueryListEvent) => void) | undefined;
      spyOn(window, 'matchMedia').and.returnValue({
        matches: false,
        addEventListener: (_event: string, cb: (event: MediaQueryListEvent) => void) => {
          callback = cb;
        },
      } as unknown as MediaQueryList);

      createService();
      expect(service.theme()).toBe('andytipster-light');

      // Simulate system dark mode change
      callback!({ matches: true } as MediaQueryListEvent);
      expect(service.theme()).toBe('andytipster-dark');
    });

    it('should NOT respond to system changes when user has stored preference', () => {
      localStorage.setItem('andytipster-theme', 'andytipster-light');

      let callback: ((event: MediaQueryListEvent) => void) | undefined;
      spyOn(window, 'matchMedia').and.returnValue({
        matches: false,
        addEventListener: (_event: string, cb: (event: MediaQueryListEvent) => void) => {
          callback = cb;
        },
      } as unknown as MediaQueryList);

      createService();

      // Simulate system dark mode change
      callback!({ matches: true } as MediaQueryListEvent);
      // Should NOT change because user has stored preference
      expect(service.theme()).toBe('andytipster-light');
    });
  });
});
