import { TestConfigContextProvider } from '@admin/contexts/ConfigContext';
import { testRelease } from '@admin/pages/release/__data__/testRelease';
import { ReleaseVersionContextProvider } from '@admin/pages/release/contexts/ReleaseVersionContext';
import {
  releaseApiDataSetIndicatorsMappingRoute,
  ReleaseDataSetRouteParams,
} from '@admin/routes/releaseRoutes';
import _apiDataSetService, {
  ApiDataSet,
} from '@admin/services/apiDataSetService';
import testFiltersMapping from '@admin/pages/release/data/__data__/testFiltersMapping';
import _apiDataSetVersionService from '@admin/services/apiDataSetVersionService';
import { ReleaseVersion } from '@admin/services/releaseVersionService';
import render from '@common-test/render';
import { act, screen, waitFor, within } from '@testing-library/react';
import React from 'react';
import { generatePath, MemoryRouter, Route } from 'react-router-dom';
import testIndicatorsMapping from '@admin/pages/release/data/__data__/testIndicatorsMapping';
import ReleaseApiDataSetIndicatorsMappingPage from '@admin/pages/release/data/ReleaseApiDataSetIndicatorsMappingPage';

jest.mock('@admin/services/apiDataSetService');
jest.mock('@admin/services/apiDataSetVersionService');

const apiDataSetService = jest.mocked(_apiDataSetService);
const apiDataSetVersionService = jest.mocked(_apiDataSetVersionService);

