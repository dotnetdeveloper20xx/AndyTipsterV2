import axe, { AxeResults, RunOptions, Spec } from 'axe-core';
import { ComponentFixture } from '@angular/core/testing';

/**
 * Configuration options for accessibility testing.
 */
export interface A11yTestOptions {
  /** axe-core run options (rules to enable/disable, etc.) */
  runOptions?: RunOptions;
  /** axe-core spec configuration */
  spec?: Spec;
  /** Rules to disable for this specific test */
  disableRules?: string[];
}

/**
 * Runs axe-core accessibility checks on a rendered Angular component fixture.
 *
 * @param fixture - The Angular ComponentFixture to test
 * @param options - Optional configuration for the accessibility scan
 * @returns Promise<AxeResults> - The full axe-core results
 *
 * @example
 * ```typescript
 * it('should have no accessibility violations', async () => {
 *   const results = await testAccessibility(fixture);
 *   expect(results.violations.length).toBe(0);
 * });
 * ```
 */
export async function testAccessibility<T>(
  fixture: ComponentFixture<T>,
  options?: A11yTestOptions
): Promise<AxeResults> {
  fixture.detectChanges();

  const element = fixture.nativeElement as HTMLElement;

  // Configure axe-core if spec is provided
  if (options?.spec) {
    axe.configure(options.spec);
  }

  const runOptions: RunOptions = options?.runOptions ?? {};

  // Merge disabled rules into run options
  if (options?.disableRules?.length) {
    runOptions.rules = runOptions.rules ?? {};
    for (const rule of options.disableRules) {
      (runOptions.rules as Record<string, { enabled: boolean }>)[rule] = { enabled: false };
    }
  }

  return axe.run(element, runOptions);
}

/**
 * Formats axe-core violations into a human-readable string for test failure messages.
 */
export function formatViolations(violations: AxeResults['violations']): string {
  if (violations.length === 0) return 'No violations found.';

  return violations
    .map((violation) => {
      const nodes = violation.nodes
        .map((node) => `  - ${node.html}\n    Fix: ${node.failureSummary}`)
        .join('\n');
      return `[${violation.impact}] ${violation.id}: ${violation.description}\n${nodes}`;
    })
    .join('\n\n');
}

/**
 * Jasmine custom matcher helper - asserts no accessibility violations exist.
 * Use within an `it` block:
 *
 * @example
 * ```typescript
 * it('should pass accessibility checks', async () => {
 *   const results = await testAccessibility(fixture);
 *   expectNoViolations(results);
 * });
 * ```
 */
export function expectNoViolations(results: AxeResults): void {
  const violations = results.violations;
  if (violations.length > 0) {
    throw new Error(
      `Expected no accessibility violations but found ${violations.length}:\n\n${formatViolations(violations)}`
    );
  }
}
