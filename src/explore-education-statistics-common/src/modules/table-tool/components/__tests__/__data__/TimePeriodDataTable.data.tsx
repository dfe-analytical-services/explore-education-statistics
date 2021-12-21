import { UnmappedTableHeadersConfig } from '@common/services/permalinkService';
import { TableDataResponse } from '@common/services/tableBuilderService';

export const testData1 = {
  fullTable: {
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
          name: 'school_type',
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
          value: '0003d2ac-4425-4432-2afb-08d78f6f2b08',
          label: 'Number of authorised absence sessions',
          unit: '',
          name: 'sess_authorised',
          decimalPlaces: 0,
        },
        {
          value: '829460cd-ae9e-4266-2aff-08d78f6f2b08',
          label: 'Number of overall absence sessions',
          unit: '',
          name: 'sess_overall',
          decimalPlaces: 0,
        },
      ],
      locationsHierarchical: {
        localAuthority: [
          { value: 'E09000003', label: 'Barnet' },
          { value: 'E08000016', label: 'Barnsley' },
        ],
      },
      boundaryLevels: [],
      publicationName: 'Pupil absence in schools in England',
      subjectName: 'Absence by characteristic',
      timePeriodRange: [
        { label: '2013/14', code: 'AY', year: 2013 },
        { label: '2014/15', code: 'AY', year: 2014 },
      ],
    },
    results: [
      {
        filters: [
          '598ed9fd-b37e-4e08-baec-08d78f6f2c4d',
          'd7e7e412-f462-444f-84ac-3454fa471cb8',
        ],
        geographicLevel: 'localAuthority',
        location: {
          country: { code: 'E92000001', name: 'England' },
          localAuthority: { code: 'E09000003', name: 'Barnet' },
          region: { code: 'E13000002', name: 'Outer London' },
        },
        measures: {
          '0003d2ac-4425-4432-2afb-08d78f6f2b08': '33725',
          '829460cd-ae9e-4266-2aff-08d78f6f2b08': '41239',
        },
        timePeriod: '2014_AY',
      },
      {
        filters: [
          '067de12b-014b-4bbd-baf1-08d78f6f2c4d',
          'd7e7e412-f462-444f-84ac-3454fa471cb8',
        ],
        geographicLevel: 'localAuthority',
        location: {
          country: { code: 'E92000001', name: 'England' },
          localAuthority: { code: 'E09000003', name: 'Barnet' },
          region: { code: 'E13000002', name: 'Outer London' },
        },
        measures: {
          '0003d2ac-4425-4432-2afb-08d78f6f2b08': '31241',
          '829460cd-ae9e-4266-2aff-08d78f6f2b08': '41945',
        },
        timePeriod: '2013_AY',
      },
      {
        filters: [
          '067de12b-014b-4bbd-baf1-08d78f6f2c4d',
          'a9fe9fa6-e91f-460b-a0b1-66877b97c581',
        ],
        geographicLevel: 'localAuthority',
        location: {
          country: { code: 'E92000001', name: 'England' },
          localAuthority: { code: 'E08000016', name: 'Barnsley' },
          region: { code: 'E12000003', name: 'Yorkshire and the Humber' },
        },
        measures: {
          '0003d2ac-4425-4432-2afb-08d78f6f2b08': '442',
          '829460cd-ae9e-4266-2aff-08d78f6f2b08': '788',
        },
        timePeriod: '2013_AY',
      },
      {
        filters: [
          '598ed9fd-b37e-4e08-baec-08d78f6f2c4d',
          'd7e7e412-f462-444f-84ac-3454fa471cb8',
        ],
        geographicLevel: 'localAuthority',
        location: {
          country: { code: 'E92000001', name: 'England' },
          localAuthority: { code: 'E08000016', name: 'Barnsley' },
          region: { code: 'E12000003', name: 'Yorkshire and the Humber' },
        },
        measures: {
          '0003d2ac-4425-4432-2afb-08d78f6f2b08': '1582',
          '829460cd-ae9e-4266-2aff-08d78f6f2b08': '2122',
        },
        timePeriod: '2014_AY',
      },
      {
        filters: [
          '598ed9fd-b37e-4e08-baec-08d78f6f2c4d',
          'a9fe9fa6-e91f-460b-a0b1-66877b97c581',
        ],
        geographicLevel: 'localAuthority',
        location: {
          country: { code: 'E92000001', name: 'England' },
          localAuthority: { code: 'E08000016', name: 'Barnsley' },
          region: { code: 'E12000003', name: 'Yorkshire and the Humber' },
        },
        measures: {
          '0003d2ac-4425-4432-2afb-08d78f6f2b08': '481',
          '829460cd-ae9e-4266-2aff-08d78f6f2b08': '752',
        },
        timePeriod: '2013_AY',
      },
      {
        filters: [
          '067de12b-014b-4bbd-baf1-08d78f6f2c4d',
          'd7e7e412-f462-444f-84ac-3454fa471cb8',
        ],
        geographicLevel: 'localAuthority',
        location: {
          country: { code: 'E92000001', name: 'England' },
          localAuthority: { code: 'E08000016', name: 'Barnsley' },
          region: { code: 'E12000003', name: 'Yorkshire and the Humber' },
        },
        measures: {
          '0003d2ac-4425-4432-2afb-08d78f6f2b08': '904',
          '829460cd-ae9e-4266-2aff-08d78f6f2b08': '1215',
        },
        timePeriod: '2013_AY',
      },
      {
        filters: [
          '598ed9fd-b37e-4e08-baec-08d78f6f2c4d',
          'd7e7e412-f462-444f-84ac-3454fa471cb8',
        ],
        geographicLevel: 'localAuthority',
        location: {
          country: { code: 'E92000001', name: 'England' },
          localAuthority: { code: 'E09000003', name: 'Barnet' },
          region: { code: 'E13000002', name: 'Outer London' },
        },
        measures: {
          '0003d2ac-4425-4432-2afb-08d78f6f2b08': '32125',
          '829460cd-ae9e-4266-2aff-08d78f6f2b08': '39697',
        },
        timePeriod: '2013_AY',
      },
      {
        filters: [
          '598ed9fd-b37e-4e08-baec-08d78f6f2c4d',
          'a9fe9fa6-e91f-460b-a0b1-66877b97c581',
        ],
        geographicLevel: 'localAuthority',
        location: {
          country: { code: 'E92000001', name: 'England' },
          localAuthority: { code: 'E09000003', name: 'Barnet' },
          region: { code: 'E13000002', name: 'Outer London' },
        },
        measures: {
          '0003d2ac-4425-4432-2afb-08d78f6f2b08': '31244',
          '829460cd-ae9e-4266-2aff-08d78f6f2b08': '36083',
        },
        timePeriod: '2014_AY',
      },
      {
        filters: [
          '067de12b-014b-4bbd-baf1-08d78f6f2c4d',
          'a9fe9fa6-e91f-460b-a0b1-66877b97c581',
        ],
        geographicLevel: 'localAuthority',
        location: {
          country: { code: 'E92000001', name: 'England' },
          localAuthority: { code: 'E09000003', name: 'Barnet' },
          region: { code: 'E13000002', name: 'Outer London' },
        },
        measures: {
          '0003d2ac-4425-4432-2afb-08d78f6f2b08': '26594',
          '829460cd-ae9e-4266-2aff-08d78f6f2b08': '37084',
        },
        timePeriod: '2014_AY',
      },
      {
        filters: [
          '598ed9fd-b37e-4e08-baec-08d78f6f2c4d',
          'a9fe9fa6-e91f-460b-a0b1-66877b97c581',
        ],
        geographicLevel: 'localAuthority',
        location: {
          country: { code: 'E92000001', name: 'England' },
          localAuthority: { code: 'E09000003', name: 'Barnet' },
          region: { code: 'E13000002', name: 'Outer London' },
        },
        measures: {
          '0003d2ac-4425-4432-2afb-08d78f6f2b08': '30389',
          '829460cd-ae9e-4266-2aff-08d78f6f2b08': '34689',
        },
        timePeriod: '2013_AY',
      },
      {
        filters: [
          '067de12b-014b-4bbd-baf1-08d78f6f2c4d',
          'd7e7e412-f462-444f-84ac-3454fa471cb8',
        ],
        geographicLevel: 'localAuthority',
        location: {
          country: { code: 'E92000001', name: 'England' },
          localAuthority: { code: 'E08000016', name: 'Barnsley' },
          region: { code: 'E12000003', name: 'Yorkshire and the Humber' },
        },
        measures: {
          '0003d2ac-4425-4432-2afb-08d78f6f2b08': '939',
          '829460cd-ae9e-4266-2aff-08d78f6f2b08': '1268',
        },
        timePeriod: '2014_AY',
      },
      {
        filters: [
          '067de12b-014b-4bbd-baf1-08d78f6f2c4d',
          'd7e7e412-f462-444f-84ac-3454fa471cb8',
        ],
        geographicLevel: 'localAuthority',
        location: {
          country: { code: 'E92000001', name: 'England' },
          localAuthority: { code: 'E09000003', name: 'Barnet' },
          region: { code: 'E13000002', name: 'Outer London' },
        },
        measures: {
          '0003d2ac-4425-4432-2afb-08d78f6f2b08': '31322',
          '829460cd-ae9e-4266-2aff-08d78f6f2b08': '41228',
        },
        timePeriod: '2014_AY',
      },
      {
        filters: [
          '598ed9fd-b37e-4e08-baec-08d78f6f2c4d',
          'd7e7e412-f462-444f-84ac-3454fa471cb8',
        ],
        geographicLevel: 'localAuthority',
        location: {
          country: { code: 'E92000001', name: 'England' },
          localAuthority: { code: 'E08000016', name: 'Barnsley' },
          region: { code: 'E12000003', name: 'Yorkshire and the Humber' },
        },
        measures: {
          '0003d2ac-4425-4432-2afb-08d78f6f2b08': '1135',
          '829460cd-ae9e-4266-2aff-08d78f6f2b08': '1512',
        },
        timePeriod: '2013_AY',
      },
      {
        filters: [
          '067de12b-014b-4bbd-baf1-08d78f6f2c4d',
          'a9fe9fa6-e91f-460b-a0b1-66877b97c581',
        ],
        geographicLevel: 'localAuthority',
        location: {
          country: { code: 'E92000001', name: 'England' },
          localAuthority: { code: 'E09000003', name: 'Barnet' },
          region: { code: 'E13000002', name: 'Outer London' },
        },
        measures: {
          '0003d2ac-4425-4432-2afb-08d78f6f2b08': '25741',
          '829460cd-ae9e-4266-2aff-08d78f6f2b08': '33422',
        },
        timePeriod: '2013_AY',
      },
      {
        filters: [
          '067de12b-014b-4bbd-baf1-08d78f6f2c4d',
          'a9fe9fa6-e91f-460b-a0b1-66877b97c581',
        ],
        geographicLevel: 'localAuthority',
        location: {
          country: { code: 'E92000001', name: 'England' },
          localAuthority: { code: 'E08000016', name: 'Barnsley' },
          region: { code: 'E12000003', name: 'Yorkshire and the Humber' },
        },
        measures: {
          '0003d2ac-4425-4432-2afb-08d78f6f2b08': '745',
          '829460cd-ae9e-4266-2aff-08d78f6f2b08': '1105',
        },
        timePeriod: '2014_AY',
      },
      {
        filters: [
          '598ed9fd-b37e-4e08-baec-08d78f6f2c4d',
          'a9fe9fa6-e91f-460b-a0b1-66877b97c581',
        ],
        geographicLevel: 'localAuthority',
        location: {
          country: { code: 'E92000001', name: 'England' },
          localAuthority: { code: 'E08000016', name: 'Barnsley' },
          region: { code: 'E12000003', name: 'Yorkshire and the Humber' },
        },
        measures: {
          '0003d2ac-4425-4432-2afb-08d78f6f2b08': '274',
          '829460cd-ae9e-4266-2aff-08d78f6f2b08': '571',
        },
        timePeriod: '2014_AY',
      },
    ],
  } as TableDataResponse,
  tableHeadersConfig: {
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
        { value: 'E09000003', type: 'Location', level: 'localAuthority' },
        { value: 'E08000016', type: 'Location', level: 'localAuthority' },
      ],
    ],
    columns: [
      { value: '2013_AY', type: 'TimePeriod' },
      { value: '2014_AY', type: 'TimePeriod' },
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
  } as UnmappedTableHeadersConfig,
};

