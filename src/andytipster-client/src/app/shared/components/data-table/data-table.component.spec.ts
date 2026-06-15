import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { DataTableComponent } from './data-table.component';
import { DataTableColumn } from './data-table.types';
import { DataTableExportService } from './data-table-export.service';

interface TestRow {
  id: number;
  name: string;
  email: string;
  status: string;
}

describe('DataTableComponent', () => {
  let component: DataTableComponent<TestRow>;
  let fixture: ComponentFixture<DataTableComponent<TestRow>>;
  let exportService: jasmine.SpyObj<DataTableExportService>;

  const testColumns: DataTableColumn<TestRow>[] = [
    { field: 'id', header: 'ID', sortable: true },
    { field: 'name', header: 'Name', sortable: true, filterable: true, filterType: 'text' },
    {
      field: 'email',
      header: 'Email',
      sortable: true,
      filterable: true,
      filterType: 'text',
    },
    {
      field: 'status',
      header: 'Status',
      filterable: true,
      filterType: 'dropdown',
      filterOptions: [
        { label: 'Active', value: 'active' },
        { label: 'Inactive', value: 'inactive' },
      ],
    },
  ];

  const testData: TestRow[] = [
    { id: 1, name: 'Alice', email: 'alice@test.com', status: 'active' },
    { id: 2, name: 'Bob', email: 'bob@test.com', status: 'inactive' },
    { id: 3, name: 'Charlie', email: 'charlie@test.com', status: 'active' },
  ];

  beforeEach(async () => {
    exportService = jasmine.createSpyObj('DataTableExportService', [
      'exportToCsv',
      'exportToExcel',
    ]);

    await TestBed.configureTestingModule({
      imports: [DataTableComponent],
      providers: [{ provide: DataTableExportService, useValue: exportService }],
    }).compileComponents();

    fixture = TestBed.createComponent(DataTableComponent<TestRow>);
    component = fixture.componentInstance;

    fixture.componentRef.setInput('columns', testColumns);
    fixture.componentRef.setInput('data', testData);
    fixture.componentRef.setInput('totalCount', 3);
    fixture.componentRef.setInput('pageSize', 25);
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('Rendering', () => {
    it('should render table with correct number of rows', () => {
      const rows = fixture.nativeElement.querySelectorAll('tbody tr');
      expect(rows.length).toBe(3);
    });

    it('should render column headers', () => {
      const headers = fixture.nativeElement.querySelectorAll('thead tr:first-child th');
      // +1 for checkbox column
      expect(headers.length).toBe(testColumns.length + 1);
    });

    it('should show loading state when loading', () => {
      fixture.componentRef.setInput('loading', true);
      fixture.detectChanges();
      const skeleton = fixture.nativeElement.querySelector('app-skeleton-loader');
      expect(skeleton).toBeTruthy();
    });

    it('should show empty state when no data', () => {
      fixture.componentRef.setInput('data', []);
      fixture.componentRef.setInput('totalCount', 0);
      fixture.detectChanges();
      const emptyState = fixture.nativeElement.querySelector('app-empty-state');
      expect(emptyState).toBeTruthy();
    });

    it('should show error state with retry button', () => {
      fixture.componentRef.setInput('error', 'Something went wrong');
      fixture.detectChanges();
      const errorText = fixture.nativeElement.querySelector('p');
      expect(errorText.textContent).toContain('Something went wrong');
      const retryBtn = fixture.nativeElement.querySelector('button');
      expect(retryBtn.textContent).toContain('Retry');
    });
  });

  describe('Search', () => {
    it('should debounce search input by 300ms', fakeAsync(() => {
      spyOn(component.search, 'emit');
      const input = fixture.nativeElement.querySelector('input[aria-label="Search table"]');
      input.value = 'test';
      input.dispatchEvent(new Event('input'));
      fixture.detectChanges();

      tick(200);
      expect(component.search.emit).not.toHaveBeenCalled();

      tick(100);
      expect(component.search.emit).toHaveBeenCalledWith('test');
    }));
  });

  describe('Sorting', () => {
    it('should cycle through sort directions: none -> asc -> desc -> none', () => {
      spyOn(component.sortChange, 'emit');

      component.toggleSort(testColumns[0]);
      expect(component.getSortDirection('id')).toBe('asc');

      component.toggleSort(testColumns[0]);
      expect(component.getSortDirection('id')).toBe('desc');

      component.toggleSort(testColumns[0]);
      expect(component.getSortDirection('id')).toBeNull();
    });

    it('should not sort non-sortable columns', () => {
      spyOn(component.sortChange, 'emit');
      component.toggleSort({ field: 'status', header: 'Status', sortable: false });
      expect(component.sortChange.emit).not.toHaveBeenCalled();
    });
  });

  describe('Filtering', () => {
    it('should add a filter state for a column', () => {
      spyOn(component.filterChange, 'emit');
      component.onFilterChange(testColumns[1], 'Alice');
      expect(component.getFilterValue('name')).toBe('Alice');
      expect(component.filterChange.emit).toHaveBeenCalled();
    });

    it('should remove a filter when value is empty', () => {
      component.onFilterChange(testColumns[1], 'Alice');
      component.onFilterChange(testColumns[1], '');
      expect(component.getFilterValue('name')).toBe('');
    });

    it('should toggle filter row visibility', () => {
      expect(component.showFilters()).toBeFalse();
      component.toggleFilters();
      expect(component.showFilters()).toBeTrue();
    });
  });

  describe('Selection', () => {
    it('should toggle individual row selection', () => {
      spyOn(component.selectionChange, 'emit');
      component.toggleRowSelection(0);
      expect(component.isRowSelected(0)).toBeTrue();
      expect(component.selectionChange.emit).toHaveBeenCalled();
    });

    it('should select all rows', () => {
      component.toggleSelectAll();
      expect(component.allSelected()).toBeTrue();
      expect(component.selectedRows().size).toBe(3);
    });

    it('should deselect all rows when all are selected', () => {
      component.toggleSelectAll(); // select all
      component.toggleSelectAll(); // deselect all
      expect(component.selectedRows().size).toBe(0);
    });

    it('should clear selection', () => {
      component.toggleRowSelection(0);
      component.toggleRowSelection(1);
      component.clearSelection();
      expect(component.selectedRows().size).toBe(0);
    });
  });

  describe('Pagination', () => {
    it('should calculate total pages', () => {
      fixture.componentRef.setInput('totalCount', 100);
      fixture.detectChanges();
      expect(component.totalPages()).toBe(4); // 100 / 25
    });

    it('should navigate to next page', () => {
      spyOn(component.pageChange, 'emit');
      fixture.componentRef.setInput('totalCount', 100);
      fixture.detectChanges();

      component.goToPage(2);
      expect(component.currentPage()).toBe(2);
      expect(component.pageChange.emit).toHaveBeenCalledWith({ page: 2, pageSize: 25 });
    });

    it('should not navigate beyond total pages', () => {
      fixture.componentRef.setInput('totalCount', 3);
      fixture.detectChanges();
      component.goToPage(2);
      expect(component.currentPage()).toBe(1); // totalPages is 1
    });

    it('should change page size and reset to page 1', () => {
      spyOn(component.pageChange, 'emit');
      component.goToPage(2); // move to page 2

      const event = { target: { value: '50' } } as unknown as Event;
      component.onPageSizeChange(event);

      expect(component.currentPageSize()).toBe(50);
      expect(component.currentPage()).toBe(1);
      expect(component.pageChange.emit).toHaveBeenCalledWith({ page: 1, pageSize: 50 });
    });

    it('should show correct pagination range', () => {
      fixture.componentRef.setInput('totalCount', 100);
      fixture.detectChanges();
      const range = component.paginationRange();
      expect(range.start).toBe(1);
      expect(range.end).toBe(25);
      expect(range.total).toBe(100);
    });
  });

  describe('Export', () => {
    it('should call export service for CSV', () => {
      spyOn(component.export, 'emit');
      component.onExport('csv', 'filtered');
      expect(exportService.exportToCsv).toHaveBeenCalledWith(testColumns, testData);
      expect(component.export.emit).toHaveBeenCalledWith({ format: 'csv', rows: 'filtered' });
    });

    it('should call export service for Excel', () => {
      spyOn(component.export, 'emit');
      component.onExport('excel', 'filtered');
      expect(exportService.exportToExcel).toHaveBeenCalledWith(testColumns, testData);
    });

    it('should export only selected rows when requested', () => {
      component.toggleRowSelection(0);
      component.onExport('csv', 'selected');
      expect(exportService.exportToCsv).toHaveBeenCalledWith(testColumns, [testData[0]]);
    });
  });

  describe('Bulk Actions', () => {
    it('should emit bulk action event', () => {
      spyOn(component.bulkAction, 'emit');
      component.toggleRowSelection(0);
      component.toggleRowSelection(1);

      component.onBulkAction({ id: 'delete', label: 'Delete', variant: 'error' });
      expect(component.bulkAction.emit).toHaveBeenCalledWith({
        action: 'delete',
        selectedRows: [testData[0], testData[1]],
      });
    });
  });

  describe('Cell Rendering', () => {
    it('should render cell values from field path', () => {
      const value = component.getCellValue(testData[0], testColumns[1]);
      expect(value).toBe('Alice');
    });

    it('should use custom render function when provided', () => {
      const col: DataTableColumn<TestRow> = {
        field: 'name',
        header: 'Name',
        render: (row) => `User: ${row.name}`,
      };
      const value = component.getCellValue(testData[0], col);
      expect(value).toBe('User: Alice');
    });
  });

  describe('Retry', () => {
    it('should emit retry event', () => {
      spyOn(component.retry, 'emit');
      component.onRetry();
      expect(component.retry.emit).toHaveBeenCalled();
    });
  });
});
