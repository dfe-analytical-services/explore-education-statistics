import { testTableData } from '@admin/pages/release/datablocks/__data__/tableToolServiceData';
import {
  ChartBuilderForms,
  ChartBuilderFormsContextProvider,
} from '@admin/pages/release/datablocks/components/chart/contexts/ChartBuilderFormsContext';
import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';
import mapFullTableMeta from '@common/modules/table-tool/utils/mapFullTableMeta';
import mapFullTable from '@common/modules/table-tool/utils/mapFullTable';
import { TableDataSubjectMeta } from '@common/services/tableBuilderService';
import { render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import noop from 'lodash/noop';
import React from 'react';
import ChartBuilder from '../ChartBuilder';

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

  const testTable = mapFullTable(testTableData);
  const testMeta: TableDataSubjectMeta = {
    publicationName: '',
    subjectName: '',
    geoJsonAvailable: false,
    footnotes: [],
    boundaryLevels: [],
    locations: {
      localAuthority: [
        { id: 'barnet', label: 'Barnet', value: 'barnet' },
        { id: 'barnsley', label: 'Barnsley', value: 'barnsley' },
      ],
    },
    timePeriodRange: [
      {
        label: '2019/20',
        year: 2019,
        code: 'AY',
      },
      {
        label: '2020/21',
        year: 2020,
        code: 'AY',
      },
    ],
    indicators: [
      {
        value: 'authorised-absence-sessions',
        label: 'Number of authorised absence sessions',
        unit: '',
        name: 'sess_authorised',
        decimalPlaces: 2,
      },
      {
        value: 'unauthorised-absence-sessions',
        label: 'Number of unauthorised absence sessions',
        unit: '',
        name: 'sess_unauthorised',
        decimalPlaces: 2,
      },
    ],
    filters: {
      Characteristic: {
        legend: 'Characteristic',
        name: 'characteristic',
        options: {
          gender: {
            id: 'gender',
            label: 'Gender',
            options: [
              {
                value: 'male',
                label: 'Male',
              },
              {
                value: 'female',
                label: 'Female',
              },
            ],
            order: 0,
          },
        },
        order: 0,
      },
    },
  };
  const testSubjectMeta: FullTableMeta = mapFullTableMeta(testMeta);

  test('renders the chart type selector only when no initial value', () => {
    render(
      <ChartBuilderFormsContextProvider initialForms={testFormState}>
        <ChartBuilder
          releaseId="release-1"
          data={testTable.results}
          meta={testSubjectMeta}
          tableTitle="Table title"
          onChartSave={jest.fn(() => Promise.resolve())}
          onChartDelete={noop}
          onTableQueryUpdate={jest.fn(() => Promise.resolve())}
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

  test('renders the chart preview and tabs, with configuration selected, when a chart type is selected', () => {
    render(
      <ChartBuilderFormsContextProvider initialForms={testFormState}>
        <ChartBuilder
          releaseId="release-1"
          data={testTable.results}
          meta={testSubjectMeta}
          tableTitle="Table title"
          onChartSave={jest.fn(() => Promise.resolve())}
          onChartDelete={noop}
          onTableQueryUpdate={jest.fn(() => Promise.resolve())}
        />
      </ChartBuilderFormsContextProvider>,
    );

    userEvent.click(screen.getByRole('button', { name: 'Line' }));

    expect(
      screen.getByRole('button', { name: 'Chart preview' }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('tab', { name: 'Chart configuration' }),
    ).toBeInTheDocument();
    expect(screen.getByRole('tab', { name: 'Data sets' })).toBeInTheDocument();
    expect(screen.getByRole('tab', { name: 'Legend' })).toBeInTheDocument();
    expect(
      screen.getByRole('tab', { name: 'X Axis (major axis)' }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('tab', { name: 'Y Axis (minor axis)' }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('heading', { name: 'Chart configuration' }),
    ).toBeInTheDocument();
  });

  describe('data sets', () => {
    test('adding data sets', async () => {
      render(
        <ChartBuilderFormsContextProvider initialForms={testFormState}>
          <ChartBuilder
            releaseId="release-1"
            data={testTable.results}
            meta={testSubjectMeta}
            tableTitle="Table title"
            onChartSave={jest.fn(() => Promise.resolve())}
            onChartDelete={noop}
            onTableQueryUpdate={jest.fn(() => Promise.resolve())}
          />
        </ChartBuilderFormsContextProvider>,
      );

      userEvent.click(screen.getByRole('button', { name: 'Line' }));

      userEvent.click(screen.getByRole('tab', { name: 'Data sets' }));

      expect(
        screen.getByRole('heading', { name: 'Data sets' }),
      ).toBeInTheDocument();

      userEvent.selectOptions(screen.getByLabelText('Characteristic'), 'male');
      userEvent.selectOptions(
        screen.getByLabelText('Indicator'),
        'unauthorised-absence-sessions',
      );

      userEvent.click(screen.getByRole('button', { name: 'Add data set' }));

      await waitFor(() => {
        expect(screen.getByText('Remove all')).toBeInTheDocument();
      });

      const tableRows = screen.getAllByRole('row');
      expect(tableRows).toHaveLength(2);
      expect(tableRows[1]).toHaveTextContent(
        'Number of unauthorised absence sessions (Male, All locations, All time periods)',
      );

      userEvent.selectOptions(
        screen.getByLabelText('Characteristic'),
        'female',
      );
      userEvent.selectOptions(
        screen.getByLabelText('Indicator'),
        'unauthorised-absence-sessions',
      );

      userEvent.click(screen.getByRole('button', { name: 'Add data set' }));

      await waitFor(() => {
        expect(screen.getByText('Reorder data sets')).toBeInTheDocument();
      });

      const updatedTableRows = screen.getAllByRole('row');
      expect(updatedTableRows).toHaveLength(3);
      expect(updatedTableRows[2]).toHaveTextContent(
        'Number of unauthorised absence sessions (Female, All locations, All time periods)',
      );
    });

    test('removing data sets', async () => {
      render(
        <ChartBuilderFormsContextProvider initialForms={testFormState}>
          <ChartBuilder
            releaseId="release-1"
            data={testTable.results}
            meta={testSubjectMeta}
            tableTitle="Table title"
            onChartSave={jest.fn(() => Promise.resolve())}
            onChartDelete={noop}
            onTableQueryUpdate={jest.fn(() => Promise.resolve())}
          />
        </ChartBuilderFormsContextProvider>,
      );

      userEvent.click(screen.getByRole('button', { name: 'Line' }));

      userEvent.click(screen.getByRole('tab', { name: 'Data sets' }));

      expect(
        screen.getByRole('heading', { name: 'Data sets' }),
      ).toBeInTheDocument();

      userEvent.click(screen.getByRole('button', { name: 'Add data set' }));

      await waitFor(() => {
        expect(screen.getByText('Remove all')).toBeInTheDocument();
      });

      const tableRows = screen.getAllByRole('row');
      expect(tableRows).toHaveLength(5);
      expect(tableRows[1]).toHaveTextContent(
        'Number of authorised absence sessions (Male, All locations, All time periods)',
      );
      expect(tableRows[2]).toHaveTextContent(
        'Number of unauthorised absence sessions (Male, All locations, All time periods)',
      );
      expect(tableRows[3]).toHaveTextContent(
        'Number of authorised absence sessions (Female, All locations, All time periods)',
      );
      expect(tableRows[4]).toHaveTextContent(
        'Number of unauthorised absence sessions (Female, All locations, All time periods)',
      );

      userEvent.click(
        within(tableRows[2]).getByRole('button', { name: 'Remove' }),
      );

      await waitFor(() => {
        expect(
          screen.queryByText(
            'Number of unauthorised absence sessions (Male, All locations, All time periods)',
          ),
        ).not.toBeInTheDocument();
      });

      userEvent.click(
        screen.getByRole('button', { name: 'Remove all data sets' }),
      );

      const modal = within(screen.getByRole('dialog'));
      expect(modal.getByText('Remove all data sets')).toBeInTheDocument();
      userEvent.click(modal.getByRole('button', { name: 'Confirm' }));

      await waitFor(() => {
        expect(
          screen.queryByText(
            'Number of authorised absence sessions (Male, All locations, All time periods)',
          ),
        ).not.toBeInTheDocument();
        expect(
          screen.queryByText(
            'Number of authorised absence sessions (Female, All locations, All time periods)',
          ),
        ).not.toBeInTheDocument();
        expect(
          screen.queryByText(
            'Number of unauthorised absence sessions (Female, All locations, All time periods)',
          ),
        ).not.toBeInTheDocument();
      });
    });
  });
});
