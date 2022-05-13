import { UnmappedTableHeadersConfig } from '@common/services/permalinkService';
import {
  CategoryFilter,
  Indicator,
  LocationFilter,
  TimePeriodFilter,
} from '@common/modules/table-tool/types/filters';
import { FullTable } from '@common/modules/table-tool/types/fullTable';
import mapFullTable from '@common/modules/table-tool/utils/mapFullTable';
import mapTableHeadersConfig from '@common/modules/table-tool/utils/mapTableHeadersConfig';
import { TableDataResponse } from '@common/services/tableBuilderService';

describe('mapTableHeadersConfig', () => {
  const testHeaderConfig: UnmappedTableHeadersConfig = {
    columnGroups: [
      [
        {
          value: '598ed9fd-b37e-4e08-baec-08d78f6f2c4d',
          type: 'Filter',
        },
        {
          value: '067de12b-014b-4bbd-baf1-08d78f6f2c4d',
          type: 'Filter',
        },
      ],
    ],
    rowGroups: [
      [
        {
          value: 'd7e7e412-f462-444f-84ac-3454fa471cb8',
          type: 'Filter',
        },
        {
          value: 'a9fe9fa6-e91f-460b-a0b1-66877b97c581',
          type: 'Filter',
        },
      ],
      [
        {
          value: 'b63e4d6d-973c-4c29-9b49-2fbc83eff666',
          type: 'Location',
          level: 'localAuthority',
        },
        {
          value: 'c6f2a76f-d959-452f-a8e5-593066c7d6d4',
          type: 'Location',
          level: 'localAuthority',
        },
      ],
    ],
    columns: [
      { value: '2014_AY', type: 'TimePeriod' },
      { value: '2015_AY', type: 'TimePeriod' },
    ],
    rows: [
      {
        value: '9cf0dcf1-367e-4207-2b50-08d78f6f2b08',
        type: 'Indicator',
      },
      {
        value: 'd1c4a0be-8756-470d-2b51-08d78f6f2b08',
        type: 'Indicator',
      },
      {
        value: '63043c27-f055-472f-2b56-08d78f6f2b08',
        type: 'Indicator',
      },
    ],
  };

  const testTableData: TableDataResponse = {
    subjectMeta: {
      filters: {
        Characteristic: {
          totalValue: '',
          hint: 'Filter by pupil characteristic',
          legend: 'Characteristic',
          name: 'characteristic',
          options: {
            EthnicGroupMajor: {
              id: '872e2ee8-1972-4db8-9e4b-ba279c544f22',
              label: 'Ethnic group major',
              options: [
                {
                  label: 'Ethnicity Major Asian Total',
                  value: '598ed9fd-b37e-4e08-baec-08d78f6f2c4d',
                },
                {
                  label: 'Ethnicity Major Black Total',
                  value: '067de12b-014b-4bbd-baf1-08d78f6f2c4d',
                },
              ],
              order: 0,
            },
          },
          order: 0,
        },
        SchoolType: {
          totalValue: '',
          hint: 'Filter by school type',
          legend: 'School type',
          name: 'school_type',
          options: {
            Default: {
              id: '02efb195-51e8-4a4b-bba0-f772c1b460ab',
              label: 'Default',
              options: [
                {
                  label: 'State-funded primary',
                  value: 'd7e7e412-f462-444f-84ac-3454fa471cb8',
                },
                {
                  label: 'State-funded secondary',
                  value: 'a9fe9fa6-e91f-460b-a0b1-66877b97c581',
                },
              ],
              order: 0,
            },
          },
          order: 1,
        },
      },
      footnotes: [],
      geoJsonAvailable: false,
      indicators: [
        {
          value: '9cf0dcf1-367e-4207-2b50-08d78f6f2b08',
          label: 'Number of overall absence sessions',
          unit: '',
          name: 'sess_overall,Indicator',
          decimalPlaces: 2,
        },
        {
          value: 'd1c4a0be-8756-470d-2b51-08d78f6f2b08',
          label: 'Number of authorised absence sessions',
          unit: '',
          name: 'sess_overall,Indicator',
          decimalPlaces: 2,
        },
        {
          value: '63043c27-f055-472f-2b56-08d78f6f2b08',
          label: 'Number of persistent absentees',
          unit: '',
          name: 'enrolments_pa_10_exact,Indicator',
          decimalPlaces: 2,
        },
      ],
      locations: {
        localAuthority: [
          {
            id: 'b63e4d6d-973c-4c29-9b49-2fbc83eff666',
            value: 'E09000003',
            label: 'Barnet',
          },
          {
            id: 'c6f2a76f-d959-452f-a8e5-593066c7d6d4',
            value: 'E08000016',
            label: 'Barnsley',
          },
        ],
      },
      boundaryLevels: [],
      publicationName: 'Pupil absence in schools in England',
      subjectName: 'Absence by characteristic',
      timePeriodRange: [
        { label: '2014/15', code: 'AY', year: 2014 },
        { label: '2015/16', code: 'AY', year: 2015 },
      ],
    },
    results: [
      {
        filters: [],
        geographicLevel: '',
        locationId: '',
        measures: {},
        timePeriod: '2014_AY',
      },
      {
        filters: [],
        geographicLevel: '',
        locationId: '',
        measures: {},
        timePeriod: '2015_AY',
      },
    ],
  };

  test('maps headers to their classes', () => {
    const table: FullTable = mapFullTable(testTableData);
    const headers = mapTableHeadersConfig(testHeaderConfig, table);

    expect(headers.columnGroups).toEqual([
      [
        new CategoryFilter({
          value: '598ed9fd-b37e-4e08-baec-08d78f6f2c4d',
          label: 'Ethnicity Major Asian Total',
          group: 'Ethnic group major',
          category: 'Characteristic',
        }),
        new CategoryFilter({
          value: '067de12b-014b-4bbd-baf1-08d78f6f2c4d',
          label: 'Ethnicity Major Black Total',
          group: 'Ethnic group major',
          category: 'Characteristic',
        }),
      ],
    ]);
    expect(headers.rowGroups).toEqual([
      [
        new CategoryFilter({
          value: 'd7e7e412-f462-444f-84ac-3454fa471cb8',
          label: 'State-funded primary',
          group: 'Default',
          category: 'School type',
        }),
        new CategoryFilter({
          value: 'a9fe9fa6-e91f-460b-a0b1-66877b97c581',
          label: 'State-funded secondary',
          group: 'Default',
          category: 'School type',
        }),
      ],
      [
        new LocationFilter({
          id: 'b63e4d6d-973c-4c29-9b49-2fbc83eff666',
          value: 'E09000003',
          label: 'Barnet',
          level: 'localAuthority',
        }),
        new LocationFilter({
          id: 'c6f2a76f-d959-452f-a8e5-593066c7d6d4',
          value: 'E08000016',
          label: 'Barnsley',
          level: 'localAuthority',
        }),
      ],
    ]);
    expect(headers.columns).toEqual([
      new TimePeriodFilter({
        year: 2014,
        code: 'AY',
        label: '2014/15',
        order: 0,
      }),
      new TimePeriodFilter({
        year: 2015,
        code: 'AY',
        label: '2015/16',
        order: 1,
      }),
    ]);
    expect(headers.rows).toEqual([
      new Indicator({
        value: '9cf0dcf1-367e-4207-2b50-08d78f6f2b08',
        label: 'Number of overall absence sessions',
        unit: '',
        name: 'sess_overall,Indicator',
        decimalPlaces: 2,
      }),
      new Indicator({
        value: 'd1c4a0be-8756-470d-2b51-08d78f6f2b08',
        label: 'Number of authorised absence sessions',
        unit: '',
        name: 'sess_overall,Indicator',
        decimalPlaces: 2,
      }),
      new Indicator({
        value: '63043c27-f055-472f-2b56-08d78f6f2b08',
        label: 'Number of persistent absentees',
        unit: '',
        name: 'enrolments_pa_10_exact,Indicator',
        decimalPlaces: 2,
      }),
    ]);
  });

  test('correctly maps locations with same `value` but different `level`', () => {
    const table: FullTable = mapFullTable({
      results: [],
      subjectMeta: {
        publicationName: '',
        subjectName: '',
        filters: {},
        footnotes: [],
        locations: {
          localAuthority: [
            {
              id: 'b63e4d6d-973c-4c29-9b49-2fbc83eff666',
              value: 'E09000003',
              label: 'Barnet (local authority)',
            },
          ],
          localAuthorityDistrict: [
            {
              id: '61a57251-cca7-4a87-df80-08d82d586d38',
              value: 'E09000003',
              label: 'Barnet (local authority district)',
            },
          ],
        },
        indicators: [],
        boundaryLevels: [],
        timePeriodRange: [],
        geoJsonAvailable: false,
      },
    });

    const headers = mapTableHeadersConfig(
      {
        columnGroups: [],
        columns: [],
        rowGroups: [
          [
            {
              type: 'Location',
              value: '61a57251-cca7-4a87-df80-08d82d586d38',
              level: 'localAuthorityDistrict',
            },
            {
              type: 'Location',
              value: 'b63e4d6d-973c-4c29-9b49-2fbc83eff666',
              level: 'localAuthority',
            },
          ],
        ],
        rows: [
          {
            type: 'Location',
            value: 'b63e4d6d-973c-4c29-9b49-2fbc83eff666',
            level: 'localAuthority',
          },
          {
            type: 'Location',
            value: '61a57251-cca7-4a87-df80-08d82d586d38',
            level: 'localAuthorityDistrict',
          },
        ],
      },
      table,
    );

    expect(headers.rowGroups).toEqual([
      [
        new LocationFilter({
          id: '61a57251-cca7-4a87-df80-08d82d586d38',
          value: 'E09000003',
          label: 'Barnet (local authority district)',
          level: 'localAuthorityDistrict',
        }),
        new LocationFilter({
          id: 'b63e4d6d-973c-4c29-9b49-2fbc83eff666',
          value: 'E09000003',
          label: 'Barnet (local authority)',
          level: 'localAuthority',
        }),
      ],
    ]);

    expect(headers.rows).toEqual([
      new LocationFilter({
        id: 'b63e4d6d-973c-4c29-9b49-2fbc83eff666',
        value: 'E09000003',
        label: 'Barnet (local authority)',
        level: 'localAuthority',
      }),
      new LocationFilter({
        id: '61a57251-cca7-4a87-df80-08d82d586d38',
        value: 'E09000003',
        label: 'Barnet (local authority district)',
        level: 'localAuthorityDistrict',
      }),
    ]);
  });

  test('correctly maps term time periods to only return those in the table result', () => {
    const testHeaderConfigWithTerms: UnmappedTableHeadersConfig = {
      ...testHeaderConfig,
      columns: [
        {
          value: '2017_T1',
          type: 'TimePeriod',
        },
        {
          value: '2017_T1T2',
          type: 'TimePeriod',
        },
        {
          value: '2017_T2',
          type: 'TimePeriod',
        },
        {
          value: '2017_T3',
          type: 'TimePeriod',
        },
        {
          value: '2018_T1',
          type: 'TimePeriod',
        },
      ],
    };

    const testTableDataWithTerms = {
      ...testTableData,
      subjectMeta: {
        ...testTableData.subjectMeta,
        timePeriodRange: [
          { code: 'T1', label: '2017/18 Autumn Term', year: 2017 },
          { code: 'T1T2', label: '2017/18 Autumn and Spring Term', year: 2017 },
          { code: 'T2', label: '2017/18 Spring Term', year: 2017 },
          { code: 'T3', label: '2017/18 Summer Term', year: 2017 },
          { code: 'T1', label: '2018/19 Autumn Term', year: 2018 },
        ],
      },
      results: [
        {
          filters: [],
          geographicLevel: '',
          locationId: '',
          measures: {},
          timePeriod: '2018_T1',
        },
        {
          filters: [],
          geographicLevel: '',
          locationId: '',
          measures: {},
          timePeriod: '2017_T1',
        },
      ],
    };

    const table: FullTable = mapFullTable(testTableDataWithTerms);
    const headers = mapTableHeadersConfig(testHeaderConfigWithTerms, table);

    expect(headers.columns).toEqual([
      new TimePeriodFilter({
        year: 2017,
        code: 'T1',
        label: '2017/18 Autumn Term',
        order: 0,
      }),
      new TimePeriodFilter({
        year: 2018,
        code: 'T1',
        label: '2018/19 Autumn Term',
        order: 4,
      }),
    ]);
  });

  test('throws error if there is an invalid header type', () => {
    const fullTable = mapFullTable(testTableData);

    const headers: UnmappedTableHeadersConfig = {
      columnGroups: [],
      columns: [
        {
          value: 'abcd',
          type: 'NotValid' as 'Indicator',
        },
      ],
      rowGroups: [],
      rows: [],
    };

    expect(() => {
      mapTableHeadersConfig(headers, fullTable);
    }).toThrowError(
      `Invalid header type: ${JSON.stringify(headers.columns[0])}`,
    );
  });

  test('throws error if could not find header filter', () => {
    const fullTable = mapFullTable(testTableData);

    const headers: UnmappedTableHeadersConfig = {
      columnGroups: [],
      columns: [
        {
          value: 'will not find me',
          type: 'Indicator',
        },
      ],
      rowGroups: [],
      rows: [],
    };

    expect(() => {
      mapTableHeadersConfig(headers, fullTable);
    }).toThrowError(
      `Could not find matching filter for header: ${JSON.stringify(
        headers.columns[0],
      )}`,
    );
  });

  test('does not map any headers', () => {
    const fullTable = mapFullTable(testTableData);
    const headers = mapTableHeadersConfig(
      {
        columnGroups: [],
        columns: [],
        rowGroups: [],
        rows: [],
      },
      fullTable,
    );

    expect(headers.columnGroups).toEqual([]);
    expect(headers.rowGroups).toEqual([]);
    expect(headers.columns).toEqual([]);
    expect(headers.rows).toEqual([]);
  });
});
