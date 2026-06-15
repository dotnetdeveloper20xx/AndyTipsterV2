import { Component } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { RequiredAltDirective } from './required-alt.directive';

@Component({
  standalone: true,
  imports: [RequiredAltDirective],
  template: `
    <img src="valid.png" alt="A valid description" />
    <img src="missing-alt.png" />
    <img src="empty-alt.png" alt="" />
  `,
})
class TestHostComponent {}

describe('RequiredAltDirective', () => {
  let fixture: ComponentFixture<TestHostComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TestHostComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(TestHostComponent);
  });

  it('should warn for images without alt attribute', () => {
    const warnSpy = spyOn(console, 'warn');
    fixture.detectChanges();

    expect(warnSpy).toHaveBeenCalledTimes(2);
    expect(warnSpy.calls.argsFor(0)[0]).toContain('missing-alt.png');
    expect(warnSpy.calls.argsFor(1)[0]).toContain('empty-alt.png');
  });

  it('should not warn for images with valid alt text', () => {
    const warnSpy = spyOn(console, 'warn');
    fixture.detectChanges();

    // Should not have warned about valid.png
    const allCalls = warnSpy.calls.allArgs().map((args) => args[0]);
    expect(allCalls.some((msg: string) => msg.includes('valid.png'))).toBeFalse();
  });
});
