import DownloadTable from '@common/modules/table-tool/components/DownloadTable';
import {
  CategoryFilter,
  Indicator,
  LocationFilter,
  TimePeriodFilter,
} from '@common/modules/table-tool/types/filters';
import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import React, { createRef } from 'react';
import { WorkBook, writeFile } from 'xlsx';

// Mock this util to avoid errors being thrown due to unimplemented
// navigation APIs being used (i.e. from link being clicked).
jest.mock('@common/utils/file/downloadFile', () => jest.fn());

jest.mock('xlsx', () => {
  const { utils } = jest.requireActual('xlsx');

  return {
    writeFile: jest.fn(),
    utils,
  };
});

describe('DownloadTable', () => {
  const basicSubjectMeta: FullTableMeta = {
    geoJsonAvailable: false,
    publicationName: '',
    subjectName: 'The subject',
    footnotes: [],
    boundaryLevels: [],
    filters: {
      Characteristic: {
        name: 'characteristic',
        options: [
          new CategoryFilter({
            value: 'gender-female',
            label: 'Female',
            group: 'Gender',
            category: 'Characteristic',
          }),
        ],
        order: 0,
      },
    },
    indicators: [
      new Indicator({
        label: 'Authorised absence rate',
        value: 'authorised-absence-rate',
        unit: '%',
        name: 'sess_authorised_percent',
      }),
    ],
    locations: [
      new LocationFilter({
        id: 'england-id',
        value: 'england',
        label: 'England',
        level: 'country',
      }),
    ],
    timePeriodRange: [
      new TimePeriodFilter({
        code: 'AY',
        year: 2015,
        label: '2015/16',
        order: 0,
      }),
    ],
  };

  test('renders the form', () => {
    const onCsvDownload = jest.fn();
    const ref = createRef<HTMLElement>();

    render(
      <DownloadTable
        fileName="The file name"
        fullTable={{
          subjectMeta: basicSubjectMeta,
          results: [],
        }}
        tableRef={ref}
        onCsvDownload={onCsvDownload}
      />,
    );

    expect(screen.getByText('Download Table')).toBeInTheDocument();
    expect(
      screen.getByRole('radio', {
        name: 'Table in ODS format (spreadsheet, with title and footnotes)',
      }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('radio', {
        name: 'Table in CSV format (flat file, with location codes)',
      }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('button', {
        name: 'Download table',
      }),
    ).toBeInTheDocument();
  });

  test('downloads the ods file', async () => {
    const onCsvDownload = jest.fn();
    const ref = createRef<HTMLTableElement>();

    render(
      <>
        <table ref={ref}>
          <tbody>
            <tr>
              <th />
              <th>Date 1</th>
              <th>Date 2</th>
            </tr>
            <tr>
              <th>Indicator</th>
              <td>101</td>
              <td>102</td>
            </tr>
          </tbody>
        </table>
        <DownloadTable
          fileName="The file name"
          fullTable={{
            subjectMeta: basicSubjectMeta,
            results: [],
          }}
          tableRef={ref}
          onCsvDownload={onCsvDownload}
        />
      </>,
    );

    userEvent.click(
      screen.getByRole('radio', {
        name: 'Table in ODS format (spreadsheet, with title and footnotes)',
      }),
    );
    userEvent.click(
      screen.getByRole('button', {
        name: 'Download table',
      }),
    );

    const mockedWriteFile = writeFile as jest.Mock;

    await waitFor(() => {
      expect(mockedWriteFile).toHaveBeenCalledTimes(1);

      const workbook = mockedWriteFile.mock.calls[0][0] as WorkBook;

      expect(workbook.Sheets.Sheet1.A1.v).toBe(
        "Authorised absence rate for 'The subject' for Female in England for 2015/16",
      );
      expect(workbook.Sheets.Sheet1.B3.v).toBe('Date 1');
      expect(workbook.Sheets.Sheet1.C3.v).toBe('Date 2');
      expect(workbook.Sheets.Sheet1.A4.v).toBe('Indicator');
      expect(workbook.Sheets.Sheet1.B4.v).toBe('101');
      expect(workbook.Sheets.Sheet1.C4.v).toBe('102');

      expect(mockedWriteFile.mock.calls[0][1]).toBe('The file name.ods');
    });
  });

  test('calls the `onCsvDownload` handler when downloading the csv file', async () => {
    const onCsvDownload = jest.fn();
    const ref = createRef<HTMLElement>();

    render(
      <DownloadTable
        fileName="The file name"
        fullTable={{
          subjectMeta: basicSubjectMeta,
          results: [],
        }}
        tableRef={ref}
        onCsvDownload={onCsvDownload}
      />,
    );

    userEvent.click(
      screen.getByRole('radio', {
        name: 'Table in CSV format (flat file, with location codes)',
      }),
    );

    expect(onCsvDownload).not.toHaveBeenCalled();

    userEvent.click(
      screen.getByRole('button', {
        name: 'Download table',
      }),
    );

    await waitFor(() => {
      expect(onCsvDownload).toHaveBeenCalledTimes(1);
    });
  });
});
