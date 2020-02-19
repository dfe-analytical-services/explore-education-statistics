import {
  appendColumnWidths,
  appendFootnotes,
} from '@common/modules/table-tool/components/DownloadExcelButton';
import { utils } from 'xlsx';

describe('DownloadExcelButton', () => {
  describe('appendColumnWidths', () => {
    test('sets column widths using the cells with the longest string content', () => {
      const sheet = utils.aoa_to_sheet([
        ['test', 'test', 'testtest'],
        ['test', 'testtest', 'test'],
        ['testtest', 'test', 'testtesttest'],
      ]);

      appendColumnWidths(sheet);

      expect(sheet['!cols']).toEqual([{ wch: 8 }, { wch: 8 }, { wch: 12 }]);
    });

    test('sets column widths using the cells with the longest number content', () => {
      const sheet = utils.aoa_to_sheet([
        ['test', 'test', 'testtest'],
        ['test', 12345678, 'test'],
        ['testtest', 'test', 'testtesttest'],
      ]);

      appendColumnWidths(sheet);

      expect(sheet['!cols']).toEqual([{ wch: 8 }, { wch: 8 }, { wch: 12 }]);
    });
  });

  describe('appendFootnotes', () => {
    test('adds single footnote to end of sheet', () => {
      const sheet = utils.aoa_to_sheet([
        ['test', 'test', 'test'],
        ['test', 'test', 'test'],
        ['test', 'test', 'test'],
      ]);

      appendFootnotes(sheet, [
        {
          id: '1',
          label: 'Test footnote 1',
        },
      ]);

      expect(sheet['!ref']).toBe('A1:C5');
      expect(sheet.A5).toEqual({
        t: 's',
        v: '(1) Test footnote 1',
      });
    });

    test('adds multiple footnotes to end of sheet', () => {
      const sheet = utils.aoa_to_sheet([
        ['test', 'test', 'test'],
        ['test', 'test', 'test'],
        ['test', 'test', 'test'],
      ]);

      appendFootnotes(sheet, [
        {
          id: '1',
          label: 'Test footnote 1',
        },
        {
          id: '2',
          label: 'Test footnote 2',
        },
        {
          id: '3',
          label: 'Test footnote 3',
        },
      ]);

      expect(sheet['!ref']).toBe('A1:C7');
      expect(sheet.A5).toEqual({
        t: 's',
        v: '(1) Test footnote 1',
      });
      expect(sheet.A6).toEqual({
        t: 's',
        v: '(2) Test footnote 2',
      });
      expect(sheet.A7).toEqual({
        t: 's',
        v: '(3) Test footnote 3',
      });
    });
  });
});
