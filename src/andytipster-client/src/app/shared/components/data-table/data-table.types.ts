export interface DataTableColumn<T = any> {
  field: string;
  header: string;
  sortable?: boolean;
  filterable?: boolean;
  filterType?: 'text' | 'dropdown' | 'date-range';
  filterOptions?: { label: string; value: any }[];
  render?: (row: T) => string;
  width?: string;
}

export interface PageChangeEvent {
  page: number;
  pageSize: number;
}

export interface SortState {
  column: string;
  direction: 'asc' | 'desc';
}

export interface FilterState {
  column: string;
  value: any;
}

export interface BulkActionEvent {
  action: string;
  selectedRows: any[];
}

export interface ExportEvent {
  format: 'csv' | 'excel';
  rows: 'filtered' | 'selected';
}

export interface EmptyStateConfig {
  title?: string;
  message?: string;
  ctaText?: string;
  ctaRoute?: string;
}

export interface BulkAction {
  id: string;
  label: string;
  icon?: string;
  variant?: 'primary' | 'secondary' | 'error' | 'warning';
}
