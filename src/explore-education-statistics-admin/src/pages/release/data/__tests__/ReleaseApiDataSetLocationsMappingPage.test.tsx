import { TestConfigContextProvider } from '@admin/contexts/ConfigContext';
import { testRelease } from '@admin/pages/release/__data__/testRelease';
import ReleaseApiDataSetLocationsMappingPage from '@admin/pages/release/data/ReleaseApiDataSetLocationsMappingPage';
import { ReleaseContextProvider } from '@admin/pages/release/contexts/ReleaseContext';
import {
  releaseApiDataSetLocationsMappingRoute,
  ReleaseDataSetRouteParams,
} from '@admin/routes/releaseRoutes';
import _apiDataSetService, {
  ApiDataSet,
} from '@admin/services/apiDataSetService';
import testLocationsMapping from '@admin/pages/release/data/__data__/testLocationsMapping';
import _apiDataSetVersionService from '@admin/services/apiDataSetVersionService';
import { Release } from '@admin/services/releaseService';
import render from '@common-test/render';
import { screen, waitFor, within } from '@testing-library/react';
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

    // mappable
    expect(
      screen.getByRole('heading', {
        name: 'Locations not found in the new data set',
      }),
    ).toBeInTheDocument();

    // mappable LAs
    const mappableLocalAuthoritiesTable = screen.getByRole('table', {
      name: 'Local Authorities 1 unmapped location 2 mapped locations',
    });
    expect(mappableLocalAuthoritiesTable).toBe(
      screen.getByTestId('mappable-table-localAuthority'),
    );
    expect(
      within(mappableLocalAuthoritiesTable).getAllByRole('row'),
    ).toHaveLength(4);

    // mappable regions
    const mappableRegionsTable = screen.getByRole('table', {
      name: 'Regions 3 mapped locations',
    });
    expect(mappableRegionsTable).toBe(
      screen.getByTestId('mappable-table-region'),
    );
    expect(within(mappableRegionsTable).getAllByRole('row')).toHaveLength(4);

    // new locations
    expect(
      screen.getByRole('heading', {
        name: 'New locations (4) No action required',
      }),
    ).toBeInTheDocument();

    const newLocationsAccordion = within(
      screen.getByTestId('new-locations-accordion'),
    );

    // new LAs
    expect(
      newLocationsAccordion.getByRole('heading', {
        name: 'Local Authorities (2)',
      }),
    ).toBeInTheDocument();
    expect(
      within(
        newLocationsAccordion.getByTestId('new-locations-table-localAuthority'),
      ).getAllByRole('row'),
    ).toHaveLength(3);

    // new regions
    expect(
      newLocationsAccordion.getByRole('heading', {
        name: 'Regions (2)',
      }),
    ).toBeInTheDocument();
    expect(
      within(
        newLocationsAccordion.getByTestId('new-locations-table-region'),
      ).getAllByRole('row'),
    ).toHaveLength(3);

    // auto mapped
    expect(
      screen.getByRole('heading', {
        name: 'Auto mapped locations (12) No action required',
      }),
    ).toBeInTheDocument();

    const autoMappedAccordion = within(
      screen.getByTestId('auto-mapped-accordion'),
    );

    // auto mapped LA
    expect(
      autoMappedAccordion.getByRole('heading', {
        name: 'Local Authorities (2)',
      }),
    ).toBeInTheDocument();
    expect(
      within(
        autoMappedAccordion.getByTestId('auto-mapped-table-localAuthority'),
      ).getAllByRole('row'),
    ).toHaveLength(3);

    // auto mapped regions
    expect(
      autoMappedAccordion.getByRole('heading', {
        name: 'Regions (1)',
      }),
    ).toBeInTheDocument();
    expect(
      within(
        autoMappedAccordion.getByTestId('auto-mapped-table-region'),
      ).getAllByRole('row'),
    ).toHaveLength(2);

    // auto mapped englishDevolvedArea
    expect(
      autoMappedAccordion.getByRole('heading', {
        name: 'English Devolved Areas (9)',
      }),
    ).toBeInTheDocument();
    expect(
      within(
        autoMappedAccordion.getByTestId(
          'auto-mapped-table-englishDevolvedArea',
        ),
      ).getAllByRole('row'),
    ).toHaveLength(10);
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
    expect(navItems).toHaveLength(10);

    expect(
      within(navItems[0]).getByRole('link', {
        name: 'Locations not found in the new data set',
      }),
    ).toHaveAttribute('href', '#mappable-locations');
    const mappableSubItems = within(navItems[0]).getAllByRole('listitem');
    expect(mappableSubItems).toHaveLength(2);
    expect(
      within(mappableSubItems[0]).getByRole('link', {
        name: 'Local Authorities',
      }),
    ).toHaveAttribute('href', '#mappable-localAuthority');
    expect(
      within(mappableSubItems[1]).getByRole('link', {
        name: 'Regions',
      }),
    ).toHaveAttribute('href', '#mappable-region');

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
    ).toHaveAttribute('href', '#new-locations-localAuthority');
    expect(
      within(newSubItems[1]).getByRole('link', {
        name: 'Regions',
      }),
    ).toHaveAttribute('href', '#new-locations-region');

    expect(
      within(navItems[6]).getByRole('link', {
        name: 'Auto mapped locations',
      }),
    ).toHaveAttribute('href', '#auto-mapped-locations');
    const autoMappedSubItems = within(navItems[6]).getAllByRole('listitem');
    expect(autoMappedSubItems).toHaveLength(3);
    expect(
      within(autoMappedSubItems[0]).getByRole('link', {
        name: 'Local Authorities',
      }),
    ).toHaveAttribute('href', '#auto-mapped-localAuthority');
    expect(
      within(autoMappedSubItems[1]).getByRole('link', {
        name: 'Regions',
      }),
    ).toHaveAttribute('href', '#auto-mapped-region');
    expect(
      within(autoMappedSubItems[2]).getByRole('link', {
        name: 'English Devolved Areas',
      }),
    ).toHaveAttribute('href', '#auto-mapped-englishDevolvedArea');
  });

  test('renders the notification banner if there are mappable locations', async () => {
    apiDataSetService.getDataSet.mockResolvedValue(testDataSet);
    apiDataSetVersionService.getLocationsMapping.mockResolvedValue(
      testLocationsMapping,
    );

    renderPage();

    expect(await screen.findByText('Data set title')).toBeInTheDocument();

    expect(
      screen.getByRole('heading', {
        name: 'Action required',
      }),
    ).toBeInTheDocument();

    const banner = within(screen.getByTestId('notificationBanner'));
    expect(
      banner.getByRole('link', {
        name: 'There is 1 unmapped local authority',
      }),
    ).toHaveAttribute('href', '#mappable-localAuthority');
  });

  test('does not render the notification banner if there are no unmapped locations', async () => {
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
              publicId: 'location-1-public-id',
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
        name: 'Action required',
      }),
    ).not.toBeInTheDocument();

    expect(screen.queryByTestId('notificationBanner')).not.toBeInTheDocument();
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
              publicId: 'location-1-public-id',
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
      screen.queryByTestId('mappable-localAuthority'),
    ).not.toBeInTheDocument();

    expect(screen.queryByTestId('mappable-region')).not.toBeInTheDocument();
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
              publicId: 'location-1-public-id',
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
              publicId: 'location-2-public-id',
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

    expect(
      screen.queryByTestId('new-locations-localAuthority'),
    ).not.toBeInTheDocument();

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
              publicId: 'location-1-public-id',
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

  describe('updating mappings', () => {
    beforeEach(() => {
      jest.useFakeTimers({ advanceTimers: true });
    });
    afterEach(() => {
      jest.useRealTimers();
    });

    test('changing an unmapped location to mapped', async () => {
      apiDataSetService.getDataSet.mockResolvedValue(testDataSet);
      apiDataSetVersionService.getLocationsMapping.mockResolvedValue(
        testLocationsMapping,
      );

      const { user } = renderPage();

      expect(await screen.findByText('Data set title')).toBeInTheDocument();

      expect(
        screen.getByRole('link', {
          name: 'There is 1 unmapped local authority',
        }),
      ).toHaveAttribute('href', '#mappable-localAuthority');

      // unmapped table
      const unmappedTable = screen.getByRole('table', {
        name: 'Local Authorities 1 unmapped location 2 mapped locations',
      });
      const unmappedLocation = within(unmappedTable).getAllByRole('row')[1];
      const unmappedLocationCells =
        within(unmappedLocation).getAllByRole('cell');
      expect(within(unmappedLocationCells[0]).getByText('Location 2'));
      expect(within(unmappedLocationCells[1]).getByText('Unmapped'));
      expect(within(unmappedLocationCells[2]).getByText('N/A'));

      // new locations table
      expect(
        screen.getByRole('heading', {
          name: 'New locations (4) No action required',
        }),
      ).toBeInTheDocument();
      const newLocationsAccordion = within(
        screen.getByTestId('new-locations-accordion'),
      );
      expect(
        newLocationsAccordion.getByRole('heading', {
          name: /Local Authorities \(2\)/,
        }),
      ).toBeInTheDocument();
      const newLocationsTable = screen.getByTestId(
        'new-locations-table-localAuthority',
      );
      const newLocationRows = within(newLocationsTable).getAllByRole('row');
      expect(newLocationRows).toHaveLength(3);
      const newLocationCells = within(newLocationRows[1]).getAllByRole('cell');
      expect(within(newLocationCells[0]).getByText('Not applicable'));
      expect(within(newLocationCells[1]).getByText('Location 6'));

      await user.click(
        within(unmappedLocation).getByRole('button', {
          name: 'Edit mapping for Location 2',
        }),
      );

      expect(
        await screen.findByText('Map existing location'),
      ).toBeInTheDocument();

      const modal = within(screen.getByRole('dialog'));

      await user.click(modal.getByLabelText('Location 6'));

      expect(
        apiDataSetVersionService.updateLocationsMapping,
      ).not.toHaveBeenCalled();

      await user.click(
        modal.getByRole('button', { name: 'Update location mapping' }),
      );

      await waitFor(() =>
        expect(
          screen.queryByText('Map existing location'),
        ).not.toBeInTheDocument(),
      );

      jest.runAllTimers();

      await waitFor(() => {
        expect(
          apiDataSetVersionService.updateLocationsMapping,
        ).toHaveBeenCalledWith('draft-version-id', {
          updates: [
            {
              candidateKey: 'Location6Key',
              level: 'localAuthority',
              sourceKey: 'Location2Key',
              type: 'ManualMapped',
            },
          ],
        });

        expect(
          screen.queryByText('There is 1 unmapped local authority'),
        ).not.toBeInTheDocument();
      });

      // Update location in unmapped table
      expect(within(unmappedLocationCells[0]).getByText('Location 2'));
      expect(within(unmappedLocationCells[1]).getByText('Location 6'));
      expect(within(unmappedLocationCells[2]).getByText('Minor'));

      // Remove from new locations table
      expect(
        screen.getByRole('heading', {
          name: 'New locations (3) No action required',
        }),
      ).toBeInTheDocument();
      expect(
        screen.getByRole('heading', {
          name: /Local Authorities \(1\)/,
        }),
      ).toBeInTheDocument();
      expect(within(newLocationsTable).getAllByRole('row')).toHaveLength(2);
      expect(
        within(newLocationsTable).queryByText('Location 6'),
      ).not.toBeInTheDocument();
    });

    test('changing an unmapped location to "no mapping available" via the edit modal', async () => {
      apiDataSetService.getDataSet.mockResolvedValue(testDataSet);
      apiDataSetVersionService.getLocationsMapping.mockResolvedValue(
        testLocationsMapping,
      );

      const { user } = renderPage();

      expect(await screen.findByText('Data set title')).toBeInTheDocument();

      expect(
        screen.getByRole('link', {
          name: 'There is 1 unmapped local authority',
        }),
      ).toHaveAttribute('href', '#mappable-localAuthority');

      // unmapped table
      const unmappedTable = screen.getByRole('table', {
        name: 'Local Authorities 1 unmapped location 2 mapped locations',
      });
      const unmappedLocation = within(unmappedTable).getAllByRole('row')[1];
      const unmappedLocationCells =
        within(unmappedLocation).getAllByRole('cell');
      expect(within(unmappedLocationCells[0]).getByText('Location 2'));
      expect(within(unmappedLocationCells[1]).getByText('Unmapped'));
      expect(within(unmappedLocationCells[2]).getByText('N/A'));

      await user.click(
        within(unmappedLocation).getByRole('button', {
          name: 'Edit mapping for Location 2',
        }),
      );

      expect(
        await screen.findByText('Map existing location'),
      ).toBeInTheDocument();

      const modal = within(screen.getByRole('dialog'));

      await user.click(modal.getByLabelText('No mapping available'));

      expect(
        apiDataSetVersionService.updateLocationsMapping,
      ).not.toHaveBeenCalled();

      await user.click(
        modal.getByRole('button', { name: 'Update location mapping' }),
      );

      await waitFor(() =>
        expect(
          screen.queryByText('Map existing location'),
        ).not.toBeInTheDocument(),
      );

      jest.runAllTimers();

      await waitFor(() => {
        expect(
          apiDataSetVersionService.updateLocationsMapping,
        ).toHaveBeenCalledWith('draft-version-id', {
          updates: [
            {
              level: 'localAuthority',
              sourceKey: 'Location2Key',
              type: 'ManualNone',
            },
          ],
        });

        expect(
          screen.queryByText('There is 1 unmapped local authority'),
        ).not.toBeInTheDocument();
      });

      // Update location in unmapped table
      expect(within(unmappedLocationCells[0]).getByText('Location 2'));
      expect(
        within(unmappedLocationCells[1]).getByText('No mapping available'),
      );
      expect(within(unmappedLocationCells[2]).getByText('Major'));
    });

    test('changing an unmapped location to "no mapping available" via the row action button', async () => {
      apiDataSetService.getDataSet.mockResolvedValue(testDataSet);
      apiDataSetVersionService.getLocationsMapping.mockResolvedValue(
        testLocationsMapping,
      );

      const { user } = renderPage();

      expect(await screen.findByText('Data set title')).toBeInTheDocument();

      expect(
        screen.getByRole('link', {
          name: 'There is 1 unmapped local authority',
        }),
      ).toHaveAttribute('href', '#mappable-localAuthority');

      // unmapped table
      const unmappedTable = screen.getByRole('table', {
        name: 'Local Authorities 1 unmapped location 2 mapped locations',
      });
      const unmappedLocation = within(unmappedTable).getAllByRole('row')[1];
      const unmappedLocationCells =
        within(unmappedLocation).getAllByRole('cell');
      expect(within(unmappedLocationCells[0]).getByText('Location 2'));
      expect(within(unmappedLocationCells[1]).getByText('Unmapped'));
      expect(within(unmappedLocationCells[2]).getByText('N/A'));

      expect(
        apiDataSetVersionService.updateLocationsMapping,
      ).not.toHaveBeenCalled();

      await user.click(
        within(unmappedLocation).getByRole('button', {
          name: 'No mapping for Location 2',
        }),
      );

      jest.runAllTimers();

      await waitFor(() => {
        expect(
          apiDataSetVersionService.updateLocationsMapping,
        ).toHaveBeenCalledWith('draft-version-id', {
          updates: [
            {
              level: 'localAuthority',
              sourceKey: 'Location2Key',
              type: 'ManualNone',
            },
          ],
        });

        expect(
          screen.queryByText('There is 1 unmapped local authority'),
        ).not.toBeInTheDocument();
      });

      // Update location in unmapped table
      expect(within(unmappedLocationCells[0]).getByText('Location 2'));
      expect(
        within(unmappedLocationCells[1]).getByText('No mapping available'),
      );
      expect(within(unmappedLocationCells[2]).getByText('Major'));
    });

    test('changing an auto mapped location to "no mapping available"', async () => {
      apiDataSetService.getDataSet.mockResolvedValue(testDataSet);
      apiDataSetVersionService.getLocationsMapping.mockResolvedValue(
        testLocationsMapping,
      );

      const { user } = renderPage();

      expect(await screen.findByText('Data set title')).toBeInTheDocument();

      // auto mapped table
      expect(
        screen.getByRole('heading', {
          name: 'Auto mapped locations (12) No action required',
        }),
      ).toBeInTheDocument();
      const autoMappedAccordion = within(
        screen.getByTestId('auto-mapped-accordion'),
      );
      expect(
        autoMappedAccordion.getByRole('heading', {
          name: /Local Authorities \(2\) /,
        }),
      ).toBeInTheDocument();
      const autoMappedTable = screen.getByTestId(
        'auto-mapped-table-localAuthority',
      );
      const autoMappedLocation = within(autoMappedTable).getAllByRole('row')[1];
      const autoMappedLocationCells =
        within(autoMappedLocation).getAllByRole('cell');
      expect(within(autoMappedLocationCells[0]).getByText('Location 1'));
      expect(within(autoMappedLocationCells[1]).getByText('Location 1'));
      expect(within(autoMappedLocationCells[2]).getByText('Minor'));

      // unmapped table
      const unmappedTable = screen.getByRole('table', {
        name: 'Local Authorities 1 unmapped location 2 mapped locations',
      });
      expect(within(unmappedTable).getAllByRole('row')).toHaveLength(4);

      // new locations table
      expect(
        screen.getByRole('heading', {
          name: 'New locations (4) No action required',
        }),
      ).toBeInTheDocument();
      const newLocationsAccordion = within(
        screen.getByTestId('new-locations-accordion'),
      );
      expect(
        newLocationsAccordion.getByRole('heading', {
          name: /Local Authorities \(2\)/,
        }),
      ).toBeInTheDocument();
      const newLocationsTable = screen.getByTestId(
        'new-locations-table-localAuthority',
      );
      expect(within(newLocationsTable).getAllByRole('row')).toHaveLength(3);

      await user.click(
        within(autoMappedLocation).getByRole('button', {
          name: 'Edit mapping for Location 1',
        }),
      );

      expect(
        await screen.findByText('Map existing location'),
      ).toBeInTheDocument();

      const modal = within(screen.getByRole('dialog'));

      await user.click(modal.getByLabelText('No mapping available'));

      expect(
        apiDataSetVersionService.updateLocationsMapping,
      ).not.toHaveBeenCalled();

      await user.click(
        modal.getByRole('button', { name: 'Update location mapping' }),
      );

      await waitFor(() =>
        expect(
          screen.queryByText('Map existing location'),
        ).not.toBeInTheDocument(),
      );

      jest.runAllTimers();

      await waitFor(() => {
        expect(
          apiDataSetVersionService.updateLocationsMapping,
        ).toHaveBeenCalledWith('draft-version-id', {
          updates: [
            {
              level: 'localAuthority',
              sourceKey: 'Location1Key',
              type: 'ManualNone',
            },
          ],
        });

        // Remove location from auto mapped table
        expect(
          screen.getByText('Auto mapped locations (11)'),
        ).toBeInTheDocument();
      });

      expect(
        autoMappedAccordion.getByRole('heading', {
          name: /Local Authorities \(1\)/,
        }),
      ).toBeInTheDocument();
      expect(
        within(autoMappedTable).queryByText('Location 1'),
      ).not.toBeInTheDocument();

      // Add location to unmapped table
      const unmappedRows = within(unmappedTable).getAllByRole('row');
      expect(unmappedRows).toHaveLength(5);
      const unmappedLocationCells = within(unmappedRows[4]).getAllByRole(
        'cell',
      );
      expect(within(unmappedLocationCells[0]).getByText('Location 1'));
      expect(
        within(unmappedLocationCells[1]).getByText('No mapping available'),
      );
      expect(within(unmappedLocationCells[2]).getByText('Major'));

      // Add location to new locations table
      expect(
        screen.getByRole('heading', {
          name: 'New locations (5) No action required',
        }),
      ).toBeInTheDocument();
      expect(
        newLocationsAccordion.getByRole('heading', {
          name: /Local Authorities \(3\)/,
        }),
      ).toBeInTheDocument();
      const newLocationRows = within(newLocationsTable).getAllByRole('row');
      expect(newLocationRows).toHaveLength(4);
      const newLocationCells = within(newLocationRows[3]).getAllByRole('cell');
      expect(within(newLocationCells[0]).getByText('Not applicable'));
      expect(within(newLocationCells[1]).getByText('Location 1'));
    });

    test('changing the mapping on an auto mapped location"', async () => {
      apiDataSetService.getDataSet.mockResolvedValue(testDataSet);
      apiDataSetVersionService.getLocationsMapping.mockResolvedValue(
        testLocationsMapping,
      );

      const { user } = renderPage();

      expect(await screen.findByText('Data set title')).toBeInTheDocument();

      // auto mapped table
      expect(
        screen.getByRole('heading', {
          name: 'Auto mapped locations (12) No action required',
        }),
      ).toBeInTheDocument();
      const autoMappedAccordion = within(
        screen.getByTestId('auto-mapped-accordion'),
      );
      expect(
        autoMappedAccordion.getByRole('heading', {
          name: /Local Authorities \(2\)/,
        }),
      ).toBeInTheDocument();
      const autoMappedTable = screen.getByTestId(
        'auto-mapped-table-localAuthority',
      );
      const autoMappedLocation = within(autoMappedTable).getAllByRole('row')[1];
      const autoMappedLocationCells =
        within(autoMappedLocation).getAllByRole('cell');
      expect(within(autoMappedLocationCells[0]).getByText('Location 1'));
      expect(within(autoMappedLocationCells[1]).getByText('Location 1'));
      expect(within(autoMappedLocationCells[2]).getByText('Minor'));

      // unmapped table
      const unmappedTable = screen.getByRole('table', {
        name: 'Local Authorities 1 unmapped location 2 mapped locations',
      });
      expect(within(unmappedTable).getAllByRole('row')).toHaveLength(4);

      await user.click(
        within(autoMappedLocation).getByRole('button', {
          name: 'Edit mapping for Location 1',
        }),
      );

      expect(
        await screen.findByText('Map existing location'),
      ).toBeInTheDocument();

      const modal = within(screen.getByRole('dialog'));

      await user.click(modal.getByLabelText('Location 6'));

      expect(
        apiDataSetVersionService.updateLocationsMapping,
      ).not.toHaveBeenCalled();

      await user.click(
        modal.getByRole('button', { name: 'Update location mapping' }),
      );

      await waitFor(() =>
        expect(
          screen.queryByText('Map existing location'),
        ).not.toBeInTheDocument(),
      );

      jest.runAllTimers();

      await waitFor(() => {
        expect(
          apiDataSetVersionService.updateLocationsMapping,
        ).toHaveBeenCalledWith('draft-version-id', {
          updates: [
            {
              candidateKey: 'Location6Key',
              level: 'localAuthority',
              sourceKey: 'Location1Key',
              type: 'ManualMapped',
            },
          ],
        });

        expect(
          screen.getByText('Auto mapped locations (11)'),
        ).toBeInTheDocument();
      });

      expect(
        autoMappedAccordion.getByRole('heading', {
          name: /Local Authorities \(1\)/,
        }),
      ).toBeInTheDocument();

      expect(
        within(autoMappedTable).queryByText('Location 1'),
      ).not.toBeInTheDocument();

      // Add location to unmapped & manually mapped table
      const unmappedRows = within(unmappedTable).getAllByRole('row');
      expect(unmappedRows).toHaveLength(5);
      const unmappedLocationCells = within(unmappedRows[4]).getAllByRole(
        'cell',
      );
      expect(within(unmappedLocationCells[0]).getByText('Location 1'));
      expect(within(unmappedLocationCells[1]).getByText('Location 6'));
      expect(within(unmappedLocationCells[2]).getByText('Minor'));
    });

    test('batches updates made in quick succession', async () => {
      apiDataSetService.getDataSet.mockResolvedValue(testDataSet);
      apiDataSetVersionService.getLocationsMapping.mockResolvedValue(
        testLocationsMapping,
      );

      const { user } = renderPage();

      expect(await screen.findByText('Data set title')).toBeInTheDocument();

      expect(
        screen.getByRole('link', {
          name: 'There is 1 unmapped local authority',
        }),
      ).toHaveAttribute('href', '#mappable-localAuthority');

      // unmapped table
      const unmappedTable = screen.getByRole('table', {
        name: 'Local Authorities 1 unmapped location 2 mapped locations',
      });
      const unmappedLocation1 = within(unmappedTable).getAllByRole('row')[1];
      const unmappedLocation1Cells =
        within(unmappedLocation1).getAllByRole('cell');
      expect(within(unmappedLocation1Cells[0]).getByText('Location 2'));
      expect(within(unmappedLocation1Cells[1]).getByText('Unmapped'));
      expect(within(unmappedLocation1Cells[2]).getByText('N/A'));

      const unmappedLocation2 = within(unmappedTable).getAllByRole('row')[2];
      const unmappedLocation2Cells =
        within(unmappedLocation2).getAllByRole('cell');
      expect(within(unmappedLocation2Cells[0]).getByText('Location 3'));
      expect(within(unmappedLocation2Cells[1]).getByText('Location 3 updated'));
      expect(within(unmappedLocation2Cells[2]).getByText('Minor'));

      expect(
        apiDataSetVersionService.updateLocationsMapping,
      ).not.toHaveBeenCalled();

      await user.click(
        within(unmappedTable).getByRole('button', {
          name: 'No mapping for Location 2',
        }),
      );
      await user.click(
        within(unmappedTable).getByRole('button', {
          name: 'No mapping for Location 3',
        }),
      );

      jest.runAllTimers();

      await waitFor(() => {
        expect(
          apiDataSetVersionService.updateLocationsMapping,
        ).toHaveBeenCalledWith('draft-version-id', {
          updates: [
            {
              level: 'localAuthority',
              sourceKey: 'Location2Key',
              type: 'ManualNone',
            },
            {
              level: 'localAuthority',
              sourceKey: 'Location3Key',
              type: 'ManualNone',
            },
          ],
        });

        expect(
          screen.queryByText('There is 1 unmapped local authority'),
        ).not.toBeInTheDocument();
      });

      // Update locations in unmapped table
      expect(within(unmappedLocation1Cells[0]).getByText('Location 2'));
      expect(
        within(unmappedLocation1Cells[1]).getByText('No mapping available'),
      );
      expect(within(unmappedLocation1Cells[2]).getByText('Major'));

      expect(within(unmappedLocation2Cells[0]).getByText('Location 3'));
      expect(
        within(unmappedLocation2Cells[1]).getByText('No mapping available'),
      );
      expect(within(unmappedLocation2Cells[2]).getByText('Major'));
    });
  });

  function renderPage(options?: { release?: Release; dataSetId?: string }) {
    const { release = testRelease, dataSetId = 'data-set-id' } = options ?? {};

    return render(
      <TestConfigContextProvider>
        <ReleaseContextProvider release={release}>
          <MemoryRouter
            initialEntries={[
              generatePath<ReleaseDataSetRouteParams>(
                releaseApiDataSetLocationsMappingRoute.path,
                {
                  publicationId: release.publicationId,
                  releaseId: release.id,
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
        </ReleaseContextProvider>
      </TestConfigContextProvider>,
    );
  }
});
