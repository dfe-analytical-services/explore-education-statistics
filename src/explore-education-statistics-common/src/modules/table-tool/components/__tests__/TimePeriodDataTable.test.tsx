import {
  testData1,
  testData2,
  testData3,
  testDataNoFilters,
} from '@common/modules/table-tool/components/__tests__/__data__/TimePeriodDataTable.data';
import TimePeriodDataTable, {
  createGroupHeaders,
} from '@common/modules/table-tool/components/TimePeriodDataTable';
import { UnmappedFullTable } from '@common/modules/table-tool/services/tableBuilderService';
import mapFullTable from '@common/modules/table-tool/utils/mapFullTable';
import mapTableHeadersConfig from '@common/modules/table-tool/utils/mapTableHeadersConfig';
import React from 'react';
import { render } from 'react-testing-library';

describe('TimePeriodDataTable', () => {
  test('renders table with two of every option', () => {
    const fullTable = mapFullTable(testData1.fullTable);

    const { container } = render(
      <TimePeriodDataTable
        fullTable={fullTable}
        tableHeadersConfig={mapTableHeadersConfig(
          testData1.tableHeadersConfig,
          fullTable.subjectMeta,
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
          fullTable.subjectMeta,
        )}
      />,
    );

    expect(
      findByText(
        "Table showing 'Absence by characteristic' from 'Pupil absence' for 2013/14 to 2014/15 for Barnet and Barnsley",
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
          fullTable.subjectMeta,
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
          fullTable.subjectMeta,
        )}
      />,
    );

    expect(
      findByText(
        "Table showing Authorised absence rate for 'Absence by characteristic' from 'Pupil absence' for 2013/14 to 2014/15 for Barnet and Barnsley",
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
          fullTable.subjectMeta,
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
          fullTable.subjectMeta,
        )}
      />,
    );

    expect(
      findByText(
        "Table showing 'Absence by characteristic' from 'Pupil absence' for 2014/15 for Barnet and Barnsley",
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
          fullTable.subjectMeta,
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
          },
        ],
        locations: [{ value: 'E92000001', label: 'England', level: 'country' }],
        publicationName: 'Pupil absence in schools in England',
        subjectName: 'Absence in prus',
        timePeriodRange: [{ label: '2014/15', code: 'AY', year: 2014 }],
        geoJsonAvailable: false,
      },
      results: [
        {
          filters: ['598ed9fd-b37e-4e08-baec-08d78f6f2c4d'],
          geographicLevel: 'Country',
          location: { country: { code: 'E92000001', name: 'England' } },
          measures: { '6160c4f8-4c9f-40f0-a623-2a4f742860af': '18.3' },
          timePeriod: '2014_AY',
        },
      ],
    } as UnmappedFullTable);

    const tableHeadersConfig = mapTableHeadersConfig(
      {
        columnGroups: [
          [
            {
              value: '598ed9fd-b37e-4e08-baec-08d78f6f2c4d',
              label: 'Ethnicity Major Asian Total',
            },
          ],
        ],
        rowGroups: [[{ value: 'E92000001', label: 'England' }]],
        columns: [{ value: '2014_AY', label: '2014/15' }],
        rows: [
          {
            value: '6160c4f8-4c9f-40f0-a623-2a4f742860af',
            label: 'Authorised absence rate',
          },
        ],
      },
      fullTable.subjectMeta,
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
          },
        ],
        locations: [{ value: 'E92000001', label: 'England', level: 'country' }],
        publicationName: 'Pupil absence in schools in England',
        subjectName: 'Absence in prus',
        timePeriodRange: [{ label: '2014/15', code: 'AY', year: 2014 }],
        geoJsonAvailable: false,
      },
      results: [
        {
          filters: [],
          geographicLevel: 'Country',
          location: { country: { code: 'E92000001', name: 'England' } },
          measures: { '6160c4f8-4c9f-40f0-a623-2a4f742860af': '18.3' },
          timePeriod: '2014_AY',
        },
      ],
    } as UnmappedFullTable);

    const tableHeadersConfig = mapTableHeadersConfig(
      {
        columnGroups: [],
        rowGroups: [[{ value: 'E92000001', label: 'England' }]],
        columns: [{ value: '2014_AY', label: '2014/15' }],
        rows: [
          {
            value: '6160c4f8-4c9f-40f0-a623-2a4f742860af',
            label: 'Authorised absence rate',
          },
        ],
      },
      fullTable.subjectMeta,
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
            unit: '%',
          },
        ],
        locations: [{ value: 'E92000001', label: 'England', level: 'country' }],
        publicationName: 'Pupil absence in schools in England',
        subjectName: 'Absence in prus',
        timePeriodRange: [{ label: '2014/15', code: 'AY', year: 2014 }],
        geoJsonAvailable: false,
      },
      results: [
        {
          filters: [],
          geographicLevel: 'Country',
          location: { country: { code: 'E92000001', name: 'England' } },
          measures: { '6160c4f8-4c9f-40f0-a623-2a4f742860af': '18.3' },
          timePeriod: '2014_AY',
        },
      ],
    } as UnmappedFullTable);

    const tableHeadersConfig = mapTableHeadersConfig(
      {
        columnGroups: [],
        rowGroups: [[{ value: 'E92000001', label: 'England' }]],
        columns: [{ value: '2014_AY', label: '2014/15' }],
        rows: [
          {
            value: '6160c4f8-4c9f-40f0-a623-2a4f742860af',
            label: 'Authorised absence rate',
          },
        ],
      },
      fullTable.subjectMeta,
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

  describe('createGroupHeaders', () => {
    test('creates extra filter group headers', () => {
      const data = [
        [{ value: 'E92000001', label: 'England', level: '6' }],
        [
          {
            value: '1',
            label: 'Total',
            filterGroup: 'Total',
            isTotal: true,
          },
          {
            value: '53',
            label: 'Gender male',
            filterGroup: 'Gender',
            isTotal: false,
          },
          {
            value: '52',
            label: 'Gender female',
            filterGroup: 'Gender',
            isTotal: false,
          },
          {
            value: '71',
            label: 'Ethnicity Major Asian Total',
            filterGroup: 'Ethnic group major',
            isTotal: false,
          },
          {
            value: '72',
            label: 'Ethnicity Major Black Total',
            filterGroup: 'Ethnic group major',
            isTotal: false,
          },
        ],
      ];

      const result = createGroupHeaders(data);

      expect(result).toStrictEqual([
        { headers: [{ text: 'England' }] },
        {
          groups: [
            { text: 'Total' },
            { text: 'Gender', span: 2 },
            { text: 'Ethnic group major', span: 2 },
          ],
          headers: [
            { text: 'Total' },
            { text: 'Gender male' },
            { text: 'Gender female' },
            { text: 'Ethnicity Major Asian Total' },
            { text: 'Ethnicity Major Black Total' },
          ],
        },
      ]);
    });

    test('creates extra filter group headers in reverse', () => {
      const data = [
        [{ value: 'E92000001', label: 'England', level: '6' }],
        [
          {
            value: '71',
            label: 'Ethnicity Major Asian Total',
            filterGroup: 'Ethnic group major',
            isTotal: false,
          },
          {
            value: '72',
            label: 'Ethnicity Major Black Total',
            filterGroup: 'Ethnic group major',
            isTotal: false,
          },
          {
            value: '53',
            label: 'Gender male',
            filterGroup: 'Gender',
            isTotal: false,
          },
          {
            value: '52',
            label: 'Gender female',
            filterGroup: 'Gender',
            isTotal: false,
          },

          {
            value: '1',
            label: 'Total',
            filterGroup: 'Total',
            isTotal: true,
          },
        ],
      ];

      const result = createGroupHeaders(data);

      expect(result).toStrictEqual([
        { headers: [{ text: 'England' }] },
        {
          groups: [
            { text: 'Ethnic group major', span: 2 },
            { text: 'Gender', span: 2 },
            { text: 'Total' },
          ],
          headers: [
            { text: 'Ethnicity Major Asian Total' },
            { text: 'Ethnicity Major Black Total' },
            { text: 'Gender male' },
            { text: 'Gender female' },
            { text: 'Total' },
          ],
        },
      ]);
    });

    test('does not create `Default` filter group headers for only `Default` filters', () => {
      const data = [
        [{ value: 'E92000001', label: 'England', level: '6' }],
        [
          {
            value: '1',
            label: 'Total',
            filterGroup: 'Total',
            isTotal: true,
          },
          {
            value: '53',
            label: 'Gender male',
            filterGroup: 'Gender',
            isTotal: false,
          },
          {
            value: '52',
            label: 'Gender female',
            filterGroup: 'Gender',
            isTotal: false,
          },
        ],
        [
          {
            value: '100',
            label: 'State-funded primary',
            filterGroup: 'Default',
            isTotal: false,
          },
        ],
      ];

      const result = createGroupHeaders(data);

      expect(result).toStrictEqual([
        { headers: [{ text: 'England' }] },
        {
          groups: [{ text: 'Total' }, { text: 'Gender', span: 2 }],
          headers: [
            { text: 'Total' },
            { text: 'Gender male' },
            { text: 'Gender female' },
          ],
        },
        { headers: [{ text: 'State-funded primary' }] },
      ]);
    });

    test('creates `Default` filter group header if some sibling filters are not `Default`', () => {
      const data = [
        [{ value: 'E92000001', label: 'England', level: '6' }],
        [
          {
            value: '1',
            label: 'Total',
            filterGroup: 'Total',
            isTotal: true,
          },
          {
            value: '53',
            label: 'Gender male',
            filterGroup: 'Gender',
            isTotal: false,
          },
          {
            value: '52',
            label: 'Gender female',
            filterGroup: 'Gender',
            isTotal: false,
          },
          {
            value: '100',
            label: 'Some default filter',
            filterGroup: 'Default',
            isTotal: false,
          },
        ],
      ];

      const result = createGroupHeaders(data);

      expect(result).toStrictEqual([
        { headers: [{ text: 'England' }] },
        {
          groups: [
            { text: 'Total' },
            { text: 'Gender', span: 2 },
            { text: 'Default' },
          ],
          headers: [
            { text: 'Total' },
            { text: 'Gender male' },
            { text: 'Gender female' },
            { text: 'Some default filter' },
          ],
        },
      ]);
    });
  });
});
