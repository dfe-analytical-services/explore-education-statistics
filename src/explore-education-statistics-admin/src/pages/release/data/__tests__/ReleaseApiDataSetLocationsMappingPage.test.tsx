import { TestConfigContextProvider } from '@admin/contexts/ConfigContext';
import { testRelease } from '@admin/pages/release/__data__/testRelease';
import ReleaseApiDataSetLocationsMappingPage from '@admin/pages/release/data/ReleaseApiDataSetLocationsMappingPage';
import { ReleaseVersionContextProvider } from '@admin/pages/release/contexts/ReleaseVersionContext';
import {
  releaseApiDataSetLocationsMappingRoute,
  ReleaseDataSetRouteParams,
} from '@admin/routes/releaseRoutes';
import _apiDataSetService, {
  ApiDataSet,
} from '@admin/services/apiDataSetService';
import testLocationsMapping, {
  testLocationsMappingGroups,
} from '@admin/pages/release/data/__data__/testLocationsMapping';
import _apiDataSetVersionService from '@admin/services/apiDataSetVersionService';
import { ReleaseVersion } from '@admin/services/releaseVersionService';
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
        name: 'Locations not found in new data set',
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
        newLocationsAccordion.getByTestId('new-items-table-localAuthority'),
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
        newLocationsAccordion.getByTestId('new-items-table-region'),
      ).getAllByRole('row'),
    ).toHaveLength(3);

    // auto mapped
    expect(
      screen.getByRole('heading', {
        name: 'Auto mapped locations (6) No action required',
      }),
    ).toBeInTheDocument();

    const autoMappedAccordion = within(
      screen.getByTestId('auto-mapped-locations-accordion'),
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
        name: 'English Devolved Areas (3)',
      }),
    ).toBeInTheDocument();
    expect(
      within(
        autoMappedAccordion.getByTestId(
          'auto-mapped-table-englishDevolvedArea',
        ),
      ).getAllByRole('row'),
    ).toHaveLength(4);
  });

  test('renders the location codes in the mappings tables', async () => {
    apiDataSetService.getDataSet.mockResolvedValue(testDataSet);
    apiDataSetVersionService.getLocationsMapping.mockResolvedValue(
      testLocationsMapping,
    );

    renderPage();

    expect(await screen.findByText('Data set title')).toBeInTheDocument();
    expect(screen.getByText('Map locations')).toBeInTheDocument();

    // mappable LAs
    const mappableRows = within(
      screen.getByRole('table', {
        name: 'Local Authorities 1 unmapped location 2 mapped locations',
      }),
    ).getAllByRole('row');
    const mappableRow2Cells = within(mappableRows[2]).getAllByRole('cell');
    expect(mappableRow2Cells[0]).toHaveTextContent('Location 3');
    expect(mappableRow2Cells[0]).toHaveTextContent('Code: location-3-code');
    expect(mappableRow2Cells[1]).toHaveTextContent('Location 3 updated');
    expect(mappableRow2Cells[1]).toHaveTextContent(
      'Code: location-3-code-updated',
    );

    // new locations
    const newRows = within(
      screen.getByTestId('new-items-table-localAuthority'),
    ).getAllByRole('row');
    const newRow1Cells = within(newRows[1]).getAllByRole('cell');
    expect(newRow1Cells[1]).toHaveTextContent('Location 6');
    expect(newRow1Cells[1]).toHaveTextContent('location-6-code');

    // auto mapped
    const autoMappedRows = within(
      screen.getByTestId('auto-mapped-table-localAuthority'),
    ).getAllByRole('row');
    const autoMappedRow1Cells = within(autoMappedRows[1]).getAllByRole('cell');
    expect(autoMappedRow1Cells[0]).toHaveTextContent('Location 1');
    expect(autoMappedRow1Cells[0]).toHaveTextContent('Code: location-1-code');
    expect(autoMappedRow1Cells[1]).toHaveTextContent('Location 1');
    expect(autoMappedRow1Cells[1]).toHaveTextContent('Code: location-1-code');
  });

  test('sets the mappable type to major if the location code has changed', async () => {
    apiDataSetService.getDataSet.mockResolvedValue(testDataSet);
    apiDataSetVersionService.getLocationsMapping.mockResolvedValue(
      testLocationsMapping,
    );

    renderPage();

    expect(await screen.findByText('Data set title')).toBeInTheDocument();
    expect(screen.getByText('Map locations')).toBeInTheDocument();

    // mappable LAs
    const mappableRows = within(
      screen.getByRole('table', {
        name: 'Local Authorities 1 unmapped location 2 mapped locations',
      }),
    ).getAllByRole('row');
    const mappableRow2Cells = within(mappableRows[2]).getAllByRole('cell');
    expect(mappableRow2Cells[0]).toHaveTextContent('Location 3');
    expect(mappableRow2Cells[0]).toHaveTextContent('Code: location-3-code');
    expect(mappableRow2Cells[1]).toHaveTextContent('Location 3 updated');
    expect(mappableRow2Cells[1]).toHaveTextContent(
      'Code: location-3-code-updated',
    );
    expect(mappableRow2Cells[2]).toHaveTextContent('Major');
  });

  test('renders the navigation correctly', async () => {
    apiDataSetService.getDataSet.mockResolvedValue(testDataSet);
    apiDataSetVersionService.getLocationsMapping.mockResolvedValue(
      testLocationsMapping,
    );

    renderPage();

    expect(await screen.findByText('Data set title')).toBeInTheDocument();

    const nav = within(
      screen.getByRole('navigation', { name: 'On this page' }),
    );

    const navItems = nav.getAllByRole('listitem');

    expect(navItems).toHaveLength(10);

    expect(
      within(navItems[0]).getByRole('link', {
        name: 'Locations not found in new data set',
      }),
    ).toHaveAttribute('href', '#mappable-locations');

    const mappableSubItems = within(navItems[0]).getAllByRole('listitem');

    expect(mappableSubItems).toHaveLength(2);
    expect(
      within(mappableSubItems[0]).getByRole('link', {
        name: 'Local Authorities',
      }),
    ).toHaveAttribute('href', '#mappable-table-localAuthority');
    expect(
      within(mappableSubItems[1]).getByRole('link', { name: 'Regions' }),
    ).toHaveAttribute('href', '#mappable-table-region');

    expect(
      within(navItems[3]).getByRole('link', { name: 'New locations' }),
    ).toHaveAttribute('href', '#new-locations');

    const newSubItems = within(navItems[3]).getAllByRole('listitem');

    expect(newSubItems).toHaveLength(2);
    expect(
      within(newSubItems[0]).getByRole('link', { name: 'Local Authorities' }),
    ).toHaveAttribute('href', '#new-locations-localAuthority');
    expect(
      within(newSubItems[1]).getByRole('link', { name: 'Regions' }),
    ).toHaveAttribute('href', '#new-locations-region');

    expect(
      within(navItems[6]).getByRole('link', { name: 'Auto mapped locations' }),
    ).toHaveAttribute('href', '#auto-mapped-locations');

    const autoMappedSubItems = within(navItems[6]).getAllByRole('listitem');

    expect(autoMappedSubItems).toHaveLength(3);

    expect(
      within(autoMappedSubItems[0]).getByRole('link', {
        name: 'English Devolved Areas',
      }),
    ).toHaveAttribute('href', '#auto-mapped-locations-englishDevolvedArea');
    expect(
      within(autoMappedSubItems[1]).getByRole('link', {
        name: 'Local Authorities',
      }),
    ).toHaveAttribute('href', '#auto-mapped-locations-localAuthority');
    expect(
      within(autoMappedSubItems[2]).getByRole('link', { name: 'Regions' }),
    ).toHaveAttribute('href', '#auto-mapped-locations-region');
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
    ).toHaveAttribute('href', '#mappable-table-localAuthority');
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

  test('renders a message if there are no mappable locations', async () => {
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
        name: 'Locations not found in new data set',
      }),
    ).toBeInTheDocument();

    expect(screen.getByText('No locations.')).toBeInTheDocument();

    expect(
      screen.queryByTestId('mappable-locations-localAuthority'),
    ).not.toBeInTheDocument();

    expect(
      screen.queryByTestId('mappable-locations-region'),
    ).not.toBeInTheDocument();
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

  describe('with location group mappings', () => {
    test('renders navigation items for location groups', async () => {
      apiDataSetService.getDataSet.mockResolvedValue(testDataSet);
      apiDataSetVersionService.getLocationsMapping.mockResolvedValue(
        testLocationsMappingGroups,
      );

      renderPage();

      expect(await screen.findByText('Data set title')).toBeInTheDocument();

      const nav = within(
        screen.getByRole('navigation', { name: 'On this page' }),
      );

      const navItems = nav.getAllByRole('listitem');

      expect(navItems).toHaveLength(12);

      expect(
        within(navItems[0]).getByRole('link', {
          name: 'Location groups not found in new data set',
        }),
      ).toHaveAttribute('href', '#deleted-location-groups');

      expect(
        within(navItems[1]).getByRole('link', {
          name: 'Locations not found in new data set',
        }),
      ).toHaveAttribute('href', '#mappable-locations');

      expect(
        within(navItems[4]).getByRole('link', { name: 'New location groups' }),
      ).toHaveAttribute('href', '#new-location-groups');

      expect(
        within(navItems[5]).getByRole('link', { name: 'New locations' }),
      ).toHaveAttribute('href', '#new-locations');

      expect(
        within(navItems[8]).getByRole('link', {
          name: 'Auto mapped locations',
        }),
      ).toHaveAttribute('href', '#auto-mapped-locations');
    });

    test('renders tables for deleted and new location groups', async () => {
      apiDataSetService.getDataSet.mockResolvedValue(testDataSet);
      apiDataSetVersionService.getLocationsMapping.mockResolvedValue(
        testLocationsMappingGroups,
      );

      renderPage();

      expect(await screen.findByText('Data set title')).toBeInTheDocument();

      expect(screen.getByText('Map locations')).toBeInTheDocument();

      // Deleted location groups
      const deletedGroupRows = within(
        screen.getByTestId('deleted-location-groups-table'),
      ).getAllByRole('row');

      const deletedGroup1Cells = within(deletedGroupRows[1]).getAllByRole(
        'cell',
      );
      expect(deletedGroup1Cells[0]).toHaveTextContent(
        'Local Authority Districts',
      );
      expect(deletedGroup1Cells[1]).toHaveTextContent('No mapping available');
      expect(deletedGroup1Cells[2]).toHaveTextContent('Major');

      const deletedGroup2Cells = within(deletedGroupRows[2]).getAllByRole(
        'cell',
      );
      expect(deletedGroup2Cells[0]).toHaveTextContent('Sponsors');
      expect(deletedGroup2Cells[1]).toHaveTextContent('No mapping available');
      expect(deletedGroup2Cells[2]).toHaveTextContent('Major');

      // New location groups
      const newGroupRows = within(
        screen.getByTestId('new-location-groups-table'),
      ).getAllByRole('row');

      const newGroup1Cells = within(newGroupRows[1]).getAllByRole('cell');
      expect(newGroup1Cells[0]).toHaveTextContent('No mapping available');
      expect(newGroup1Cells[1]).toHaveTextContent('Opportunity Areas');
      expect(newGroup1Cells[2]).toHaveTextContent('Minor');

      const newGroup2Cells = within(newGroupRows[2]).getAllByRole('cell');
      expect(newGroup2Cells[0]).toHaveTextContent('No mapping available');
      expect(newGroup2Cells[1]).toHaveTextContent('Wards');
      expect(newGroup2Cells[2]).toHaveTextContent('Minor');
    });
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
      ).toHaveAttribute('href', '#mappable-table-localAuthority');

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
        'new-items-table-localAuthority',
      );
      const newLocationRows = within(newLocationsTable).getAllByRole('row');
      expect(newLocationRows).toHaveLength(3);
      const newLocationCells = within(newLocationRows[1]).getAllByRole('cell');
      expect(within(newLocationCells[0]).getByText('Not applicable'));
      expect(within(newLocationCells[1]).getByText('Location 6'));

      await user.click(
        within(unmappedLocation).getByRole('button', {
          name: 'Map option for Location 2',
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
      expect(within(unmappedLocationCells[2]).getByText('Major'));

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
      ).toHaveAttribute('href', '#mappable-table-localAuthority');

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
          name: 'Map option for Location 2',
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
      ).toHaveAttribute('href', '#mappable-table-localAuthority');

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
          name: 'Auto mapped locations (6) No action required',
        }),
      ).toBeInTheDocument();
      const autoMappedAccordion = within(
        screen.getByTestId('auto-mapped-locations-accordion'),
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
        'new-items-table-localAuthority',
      );
      expect(within(newLocationsTable).getAllByRole('row')).toHaveLength(3);

      await user.click(
        within(autoMappedLocation).getByRole('button', {
          name: 'Map option for Location 1',
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
          screen.getByText('Auto mapped locations (5)'),
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

    test('changing the mapping on an auto mapped location', async () => {
      apiDataSetService.getDataSet.mockResolvedValue(testDataSet);
      apiDataSetVersionService.getLocationsMapping.mockResolvedValue(
        testLocationsMapping,
      );

      const { user } = renderPage();

      expect(await screen.findByText('Data set title')).toBeInTheDocument();

      // auto mapped table
      expect(
        screen.getByRole('heading', {
          name: 'Auto mapped locations (6) No action required',
        }),
      ).toBeInTheDocument();
      const autoMappedAccordion = within(
        screen.getByTestId('auto-mapped-locations-accordion'),
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
          name: 'Map option for Location 1',
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
          screen.getByText('Auto mapped locations (5)'),
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
      expect(within(unmappedLocationCells[2]).getByText('Major'));
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
      ).toHaveAttribute('href', '#mappable-table-localAuthority');

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
      expect(within(unmappedLocation2Cells[2]).getByText('Major'));

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

  function renderPage(options?: {
    releaseVersion?: ReleaseVersion;
    dataSetId?: string;
  }) {
    const { releaseVersion = testRelease, dataSetId = 'data-set-id' } =
      options ?? {};

    return render(
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
