import writeCsvFile from '@common/modules/table-tool/components/utils/writeCsvFile';
import { waitFor } from '@testing-library/react';
import { WorkBook, writeFile } from 'xlsx';

jest.mock('xlsx', () => {
  const { utils } = jest.requireActual('xlsx');

  return {
    writeFile: jest.fn(),
    utils,
  };
});

describe('writeCsvFile', () => {
  test('creates and downloads the csv file', async () => {
    writeCsvFile(
      [
        [
          'location',
          'location_code',
          'geographic_level',
          'time_period',
          'characteristic',
          'sess_authorised_percent',
        ],
      ],
      'The file name',
    );

    const mockedWriteFile = writeFile as jest.Mock;
    await waitFor(() => {
      expect(mockedWriteFile).toHaveBeenCalledTimes(1);

      const workbook = mockedWriteFile.mock.calls[0][0] as WorkBook;

      expect(workbook.Sheets.Sheet1.A1.v).toBe('location');
      expect(workbook.Sheets.Sheet1.B1.v).toBe('location_code');
      expect(workbook.Sheets.Sheet1.C1.v).toBe('geographic_level');
      expect(workbook.Sheets.Sheet1.D1.v).toBe('time_period');
      expect(workbook.Sheets.Sheet1.E1.v).toBe('characteristic');
      expect(workbook.Sheets.Sheet1.F1.v).toBe('sess_authorised_percent');

      expect(mockedWriteFile.mock.calls[0][1]).toBe('The file name.csv');
    });
  });
});
