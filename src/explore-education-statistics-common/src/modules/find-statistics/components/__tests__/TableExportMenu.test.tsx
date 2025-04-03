import TableExportMenu from '@common/modules/find-statistics/components/TableExportMenu';
import React, { createRef } from 'react';
import { screen, waitFor } from '@testing-library/react';
import render from '@common-test/render';
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

describe('TableExportMenu', () => {
  test('renders', () => {
    const ref = createRef<HTMLTableElement>();

    render(
      <TableExportMenu
        tableRef={ref}
        title="Test title"
        onCsvDownload={jest.fn()}
      />,
    );

    expect(
      screen.getByRole('button', { name: 'Export options for Test title' }),
    ).toBeInTheDocument();

    expect(
      screen.queryByRole('button', { name: 'Download table as ODS' }),
    ).not.toBeInTheDocument();

    expect(
      screen.queryByRole('button', { name: 'Download table as CSV' }),
    ).not.toBeInTheDocument();

    expect(
      screen.queryByRole('button', { name: 'Copy table to clipboard' }),
    ).not.toBeInTheDocument();
  });

  test('renders the options when expanded', async () => {
    const ref = createRef<HTMLTableElement>();

    const { user } = render(
      <TableExportMenu
        tableRef={ref}
        title="Test title"
        onCsvDownload={jest.fn()}
      />,
    );

    await user.click(
      screen.getByRole('button', { name: 'Export options for Test title' }),
    );

    expect(
      screen.getByRole('button', { name: 'Download table as ODS' }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('button', { name: 'Download table as CSV' }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('button', { name: 'Copy table to clipboard' }),
    ).toBeInTheDocument();
  });

  test('downloads the table as an ODS file', async () => {
    const ref = createRef<HTMLTableElement>();

    const { user } = render(
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
        <TableExportMenu
          tableRef={ref}
          title="Test title"
          onCsvDownload={jest.fn()}
        />
      </>,
    );

    await user.click(
      screen.getByRole('button', { name: 'Export options for Test title' }),
    );

    await user.click(
      screen.getByRole('button', { name: 'Download table as ODS' }),
    );

    const mockedWriteFile = writeFile as jest.Mock;

    await waitFor(() => {
      expect(mockedWriteFile).toHaveBeenCalledTimes(1);

      const workbook = mockedWriteFile.mock.calls[0][0] as WorkBook;

      expect(workbook.Sheets.Sheet1.A1.v).toBe('Test title');
      expect(workbook.Sheets.Sheet1.B3.v).toBe('Date 1');
      expect(workbook.Sheets.Sheet1.C3.v).toBe('Date 2');
      expect(workbook.Sheets.Sheet1.A4.v).toBe('Indicator');
      expect(workbook.Sheets.Sheet1.B4.v).toBe('101');
      expect(workbook.Sheets.Sheet1.C4.v).toBe('102');

      expect(mockedWriteFile.mock.calls[0][1]).toBe('table.ods');
    });
  });

  test('calls the CSV download handler', async () => {
    const ref = createRef<HTMLTableElement>();
    const handleCsvDownload = jest.fn();

    const { user } = render(
      <TableExportMenu
        tableRef={ref}
        title="Test title"
        onCsvDownload={handleCsvDownload}
      />,
    );

    await user.click(
      screen.getByRole('button', { name: 'Export options for Test title' }),
    );

    expect(handleCsvDownload).not.toHaveBeenCalled();

    await user.click(
      screen.getByRole('button', { name: 'Download table as CSV' }),
    );

    expect(handleCsvDownload).toHaveBeenCalledTimes(1);
  });
});
