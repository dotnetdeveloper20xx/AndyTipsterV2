import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ThemeToggleComponent } from './theme-toggle.component';
import { ThemeService } from '../../../core/services/theme.service';

describe('ThemeToggleComponent', () => {
  let component: ThemeToggleComponent;
  let fixture: ComponentFixture<ThemeToggleComponent>;
  let themeService: ThemeService;

  beforeEach(async () => {
    localStorage.clear();
    spyOn(window, 'matchMedia').and.returnValue({
      matches: false,
      addEventListener: () => {},
    } as unknown as MediaQueryList);

    await TestBed.configureTestingModule({
      imports: [ThemeToggleComponent],
    }).compileComponents();

    themeService = TestBed.inject(ThemeService);
    fixture = TestBed.createComponent(ThemeToggleComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should have an accessible label', () => {
    const label = fixture.nativeElement.querySelector('label');
    expect(label.getAttribute('aria-label')).toBe('Toggle dark mode');
  });

  it('should reflect current theme state in checkbox', () => {
    const checkbox = fixture.nativeElement.querySelector('input[type="checkbox"]');
    expect(checkbox.checked).toBe(false); // Light mode = unchecked
  });

  it('should toggle theme when checkbox changes', () => {
    const toggleSpy = spyOn(themeService, 'toggleTheme');
    const checkbox = fixture.nativeElement.querySelector('input[type="checkbox"]');
    checkbox.dispatchEvent(new Event('change'));
    expect(toggleSpy).toHaveBeenCalled();
  });

  it('should have sun and moon SVG icons', () => {
    const svgs = fixture.nativeElement.querySelectorAll('svg');
    expect(svgs.length).toBe(2);
    // Both should be aria-hidden
    expect(svgs[0].getAttribute('aria-hidden')).toBe('true');
    expect(svgs[1].getAttribute('aria-hidden')).toBe('true');
  });
});
