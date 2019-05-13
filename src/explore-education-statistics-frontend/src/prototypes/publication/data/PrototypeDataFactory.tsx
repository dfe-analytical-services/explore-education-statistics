/* eslint-disable @typescript-eslint/camelcase,@typescript-eslint/no-unused-vars */
import { DataBlockProps } from '@common/modules/find-statistics/components/DataBlock';
import { ChartProps } from 'explore-education-statistics-common/src/modules/find-statistics/components/charts/ChartFunctions';
import { GeographicLevel } from 'explore-education-statistics-common/src/services/dataBlockService';
import React from 'react';
import TimePeriod from 'explore-education-statistics-common/src/services/types/TimePeriod';

export const newApiTest: DataBlockProps = {
  type: 'datablock',
  id: 'test',
  dataBlockRequest: {
    subjectId: 1,
    regions: [],
    localAuthorityDistricts: [],
    localAuthorities: [],
    countries: [],
    geographicLevel: GeographicLevel.National,
    startYear: '2014',
    endYear: '2017',
    indicators: ['23', '26', '28'],
    filters: ['1', '2'],
  },
};

export const newApiHorizontalData: ChartProps = {
  data: {
    publicationId: 'test',
    releaseDate: new Date(),
    releaseId: 1,
    subjectId: 1,
    geographicLevel: GeographicLevel.National,
    result: [
      {
        filters: [1],
        location: {
          country: {
            country_code: 'E92000001',
            country_name: 'England',
          },
          region: {
            region_code: '',
            region_name: '',
          },
          localAuthority: {
            new_la_code: '',
            old_la_code: '',
            la_name: '',
          },
          localAuthorityDistrict: {
            sch_lad_code: '',
            sch_lad_name: '',
          },
        },
        measures: {
          '28': '5',
          '26': '10',
          '23': '3',
        },
        timeIdentifier: 'HT6',
        year: 2014,
      },
      {
        filters: [1],
        location: {
          country: {
            country_code: 'E92000001',
            country_name: 'England',
          },
          region: {
            region_code: '',
            region_name: '',
          },
          localAuthority: {
            new_la_code: '',
            old_la_code: '',
            la_name: '',
          },
          localAuthorityDistrict: {
            sch_lad_code: '',
            sch_lad_name: '',
          },
        },
        measures: {
          '28': '5',
          '26': '10',
          '23': '3',
        },
        timeIdentifier: 'HT6',
        year: 2015,
      },
    ],
  },

  meta: {
    filters: {
      '1': {
        label: 'All Schools',
        value: '1',
      },
    },

    indicators: {
      '23': {
        label: 'Unauthorised absence rate',
        unit: '%',
        value: '23',
      },
      '26': {
        label: 'Overall absence rate',
        unit: '%',
        value: '26',
      },
      '28': {
        label: 'Authorised absence rate',
        unit: '%',
        value: '28',
      },
    },

    timePeriods: {
      '2014': new TimePeriod(2014, 'HT6'),
      '2015': new TimePeriod(2015, 'HT6'),
    },
  },

  labels: {
    '28': 'Authorised absence rate',
    '26': 'Overall absence rate',
    '23': 'Unauthorised absence rate',
  },

  chartDataKeys: ['23', '26', '28'],
  xAxis: { title: 'test x axis' },
  yAxis: { title: 'test y axis' },
};
