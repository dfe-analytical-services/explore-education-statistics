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
import { render, screen } from '@testing-library/react';
import React from 'react';

describe('TimePeriodDataTable', () => {
  test('renders table with two of every option', () => {
    const fullTable = mapFullTable(testData1.fullTable);

    render(
      <TimePeriodDataTable
        fullTable={fullTable}
        tableHeadersConfig={mapTableHeadersConfig(
          testData1.tableHeadersConfig,
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
    const fullTable = mapFullTable(testData1.fullTable);

    render(
      <TimePeriodDataTable
        fullTable={fullTable}
        tableHeadersConfig={mapTableHeadersConfig(
          testData1.tableHeadersConfig,
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
    const fullTable = mapFullTable(testData2.fullTable);

    render(
      <TimePeriodDataTable
        fullTable={fullTable}
        tableHeadersConfig={mapTableHeadersConfig(
          testData2.tableHeadersConfig,
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
    const fullTable = mapFullTable(testData2.fullTable);

    render(
      <TimePeriodDataTable
        fullTable={fullTable}
        tableHeadersConfig={mapTableHeadersConfig(
          testData2.tableHeadersConfig,
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
    const fullTable = mapFullTable(testData3.fullTable);

    render(
      <TimePeriodDataTable
        fullTable={fullTable}
        tableHeadersConfig={mapTableHeadersConfig(
          testData3.tableHeadersConfig,
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
    const fullTable = mapFullTable(testData3.fullTable);

    render(
      <TimePeriodDataTable
        fullTable={fullTable}
        tableHeadersConfig={mapTableHeadersConfig(
          testData3.tableHeadersConfig,
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
    const fullTable = mapFullTable(testDataNoFilters.fullTable);

    render(
      <TimePeriodDataTable
        fullTable={fullTable}
        tableHeadersConfig={mapTableHeadersConfig(
          testDataNoFilters.tableHeadersConfig,
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
        locationsHierarchical: {
          country: [{ value: 'E92000001', label: 'England' }],
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
        locationsHierarchical: {
          country: [{ value: 'E92000001', label: 'England' }],
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
        locationsHierarchical: {
          country: [{ value: 'E92000001', label: 'England' }],
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
        locationsHierarchical: {
          country: [{ value: 'england', label: 'England' }],
          localAuthority: [
            {
              value: 'yorkshire',
              label: 'Yorkshire and the Humber',
              options: [{ value: 'barnsley', label: 'Barnsley' }],
            },
            {
              value: 'outer-london',
              label: 'Outer London',
              options: [{ value: 'barnet', label: 'Barnet' }],
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
          location: {
            country: { code: 'england', name: 'England' },
          },
          measures: { 'authorised-absence-rate': '18.3' },
          timePeriod: '2014_AY',
        },
        {
          filters: [],
          geographicLevel: 'localAuthority',
          location: {
            localAuthority: { code: 'barnet', name: 'Barnet' },
          },
          measures: { 'authorised-absence-rate': '20.2' },
          timePeriod: '2014_AY',
        },
        {
          filters: [],
          geographicLevel: 'localAuthority',
          location: {
            localAuthority: { code: 'barnsley', name: 'Barnsley' },
          },
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
            { value: 'england', level: 'country', type: 'Location' },
            { value: 'barnsley', level: 'localAuthority', type: 'Location' },
            { value: 'barnet', level: 'localAuthority', type: 'Location' },
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
});
