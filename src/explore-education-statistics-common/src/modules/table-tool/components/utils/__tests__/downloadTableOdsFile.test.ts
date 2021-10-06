import {
  appendColumnWidths,
  appendFootnotes,
  appendTitle,
} from '@common/modules/table-tool/components/utils/downloadTableOdsFile';
import { utils } from 'xlsx';

describe('downloadTableOdsFile', () => {
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

  describe('appendTitle', () => {
    test('adds title to start of sheet', () => {
      const sheet = utils.aoa_to_sheet([
        ['test', 'test', 'test'],
        ['test', 'test', 'test'],
        ['test', 'test', 'test'],
      ]);

      expect(sheet['!ref']).toBe('A1:C3');

      appendTitle(sheet, 'Test title');

      expect(sheet['!ref']).toBe('A1:C5');

      expect(sheet.A1.v).toBe('Test title');
      expect(sheet.A2.v).toBe('');
      expect(sheet.A3.v).toBe('test');
    });

    test('preserves empty existing cells', () => {
      const sheet = utils.aoa_to_sheet([
        ['', '', 'test'],
        ['test', 'test', 'test'],
        ['test', 'test', 'test'],
      ]);

      expect(sheet['!ref']).toBe('A1:C3');

      appendTitle(sheet, 'Test title');

      expect(sheet['!ref']).toBe('A1:C5');

      expect(sheet.A1.v).toBe('Test title');
      expect(sheet.A2.v).toBe('');
      expect(sheet.A3.v).toBe('');
      expect(sheet.B3.v).toBe('');
    });
  });

  describe('appendFootnotes', () => {
    test('adds single footnote to end of sheet', () => {
      const sheet = utils.aoa_to_sheet([
        ['test', 'test', 'test'],
        ['test', 'test', 'test'],
        ['test', 'test', 'test'],
      ]);

      expect(sheet['!ref']).toBe('A1:C3');

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

      expect(sheet['!ref']).toBe('A1:C3');

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
