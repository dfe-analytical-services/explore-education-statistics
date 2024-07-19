import { TestConfigContextProvider } from '@admin/contexts/ConfigContext';
import { testReleaseVersion } from '@admin/pages/release/__data__/testReleaseVersion';
import ReleaseApiDataSetLocationsMappingPage from '@admin/pages/release/data/ReleaseApiDataSetLocationsMappingPage';
import { ReleaseVersionContextProvider } from '@admin/pages/release/contexts/ReleaseContext';
import {
  releaseApiDataSetLocationsMappingRoute,
  ReleaseDataSetRouteParams,
} from '@admin/routes/releaseRoutes';
import _apiDataSetService, {
  ApiDataSet,
} from '@admin/services/apiDataSetService';
import testLocationsMapping from '@admin/pages/release/data/__data__/testLocationsMapping';
import _apiDataSetVersionService from '@admin/services/apiDataSetVersionService';
import { ReleaseVersion } from '@admin/services/releaseService';
import render from '@common-test/render';
import { screen, within } from '@testing-library/react';
import React from 'react';
import { generatePath, MemoryRouter, Route } from 'react-router-dom';

jest.mock('@admin/services/apiDataSetService');
jest.mock('@admin/services/apiDataSetVersionService');

const apiDataSetService = jest.mocked(_apiDataSetService);
const apiDataSetVersionService = jest.mocked(_apiDataSetVersionService);

