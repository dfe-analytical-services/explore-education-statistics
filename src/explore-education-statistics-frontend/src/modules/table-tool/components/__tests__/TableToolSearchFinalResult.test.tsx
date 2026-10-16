import render from '@common-test/render';
import _tableBuilderService from '@common/services/tableBuilderService';
import { testReleaseVersionSummary } from '@frontend/modules/find-statistics/__tests__/__data__/testReleaseData';
import TableToolSearchFinalResult from '@frontend/modules/table-tool/components/TableToolSearchFinalResult';
import { screen } from '@testing-library/react';
import { testFinalResult, testTableDataResponse } from './__data__/tableData';

jest.mock('@common/services/tableBuilderService');

const tableBuilderService = jest.mocked(_tableBuilderService);

describe('TableToolSearchFinalResult', () => {
  test('renders dataset details correctly', async () => {
    tableBuilderService.getTableData.mockResolvedValue(testTableDataResponse);

    render(
      <TableToolSearchFinalResult
        releaseVersionSummary={testReleaseVersionSummary}
        dataset={testFinalResult}
      />,
    );

    expect(
      screen.getByRole('heading', { name: 'Test dataset title' }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('link', { name: /View this data set/ }),
    ).toHaveAttribute('href', '/data-catalogue/data-set/test-data-set-file-id');

    expect(
      screen.getByText('Test AI relevance summary explanation.'),
    ).toBeInTheDocument();
  });

  test('renders table when table query resolves successfully', async () => {
    tableBuilderService.getTableData.mockResolvedValue(testTableDataResponse);

    render(
      <TableToolSearchFinalResult
        releaseVersionSummary={testReleaseVersionSummary}
        dataset={testFinalResult}
      />,
    );

    expect(await screen.findByRole('table')).toBeInTheDocument();
    expect(screen.getByTestId('dataTableCaption')).toHaveTextContent(
      /Number of applications received/,
    );

    expect(
      screen.getByRole('link', { name: /View and edit this table/ }),
    ).toHaveAttribute(
      'href',
      '/data-tables/publication-slug/release-slug?fromSearch&sub=testsubjectid&tp=2014%7CAY%7C2016%7CAY',
    );
  });

  test('renders error message when table query fails', async () => {
    tableBuilderService.getTableData.mockRejectedValue(new Error('API error'));

    render(
      <TableToolSearchFinalResult
        releaseVersionSummary={testReleaseVersionSummary}
        dataset={testFinalResult}
      />,
    );

    expect(
      await screen.findByText('Error loading table preview.'),
    ).toBeInTheDocument();
    expect(screen.queryByRole('table')).not.toBeInTheDocument();
  });

  test('renders validation warnings if present', async () => {
    tableBuilderService.getTableData.mockResolvedValue(testTableDataResponse);

    render(
      <TableToolSearchFinalResult
        releaseVersionSummary={testReleaseVersionSummary}
        dataset={{
          ...testFinalResult,
          validationWarnings: [
            {
              code: 'no_location_requirement',
              message: 'Test location warning message.',
            },
            {
              code: 'unfiltered_filters',
              message: 'Test filter warning message.',
            },
          ],
        }}
      />,
    );

    expect(
      await screen.findByText('Test location warning message.'),
    ).toBeInTheDocument();
    expect(
      screen.getByText('Test filter warning message.'),
    ).toBeInTheDocument();

    expect(tableBuilderService.getTableData).toHaveBeenCalledTimes(1);
  });

  test('renders validation errors instead of the table when the data set is not valid for table generation', () => {
    render(
      <TableToolSearchFinalResult
        releaseVersionSummary={testReleaseVersionSummary}
        dataset={{
          ...testFinalResult,
          isValidForTableGeneration: false,
          validationErrors: [
            {
              code: 'no_location',
              message: 'Test location error message.',
            },
            {
              code: 'no_time_period',
              message: 'Test time period error message.',
            },
          ],
          validationWarnings: [
            {
              code: 'unfiltered_filters',
              message: 'Test filter warning message.',
            },
          ],
        }}
      />,
    );

    expect(
      screen.getByText('Test location error message.'),
    ).toBeInTheDocument();
    expect(
      screen.getByText('Test time period error message.'),
    ).toBeInTheDocument();

    expect(
      screen.queryByText('Test filter warning message.'),
    ).not.toBeInTheDocument();
    expect(screen.queryByRole('table')).not.toBeInTheDocument();
    expect(
      screen.queryByRole('link', { name: /View and edit this table/ }),
    ).not.toBeInTheDocument();
    expect(screen.queryByTestId('loadingSpinner')).not.toBeInTheDocument();
    expect(tableBuilderService.getTableData).not.toHaveBeenCalled();
  });

  test('renders a fallback error when the data set is not valid for table generation and has no validation errors', () => {
    render(
      <TableToolSearchFinalResult
        releaseVersionSummary={testReleaseVersionSummary}
        dataset={{
          ...testFinalResult,
          isValidForTableGeneration: false,
        }}
      />,
    );

    expect(
      screen.getByText('A table preview could not be generated.'),
    ).toBeInTheDocument();
    expect(screen.queryByRole('table')).not.toBeInTheDocument();
    expect(tableBuilderService.getTableData).not.toHaveBeenCalled();
  });
});
