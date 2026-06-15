import { ComponentFixture, TestBed } from '@angular/core/testing';
import { DataTableComponent } from './data-table.component';
import { DataTableColumn } from './data-table.types';
import { testAccessibility, expectNoViolations } from '../../../testing/accessibility.helper';

interface TestRow {
  id: number;
  name: string;
  email: string;
}

describe('DataTableComponent Accessibility', () => {
  let fixture: ComponentFixture<DataTableComponent<TestRow>>;
  let component: DataTableComponent<TestRow>;

  const testColumns: DataTableColumn<TestRow>[] = [
    { field: 'id', header: 'ID', sortable: true },
    { field: 'name', header: 'Name', sortable: true, filterable: true },
    { field: 'email', header: 'Email', sortable: false, filterable: true },
  ];

  const testData: TestRow[] = [
    { id: 1, name: 'Alice Johnson', email: 'alice@example.com' },
    { id: 2, name: 'Bob Smith', email: 'bob@example.com' },
    { id: 3, name: 'Charlie Brown', email: 'charlie@example.com' },
  ];

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DataTableComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(DataTableComponent<TestRow>);
    component = fixture.componentInstance;

    // Set required inputs
    fixture.componentRef.setInput('columns', testColumns);
    fixture.componentRef.setInput('data', testData);
    fixture.componentRef.setInput('totalCount', testData.length);
    fixture.componentRef.setInput('loading', false);
    fixture.componentRef.setInput('error', null);

    fixture.detectChanges();
  });

  it('should have no accessibility violations in default state', async () => {
    const results = await testAccessibility(fixture, {
      // Disable color-contrast because jsdom/karma doesn't compute styles accurately
      disableRules: ['color-contrast'],
    });
    expectNoViolations(results);
    expect(results.violations.length).toBe(0);
  });

  it('should have proper table structure with role="grid"', () => {
    const table = fixture.nativeElement.querySelector('table');
    expect(table).toBeTruthy();
    expect(table.getAttribute('role')).toBe('grid');
    expect(table.getAttribute('aria-label')).toBe('Data table');
  });

  it('should have aria-sort on sortable column headers', () => {
    const headers = fixture.nativeElement.querySelectorAll('th[aria-sort]');
    expect(headers.length).toBeGreaterThan(0);
  });

  it('should have aria-label on search input', () => {
    const searchInput = fixture.nativeElement.querySelector('input[type="text"]');
    expect(searchInput.getAttribute('aria-label')).toBe('Search table');
  });

  it('should have aria-label on pagination controls', () => {
    const prevBtn = fixture.nativeElement.querySelector('button[aria-label="Previous page"]');
    const nextBtn = fixture.nativeElement.querySelector('button[aria-label="Next page"]');
    expect(prevBtn).toBeTruthy();
    expect(nextBtn).toBeTruthy();
  });

  it('should have aria-current on current page indicator', () => {
    const currentPage = fixture.nativeElement.querySelector('[aria-current="page"]');
    expect(currentPage).toBeTruthy();
  });

  it('should have no accessibility violations in error state', async () => {
    fixture.componentRef.setInput('error', 'Something went wrong');
    fixture.componentRef.setInput('data', []);
    fixture.detectChanges();

    const results = await testAccessibility(fixture, {
      disableRules: ['color-contrast'],
    });
    expectNoViolations(results);
    expect(results.violations.length).toBe(0);
  });

  it('should have alert role on error state', () => {
    fixture.componentRef.setInput('error', 'Network error');
    fixture.componentRef.setInput('data', []);
    fixture.detectChanges();

    const alert = fixture.nativeElement.querySelector('[role="alert"]');
    expect(alert).toBeTruthy();
  });
});
