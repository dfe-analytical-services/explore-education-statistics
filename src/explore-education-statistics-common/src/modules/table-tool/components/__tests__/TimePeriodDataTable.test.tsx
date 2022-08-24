import {
  testData1Table,
  testData1TableHeadersConfig,
  testData2Table,
  testData2TableHeadersConfig,
  testData3Table,
  testData3TableHeadersConfig,
  testDataFiltersWithNoResults,
  testDataNoFiltersTable,
  testDataNoFiltersTableHeadersConfig,
} from '@common/modules/table-tool/components/__tests__/__data__/timePeriodDataTable.data';
import TimePeriodDataTable from '@common/modules/table-tool/components/TimePeriodDataTable';
import { UnmappedTableHeadersConfig } from '@common/services/permalinkService';
import {
  ReleaseTableDataQuery,
  TableDataResponse,
  TableDataResult,
} from '@common/services/tableBuilderService';
import mapFullTable from '@common/modules/table-tool/utils/mapFullTable';
import mapTableHeadersConfig from '@common/modules/table-tool/utils/mapTableHeadersConfig';
import {
  CategoryFilter,
  Indicator,
  LocationFilter,
  TimePeriodFilter,
} from '@common/modules/table-tool/types/filters';
import { render, screen } from '@testing-library/react';
import React from 'react';
import { FullTable, FullTableMeta } from '../../types/fullTable';
import { TableHeadersConfig } from '../../types/tableHeaders';

