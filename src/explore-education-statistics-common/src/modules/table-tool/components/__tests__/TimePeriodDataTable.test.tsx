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
} from '@common/services/tableBuilderService';
import mapFullTable from '@common/modules/table-tool/utils/mapFullTable';
import mapTableHeadersConfig from '@common/modules/table-tool/utils/mapTableHeadersConfig';
import { render, screen } from '@testing-library/react';
import React from 'react';

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
                    value: 'filter-1',
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
            value: 'indicator-6',
            label: 'Authorised absence rate',
            unit: '%',
            name: 'sess_authorised_percent',
            decimalPlaces: 1,
          },
        ],
        locations: {
          country: [
            {
              id: 'location-5',
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
          filters: ['filter-1'],
          geographicLevel: 'country',
          locationId: 'location-5',
          measures: { 'indicator-6': '18.3' },
          timePeriod: '2014_AY',
        },
      ],
    } as TableDataResponse);

    const tableHeadersConfig = mapTableHeadersConfig(
      {
        columnGroups: [
          [
            {
              value: 'filter-1',
              type: 'Filter',
            },
          ],
        ],
        rowGroups: [
          [
            {
              value: 'location-5',
              type: 'Location',
              level: 'country',
            },
          ],
        ],
        columns: [{ value: '2014_AY', type: 'TimePeriod' }],
        rows: [
          {
            value: 'indicator-6',
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
            value: 'indicator-6',
            label: 'Authorised absence rate',
            unit: '%',
            name: 'sess_authorised_percent',
            decimalPlaces: 1,
          },
        ],
        locations: {
          country: [
            {
              id: 'location-5',
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
          locationId: 'location-5',
          measures: { 'indicator-6': '18.3' },
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
              value: 'location-5',
              level: 'country',
              type: 'Location',
            },
          ],
        ],
        columns: [{ value: '2014_AY', type: 'TimePeriod' }],
        rows: [
          {
            value: 'indicator-6',
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
            value: 'indicator-6',
            label: 'Authorised absence rate',
            name: 'sess_authorised_percent',
            unit: '%',
          },
        ],
        locations: {
          country: [
            {
              id: 'location-5',
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
          locationId: 'location-5',
          measures: { 'indicator-6': '18.3' },
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
              value: 'location-5',
              type: 'Location',
              level: 'country',
            },
          ],
        ],
        columns: [{ value: '2014_AY', type: 'TimePeriod' }],
        rows: [
          {
            value: 'indicator-6',
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
              id: 'location-5',
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
                  id: 'location-2',
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
                  id: 'location-1',
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
          locationId: 'location-5',
          measures: { 'authorised-absence-rate': '18.3' },
          timePeriod: '2014_AY',
        },
        {
          filters: [],
          geographicLevel: 'localAuthority',
          locationId: 'location-1',
          measures: { 'authorised-absence-rate': '20.2' },
          timePeriod: '2014_AY',
        },
        {
          filters: [],
          geographicLevel: 'localAuthority',
          locationId: 'location-2',
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
              value: 'location-5',
              level: 'country',
              type: 'Location',
            },
            {
              value: 'location-2',
              level: 'localAuthority',
              type: 'Location',
            },
            {
              value: 'location-1',
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

  test('renders table correctly when region group is missing in row headers', () => {
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
              id: 'location-5',
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
                  id: 'location-2',
                  value: 'barnsley',
                  label: 'Barnsley',
                },
                {
                  id: 'location-1',
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
          locationId: 'location-5',
          measures: { 'authorised-absence-rate': '18.3' },
          timePeriod: '2014_AY',
        },
        {
          filters: [],
          geographicLevel: 'localAuthorityDistrict',
          locationId: 'location-1',
          measures: { 'authorised-absence-rate': '20.2' },
          timePeriod: '2014_AY',
        },
        {
          filters: [],
          geographicLevel: 'localAuthorityDistrict',
          locationId: 'location-2',
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
              value: 'location-5',
              level: 'country',
              type: 'Location',
            },
            {
              value: 'location-2',
              level: 'localAuthorityDistrict',
              type: 'Location',
            },
            {
              value: 'location-1',
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

    expect(row1Headers[0]).toHaveAttribute('scope', 'row');
    expect(row1Headers[0]).toHaveAttribute('colspan', '1');
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

    expect(row2Cells).toHaveLength(1);
    expect(row2Cells[0]).toHaveTextContent('21.5%');

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

  test('renders table correctly when region group is missing in row headers and other locations have hierarchy', () => {
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
        // Contains a mixture of hierarchical (LAs), flat (country) locations and an LAD with no region
        locations: {
          country: [
            {
              id: 'location-5',
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
                  id: 'location-2',
                  value: 'barnsley',
                  label: 'Barnsley',
                },
              ],
            },
          ],
          localAuthorityDistrict: [
            {
              id: 'location-1',
              value: 'barnet',
              label: 'Barnet',
              level: 'localAuthorityDistrict',
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
          locationId: 'location-5',
          measures: { 'authorised-absence-rate': '18.3' },
          timePeriod: '2014_AY',
        },
        {
          filters: [],
          geographicLevel: 'localAuthorityDistrict',
          locationId: 'location-1',
          measures: { 'authorised-absence-rate': '20.2' },
          timePeriod: '2014_AY',
        },
        {
          filters: [],
          geographicLevel: 'localAuthority',
          locationId: 'location-2',
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
              value: 'location-5',
              level: 'country',
              type: 'Location',
            },
            {
              value: 'location-2',
              level: 'localAuthority',
              type: 'Location',
            },
            {
              value: 'location-1',
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

    // Barnet should take up two columns so that we don't get an
    // asymmetric table due to the local authority options having hierarchies
    const row3Headers = rows[2].querySelectorAll('th');
    const row3Cells = rows[2].querySelectorAll('td');

    expect(row3Headers).toHaveLength(1);

    expect(row3Headers[0]).toHaveAttribute('scope', 'row');
    expect(row3Headers[0]).toHaveAttribute('colspan', '2');
    expect(row3Headers[0]).toHaveTextContent('Barnet');

    expect(row3Cells).toHaveLength(1);
    expect(row3Cells[0]).toHaveTextContent('20.2%');

    expect(screen.getByRole('table')).toMatchSnapshot();
  });

  test('renders table correctly when region group is missing in column headers', () => {
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
        locations: {
          country: [
            {
              id: 'location-5',
              value: 'england',
              label: 'England',
            },
          ],
          // LADs without region info
          localAuthorityDistrict: [
            {
              value: '',
              label: '',
              level: 'region',
              options: [
                {
                  id: 'location-2',
                  value: 'barnsley',
                  label: 'Barnsley',
                },
                {
                  id: 'location-1',
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
          locationId: 'location-5',
          measures: { 'authorised-absence-rate': '18.3' },
          timePeriod: '2014_AY',
        },
        {
          filters: [],
          geographicLevel: 'localAuthorityDistrict',
          locationId: 'location-1',
          measures: { 'authorised-absence-rate': '20.2' },
          timePeriod: '2014_AY',
        },
        {
          filters: [],
          geographicLevel: 'localAuthorityDistrict',
          locationId: 'location-2',
          measures: { 'authorised-absence-rate': '21.5' },
          timePeriod: '2014_AY',
        },
      ],
    } as TableDataResponse);

    const tableHeadersConfig = mapTableHeadersConfig(
      {
        columnGroups: [
          [
            {
              value: 'location-5',
              level: 'country',
              type: 'Location',
            },
            {
              value: 'location-2',
              level: 'localAuthorityDistrict',
              type: 'Location',
            },
            {
              value: 'location-1',
              level: 'localAuthorityDistrict',
              type: 'Location',
            },
          ],
        ],
        rowGroups: [],
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

    const headerRows = table.querySelectorAll('thead tr');
    expect(headerRows).toHaveLength(1);

    const headerRow1Headers = headerRows[0].querySelectorAll('th');
    expect(headerRow1Headers).toHaveLength(3);

    expect(headerRow1Headers[0]).toHaveAttribute('scope', 'col');
    expect(headerRow1Headers[0]).toHaveAttribute('colspan', '1');
    expect(headerRow1Headers[0]).toHaveAttribute('rowspan', '1');
    expect(headerRow1Headers[0]).toHaveTextContent('England');

    expect(headerRow1Headers[1]).toHaveAttribute('scope', 'col');
    expect(headerRow1Headers[1]).toHaveAttribute('colspan', '1');
    expect(headerRow1Headers[1]).toHaveAttribute('rowspan', '1');
    expect(headerRow1Headers[1]).toHaveTextContent('Barnsley');

    expect(headerRow1Headers[2]).toHaveAttribute('scope', 'col');
    expect(headerRow1Headers[2]).toHaveAttribute('colspan', '1');
    expect(headerRow1Headers[2]).toHaveAttribute('rowspan', '1');
    expect(headerRow1Headers[2]).toHaveTextContent('Barnet');

    expect(screen.getByRole('table')).toMatchSnapshot();
  });

  test('renders table correctly when region group is missing in column headers and other locations have hierarchy', () => {
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
        // Contains a mixture of hierarchical (LAs), flat (country) locations and an LAD with no region
        locations: {
          country: [
            {
              id: 'location-5',
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
                  id: 'location-2',
                  value: 'barnsley',
                  label: 'Barnsley',
                },
              ],
            },
          ],
          localAuthorityDistrict: [
            {
              id: 'location-1',
              value: 'barnet',
              label: 'Barnet',
              level: 'localAuthorityDistrict',
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
          locationId: 'location-5',
          measures: { 'authorised-absence-rate': '18.3' },
          timePeriod: '2014_AY',
        },
        {
          filters: [],
          geographicLevel: 'localAuthorityDistrict',
          locationId: 'location-1',
          measures: { 'authorised-absence-rate': '20.2' },
          timePeriod: '2014_AY',
        },
        {
          filters: [],
          geographicLevel: 'localAuthority',
          locationId: 'location-2',
          measures: { 'authorised-absence-rate': '21.5' },
          timePeriod: '2014_AY',
        },
      ],
    } as TableDataResponse);

    const tableHeadersConfig = mapTableHeadersConfig(
      {
        columnGroups: [
          [
            {
              value: 'location-5',
              level: 'country',
              type: 'Location',
            },
            {
              value: 'location-2',
              level: 'localAuthority',
              type: 'Location',
            },
            {
              value: 'location-1',
              level: 'localAuthorityDistrict',
              type: 'Location',
            },
          ],
        ],
        rowGroups: [],
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

    const headerRows = table.querySelectorAll('thead tr');
    expect(headerRows).toHaveLength(2);

    const headerRow1Headers = headerRows[0].querySelectorAll('th');
    expect(headerRow1Headers).toHaveLength(3);

    expect(headerRow1Headers[0]).toHaveAttribute('scope', 'col');
    expect(headerRow1Headers[0]).toHaveAttribute('colspan', '1');
    expect(headerRow1Headers[0]).toHaveAttribute('rowspan', '2');
    expect(headerRow1Headers[0]).toHaveTextContent('England');

    expect(headerRow1Headers[1]).toHaveAttribute('scope', 'colgroup');
    expect(headerRow1Headers[1]).toHaveAttribute('colspan', '1');
    expect(headerRow1Headers[1]).toHaveAttribute('rowspan', '1');
    expect(headerRow1Headers[1]).toHaveTextContent('Yorkshire and the Humber');

    expect(headerRow1Headers[2]).toHaveAttribute('scope', 'col');
    expect(headerRow1Headers[2]).toHaveAttribute('colspan', '1');
    expect(headerRow1Headers[2]).toHaveAttribute('rowspan', '2');
    expect(headerRow1Headers[2]).toHaveTextContent('Barnet');

    const headerRow2Headers = headerRows[1].querySelectorAll('th');
    expect(headerRow2Headers).toHaveLength(1);

    expect(headerRow2Headers[0]).toHaveAttribute('scope', 'col');
    expect(headerRow2Headers[0]).toHaveAttribute('colspan', '1');
    expect(headerRow2Headers[0]).toHaveAttribute('rowspan', '1');
    expect(headerRow2Headers[0]).toHaveTextContent('Barnsley');

    expect(screen.getByRole('table')).toMatchSnapshot();
  });

  test('renders table with completely empty rows removed', () => {
    const fullTable = mapFullTable(testDataFiltersWithNoResults);

    const tableHeadersConfig: UnmappedTableHeadersConfig = {
      columnGroups: [
        [
          {
            value: 'filter-7',
            type: 'Filter',
          },
        ],
      ],
      rowGroups: [
        [
          {
            value: 'location-3',
            type: 'Location',
            level: 'localAuthority',
          },
          {
            value: 'location-4',
            type: 'Location',
            level: 'localAuthority',
          },
        ],
        [
          {
            value: 'filter-5',
            type: 'Filter',
          },
          {
            value: 'filter-6',
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
          value: 'indicator-1',
          type: 'Indicator',
        },
        {
          value: 'indicator-2',
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
            value: 'filter-5',
            type: 'Filter',
          },
          {
            value: 'filter-6',
            type: 'Filter',
          },
        ],
      ],
      rowGroups: [
        [
          {
            value: 'location-3',
            type: 'Location',
            level: 'localAuthority',
          },
          {
            value: 'location-4',
            type: 'Location',
            level: 'localAuthority',
          },
        ],
        [
          {
            value: 'filter-7',
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
          value: 'indicator-1',
          type: 'Indicator',
        },
        {
          value: 'indicator-2',
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
            value: 'filter-7',
            type: 'Filter',
          },
        ],
      ],
      rowGroups: [
        [
          {
            value: 'location-3',
            type: 'Location',
            level: 'localAuthority',
          },
          {
            value: 'location-4',
            type: 'Location',
            level: 'localAuthority',
          },
        ],
        [
          {
            value: 'filter-5',
            type: 'Filter',
          },
          {
            value: 'filter-6',
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
          value: 'indicator-1',
          type: 'Indicator',
        },
        {
          value: 'indicator-2',
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
            value: 'filter-7',
            type: 'Filter',
          },
        ],
      ],
      rowGroups: [
        [
          {
            value: 'location-3',
            type: 'Location',
            level: 'localAuthority',
          },
          {
            value: 'location-4',
            type: 'Location',
            level: 'localAuthority',
          },
        ],
        [
          {
            value: 'filter-5',
            type: 'Filter',
          },
          {
            value: 'filter-6',
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
          value: 'indicator-1',
          type: 'Indicator',
        },
        {
          value: 'indicator-2',
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

  test('does not show the missing rows or columns warning when none are missing', () => {
    const fullTable = mapFullTable(testData1Table);

    const testQuery: ReleaseTableDataQuery = {
      subjectId: 'subject-1-id',
      timePeriod: {
        startYear: 2013,
        startCode: 'AY',
        endYear: 2014,
        endCode: 'AY',
      },
      filters: ['filter-1', 'filter-2', 'filter-3', 'filter-4'],
      indicators: ['indicator-2', 'indicator-1'],
      locationIds: ['location-2', 'location-1'],
      includeGeoJson: false,
      releaseId: 'release-1-id',
    };

    render(
      <TimePeriodDataTable
        fullTable={fullTable}
        tableHeadersConfig={mapTableHeadersConfig(
          testData1TableHeadersConfig,
          fullTable,
        )}
        query={testQuery}
      />,
    );

    expect(
      screen.queryByTestId('missing-data-warning'),
    ).not.toBeInTheDocument();
  });

  test('shows the missing rows or columns warning when some are missing', () => {
    const fullTable = mapFullTable(testData1Table);

    const testQuery: ReleaseTableDataQuery = {
      subjectId: 'subject-1-id',
      timePeriod: {
        startYear: 2013,
        startCode: 'AY',
        endYear: 2014,
        endCode: 'AY',
      },
      filters: ['filter-1', 'filter-4'], // filter-2 and filter-3 are missing
      indicators: ['indicator-2', 'indicator-1'],
      locationIds: ['location-2', 'location-1'],
      includeGeoJson: false,
      releaseId: 'release-1-id',
    };

    render(
      <TimePeriodDataTable
        fullTable={fullTable}
        tableHeadersConfig={mapTableHeadersConfig(
          testData1TableHeadersConfig,
          fullTable,
        )}
        query={testQuery}
      />,
    );

    expect(
      screen.getByText(
        'Some rows and columns are not shown in this table as the data does not exist in the underlying file.',
      ),
    ).toBeInTheDocument();
  });
});
