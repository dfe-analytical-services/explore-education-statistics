import { testFullTable } from '@admin/pages/release/datablocks/components/chart/__tests__/__data__/testTableData';
import ChartBuilder from '@admin/pages/release/datablocks/components/chart/ChartBuilder';
import {
  ChartBuilderForms,
  ChartBuilderFormsContextProvider,
} from '@admin/pages/release/datablocks/components/chart/contexts/ChartBuilderFormsContext';
import render from '@common-test/render';
import { RefContext } from '@common/contexts/RefContext';
import { Chart } from '@common/modules/charts/types/chart';
import { screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import noop from 'lodash/noop';
import React from 'react';

describe('ChartBuilder', () => {
  const testFormState: ChartBuilderForms = {
    options: {
      isValid: true,
      submitCount: 0,
      id: 'options',
      title: 'Options',
    },
    dataSets: {
      isValid: true,
      submitCount: 0,
      id: 'dataSets',
      title: 'Data sets',
    },
  };

  test('renders the chart type selector only when no initial value', () => {
    render(
      <ChartBuilderFormsContextProvider initialForms={testFormState}>
        <ChartBuilder
          releaseVersionId="release-1"
          data={testFullTable.results}
          meta={testFullTable.subjectMeta}
          tableTitle="Table title"
          onChartSave={jest.fn()}
          onChartDelete={noop}
          onTableQueryUpdate={jest.fn()}
        />
      </ChartBuilderFormsContextProvider>,
    );

    expect(screen.getByText('Choose chart type')).toBeInTheDocument();

    expect(screen.getByRole('button', { name: 'Line' })).toBeInTheDocument();

    expect(
      screen.getByRole('button', { name: 'Vertical bar' }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('button', { name: 'Horizontal bar' }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('button', {
        name: 'Choose an infographic as alternative',
      }),
    ).toBeInTheDocument();

    expect(
      screen.queryByRole('button', { name: 'Chart preview' }),
    ).not.toBeInTheDocument();
  });

  test('renders the chart preview and tabs, with configuration selected, when a chart type is selected', async () => {
    render(
      <ChartBuilderFormsContextProvider initialForms={testFormState}>
        <ChartBuilder
          releaseVersionId="release-1"
          data={testFullTable.results}
          meta={testFullTable.subjectMeta}
          tableTitle="Table title"
          onChartSave={jest.fn()}
          onChartDelete={noop}
          onTableQueryUpdate={jest.fn()}
        />
      </ChartBuilderFormsContextProvider>,
    );

    await userEvent.click(screen.getByRole('button', { name: 'Line' }));

    expect(
      screen.getByRole('button', { name: 'Chart preview' }),
    ).toBeInTheDocument();

    const tabs = screen.getAllByRole('tab');

    expect(tabs).toHaveLength(5);

    expect(tabs[0]).toHaveTextContent('Chart configuration');
    expect(tabs[0]).toHaveAttribute('aria-selected', 'true');

    expect(tabs[1]).toHaveTextContent('Data sets');
    expect(tabs[1]).toHaveAttribute('aria-selected', 'false');

    expect(tabs[2]).toHaveTextContent('Legend');
    expect(tabs[2]).toHaveAttribute('aria-selected', 'false');

    expect(tabs[3]).toHaveTextContent('X Axis (major axis)');
    expect(tabs[3]).toHaveAttribute('aria-selected', 'false');

    expect(tabs[4]).toHaveTextContent('Y Axis (minor axis)');
    expect(tabs[4]).toHaveAttribute('aria-selected', 'false');

    expect(
      screen.getByRole('heading', { name: 'Chart configuration' }),
    ).toBeInTheDocument();
  });

  describe('data sets', () => {
    test('adding data sets', async () => {
      const myMockRef = { current: null } as React.MutableRefObject<null>;

      const { user } = render(
        <RefContext.Provider value={myMockRef}>
          <ChartBuilderFormsContextProvider initialForms={testFormState}>
            <ChartBuilder
              releaseVersionId="release-1"
              data={testFullTable.results}
              meta={testFullTable.subjectMeta}
              tableTitle="Table title"
              onChartSave={jest.fn()}
              onChartDelete={noop}
              onTableQueryUpdate={jest.fn()}
            />
          </ChartBuilderFormsContextProvider>
        </RefContext.Provider>,
      );

      await user.click(screen.getByRole('button', { name: 'Line' }));

      expect(await screen.findByText('Chart preview')).toBeInTheDocument();

      await user.click(screen.getByRole('tab', { name: 'Data sets' }));

      await user.selectOptions(
        screen.getByLabelText('Characteristic'),
        'ethnicity-major-chinese',
      );
      await user.selectOptions(
        screen.getByLabelText('School type'),
        'state-funded-primary',
      );
      await user.selectOptions(
        screen.getByLabelText('Indicator'),
        'authorised-absence-sessions',
      );
      await user.selectOptions(
        screen.getByLabelText('Location'),
        '{"level":"localAuthority","value":"barnet"}',
      );
      await user.selectOptions(screen.getByLabelText('Time period'), '2014_AY');

      await user.click(screen.getByRole('button', { name: 'Add data set' }));

      expect(await screen.findByText('Remove all')).toBeInTheDocument();

      const tableRows = screen.getAllByRole('row');
      expect(tableRows).toHaveLength(2);
      expect(tableRows[1]).toHaveTextContent(
        'Number of authorised absence sessions (Ethnicity Major Chinese, State-funded primary, Barnet, 2014/15)',
      );

      await user.selectOptions(
        screen.getByLabelText('Characteristic'),
        'ethnicity-major-chinese',
      );
      await user.selectOptions(
        screen.getByLabelText('School type'),
        'state-funded-secondary',
      );
      await user.selectOptions(
        screen.getByLabelText('Indicator'),
        'authorised-absence-sessions',
      );
      await user.selectOptions(
        screen.getByLabelText('Location'),
        '{"level":"localAuthority","value":"barnet"}',
      );
      await user.selectOptions(screen.getByLabelText('Time period'), '2014_AY');

      await user.click(screen.getByRole('button', { name: 'Add data set' }));

      expect(await screen.findByText('Reorder data sets')).toBeInTheDocument();

      const updatedTableRows = screen.getAllByRole('row');
      expect(updatedTableRows).toHaveLength(3);
      expect(updatedTableRows[2]).toHaveTextContent(
        'Number of authorised absence sessions (Ethnicity Major Chinese, State-funded secondary, Barnet, 2014/15)',
      );
    });

    test('removing a data set', async () => {
      const myMockRef = { current: null } as React.MutableRefObject<null>;

      render(
        <RefContext.Provider value={myMockRef}>
          <ChartBuilderFormsContextProvider initialForms={testFormState}>
            <ChartBuilder
              releaseVersionId="release-1"
              data={testFullTable.results}
              meta={testFullTable.subjectMeta}
              tableTitle="Table title"
              onChartSave={jest.fn()}
              onChartDelete={noop}
              onTableQueryUpdate={jest.fn()}
            />
          </ChartBuilderFormsContextProvider>
        </RefContext.Provider>,
      );

      await userEvent.click(screen.getByRole('button', { name: 'Line' }));

      await waitFor(() => {
        expect(
          screen.getByText('Data sets', {
            selector: '[role="tab"]',
          }),
        ).toBeInTheDocument();
      });

      await userEvent.click(screen.getByRole('tab', { name: 'Data sets' }));

      expect(
        screen.getByRole('heading', { name: 'Data sets' }),
      ).toBeInTheDocument();

      await userEvent.selectOptions(
        screen.getByLabelText('Indicator'),
        'authorised-absence-sessions',
      );
      await userEvent.selectOptions(
        screen.getByLabelText('Time period'),
        '2014_AY',
      );

      await userEvent.click(
        screen.getByRole('button', { name: 'Add data set' }),
      );

      await waitFor(() => {
        expect(screen.getByText('Remove all')).toBeInTheDocument();
      });

      const tableRows = screen.getAllByRole('row');
      expect(tableRows).toHaveLength(5);
      expect(tableRows[1]).toHaveTextContent(
        'Number of authorised absence sessions (Ethnicity Major Chinese, State-funded primary, All locations, 2014/15)',
      );
      expect(tableRows[2]).toHaveTextContent(
        'Number of authorised absence sessions (Ethnicity Major Chinese, State-funded secondary, All locations, 2014/15)',
      );
      expect(tableRows[3]).toHaveTextContent(
        'Number of authorised absence sessions (Ethnicity Major Black Total, State-funded primary, All locations, 2014/15)',
      );
      expect(tableRows[4]).toHaveTextContent(
        'Number of authorised absence sessions (Ethnicity Major Black Total, State-funded secondary, All locations, 2014/15)',
      );

      await userEvent.click(
        within(tableRows[2]).getByRole('button', { name: 'Remove' }),
      );

      await waitFor(() => {
        expect(
          screen.queryByText(
            'Number of authorised absence sessions (Ethnicity Major Chinese, State-funded secondary, All locations, 2014/15)',
          ),
        ).not.toBeInTheDocument();
      });
      expect(screen.getAllByRole('row')).toHaveLength(4);
    });

    test('removing all data sets', async () => {
      const myMockRef = { current: null } as React.MutableRefObject<null>;

      const { user } = render(
        <RefContext.Provider value={myMockRef}>
          <ChartBuilderFormsContextProvider initialForms={testFormState}>
            <ChartBuilder
              releaseVersionId="release-1"
              data={testFullTable.results}
              meta={testFullTable.subjectMeta}
              tableTitle="Table title"
              onChartSave={jest.fn()}
              onChartDelete={noop}
              onTableQueryUpdate={jest.fn()}
            />
          </ChartBuilderFormsContextProvider>
        </RefContext.Provider>,
      );

      await user.click(screen.getByRole('button', { name: 'Line' }));

      await waitFor(() => {
        expect(
          screen.getByText('Data sets', {
            selector: '[role="tab"]',
          }),
        ).toBeInTheDocument();
      });

      await user.click(screen.getByRole('tab', { name: 'Data sets' }));

      expect(
        screen.getByRole('heading', { name: 'Data sets' }),
      ).toBeInTheDocument();

      await user.selectOptions(
        screen.getByLabelText('Indicator'),
        'authorised-absence-sessions',
      );
      await user.selectOptions(screen.getByLabelText('Time period'), '2014_AY');

      await user.click(screen.getByRole('button', { name: 'Add data set' }));

      await waitFor(() => {
        expect(screen.getByText('Remove all')).toBeInTheDocument();
      });

      const tableRows = screen.getAllByRole('row');
      expect(tableRows).toHaveLength(5);
      expect(tableRows[1]).toHaveTextContent(
        'Number of authorised absence sessions (Ethnicity Major Chinese, State-funded primary, All locations, 2014/15)',
      );
      expect(tableRows[2]).toHaveTextContent(
        'Number of authorised absence sessions (Ethnicity Major Chinese, State-funded secondary, All locations, 2014/15)',
      );
      expect(tableRows[3]).toHaveTextContent(
        'Number of authorised absence sessions (Ethnicity Major Black Total, State-funded primary, All locations, 2014/15)',
      );
      expect(tableRows[4]).toHaveTextContent(
        'Number of authorised absence sessions (Ethnicity Major Black Total, State-funded secondary, All locations, 2014/15)',
      );

      await user.click(
        screen.getByRole('button', { name: 'Remove all data sets' }),
      );

      const modal = within(screen.getByRole('dialog'));
      expect(modal.getByText('Remove all data sets')).toBeInTheDocument();
      await user.click(modal.getByRole('button', { name: 'Confirm' }));

      await waitFor(() => {
        expect(
          screen.queryByText(
            'Number of authorised absence sessions (Ethnicity Major Chinese, State-funded primary, All locations, 2014/15)',
          ),
        ).not.toBeInTheDocument();
        expect(
          screen.queryByText(
            'Number of authorised absence sessions (Ethnicity Major Chinese, State-funded secondary, All locations, 2014/15)',
          ),
        ).not.toBeInTheDocument();
        expect(
          screen.queryByText(
            'Number of authorised absence sessions (Ethnicity Major Black Total, State-funded primary, All locations, 2014/15)',
          ),
        ).not.toBeInTheDocument();
        expect(
          screen.queryByText(
            'Number of authorised absence sessions (Ethnicity Major Black Total, State-funded secondary, All locations, 2014/15)',
          ),
        ).not.toBeInTheDocument();
      });

      expect(screen.getAllByRole('row')).toHaveLength(1);
    });
  });

  test('calls `onTableQueryUpdate` when change boundary level', async () => {
    const testInitialChart: Chart = {
      type: 'map',
      boundaryLevel: 2,
      map: {
        dataSetConfigs: [],
      },
      title: 'Data block title',
      subtitle: '',
      alt: 'd',
      height: 600,
      includeNonNumericData: false,
      axes: {
        major: {
          type: 'major',
          groupBy: 'locations',
          groupByFilter: '',
          groupByFilterGroups: false,
          sortBy: 'name',
          sortAsc: false,
          dataSets: [
            {
              order: 0,
              indicator: 'overall-absence-sessions',
              filters: ['state-funded-primary'],
              timePeriod: '2014_AY',
            },
          ],
          referenceLines: [],
          visible: true,
          unit: '',
          showGrid: false,
          label: {
            text: '',
            rotated: false,
          },
          min: 0,
          size: 50,
          tickConfig: 'default',
          tickSpacing: 1,
        },
      },
      legend: { items: [] },
    };

    const handleUpdate = jest.fn();
    const myMockRef = { current: null } as React.MutableRefObject<null>;

    const { user } = render(
      <RefContext.Provider value={myMockRef}>
        <ChartBuilderFormsContextProvider initialForms={testFormState}>
          <ChartBuilder
            releaseVersionId="release-1"
            data={testFullTable.results}
            initialChart={testInitialChart}
            meta={{
              ...testFullTable.subjectMeta,
              boundaryLevels: [
                {
                  id: 1,
                  label: 'Boundary level 1',
                },
                {
                  id: 2,
                  label: 'Boundary level 2',
                },
              ],
            }}
            tableTitle="Table title"
            onChartSave={jest.fn()}
            onChartDelete={noop}
            onTableQueryUpdate={handleUpdate}
          />
        </ChartBuilderFormsContextProvider>
      </RefContext.Provider>,
    );

    expect(
      screen.getByRole('button', { name: 'Chart preview' }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('tab', { name: 'Boundary levels' }),
    ).toBeInTheDocument();

    await user.click(screen.getByRole('tab', { name: 'Boundary levels' }));

    await user.selectOptions(screen.getByLabelText('Default boundary level'), [
      '1',
    ]);

    expect(handleUpdate).toHaveBeenCalledWith({}, 1);
  });

  describe('data groupings tab', () => {
    const testInitialChart: Chart = {
      type: 'map',
      boundaryLevel: 2,
      legend: {
        items: [
          {
            colour: '#12436D',
            dataSet: {
              filters: ['ethnicity-major-chinese', 'state-funded-primary'],
              indicator: 'authorised-absence-sessions',
              timePeriod: '2014_AY',
            },
            inlinePosition: undefined,
            label:
              'Number of authorised absence sessions (Ethnicity Major Chinese, State-funded primary, 2014/15)',
            lineStyle: undefined,
            symbol: undefined,
          },
          {
            colour: '#F46A25',
            dataSet: {
              filters: ['ethnicity-major-chinese', 'state-funded-primary'],
              indicator: 'authorised-absence-sessions',
              timePeriod: '2015_AY',
            },
            inlinePosition: undefined,
            label:
              'Number of authorised absence sessions (Ethnicity Major Chinese, State-funded primary, 2015/16)',
            lineStyle: undefined,
            symbol: undefined,
          },
        ],
        position: 'bottom',
      },
      map: {
        dataSetConfigs: [
          {
            dataGrouping: {
              customGroups: [],
              numberOfGroups: 5,
              type: 'EqualIntervals',
            },
            dataSet: {
              filters: ['ethnicity-major-chinese', 'state-funded-primary'],
              indicator: 'authorised-absence-sessions',
              timePeriod: '2014_AY',
            },
          },
          {
            dataGrouping: {
              customGroups: [],
              numberOfGroups: 5,
              type: 'EqualIntervals',
            },
            dataSet: {
              filters: ['ethnicity-major-chinese', 'state-funded-primary'],
              indicator: 'authorised-absence-sessions',
              timePeriod: '2015_AY',
            },
          },
        ],
      },
      title: 'Data block title',
      subtitle: '',
      alt: 'd',
      height: 600,
      includeNonNumericData: false,
      axes: {
        major: {
          type: 'major',
          groupBy: 'locations',
          groupByFilter: '',
          groupByFilterGroups: false,
          sortBy: 'name',
          sortAsc: false,
          dataSets: [
            {
              order: 0,
              filters: ['ethnicity-major-chinese', 'state-funded-primary'],
              indicator: 'authorised-absence-sessions',
              timePeriod: '2014_AY',
            },
            {
              order: 1,
              filters: ['ethnicity-major-chinese', 'state-funded-primary'],
              indicator: 'authorised-absence-sessions',
              timePeriod: '2015_AY',
            },
          ],
          referenceLines: [],
          visible: true,
          unit: '',
          showGrid: false,
          label: {
            text: '',
            rotated: false,
          },
          min: 0,
          size: 50,
          tickConfig: 'default',
          tickSpacing: 1,
        },
      },
    };

    test('save chart with updated data groupings', async () => {
      const handleSubmit = jest.fn();
      const myMockRef = { current: null } as React.MutableRefObject<null>;

      const { user } = render(
        <RefContext.Provider value={myMockRef}>
          <ChartBuilderFormsContextProvider initialForms={testFormState}>
            <ChartBuilder
              releaseVersionId="release-1"
              data={testFullTable.results}
              initialChart={testInitialChart}
              meta={{
                ...testFullTable.subjectMeta,
                boundaryLevels: [
                  {
                    id: 1,
                    label: 'Boundary level 1',
                  },
                  {
                    id: 2,
                    label: 'Boundary level 2',
                  },
                ],
              }}
              tableTitle="Table title"
              onChartSave={handleSubmit}
              onChartDelete={noop}
              onTableQueryUpdate={jest.fn()}
            />
          </ChartBuilderFormsContextProvider>
        </RefContext.Provider>,
      );

      expect(
        screen.getByRole('button', { name: 'Chart preview' }),
      ).toBeInTheDocument();

      expect(
        screen.getByRole('tab', { name: 'Data groupings' }),
      ).toBeInTheDocument();

      await user.click(screen.getByRole('tab', { name: 'Data groupings' }));

      expect(await screen.findByRole('heading', { name: 'Data groupings' }));

      await user.click(screen.getAllByRole('button', { name: 'Edit' })[0]);

      expect(await screen.findByRole('dialog')).toBeInTheDocument();
      expect(screen.getByText('Edit groupings')).toBeInTheDocument();

      await user.click(screen.getByLabelText('Quantiles'));
      await user.click(screen.getByRole('button', { name: 'Done' }));

      await user.click(
        screen.getByRole('button', { name: 'Save chart options' }),
      );

      await waitFor(() => {
        expect(handleSubmit).toHaveBeenCalledTimes(1);
        expect(handleSubmit).toHaveBeenCalledWith(
          {
            ...testInitialChart,
            map: {
              dataSetConfigs: [
                {
                  dataGrouping: {
                    customGroups: [],
                    numberOfGroups: 5,
                    type: 'Quantiles',
                  },
                  dataSet: {
                    filters: [
                      'ethnicity-major-chinese',
                      'state-funded-primary',
                    ],
                    indicator: 'authorised-absence-sessions',
                    timePeriod: '2014_AY',
                  },
                },
                {
                  dataGrouping: {
                    customGroups: [],
                    numberOfGroups: 5,
                    type: 'EqualIntervals',
                  },
                  dataSet: {
                    filters: [
                      'ethnicity-major-chinese',
                      'state-funded-primary',
                    ],
                    indicator: 'authorised-absence-sessions',
                    timePeriod: '2015_AY',
                  },
                },
              ],
            },
          },
          undefined,
        );
      });
    });
  });
});