describe('TimePeriodDataTable', () => {
  test('renders table with two of every option', () => {
    const fullTable = mapFullTable(testData1Table);

    render(
      <TimePeriodDataTable
        fullTable={fullTable}
        tableHeadersConfig={mapTableHeadersConfig(
          testData1TableHeadersConfig,
          fullTable,
        )}
      />,
    );

    const table = screen.getByRole('table');

    expect(table.querySelectorAll('thead tr')).toHaveLength(2);
    expect(table.querySelectorAll('thead th')).toHaveLength(6);
    expect(table.querySelectorAll('thead th[scope="colgroup"]')).toHaveLength(
      2,
    );
    expect(table.querySelectorAll('thead th[scope="col"]')).toHaveLength(4);

    expect(table.querySelectorAll('tbody tr')).toHaveLength(8);
    expect(table.querySelectorAll('tbody th')).toHaveLength(14);
    expect(table.querySelectorAll('tbody th[scope="rowgroup"]')).toHaveLength(
      6,
    );
    expect(table.querySelectorAll('tbody th[scope="row"]')).toHaveLength(8);

    expect(table.querySelectorAll('tbody td')).toHaveLength(32);

    expect(table).toMatchSnapshot();
  });

  test('renders title without indicator when there is more than one', () => {
    const fullTable = mapFullTable(testData1Table);

    render(
      <TimePeriodDataTable
        fullTable={fullTable}
        tableHeadersConfig={mapTableHeadersConfig(
          testData1TableHeadersConfig,
          fullTable,
        )}
      />,
    );

    expect(
      screen.getByText(
        "'Absence by characteristic' for Ethnicity Major Asian Total, Ethnicity Major Black Total, State-funded primary and State-funded secondary in Barnet and Barnsley between 2013/14 and 2014/15",
      ),
    ).toBeInTheDocument();
  });

  test('renders table without indicators when there is only one', () => {
    const fullTable = mapFullTable(testData2Table);

    render(
      <TimePeriodDataTable
        fullTable={fullTable}
        tableHeadersConfig={mapTableHeadersConfig(
          testData2TableHeadersConfig,
          fullTable,
        )}
      />,
    );

    const table = screen.getByRole('table');

    expect(table.querySelectorAll('thead tr')).toHaveLength(2);
    expect(table.querySelectorAll('thead th')).toHaveLength(6);
    expect(table.querySelectorAll('thead th[scope="colgroup"]')).toHaveLength(
      2,
    );
    expect(table.querySelectorAll('thead th[scope="col"]')).toHaveLength(4);

    expect(table.querySelectorAll('tbody tr')).toHaveLength(4);
    expect(table.querySelectorAll('tbody th')).toHaveLength(6);
    expect(table.querySelectorAll('tbody th[scope="rowgroup"]')).toHaveLength(
      2,
    );
    expect(table.querySelectorAll('tbody th[scope="row"]')).toHaveLength(4);

    expect(table.querySelectorAll('tbody td')).toHaveLength(16);

    expect(table).toMatchSnapshot();
  });

  test('renders title with indicator when there is only one', () => {
    const fullTable = mapFullTable(testData2Table);

    render(
      <TimePeriodDataTable
        fullTable={fullTable}
        tableHeadersConfig={mapTableHeadersConfig(
          testData2TableHeadersConfig,
          fullTable,
        )}
      />,
    );

    expect(
      screen.getByText(
        "Authorised absence rate for 'Absence by characteristic' for Ethnicity Major Asian Total, Ethnicity Major Black Total, State-funded primary and State-funded secondary in Barnet and Barnsley between 2013/14 and 2014/15",
      ),
    ).toBeInTheDocument();
  });

  test('renders table without time period when there is only one', () => {
    const fullTable = mapFullTable(testData3Table);

    render(
      <TimePeriodDataTable
        fullTable={fullTable}
        tableHeadersConfig={mapTableHeadersConfig(
          testData3TableHeadersConfig,
          fullTable,
        )}
      />,
    );

    const table = screen.getByRole('table');

    expect(table.querySelectorAll('thead tr')).toHaveLength(1);
    expect(table.querySelectorAll('thead th')).toHaveLength(2);
    expect(table.querySelectorAll('thead th[scope="colgroup"]')).toHaveLength(
      0,
    );
    expect(table.querySelectorAll('thead th[scope="col"]')).toHaveLength(2);

    expect(table.querySelectorAll('tbody tr')).toHaveLength(4);
    expect(table.querySelectorAll('tbody th')).toHaveLength(6);
    expect(table.querySelectorAll('tbody th[scope="rowgroup"]')).toHaveLength(
      2,
    );
    expect(table.querySelectorAll('tbody th[scope="row"]')).toHaveLength(4);

    expect(table.querySelectorAll('tbody td')).toHaveLength(8);

    expect(table).toMatchSnapshot();
  });

  test('renders title with time period when there is only one', () => {
    const fullTable = mapFullTable(testData3Table);

    render(
      <TimePeriodDataTable
        fullTable={fullTable}
        tableHeadersConfig={mapTableHeadersConfig(
          testData3TableHeadersConfig,
          fullTable,
        )}
      />,
    );

    expect(
      screen.getByText(
        "Authorised absence rate for 'Absence by characteristic' for Ethnicity Major Asian Total, Ethnicity Major Black Total, State-funded primary and State-funded secondary in Barnet and Barnsley for 2014/15",
      ),
    ).toBeInTheDocument();
  });

  test('renders table with no filters', () => {
    const fullTable = mapFullTable(testDataNoFiltersTable);

    render(
      <TimePeriodDataTable
        fullTable={fullTable}
        tableHeadersConfig={mapTableHeadersConfig(
          testDataNoFiltersTableHeadersConfig,
          fullTable,
        )}
      />,
    );

    const table = screen.getByRole('table');

    expect(table.querySelectorAll('thead tr')).toHaveLength(1);
    expect(table.querySelectorAll('thead th')).toHaveLength(3);
    expect(table.querySelectorAll('thead th[scope="colgroup"]')).toHaveLength(
      0,
    );
    expect(table.querySelectorAll('thead th[scope="col"]')).toHaveLength(3);

    expect(table.querySelectorAll('tbody tr')).toHaveLength(3);
    expect(table.querySelectorAll('tbody th')).toHaveLength(4);
    expect(table.querySelectorAll('tbody th[scope="rowgroup"]')).toHaveLength(
      1,
    );
    expect(table.querySelectorAll('tbody th[scope="row"]')).toHaveLength(3);

    expect(table.querySelectorAll('tbody td')).toHaveLength(9);

    expect(table).toMatchSnapshot();
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
                id: 'ethnic-group-major',
                label: 'Ethnic group major',
                options: [
                  {
                    label: 'Ethnicity Major Asian Total',
                    value: '598ed9fd-b37e-4e08-baec-08d78f6f2c4d',
                  },
                ],
                order: 0,
              },
            },
            order: 0,
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
        locations: {
          country: [
            {
              id: 'dd590fcf-b0c1-4fa3-8599-d13c0f540793',
              value: 'E92000001',
              label: 'England',
            },
          ],
        },
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
          locationId: 'dd590fcf-b0c1-4fa3-8599-d13c0f540793',
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
          [
            {
              value: 'dd590fcf-b0c1-4fa3-8599-d13c0f540793',
              type: 'Location',
              level: 'country',
            },
          ],
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

    render(
      <TimePeriodDataTable
        fullTable={fullTable}
        tableHeadersConfig={tableHeadersConfig}
      />,
    );

    const table = screen.getByRole('table');

    expect(table.querySelectorAll('thead tr')).toHaveLength(1);
    expect(table.querySelectorAll('thead th')).toHaveLength(1);
    expect(table.querySelectorAll('thead th[scope="colgroup"]')).toHaveLength(
      0,
    );
    expect(table.querySelectorAll('thead th[scope="col"]')).toHaveLength(1);
    expect(table.querySelector('thead th[scope="col"]')).toHaveTextContent(
      'Ethnicity Major Asian Total',
    );

    expect(table.querySelectorAll('tbody tr')).toHaveLength(1);
    expect(table.querySelectorAll('tbody th')).toHaveLength(1);
    expect(table.querySelectorAll('tbody th[scope="rowgroup"]')).toHaveLength(
      0,
    );
    expect(table.querySelectorAll('tbody th[scope="row"]')).toHaveLength(1);
    expect(table.querySelector('tbody th[scope="row"]')).toHaveTextContent(
      'England',
    );

    expect(table.querySelectorAll('tbody td')).toHaveLength(1);

    expect(table).toMatchSnapshot();
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
        locations: {
          country: [
            {
              id: 'dd590fcf-b0c1-4fa3-8599-d13c0f540793',
              value: 'E92000001',
              label: 'England',
            },
          ],
        },
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
          locationId: 'dd590fcf-b0c1-4fa3-8599-d13c0f540793',
          measures: { '6160c4f8-4c9f-40f0-a623-2a4f742860af': '18.3' },
          timePeriod: '2014_AY',
        },
      ],
    } as TableDataResponse);

    const tableHeadersConfig = mapTableHeadersConfig(
      {
        columnGroups: [],
        rowGroups: [
          [
            {
              value: 'dd590fcf-b0c1-4fa3-8599-d13c0f540793',
              level: 'country',
              type: 'Location',
            },
          ],
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

    render(
      <TimePeriodDataTable
        fullTable={fullTable}
        tableHeadersConfig={tableHeadersConfig}
      />,
    );

    const table = screen.getByRole('table');

    expect(table.querySelectorAll('thead tr')).toHaveLength(1);
    expect(table.querySelectorAll('thead th')).toHaveLength(1);
    expect(table.querySelectorAll('thead th[scope="colgroup"]')).toHaveLength(
      0,
    );
    expect(table.querySelectorAll('thead th[scope="col"]')).toHaveLength(1);
    expect(table.querySelector('thead th[scope="col"]')).toHaveTextContent(
      '2014/15',
    );

    expect(table.querySelectorAll('tbody tr')).toHaveLength(1);
    expect(table.querySelectorAll('tbody th')).toHaveLength(1);
    expect(table.querySelectorAll('tbody th[scope="rowgroup"]')).toHaveLength(
      0,
    );
    expect(table.querySelectorAll('tbody th[scope="row"]')).toHaveLength(1);
    expect(table.querySelector('tbody th[scope="row"]')).toHaveTextContent(
      'England',
    );

    expect(table.querySelectorAll('tbody td')).toHaveLength(1);

    expect(table).toMatchSnapshot();
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
        locations: {
          country: [
            {
              id: 'dd590fcf-b0c1-4fa3-8599-d13c0f540793',
              value: 'E92000001',
              label: 'England',
            },
          ],
        },
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
          locationId: 'dd590fcf-b0c1-4fa3-8599-d13c0f540793',
          measures: { '6160c4f8-4c9f-40f0-a623-2a4f742860af': '18.3' },
          timePeriod: '2014_AY',
        },
      ],
    } as TableDataResponse);

    const tableHeadersConfig = mapTableHeadersConfig(
      {
        columnGroups: [],
        rowGroups: [
          [
            {
              value: 'dd590fcf-b0c1-4fa3-8599-d13c0f540793',
              type: 'Location',
              level: 'country',
            },
          ],
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

    render(
      <TimePeriodDataTable
        fullTable={fullTable}
        tableHeadersConfig={tableHeadersConfig}
      />,
    );

    const table = screen.getByRole('table');

    expect(table.querySelectorAll('thead tr')).toHaveLength(1);
    expect(table.querySelectorAll('thead th')).toHaveLength(1);
    expect(table.querySelectorAll('thead th[scope="colgroup"]')).toHaveLength(
      0,
    );
    expect(table.querySelectorAll('thead th[scope="col"]')).toHaveLength(1);
    expect(table.querySelector('thead th[scope="col"]')).toHaveTextContent(
      '2014/15',
    );

    expect(table.querySelectorAll('tbody tr')).toHaveLength(1);
    expect(table.querySelectorAll('tbody th')).toHaveLength(1);
    expect(table.querySelectorAll('tbody th[scope="rowgroup"]')).toHaveLength(
      0,
    );
    expect(table.querySelectorAll('tbody th[scope="row"]')).toHaveLength(1);
    expect(table.querySelector('tbody th[scope="row"]')).toHaveTextContent(
      'England',
    );

    expect(table.querySelectorAll('tbody td')).toHaveLength(1);

    expect(table).toMatchSnapshot();
  });

  test('renders table with location hierarchies', () => {
    const fullTable = mapFullTable({
      subjectMeta: {
        filters: {},
        footnotes: [],
        indicators: [
          {
            value: 'authorised-absence-rate',
            label: 'Authorised absence rate',
            unit: '%',
            name: 'sess_authorised_percent',
            decimalPlaces: 1,
          },
        ],
        // Contains a mixture of hierarchical (LAs) and flat (country) locations
        locations: {
          country: [
            {
              id: 'dd590fcf-b0c1-4fa3-8599-d13c0f540793',
              value: 'england',
              label: 'England',
            },
          ],
          localAuthority: [
            {
              value: 'yorkshire',
              label: 'Yorkshire and the Humber',
              level: 'region',
              options: [
                {
                  id: 'c6f2a76f-d959-452f-a8e5-593066c7d6d4',
                  value: 'barnsley',
                  label: 'Barnsley',
                },
              ],
            },
            {
              value: 'outer-london',
              label: 'Outer London',
              level: 'region',
              options: [
                {
                  id: 'b63e4d6d-973c-4c29-9b49-2fbc83eff666',
                  value: 'barnet',
                  label: 'Barnet',
                },
              ],
            },
          ],
        },
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
          locationId: 'dd590fcf-b0c1-4fa3-8599-d13c0f540793',
          measures: { 'authorised-absence-rate': '18.3' },
          timePeriod: '2014_AY',
        },
        {
          filters: [],
          geographicLevel: 'localAuthority',
          locationId: 'b63e4d6d-973c-4c29-9b49-2fbc83eff666',
          measures: { 'authorised-absence-rate': '20.2' },
          timePeriod: '2014_AY',
        },
        {
          filters: [],
          geographicLevel: 'localAuthority',
          locationId: 'c6f2a76f-d959-452f-a8e5-593066c7d6d4',
          measures: { 'authorised-absence-rate': '21.5' },
          timePeriod: '2014_AY',
        },
      ],
    } as TableDataResponse);

    const tableHeadersConfig = mapTableHeadersConfig(
      {
        columnGroups: [],
        rowGroups: [
          [
            {
              value: 'dd590fcf-b0c1-4fa3-8599-d13c0f540793',
              level: 'country',
              type: 'Location',
            },
            {
              value: 'c6f2a76f-d959-452f-a8e5-593066c7d6d4',
              level: 'localAuthority',
              type: 'Location',
            },
            {
              value: 'b63e4d6d-973c-4c29-9b49-2fbc83eff666',
              level: 'localAuthority',
              type: 'Location',
            },
          ],
        ],
        columns: [{ value: '2014_AY', type: 'TimePeriod' }],
        rows: [
          {
            value: 'authorised-absence-rate',
            type: 'Indicator',
          },
        ],
      },
      fullTable,
    );

    render(
      <TimePeriodDataTable
        fullTable={fullTable}
        tableHeadersConfig={tableHeadersConfig}
      />,
    );

    const table = screen.getByRole('table');

    expect(table.querySelectorAll('thead tr')).toHaveLength(1);
    expect(table.querySelectorAll('thead th')).toHaveLength(1);
    expect(table.querySelectorAll('thead th[scope="colgroup"]')).toHaveLength(
      0,
    );
    expect(table.querySelectorAll('thead th[scope="col"]')).toHaveLength(1);
    expect(table.querySelector('thead th[scope="col"]')).toHaveTextContent(
      '2014/15',
    );

    const rows = table.querySelectorAll('tbody tr');

    expect(rows).toHaveLength(3);

    // Row 1

    const row1Headers = rows[0].querySelectorAll('th');
    const row1Cells = rows[0].querySelectorAll('td');

    expect(row1Headers).toHaveLength(1);

    // England should take up two columns so that we don't get an
    // asymmetric table due to the local authority options having hierarchies
    expect(row1Headers[0]).toHaveAttribute('scope', 'row');
    expect(row1Headers[0]).toHaveAttribute('colspan', '2');
    expect(row1Headers[0]).toHaveTextContent('England');

    expect(row1Cells).toHaveLength(1);
    expect(row1Cells[0]).toHaveTextContent('18.3%');

    // Row 2

    const row2Headers = rows[1].querySelectorAll('th');
    const row2Cells = rows[1].querySelectorAll('td');

    expect(row2Headers).toHaveLength(2);

    expect(row2Headers[0]).toHaveAttribute('scope', 'rowgroup');
    expect(row2Headers[0]).toHaveAttribute('colspan', '1');
    expect(row2Headers[0]).toHaveTextContent('Yorkshire and the Humber');

    expect(row2Headers[1]).toHaveAttribute('scope', 'row');
    expect(row2Headers[1]).toHaveAttribute('colspan', '1');
    expect(row2Headers[1]).toHaveTextContent('Barnsley');

    expect(row2Cells).toHaveLength(1);
    expect(row2Cells[0]).toHaveTextContent('21.5%');

    // Row 3

    const row3Headers = rows[2].querySelectorAll('th');
    const row3Cells = rows[2].querySelectorAll('td');

    expect(row3Headers).toHaveLength(2);

    expect(row3Headers[0]).toHaveAttribute('scope', 'rowgroup');
    expect(row3Headers[0]).toHaveAttribute('colspan', '1');
    expect(row3Headers[0]).toHaveTextContent('Outer London');

    expect(row3Headers[1]).toHaveAttribute('scope', 'row');
    expect(row3Headers[1]).toHaveAttribute('colspan', '1');
    expect(row3Headers[1]).toHaveTextContent('Barnet');

    expect(row3Cells).toHaveLength(1);
    expect(row3Cells[0]).toHaveTextContent('20.2%');

    expect(screen.getByRole('table')).toMatchSnapshot();
  });

  test('renders table with empty cells when region group is missing', () => {
    const fullTable = mapFullTable({
      subjectMeta: {
        filters: {},
        footnotes: [],
        indicators: [
          {
            value: 'authorised-absence-rate',
            label: 'Authorised absence rate',
            unit: '%',
            name: 'sess_authorised_percent',
            decimalPlaces: 1,
          },
        ],
        // LADs without region info
        locations: {
          country: [
            {
              id: 'dd590fcf-b0c1-4fa3-8599-d13c0f540793',
              value: 'england',
              label: 'England',
            },
          ],
          localAuthorityDistrict: [
            {
              value: '',
              label: '',
              level: 'region',
              options: [
                {
                  id: 'c6f2a76f-d959-452f-a8e5-593066c7d6d4',
                  value: 'barnsley',
                  label: 'Barnsley',
                },
                {
                  id: 'b63e4d6d-973c-4c29-9b49-2fbc83eff666',
                  value: 'barnet',
                  label: 'Barnet',
                },
              ],
            },
          ],
        },
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
          locationId: 'dd590fcf-b0c1-4fa3-8599-d13c0f540793',
          measures: { 'authorised-absence-rate': '18.3' },
          timePeriod: '2014_AY',
        },
        {
          filters: [],
          geographicLevel: 'localAuthorityDistrict',
          locationId: 'b63e4d6d-973c-4c29-9b49-2fbc83eff666',
          measures: { 'authorised-absence-rate': '20.2' },
          timePeriod: '2014_AY',
        },
        {
          filters: [],
          geographicLevel: 'localAuthorityDistrict',
          locationId: 'c6f2a76f-d959-452f-a8e5-593066c7d6d4',
          measures: { 'authorised-absence-rate': '21.5' },
          timePeriod: '2014_AY',
        },
      ],
    } as TableDataResponse);

    const tableHeadersConfig = mapTableHeadersConfig(
      {
        columnGroups: [],
        rowGroups: [
          [
            {
              value: 'dd590fcf-b0c1-4fa3-8599-d13c0f540793',
              level: 'country',
              type: 'Location',
            },
            {
              value: 'c6f2a76f-d959-452f-a8e5-593066c7d6d4',
              level: 'localAuthorityDistrict',
              type: 'Location',
            },
            {
              value: 'b63e4d6d-973c-4c29-9b49-2fbc83eff666',
              level: 'localAuthorityDistrict',
              type: 'Location',
            },
          ],
        ],
        columns: [{ value: '2014_AY', type: 'TimePeriod' }],
        rows: [
          {
            value: 'authorised-absence-rate',
            type: 'Indicator',
          },
        ],
      },
      fullTable,
    );

    render(
      <TimePeriodDataTable
        fullTable={fullTable}
        tableHeadersConfig={tableHeadersConfig}
      />,
    );

    const table = screen.getByRole('table');

    expect(table.querySelectorAll('thead tr')).toHaveLength(1);
    expect(table.querySelectorAll('thead th')).toHaveLength(1);
    expect(table.querySelectorAll('thead th[scope="colgroup"]')).toHaveLength(
      0,
    );
    expect(table.querySelectorAll('thead th[scope="col"]')).toHaveLength(1);
    expect(table.querySelector('thead th[scope="col"]')).toHaveTextContent(
      '2014/15',
    );
    expect(table.querySelectorAll('thead td')).toHaveLength(1);

    const rows = table.querySelectorAll('tbody tr');

    expect(rows).toHaveLength(3);

    // Row 1

    const row1Headers = rows[0].querySelectorAll('th');
    const row1Cells = rows[0].querySelectorAll('td');

    expect(row1Headers).toHaveLength(1);

    // England should take up two columns so that we don't get an
    // asymmetric table due to the local authority options having hierarchies
    expect(row1Headers[0]).toHaveAttribute('scope', 'row');
    expect(row1Headers[0]).toHaveAttribute('colspan', '2');
    expect(row1Headers[0]).toHaveTextContent('England');

    expect(row1Cells).toHaveLength(1);
    expect(row1Cells[0]).toHaveTextContent('18.3%');

    // Row 2

    const row2Headers = rows[1].querySelectorAll('th');
    const row2Cells = rows[1].querySelectorAll('td');

    expect(row2Headers).toHaveLength(1);

    expect(row2Headers[0]).toHaveAttribute('scope', 'row');
    expect(row2Headers[0]).toHaveAttribute('colspan', '1');
    expect(row2Headers[0]).toHaveTextContent('Barnsley');

    expect(row2Cells).toHaveLength(2);
    expect(row2Cells[0]).toHaveTextContent('');
    expect(row2Cells[0]).toHaveAttribute('rowspan', '2');
    expect(row2Cells[1]).toHaveTextContent('21.5%');

    // Row 3
    const row3Headers = rows[2].querySelectorAll('th');
    const row3Cells = rows[2].querySelectorAll('td');

    expect(row3Headers).toHaveLength(1);

    expect(row3Headers[0]).toHaveAttribute('scope', 'row');
    expect(row3Headers[0]).toHaveAttribute('colspan', '1');
    expect(row3Headers[0]).toHaveTextContent('Barnet');

    expect(row3Cells).toHaveLength(1);
    expect(row3Cells[0]).toHaveTextContent('20.2%');

    expect(screen.getByRole('table')).toMatchSnapshot();
  });

  test('renders table with completely empty rows removed', () => {
    const fullTable = mapFullTable(testDataFiltersWithNoResults);

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
          {
            value: '206db2c4-76f9-4dbd-9a96-6927480625ec',
            type: 'Location',
            level: 'localAuthority',
          },
          {
            value: '5c067998-b851-4e03-83b3-c11f71c07a4d',
            type: 'Location',
            level: 'localAuthority',
          },
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

    render(
      <TimePeriodDataTable
        fullTable={fullTable}
        tableHeadersConfig={mapTableHeadersConfig(
          tableHeadersConfig,
          fullTable,
        )}
      />,
    );

    const table = screen.getByRole('table');

    expect(table.querySelectorAll('thead tr')).toHaveLength(2);
    expect(table.querySelectorAll('thead th')).toHaveLength(4);
    expect(table.querySelectorAll('thead th[scope="colgroup"]')).toHaveLength(
      1,
    );
    expect(table.querySelectorAll('thead th[scope="col"]')).toHaveLength(3);

    expect(table.querySelectorAll('tbody tr')).toHaveLength(6);
    expect(table.querySelectorAll('tbody th')).toHaveLength(11);
    expect(table.querySelectorAll('tbody th[scope="rowgroup"]')).toHaveLength(
      5,
    );
    expect(table.querySelectorAll('tbody th[scope="row"]')).toHaveLength(6);

    expect(table.querySelectorAll('tbody td')).toHaveLength(18);

    expect(table).toMatchSnapshot();
  });

  test('renders table with completely empty columns removed', () => {
    const fullTable = mapFullTable(testDataFiltersWithNoResults);

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
          {
            value: '206db2c4-76f9-4dbd-9a96-6927480625ec',
            type: 'Location',
            level: 'localAuthority',
          },
          {
            value: '5c067998-b851-4e03-83b3-c11f71c07a4d',
            type: 'Location',
            level: 'localAuthority',
          },
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

    render(
      <TimePeriodDataTable
        fullTable={fullTable}
        tableHeadersConfig={mapTableHeadersConfig(
          tableHeadersConfig,
          fullTable,
        )}
      />,
    );

    const table = screen.getByRole('table');

    expect(table.querySelectorAll('thead tr')).toHaveLength(2);
    expect(table.querySelectorAll('thead th')).toHaveLength(6);
    expect(table.querySelectorAll('thead th[scope="colgroup"]')).toHaveLength(
      2,
    );
    expect(table.querySelectorAll('thead th[scope="col"]')).toHaveLength(4);

    expect(table.querySelectorAll('tbody tr')).toHaveLength(4);
    expect(table.querySelectorAll('tbody th')).toHaveLength(8);
    expect(table.querySelectorAll('tbody th[scope="rowgroup"]')).toHaveLength(
      4,
    );
    expect(table.querySelectorAll('tbody th[scope="row"]')).toHaveLength(4);

    expect(table.querySelectorAll('tbody td')).toHaveLength(16);

    expect(table).toMatchSnapshot();
  });

  test('renders table correctly with no `dataBlockId` ', () => {
    const fullTable = mapFullTable(testDataFiltersWithNoResults);

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
          {
            value: '206db2c4-76f9-4dbd-9a96-6927480625ec',
            type: 'Location',
            level: 'localAuthority',
          },
          {
            value: '5c067998-b851-4e03-83b3-c11f71c07a4d',
            type: 'Location',
            level: 'localAuthority',
          },
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

    render(
      <TimePeriodDataTable
        fullTable={fullTable}
        tableHeadersConfig={mapTableHeadersConfig(
          tableHeadersConfig,
          fullTable,
        )}
      />,
    );
    expect(screen.getByRole('table')).toHaveAttribute(
      'aria-labelledby',
      'dataTableCaption',
    );
    expect(screen.getByTestId('dataTableCaption')).toHaveAttribute(
      'id',
      'dataTableCaption',
    );
  });

  test('renders table & caption correctly with `dataBlockId`', () => {
    const fullTable = mapFullTable(testDataFiltersWithNoResults);

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
          {
            value: '206db2c4-76f9-4dbd-9a96-6927480625ec',
            type: 'Location',
            level: 'localAuthority',
          },
          {
            value: '5c067998-b851-4e03-83b3-c11f71c07a4d',
            type: 'Location',
            level: 'localAuthority',
          },
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

    render(
      <TimePeriodDataTable
        dataBlockId="test-datablock-id"
        fullTable={fullTable}
        tableHeadersConfig={mapTableHeadersConfig(
          tableHeadersConfig,
          fullTable,
        )}
      />,
    );

    expect(screen.getByRole('table')).toHaveAttribute(
      'aria-labelledby',
      'dataTableCaption-test-datablock-id',
    );
    expect(screen.getByTestId('dataTableCaption')).toHaveAttribute(
      'id',
      'dataTableCaption-test-datablock-id',
    );
  });

  describe('excluded rows or columns warning', () => {
    const testSubjectMeta: FullTableMeta = {
      filters: {
        Category1: {
          name: 'category_1',
          options: [
            new CategoryFilter({
              value: 'filter-1',
              label: 'Filter 1 Option 1',
              category: 'Category 1',
            }),
          ],
          order: 0,
        },
        Category2: {
          name: 'category_2',
          options: [
            new CategoryFilter({
              value: 'filter-2',
              label: 'Filter 2 Option 1',
              category: 'Category 2',
            }),
          ],
          order: 1,
        },
      },
      footnotes: [],
      indicators: [
        new Indicator({
          value: 'indicator-1',
          label: 'Indicator 1',
          unit: '',
          name: 'indicator_1',
        }),
      ],
      locations: [
        new LocationFilter({
          value: 'location-1',
          label: 'England',
          level: 'country',
        }),
      ],
      boundaryLevels: [],
      publicationName: 'Permanent and fixed-period exclusions in England',
      subjectName: 'Duration of fixed exclusions',
      timePeriodRange: [
        new TimePeriodFilter({
          label: '2006/07',
          year: 2006,
          code: 'AY',
          order: 0,
        }),
        new TimePeriodFilter({
          label: '2007/08',
          year: 2007,
          code: 'AY',
          order: 1,
        }),
        new TimePeriodFilter({
          label: '2008/09',
          year: 2008,
          code: 'AY',
          order: 2,
        }),
      ],
      geoJsonAvailable: true,
    };

    const testResults: TableDataResult[] = [
      {
        filters: ['filter-1', 'filter-2'],
        geographicLevel: 'country',
        locationId: 'location-1',
        measures: {
          'indicator-1': '370',
        },
        timePeriod: '2006_AY',
      },
      {
        filters: ['filter-1', 'filter-2'],
        geographicLevel: 'country',
        locationId: 'location-1',
        measures: {
          'indicator-1': '5',
        },
        timePeriod: '2007_AY',
      },
      {
        filters: ['filter-1', 'filter-2'],
        geographicLevel: 'country',
        locationId: 'location-1',
        measures: {
          'indicator-1': '35',
        },
        timePeriod: '2008_AY',
      },
    ];

    const testTableHeadersConfig: TableHeadersConfig = {
      columnGroups: [],
      rowGroups: [],
      columns: [
        new Indicator({
          value: 'indicator-1',
          label: 'Indicator 1',
          name: 'indicator_1_name',
          unit: '',
        }),
      ],
      rows: [
        new TimePeriodFilter({
          label: '2006/07',
          year: 2006,
          code: 'AY',
          order: 0,
        }),
        new TimePeriodFilter({
          label: '2007/08',
          year: 2007,
          code: 'AY',
          order: 1,
        }),
        new TimePeriodFilter({
          label: '2008/09',
          year: 2008,
          code: 'AY',
          order: 2,
        }),
      ],
    };

    test('shows the warning when a row or column for an indicator is excluded because it has no data', () => {
      const testFullTable: FullTable = {
        subjectMeta: testSubjectMeta,
        results: testResults,
      };

      const testQuery: ReleaseTableDataQuery = {
        subjectId: 'subject-1-id',
        timePeriod: {
          startYear: 2006,
          startCode: 'AY',
          endYear: 2008,
          endCode: 'AY',
        },
        filters: ['filter-2', 'filter-1'],
        indicators: ['indicator-1', 'indicator-2'],
        locationIds: ['location-1'],
        includeGeoJson: false,
        releaseId: 'release-1-id',
      };

      render(
        <TimePeriodDataTable
          fullTable={testFullTable}
          query={testQuery}
          tableHeadersConfig={testTableHeadersConfig}
        />,
      );

      expect(
        screen.getByText(
          'Some rows and columns are not shown in this table as the data does not exist in the underlying file.',
        ),
      ).toBeInTheDocument();
    });

    test('shows the warning when a row or column for a filter is excluded because it has no data', () => {
      const testFullTable: FullTable = {
        subjectMeta: testSubjectMeta,
        results: testResults,
      };

      const testQuery: ReleaseTableDataQuery = {
        subjectId: 'subject-1-id',
        timePeriod: {
          startYear: 2006,
          startCode: 'AY',
          endYear: 2008,
          endCode: 'AY',
        },
        filters: ['filter-2', 'filter-1', 'filter-3'],
        indicators: ['indicator-1'],
        locationIds: ['location-1'],
        includeGeoJson: false,
        releaseId: 'release-1-id',
      };

      render(
        <TimePeriodDataTable
          fullTable={testFullTable}
          query={testQuery}
          tableHeadersConfig={testTableHeadersConfig}
        />,
      );

      expect(
        screen.getByText(
          'Some rows and columns are not shown in this table as the data does not exist in the underlying file.',
        ),
      ).toBeInTheDocument();
    });

    test('shows the warning when a row or column for a location is excluded because it has no data', () => {
      const testFullTable: FullTable = {
        subjectMeta: testSubjectMeta,
        results: testResults,
      };

      const testQuery: ReleaseTableDataQuery = {
        subjectId: 'subject-1-id',
        timePeriod: {
          startYear: 2006,
          startCode: 'AY',
          endYear: 2008,
          endCode: 'AY',
        },
        filters: ['filter-2', 'filter-1', 'filter-3'],
        indicators: ['indicator-1'],
        locationIds: ['location-1', 'location-2'],
        includeGeoJson: false,
        releaseId: 'release-1-id',
      };

      render(
        <TimePeriodDataTable
          fullTable={testFullTable}
          query={testQuery}
          tableHeadersConfig={testTableHeadersConfig}
        />,
      );

      expect(
        screen.getByText(
          'Some rows and columns are not shown in this table as the data does not exist in the underlying file.',
        ),
      ).toBeInTheDocument();
    });

    test('shows the warning when a row or column for a time period is excluded because it has no data', () => {
      const testFullTable: FullTable = {
        subjectMeta: testSubjectMeta,
        results: [
          {
            filters: ['filter-1', 'filter-2'],
            geographicLevel: 'country',
            locationId: 'location-1',
            measures: {
              'indicator-1': '370',
            },
            timePeriod: '2006_AY',
          },
          {
            filters: ['filter-1', 'filter-2'],
            geographicLevel: 'country',
            locationId: 'location-1',
            measures: {
              'indicator-1': '35',
            },
            timePeriod: '2008_AY',
          },
        ],
      };

      const testTableHeadersConfigTimePeriods: TableHeadersConfig = {
        columnGroups: [],
        rowGroups: [],
        columns: [
          new Indicator({
            value: 'indicator-1',
            label: 'Indicator 1',
            name: 'indicator_1_name',
            unit: '',
          }),
        ],
        rows: [
          new TimePeriodFilter({
            label: '2006/07',
            year: 2006,
            code: 'AY',
            order: 0,
          }),
          new TimePeriodFilter({
            label: '2008/09',
            year: 2008,
            code: 'AY',
            order: 2,
          }),
        ],
      };

      const testQuery: ReleaseTableDataQuery = {
        subjectId: 'subject-1-id',
        timePeriod: {
          startYear: 2006,
          startCode: 'AY',
          endYear: 2008,
          endCode: 'AY',
        },
        filters: ['filter-2', 'filter-1'],
        indicators: ['indicator-1'],
        locationIds: ['location-1'],
        includeGeoJson: false,
        releaseId: 'release-1-id',
      };

      render(
        <TimePeriodDataTable
          fullTable={testFullTable}
          query={testQuery}
          tableHeadersConfig={testTableHeadersConfigTimePeriods}
        />,
      );

      expect(
        screen.getByText(
          'Some rows and columns are not shown in this table as the data does not exist in the underlying file.',
        ),
      ).toBeInTheDocument();
    });

    test('does not show the warning when no rows or columns are excluded', () => {
      const testFullTable: FullTable = {
        subjectMeta: testSubjectMeta,
        results: testResults,
      };

      const testQuery: ReleaseTableDataQuery = {
        subjectId: 'subject-1-id',
        timePeriod: {
          startYear: 2006,
          startCode: 'AY',
          endYear: 2008,
          endCode: 'AY',
        },
        filters: ['filter-2', 'filter-1'],
        indicators: ['indicator-1'],
        locationIds: ['location-1'],
        includeGeoJson: false,
        releaseId: 'release-1-id',
      };

      render(
        <TimePeriodDataTable
          fullTable={testFullTable}
          query={testQuery}
          tableHeadersConfig={testTableHeadersConfig}
        />,
      );

      expect(
        screen.queryByText(
          'Some rows and columns are not shown in this table as the data does not exist in the underlying file.',
        ),
      ).not.toBeInTheDocument();
    });
  });
});
