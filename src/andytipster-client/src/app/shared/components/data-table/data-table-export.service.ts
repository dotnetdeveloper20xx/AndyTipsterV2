import { Injectable } from '@angular/core';
import { DataTableColumn } from './data-table.types';

@Injectable({ providedIn: 'root' })
export class DataTableExportService {
  /**
   * Export rows to CSV format and trigger download.
   */
  exportToCsv<T>(columns: DataTableColumn<T>[], rows: T[], filename = 'export'): void {
    const headers = columns.map((col) => col.header);
    const csvRows: string[] = [headers.join(',')];

    for (const row of rows) {
      const values = columns.map((col) => {
        const value = this.getNestedValue(row, col.field);
        const rendered = col.render ? col.render(row) : String(value ?? '');
        return this.escapeCsvValue(rendered);
      });
      csvRows.push(values.join(','));
    }

    const csvContent = csvRows.join('\n');
    this.downloadFile(csvContent, `${filename}.csv`, 'text/csv;charset=utf-8;');
  }

  /**
   * Export rows to Excel-compatible CSV (with BOM for proper encoding).
   */
  exportToExcel<T>(columns: DataTableColumn<T>[], rows: T[], filename = 'export'): void {
    const headers = columns.map((col) => col.header);
    const tsvRows: string[] = [headers.join('\t')];

    for (const row of rows) {
      const values = columns.map((col) => {
        const value = this.getNestedValue(row, col.field);
        const rendered = col.render ? col.render(row) : String(value ?? '');
        return rendered.replace(/\t/g, ' ').replace(/\n/g, ' ');
      });
      tsvRows.push(values.join('\t'));
    }

    // BOM + TSV content for Excel compatibility
    const bom = '\uFEFF';
    const content = bom + tsvRows.join('\n');
    this.downloadFile(content, `${filename}.xls`, 'application/vnd.ms-excel;charset=utf-8;');
  }

  private escapeCsvValue(value: string): string {
    if (value.includes(',') || value.includes('"') || value.includes('\n')) {
      return `"${value.replace(/"/g, '""')}"`;
    }
    return value;
  }

  private getNestedValue(obj: any, path: string): any {
    return path.split('.').reduce((current, key) => current?.[key], obj);
  }

  private downloadFile(content: string, filename: string, mimeType: string): void {
    const blob = new Blob([content], { type: mimeType });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = filename;
    link.style.display = 'none';
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    URL.revokeObjectURL(url);
  }
}
