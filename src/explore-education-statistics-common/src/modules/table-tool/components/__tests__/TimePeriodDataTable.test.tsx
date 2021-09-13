import {
  testData1,
  testData2,
  testData3,
  testDataFiltersWithNoResults,
  testDataNoFilters,
} from '@common/modules/table-tool/components/__tests__/__data__/TimePeriodDataTable.data';
import TimePeriodDataTable from '@common/modules/table-tool/components/TimePeriodDataTable';
import { UnmappedTableHeadersConfig } from '@common/services/permalinkService';
import { TableDataResponse } from '@common/services/tableBuilderService';
import mapFullTable from '@common/modules/table-tool/utils/mapFullTable';
import mapTableHeadersConfig from '@common/modules/table-tool/utils/mapTableHeadersConfig';
import { render } from '@testing-library/react';
import React from 'react';

describe('TimePeriodDataTable', () => {
  test('renders table with two of every option', () => {
    const fullTable = mapFullTable(testData1.fullTable);

    const { container } = render(
      <TimePeriodDataTable
        fullTable={fullTable}
        tableHeadersConfig={mapTableHeadersConfig(
          testData1.tableHeadersConfig,
          fullTable,
        )}
      />,
    );

    expect(container.querySelectorAll('table thead tr')).toHaveLength(2);
    expect(container.querySelectorAll('table thead th')).toHaveLength(6);
    expect(
      container.querySelectorAll('table thead th[scope="colgroup"]'),
    ).toHaveLength(2);
    expect(
      container.querySelectorAll('table thead th[scope="col"]'),
    ).toHaveLength(4);

    expect(container.querySelectorAll('table tbody tr')).toHaveLength(8);
    expect(container.querySelectorAll('table tbody th')).toHaveLength(14);
    expect(
      container.querySelectorAll('table tbody th[scope="rowgroup"]'),
    ).toHaveLength(6);
    expect(
      container.querySelectorAll('table tbody th[scope="row"]'),
    ).toHaveLength(8);

    expect(container.querySelectorAll('table tbody td')).toHaveLength(32);

    // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
    expect(container.querySelector('table')!.innerHTML).toMatchSnapshot();
  });

  test('renders title without indicator when there is more than one', () => {
    const fullTable = mapFullTable(testData1.fullTable);

    const { findByText } = render(
      <TimePeriodDataTable
        fullTable={fullTable}
        tableHeadersConfig={mapTableHeadersConfig(
          testData1.tableHeadersConfig,
          fullTable,
        )}
      />,
    );

    expect(
      findByText(
        "'Absence by characteristic' from 'Pupil absence' for 2013/14 to 2014/15 for Barnet and Barnsley",
      ),
    ).not.toBeNull();
  });

  test('renders table without indicators when there is only one', () => {
    const fullTable = mapFullTable(testData2.fullTable);

    const { container } = render(
      <TimePeriodDataTable
        fullTable={fullTable}
        tableHeadersConfig={mapTableHeadersConfig(
          testData2.tableHeadersConfig,
          fullTable,
        )}
      />,
    );

    expect(container.querySelectorAll('table thead tr')).toHaveLength(2);
    expect(container.querySelectorAll('table thead th')).toHaveLength(6);
    expect(
      container.querySelectorAll('table thead th[scope="colgroup"]'),
    ).toHaveLength(2);
    expect(
      container.querySelectorAll('table thead th[scope="col"]'),
    ).toHaveLength(4);

    expect(container.querySelectorAll('table tbody tr')).toHaveLength(4);
    expect(container.querySelectorAll('table tbody th')).toHaveLength(6);
    expect(
      container.querySelectorAll('table tbody th[scope="rowgroup"]'),
    ).toHaveLength(2);
    expect(
      container.querySelectorAll('table tbody th[scope="row"]'),
    ).toHaveLength(4);

    expect(container.querySelectorAll('table tbody td')).toHaveLength(16);

    // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
    expect(container.querySelector('table')!.innerHTML).toMatchSnapshot();
  });

  test('renders title with indicator when there is only one', () => {
    const fullTable = mapFullTable(testData2.fullTable);

    const { findByText } = render(
      <TimePeriodDataTable
        fullTable={fullTable}
        tableHeadersConfig={mapTableHeadersConfig(
          testData2.tableHeadersConfig,
          fullTable,
        )}
      />,
    );

    expect(
      findByText(
        "Authorised absence rate for 'Absence by characteristic' from 'Pupil absence' for 2013/14 to 2014/15 for Barnet and Barnsley",
      ),
    ).not.toBeNull();
  });

  test('renders table without time period when there is only one', () => {
    const fullTable = mapFullTable(testData3.fullTable);

    const { container } = render(
      <TimePeriodDataTable
        fullTable={fullTable}
        tableHeadersConfig={mapTableHeadersConfig(
          testData3.tableHeadersConfig,
          fullTable,
        )}
      />,
    );

    expect(container.querySelectorAll('table thead tr')).toHaveLength(1);
    expect(container.querySelectorAll('table thead th')).toHaveLength(2);
    expect(
      container.querySelectorAll('table thead th[scope="colgroup"]'),
    ).toHaveLength(0);
    expect(
      container.querySelectorAll('table thead th[scope="col"]'),
    ).toHaveLength(2);

    expect(container.querySelectorAll('table tbody tr')).toHaveLength(4);
    expect(container.querySelectorAll('table tbody th')).toHaveLength(6);
    expect(
      container.querySelectorAll('table tbody th[scope="rowgroup"]'),
    ).toHaveLength(2);
    expect(
      container.querySelectorAll('table tbody th[scope="row"]'),
    ).toHaveLength(4);

    expect(container.querySelectorAll('table tbody td')).toHaveLength(8);

    // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
    expect(container.querySelector('table')!.innerHTML).toMatchSnapshot();
  });

  test('renders title with time period when there is only one', () => {
    const fullTable = mapFullTable(testData3.fullTable);

    const { findByText } = render(
      <TimePeriodDataTable
        fullTable={fullTable}
        tableHeadersConfig={mapTableHeadersConfig(
          testData3.tableHeadersConfig,
          fullTable,
        )}
      />,
    );

    expect(
      findByText(
        "'Absence by characteristic' from 'Pupil absence' for 2014/15 for Barnet and Barnsley",
      ),
    ).not.toBeNull();
  });

  test('renders table with no filters', () => {
    const fullTable = mapFullTable(testDataNoFilters.fullTable);

    const { container } = render(
      <TimePeriodDataTable
        fullTable={fullTable}
        tableHeadersConfig={mapTableHeadersConfig(
          testDataNoFilters.tableHeadersConfig,
          fullTable,
        )}
      />,
    );

    expect(container.querySelectorAll('table thead tr')).toHaveLength(1);
    expect(container.querySelectorAll('table thead th')).toHaveLength(3);
    expect(
      container.querySelectorAll('table thead th[scope="colgroup"]'),
    ).toHaveLength(0);
    expect(
      container.querySelectorAll('table thead th[scope="col"]'),
    ).toHaveLength(3);

    expect(container.querySelectorAll('table tbody tr')).toHaveLength(3);
    expect(container.querySelectorAll('table tbody th')).toHaveLength(4);
    expect(
      container.querySelectorAll('table tbody th[scope="rowgroup"]'),
    ).toHaveLength(1);
    expect(
      container.querySelectorAll('table tbody th[scope="row"]'),
    ).toHaveLength(3);

    expect(container.querySelectorAll('table tbody td')).toHaveLength(9);

    // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
    expect(container.querySelector('table')!.innerHTML).toMatchSnapshot();
  });

  test('renders table with only one of each option', () => {
    const fullTable = mapFullTable({
      subjectMeta: {
        filters: {
          Characteristic: {
            totalValue: '',
            hint: 'Filter by pupil characteristic',
            legend: 'Characteristic',
            name: 'characteristic',
            options: {
              EthnicGroupMajor: {
                label: 'Ethnic group major',
                options: [
                  {
                    label: 'Ethnicity Major Asian Total',
                    value: '598ed9fd-b37e-4e08-baec-08d78f6f2c4d',
                  },
                ],
              },
            },
          },
        },
        footnotes: [],
        indicators: [
          {
            value: '6160c4f8-4c9f-40f0-a623-2a4f742860af',
            label: 'Authorised absence rate',
            unit: '%',
            name: 'sess_authorised_percent',
            decimalPlaces: 1,
          },
        ],
        locations: [{ value: 'E92000001', label: 'England', level: 'country' }],
        boundaryLevels: [],
        publicationName: 'Pupil absence in schools in England',
        subjectName: 'Absence in prus',
        timePeriodRange: [{ label: '2014/15', code: 'AY', year: 2014 }],
        geoJsonAvailable: false,
      },
      results: [
        {
          filters: ['598ed9fd-b37e-4e08-baec-08d78f6f2c4d'],
          geographicLevel: 'country',
          location: { country: { code: 'E92000001', name: 'England' } },
          measures: { '6160c4f8-4c9f-40f0-a623-2a4f742860af': '18.3' },
          timePeriod: '2014_AY',
        },
      ],
    } as TableDataResponse);

    const tableHeadersConfig = mapTableHeadersConfig(
      {
        columnGroups: [
          [
            {
              value: '598ed9fd-b37e-4e08-baec-08d78f6f2c4d',
              type: 'Filter',
            },
          ],
        ],
        rowGroups: [
          [{ value: 'E92000001', type: 'Location', level: 'country' }],
        ],
        columns: [{ value: '2014_AY', type: 'TimePeriod' }],
        rows: [
          {
            value: '6160c4f8-4c9f-40f0-a623-2a4f742860af',
            type: 'Indicator',
          },
        ],
      },
      fullTable,
    );

    const { container } = render(
      <TimePeriodDataTable
        fullTable={fullTable}
        tableHeadersConfig={tableHeadersConfig}
      />,
    );

    expect(container.querySelectorAll('table thead tr')).toHaveLength(1);
    expect(container.querySelectorAll('table thead th')).toHaveLength(1);
    expect(
      container.querySelectorAll('table thead th[scope="colgroup"]'),
    ).toHaveLength(0);
    expect(
      container.querySelectorAll('table thead th[scope="col"]'),
    ).toHaveLength(1);
    expect(
      container.querySelector('table thead th[scope="col"]'),
    ).toHaveTextContent('Ethnicity Major Asian Total');

    expect(container.querySelectorAll('table tbody tr')).toHaveLength(1);
    expect(container.querySelectorAll('table tbody th')).toHaveLength(1);
    expect(
      container.querySelectorAll('table tbody th[scope="rowgroup"]'),
    ).toHaveLength(0);
    expect(
      container.querySelectorAll('table tbody th[scope="row"]'),
    ).toHaveLength(1);
    expect(
      container.querySelector('table tbody th[scope="row"]'),
    ).toHaveTextContent('England');

    expect(container.querySelectorAll('table tbody td')).toHaveLength(1);

    // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
    expect(container.querySelector('table')!.innerHTML).toMatchSnapshot();
  });

  test('renders table with only one of each option and no filters', () => {
    const fullTable = mapFullTable({
      subjectMeta: {
        filters: {},
        footnotes: [],
        indicators: [
          {
            value: '6160c4f8-4c9f-40f0-a623-2a4f742860af',
            label: 'Authorised absence rate',
            unit: '%',
            name: 'sess_authorised_percent',
            decimalPlaces: 1,
          },
        ],
        locations: [{ value: 'E92000001', label: 'England', level: 'country' }],
        boundaryLevels: [],
        publicationName: 'Pupil absence in schools in England',
        subjectName: 'Absence in prus',
        timePeriodRange: [{ label: '2014/15', code: 'AY', year: 2014 }],
        geoJsonAvailable: false,
      },
      results: [
        {
          filters: [],
          geographicLevel: 'country',
          location: { country: { code: 'E92000001', name: 'England' } },
          measures: { '6160c4f8-4c9f-40f0-a623-2a4f742860af': '18.3' },
          timePeriod: '2014_AY',
        },
      ],
    } as TableDataResponse);

    const tableHeadersConfig = mapTableHeadersConfig(
      {
        columnGroups: [],
        rowGroups: [
          [{ value: 'E92000001', level: 'country', type: 'Location' }],
        ],
        columns: [{ value: '2014_AY', type: 'TimePeriod' }],
        rows: [
          {
            value: '6160c4f8-4c9f-40f0-a623-2a4f742860af',
            type: 'Indicator',
          },
        ],
      },
      fullTable,
    );

    const { container } = render(
      <TimePeriodDataTable
        fullTable={fullTable}
        tableHeadersConfig={tableHeadersConfig}
      />,
    );

    expect(container.querySelectorAll('table thead tr')).toHaveLength(1);
    expect(container.querySelectorAll('table thead th')).toHaveLength(1);
    expect(
      container.querySelectorAll('table thead th[scope="colgroup"]'),
    ).toHaveLength(0);
    expect(
      container.querySelectorAll('table thead th[scope="col"]'),
    ).toHaveLength(1);
    expect(
      container.querySelector('table thead th[scope="col"]'),
    ).toHaveTextContent('2014/15');

    expect(container.querySelectorAll('table tbody tr')).toHaveLength(1);
    expect(container.querySelectorAll('table tbody th')).toHaveLength(1);
    expect(
      container.querySelectorAll('table tbody th[scope="rowgroup"]'),
    ).toHaveLength(0);
    expect(
      container.querySelectorAll('table tbody th[scope="row"]'),
    ).toHaveLength(1);
    expect(
      container.querySelector('table tbody th[scope="row"]'),
    ).toHaveTextContent('England');

    expect(container.querySelectorAll('table tbody td')).toHaveLength(1);

    // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
    expect(container.querySelector('table')!.innerHTML).toMatchSnapshot();
  });

  test('renders table with only one of each option and no filters or locations', () => {
    const fullTable = mapFullTable({
      subjectMeta: {
        filters: {},
        footnotes: [],
        indicators: [
          {
            value: '6160c4f8-4c9f-40f0-a623-2a4f742860af',
            label: 'Authorised absence rate',
            name: 'sess_authorised_percent',
            unit: '%',
          },
        ],
        locations: [{ value: 'E92000001', label: 'England', level: 'country' }],
        boundaryLevels: [],
        publicationName: 'Pupil absence in schools in England',
        subjectName: 'Absence in prus',
        timePeriodRange: [{ label: '2014/15', code: 'AY', year: 2014 }],
        geoJsonAvailable: false,
      },
      results: [
        {
          filters: [],
          geographicLevel: 'country',
          location: { country: { code: 'E92000001', name: 'England' } },
          measures: { '6160c4f8-4c9f-40f0-a623-2a4f742860af': '18.3' },
          timePeriod: '2014_AY',
        },
      ],
    } as TableDataResponse);

    const tableHeadersConfig = mapTableHeadersConfig(
      {
        columnGroups: [],
        rowGroups: [
          [{ value: 'E92000001', type: 'Location', level: 'country' }],
        ],
        columns: [{ value: '2014_AY', type: 'TimePeriod' }],
        rows: [
          {
            value: '6160c4f8-4c9f-40f0-a623-2a4f742860af',
            type: 'Indicator',
          },
        ],
      },
      fullTable,
    );

    const { container } = render(
      <TimePeriodDataTable
        fullTable={fullTable}
        tableHeadersConfig={tableHeadersConfig}
      />,
    );

    expect(container.querySelectorAll('table thead tr')).toHaveLength(1);
    expect(container.querySelectorAll('table thead th')).toHaveLength(1);
    expect(
      container.querySelectorAll('table thead th[scope="colgroup"]'),
    ).toHaveLength(0);
    expect(
      container.querySelectorAll('table thead th[scope="col"]'),
    ).toHaveLength(1);
    expect(
      container.querySelector('table thead th[scope="col"]'),
    ).toHaveTextContent('2014/15');

    expect(container.querySelectorAll('table tbody tr')).toHaveLength(1);
    expect(container.querySelectorAll('table tbody th')).toHaveLength(1);
    expect(
      container.querySelectorAll('table tbody th[scope="rowgroup"]'),
    ).toHaveLength(0);
    expect(
      container.querySelectorAll('table tbody th[scope="row"]'),
    ).toHaveLength(1);
    expect(
      container.querySelector('table tbody th[scope="row"]'),
    ).toHaveTextContent('England');

    expect(container.querySelectorAll('table tbody td')).toHaveLength(1);

    // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
    expect(container.querySelector('table')!.innerHTML).toMatchSnapshot();
  });

  test('renders table with completely empty rows removed', () => {
    const fullTable = mapFullTable(testDataFiltersWithNoResults.fullTable);

    const tableHeadersConfig: UnmappedTableHeadersConfig = {
      columnGroups: [
        [
          {
            value: 'b3207d77-143b-43d5-8b48-32d29727e96f',
            type: 'Filter',
          },
        ],
      ],
      rowGroups: [
        [
          { value: 'E08000026', type: 'Location', level: 'localAuthority' },
          { value: 'E09000008', type: 'Location', level: 'localAuthority' },
        ],
        [
          {
            value: '5675d1fa-77fd-4dfd-bb1f-08d78f6f2c4d',
            type: 'Filter',
          },
          {
            value: '53da1e17-184f-43f6-bb27-08d78f6f2c4d',
            type: 'Filter',
          },
        ],
      ],
      columns: [
        { value: '2013_AY', type: 'TimePeriod' },
        { value: '2014_AY', type: 'TimePeriod' },
        { value: '2015_AY', type: 'TimePeriod' },
      ],
      rows: [
        {
          value: '0003d2ac-4425-4432-2afb-08d78f6f2b08',
          type: 'Indicator',
        },
        {
          value: '829460cd-ae9e-4266-2aff-08d78f6f2b08',
          type: 'Indicator',
        },
      ],
    };

    const { container } = render(
      <TimePeriodDataTable
        fullTable={fullTable}
        tableHeadersConfig={mapTableHeadersConfig(
          tableHeadersConfig,
          fullTable,
        )}
      />,
    );

    expect(container.querySelectorAll('table thead tr')).toHaveLength(2);
    expect(container.querySelectorAll('table thead th')).toHaveLength(4);
    expect(
      container.querySelectorAll('table thead th[scope="colgroup"]'),
    ).toHaveLength(1);
    expect(
      container.querySelectorAll('table thead th[scope="col"]'),
    ).toHaveLength(3);

    expect(container.querySelectorAll('table tbody tr')).toHaveLength(6);
    expect(container.querySelectorAll('table tbody th')).toHaveLength(11);
    expect(
      container.querySelectorAll('table tbody th[scope="rowgroup"]'),
    ).toHaveLength(5);
    expect(
      container.querySelectorAll('table tbody th[scope="row"]'),
    ).toHaveLength(6);

    expect(container.querySelectorAll('table tbody td')).toHaveLength(18);

    // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
    expect(container.querySelector('table')!.innerHTML).toMatchSnapshot();
  });

  test('renders table with completely empty columns removed', () => {
    const fullTable = mapFullTable(testDataFiltersWithNoResults.fullTable);

    const tableHeadersConfig: UnmappedTableHeadersConfig = {
      columnGroups: [
        [
          {
            value: '5675d1fa-77fd-4dfd-bb1f-08d78f6f2c4d',
            type: 'Filter',
          },
          {
            value: '53da1e17-184f-43f6-bb27-08d78f6f2c4d',
            type: 'Filter',
          },
        ],
      ],
      rowGroups: [
        [
          { value: 'E08000026', type: 'Location', level: 'localAuthority' },
          { value: 'E09000008', type: 'Location', level: 'localAuthority' },
        ],
        [
          {
            value: 'b3207d77-143b-43d5-8b48-32d29727e96f',
            type: 'Filter',
          },
        ],
      ],
      columns: [
        { value: '2013_AY', type: 'TimePeriod' },
        { value: '2014_AY', type: 'TimePeriod' },
        { value: '2015_AY', type: 'TimePeriod' },
      ],
      rows: [
        {
          value: '0003d2ac-4425-4432-2afb-08d78f6f2b08',
          type: 'Indicator',
        },
        {
          value: '829460cd-ae9e-4266-2aff-08d78f6f2b08',
          type: 'Indicator',
        },
      ],
    };

    const { container } = render(
      <TimePeriodDataTable
        fullTable={fullTable}
        tableHeadersConfig={mapTableHeadersConfig(
          tableHeadersConfig,
          fullTable,
        )}
      />,
    );

    expect(container.querySelectorAll('table thead tr')).toHaveLength(2);
    expect(container.querySelectorAll('table thead th')).toHaveLength(6);
    expect(
      container.querySelectorAll('table thead th[scope="colgroup"]'),
    ).toHaveLength(2);
    expect(
      container.querySelectorAll('table thead th[scope="col"]'),
    ).toHaveLength(4);

    expect(container.querySelectorAll('table tbody tr')).toHaveLength(4);
    expect(container.querySelectorAll('table tbody th')).toHaveLength(8);
    expect(
      container.querySelectorAll('table tbody th[scope="rowgroup"]'),
    ).toHaveLength(4);
    expect(
      container.querySelectorAll('table tbody th[scope="row"]'),
    ).toHaveLength(4);

    expect(container.querySelectorAll('table tbody td')).toHaveLength(16);

    // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
    expect(container.querySelector('table')!.innerHTML).toMatchSnapshot();
  });
});