export const testData2 = {
  fullTable: {
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
          name: 'school_type',
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
          value: 'f9ae4976-7cd3-4718-834a-09349b6eb377',
          label: 'Authorised absence rate',
          unit: '%',
          name: 'sess_authorised_percent',
          decimalPlaces: 1,
        },
      ],
      locationsHierarchical: {
        localAuthority: [
          { value: 'E09000003', label: 'Barnet' },
          { value: 'E08000016', label: 'Barnsley' },
        ],
      },
      boundaryLevels: [],
      publicationName: 'Pupil absence in schools in England',
      subjectName: 'Absence by characteristic',
      timePeriodRange: [
        { label: '2013/14', code: 'AY', year: 2013 },
        { label: '2014/15', code: 'AY', year: 2014 },
      ],
    },
    results: [
      {
        filters: [
          '598ed9fd-b37e-4e08-baec-08d78f6f2c4d',
          'd7e7e412-f462-444f-84ac-3454fa471cb8',
        ],
        geographicLevel: 'localAuthority',
        location: {
          country: { code: 'E92000001', name: 'England' },
          localAuthority: { code: 'E09000003', name: 'Barnet' },
          region: { code: 'E13000002', name: 'Outer London' },
        },
        measures: { 'f9ae4976-7cd3-4718-834a-09349b6eb377': '3.4' },
        timePeriod: '2014_AY',
      },
      {
        filters: [
          '067de12b-014b-4bbd-baf1-08d78f6f2c4d',
          'd7e7e412-f462-444f-84ac-3454fa471cb8',
        ],
        geographicLevel: 'localAuthority',
        location: {
          country: { code: 'E92000001', name: 'England' },
          localAuthority: { code: 'E09000003', name: 'Barnet' },
          region: { code: 'E13000002', name: 'Outer London' },
        },
        measures: { 'f9ae4976-7cd3-4718-834a-09349b6eb377': '2.9' },
        timePeriod: '2013_AY',
      },
      {
        filters: [
          '067de12b-014b-4bbd-baf1-08d78f6f2c4d',
          'a9fe9fa6-e91f-460b-a0b1-66877b97c581',
        ],
        geographicLevel: 'localAuthority',
        location: {
          country: { code: 'E92000001', name: 'England' },
          localAuthority: { code: 'E08000016', name: 'Barnsley' },
          region: { code: 'E12000003', name: 'Yorkshire and the Humber' },
        },
        measures: { 'f9ae4976-7cd3-4718-834a-09349b6eb377': '1.8' },
        timePeriod: '2013_AY',
      },
      {
        filters: [
          '598ed9fd-b37e-4e08-baec-08d78f6f2c4d',
          'd7e7e412-f462-444f-84ac-3454fa471cb8',
        ],
        geographicLevel: 'localAuthority',
        location: {
          country: { code: 'E92000001', name: 'England' },
          localAuthority: { code: 'E08000016', name: 'Barnsley' },
          region: { code: 'E12000003', name: 'Yorkshire and the Humber' },
        },
        measures: { 'f9ae4976-7cd3-4718-834a-09349b6eb377': '4.3' },
        timePeriod: '2014_AY',
      },
      {
        filters: [
          '598ed9fd-b37e-4e08-baec-08d78f6f2c4d',
          'a9fe9fa6-e91f-460b-a0b1-66877b97c581',
        ],
        geographicLevel: 'localAuthority',
        location: {
          country: { code: 'E92000001', name: 'England' },
          localAuthority: { code: 'E08000016', name: 'Barnsley' },
          region: { code: 'E12000003', name: 'Yorkshire and the Humber' },
        },
        measures: { 'f9ae4976-7cd3-4718-834a-09349b6eb377': '3.3' },
        timePeriod: '2013_AY',
      },
      {
        filters: [
          '067de12b-014b-4bbd-baf1-08d78f6f2c4d',
          'd7e7e412-f462-444f-84ac-3454fa471cb8',
        ],
        geographicLevel: 'localAuthority',
        location: {
          country: { code: 'E92000001', name: 'England' },
          localAuthority: { code: 'E08000016', name: 'Barnsley' },
          region: { code: 'E12000003', name: 'Yorkshire and the Humber' },
        },
        measures: { 'f9ae4976-7cd3-4718-834a-09349b6eb377': '2.5' },
        timePeriod: '2013_AY',
      },
      {
        filters: [
          '598ed9fd-b37e-4e08-baec-08d78f6f2c4d',
          'd7e7e412-f462-444f-84ac-3454fa471cb8',
        ],
        geographicLevel: 'localAuthority',
        location: {
          country: { code: 'E92000001', name: 'England' },
          localAuthority: { code: 'E09000003', name: 'Barnet' },
          region: { code: 'E13000002', name: 'Outer London' },
        },
        measures: { 'f9ae4976-7cd3-4718-834a-09349b6eb377': '3.4' },
        timePeriod: '2013_AY',
      },
      {
        filters: [
          '598ed9fd-b37e-4e08-baec-08d78f6f2c4d',
          'a9fe9fa6-e91f-460b-a0b1-66877b97c581',
        ],
        geographicLevel: 'localAuthority',
        location: {
          country: { code: 'E92000001', name: 'England' },
          localAuthority: { code: 'E09000003', name: 'Barnet' },
          region: { code: 'E13000002', name: 'Outer London' },
        },
        measures: { 'f9ae4976-7cd3-4718-834a-09349b6eb377': '3.3' },
        timePeriod: '2014_AY',
      },
      {
        filters: [
          '067de12b-014b-4bbd-baf1-08d78f6f2c4d',
          'a9fe9fa6-e91f-460b-a0b1-66877b97c581',
        ],
        geographicLevel: 'localAuthority',
        location: {
          country: { code: 'E92000001', name: 'England' },
          localAuthority: { code: 'E09000003', name: 'Barnet' },
          region: { code: 'E13000002', name: 'Outer London' },
        },
        measures: { 'f9ae4976-7cd3-4718-834a-09349b6eb377': '3' },
        timePeriod: '2014_AY',
      },
      {
        filters: [
          '598ed9fd-b37e-4e08-baec-08d78f6f2c4d',
          'a9fe9fa6-e91f-460b-a0b1-66877b97c581',
        ],
        geographicLevel: 'localAuthority',
        location: {
          country: { code: 'E92000001', name: 'England' },
          localAuthority: { code: 'E09000003', name: 'Barnet' },
          region: { code: 'E13000002', name: 'Outer London' },
        },
        measures: { 'f9ae4976-7cd3-4718-834a-09349b6eb377': '3.3' },
        timePeriod: '2013_AY',
      },
      {
        filters: [
          '067de12b-014b-4bbd-baf1-08d78f6f2c4d',
          'd7e7e412-f462-444f-84ac-3454fa471cb8',
        ],
        geographicLevel: 'localAuthority',
        location: {
          country: { code: 'E92000001', name: 'England' },
          localAuthority: { code: 'E08000016', name: 'Barnsley' },
          region: { code: 'E12000003', name: 'Yorkshire and the Humber' },
        },
        measures: { 'f9ae4976-7cd3-4718-834a-09349b6eb377': '2.4' },
        timePeriod: '2014_AY',
      },
      {
        filters: [
          '067de12b-014b-4bbd-baf1-08d78f6f2c4d',
          'd7e7e412-f462-444f-84ac-3454fa471cb8',
        ],
        geographicLevel: 'localAuthority',
        location: {
          country: { code: 'E92000001', name: 'England' },
          localAuthority: { code: 'E09000003', name: 'Barnet' },
          region: { code: 'E13000002', name: 'Outer London' },
        },
        measures: { 'f9ae4976-7cd3-4718-834a-09349b6eb377': '2.8' },
        timePeriod: '2014_AY',
      },
      {
        filters: [
          '598ed9fd-b37e-4e08-baec-08d78f6f2c4d',
          'd7e7e412-f462-444f-84ac-3454fa471cb8',
        ],
        geographicLevel: 'localAuthority',
        location: {
          country: { code: 'E92000001', name: 'England' },
          localAuthority: { code: 'E08000016', name: 'Barnsley' },
          region: { code: 'E12000003', name: 'Yorkshire and the Humber' },
        },
        measures: { 'f9ae4976-7cd3-4718-834a-09349b6eb377': '3.7' },
        timePeriod: '2013_AY',
      },
      {
        filters: [
          '067de12b-014b-4bbd-baf1-08d78f6f2c4d',
          'a9fe9fa6-e91f-460b-a0b1-66877b97c581',
        ],
        geographicLevel: 'localAuthority',
        location: {
          country: { code: 'E92000001', name: 'England' },
          localAuthority: { code: 'E09000003', name: 'Barnet' },
          region: { code: 'E13000002', name: 'Outer London' },
        },
        measures: { 'f9ae4976-7cd3-4718-834a-09349b6eb377': '2.9' },
        timePeriod: '2013_AY',
      },
      {
        filters: [
          '067de12b-014b-4bbd-baf1-08d78f6f2c4d',
          'a9fe9fa6-e91f-460b-a0b1-66877b97c581',
        ],
        geographicLevel: 'localAuthority',
        location: {
          country: { code: 'E92000001', name: 'England' },
          localAuthority: { code: 'E08000016', name: 'Barnsley' },
          region: { code: 'E12000003', name: 'Yorkshire and the Humber' },
        },
        measures: { 'f9ae4976-7cd3-4718-834a-09349b6eb377': '2.6' },
        timePeriod: '2014_AY',
      },
      {
        filters: [
          '598ed9fd-b37e-4e08-baec-08d78f6f2c4d',
          'a9fe9fa6-e91f-460b-a0b1-66877b97c581',
        ],
        geographicLevel: 'localAuthority',
        location: {
          country: { code: 'E92000001', name: 'England' },
          localAuthority: { code: 'E08000016', name: 'Barnsley' },
          region: { code: 'E12000003', name: 'Yorkshire and the Humber' },
        },
        measures: { 'f9ae4976-7cd3-4718-834a-09349b6eb377': '2.1' },
        timePeriod: '2014_AY',
      },
    ],
  } as TableDataResponse,
  tableHeadersConfig: {
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
        { value: 'E09000003', type: 'Location', level: 'localAuthority' },
        { value: 'E08000016', type: 'Location', level: 'localAuthority' },
      ],
    ],
    columns: [
      { value: '2013_AY', type: 'TimePeriod' },
      { value: '2014_AY', type: 'TimePeriod' },
    ],
    rows: [
      {
        value: 'f9ae4976-7cd3-4718-834a-09349b6eb377',
        type: 'Indicator',
      },
    ],
  } as UnmappedTableHeadersConfig,
};

