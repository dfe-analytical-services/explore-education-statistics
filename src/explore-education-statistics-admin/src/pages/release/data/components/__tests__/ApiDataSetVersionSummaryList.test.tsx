import ApiDataSetVersionSummaryList from '@admin/pages/release/data/components/ApiDataSetVersionSummaryList';
import { ApiDataSetDraftVersion } from '@admin/services/apiDataSetService';
import { render as baseRender, screen, within } from '@testing-library/react';
import { ReactNode } from 'react';
import { MemoryRouter } from 'react-router-dom';

describe('ApiDataSetVersionSummaryList', () => {
  const testBaseVersion: ApiDataSetDraftVersion = {
    id: 'version-id',
    status: 'Draft',
    version: '1.0',
    type: 'Major',
    file: {
      id: 'file-id',
      title: 'Test data set file',
    },
    releaseVersion: {
      id: 'release-id',
      title: 'Test release',
    },
    totalResults: 0,
  };

  test('renders correctly when data set version is missing facets', () => {
    render(
      <ApiDataSetVersionSummaryList
        dataSetVersion={testBaseVersion}
        id="version-details"
        publicationId="publication-id"
      />,
    );

    expect(screen.getByTestId('Version')).toHaveTextContent('v1.0');
    expect(screen.getByTestId('Status')).toHaveTextContent('Ready');
    expect(
      within(screen.getByTestId('Release')).getByRole('link', {
        name: 'Test release',
      }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-id/release/release-id/summary',
    );
    expect(screen.getByTestId('Data set file')).toHaveTextContent(
      'Test data set file',
    );

    expect(screen.queryByTestId('Geographic levels')).not.toBeInTheDocument();
    expect(screen.queryByTestId('Time periods')).not.toBeInTheDocument();
    expect(screen.queryByTestId('Indicators')).not.toBeInTheDocument();
    expect(screen.queryByTestId('Filters')).not.toBeInTheDocument();

    expect(screen.queryByTestId('Actions')).not.toBeInTheDocument();
  });

  test('renders correctly when data set version has facets', () => {
    render(
      <ApiDataSetVersionSummaryList
        dataSetVersion={{
          ...testBaseVersion,
          geographicLevels: ['National', 'Local authority'],
          timePeriods: {
            start: '2018',
            end: '2024',
          },
          indicators: ['Test indicator 1', 'Test indicator 2'],
          filters: ['Test filter 1', 'Test filter 2'],
        }}
        id="version-details"
        publicationId="publication-id"
      />,
    );

    expect(screen.getByTestId('Version')).toHaveTextContent('v1.0');
    expect(screen.getByTestId('Status')).toHaveTextContent('Ready');
    expect(
      within(screen.getByTestId('Release')).getByRole('link', {
        name: 'Test release',
      }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-id/release/release-id/summary',
    );
    expect(screen.getByTestId('Data set file')).toHaveTextContent(
      'Test data set file',
    );

    expect(screen.getByTestId('Geographic levels')).toHaveTextContent(
      'National, Local authority',
    );
    expect(screen.getByTestId('Time periods')).toHaveTextContent(
      '2018 to 2024',
    );

    const indicators = within(screen.getByTestId('Indicators')).getAllByRole(
      'listitem',
    );

    expect(indicators).toHaveLength(2);
    expect(indicators[0]).toHaveTextContent('Test indicator 1');
    expect(indicators[1]).toHaveTextContent('Test indicator 2');

    const filters = within(screen.getByTestId('Filters')).getAllByRole(
      'listitem',
    );

    expect(filters).toHaveLength(2);
    expect(filters[0]).toHaveTextContent('Test filter 1');
    expect(filters[1]).toHaveTextContent('Test filter 2');

    expect(screen.queryByTestId('Actions')).not.toBeInTheDocument();
  });

  test('renders indicators as collapsible list', () => {
    render(
      <ApiDataSetVersionSummaryList
        dataSetVersion={{
          ...testBaseVersion,
          indicators: [
            'Test indicator 1',
            'Test indicator 2',
            'Test indicator 3',
            'Test indicator 4',
          ],
        }}
        id="version-details"
        publicationId="publication-id"
      />,
    );

    const indicators = within(screen.getByTestId('Indicators')).getAllByRole(
      'listitem',
    );

    expect(indicators).toHaveLength(3);
    expect(indicators[0]).toHaveTextContent('Test indicator 1');
    expect(indicators[1]).toHaveTextContent('Test indicator 2');
    expect(indicators[2]).toHaveTextContent('Test indicator 3');

    expect(
      within(screen.getByTestId('Indicators')).getByRole('button', {
        name: 'Show 1 more indicator',
      }),
    ).toBeInTheDocument();
  });

  test('renders indicators as pluralised collapsible list', () => {
    render(
      <ApiDataSetVersionSummaryList
        dataSetVersion={{
          ...testBaseVersion,
          indicators: [
            'Test indicator 1',
            'Test indicator 2',
            'Test indicator 3',
            'Test indicator 4',
            'Test indicator 5',
          ],
        }}
        id="version-details"
        publicationId="publication-id"
      />,
    );

    const indicators = within(screen.getByTestId('Indicators')).getAllByRole(
      'listitem',
    );

    expect(indicators).toHaveLength(3);
    expect(indicators[0]).toHaveTextContent('Test indicator 1');
    expect(indicators[1]).toHaveTextContent('Test indicator 2');
    expect(indicators[2]).toHaveTextContent('Test indicator 3');

    expect(
      within(screen.getByTestId('Indicators')).getByRole('button', {
        name: 'Show 2 more indicators',
      }),
    ).toBeInTheDocument();
  });

  test('renders collapsible indicators list button with hidden text', () => {
    render(
      <ApiDataSetVersionSummaryList
        dataSetVersion={{
          ...testBaseVersion,
          indicators: [
            'Test indicator 1',
            'Test indicator 2',
            'Test indicator 3',
            'Test indicator 4',
          ],
        }}
        collapsibleButtonHiddenText="for draft version"
        id="version-details"
        publicationId="publication-id"
      />,
    );

    expect(
      within(screen.getByTestId('Indicators')).getByRole('button', {
        name: 'Show 1 more indicator for draft version',
      }),
    ).toBeInTheDocument();
  });

  test('renders filters as collapsible list', () => {
    render(
      <ApiDataSetVersionSummaryList
        dataSetVersion={{
          ...testBaseVersion,
          filters: [
            'Test filter 1',
            'Test filter 2',
            'Test filter 3',
            'Test filter 4',
          ],
        }}
        id="version-details"
        publicationId="publication-id"
      />,
    );

    const filters = within(screen.getByTestId('Filters')).getAllByRole(
      'listitem',
    );

    expect(filters).toHaveLength(3);
    expect(filters[0]).toHaveTextContent('Test filter 1');
    expect(filters[1]).toHaveTextContent('Test filter 2');
    expect(filters[2]).toHaveTextContent('Test filter 3');

    expect(
      within(screen.getByTestId('Filters')).getByRole('button', {
        name: 'Show 1 more filter',
      }),
    ).toBeInTheDocument();
  });

  test('renders filters as pluralised collapsible list', () => {
    render(
      <ApiDataSetVersionSummaryList
        dataSetVersion={{
          ...testBaseVersion,
          filters: [
            'Test filter 1',
            'Test filter 2',
            'Test filter 3',
            'Test filter 4',
            'Test filter 5',
          ],
        }}
        id="version-details"
        publicationId="publication-id"
      />,
    );

    const filters = within(screen.getByTestId('Filters')).getAllByRole(
      'listitem',
    );

    expect(filters).toHaveLength(3);
    expect(filters[0]).toHaveTextContent('Test filter 1');
    expect(filters[1]).toHaveTextContent('Test filter 2');
    expect(filters[2]).toHaveTextContent('Test filter 3');

    expect(
      within(screen.getByTestId('Filters')).getByRole('button', {
        name: 'Show 2 more filters',
      }),
    ).toBeInTheDocument();
  });

  test('renders collapsible filters list button with hidden text', () => {
    render(
      <ApiDataSetVersionSummaryList
        dataSetVersion={{
          ...testBaseVersion,
          filters: [
            'Test filter 1',
            'Test filter 2',
            'Test filter 3',
            'Test filter 4',
          ],
        }}
        collapsibleButtonHiddenText="for draft version"
        id="version-details"
        publicationId="publication-id"
      />,
    );

    expect(
      within(screen.getByTestId('Filters')).getByRole('button', {
        name: 'Show 1 more filter for draft version',
      }),
    ).toBeInTheDocument();
  });

  test('renders additional actions', () => {
    render(
      <ApiDataSetVersionSummaryList
        dataSetVersion={testBaseVersion}
        id="version-details"
        publicationId="publication-id"
        actions={<button type="button">Some action</button>}
      />,
    );

    expect(
      within(screen.getByTestId('Actions')).getByRole('button', {
        name: 'Some action',
      }),
    ).toBeInTheDocument();
  });

  function render(ui: ReactNode) {
    return baseRender(<MemoryRouter>{ui}</MemoryRouter>);
  }
});
