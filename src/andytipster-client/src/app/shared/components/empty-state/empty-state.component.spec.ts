import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { EmptyStateComponent } from './empty-state.component';

describe('EmptyStateComponent', () => {
  let component: EmptyStateComponent;
  let fixture: ComponentFixture<EmptyStateComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EmptyStateComponent],
      providers: [provideRouter([])],
    }).compileComponents();

    fixture = TestBed.createComponent(EmptyStateComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('should display default title', () => {
    fixture.detectChanges();
    const title = fixture.nativeElement.querySelector('h3');
    expect(title.textContent).toContain('Nothing here yet');
  });

  it('should display custom title and message', () => {
    fixture.componentRef.setInput('title', 'No Tips Found');
    fixture.componentRef.setInput('message', 'There are no tips available for today.');
    fixture.detectChanges();

    const title = fixture.nativeElement.querySelector('h3');
    const message = fixture.nativeElement.querySelector('p');
    expect(title.textContent).toContain('No Tips Found');
    expect(message.textContent).toContain('There are no tips available for today.');
  });

  it('should display CTA button when ctaText and ctaRoute are provided', () => {
    fixture.componentRef.setInput('ctaText', 'Create Tip');
    fixture.componentRef.setInput('ctaRoute', '/admin/tips/new');
    fixture.detectChanges();

    const button = fixture.nativeElement.querySelector('a.btn');
    expect(button).toBeTruthy();
    expect(button.textContent.trim()).toBe('Create Tip');
  });

  it('should not display CTA button when ctaText is empty', () => {
    fixture.componentRef.setInput('ctaText', '');
    fixture.componentRef.setInput('ctaRoute', '/admin/tips/new');
    fixture.detectChanges();

    const button = fixture.nativeElement.querySelector('a.btn');
    expect(button).toBeNull();
  });

  it('should include an SVG illustration', () => {
    fixture.detectChanges();
    const svg = fixture.nativeElement.querySelector('svg');
    expect(svg).toBeTruthy();
    expect(svg.getAttribute('aria-hidden')).toBe('true');
  });
});
