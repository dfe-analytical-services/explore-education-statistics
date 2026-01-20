import { TestConfigContextProvider } from '@admin/contexts/ConfigContext';
import { testRelease } from '@admin/pages/release/__data__/testRelease';
import ReleaseApiDataSetFiltersMappingPage from '@admin/pages/release/data/ReleaseApiDataSetFiltersMappingPage';
import { ReleaseVersionContextProvider } from '@admin/pages/release/contexts/ReleaseVersionContext';
import {
  releaseApiDataSetFiltersMappingRoute,
  ReleaseDataSetRouteParams,
} from '@admin/routes/releaseRoutes';
import _apiDataSetService, {
  ApiDataSet,
} from '@admin/services/apiDataSetService';
import testFiltersMapping, {
  testFiltersMappingUnmappedColumns,
} from '@admin/pages/release/data/__data__/testFiltersMapping';
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

describe('ReleaseApiDataSetFiltersMappingPage', () => {
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
    apiDataSetVersionService.getFiltersMapping.mockResolvedValue(
      testFiltersMapping,
    );

    renderPage();

    expect(await screen.findByText('Data set title')).toBeInTheDocument();
    expect(screen.getByText('Map filters')).toBeInTheDocument();

    // mappable
    expect(
      screen.getByRole('heading', {
        name: 'Filter options not found in new data set',
      }),
    ).toBeInTheDocument();

    // mappable Filter 1
    const mappableFilter1Table = screen.getByRole('table', {
      name: 'Filter 1 2 unmapped filter options 1 mapped filter option Column: Filter1Key',
    });
    expect(mappableFilter1Table).toBe(
      screen.getByTestId('mappable-table-filter1Key'),
    );
    expect(within(mappableFilter1Table).getAllByRole('row')).toHaveLength(4);

    // mappable Filter 2
    const mappableFilter2Table = screen.getByRole('table', {
      name: 'Filter 2 1 unmapped filter option 2 mapped filter options Column: Filter2Key',
    });
    expect(mappableFilter2Table).toBe(
      screen.getByTestId('mappable-table-filter2Key'),
    );
    expect(within(mappableFilter2Table).getAllByRole('row')).toHaveLength(4);

    // new filter options
    expect(
      screen.getByRole('heading', {
        name: 'Filter options not found in old dataset (1) No action required',
      }),
    ).toBeInTheDocument();

    const newFilterOptionsAccordion = within(
      screen.getByTestId('new-filter-options-accordion'),
    );

    // new Filter 1
    expect(
      newFilterOptionsAccordion.getByRole('heading', {
        name: /Filter 1 \(1\) Column: Filter1Key/,
      }),
    ).toBeInTheDocument();
    expect(
      within(
        newFilterOptionsAccordion.getByTestId('new-items-table-Filter1Key'),
      ).getAllByRole('row'),
    ).toHaveLength(2);

    // auto mapped
    expect(
      screen.getByRole('heading', {
        name: 'Filter options found in both (4) No action required',
      }),
    ).toBeInTheDocument();

    const autoMappedAccordion = within(
      screen.getByTestId('auto-mapped-filter-options-accordion'),
    );

    // auto mapped Filter 1
    expect(
      autoMappedAccordion.getByRole('heading', {
        name: /Filter 1 \(1\) Column: Filter1Key/,
      }),
    ).toBeInTheDocument();
    expect(
      within(
        autoMappedAccordion.getByTestId('auto-mapped-table-filter1Key'),
      ).getAllByRole('row'),
    ).toHaveLength(2);

    // auto mapped Filter3Key
    expect(
      autoMappedAccordion.getByRole('heading', {
        name: /Filter 3 \(3\) Column: Filter3Key/,
      }),
    ).toBeInTheDocument();
    expect(
      within(
        autoMappedAccordion.getByTestId('auto-mapped-table-filter3Key'),
      ).getAllByRole('row'),
    ).toHaveLength(4);
  });

  test('renders the navigation correctly', async () => {
    apiDataSetService.getDataSet.mockResolvedValue(testDataSet);
    apiDataSetVersionService.getFiltersMapping.mockResolvedValue(
      testFiltersMapping,
    );

    renderPage();

    expect(await screen.findByText('Data set title')).toBeInTheDocument();

    const nav = within(
      screen.getByRole('navigation', {
        name: 'On this page',
      }),
    );
    const navItems = nav.getAllByRole('listitem');
    expect(navItems).toHaveLength(8);

    expect(
      within(navItems[0]).getByRole('link', {
        name: 'Filter options not found in new data set',
      }),
    ).toHaveAttribute('href', '#mappable-filter-options');
    const mappableSubItems = within(navItems[0]).getAllByRole('listitem');
    expect(mappableSubItems).toHaveLength(2);
    expect(
      within(mappableSubItems[0]).getByRole('link', {
        name: 'Filter 1',
      }),
    ).toHaveAttribute('href', '#mappable-table-filter1Key');
    expect(
      within(mappableSubItems[1]).getByRole('link', {
        name: 'Filter 2',
      }),
    ).toHaveAttribute('href', '#mappable-table-filter2Key');

    expect(
      within(navItems[3]).getByRole('link', {
        name: 'Filter options not found in old dataset',
      }),
    ).toHaveAttribute('href', '#new-filter-options');
    const newSubItems = within(navItems[3]).getAllByRole('listitem');
    expect(newSubItems).toHaveLength(1);
    expect(
      within(newSubItems[0]).getByRole('link', {
        name: 'Filter 1',
      }),
    ).toHaveAttribute('href', '#new-filter-options-filter1Key');

    expect(
      within(navItems[5]).getByRole('link', {
        name: 'Filter options found in both',
      }),
    ).toHaveAttribute('href', '#auto-mapped-filter-options');
    const autoMappedSubItems = within(navItems[5]).getAllByRole('listitem');
    expect(autoMappedSubItems).toHaveLength(2);
    expect(
      within(autoMappedSubItems[0]).getByRole('link', {
        name: 'Filter 1',
      }),
    ).toHaveAttribute('href', '#auto-mapped-filter-options-filter1Key');
    expect(
      within(autoMappedSubItems[1]).getByRole('link', {
        name: 'Filter 3',
      }),
    ).toHaveAttribute('href', '#auto-mapped-filter-options-filter3Key');
  });

  test('renders the notification banner if there are mappable filter options', async () => {
    apiDataSetService.getDataSet.mockResolvedValue(testDataSet);
    apiDataSetVersionService.getFiltersMapping.mockResolvedValue(
      testFiltersMapping,
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
        name: 'There are 2 unmapped Filter 1 filter options',
      }),
    ).toHaveAttribute('href', '#mappable-table-filter1Key');
    expect(
      banner.getByRole('link', {
        name: 'There is 1 unmapped Filter 2 filter option',
      }),
    ).toHaveAttribute('href', '#mappable-table-filter2Key');
  });

  test('does not render the notification banner if there are no unmapped filter options', async () => {
    apiDataSetService.getDataSet.mockResolvedValue(testDataSet);
    apiDataSetVersionService.getFiltersMapping.mockResolvedValue({
      candidates: {
        Filter1Key: {
          label: 'Filter 1',
          options: {
            Filter1Option1Key: {
              label: 'Filter 1 Option 1',
            },
          },
        },
      },
      mappings: {
        Filter1Key: {
          candidateKey: 'Filter1Key',
          optionMappings: {
            Filter1Option1Key: {
              candidateKey: 'Filter1Option1Key',
              publicId: 'filter-1-option-1-public-id',
              source: { label: 'Filter 1 Option 1' },
              type: 'AutoMapped',
            },
          },
          publicId: 'filter-1-public-id',
          source: {
            label: 'Filter 1',
          },
          type: 'AutoMapped',
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

  test('renders a message if there are no mappable filter options', async () => {
    apiDataSetService.getDataSet.mockResolvedValue(testDataSet);
    apiDataSetVersionService.getFiltersMapping.mockResolvedValue({
      candidates: {
        Filter1Key: {
          label: 'Filter 1',
          options: {
            Filter1Option1Key: {
              label: 'Filter 1 Option 1',
            },
          },
        },
      },
      mappings: {
        Filter1Key: {
          candidateKey: 'Filter1Key',
          optionMappings: {
            Filter1Option1Key: {
              candidateKey: 'Filter1Option1Key',
              publicId: 'filter-1-option-1-public-id',
              source: { label: 'Filter 1 Option 1' },
              type: 'AutoMapped',
            },
          },
          publicId: 'filter-1-public-id',
          source: {
            label: 'Filter 1',
          },
          type: 'AutoMapped',
        },
      },
    });

    renderPage();

    expect(await screen.findByText('Data set title')).toBeInTheDocument();

    expect(
      screen.getByRole('heading', {
        name: 'Filter options not found in new data set',
      }),
    ).toBeInTheDocument();

    expect(screen.getByText('No filter options.')).toBeInTheDocument();

    expect(screen.queryByTestId('mappable-Filter1Key')).not.toBeInTheDocument();

    expect(screen.queryByTestId('mappable-Filter2Key')).not.toBeInTheDocument();
  });

  test('renders a message if there are no new filter options', async () => {
    apiDataSetService.getDataSet.mockResolvedValue(testDataSet);
    apiDataSetVersionService.getFiltersMapping.mockResolvedValue({
      candidates: {
        Filter1Key: {
          label: 'Filter 1',
          options: {
            Filter1Option1Key: {
              label: 'Filter 1 Option 1',
            },
          },
        },
      },
      mappings: {
        Filter1Key: {
          candidateKey: 'Filter1Key',
          optionMappings: {
            Filter1Option1Key: {
              candidateKey: 'Filter1Option1Key',
              publicId: 'filter-1-option-1-public-id',
              source: { label: 'Filter 1 Option 1' },
              type: 'AutoMapped',
            },
          },
          publicId: 'filter-1-public-id',
          source: {
            label: 'Filter 1',
          },
          type: 'AutoMapped',
        },
      },
    });

    renderPage();

    expect(await screen.findByText('Data set title')).toBeInTheDocument();

    expect(
      screen.getByRole('heading', {
        name: 'Filter options not found in old dataset (0) No action required',
      }),
    ).toBeInTheDocument();

    expect(screen.getByText('No new filter options.')).toBeInTheDocument();

    expect(
      screen.queryByTestId('new-filter-options-Filter1Key'),
    ).not.toBeInTheDocument();

    expect(
      screen.queryByTestId('new-filter-options-Filter2Key'),
    ).not.toBeInTheDocument();
  });

  test('renders a message if there are no filter options found in both', async () => {
    apiDataSetService.getDataSet.mockResolvedValue(testDataSet);
    apiDataSetVersionService.getFiltersMapping.mockResolvedValue({
      candidates: {
        Filter1Key: {
          label: 'Filter 1',
          options: {
            Filter1Option1Key: {
              label: 'Filter 1 Option 1',
            },
          },
        },
      },
      mappings: {
        Filter1Key: {
          candidateKey: 'Filter1Key',
          optionMappings: {
            Filter1Option1Key: {
              candidateKey: 'Filter1Option1Key',
              publicId: 'filter-1-option-1-public-id',
              source: { label: 'Filter 1 Option 1' },
              type: 'ManualMapped',
            },
          },
          publicId: 'filter-1-public-id',
          source: {
            label: 'Filter 1',
          },
          type: 'AutoMapped',
        },
      },
    });

    renderPage();

    expect(await screen.findByText('Data set title')).toBeInTheDocument();

    expect(
      screen.getByRole('heading', {
        name: 'Filter options found in both (0) No action required',
      }),
    ).toBeInTheDocument();

    expect(
      screen.getByText('No filter options found in both.'),
    ).toBeInTheDocument();

    expect(screen.queryByTestId('auto-mapped')).not.toBeInTheDocument();
  });

  describe('updating mappings', () => {
    beforeEach(() => {
      jest.useFakeTimers({ advanceTimers: true });
    });
    afterEach(() => {
      jest.useRealTimers();
    });

    test('changing an unmapped filter option to mapped', async () => {
      apiDataSetService.getDataSet.mockResolvedValue(testDataSet);
      apiDataSetVersionService.getFiltersMapping.mockResolvedValue(
        testFiltersMapping,
      );

      const { user } = renderPage();

      expect(await screen.findByText('Data set title')).toBeInTheDocument();

      expect(
        screen.getByRole('link', {
          name: 'There are 2 unmapped Filter 1 filter options',
        }),
      ).toHaveAttribute('href', '#mappable-table-filter1Key');

      // mappable table
      const mappableFilter1Table = screen.getByRole('table', {
        name: 'Filter 1 2 unmapped filter options 1 mapped filter option Column: Filter1Key',
      });
      const unmappedFilterOption =
        within(mappableFilter1Table).getAllByRole('row')[1];
      const mappableFilterOptionCells =
        within(unmappedFilterOption).getAllByRole('cell');
      expect(
        within(mappableFilterOptionCells[0]).getByText('Filter 1 Option 2'),
      );
      expect(within(mappableFilterOptionCells[1]).getByText('Unmapped'));
      expect(within(mappableFilterOptionCells[2]).getByText('N/A'));

      // new filter1 table
      expect(
        screen.getByRole('heading', {
          name: 'Filter options not found in old dataset (1) No action required',
        }),
      ).toBeInTheDocument();
      const newFilterOptionsAccordion = within(
        screen.getByTestId('new-filter-options-accordion'),
      );
      expect(
        newFilterOptionsAccordion.getByRole('heading', {
          name: /Filter 1 \(1\)/,
        }),
      ).toBeInTheDocument();
      const newFilterOptionsTable = screen.getByTestId(
        'new-items-table-Filter1Key',
      );
      const newFilterOptionRows = within(newFilterOptionsTable).getAllByRole(
        'row',
      );
      expect(newFilterOptionRows).toHaveLength(2);
      const newFilterOptionCells = within(newFilterOptionRows[1]).getAllByRole(
        'cell',
      );
      expect(within(newFilterOptionCells[0]).getByText('Not applicable'));
      expect(
        within(newFilterOptionCells[1]).getByText('Filter 1 Option 2 updated'),
      );

      await user.click(
        within(unmappedFilterOption).getByRole('button', {
          name: 'Map option for Filter 1 Option 2',
        }),
      );

      expect(
        await screen.findByText('Map existing filter option'),
      ).toBeInTheDocument();

      const modal = within(screen.getByRole('dialog'));

      await user.click(modal.getByLabelText('Filter 1 Option 2 updated'));

      expect(
        apiDataSetVersionService.updateFilterOptionsMapping,
      ).not.toHaveBeenCalled();

      await user.click(
        modal.getByRole('button', { name: 'Update filter option mapping' }),
      );

      await waitFor(() =>
        expect(
          screen.queryByText('Map existing filter option'),
        ).not.toBeInTheDocument(),
      );

      jest.runAllTimers();

      await waitFor(() => {
        expect(
          apiDataSetVersionService.updateFilterOptionsMapping,
        ).toHaveBeenCalledWith('draft-version-id', {
          updates: [
            {
              candidateKey: 'Filter1Option2UpdatedKey',
              filterKey: 'Filter1Key',
              sourceKey: 'Filter1Option2Key',
              type: 'ManualMapped',
            },
          ],
        });

        expect(
          screen.getByText('There is 1 unmapped Filter 1 filter option'),
        ).toBeInTheDocument();
      });

      // Update in unmapped table
      expect(
        within(mappableFilterOptionCells[0]).getByText('Filter 1 Option 2'),
      );
      expect(
        within(mappableFilterOptionCells[1]).getByText(
          'Filter 1 Option 2 updated',
        ),
      );
      expect(within(mappableFilterOptionCells[2]).getByText('Minor'));

      // Remove from new filter options
      expect(
        screen.getByRole('heading', {
          name: 'Filter options not found in old dataset (0) No action required',
        }),
      ).toBeInTheDocument();
      expect(
        screen.queryByTestId('new-items-table-Filter1Key'),
      ).not.toBeInTheDocument();
    });

    test('changing an unmapped filter option to "no mapping available" via the edit modal', async () => {
      apiDataSetService.getDataSet.mockResolvedValue(testDataSet);
      apiDataSetVersionService.getFiltersMapping.mockResolvedValue(
        testFiltersMapping,
      );

      const { user } = renderPage();

      expect(await screen.findByText('Data set title')).toBeInTheDocument();

      // unmapped table
      expect(
        screen.getByRole('link', {
          name: 'There are 2 unmapped Filter 1 filter options',
        }),
      ).toHaveAttribute('href', '#mappable-table-filter1Key');

      const mappableFilter1Table = screen.getByRole('table', {
        name: 'Filter 1 2 unmapped filter options 1 mapped filter option Column: Filter1Key',
      });
      const unmappedFilterOption =
        within(mappableFilter1Table).getAllByRole('row')[1];
      const mappableFilterOptionCells =
        within(unmappedFilterOption).getAllByRole('cell');
      expect(
        within(mappableFilterOptionCells[0]).getByText('Filter 1 Option 2'),
      );
      expect(within(mappableFilterOptionCells[1]).getByText('Unmapped'));
      expect(within(mappableFilterOptionCells[2]).getByText('N/A'));

      await user.click(
        within(unmappedFilterOption).getByRole('button', {
          name: 'Map option for Filter 1 Option 2',
        }),
      );

      expect(
        await screen.findByText('Map existing filter option'),
      ).toBeInTheDocument();

      const modal = within(screen.getByRole('dialog'));

      await user.click(modal.getByLabelText('No mapping available'));

      expect(
        apiDataSetVersionService.updateFilterOptionsMapping,
      ).not.toHaveBeenCalled();

      await user.click(
        modal.getByRole('button', { name: 'Update filter option mapping' }),
      );

      await waitFor(() =>
        expect(
          screen.queryByText('Map existing filter option'),
        ).not.toBeInTheDocument(),
      );

      jest.runAllTimers();

      await waitFor(() => {
        expect(
          apiDataSetVersionService.updateFilterOptionsMapping,
        ).toHaveBeenCalledWith('draft-version-id', {
          updates: [
            {
              filterKey: 'Filter1Key',
              sourceKey: 'Filter1Option2Key',
              type: 'ManualNone',
            },
          ],
        });

        expect(
          screen.getByText('There is 1 unmapped Filter 1 filter option'),
        ).toBeInTheDocument();
      });

      // Update in unmapped table
      expect(
        within(mappableFilterOptionCells[0]).getByText('Filter 1 Option 2'),
      );
      expect(
        within(mappableFilterOptionCells[1]).getByText('No mapping available'),
      );
      expect(within(mappableFilterOptionCells[2]).getByText('Major'));
    });

    test('changing an unmapped filter option to "no mapping available" via the row action button', async () => {
      apiDataSetService.getDataSet.mockResolvedValue(testDataSet);
      apiDataSetVersionService.getFiltersMapping.mockResolvedValue(
        testFiltersMapping,
      );

      const { user } = renderPage();

      expect(await screen.findByText('Data set title')).toBeInTheDocument();

      // unmapped table
      expect(
        screen.getByRole('link', {
          name: 'There are 2 unmapped Filter 1 filter options',
        }),
      ).toHaveAttribute('href', '#mappable-table-filter1Key');

      const mappableFilter1Table = screen.getByRole('table', {
        name: 'Filter 1 2 unmapped filter options 1 mapped filter option Column: Filter1Key',
      });
      const unmappedFilterOption =
        within(mappableFilter1Table).getAllByRole('row')[1];
      const mappableFilterOptionCells =
        within(unmappedFilterOption).getAllByRole('cell');
      expect(
        within(mappableFilterOptionCells[0]).getByText('Filter 1 Option 2'),
      );
      expect(within(mappableFilterOptionCells[1]).getByText('Unmapped'));
      expect(within(mappableFilterOptionCells[2]).getByText('N/A'));

      expect(
        apiDataSetVersionService.updateFilterOptionsMapping,
      ).not.toHaveBeenCalled();

      await user.click(
        within(unmappedFilterOption).getByRole('button', {
          name: 'No mapping for Filter 1 Option 2',
        }),
      );

      jest.runAllTimers();

      await waitFor(() => {
        expect(
          apiDataSetVersionService.updateFilterOptionsMapping,
        ).toHaveBeenCalledWith('draft-version-id', {
          updates: [
            {
              filterKey: 'Filter1Key',
              sourceKey: 'Filter1Option2Key',
              type: 'ManualNone',
            },
          ],
        });

        expect(
          screen.getByText('There is 1 unmapped Filter 1 filter option'),
        ).toBeInTheDocument();
      });

      // Update in unmapped table
      expect(
        within(mappableFilterOptionCells[0]).getByText('Filter 1 Option 2'),
      );
      expect(
        within(mappableFilterOptionCells[1]).getByText('No mapping available'),
      );
      expect(within(mappableFilterOptionCells[2]).getByText('Major'));
    });

    test('changing an auto mapped filter option to "no mapping available"', async () => {
      apiDataSetService.getDataSet.mockResolvedValue(testDataSet);
      apiDataSetVersionService.getFiltersMapping.mockResolvedValue(
        testFiltersMapping,
      );

      const { user } = renderPage();

      expect(await screen.findByText('Data set title')).toBeInTheDocument();

      // auto mapped table
      expect(
        screen.getByRole('heading', {
          name: 'Filter options found in both (4) No action required',
        }),
      ).toBeInTheDocument();
      const autoMappedAccordion = within(
        screen.getByTestId('auto-mapped-filter-options-accordion'),
      );
      expect(
        autoMappedAccordion.getByRole('heading', {
          name: /Filter 1 \(1\) /,
        }),
      ).toBeInTheDocument();
      const autoMappedTable = screen.getByTestId(
        'auto-mapped-table-filter1Key',
      );
      const autoMappedFilter = within(autoMappedTable).getAllByRole('row')[1];
      const autoMappedFilterCells =
        within(autoMappedFilter).getAllByRole('cell');
      expect(within(autoMappedFilterCells[0]).getByText('Filter 1 Option 1'));
      expect(within(autoMappedFilterCells[1]).getByText('Filter 1 Option 1'));
      expect(within(autoMappedFilterCells[2]).getByText('Minor'));

      // unmapped table
      const mappableFilter1Table = screen.getByRole('table', {
        name: 'Filter 1 2 unmapped filter options 1 mapped filter option Column: Filter1Key',
      });
      expect(within(mappableFilter1Table).getAllByRole('row')).toHaveLength(4);

      // new filter options table
      expect(
        screen.getByRole('heading', {
          name: 'Filter options not found in old dataset (1) No action required',
        }),
      ).toBeInTheDocument();
      const newFilterOptionsAccordion = within(
        screen.getByTestId('new-filter-options-accordion'),
      );
      expect(
        newFilterOptionsAccordion.getByRole('heading', {
          name: /Filter 1 \(1\)/,
        }),
      ).toBeInTheDocument();
      const newFilterOptionsTable = screen.getByTestId(
        'new-items-table-Filter1Key',
      );
      expect(within(newFilterOptionsTable).getAllByRole('row')).toHaveLength(2);

      await user.click(
        within(autoMappedFilter).getByRole('button', {
          name: 'Map option for Filter 1 Option 1',
        }),
      );

      expect(
        await screen.findByText('Map existing filter option'),
      ).toBeInTheDocument();

      const modal = within(screen.getByRole('dialog'));

      await user.click(modal.getByLabelText('No mapping available'));

      expect(
        apiDataSetVersionService.updateFilterOptionsMapping,
      ).not.toHaveBeenCalled();

      await user.click(
        modal.getByRole('button', { name: 'Update filter option mapping' }),
      );

      await waitFor(() =>
        expect(
          screen.queryByText('Map existing filter option'),
        ).not.toBeInTheDocument(),
      );

      jest.runAllTimers();

      await waitFor(() => {
        expect(
          apiDataSetVersionService.updateFilterOptionsMapping,
        ).toHaveBeenCalledWith('draft-version-id', {
          updates: [
            {
              filterKey: 'Filter1Key',
              sourceKey: 'Filter1Option1Key',
              type: 'ManualNone',
            },
          ],
        });

        expect(
          screen.getByText('Filter options found in both (3)'),
        ).toBeInTheDocument();
      });

      expect(
        autoMappedAccordion.queryByRole('heading', {
          name: /Filter 1 \(1\)/,
        }),
      ).not.toBeInTheDocument();

      // Add to unmapped table
      const mappableRows = within(mappableFilter1Table).getAllByRole('row');
      expect(mappableRows).toHaveLength(5);
      const mappableFilterOptionCells = within(mappableRows[4]).getAllByRole(
        'cell',
      );
      expect(
        within(mappableFilterOptionCells[0]).getByText('Filter 1 Option 1'),
      );
      expect(
        within(mappableFilterOptionCells[1]).getByText('No mapping available'),
      );
      expect(within(mappableFilterOptionCells[2]).getByText('Major'));

      // Add to new filter options table
      expect(
        screen.getByRole('heading', {
          name: 'Filter options not found in old dataset (2) No action required',
        }),
      ).toBeInTheDocument();
      expect(
        newFilterOptionsAccordion.getByRole('heading', {
          name: /Filter 1 \(2\)/,
        }),
      ).toBeInTheDocument();
      const newFilterOptionRows = within(newFilterOptionsTable).getAllByRole(
        'row',
      );
      expect(newFilterOptionRows).toHaveLength(3);
      const newFilterOptionCells = within(newFilterOptionRows[2]).getAllByRole(
        'cell',
      );
      expect(within(newFilterOptionCells[0]).getByText('Not applicable'));
      expect(within(newFilterOptionCells[1]).getByText('Filter 1 Option 1'));
    });

    test('changing the mapping on an auto mapped filter option"', async () => {
      apiDataSetService.getDataSet.mockResolvedValue(testDataSet);
      apiDataSetVersionService.getFiltersMapping.mockResolvedValue(
        testFiltersMapping,
      );

      const { user } = renderPage();

      expect(await screen.findByText('Data set title')).toBeInTheDocument();

      // auto mapped table
      expect(
        screen.getByRole('heading', {
          name: 'Filter options found in both (4) No action required',
        }),
      ).toBeInTheDocument();
      const autoMappedAccordion = within(
        screen.getByTestId('auto-mapped-filter-options-accordion'),
      );
      expect(
        autoMappedAccordion.getByRole('heading', {
          name: /Filter 1 \(1\)/,
        }),
      ).toBeInTheDocument();
      const autoMappedTable = screen.getByTestId(
        'auto-mapped-table-filter1Key',
      );
      const autoMappedFilter = within(autoMappedTable).getAllByRole('row')[1];
      const autoMappedFilterCells =
        within(autoMappedFilter).getAllByRole('cell');
      expect(within(autoMappedFilterCells[0]).getByText('Filter 1 Option 1'));
      expect(within(autoMappedFilterCells[1]).getByText('Filter 1 Option 1'));
      expect(within(autoMappedFilterCells[2]).getByText('Minor'));

      // unmapped table
      const mappableFilter1Table = screen.getByRole('table', {
        name: 'Filter 1 2 unmapped filter options 1 mapped filter option Column: Filter1Key',
      });
      expect(within(mappableFilter1Table).getAllByRole('row')).toHaveLength(4);

      await user.click(
        within(autoMappedFilter).getByRole('button', {
          name: 'Map option for Filter 1 Option 1',
        }),
      );

      expect(
        await screen.findByText('Map existing filter option'),
      ).toBeInTheDocument();

      const modal = within(screen.getByRole('dialog'));

      await user.click(modal.getByLabelText('Filter 1 Option 2 updated'));

      expect(
        apiDataSetVersionService.updateFilterOptionsMapping,
      ).not.toHaveBeenCalled();

      await user.click(
        modal.getByRole('button', { name: 'Update filter option mapping' }),
      );

      await waitFor(() =>
        expect(
          screen.queryByText('Map existing filter option'),
        ).not.toBeInTheDocument(),
      );

      jest.runAllTimers();

      await waitFor(() => {
        expect(
          apiDataSetVersionService.updateFilterOptionsMapping,
        ).toHaveBeenCalledWith('draft-version-id', {
          updates: [
            {
              candidateKey: 'Filter1Option2UpdatedKey',
              filterKey: 'Filter1Key',
              sourceKey: 'Filter1Option1Key',
              type: 'ManualMapped',
            },
          ],
        });

        expect(
          screen.getByText('Filter options found in both (3)'),
        ).toBeInTheDocument();
      });

      expect(
        autoMappedAccordion.queryByRole('heading', {
          name: /Filter 1 \(1\)/,
        }),
      ).not.toBeInTheDocument();

      // Add to mappable table
      const mappableRows = within(mappableFilter1Table).getAllByRole('row');
      expect(mappableRows).toHaveLength(5);
      const mappableFilterOptionCells = within(mappableRows[4]).getAllByRole(
        'cell',
      );
      expect(
        within(mappableFilterOptionCells[0]).getByText('Filter 1 Option 1'),
      );
      expect(
        within(mappableFilterOptionCells[1]).getByText(
          'Filter 1 Option 2 updated',
        ),
      );
      expect(within(mappableFilterOptionCells[2]).getByText('Minor'));
    });

    test('batches updates made in quick succession', async () => {
      apiDataSetService.getDataSet.mockResolvedValue(testDataSet);
      apiDataSetVersionService.getFiltersMapping.mockResolvedValue(
        testFiltersMapping,
      );

      const { user } = renderPage();

      expect(await screen.findByText('Data set title')).toBeInTheDocument();

      expect(
        screen.getByRole('link', {
          name: 'There are 2 unmapped Filter 1 filter options',
        }),
      ).toHaveAttribute('href', '#mappable-table-filter1Key');

      // mappable table
      const mappableFilter1Table = screen.getByRole('table', {
        name: 'Filter 1 2 unmapped filter options 1 mapped filter option Column: Filter1Key',
      });
      const unmappedFilterOption1 =
        within(mappableFilter1Table).getAllByRole('row')[1];
      const mappableFilterOption1Cells = within(
        unmappedFilterOption1,
      ).getAllByRole('cell');
      expect(
        within(mappableFilterOption1Cells[0]).getByText('Filter 1 Option 2'),
      );
      expect(within(mappableFilterOption1Cells[1]).getByText('Unmapped'));
      expect(within(mappableFilterOption1Cells[2]).getByText('N/A'));

      const unmappedFilterOption2 =
        within(mappableFilter1Table).getAllByRole('row')[2];
      const mappableFilterOption2Cells = within(
        unmappedFilterOption2,
      ).getAllByRole('cell');
      expect(
        within(mappableFilterOption2Cells[0]).getByText('Filter 1 Option 3'),
      );
      expect(within(mappableFilterOption2Cells[1]).getByText('Unmapped'));
      expect(within(mappableFilterOption2Cells[2]).getByText('N/A'));

      expect(
        apiDataSetVersionService.updateFilterOptionsMapping,
      ).not.toHaveBeenCalled();

      await user.click(
        within(mappableFilter1Table).getByRole('button', {
          name: 'No mapping for Filter 1 Option 2',
        }),
      );
      await user.click(
        within(mappableFilter1Table).getByRole('button', {
          name: 'No mapping for Filter 1 Option 3',
        }),
      );

      jest.runAllTimers();

      await waitFor(() => {
        expect(
          apiDataSetVersionService.updateFilterOptionsMapping,
        ).toHaveBeenCalledWith('draft-version-id', {
          updates: [
            {
              filterKey: 'Filter1Key',
              sourceKey: 'Filter1Option2Key',
              type: 'ManualNone',
            },
            {
              filterKey: 'Filter1Key',
              sourceKey: 'Filter1Option3Key',
              type: 'ManualNone',
            },
          ],
        });

        expect(
          screen.queryByText('There are 2 unmapped Filter 1 filter options'),
        ).not.toBeInTheDocument();
      });

      // Update in unmapped table
      expect(
        within(mappableFilterOption1Cells[0]).getByText('Filter 1 Option 2'),
      );
      expect(
        within(mappableFilterOption1Cells[1]).getByText('No mapping available'),
      );
      expect(within(mappableFilterOption1Cells[2]).getByText('Major'));

      expect(
        within(mappableFilterOption2Cells[0]).getByText('Filter 1 Option 3'),
      );
      expect(
        within(mappableFilterOption2Cells[1]).getByText('No mapping available'),
      );
      expect(within(mappableFilterOption2Cells[2]).getByText('Major'));
    });
  });

  describe('unmapped filter columns', () => {
    test('renders navigation items for filter columns', async () => {
      apiDataSetService.getDataSet.mockResolvedValue(testDataSet);
      apiDataSetVersionService.getFiltersMapping.mockResolvedValue(
        testFiltersMappingUnmappedColumns,
      );

      renderPage();

      expect(await screen.findByText('Data set title')).toBeInTheDocument();

      const nav = within(
        screen.getByRole('navigation', { name: 'On this page' }),
      );

      const navItems = nav.getAllByRole('listitem');

      expect(navItems).toHaveLength(6);

      expect(
        within(navItems[0]).getByRole('link', {
          name: 'Filter columns not found in new data set',
        }),
      ).toHaveAttribute('href', '#mappable-filter-columns');

      expect(
        within(navItems[1]).getByRole('link', {
          name: 'Filter options not found in new data set',
        }),
      ).toHaveAttribute('href', '#mappable-filter-options');

      expect(
        within(navItems[2]).getByRole('link', { name: 'New filter columns' }),
      ).toHaveAttribute('href', '#new-filter-columns');

      expect(
        within(navItems[3]).getByRole('link', {
          name: 'Filter options not found in old dataset',
        }),
      ).toHaveAttribute('href', '#new-filter-options');

      expect(
        within(navItems[4]).getByRole('link', {
          name: 'Filter options found in both',
        }),
      ).toHaveAttribute('href', '#auto-mapped-filter-options');
    });

    test('renders correctly with unmapped filter columns', async () => {
      apiDataSetService.getDataSet.mockResolvedValue(testDataSet);
      apiDataSetVersionService.getFiltersMapping.mockResolvedValue(
        testFiltersMappingUnmappedColumns,
      );

      renderPage();

      expect(await screen.findByText('Data set title')).toBeInTheDocument();

      expect(
        screen.getByRole('heading', {
          name: 'Filter columns not found in new data set No action required',
        }),
      ).toBeInTheDocument();

      const unmappedRows = within(
        screen.getByTestId('mappable-filter-columns-table'),
      ).getAllByRole('row');

      expect(unmappedRows).toHaveLength(2);
      const unmappedRow1Cells = within(unmappedRows[1]).getAllByRole('cell');

      expect(unmappedRow1Cells[0]).toHaveTextContent('Filter 1');
      expect(unmappedRow1Cells[0]).toHaveTextContent('Column: Filter1Key');
      expect(unmappedRow1Cells[0]).toHaveTextContent('View filter options');
      expect(unmappedRow1Cells[1]).toHaveTextContent('No mapping available');
      expect(unmappedRow1Cells[2]).toHaveTextContent('Major');

      expect(
        screen.getByRole('heading', {
          name: 'New filter columns No action required',
        }),
      ).toBeInTheDocument();

      const newColumnsTableRows = within(
        screen.getByTestId('new-filter-columns-table'),
      ).getAllByRole('row');

      expect(newColumnsTableRows).toHaveLength(2);

      const newColumnsRow1Cells = within(newColumnsTableRows[1]).getAllByRole(
        'cell',
      );

      expect(newColumnsRow1Cells[0]).toHaveTextContent('No mapping available');
      expect(newColumnsRow1Cells[1]).toHaveTextContent('Filter 1');
      expect(newColumnsRow1Cells[1]).toHaveTextContent(
        'Column: Filter1UpdatedKey',
      );
      expect(newColumnsRow1Cells[1]).toHaveTextContent('View filter options');
      expect(newColumnsRow1Cells[2]).toHaveTextContent('Minor');
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
                releaseApiDataSetFiltersMappingRoute.path,
                {
                  publicationId: releaseVersion.publicationId,
                  releaseVersionId: releaseVersion.id,
                  dataSetId,
                },
              ),
            ]}
          >
            <Route
              component={ReleaseApiDataSetFiltersMappingPage}
              path={releaseApiDataSetFiltersMappingRoute.path}
            />
          </MemoryRouter>
        </ReleaseVersionContextProvider>
      </TestConfigContextProvider>,
    );
  }
});
