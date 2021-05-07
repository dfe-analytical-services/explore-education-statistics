import { FinalStepRenderProps } from '@common/modules/table-tool/components/TableToolWizard';
import {
  CategoryFilter,
  Indicator,
  LocationFilter,
  TimePeriodFilter,
} from '@common/modules/table-tool/types/filters';
import { FullTable } from '@common/modules/table-tool/types/fullTable';
import { TableHeadersConfig } from '@common/modules/table-tool/types/tableHeaders';
import { TableDataQuery } from '@common/services/tableBuilderService';
import TableToolFinalStep from '@frontend/modules/table-tool/components/TableToolFinalStep';
import { within } from '@testing-library/dom';
import { render, screen, waitFor } from '@testing-library/react';
import React from 'react';
import userEvent from '@testing-library/user-event';

describe('TableToolFinalStep', () => {
  const testPublication: FinalStepRenderProps['publication'] = {
    id: '536154f5-7f82-4dc7-060a-08d9097c1945',
    title: 'Test publication',
    slug: 'test-publication',
  };

  const testQuery: TableDataQuery = {
    publicationId: '536154f5-7f82-4dc7-060a-08d9097c1945',
    subjectId: '1f1b1780-a607-454e-b331-08d9097c40f5',
    indicators: [
      '18a27dde-e54e-46d0-6656-08d9097c4255',
      '6240d58e-c160-4c39-6657-08d9097c4255',
    ],
    filters: [
      'bfd88241-1130-4df8-9e49-a411618d082f',
      '3f223187-a2aa-420c-a3d8-e2a94f77e4b5',
    ],
    locations: { country: ['E92000001'] },
    timePeriod: {
      startYear: 2020,
      startCode: 'W23',
      endYear: 2020,
      endCode: 'W26',
    },
  };

  const testTable: FullTable = {
    subjectMeta: {
      filters: {
        Date: {
          name: 'date',
          options: [
            new CategoryFilter({
              value: '3f223187-a2aa-420c-a3d8-e2a94f77e4b5',
              label: '02/06/2020',
              group: 'Default',
              isTotal: false,
              category: 'Date',
            }),
            new CategoryFilter({
              value: 'bfd88241-1130-4df8-9e49-a411618d082f',
              label: '02/04/2021',
              group: 'Default',
              isTotal: false,
              category: 'Date',
            }),
          ],
        },
      },
      footnotes: [],
      indicators: [
        new Indicator({
          value: '18a27dde-e54e-46d0-6656-08d9097c4255',
          label: 'Number of open settings',
          unit: '',
          name: 'open_settings',
        }),
        new Indicator({
          value: '6240d58e-c160-4c39-6657-08d9097c4255',
          label: 'Proportion of settings open',
          unit: '%',
          decimalPlaces: 0,
          name: 'proportion_of_settings_open',
        }),
      ],
      locations: [
        new LocationFilter({
          value: 'E92000001',
          label: 'England',
          level: 'country',
        }),
      ],
      boundaryLevels: [
        {
          id: 1,
          label:
            'Countries December 2017 Ultra Generalised Clipped Boundaries in UK',
        },
      ],
      publicationName: 'Test publication',
      subjectName: 'dates',
      timePeriodRange: [
        new TimePeriodFilter({
          label: '2020 Week 23',
          year: 2020,
          code: 'W23',
          order: 0,
        }),
        new TimePeriodFilter({
          label: '2020 Week 24',
          year: 2020,
          code: 'W24',
          order: 1,
        }),
        new TimePeriodFilter({
          label: '2020 Week 25',
          year: 2020,
          code: 'W25',
          order: 2,
        }),
        new TimePeriodFilter({
          label: '2020 Week 26',
          year: 2020,
          code: 'W26',
          order: 3,
        }),
      ],
      geoJsonAvailable: true,
    },
    results: [
      {
        filters: ['3f223187-a2aa-420c-a3d8-e2a94f77e4b5'],
        geographicLevel: 'country',
        location: {
          country: { code: 'E92000001', name: 'England' },
        },
        measures: {
          '18a27dde-e54e-46d0-6656-08d9097c4255': '22500',
          '6240d58e-c160-4c39-6657-08d9097c4255': '0.9',
        },
        timePeriod: '2020_W23',
      },
      {
        filters: ['bfd88241-1130-4df8-9e49-a411618d082f'],
        geographicLevel: 'country',
        location: {
          country: { code: 'E92000001', name: 'England' },
        },
        measures: {
          '18a27dde-e54e-46d0-6656-08d9097c4255': '18700',
          '6240d58e-c160-4c39-6657-08d9097c4255': '0.76',
        },
        timePeriod: '2021_W14',
      },
    ],
  };

  const testTableHeaders: TableHeadersConfig = {
    columnGroups: [],
    rowGroups: [
      [
        new Indicator({
          value: '18a27dde-e54e-46d0-6656-08d9097c4255',
          label: 'Number of open settings',
          unit: '',
          name: 'open_settings',
        }),
        new Indicator({
          value: '6240d58e-c160-4c39-6657-08d9097c4255',
          label: 'Proportion of settings open',
          unit: '%',
          decimalPlaces: 0,
          name: 'proportion_of_settings_open',
        }),
      ],
    ],
    columns: [
      new CategoryFilter({
        value: 'bfd88241-1130-4df8-9e49-a411618d082f',
        label: '02/04/2021',
        group: 'Default',
        isTotal: false,
        category: 'Date',
      }),
      new CategoryFilter({
        value: '3f223187-a2aa-420c-a3d8-e2a94f77e4b5',
        label: '02/06/2020',
        group: 'Default',
        isTotal: false,
        category: 'Date',
      }),
    ],
    rows: [
      new TimePeriodFilter({
        label: '2020 Week 23',
        year: 2020,
        code: 'W23',
        order: 0,
      }),
      new TimePeriodFilter({
        label: '2020 Week 24',
        year: 2020,
        code: 'W24',
        order: 1,
      }),
      new TimePeriodFilter({
        label: '2020 Week 25',
        year: 2020,
        code: 'W25',
        order: 2,
      }),
      new TimePeriodFilter({
        label: '2020 Week 26',
        year: 2020,
        code: 'W26',
        order: 3,
      }),
    ],
  };

  test('renders the final step successfully', async () => {
    render(
      <TableToolFinalStep
        publication={testPublication}
        query={testQuery}
        table={testTable}
        tableHeaders={testTableHeaders}
      />,
    );

    // test the reordering controls for the table headers are present
    const groups = screen.queryAllByRole('group');
    expect(groups).toHaveLength(1);
    const reorderTableHeadersGroup = groups[0];
    expect(
      within(reorderTableHeadersGroup).queryByRole('button', {
        name: 'Re-order table headers',
      }),
    ).toBeInTheDocument();

    // test that the preview table is rendered correctly
    expect(
      screen.queryByText(
        "Table showing 'dates' for 02/06/2020 and 02/04/2021 from 'Test publication' in England between 2020 Week 23 and 2020 Week 26",
      ),
    ).toBeInTheDocument();
    const table = screen.queryByTestId('dataTableCaption-table');
    expect(table).toBeInTheDocument();
    expect(table).toMatchSnapshot();

    // test the permalink controls are present
    expect(screen.queryByText('Share your table')).toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: 'Generate permanent link' }),
    ).toBeInTheDocument();

    // test that the additional options are rendered correctly
    expect(screen.queryByText('Additional options')).toBeInTheDocument();
    expect(
      screen.queryByRole('link', { name: 'View the release for this data' }),
    ).toBeInTheDocument();
    expect(
      screen.queryByRole('button', {
        name: 'Download the data of this table (CSV)',
      }),
    ).toBeInTheDocument();
    expect(
      screen.queryByRole('button', {
        name: 'Download table as Excel spreadsheet (XLSX)',
      }),
    ).toBeInTheDocument();

    expect(
      screen.getByTestId('Table tool final step container'),
    ).toMatchSnapshot();
  });

  test('shows and hides table header re-ordering controls successfully', async () => {
    render(
      <TableToolFinalStep
        publication={testPublication}
        query={testQuery}
        table={testTable}
        tableHeaders={testTableHeaders}
      />,
    );

    const reorderRevealButton = screen.getByRole('button', {
      name: 'Re-order table headers',
    });

    expect(
      screen.queryByRole('button', { name: 'Re-order table' }),
    ).not.toBeInTheDocument();

    userEvent.click(reorderRevealButton);

    await waitFor(() =>
      expect(
        screen.queryByRole('button', { name: 'Re-order table' }),
      ).toBeInTheDocument(),
    );

    expect(
      screen.getByTestId('Table tool final step container'),
    ).toMatchSnapshot();
  });

  test(`renders the 'View the release for this data' URL with the Release's slug, if supplied`, async () => {
    render(
      <TableToolFinalStep
        publication={testPublication}
        query={testQuery}
        table={testTable}
        tableHeaders={testTableHeaders}
        releaseId="test-release-id"
        releaseSlug="test-release-slug"
      />,
    );

    const viewReleaseLink = screen.getByRole('link', {
      name: 'View the release for this data',
    }) as HTMLAnchorElement;

    expect(viewReleaseLink.href).toEqual(
      'http://localhost/find-statistics/test-publication/test-release-slug',
    );

    expect(
      screen.getByTestId('Table tool final step container'),
    ).toMatchSnapshot();
  });

  test(`renders the 'View the release for this data' URL with only the Publication slug, if the Release slug is not supplied`, async () => {
    render(
      <TableToolFinalStep
        publication={testPublication}
        query={testQuery}
        table={testTable}
        tableHeaders={testTableHeaders}
      />,
    );

    const viewReleaseLink = screen.getByRole('link', {
      name: 'View the release for this data',
    }) as HTMLAnchorElement;

    expect(viewReleaseLink.href).toEqual(
      'http://localhost/find-statistics/test-publication',
    );

    expect(
      screen.getByTestId('Table tool final step container'),
    ).toMatchSnapshot();
  });

  test('renders the Table Tool final step correctly when this is the latest data', async () => {
    render(
      <TableToolFinalStep
        publication={testPublication}
        query={testQuery}
        table={testTable}
        tableHeaders={testTableHeaders}
        latestData
      />,
    );

    expect(screen.queryByText('This is the latest data')).toBeInTheDocument();
    expect(
      screen.queryByText('This data is not from the latest release'),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByTestId('View latest data link'),
    ).not.toBeInTheDocument();

    expect(
      screen.getByTestId('Table tool final step container'),
    ).toMatchSnapshot();
  });

  test(`renders the Table Tool final step correctly if this is not the latest data`, async () => {
    render(
      <TableToolFinalStep
        publication={testPublication}
        query={testQuery}
        table={testTable}
        tableHeaders={testTableHeaders}
        latestData={false}
        latestReleaseTitle="Latest Release Title"
      />,
    );

    const viewReleaseLink = screen.getByRole('link', {
      name: 'View the release for this data',
    }) as HTMLAnchorElement;

    expect(viewReleaseLink.href).toEqual(
      'http://localhost/find-statistics/test-publication',
    );

    expect(
      screen.queryByText('This is the latest data'),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByText('This data is not from the latest release'),
    ).toBeInTheDocument();

    const latestDataLink = screen.queryByTestId(
      'View latest data link',
    ) as HTMLAnchorElement;
    expect(latestDataLink).toBeInTheDocument();
    expect(latestDataLink.href).toEqual(
      'http://localhost/find-statistics/test-publication',
    );
    expect(latestDataLink.text).toContain('View latest data');
    expect(latestDataLink.text).toContain('Latest Release Title');

    expect(
      screen.getByTestId('Table tool final step container'),
    ).toMatchSnapshot();
  });
});