export const testData3 = {
  fullTable: {
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
          name: 'school_type',
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
          value: 'f9ae4976-7cd3-4718-834a-09349b6eb377',
          label: 'Authorised absence rate',
          unit: '%',
          name: 'sess_authorised_percent',
          decimalPlaces: 1,
        },
      ],
      locationsHierarchical: {
        localAuthority: [
          { value: 'E09000003', label: 'Barnet' },
          { value: 'E08000016', label: 'Barnsley' },
        ],
      },
      boundaryLevels: [],
      publicationName: 'Pupil absence in schools in England',
      subjectName: 'Absence by characteristic',
      timePeriodRange: [{ label: '2014/15', code: 'AY', year: 2014 }],
    },
    results: [
      {
        filters: [
          '598ed9fd-b37e-4e08-baec-08d78f6f2c4d',
          'd7e7e412-f462-444f-84ac-3454fa471cb8',
        ],
        geographicLevel: 'localAuthority',
        location: {
          country: { code: 'E92000001', name: 'England' },
          localAuthority: { code: 'E09000003', name: 'Barnet' },
          region: { code: 'E13000002', name: 'Outer London' },
        },
        measures: { 'f9ae4976-7cd3-4718-834a-09349b6eb377': '3.4' },
        timePeriod: '2014_AY',
      },
      {
        filters: [
          '598ed9fd-b37e-4e08-baec-08d78f6f2c4d',
          'd7e7e412-f462-444f-84ac-3454fa471cb8',
        ],
        geographicLevel: 'localAuthority',
        location: {
          country: { code: 'E92000001', name: 'England' },
          localAuthority: { code: 'E08000016', name: 'Barnsley' },
          region: { code: 'E12000003', name: 'Yorkshire and the Humber' },
        },
        measures: { 'f9ae4976-7cd3-4718-834a-09349b6eb377': '4.3' },
        timePeriod: '2014_AY',
      },
      {
        filters: [
          '598ed9fd-b37e-4e08-baec-08d78f6f2c4d',
          'a9fe9fa6-e91f-460b-a0b1-66877b97c581',
        ],
        geographicLevel: 'localAuthority',
        location: {
          country: { code: 'E92000001', name: 'England' },
          localAuthority: { code: 'E09000003', name: 'Barnet' },
          region: { code: 'E13000002', name: 'Outer London' },
        },
        measures: { 'f9ae4976-7cd3-4718-834a-09349b6eb377': '3.3' },
        timePeriod: '2014_AY',
      },
      {
        filters: [
          '067de12b-014b-4bbd-baf1-08d78f6f2c4d',
          'a9fe9fa6-e91f-460b-a0b1-66877b97c581',
        ],
        geographicLevel: 'localAuthority',
        location: {
          country: { code: 'E92000001', name: 'England' },
          localAuthority: { code: 'E09000003', name: 'Barnet' },
          region: { code: 'E13000002', name: 'Outer London' },
        },
        measures: { 'f9ae4976-7cd3-4718-834a-09349b6eb377': '3' },
        timePeriod: '2014_AY',
      },
      {
        filters: [
          '067de12b-014b-4bbd-baf1-08d78f6f2c4d',
          'd7e7e412-f462-444f-84ac-3454fa471cb8',
        ],
        geographicLevel: 'localAuthority',
        location: {
          country: { code: 'E92000001', name: 'England' },
          localAuthority: { code: 'E08000016', name: 'Barnsley' },
          region: { code: 'E12000003', name: 'Yorkshire and the Humber' },
        },
        measures: { 'f9ae4976-7cd3-4718-834a-09349b6eb377': '2.4' },
        timePeriod: '2014_AY',
      },
      {
        filters: [
          '067de12b-014b-4bbd-baf1-08d78f6f2c4d',
          'd7e7e412-f462-444f-84ac-3454fa471cb8',
        ],
        geographicLevel: 'localAuthority',
        location: {
          country: { code: 'E92000001', name: 'England' },
          localAuthority: { code: 'E09000003', name: 'Barnet' },
          region: { code: 'E13000002', name: 'Outer London' },
        },
        measures: { 'f9ae4976-7cd3-4718-834a-09349b6eb377': '2.8' },
        timePeriod: '2014_AY',
      },
      {
        filters: [
          '067de12b-014b-4bbd-baf1-08d78f6f2c4d',
          'a9fe9fa6-e91f-460b-a0b1-66877b97c581',
        ],
        geographicLevel: 'localAuthority',
        location: {
          country: { code: 'E92000001', name: 'England' },
          localAuthority: { code: 'E08000016', name: 'Barnsley' },
          region: { code: 'E12000003', name: 'Yorkshire and the Humber' },
        },
        measures: { 'f9ae4976-7cd3-4718-834a-09349b6eb377': '2.6' },
        timePeriod: '2014_AY',
      },
      {
        filters: [
          '598ed9fd-b37e-4e08-baec-08d78f6f2c4d',
          'a9fe9fa6-e91f-460b-a0b1-66877b97c581',
        ],
        geographicLevel: 'localAuthority',
        location: {
          country: { code: 'E92000001', name: 'England' },
          localAuthority: { code: 'E08000016', name: 'Barnsley' },
          region: { code: 'E12000003', name: 'Yorkshire and the Humber' },
        },
        measures: { 'f9ae4976-7cd3-4718-834a-09349b6eb377': '2.1' },
        timePeriod: '2014_AY',
      },
    ],
  } as TableDataResponse,
  tableHeadersConfig: {
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
        { value: 'E09000003', type: 'Location', level: 'localAuthority' },
        { value: 'E08000016', type: 'Location', level: 'localAuthority' },
      ],
    ],
    columns: [{ value: '2014_AY', type: 'TimePeriod' }],
    rows: [
      {
        value: 'f9ae4976-7cd3-4718-834a-09349b6eb377',
        type: 'Indicator',
      },
    ],
  } as UnmappedTableHeadersConfig,
};