describe('ReleaseApiDataSetIndicatorsMappingPage', () => {
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
    apiDataSetVersionService.getIndicatorsMapping.mockResolvedValue(
      testIndicatorsMapping,
    );

    renderPage();

    expect(await screen.findByText('Data set title')).toBeInTheDocument();
    expect(screen.getByText('Map indicators')).toBeInTheDocument();

    // Mappable indicators. This table should contain 2 manually mapped indicators, 1
    // indicator that has been manually mapped to "None", and 1 indicator that has
    // automatically been mapped to "None".
    expect(
      screen.getByRole('heading', {
        name: 'Indicators not found in new data set (4)',
      }),
    ).toBeInTheDocument();

    // The table should indicate that the 1 indicator that has automatically been
    // mapped to "None" still needs manual intervention, but the 2 manually mapped
    // indicator and the 1 indicator that was manually set to "None" are complete.
    const mappableIndicatorsTable = screen.getByRole('table', {
      name: 'Indicators that require mapping 1 unmapped indicator 3 mapped indicators',
    });
    expect(mappableIndicatorsTable).toBe(
      screen.getByTestId('mappable-table-default'),
    );
    expect(within(mappableIndicatorsTable).getAllByRole('row')).toHaveLength(5);

    // New indicators.
    expect(
      screen.getByRole('heading', {
        name: 'Indicators not found in old data set (1) No action required',
      }),
    ).toBeInTheDocument();

    const newIndicatorsTable = screen.getByRole('table', {
      name: 'Table showing new indicators',
    });
    expect(newIndicatorsTable).toBe(
      screen.getByTestId('new-items-table-default'),
    );
    expect(within(newIndicatorsTable).getAllByRole('row')).toHaveLength(2);

    // Auto-mapped indicators.
    expect(
      screen.getByRole('heading', {
        name: 'Indicators found in both (1) No action required',
      }),
    ).toBeInTheDocument();

    const autoMappedIndicatorsTable = screen.getByRole('table', {
      name: 'Table showing auto mapped indicators',
    });
    expect(autoMappedIndicatorsTable).toBe(
      screen.getByTestId('auto-mapped-table-default'),
    );
    expect(within(autoMappedIndicatorsTable).getAllByRole('row')).toHaveLength(
      2,
    );
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
    expect(navItems).toHaveLength(3);

    expect(
      within(navItems[0]).getByRole('link', {
        name: 'Indicators not found in new data set',
      }),
    ).toHaveAttribute('href', '#mappable-indicators');

    expect(
      within(navItems[1]).getByRole('link', {
        name: 'Indicators not found in old data set',
      }),
    ).toHaveAttribute('href', '#new-indicators');

    expect(
      within(navItems[2]).getByRole('link', {
        name: 'Indicators found in both',
      }),
    ).toHaveAttribute('href', '#auto-mapped-indicators');
  });

  test('renders the notification banner if there are mappable indicators', async () => {
    apiDataSetService.getDataSet.mockResolvedValue(testDataSet);
    apiDataSetVersionService.getIndicatorsMapping.mockResolvedValue(
      testIndicatorsMapping,
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
        name: 'There is 1 unmapped indicator',
      }),
    ).toHaveAttribute('href', '#mappable-table-default');
  });

  test('does not render the notification banner if there are no unmapped indicators', async () => {
    apiDataSetService.getDataSet.mockResolvedValue(testDataSet);
    apiDataSetVersionService.getIndicatorsMapping.mockResolvedValue({
      ...testIndicatorsMapping,
      mappings: {
        ...testIndicatorsMapping.mappings,
        Indicator4Deleted: {
          ...testIndicatorsMapping.mappings.Indicator4Deleted,
          type: 'ManualNone',
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

  test('renders a message if there are no mappable indicators', async () => {
    apiDataSetService.getDataSet.mockResolvedValue(testDataSet);
    apiDataSetVersionService.getIndicatorsMapping.mockResolvedValue({
      candidates: {
        Indicator1: {
          label: 'Indicator 1',
        },
      },
      mappings: {
        Indicator1: {
          candidateKey: 'Indicator1',
          publicId: 'indicator-1-public-id',
          source: { label: 'Indicator 1' },
          type: 'AutoMapped',
        },
      },
    });

    renderPage();

    expect(await screen.findByText('Data set title')).toBeInTheDocument();

    expect(
      screen.getByRole('heading', {
        name: 'Indicators not found in new data set (0)',
      }),
    ).toBeInTheDocument();

    expect(screen.getByText('No mappable indicators.')).toBeInTheDocument();

    expect(
      screen.queryByTestId('mappable-table-default'),
    ).not.toBeInTheDocument();
  });

  test('renders a message if there are no new indicators', async () => {
    apiDataSetService.getDataSet.mockResolvedValue(testDataSet);
    apiDataSetVersionService.getIndicatorsMapping.mockResolvedValue({
      candidates: {
        Indicator1: {
          label: 'Indicator 1',
        },
      },
      mappings: {
        Indicator1: {
          candidateKey: 'Indicator1',
          publicId: 'indicator-1-public-id',
          source: { label: 'Indicator 1' },
          type: 'AutoMapped',
        },
      },
    });

    renderPage();

    expect(await screen.findByText('Data set title')).toBeInTheDocument();

    expect(
      screen.getByRole('heading', {
        name: 'Indicators not found in old data set (0) No action required',
      }),
    ).toBeInTheDocument();

    expect(screen.getByText('No new indicators.')).toBeInTheDocument();

    expect(
      screen.queryByTestId('new-items-table-default'),
    ).not.toBeInTheDocument();
  });

  test('renders a message if there are no indicators automatically found in both', async () => {
    apiDataSetService.getDataSet.mockResolvedValue(testDataSet);
    apiDataSetVersionService.getIndicatorsMapping.mockResolvedValue({
      candidates: {
        Indicator1: {
          label: 'Indicator 1',
        },
      },
      mappings: {
        Indicator1: {
          candidateKey: 'Indicator1',
          publicId: 'indicator-1-public-id',
          source: { label: 'Indicator 1' },
          type: 'ManualMapped',
        },
      },
    });

    renderPage();

    expect(await screen.findByText('Data set title')).toBeInTheDocument();

    expect(
      screen.getByRole('heading', {
        name: 'Indicators found in both (0) No action required',
      }),
    ).toBeInTheDocument();

    expect(
      screen.getByText('No indicators found in both.'),
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

    test('changing an unmapped indicator to mapped', async () => {
      apiDataSetService.getDataSet.mockResolvedValue(testDataSet);
      apiDataSetVersionService.getIndicatorsMapping.mockResolvedValue(
        testIndicatorsMapping,
      );

      const { user } = renderPage();

      expect(await screen.findByText('Data set title')).toBeInTheDocument();

      expect(
        screen.getByRole('link', {
          name: 'There is 1 unmapped indicator',
        }),
      ).toHaveAttribute('href', '#mappable-table-default');

      // Mappable table
      const mappableIndicatorsTable = screen.getByRole('table', {
        name: 'Indicators that require mapping 1 unmapped indicator 3 mapped indicators',
      });
      const unmappedIndicatorRow = within(mappableIndicatorsTable).getAllByRole(
        'row',
      )[3];
      const mappableIndicatorCells =
        within(unmappedIndicatorRow).getAllByRole('cell');
      expect(within(mappableIndicatorCells[0]).getByText('Indicator 4'));
      expect(within(mappableIndicatorCells[1]).getByText('Unmapped'));
      expect(within(mappableIndicatorCells[2]).getByText('N/A'));

      // New indicators table.
      expect(
        screen.getByRole('heading', {
          name: 'Indicators not found in old data set (1) No action required',
        }),
      ).toBeInTheDocument();

      const newIndicatorsTable = screen.getByRole('table', {
        name: 'Table showing new indicators',
      });

      const newIndicatorRows = within(newIndicatorsTable).getAllByRole('row');
      expect(newIndicatorRows).toHaveLength(2);
      const newIndicatorCells = within(newIndicatorRows[1]).getAllByRole(
        'cell',
      );
      expect(within(newIndicatorCells[0]).getByText('Not applicable'));
      expect(within(newIndicatorCells[1]).getByText('Indicator 6 new'));

      await user.click(
        within(unmappedIndicatorRow).getByRole('button', {
          name: 'Map indicator for Indicator 4',
        }),
      );

      expect(
        await screen.findByText('Map existing indicator'),
      ).toBeInTheDocument();

      const modal = within(screen.getByRole('dialog'));

      await user.click(modal.getByLabelText('Indicator 6 new'));

      expect(
        apiDataSetVersionService.updateIndicatorsMapping,
      ).not.toHaveBeenCalled();

      await user.click(
        modal.getByRole('button', { name: 'Update indicator mapping' }),
      );

      await waitFor(() =>
        expect(
          screen.queryByText('Map existing indicator'),
        ).not.toBeInTheDocument(),
      );

      act(() => jest.runAllTimers());

      await waitFor(() => {
        expect(
          apiDataSetVersionService.updateIndicatorsMapping,
        ).toHaveBeenCalledWith('draft-version-id', {
          updates: [
            {
              candidateKey: 'Indicator6New',
              sourceKey: 'Indicator4Deleted',
              type: 'ManualMapped',
            },
          ],
        });

        expect(
          screen.getByRole('table', {
            name: 'Indicators that require mapping 4 mapped indicators',
          }),
        ).toBeInTheDocument();
      });

      // Update in mappable table
      expect(within(mappableIndicatorCells[0]).getByText('Indicator 4'));
      expect(within(mappableIndicatorCells[1]).getByText('Indicator 6 new'));
      expect(within(mappableIndicatorCells[2]).getByText('Minor'));

      // Remove from new filter options
      expect(
        screen.getByRole('heading', {
          name: 'Indicators not found in old data set (0) No action required',
        }),
      ).toBeInTheDocument();
      expect(
        screen.queryByTestId('new-items-table-default'),
      ).not.toBeInTheDocument();
    });

    test('changing an unmapped indicator to "no mapping available" via the edit modal', async () => {
      apiDataSetService.getDataSet.mockResolvedValue(testDataSet);
      apiDataSetVersionService.getIndicatorsMapping.mockResolvedValue(
        testIndicatorsMapping,
      );

      const { user } = renderPage();

      expect(await screen.findByText('Data set title')).toBeInTheDocument();

      // Mappable table
      expect(
        screen.getByRole('link', {
          name: 'There is 1 unmapped indicator',
        }),
      ).toHaveAttribute('href', '#mappable-table-default');

      const mappableIndicatorsTable = screen.getByRole('table', {
        name: 'Indicators that require mapping 1 unmapped indicator 3 mapped indicators',
      });
      const currentlyMappedIndicatorRow = within(
        mappableIndicatorsTable,
      ).getAllByRole('row')[1];
      const mappableIndicatorCells = within(
        currentlyMappedIndicatorRow,
      ).getAllByRole('cell');
      expect(within(mappableIndicatorCells[0]).getByText('Indicator 2'));
      expect(
        within(mappableIndicatorCells[1]).getByText(
          'Indicator 2 column name updated',
        ),
      );
      expect(within(mappableIndicatorCells[2]).getByText('Minor'));

      await user.click(
        within(currentlyMappedIndicatorRow).getByRole('button', {
          name: 'Map indicator for Indicator 2',
        }),
      );

      expect(
        await screen.findByText('Map existing indicator'),
      ).toBeInTheDocument();

      const modal = within(screen.getByRole('dialog'));

      await user.click(modal.getByLabelText('No mapping available'));

      expect(
        apiDataSetVersionService.updateIndicatorsMapping,
      ).not.toHaveBeenCalled();

      await user.click(
        modal.getByRole('button', { name: 'Update indicator mapping' }),
      );

      await waitFor(() =>
        expect(
          screen.queryByText('Map existing indicator'),
        ).not.toBeInTheDocument(),
      );

      act(() => {
        jest.runAllTimers();
      });

      await waitFor(() => {
        expect(
          apiDataSetVersionService.updateIndicatorsMapping,
        ).toHaveBeenCalledWith('draft-version-id', {
          updates: [
            {
              sourceKey: 'Indicator2',
              type: 'ManualNone',
            },
          ],
        });

        // Update in mappable table
        expect(
          within(mappableIndicatorCells[1]).getByText('No mapping available'),
        ).toBeInTheDocument();
      });
      expect(within(mappableIndicatorCells[0]).getByText('Indicator 2'));
      expect(within(mappableIndicatorCells[2]).getByText('Major'));
    });

    test('changing an unmapped indicator to "no mapping available" via the row action button', async () => {
      apiDataSetService.getDataSet.mockResolvedValue(testDataSet);
      apiDataSetVersionService.getIndicatorsMapping.mockResolvedValue(
        testIndicatorsMapping,
      );

      const { user } = renderPage();

      expect(await screen.findByText('Data set title')).toBeInTheDocument();

      // Mappable table
      expect(
        screen.getByRole('link', {
          name: 'There is 1 unmapped indicator',
        }),
      ).toHaveAttribute('href', '#mappable-table-default');

      const mappableIndicatorsTable = screen.getByRole('table', {
        name: 'Indicators that require mapping 1 unmapped indicator 3 mapped indicators',
      });
      const unmappedIndicatorRow = within(mappableIndicatorsTable).getAllByRole(
        'row',
      )[3];
      const mappableIndicatorCells =
        within(unmappedIndicatorRow).getAllByRole('cell');
      expect(within(mappableIndicatorCells[0]).getByText('Indicator 4'));
      expect(within(mappableIndicatorCells[1]).getByText('Unmapped'));
      expect(within(mappableIndicatorCells[2]).getByText('N/A'));

      expect(
        apiDataSetVersionService.updateIndicatorsMapping,
      ).not.toHaveBeenCalled();

      await user.click(
        within(unmappedIndicatorRow).getByRole('button', {
          name: 'No mapping for Indicator 4',
        }),
      );

      act(() => {
        jest.runAllTimers();
      });

      await waitFor(() => {
        expect(
          apiDataSetVersionService.updateIndicatorsMapping,
        ).toHaveBeenCalledWith('draft-version-id', {
          updates: [
            {
              sourceKey: 'Indicator4Deleted',
              type: 'ManualNone',
            },
          ],
        });

        expect(
          within(mappableIndicatorCells[1]).getByText('No mapping available'),
        );
      });

      // Update in mappable table
      expect(within(mappableIndicatorCells[0]).getByText('Indicator 4'));
      expect(
        within(mappableIndicatorCells[1]).getByText('No mapping available'),
      );
      expect(within(mappableIndicatorCells[2]).getByText('Major'));
    });

    test('changing an auto mapped indicator to "no mapping available"', async () => {
      apiDataSetService.getDataSet.mockResolvedValue(testDataSet);
      apiDataSetVersionService.getIndicatorsMapping.mockResolvedValue(
        testIndicatorsMapping,
      );

      const { user } = renderPage();

      expect(await screen.findByText('Data set title')).toBeInTheDocument();

      // Auto-mapped table
      expect(
        screen.getByRole('heading', {
          name: 'Indicators found in both (1) No action required',
        }),
      ).toBeInTheDocument();
      const autoMappedAccordion = within(
        screen.getByTestId('auto-mapped-indicators-accordion'),
      );
      expect(
        autoMappedAccordion.getByRole('heading', {
          name: /Mapped indicators These indicators have been mapped automatically. There is no action required./,
        }),
      ).toBeInTheDocument();
      const autoMappedTable = screen.getByTestId('auto-mapped-table-default');
      const autoMappedIndicator =
        within(autoMappedTable).getAllByRole('row')[1];
      const autoMappedIndicatorCells =
        within(autoMappedIndicator).getAllByRole('cell');
      expect(within(autoMappedIndicatorCells[0]).getByText('Indicator 1'));
      expect(
        within(autoMappedIndicatorCells[1]).getByText(
          'Indicator 1 label updated',
        ),
      );
      expect(within(autoMappedIndicatorCells[2]).getByText('Minor'));

      // Mappable table
      expect(
        screen.getByRole('link', {
          name: 'There is 1 unmapped indicator',
        }),
      ).toHaveAttribute('href', '#mappable-table-default');

      const mappableIndicatorsTable = screen.getByRole('table', {
        name: 'Indicators that require mapping 1 unmapped indicator 3 mapped indicators',
      });
      expect(within(mappableIndicatorsTable).getAllByRole('row')).toHaveLength(
        5,
      );

      // New indicators table
      expect(
        screen.getByRole('heading', {
          name: 'Indicators not found in old data set (1) No action required',
        }),
      ).toBeInTheDocument();

      const newIndicatorsTable = screen.getByRole('table', {
        name: 'Table showing new indicators',
      });

      expect(within(newIndicatorsTable).getAllByRole('row')).toHaveLength(2);

      await user.click(
        within(autoMappedIndicator).getByRole('button', {
          name: 'Map indicator for Indicator 1',
        }),
      );

      expect(
        await screen.findByText('Map existing indicator'),
      ).toBeInTheDocument();

      const modal = within(screen.getByRole('dialog'));

      await user.click(modal.getByLabelText('No mapping available'));

      expect(
        apiDataSetVersionService.updateIndicatorsMapping,
      ).not.toHaveBeenCalled();

      await user.click(
        modal.getByRole('button', { name: 'Update indicator mapping' }),
      );

      await waitFor(() =>
        expect(
          screen.queryByText('Map existing indicator'),
        ).not.toBeInTheDocument(),
      );

      act(() => {
        jest.runAllTimers();
      });

      await waitFor(() => {
        expect(
          apiDataSetVersionService.updateIndicatorsMapping,
        ).toHaveBeenCalledWith('draft-version-id', {
          updates: [
            {
              sourceKey: 'Indicator1',
              type: 'ManualNone',
            },
          ],
        });

        expect(
          screen.getByRole('heading', {
            name: 'Indicators found in both (0) No action required',
          }),
        ).toBeInTheDocument();
      });

      // Add to mappable table
      const mappableRows = within(mappableIndicatorsTable).getAllByRole('row');
      expect(mappableRows).toHaveLength(6);
      const mappableIndicatorCells = within(mappableRows[5]).getAllByRole(
        'cell',
      );
      expect(within(mappableIndicatorCells[0]).getByText('Indicator 1'));
      expect(
        within(mappableIndicatorCells[1]).getByText('No mapping available'),
      );
      expect(within(mappableIndicatorCells[2]).getByText('Major'));

      // Add to new indicators table
      expect(
        screen.getByRole('heading', {
          name: 'Indicators not found in old data set (2) No action required',
        }),
      ).toBeInTheDocument();

      const newIndicatorRows = within(newIndicatorsTable).getAllByRole('row');
      expect(newIndicatorRows).toHaveLength(3);
      const newIndicatorCells = within(newIndicatorRows[2]).getAllByRole(
        'cell',
      );
      expect(within(newIndicatorCells[0]).getByText('Not applicable'));
      expect(
        within(newIndicatorCells[1]).getByText('Indicator 1 label updated'),
      );
    });

    test('changing the mapping on an auto mapped indicator"', async () => {
      apiDataSetService.getDataSet.mockResolvedValue(testDataSet);
      apiDataSetVersionService.getIndicatorsMapping.mockResolvedValue(
        testIndicatorsMapping,
      );

      const { user } = renderPage();

      expect(await screen.findByText('Data set title')).toBeInTheDocument();

      // Auto-mapped table
      expect(
        screen.getByRole('heading', {
          name: 'Indicators found in both (1) No action required',
        }),
      ).toBeInTheDocument();
      const autoMappedAccordion = within(
        screen.getByTestId('auto-mapped-indicators-accordion'),
      );
      expect(
        autoMappedAccordion.getByRole('heading', {
          name: /Mapped indicators These indicators have been mapped automatically. There is no action required./,
        }),
      ).toBeInTheDocument();
      const autoMappedTable = screen.getByTestId('auto-mapped-table-default');
      const autoMappedIndicator =
        within(autoMappedTable).getAllByRole('row')[1];
      const autoMappedIndicatorCells =
        within(autoMappedIndicator).getAllByRole('cell');
      expect(within(autoMappedIndicatorCells[0]).getByText('Indicator 1'));
      expect(
        within(autoMappedIndicatorCells[1]).getByText(
          'Indicator 1 label updated',
        ),
      );
      expect(within(autoMappedIndicatorCells[2]).getByText('Minor'));

      // Mappable table
      expect(
        screen.getByRole('link', {
          name: 'There is 1 unmapped indicator',
        }),
      ).toHaveAttribute('href', '#mappable-table-default');

      const mappableIndicatorsTable = screen.getByRole('table', {
        name: 'Indicators that require mapping 1 unmapped indicator 3 mapped indicators',
      });
      expect(within(mappableIndicatorsTable).getAllByRole('row')).toHaveLength(
        5,
      );

      await user.click(
        within(autoMappedIndicator).getByRole('button', {
          name: 'Map indicator for Indicator 1',
        }),
      );

      expect(
        await screen.findByText('Map existing indicator'),
      ).toBeInTheDocument();

      const modal = within(screen.getByRole('dialog'));

      await user.click(modal.getByLabelText('Indicator 6 new'));

      expect(
        apiDataSetVersionService.updateIndicatorsMapping,
      ).not.toHaveBeenCalled();

      await user.click(
        modal.getByRole('button', { name: 'Update indicator mapping' }),
      );

      await waitFor(() =>
        expect(
          screen.queryByText('Map existing indicator'),
        ).not.toBeInTheDocument(),
      );

      act(() => {
        jest.runAllTimers();
      });

      await waitFor(() => {
        expect(
          apiDataSetVersionService.updateIndicatorsMapping,
        ).toHaveBeenCalledWith('draft-version-id', {
          updates: [
            {
              candidateKey: 'Indicator6New',
              sourceKey: 'Indicator1',
              type: 'ManualMapped',
            },
          ],
        });

        expect(
          screen.getByText('Indicators found in both (0)'),
        ).toBeInTheDocument();
      });

      // Add to mappable table
      const mappableRows = within(mappableIndicatorsTable).getAllByRole('row');
      expect(mappableRows).toHaveLength(6);
      const mappableIndicatorCells = within(mappableRows[5]).getAllByRole(
        'cell',
      );
      expect(within(mappableIndicatorCells[0]).getByText('Indicator 1'));
      expect(within(mappableIndicatorCells[1]).getByText('Indicator 6 new'));
      expect(within(mappableIndicatorCells[2]).getByText('Minor'));

      // Remove from New indicators table
      expect(
        screen.getByRole('heading', {
          name: 'Indicators not found in old data set (0) No action required',
        }),
      ).toBeInTheDocument();
    });

    test('batches updates made in quick succession', async () => {
      apiDataSetService.getDataSet.mockResolvedValue(testDataSet);
      apiDataSetVersionService.getIndicatorsMapping.mockResolvedValue(
        testIndicatorsMapping,
      );

      const { user } = renderPage();

      expect(await screen.findByText('Data set title')).toBeInTheDocument();

      expect(
        screen.getByRole('link', {
          name: 'There is 1 unmapped indicator',
        }),
      ).toHaveAttribute('href', '#mappable-table-default');

      // Mappable table
      const mappableIndicatorsTable = screen.getByRole('table', {
        name: 'Indicators that require mapping 1 unmapped indicator 3 mapped indicators',
      });
      const mappedIndicator1Row = within(mappableIndicatorsTable).getAllByRole(
        'row',
      )[1];
      const mappableIndicator1Cells =
        within(mappedIndicator1Row).getAllByRole('cell');
      expect(within(mappableIndicator1Cells[0]).getByText('Indicator 2'));
      expect(
        within(mappableIndicator1Cells[1]).getByText(
          'Indicator 2 column name updated',
        ),
      );
      expect(within(mappableIndicator1Cells[2]).getByText('Minor'));

      const mappedIndicator2Row = within(mappableIndicatorsTable).getAllByRole(
        'row',
      )[2];
      const mappableIndicator2Cells =
        within(mappedIndicator2Row).getAllByRole('cell');
      expect(within(mappableIndicator2Cells[0]).getByText('Indicator 3'));
      expect(
        within(mappableIndicator2Cells[1]).getByText(
          'Indicator 3 column name updated',
        ),
      );
      expect(within(mappableIndicator2Cells[2]).getByText('Minor'));

      expect(
        apiDataSetVersionService.updateIndicatorsMapping,
      ).not.toHaveBeenCalled();

      await user.click(
        within(mappedIndicator1Row).getByRole('button', {
          name: 'No mapping for Indicator 2',
        }),
      );
      await user.click(
        within(mappedIndicator2Row).getByRole('button', {
          name: 'No mapping for Indicator 3',
        }),
      );

      act(() => {
        jest.runAllTimers();
      });

      await waitFor(() => {
        expect(
          apiDataSetVersionService.updateIndicatorsMapping,
        ).toHaveBeenCalledWith('draft-version-id', {
          updates: [
            {
              sourceKey: 'Indicator2',
              type: 'ManualNone',
            },
            {
              sourceKey: 'Indicator3',
              type: 'ManualNone',
            },
          ],
        });

        expect(
          within(mappableIndicator1Cells[1]).getByText('No mapping available'),
        ).toBeInTheDocument();
      });

      // Update in mappable table
      expect(within(mappableIndicator1Cells[0]).getByText('Indicator 2'));
      expect(
        within(mappableIndicator1Cells[1]).getByText('No mapping available'),
      );
      expect(within(mappableIndicator1Cells[2]).getByText('Major'));

      expect(within(mappableIndicator2Cells[0]).getByText('Indicator 3'));
      expect(
        within(mappableIndicator2Cells[1]).getByText('No mapping available'),
      );
      expect(within(mappableIndicator2Cells[2]).getByText('Major'));
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
                releaseApiDataSetIndicatorsMappingRoute.path,
                {
                  publicationId: releaseVersion.publicationId,
                  releaseVersionId: releaseVersion.id,
                  dataSetId,
                },
              ),
            ]}
          >
            <Route
              component={ReleaseApiDataSetIndicatorsMappingPage}
              path={releaseApiDataSetIndicatorsMappingRoute.path}
            />
          </MemoryRouter>
        </ReleaseVersionContextProvider>
      </TestConfigContextProvider>,
    );
  }
});
