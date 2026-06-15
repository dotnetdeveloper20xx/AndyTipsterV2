import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { SkeletonLoaderComponent } from './skeleton-loader.component';

describe('SkeletonLoaderComponent', () => {
  let component: SkeletonLoaderComponent;
  let fixture: ComponentFixture<SkeletonLoaderComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SkeletonLoaderComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(SkeletonLoaderComponent);
    component = fixture.componentInstance;
  });

  afterEach(() => {
    fixture.destroy();
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('should display skeleton rows based on count input', () => {
    fixture.componentRef.setInput('count', 5);
    fixture.detectChanges();
    const shimmerElements = fixture.nativeElement.querySelectorAll('.skeleton-shimmer');
    expect(shimmerElements.length).toBe(5);
  });

  it('should default to 3 rows', () => {
    fixture.detectChanges();
    const shimmerElements = fixture.nativeElement.querySelectorAll('.skeleton-shimmer');
    expect(shimmerElements.length).toBe(3);
  });

  it('should display avatar shape with circle and text elements', () => {
    fixture.componentRef.setInput('shape', 'avatar');
    fixture.componentRef.setInput('count', 2);
    fixture.detectChanges();
    const circles = fixture.nativeElement.querySelectorAll('.rounded-full');
    expect(circles.length).toBe(2);
  });

  it('should display card shape with larger elements', () => {
    fixture.componentRef.setInput('shape', 'card');
    fixture.componentRef.setInput('count', 2);
    fixture.detectChanges();
    const cards = fixture.nativeElement.querySelectorAll('.h-32');
    expect(cards.length).toBe(2);
  });

  it('should transition to error state after timeout', fakeAsync(() => {
    fixture.componentRef.setInput('timeout', 5000);
    fixture.detectChanges();

    expect(component.hasTimedOut()).toBe(false);

    tick(5000);
    fixture.detectChanges();

    expect(component.hasTimedOut()).toBe(true);
    const retryButton = fixture.nativeElement.querySelector('button');
    expect(retryButton).toBeTruthy();
    expect(retryButton.textContent.trim()).toContain('Retry');
  }));

  it('should emit retry event and reset state on retry click', fakeAsync(() => {
    fixture.componentRef.setInput('timeout', 1000);
    fixture.detectChanges();
    tick(1000);
    fixture.detectChanges();

    expect(component.hasTimedOut()).toBe(true);

    const retrySpy = spyOn(component.retry, 'emit');
    component.onRetry();
    fixture.detectChanges();

    expect(retrySpy).toHaveBeenCalled();
    expect(component.hasTimedOut()).toBe(false);

    // Verify new timeout starts
    tick(1000);
    expect(component.hasTimedOut()).toBe(true);
  }));

  it('should have accessible loading indicator', () => {
    fixture.detectChanges();
    const statusElement = fixture.nativeElement.querySelector('[role="status"]');
    expect(statusElement).toBeTruthy();
    const srOnly = fixture.nativeElement.querySelector('.sr-only');
    expect(srOnly.textContent).toContain('Loading');
  });
});
