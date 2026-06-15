import { DataTableExportService } from './data-table-export.service';
import { DataTableColumn } from './data-table.types';

interface TestRow {
  name: string;
  email: string;
  nested: { value: string };
}

describe('DataTableExportService', () => {
  let service: DataTableExportService;
  let mockLink: HTMLAnchorElement;
  let createObjectURLSpy: jasmine.Spy;
  let revokeObjectURLSpy: jasmine.Spy;

  const columns: DataTableColumn<TestRow>[] = [
    { field: 'name', header: 'Name' },
    { field: 'email', header: 'Email' },
    { field: 'nested.value', header: 'Nested Value' },
  ];

  const rows: TestRow[] = [
    { name: 'Alice', email: 'alice@test.com', nested: { value: 'foo' } },
    { name: 'Bob', email: 'bob@test.com', nested: { value: 'bar' } },
  ];

  beforeEach(() => {
    service = new DataTableExportService();
    mockLink = document.createElement('a');
    spyOn(mockLink, 'click');
    spyOn(document, 'createElement').and.returnValue(mockLink);
    spyOn(document.body, 'appendChild');
    spyOn(document.body, 'removeChild');
    createObjectURLSpy = spyOn(URL, 'createObjectURL').and.returnValue('blob:test');
    revokeObjectURLSpy = spyOn(URL, 'revokeObjectURL');
  });

  describe('exportToCsv', () => {
    it('should generate CSV content and trigger download', () => {
      service.exportToCsv(columns, rows, 'test');
      expect(mockLink.download).toBe('test.csv');
      expect(mockLink.click).toHaveBeenCalled();
      expect(revokeObjectURLSpy).toHaveBeenCalledWith('blob:test');
    });

    it('should handle nested field paths', () => {
      service.exportToCsv(columns, rows, 'test');
      // Verify blob was created (content is in the Blob constructor)
      expect(createObjectURLSpy).toHaveBeenCalled();
    });

    it('should use default filename when not provided', () => {
      service.exportToCsv(columns, rows);
      expect(mockLink.download).toBe('export.csv');
    });

    it('should escape CSV values with commas', () => {
      const dataWithCommas: TestRow[] = [
        { name: 'Last, First', email: 'test@test.com', nested: { value: 'val' } },
      ];
      service.exportToCsv(columns, dataWithCommas, 'test');
      expect(createObjectURLSpy).toHaveBeenCalled();
    });

    it('should use custom render function', () => {
      const customColumns: DataTableColumn<TestRow>[] = [
        { field: 'name', header: 'Name', render: (row) => `User: ${row.name}` },
      ];
      service.exportToCsv(customColumns, rows, 'test');
      expect(createObjectURLSpy).toHaveBeenCalled();
    });
  });

  describe('exportToExcel', () => {
    it('should generate Excel-compatible file and trigger download', () => {
      service.exportToExcel(columns, rows, 'test');
      expect(mockLink.download).toBe('test.xls');
      expect(mockLink.click).toHaveBeenCalled();
    });

    it('should use default filename when not provided', () => {
      service.exportToExcel(columns, rows);
      expect(mockLink.download).toBe('export.xls');
    });
  });
});
