// eslint-disable-next-line import/prefer-default-export
export const testData1 = {
  fullTable: {
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
      indicators: [
        {
          value: '0003d2ac-4425-4432-2afb-08d78f6f2b08',
          label: 'Number of authorised absence sessions',
          unit: '',
        },
        {
          value: '829460cd-ae9e-4266-2aff-08d78f6f2b08',
          label: 'Number of overall absence sessions',
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
        { value: '2013_AY', label: '2013/14', code: 'AY', year: 2013 },
        { value: '2014_AY', label: '2014/15', code: 'AY', year: 2014 },
      ],
    },
    results: [
      {
        filters: [
          '598ed9fd-b37e-4e08-baec-08d78f6f2c4d',
          'd7e7e412-f462-444f-84ac-3454fa471cb8',
        ],
        geographicLevel: 'LocalAuthority',
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
        geographicLevel: 'LocalAuthority',
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
        geographicLevel: 'LocalAuthority',
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
        geographicLevel: 'LocalAuthority',
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
        geographicLevel: 'LocalAuthority',
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
        geographicLevel: 'LocalAuthority',
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
        geographicLevel: 'LocalAuthority',
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
        geographicLevel: 'LocalAuthority',
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
        geographicLevel: 'LocalAuthority',
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
        geographicLevel: 'LocalAuthority',
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
        geographicLevel: 'LocalAuthority',
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
        geographicLevel: 'LocalAuthority',
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
        geographicLevel: 'LocalAuthority',
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
        geographicLevel: 'LocalAuthority',
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
        geographicLevel: 'LocalAuthority',
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
        geographicLevel: 'LocalAuthority',
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
  },
  tableHeadersConfig: {
    columnGroups: [
      [
        {
          value: '598ed9fd-b37e-4e08-baec-08d78f6f2c4d',
          label: 'Ethnicity Major Asian Total',
          filterGroup: 'Ethnic group major',
          isTotal: false,
        },
        {
          value: '067de12b-014b-4bbd-baf1-08d78f6f2c4d',
          label: 'Ethnicity Major Black Total',
          filterGroup: 'Ethnic group major',
          isTotal: false,
        },
      ],
    ],
    rowGroups: [
      [
        {
          value: 'd7e7e412-f462-444f-84ac-3454fa471cb8',
          label: 'State-funded primary',
          filterGroup: 'Default',
          isTotal: false,
        },
        {
          value: 'a9fe9fa6-e91f-460b-a0b1-66877b97c581',
          label: 'State-funded secondary',
          filterGroup: 'Default',
          isTotal: false,
        },
      ],
      [
        { value: 'E09000003', label: 'Barnet', level: 'localAuthority' },
        { value: 'E08000016', label: 'Barnsley', level: 'localAuthority' },
      ],
    ],
    columns: [
      { value: '2013_AY', label: '2013/14', code: 'AY', year: 2013 },
      { value: '2014_AY', label: '2014/15', code: 'AY', year: 2014 },
    ],
    rows: [
      {
        value: '0003d2ac-4425-4432-2afb-08d78f6f2b08',
        label: 'Number of authorised absence sessions',
        unit: '',
      },
      {
        value: '829460cd-ae9e-4266-2aff-08d78f6f2b08',
        label: 'Number of overall absence sessions',
        unit: '',
      },
    ],
  },
};

export const testData2 = {
  fullTable: {
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
      indicators: [
        {
          value: 'f9ae4976-7cd3-4718-834a-09349b6eb377',
          label: 'Authorised absence rate',
          unit: '%',
        },
      ],
      locations: [
        { value: 'E09000003', label: 'Barnet', level: 'localAuthority' },
        { value: 'E08000016', label: 'Barnsley', level: 'localAuthority' },
      ],
      publicationName: 'Pupil absence in schools in England',
      subjectName: 'Absence by characteristic',
      timePeriodRange: [
        { value: '2013_AY', label: '2013/14', code: 'AY', year: 2013 },
        { value: '2014_AY', label: '2014/15', code: 'AY', year: 2014 },
      ],
    },
    results: [
      {
        filters: [
          '598ed9fd-b37e-4e08-baec-08d78f6f2c4d',
          'd7e7e412-f462-444f-84ac-3454fa471cb8',
        ],
        geographicLevel: 'LocalAuthority',
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
        geographicLevel: 'LocalAuthority',
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
        geographicLevel: 'LocalAuthority',
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
        geographicLevel: 'LocalAuthority',
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
        geographicLevel: 'LocalAuthority',
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
        geographicLevel: 'LocalAuthority',
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
        geographicLevel: 'LocalAuthority',
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
        geographicLevel: 'LocalAuthority',
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
        geographicLevel: 'LocalAuthority',
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
        geographicLevel: 'LocalAuthority',
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
        geographicLevel: 'LocalAuthority',
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
        geographicLevel: 'LocalAuthority',
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
        geographicLevel: 'LocalAuthority',
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
        geographicLevel: 'LocalAuthority',
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
        geographicLevel: 'LocalAuthority',
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
        geographicLevel: 'LocalAuthority',
        location: {
          country: { code: 'E92000001', name: 'England' },
          localAuthority: { code: 'E08000016', name: 'Barnsley' },
          region: { code: 'E12000003', name: 'Yorkshire and the Humber' },
        },
        measures: { 'f9ae4976-7cd3-4718-834a-09349b6eb377': '2.1' },
        timePeriod: '2014_AY',
      },
    ],
  },
  tableHeadersConfig: {
    columnGroups: [
      [
        {
          value: '598ed9fd-b37e-4e08-baec-08d78f6f2c4d',
          label: 'Ethnicity Major Asian Total',
          filterGroup: 'Ethnic group major',
          isTotal: false,
        },
        {
          value: '067de12b-014b-4bbd-baf1-08d78f6f2c4d',
          label: 'Ethnicity Major Black Total',
          filterGroup: 'Ethnic group major',
          isTotal: false,
        },
      ],
    ],
    rowGroups: [
      [
        {
          value: 'd7e7e412-f462-444f-84ac-3454fa471cb8',
          label: 'State-funded primary',
          filterGroup: 'Default',
          isTotal: false,
        },
        {
          value: 'a9fe9fa6-e91f-460b-a0b1-66877b97c581',
          label: 'State-funded secondary',
          filterGroup: 'Default',
          isTotal: false,
        },
      ],
      [
        { value: 'E09000003', label: 'Barnet', level: 'localAuthority' },
        { value: 'E08000016', label: 'Barnsley', level: 'localAuthority' },
      ],
    ],
    columns: [
      { value: '2013_AY', label: '2013/14', code: 'AY', year: 2013 },
      { value: '2014_AY', label: '2014/15', code: 'AY', year: 2014 },
    ],
    rows: [
      {
        value: 'f9ae4976-7cd3-4718-834a-09349b6eb377',
        label: 'Authorised absence rate',
        unit: '%',
      },
    ],
  },
};

export const testData3 = {
  fullTable: {
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
      footnotes: [
        {
          id: 'e9076bab-6ff7-4c92-8972-2fb4affbe977',
          label:
            'State-funded secondary schools include city technology colleges and all secondary academies, including all-through academies and free schools.',
        },
        {
          id: '7fabae1a-5cc0-4a1a-861a-3ddd8ec0f7b2',
          label:
            'Absence rates are the number of absence sessions expressed as a percentage of the total number of possible sessions.',
        },
        {
          id: 'b678373f-dfa2-41ad-817e-3f011d1b5173',
          label:
            'There may be discrepancies between totals and the sum of constituent parts  as national and regional totals and totals across school types have been rounded to the nearest 5.',
        },
        {
          id: '96641ff5-d33b-495d-8691-8045c148a595',
          label:
            'Figures for pupils with unclassified or missing characteristics information should be interpreted with caution.',
        },
        {
          id: '6c06f733-c30b-45e1-980a-b587e923f73b',
          label:
            'x - 1 or 2 enrolments, or a percentage based on 1 or 2 enrolments.',
        },
        {
          id: 'fbb6262f-213a-453a-98ca-b832d6ae1c16',
          label:
            'State-funded primary schools include all primary academies, including free schools.',
        },
        {
          id: '41c1eb5a-8415-45eb-bc49-cfa42382ebba',
          label:
            'Totals may not appear to equal the sum of component parts because numbers have been rounded to the nearest 5.',
        },
        {
          id: 'e03c3b82-75df-4dee-b3ed-dac39378a9b5',
          label:
            'See "Guide to absence statistics" for more information on how absence and pupil characteristic data have been linked.',
        },
      ],
      indicators: [
        {
          value: 'f9ae4976-7cd3-4718-834a-09349b6eb377',
          label: 'Authorised absence rate',
          unit: '%',
        },
      ],
      locations: [
        { value: 'E09000003', label: 'Barnet', level: 'localAuthority' },
        { value: 'E08000016', label: 'Barnsley', level: 'localAuthority' },
      ],
      publicationName: 'Pupil absence in schools in England',
      subjectName: 'Absence by characteristic',
      timePeriodRange: [
        { value: '2014_AY', label: '2014/15', code: 'AY', year: 2014 },
      ],
    },
    results: [
      {
        filters: [
          '598ed9fd-b37e-4e08-baec-08d78f6f2c4d',
          'd7e7e412-f462-444f-84ac-3454fa471cb8',
        ],
        geographicLevel: 'LocalAuthority',
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
        geographicLevel: 'LocalAuthority',
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
        geographicLevel: 'LocalAuthority',
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
        geographicLevel: 'LocalAuthority',
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
        geographicLevel: 'LocalAuthority',
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
        geographicLevel: 'LocalAuthority',
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
        geographicLevel: 'LocalAuthority',
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
        geographicLevel: 'LocalAuthority',
        location: {
          country: { code: 'E92000001', name: 'England' },
          localAuthority: { code: 'E08000016', name: 'Barnsley' },
          region: { code: 'E12000003', name: 'Yorkshire and the Humber' },
        },
        measures: { 'f9ae4976-7cd3-4718-834a-09349b6eb377': '2.1' },
        timePeriod: '2014_AY',
      },
    ],
  },
  tableHeadersConfig: {
    columnGroups: [
      [
        {
          value: '598ed9fd-b37e-4e08-baec-08d78f6f2c4d',
          label: 'Ethnicity Major Asian Total',
          filterGroup: 'Ethnic group major',
          isTotal: false,
        },
        {
          value: '067de12b-014b-4bbd-baf1-08d78f6f2c4d',
          label: 'Ethnicity Major Black Total',
          filterGroup: 'Ethnic group major',
          isTotal: false,
        },
      ],
    ],
    rowGroups: [
      [
        {
          value: 'd7e7e412-f462-444f-84ac-3454fa471cb8',
          label: 'State-funded primary',
          filterGroup: 'Default',
          isTotal: false,
        },
        {
          value: 'a9fe9fa6-e91f-460b-a0b1-66877b97c581',
          label: 'State-funded secondary',
          filterGroup: 'Default',
          isTotal: false,
        },
      ],
      [
        { value: 'E09000003', label: 'Barnet', level: 'localAuthority' },
        { value: 'E08000016', label: 'Barnsley', level: 'localAuthority' },
      ],
    ],
    columns: [{ value: '2014_AY', label: '2014/15', code: 'AY', year: 2014 }],
    rows: [
      {
        value: 'f9ae4976-7cd3-4718-834a-09349b6eb377',
        label: 'Authorised absence rate',
        unit: '%',
      },
    ],
  },
};