export const testDataNoFilters = {
  fullTable: {
    subjectMeta: {
      geoJsonAvailable: false,
      filters: {},
      footnotes: [],
      indicators: [
        {
          value: '9cf0dcf1-367e-4207-2b50-08d78f6f2b08',
          label: 'Number of overall absence sessions',
          unit: '',
          name: 'sess_overall',
          decimalPlaces: 0,
        },
        {
          value: 'd1c4a0be-8756-470d-2b51-08d78f6f2b08',
          label: 'Number of authorised absence sessions',
          unit: '',
          name: 'sess_overall',
          decimalPlaces: 0,
        },
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
      timePeriodRange: [
        { label: '2014/15', code: 'AY', year: 2014 },
        { label: '2015/16', code: 'AY', year: 2015 },
        { label: '2016/17', code: 'AY', year: 2016 },
      ],
    },
    results: [
      {
        filters: [],
        geographicLevel: 'country',
        location: { country: { code: 'E92000001', name: 'England' } },
        measures: {
          '9cf0dcf1-367e-4207-2b50-08d78f6f2b08': '2453340',
          'd1c4a0be-8756-470d-2b51-08d78f6f2b08': '1397521',
          '6160c4f8-4c9f-40f0-a623-2a4f742860af': '18.6',
        },
        timePeriod: '2015_AY',
      },
      {
        filters: [],
        geographicLevel: 'country',
        location: { country: { code: 'E92000001', name: 'England' } },
        measures: {
          '9cf0dcf1-367e-4207-2b50-08d78f6f2b08': '2212399',
          'd1c4a0be-8756-470d-2b51-08d78f6f2b08': '1280964',
          '6160c4f8-4c9f-40f0-a623-2a4f742860af': '18.3',
        },
        timePeriod: '2014_AY',
      },
      {
        filters: [],
        geographicLevel: 'country',
        location: { country: { code: 'E92000001', name: 'England' } },
        measures: {
          '9cf0dcf1-367e-4207-2b50-08d78f6f2b08': '2637752',
          'd1c4a0be-8756-470d-2b51-08d78f6f2b08': '1488865',
          '6160c4f8-4c9f-40f0-a623-2a4f742860af': '19.2',
        },
        timePeriod: '2016_AY',
      },
    ],
  } as TableDataResponse,
  tableHeadersConfig: {
    columns: [
      { value: '2014_AY', type: 'TimePeriod' },
      { value: '2015_AY', type: 'TimePeriod' },
      { value: '2016_AY', type: 'TimePeriod' },
    ],
    columnGroups: [],
    rows: [
      {
        value: '6160c4f8-4c9f-40f0-a623-2a4f742860af',
        type: 'Indicator',
      },
      {
        value: 'd1c4a0be-8756-470d-2b51-08d78f6f2b08',
        type: 'Indicator',
      },
      {
        value: '9cf0dcf1-367e-4207-2b50-08d78f6f2b08',
        type: 'Indicator',
      },
    ],
    rowGroups: [[{ value: 'E92000001', type: 'Location', level: 'country' }]],
  } as UnmappedTableHeadersConfig,
};

export const testDataFiltersWithNoResults = {
  fullTable: {
    subjectMeta: {
      geoJsonAvailable: false,
      filters: {
        Characteristic: {
          totalValue: '',
          hint: 'Filter by pupil characteristic',
          legend: 'Characteristic',
          name: 'characteristic',
          options: {
            FirstLanguage: {
              label: 'First language',
              options: [
                {
                  label:
                    'First language Known or believed to be other than English',
                  value: '5675d1fa-77fd-4dfd-bb1f-08d78f6f2c4d',
                },
                {
                  label: 'First language Unclassified',
                  value: '53da1e17-184f-43f6-bb27-08d78f6f2c4d',
                },
              ],
            },
          },
        },
        SchoolType: {
          totalValue: '',
          hint: 'Filter by school type',
          legend: 'School type',
          name: 'school_type',
          options: {
            Default: {
              label: 'Default',
              options: [
                {
                  label: 'Special',
                  value: 'b3207d77-143b-43d5-8b48-32d29727e96f',
                },
              ],
            },
          },
        },
      },
      footnotes: [],
      indicators: [
        {
          value: '0003d2ac-4425-4432-2afb-08d78f6f2b08',
          label: 'Number of authorised absence sessions',
          unit: '',
          name: 'sess_authorised',
          decimalPlaces: 0,
        },
        {
          value: '829460cd-ae9e-4266-2aff-08d78f6f2b08',
          label: 'Number of overall absence sessions',
          unit: '',
          name: 'sess_overall',
          decimalPlaces: 0,
        },
      ],
      locationsHierarchical: {
        localAuthority: [
          { value: 'E08000026', label: 'Coventry' },
          { value: 'E09000008', label: 'Croydon' },
        ],
      },
      boundaryLevels: [],
      publicationName: 'Pupil absence in schools in England',
      subjectName: 'Absence by characteristic',
      timePeriodRange: [
        { label: '2013/14', year: 2013, code: 'AY' },
        { label: '2014/15', year: 2014, code: 'AY' },
        { label: '2015/16', year: 2015, code: 'AY' },
      ],
    },
    results: [
      {
        filters: [
          '5675d1fa-77fd-4dfd-bb1f-08d78f6f2c4d',
          'b3207d77-143b-43d5-8b48-32d29727e96f',
        ],
        geographicLevel: 'localAuthority',
        location: {
          country: { code: 'E92000001', name: 'England' },
          localAuthority: { code: 'E08000026', name: 'Coventry' },
          region: { code: 'E12000005', name: 'West Midlands' },
        },
        measures: {
          '0003d2ac-4425-4432-2afb-08d78f6f2b08': '4185',
          '829460cd-ae9e-4266-2aff-08d78f6f2b08': '5142',
        },
        timePeriod: '2015_AY',
      },
      {
        filters: [
          '53da1e17-184f-43f6-bb27-08d78f6f2c4d',
          'b3207d77-143b-43d5-8b48-32d29727e96f',
        ],
        geographicLevel: 'localAuthority',
        location: {
          country: { code: 'E92000001', name: 'England' },
          localAuthority: { code: 'E09000008', name: 'Croydon' },
          region: { code: 'E13000002', name: 'Outer London' },
        },
        measures: {
          '0003d2ac-4425-4432-2afb-08d78f6f2b08': 'x',
          '829460cd-ae9e-4266-2aff-08d78f6f2b08': 'x',
        },
        timePeriod: '2013_AY',
      },
      {
        filters: [
          '5675d1fa-77fd-4dfd-bb1f-08d78f6f2c4d',
          'b3207d77-143b-43d5-8b48-32d29727e96f',
        ],
        geographicLevel: 'localAuthority',
        location: {
          country: { code: 'E92000001', name: 'England' },
          localAuthority: { code: 'E08000026', name: 'Coventry' },
          region: { code: 'E12000005', name: 'West Midlands' },
        },
        measures: {
          '0003d2ac-4425-4432-2afb-08d78f6f2b08': '6492',
          '829460cd-ae9e-4266-2aff-08d78f6f2b08': '7280',
        },
        timePeriod: '2013_AY',
      },
      {
        filters: [
          '5675d1fa-77fd-4dfd-bb1f-08d78f6f2c4d',
          'b3207d77-143b-43d5-8b48-32d29727e96f',
        ],
        geographicLevel: 'localAuthority',
        location: {
          country: { code: 'E92000001', name: 'England' },
          localAuthority: { code: 'E09000008', name: 'Croydon' },
          region: { code: 'E13000002', name: 'Outer London' },
        },
        measures: {
          '0003d2ac-4425-4432-2afb-08d78f6f2b08': '4809',
          '829460cd-ae9e-4266-2aff-08d78f6f2b08': '5076',
        },
        timePeriod: '2014_AY',
      },
      {
        filters: [
          '5675d1fa-77fd-4dfd-bb1f-08d78f6f2c4d',
          'b3207d77-143b-43d5-8b48-32d29727e96f',
        ],
        geographicLevel: 'localAuthority',
        location: {
          country: { code: 'E92000001', name: 'England' },
          localAuthority: { code: 'E09000008', name: 'Croydon' },
          region: { code: 'E13000002', name: 'Outer London' },
        },
        measures: {
          '0003d2ac-4425-4432-2afb-08d78f6f2b08': '4179',
          '829460cd-ae9e-4266-2aff-08d78f6f2b08': '4390',
        },
        timePeriod: '2013_AY',
      },
      {
        filters: [
          '5675d1fa-77fd-4dfd-bb1f-08d78f6f2c4d',
          'b3207d77-143b-43d5-8b48-32d29727e96f',
        ],
        geographicLevel: 'localAuthority',
        location: {
          country: { code: 'E92000001', name: 'England' },
          localAuthority: { code: 'E08000026', name: 'Coventry' },
          region: { code: 'E12000005', name: 'West Midlands' },
        },
        measures: {
          '0003d2ac-4425-4432-2afb-08d78f6f2b08': '5542',
          '829460cd-ae9e-4266-2aff-08d78f6f2b08': '6493',
        },
        timePeriod: '2014_AY',
      },
      {
        filters: [
          '5675d1fa-77fd-4dfd-bb1f-08d78f6f2c4d',
          'b3207d77-143b-43d5-8b48-32d29727e96f',
        ],
        geographicLevel: 'localAuthority',
        location: {
          country: { code: 'E92000001', name: 'England' },
          localAuthority: { code: 'E09000008', name: 'Croydon' },
          region: { code: 'E13000002', name: 'Outer London' },
        },
        measures: {
          '0003d2ac-4425-4432-2afb-08d78f6f2b08': '5322',
          '829460cd-ae9e-4266-2aff-08d78f6f2b08': '5483',
        },
        timePeriod: '2015_AY',
      },
    ],
  } as TableDataResponse,
};
