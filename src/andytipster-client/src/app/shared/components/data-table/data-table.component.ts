import {
  ChangeDetectionStrategy,
  Component,
  computed,
  DestroyRef,
  inject,
  input,
  OnInit,
  output,
  signal,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { debounceTime, distinctUntilChanged, Subject } from 'rxjs';

import { EmptyStateComponent } from '../empty-state/empty-state.component';
import { SkeletonLoaderComponent } from '../skeleton-loader/skeleton-loader.component';
import { DataTableExportService } from './data-table-export.service';
import {
  BulkAction,
  BulkActionEvent,
  DataTableColumn,
  EmptyStateConfig,
  ExportEvent,
  FilterState,
  PageChangeEvent,
  SortState,
} from './data-table.types';

@Component({
  selector: 'app-data-table',
  standalone: true,
  imports: [FormsModule, SkeletonLoaderComponent, EmptyStateComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './data-table.component.html',
})
export class DataTableComponent<T = any> implements OnInit {
  private readonly destroyRef = inject(DestroyRef);
  private readonly exportService = inject(DataTableExportService);

  // Inputs
  readonly columns = input.required<DataTableColumn<T>[]>();
  readonly data = input<T[]>([]);
  readonly loading = input<boolean>(false);
  readonly error = input<string | null>(null);
  readonly totalCount = input<number>(0);
  readonly pageSize = input<number>(25);
  readonly emptyStateConfig = input<EmptyStateConfig>({});
  readonly bulkActions = input<BulkAction[]>([]);
  readonly selectable = input<boolean>(true);
  readonly exportable = input<boolean>(true);

  // Outputs
  readonly pageChange = output<PageChangeEvent>();
  readonly sortChange = output<SortState[]>();
  readonly filterChange = output<FilterState[]>();
  readonly selectionChange = output<T[]>();
  readonly search = output<string>();
  readonly bulkAction = output<BulkActionEvent>();
  readonly export = output<ExportEvent>();
  readonly retry = output<void>();

  // Internal state
  readonly currentPage = signal(1);
  readonly currentPageSize = signal(25);
  readonly searchText = signal('');
  readonly sortStates = signal<SortState[]>([]);
  readonly filterStates = signal<FilterState[]>([]);
  readonly selectedRows = signal<Set<number>>(new Set());
  readonly showFilters = signal(false);

  private readonly searchSubject = new Subject<string>();

  readonly pageSizeOptions = [10, 25, 50, 100];

  readonly totalPages = computed(() => {
    const total = this.totalCount();
    const size = this.currentPageSize();
    return Math.max(1, Math.ceil(total / size));
  });

  readonly paginationRange = computed(() => {
    const page = this.currentPage();
    const size = this.currentPageSize();
    const total = this.totalCount();
    const start = (page - 1) * size + 1;
    const end = Math.min(page * size, total);
    return { start, end, total };
  });

  readonly allSelected = computed(() => {
    const data = this.data();
    if (data.length === 0) return false;
    return this.selectedRows().size === data.length;
  });

  readonly someSelected = computed(() => {
    const selected = this.selectedRows();
    return selected.size > 0 && selected.size < this.data().length;
  });

  readonly selectedItems = computed(() => {
    const data = this.data();
    const selected = this.selectedRows();
    return data.filter((_, index) => selected.has(index));
  });

  readonly hasActiveFilters = computed(() => this.filterStates().length > 0);

  ngOnInit(): void {
    this.currentPageSize.set(this.pageSize());

    this.searchSubject
      .pipe(debounceTime(300), distinctUntilChanged(), takeUntilDestroyed(this.destroyRef))
      .subscribe((text) => {
        this.search.emit(text);
      });
  }

  onSearchInput(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    this.searchText.set(value);
    this.searchSubject.next(value);
  }

  onPageSizeChange(event: Event): void {
    const size = Number((event.target as HTMLSelectElement).value);
    this.currentPageSize.set(size);
    this.currentPage.set(1);
    this.pageChange.emit({ page: 1, pageSize: size });
  }

  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages()) return;
    this.currentPage.set(page);
    this.pageChange.emit({ page, pageSize: this.currentPageSize() });
  }

  toggleSort(column: DataTableColumn<T>): void {
    if (!column.sortable) return;

    const current = this.sortStates();
    const existing = current.find((s) => s.column === column.field);

    let newStates: SortState[];
    if (!existing) {
      newStates = [{ column: column.field, direction: 'asc' }];
    } else if (existing.direction === 'asc') {
      newStates = [{ column: column.field, direction: 'desc' }];
    } else {
      newStates = [];
    }

    this.sortStates.set(newStates);
    this.sortChange.emit(newStates);
  }

  getSortDirection(field: string): 'asc' | 'desc' | null {
    const state = this.sortStates().find((s) => s.column === field);
    return state?.direction ?? null;
  }

  onFilterChange(column: DataTableColumn<T>, value: any): void {
    const current = this.filterStates();
    let newFilters: FilterState[];

    if (value === '' || value === null || value === undefined) {
      newFilters = current.filter((f) => f.column !== column.field);
    } else {
      const existing = current.find((f) => f.column === column.field);
      if (existing) {
        newFilters = current.map((f) => (f.column === column.field ? { ...f, value } : f));
      } else {
        newFilters = [...current, { column: column.field, value }];
      }
    }

    this.filterStates.set(newFilters);
    this.filterChange.emit(newFilters);
  }

  getFilterValue(field: string): any {
    return this.filterStates().find((f) => f.column === field)?.value ?? '';
  }

  toggleSelectAll(): void {
    const data = this.data();
    if (this.allSelected()) {
      this.selectedRows.set(new Set());
    } else {
      this.selectedRows.set(new Set(data.map((_, i) => i)));
    }
    this.selectionChange.emit(this.selectedItems());
  }

  toggleRowSelection(index: number): void {
    const current = new Set(this.selectedRows());
    if (current.has(index)) {
      current.delete(index);
    } else {
      current.add(index);
    }
    this.selectedRows.set(current);
    this.selectionChange.emit(this.selectedItems());
  }

  isRowSelected(index: number): boolean {
    return this.selectedRows().has(index);
  }

  onBulkAction(action: BulkAction): void {
    this.bulkAction.emit({
      action: action.id,
      selectedRows: this.selectedItems(),
    });
  }

  clearSelection(): void {
    this.selectedRows.set(new Set());
    this.selectionChange.emit([]);
  }

  onExport(format: 'csv' | 'excel', rows: 'filtered' | 'selected'): void {
    this.export.emit({ format, rows });

    const exportRows = rows === 'selected' ? this.selectedItems() : this.data();
    if (format === 'csv') {
      this.exportService.exportToCsv(this.columns(), exportRows);
    } else {
      this.exportService.exportToExcel(this.columns(), exportRows);
    }
  }

  onRetry(): void {
    this.retry.emit();
  }

  toggleFilters(): void {
    this.showFilters.update((v) => !v);
  }

  getCellValue(row: T, column: DataTableColumn<T>): string {
    if (column.render) {
      return column.render(row);
    }
    const value = this.getNestedValue(row, column.field);
    return value != null ? String(value) : '';
  }

  private getNestedValue(obj: any, path: string): any {
    return path.split('.').reduce((current, key) => current?.[key], obj);
  }

  trackByIndex(index: number): number {
    return index;
  }
}