describe('ReleaseApiDataSetLocationsMappingPage', () => {
  const testDataSet: ApiDataSet = {
    id: 'data-set-id',
    title: 'Data set title',
    summary: 'Data set summary',
    status: 'Published',
    draftVersion: {
      id: 'draft-version-id',
      version: '2.0',
      status: 'Mapping',
      type: 'Minor',
      totalResults: 0,
      releaseVersion: {
        id: 'release-2-id',
        title: 'Test release 2',
      },
      file: {
        id: 'draft-file-id',
        title: 'Test draft file',
      },
    },
    latestLiveVersion: {
      id: 'live-version-id',
      version: '1.0',
      type: 'Minor',
      status: 'Published',
      totalResults: 10_000,
      published: '2024-03-01T09:30:00+00:00',
      releaseVersion: {
        id: 'release-1-id',
        title: 'Test release 1',
      },
      file: {
        id: 'live-file-id',
        title: 'Test live file',
      },
      filters: ['Test live filter'],
      geographicLevels: ['National', 'Local authority'],
      indicators: ['Test live indicator'],
      timePeriods: {
        start: '2018',
        end: '2023',
      },
    },
    previousReleaseIds: [],
  };

  test('renders the mappings tables correctly', async () => {
    apiDataSetService.getDataSet.mockResolvedValue(testDataSet);
    apiDataSetVersionService.getLocationsMapping.mockResolvedValue(
      testLocationsMapping,
    );

    renderPage();

    expect(await screen.findByText('Data set title')).toBeInTheDocument();
    expect(screen.getByText('Map locations')).toBeInTheDocument();

    expect(
      screen.getByRole('heading', {
        name: 'Locations not found in the new data set',
      }),
    ).toBeInTheDocument();
    expect(
      within(screen.getByTestId('unmapped-localAuthority')).getAllByRole('row'),
    ).toHaveLength(4);
    expect(
      within(screen.getByTestId('unmapped-region')).getAllByRole('row'),
    ).toHaveLength(3);

    expect(
      screen.getByRole('heading', {
        name: 'New locations (3) No action required',
      }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('heading', {
        name: 'Local Authorities (2) Show',
      }),
    ).toBeInTheDocument();
    expect(
      within(screen.getByTestId('new-localAuthority')).getAllByRole('row'),
    ).toHaveLength(3);
    expect(
      screen.getByRole('heading', {
        name: 'Regions (1) Show',
      }),
    ).toBeInTheDocument();
    expect(
      within(screen.getByTestId('new-region')).getAllByRole('row'),
    ).toHaveLength(2);

    expect(
      screen.getByRole('heading', {
        name: 'Auto mapped locations (12) No action required',
      }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('heading', {
        name: 'View and search auto mapped locations Show',
      }),
    ).toBeInTheDocument();
    // Only the first 10 are shown
    expect(
      within(screen.getByTestId('auto-mapped')).getAllByRole('row'),
    ).toHaveLength(11);
  });

  test('renders the navigation correctly', async () => {
    apiDataSetService.getDataSet.mockResolvedValue(testDataSet);
    apiDataSetVersionService.getLocationsMapping.mockResolvedValue(
      testLocationsMapping,
    );

    renderPage();

    expect(await screen.findByText('Data set title')).toBeInTheDocument();

    const nav = within(
      screen.getByRole('navigation', {
        name: 'On this page',
      }),
    );
    const navItems = nav.getAllByRole('listitem');
    expect(navItems).toHaveLength(7);

    expect(
      within(navItems[0]).getByRole('link', {
        name: 'Locations not found in the new data set',
      }),
    ).toHaveAttribute('href', '#unmapped-locations');
    const unmappedSubItems = within(navItems[0]).getAllByRole('listitem');
    expect(unmappedSubItems).toHaveLength(2);
    expect(
      within(unmappedSubItems[0]).getByRole('link', {
        name: 'Local Authorities',
      }),
    ).toHaveAttribute('href', '#unmapped-localAuthority');
    expect(
      within(unmappedSubItems[1]).getByRole('link', {
        name: 'Regions',
      }),
    ).toHaveAttribute('href', '#unmapped-region');

    expect(
      within(navItems[3]).getByRole('link', {
        name: 'New locations',
      }),
    ).toHaveAttribute('href', '#new-locations');
    const newSubItems = within(navItems[3]).getAllByRole('listitem');
    expect(newSubItems).toHaveLength(2);
    expect(
      within(newSubItems[0]).getByRole('link', {
        name: 'Local Authorities',
      }),
    ).toHaveAttribute('href', '#new-localAuthority');
    expect(
      within(newSubItems[1]).getByRole('link', {
        name: 'Regions',
      }),
    ).toHaveAttribute('href', '#new-region');

    expect(
      within(navItems[6]).getByRole('link', {
        name: 'Auto mapped locations',
      }),
    ).toHaveAttribute('href', '#auto-mapped-locations');
  });

  test('renders the error summary if there are unmapped locations', async () => {
    apiDataSetService.getDataSet.mockResolvedValue(testDataSet);
    apiDataSetVersionService.getLocationsMapping.mockResolvedValue(
      testLocationsMapping,
    );

    renderPage();

    expect(await screen.findByText('Data set title')).toBeInTheDocument();

    expect(
      screen.getByRole('heading', {
        name: 'Unmapped locations',
      }),
    ).toBeInTheDocument();

    const errorSummary = within(screen.getByTestId('errorSummary'));
    expect(
      errorSummary.getByRole('link', {
        name: 'There is 1 unmapped local authority',
      }),
    ).toHaveAttribute('href', '#unmapped-localAuthority');
  });

  test('does not render the error summary if there are no unmapped locations', async () => {
    apiDataSetService.getDataSet.mockResolvedValue(testDataSet);
    apiDataSetVersionService.getLocationsMapping.mockResolvedValue({
      levels: {
        localAuthority: {
          candidates: {
            Location1Key: {
              label: 'Location 1',
              code: 'location-1-code',
            },
          },
          mappings: {
            Location1Key: {
              candidateKey: 'Location1Key',
              type: 'AutoMapped',
              source: {
                label: 'Location 1',
                code: 'location-1-code',
              },
            },
          },
        },
      },
    });

    renderPage();

    expect(await screen.findByText('Data set title')).toBeInTheDocument();

    expect(
      screen.queryByRole('heading', {
        name: 'Unmapped locations',
      }),
    ).not.toBeInTheDocument();

    expect(screen.queryByTestId('errorSummary')).not.toBeInTheDocument();
  });

  test('renders a message if there are no unmapped or manually mapped locations', async () => {
    apiDataSetService.getDataSet.mockResolvedValue(testDataSet);
    apiDataSetVersionService.getLocationsMapping.mockResolvedValue({
      levels: {
        localAuthority: {
          candidates: {
            Location1Key: {
              label: 'Location 1',
              code: 'location-1-code',
            },
          },
          mappings: {
            Location1Key: {
              candidateKey: 'Location1Key',
              type: 'AutoMapped',
              source: {
                label: 'Location 1',
                code: 'location-1-code',
              },
            },
          },
        },
        region: {
          candidates: {
            Location1Key: {
              label: 'Location 2',
              code: 'location-2-code',
            },
          },
          mappings: {},
        },
      },
    });

    renderPage();

    expect(await screen.findByText('Data set title')).toBeInTheDocument();

    expect(
      screen.getByRole('heading', {
        name: 'Locations not found in the new data set',
      }),
    ).toBeInTheDocument();

    expect(
      screen.getByText('No locations not found in the new data set.'),
    ).toBeInTheDocument();

    expect(
      screen.queryByTestId('unmapped-localAuthority'),
    ).not.toBeInTheDocument();

    expect(screen.queryByTestId('unmapped-region')).not.toBeInTheDocument();
  });

  test('renders a message if there are no new locations', async () => {
    apiDataSetService.getDataSet.mockResolvedValue(testDataSet);
    apiDataSetVersionService.getLocationsMapping.mockResolvedValue({
      levels: {
        localAuthority: {
          candidates: {
            Location1Key: {
              label: 'Location 1',
              code: 'location-1-code',
            },
          },
          mappings: {
            Location1Key: {
              candidateKey: 'Location1Key',
              type: 'AutoMapped',
              source: {
                label: 'Location 1',
                code: 'location-1-code',
              },
            },
          },
        },
        region: {
          candidates: {
            Location2Key: {
              label: 'Location 2',
              code: 'location-2-code',
            },
          },
          mappings: {
            Location2Key: {
              candidateKey: 'Location2Key',
              type: 'AutoMapped',
              source: {
                label: 'Location 2',
                code: 'location-2-code',
              },
            },
          },
        },
      },
    });

    renderPage();

    expect(await screen.findByText('Data set title')).toBeInTheDocument();

    expect(
      screen.getByRole('heading', {
        name: 'New locations (0) No action required',
      }),
    ).toBeInTheDocument();

    expect(screen.getByText('No new locations.')).toBeInTheDocument();

    expect(screen.queryByTestId('new-localAuthority')).not.toBeInTheDocument();

    expect(screen.queryByTestId('new-region')).not.toBeInTheDocument();
  });

  test('renders a message if there are no auto mapped locations', async () => {
    apiDataSetService.getDataSet.mockResolvedValue(testDataSet);
    apiDataSetVersionService.getLocationsMapping.mockResolvedValue({
      levels: {
        localAuthority: {
          candidates: {
            Location1Key: {
              label: 'Location 1',
              code: 'location-1-code',
            },
          },
          mappings: {
            Location1Key: {
              candidateKey: 'Location1Key',
              type: 'ManualMapped',
              source: {
                label: 'Location 1',
                code: 'location-1-code',
              },
            },
          },
        },
        region: {
          candidates: {
            Location1Key: {
              label: 'Location 2',
              code: 'location-2-code',
            },
          },
          mappings: {},
        },
      },
    });

    renderPage();

    expect(await screen.findByText('Data set title')).toBeInTheDocument();

    expect(
      screen.getByRole('heading', {
        name: 'Auto mapped locations (0) No action required',
      }),
    ).toBeInTheDocument();

    expect(screen.getByText('No auto mapped locations.')).toBeInTheDocument();

    expect(screen.queryByTestId('auto-mapped')).not.toBeInTheDocument();
  });

  function renderPage(options?: {
    releaseVersion?: ReleaseVersion;
    dataSetId?: string;
  }) {
    const { releaseVersion = testReleaseVersion, dataSetId = 'data-set-id' } =
      options ?? {};

    render(
      <TestConfigContextProvider>
        <ReleaseVersionContextProvider releaseVersion={releaseVersion}>
          <MemoryRouter
            initialEntries={[
              generatePath<ReleaseDataSetRouteParams>(
                releaseApiDataSetLocationsMappingRoute.path,
                {
                  publicationId: releaseVersion.publicationId,
                  releaseVersionId: releaseVersion.id,
                  dataSetId,
                },
              ),
            ]}
          >
            <Route
              component={ReleaseApiDataSetLocationsMappingPage}
              path={releaseApiDataSetLocationsMappingRoute.path}
            />
          </MemoryRouter>
        </ReleaseVersionContextProvider>
      </TestConfigContextProvider>,
    );
  }
});
