import { TestConfigContextProvider } from '@admin/contexts/ConfigContext';
import DraftApiDataSetsTable, {
  DraftApiDataSetSummary,
} from '@admin/pages/release/data/components/DraftApiDataSetsTable';
import baseRender from '@common-test/render';
import { screen, within } from '@testing-library/react';
import { ReactNode } from 'react';
import { MemoryRouter } from 'react-router-dom';

describe('DraftApiDataSetsTable', () => {
  const testDataSets: DraftApiDataSetSummary[] = [
    {
      id: 'data-set-4',
      title: 'Data set 4 title',
      summary: 'Data set 4 summary',
      status: 'Published',
      draftVersion: {
        id: 'version-6',
        version: '1.0',
        status: 'Draft',
        type: 'Major',
      },
      previousReleaseIds: [],
    },
    {
      id: 'data-set-3',
      title: 'Data set 3 title',
      summary: 'Data set 3 summary',
      status: 'Published',
      draftVersion: {
        id: 'version-5',
        version: '1.0',
        status: 'Processing',
        type: 'Major',
      },
      previousReleaseIds: [],
    },
    {
      id: 'data-set-2',
      title: 'Data set 2 title',
      summary: 'Data set 2 summary',
      status: 'Published',
      draftVersion: {
        id: 'version-4',
        version: '2.0',
        status: 'Draft',
        type: 'Major',
      },
      latestLiveVersion: {
        file: {
          id: 'file-id',
          title: 'file-title',
        },
        published: '2024-02-01T09:30:00+00:00',
        id: 'version-3',
        version: '1.0',
        releaseVersion: {
          id: 'release-version-id',
          title: 'Release Version',
        },
        status: 'Published',
        type: 'Major',
      },
      previousReleaseIds: [],
    },
    {
      id: 'data-set-1',
      title: 'Data set 1 title',
      summary: 'Data set 1 summary',
      status: 'Published',
      draftVersion: {
        id: 'version-2',
        version: '1.1',
        status: 'Mapping',
        type: 'Minor',
      },
      latestLiveVersion: {
        file: {
          id: 'file-id',
          title: 'file-title',
        },
        published: '2024-02-01T09:30:00+00:00',
        id: 'version-1',
        version: '1.0',
        releaseVersion: {
          id: 'release-version-id',
          title: 'Release Version',
        },
        status: 'Published',
        type: 'Major',
      },
      previousReleaseIds: [],
    },
    {
      id: 'data-set-6',
      title: 'Data set 6 title',
      summary: 'Data set 6 summary',
      status: 'Published',
      draftVersion: {
        id: 'version-8',
        version: '1.0',
        status: 'Cancelled',
        type: 'Major',
      },
      previousReleaseIds: [],
    },
    {
      id: 'data-set-5',
      title: 'Data set 5 title',
      summary: 'Data set 5 summary',
      status: 'Published',
      draftVersion: {
        id: 'version-7',
        version: '1.0',
        status: 'Failed',
        type: 'Major',
      },
      previousReleaseIds: [],
    },
  ];

  const patchDataSet: DraftApiDataSetSummary[] = [
    {
      id: 'data-set-7',
      title: 'Data set 7 title',
      summary: 'Data set 7 summary',
      status: 'Published',
      draftVersion: {
        id: 'version-8',
        version: '2.0.1',
        status: 'Mapping',
        type: 'Patch',
      },
      previousReleaseIds: [],
    },
  ];

  const finalisingDataSet: DraftApiDataSetSummary[] = [
    {
      id: 'data-set-7',
      title: 'Data set 7 title',
      summary: 'Data set 7 summary',
      status: 'Published',
      draftVersion: {
        id: 'version-8',
        version: '2.0.1',
        status: 'Finalising',
        type: 'Patch',
      },
      previousReleaseIds: [],
    },
  ];

  const draftDataSet: DraftApiDataSetSummary[] = [
    {
      id: 'data-set-7',
      title: 'Data set 7 title',
      summary: 'Data set 7 summary',
      status: 'Published',
      draftVersion: {
        id: 'version-8',
        version: '2.0.1',
        status: 'Draft',
        type: 'Patch',
      },
      previousReleaseIds: [],
    },
  ];

  test('renders draft data set rows correctly', () => {
    render(
      <DraftApiDataSetsTable
        canUpdateRelease
        dataSets={testDataSets}
        publicationId="publication-1"
        releaseVersionId="release-1"
      />,
    );

    const baseDataSetUrl =
      '/publication/publication-1/release/release-1/api-data-sets';

    const rows = within(screen.getByRole('table')).getAllByRole('row');

    expect(rows).toHaveLength(7);

    // Row 1

    const row1Cells = within(rows[1]).getAllByRole('cell');

    expect(row1Cells[0]).toHaveTextContent('v1.1');
    expect(row1Cells[1]).toHaveTextContent('v1.0');
    expect(row1Cells[2]).toHaveTextContent('Data set 1 title');
    expect(row1Cells[3]).toHaveTextContent('Action required');

    expect(
      within(row1Cells[4]).getByRole('link', {
        name: 'View details / edit draft for Data set 1 title',
      }),
    ).toHaveAttribute('href', `${baseDataSetUrl}/data-set-1`);
    expect(
      within(row1Cells[4]).getByRole('button', {
        name: 'Remove draft for Data set 1 title',
      }),
    ).toBeInTheDocument();

    // Row 2

    const row2Cells = within(rows[2]).getAllByRole('cell');

    expect(row2Cells[0]).toHaveTextContent('v2.0');
    expect(row2Cells[1]).toHaveTextContent('v1.0');
    expect(row2Cells[2]).toHaveTextContent('Data set 2 title');
    expect(row2Cells[3]).toHaveTextContent('Ready');

    expect(
      within(row2Cells[4]).getByRole('link', {
        name: 'View details / edit draft for Data set 2 title',
      }),
    ).toHaveAttribute('href', `${baseDataSetUrl}/data-set-2`);

    expect(
      within(row2Cells[4]).getByRole('button', {
        name: 'Remove draft for Data set 2 title',
      }),
    ).toBeInTheDocument();

    // Row 3

    const row3Cells = within(rows[3]).getAllByRole('cell');

    expect(row3Cells[0]).toHaveTextContent('v1.0');
    expect(row3Cells[1]).toHaveTextContent('N/A');
    expect(row3Cells[2]).toHaveTextContent('Data set 3 title');
    expect(row3Cells[3]).toHaveTextContent('Processing');

    expect(
      within(row3Cells[4]).getByRole('link', {
        name: 'View details for Data set 3 title',
      }),
    ).toHaveAttribute('href', `${baseDataSetUrl}/data-set-3`);
    expect(
      within(row3Cells[4]).queryByRole('button', {
        name: /Remove draft/,
      }),
    ).not.toBeInTheDocument();

    // Row 4

    const row4Cells = within(rows[4]).getAllByRole('cell');

    expect(row4Cells[0]).toHaveTextContent('v1.0');
    expect(row4Cells[1]).toHaveTextContent('N/A');
    expect(row4Cells[2]).toHaveTextContent('Data set 4 title');
    expect(row4Cells[3]).toHaveTextContent('Ready');

    expect(
      within(row4Cells[4]).getByRole('link', {
        name: 'View details for Data set 4 title',
      }),
    ).toHaveAttribute('href', `${baseDataSetUrl}/data-set-4`);

    expect(
      within(row4Cells[4]).getByRole('button', {
        name: 'Remove draft for Data set 4 title',
      }),
    ).toBeInTheDocument();

    // Row 5

    const row5Cells = within(rows[5]).getAllByRole('cell');

    expect(row5Cells[0]).toHaveTextContent('v1.0');
    expect(row5Cells[1]).toHaveTextContent('N/A');
    expect(row5Cells[2]).toHaveTextContent('Data set 5 title');
    expect(row5Cells[3]).toHaveTextContent('Failed');

    expect(
      within(row5Cells[4]).getByRole('link', {
        name: 'View details for Data set 5 title',
      }),
    ).toHaveAttribute('href', `${baseDataSetUrl}/data-set-5`);

    expect(
      within(row5Cells[4]).getByRole('button', {
        name: 'Remove draft for Data set 5 title',
      }),
    ).toBeInTheDocument();

    // Row 6

    const row6Cells = within(rows[6]).getAllByRole('cell');

    expect(row6Cells[0]).toHaveTextContent('v1.0');
    expect(row6Cells[1]).toHaveTextContent('N/A');
    expect(row6Cells[2]).toHaveTextContent('Data set 6 title');
    expect(row6Cells[3]).toHaveTextContent('Cancelled');

    expect(
      within(row6Cells[4]).getByRole('link', {
        name: 'View details for Data set 6 title',
      }),
    ).toHaveAttribute('href', `${baseDataSetUrl}/data-set-6`);

    expect(
      within(row6Cells[4]).getByRole('button', {
        name: 'Remove draft for Data set 6 title',
      }),
    ).toBeInTheDocument();
  });

  test('renders draft data set rows correctly when cannot update the release', () => {
    render(
      <DraftApiDataSetsTable
        canUpdateRelease={false}
        dataSets={testDataSets}
        publicationId="publication-1"
        releaseVersionId="release-1"
      />,
    );

    const baseDataSetUrl =
      '/publication/publication-1/release/release-1/api-data-sets';

    const rows = within(screen.getByRole('table')).getAllByRole('row');

    expect(rows).toHaveLength(7);

    // Row 1

    const row1Cells = within(rows[1]).getAllByRole('cell');
    expect(
      within(row1Cells[4]).getByRole('link', {
        name: 'View details for Data set 1 title',
      }),
    ).toHaveAttribute('href', `${baseDataSetUrl}/data-set-1`);
    expect(
      within(row1Cells[4]).queryByRole('button', {
        name: /Remove draft/,
      }),
    ).not.toBeInTheDocument();

    // Row 2

    const row2Cells = within(rows[2]).getAllByRole('cell');
    expect(
      within(row2Cells[4]).getByRole('link', {
        name: 'View details for Data set 2 title',
      }),
    ).toHaveAttribute('href', `${baseDataSetUrl}/data-set-2`);
    expect(
      within(row2Cells[4]).queryByRole('button', {
        name: /Remove draft/,
      }),
    ).not.toBeInTheDocument();

    // Row 3

    const row3Cells = within(rows[3]).getAllByRole('cell');
    expect(
      within(row3Cells[4]).getByRole('link', {
        name: 'View details for Data set 3 title',
      }),
    ).toHaveAttribute('href', `${baseDataSetUrl}/data-set-3`);
    expect(
      within(row3Cells[4]).queryByRole('button', {
        name: /Remove draft/,
      }),
    ).not.toBeInTheDocument();

    // Row 4

    const row4Cells = within(rows[4]).getAllByRole('cell');
    expect(
      within(row4Cells[4]).getByRole('link', {
        name: 'View details for Data set 4 title',
      }),
    ).toHaveAttribute('href', `${baseDataSetUrl}/data-set-4`);
    expect(
      within(row4Cells[4]).queryByRole('button', {
        name: /Remove draft/,
      }),
    ).not.toBeInTheDocument();

    // Row 5

    const row5Cells = within(rows[5]).getAllByRole('cell');
    expect(
      within(row5Cells[4]).getByRole('link', {
        name: 'View details for Data set 5 title',
      }),
    ).toHaveAttribute('href', `${baseDataSetUrl}/data-set-5`);
    expect(
      within(row5Cells[4]).queryByRole('button', {
        name: /Remove draft/,
      }),
    ).not.toBeInTheDocument();

    // Row 6

    const row6Cells = within(rows[6]).getAllByRole('cell');
    expect(
      within(row6Cells[4]).getByRole('link', {
        name: 'View details for Data set 6 title',
      }),
    ).toHaveAttribute('href', `${baseDataSetUrl}/data-set-6`);
    expect(
      within(row5Cells[4]).queryByRole('button', {
        name: /Remove draft/,
      }),
    ).not.toBeInTheDocument();
  });

  test('renders message when no data sets', () => {
    render(
      <DraftApiDataSetsTable
        canUpdateRelease
        dataSets={[]}
        publicationId="publication-1"
        releaseVersionId="release-1"
      />,
    );

    expect(screen.getByText(/No draft API data sets/)).toBeInTheDocument();
    expect(screen.queryByRole('table')).not.toBeInTheDocument();
  });

  test('it renders Remove Draft button if working with a PATCH draft version but the feature flag is off', () => {
    render(
      <DraftApiDataSetsTable
        canUpdateRelease
        dataSets={patchDataSet}
        publicationId="publication-1"
        releaseVersionId="release-1"
      />,
    );

    const rows = within(screen.getByRole('table')).getAllByRole('row');

    expect(rows).toHaveLength(2);

    const row1Cells = within(rows[1]).getAllByRole('cell');

    expect(row1Cells[0]).toHaveTextContent('v2.0.1');
    expect(row1Cells[1]).toHaveTextContent('Data set 7 title');
    expect(row1Cells[2]).toHaveTextContent('Action required');
    expect(row1Cells[3]).toHaveTextContent('Remove draft for Data set 7 title');
  });

  test('it doesnt render Remove Draft button if working with a PATCH draft version but the feature flag is on', () => {
    render(
      <DraftApiDataSetsTable
        canUpdateRelease
        dataSets={patchDataSet}
        publicationId="publication-1"
        releaseVersionId="release-1"
      />,
      true,
    );

    const rows = within(screen.getByRole('table')).getAllByRole('row');

    expect(rows).toHaveLength(2);

    const row1Cells = within(rows[1]).getAllByRole('cell');

    expect(row1Cells[0]).toHaveTextContent('v2.0.1');
    expect(row1Cells[1]).toHaveTextContent('Data set 7 title');
    expect(row1Cells[2]).toHaveTextContent('Action required');
    expect(row1Cells[3]).not.toHaveTextContent(
      'Remove draft for Data set 7 title',
    );
  });

  test('it doesnt render Remove Draft button if working with a finalising draft version', () => {
    render(
      <DraftApiDataSetsTable
        canUpdateRelease
        dataSets={finalisingDataSet}
        publicationId="publication-1"
        releaseVersionId="release-1"
      />,
    );

    const rows = within(screen.getByRole('table')).getAllByRole('row');

    expect(rows).toHaveLength(2);

    const row1Cells = within(rows[1]).getAllByRole('cell');

    expect(row1Cells[0]).toHaveTextContent('v2.0.1');
    expect(row1Cells[1]).toHaveTextContent('Data set 7 title');
    expect(row1Cells[2]).toHaveTextContent('Finalising');
    expect(row1Cells[3]).not.toHaveTextContent(
      'Remove draft for Data set 7 title',
    );
  });

  test('it renders Remove Draft button if working with a Ready draft version', () => {
    render(
      <DraftApiDataSetsTable
        canUpdateRelease
        dataSets={draftDataSet}
        publicationId="publication-1"
        releaseVersionId="release-1"
      />,
    );

    const rows = within(screen.getByRole('table')).getAllByRole('row');

    expect(rows).toHaveLength(2);

    const row1Cells = within(rows[1]).getAllByRole('cell');

    expect(row1Cells[0]).toHaveTextContent('v2.0.1');
    expect(row1Cells[1]).toHaveTextContent('Data set 7 title');
    expect(row1Cells[2]).toHaveTextContent('Ready');
    expect(row1Cells[3]).toHaveTextContent('Remove draft for Data set 7 title');
  });

  function render(
    element: ReactNode,
    enableReplacementOfPublicApiDataSets?: boolean,
  ) {
    const defaultTestConfig = {
      appInsightsKey: '',
      publicAppUrl: 'http://localhost',
      publicApiUrl: 'http://public-api',
      publicApiDocsUrl: 'http://public-api-docs',
      permittedEmbedUrlDomains: [
        'https://department-for-education.shinyapps.io',
      ],
      oidc: {
        clientId: '',
        authority: '',
        knownAuthorities: [''],
        adminApiScope: '',
        authorityMetadata: {
          authorizationEndpoint: '',
          tokenEndpoint: '',
          issuer: '',
          userInfoEndpoint: '',
          endSessionEndpoint: '',
        },
      },
    };

    return baseRender(
      <MemoryRouter>
        <TestConfigContextProvider
          config={{
            ...defaultTestConfig,
            enableReplacementOfPublicApiDataSets:
              enableReplacementOfPublicApiDataSets ?? false,
          }}
        >
          {element}
        </TestConfigContextProvider>
      </MemoryRouter>,
    );
  }
});
