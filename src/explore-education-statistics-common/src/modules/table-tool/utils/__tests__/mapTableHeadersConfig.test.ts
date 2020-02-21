import { UnmappedTableHeadersConfig } from '@common/modules/table-tool/services/permalinkService';
import {
  CategoryFilter,
  Indicator,
  LocationFilter,
  TimePeriodFilter,
} from '@common/modules/table-tool/types/filters';
import { FullTable } from '@common/modules/table-tool/types/fullTable';
import mapFullTable from '@common/modules/table-tool/utils/mapFullTable';
import mapTableHeadersConfig from '@common/modules/table-tool/utils/mapTableHeadersConfig';

describe('mapTableHeadersConfig', () => {
  const testHeaderConfig: UnmappedTableHeadersConfig = {
    columnGroups: [
      [
        {
          value: '598ed9fd-b37e-4e08-baec-08d78f6f2c4d',
          label: 'Ethnicity Major Asian Total',
        },
        {
          value: '067de12b-014b-4bbd-baf1-08d78f6f2c4d',
          label: 'Ethnicity Major Black Total',
        },
      ],
    ],
    rowGroups: [
      [
        {
          value: 'd7e7e412-f462-444f-84ac-3454fa471cb8',
          label: 'State-funded primary',
        },
        {
          value: 'a9fe9fa6-e91f-460b-a0b1-66877b97c581',
          label: 'State-funded secondary',
        },
      ],
      [
        { value: 'E09000003', label: 'Barnet' },
        { value: 'E08000016', label: 'Barnsley' },
      ],
    ],
    columns: [
      { value: '2014_AY', label: '2014/15' },
      { value: '2015_AY', label: '2015/16' },
    ],
    rows: [
      {
        value: '9cf0dcf1-367e-4207-2b50-08d78f6f2b08',
        label: 'Number of overall absence sessions',
      },
      {
        value: 'd1c4a0be-8756-470d-2b51-08d78f6f2b08',
        label: 'Number of authorised absence sessions',
      },
      {
        value: '63043c27-f055-472f-2b56-08d78f6f2b08',
        label: 'Number of persistent absentees',
      },
    ],
  };

  const testTable: FullTable = mapFullTable({
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
                {
                  label: 'Ethnicity Major Black Total',
                  value: '067de12b-014b-4bbd-baf1-08d78f6f2c4d',
                },
              ],
            },
          },
        },
        SchoolType: {
          totalValue: '',
          hint: 'Filter by school type',
          legend: 'School type',
          options: {
            Default: {
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
            },
          },
        },
      },
      footnotes: [],
      geoJsonAvailable: false,
      indicators: [
        {
          value: '9cf0dcf1-367e-4207-2b50-08d78f6f2b08',
          label: 'Number of overall absence sessions',
          unit: '',
        },
        {
          value: 'd1c4a0be-8756-470d-2b51-08d78f6f2b08',
          label: 'Number of authorised absence sessions',
          unit: '',
        },
        {
          value: '63043c27-f055-472f-2b56-08d78f6f2b08',
          label: 'Number of persistent absentees',
          unit: '',
        },
      ],
      locations: [
        { value: 'E09000003', label: 'Barnet', level: 'localAuthority' },
        { value: 'E08000016', label: 'Barnsley', level: 'localAuthority' },
      ],
      publicationName: 'Pupil absence in schools in England',
      subjectName: 'Absence by characteristic',
      timePeriodRange: [
        { label: '2014/15', code: 'AY', year: 2014 },
        { label: '2015/16', code: 'AY', year: 2015 },
      ],
    },
    results: [],
  });

  test('maps headers to their classes', () => {
    const headers = mapTableHeadersConfig(
      testHeaderConfig,
      testTable.subjectMeta,
    );

    expect(headers.columnGroups).toEqual([
      [
        new CategoryFilter({
          value: '598ed9fd-b37e-4e08-baec-08d78f6f2c4d',
          label: 'Ethnicity Major Asian Total',
          filterGroup: 'Ethnic group major',
        }),
        new CategoryFilter({
          value: '067de12b-014b-4bbd-baf1-08d78f6f2c4d',
          label: 'Ethnicity Major Black Total',
          filterGroup: 'Ethnic group major',
        }),
      ],
    ]);
    expect(headers.rowGroups).toEqual([
      [
        new CategoryFilter({
          value: 'd7e7e412-f462-444f-84ac-3454fa471cb8',
          label: 'State-funded primary',
          filterGroup: 'Default',
        }),
        new CategoryFilter({
          value: 'a9fe9fa6-e91f-460b-a0b1-66877b97c581',
          label: 'State-funded secondary',
          filterGroup: 'Default',
        }),
      ],
      [
        new LocationFilter(
          { value: 'E09000003', label: 'Barnet' },
          'localAuthority',
        ),
        new LocationFilter(
          { value: 'E08000016', label: 'Barnsley' },
          'localAuthority',
        ),
      ],
    ]);
    expect(headers.columns).toEqual([
      new TimePeriodFilter({
        year: 2014,
        code: 'AY',
        label: '2014/15',
      }),
      new TimePeriodFilter({
        year: 2015,
        code: 'AY',
        label: '2015/16',
      }),
    ]);
    expect(headers.rows).toEqual([
      new Indicator({
        value: '9cf0dcf1-367e-4207-2b50-08d78f6f2b08',
        label: 'Number of overall absence sessions',
        unit: '',
      }),
      new Indicator({
        value: 'd1c4a0be-8756-470d-2b51-08d78f6f2b08',
        label: 'Number of authorised absence sessions',
        unit: '',
      }),
      new Indicator({
        value: '63043c27-f055-472f-2b56-08d78f6f2b08',
        label: 'Number of persistent absentees',
        unit: '',
      }),
    ]);
  });

  test('does not map any headers', () => {
    const headers = mapTableHeadersConfig(
      {
        columnGroups: [],
        columns: [],
        rowGroups: [],
        rows: [],
      },
      testTable.subjectMeta,
    );

    expect(headers.columnGroups).toEqual([]);
    expect(headers.rowGroups).toEqual([]);
    expect(headers.columns).toEqual([]);
    expect(headers.rows).toEqual([]);
  });
});
