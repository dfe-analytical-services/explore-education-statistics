import { testFullTable } from '@admin/pages/release/datablocks/components/chart/__tests__/__data__/testTableData';
import {
  ChartBuilderForms,
  ChartBuilderFormsContextProvider,
} from '@admin/pages/release/datablocks/components/chart/contexts/ChartBuilderFormsContext';
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

  test('renders the chart type selector only when no initial value', () => {
    render(
      <ChartBuilderFormsContextProvider initialForms={testFormState}>
        <ChartBuilder
          releaseId="release-1"
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

  test('renders the chart preview and tabs, with configuration selected, when a chart type is selected', () => {
    render(
      <ChartBuilderFormsContextProvider initialForms={testFormState}>
        <ChartBuilder
          releaseId="release-1"
          data={testFullTable.results}
          meta={testFullTable.subjectMeta}
          tableTitle="Table title"
          onChartSave={jest.fn()}
          onChartDelete={noop}
          onTableQueryUpdate={jest.fn()}
        />
      </ChartBuilderFormsContextProvider>,
    );

    userEvent.click(screen.getByRole('button', { name: 'Line' }));

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
      render(
        <ChartBuilderFormsContextProvider initialForms={testFormState}>
          <ChartBuilder
            releaseId="release-1"
            data={testFullTable.results}
            meta={testFullTable.subjectMeta}
            tableTitle="Table title"
            onChartSave={jest.fn()}
            onChartDelete={noop}
            onTableQueryUpdate={jest.fn()}
          />
        </ChartBuilderFormsContextProvider>,
      );

      userEvent.click(screen.getByRole('button', { name: 'Line' }));

      userEvent.click(screen.getByRole('tab', { name: 'Data sets' }));

      expect(
        screen.getByRole('heading', { name: 'Data sets' }),
      ).toBeInTheDocument();

      userEvent.selectOptions(
        screen.getByLabelText('Characteristic'),
        'ethnicity-major-chinese',
      );
      userEvent.selectOptions(
        screen.getByLabelText('School type'),
        'state-funded-primary',
      );
      userEvent.selectOptions(
        screen.getByLabelText('Indicator'),
        'authorised-absence-sessions',
      );
      userEvent.selectOptions(
        screen.getByLabelText('Location'),
        '{"level":"localAuthority","value":"barnet"}',
      );
      userEvent.selectOptions(screen.getByLabelText('Time period'), '2014_AY');

      userEvent.click(screen.getByRole('button', { name: 'Add data set' }));

      await waitFor(() => {
        expect(screen.getByText('Remove all')).toBeInTheDocument();
      });

      const tableRows = screen.getAllByRole('row');
      expect(tableRows).toHaveLength(2);
      expect(tableRows[1]).toHaveTextContent(
        'Number of authorised absence sessions (Ethnicity Major Chinese, State-funded primary, Barnet, 2014/15)',
      );

      userEvent.selectOptions(
        screen.getByLabelText('Characteristic'),
        'ethnicity-major-chinese',
      );
      userEvent.selectOptions(
        screen.getByLabelText('School type'),
        'state-funded-secondary',
      );
      userEvent.selectOptions(
        screen.getByLabelText('Indicator'),
        'authorised-absence-sessions',
      );
      userEvent.selectOptions(
        screen.getByLabelText('Location'),
        '{"level":"localAuthority","value":"barnet"}',
      );
      userEvent.selectOptions(screen.getByLabelText('Time period'), '2014_AY');

      userEvent.click(screen.getByRole('button', { name: 'Add data set' }));

      await waitFor(() => {
        expect(screen.getByText('Reorder data sets')).toBeInTheDocument();
      });

      const updatedTableRows = screen.getAllByRole('row');
      expect(updatedTableRows).toHaveLength(3);
      expect(updatedTableRows[2]).toHaveTextContent(
        'Number of authorised absence sessions (Ethnicity Major Chinese, State-funded secondary, Barnet, 2014/15)',
      );
    });

    test('removing a data set', async () => {
      render(
        <ChartBuilderFormsContextProvider initialForms={testFormState}>
          <ChartBuilder
            releaseId="release-1"
            data={testFullTable.results}
            meta={testFullTable.subjectMeta}
            tableTitle="Table title"
            onChartSave={jest.fn()}
            onChartDelete={noop}
            onTableQueryUpdate={jest.fn()}
          />
        </ChartBuilderFormsContextProvider>,
      );

      userEvent.click(screen.getByRole('button', { name: 'Line' }));

      userEvent.click(screen.getByRole('tab', { name: 'Data sets' }));

      expect(
        screen.getByRole('heading', { name: 'Data sets' }),
      ).toBeInTheDocument();

      userEvent.selectOptions(
        screen.getByLabelText('Indicator'),
        'authorised-absence-sessions',
      );
      userEvent.selectOptions(screen.getByLabelText('Time period'), '2014_AY');

      userEvent.click(screen.getByRole('button', { name: 'Add data set' }));

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

      userEvent.click(
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
      render(
        <ChartBuilderFormsContextProvider initialForms={testFormState}>
          <ChartBuilder
            releaseId="release-1"
            data={testFullTable.results}
            meta={testFullTable.subjectMeta}
            tableTitle="Table title"
            onChartSave={jest.fn()}
            onChartDelete={noop}
            onTableQueryUpdate={jest.fn()}
          />
        </ChartBuilderFormsContextProvider>,
      );

      userEvent.click(screen.getByRole('button', { name: 'Line' }));

      userEvent.click(screen.getByRole('tab', { name: 'Data sets' }));

      expect(
        screen.getByRole('heading', { name: 'Data sets' }),
      ).toBeInTheDocument();

      userEvent.selectOptions(
        screen.getByLabelText('Indicator'),
        'authorised-absence-sessions',
      );
      userEvent.selectOptions(screen.getByLabelText('Time period'), '2014_AY');

      userEvent.click(screen.getByRole('button', { name: 'Add data set' }));

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

      userEvent.click(
        screen.getByRole('button', { name: 'Remove all data sets' }),
      );

      const modal = within(screen.getByRole('dialog'));
      expect(modal.getByText('Remove all data sets')).toBeInTheDocument();
      userEvent.click(modal.getByRole('button', { name: 'Confirm' }));

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
});
