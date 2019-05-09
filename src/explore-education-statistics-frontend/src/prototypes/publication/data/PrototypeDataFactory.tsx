/* eslint-disable @typescript-eslint/camelcase,@typescript-eslint/no-unused-vars */
import { DataBlockProps } from '@common/modules/find-statistics/components/DataBlock';
import { data as OriginalData } from '@common/prototypes/publication/components/PrototypeMapBoundaries';
import {
  Axis,
  Chart,
  ReferenceLine,
} from '@common/services/publicationService';
import { CharacteristicsData } from '@common/services/tableBuilderService';
import SchoolType from '@common/services/types/SchoolType';
import { ChartProps } from 'explore-education-statistics-common/src/modules/find-statistics/components/charts/ChartFunctions';
import { GeographicLevel } from 'explore-education-statistics-common/src/services/dataBlockService';
import React from 'react';

function createDataValues(
  indicators: string[],
  labels: string[],
  schoolType: SchoolType,
  year: number | undefined,
  data: (string | number | undefined)[][],
  characteristic: string[] = ['Total'],
) {
  const characteristicsData: CharacteristicsData = {
    publicationId: '',
    releaseDate: '',
    releaseId: '',
    result: data.map((row: (string | number | undefined)[], rowIndex) => {
      const initialData = {
        schoolType,
        characteristic: {
          description: null,
          label: '',
          name: characteristic[rowIndex % characteristic.length],
          name2: null,
        },
        indicators: {},

        timePeriod: year !== undefined ? year + rowIndex * 101 : 0,
      };

      return row.reduce((result, next, index) => {
        if (next) {
          return {
            ...result,
            indicators: {
              ...result.indicators,
              [indicators[index]]: `${next}`,
            },
          };
        }
        return result;
      }, initialData);
    }),
  };

  const meta: any = {
    indicators: {
      Test: indicators.map((key: string, index: number) => ({
        label: labels[index],
        name: key,
        unit: '',
      })),
    },

    characteristics: {
      Test: [
        {
          label: 'Total',
          name: 'Total',
        },
        {
          label: 'Year',
          name: 'Year',
        },
        { label: 'London', name: 'London' },
        { label: 'Yorkshire', name: 'Yorkshire' },
        { label: 'East', name: 'East' },
        { label: 'South East', name: 'South East' },
        { label: 'South West', name: 'South West' },
        { label: 'West Midlands', name: 'West Midlands' },
        { label: 'East Midlands', name: 'East Midlands' },
        { label: 'North West', name: 'North West' },
        { label: 'North East', name: 'North East' },
      ],
    },

    publicationId: '',
  };

  return {
    data: characteristicsData,
    meta,
  };
}

function createBasicChart(
  type: string,
  indicators: string[],
  xAxis: Axis,
  yAxis: Axis,
  stacked: boolean = false,
  referenceLines: ReferenceLine[] = [],
): Chart {
  return {
    indicators,
    stacked,
    type,
    xAxis,
    yAxis,
    referenceLines,
  };
}

function createDataBlockWithChart(
  heading: string,
  indicators: string[],
  labels: string[],
  schooltype: SchoolType,
  year: number | undefined,
  data: (string | number | undefined)[][],
  charts?: Chart[],
  characteristic: string[] = ['Total'],
): DataBlockProps {
  return {
    showTables: false,

    heading,
    type: 'DataBlock',
  };
}

export const kS4SchoolPerformanceDataBlock = createDataBlockWithChart(
  'Average headline performance measures over time',
  ['attainment_english_maths', 'ebacc_entries', 'attainment_8'],
  ['Attainment in English and Maths', 'Ebacc Entries', 'Attainment 8'],
  SchoolType.Total,
  200910,
  [
    ['55', '22', '--'],
    ['55', '22', undefined],
    ['55', '22', undefined],
    ['55', '35', undefined],
    ['55', '35', undefined],
    ['55', '40', '48'],
    ['55', '40', '48'],
    ['42', '40', '48'],
    ['42', '40', '48'],
  ],
  [
    createBasicChart(
      'line',
      ['attainment_english_maths', 'ebacc_entries', 'attainment_8'],
      { title: '' },
      { title: '' },
    ),
  ],
);

export const ks4SchoolRevisedAttainmentChart = createDataBlockWithChart(
  'There is wide variation in the percentage of schools meeting the coasting and floor standard by region',
  ['la_name', 'floor_standards', 'coasting'],
  ['Region', 'Floor Standards', 'Coasting'],

  SchoolType.Total,
  201418,
  [
    ['London', 5, 3.5],
    ['Yorkshire', 7, 6],
    ['East', 8, 5],
    ['South East', 11, 9],
    ['South West', 11, 11],
    ['West Midlands', 12, 7.5],
    ['East Midlands', 14, 10],
    ['North West', 21, 12],
    ['North East', 23, 11],
  ],
  [
    createBasicChart(
      'verticalbar',
      ['floor_standards', 'coasting'],
      { title: '', key: 'la_name' },
      { title: '' },
    ),
  ],
  // ['London', 'Yorkshire', 'East', 'South East', 'South West', 'West Midlands', 'East Midlands', 'North West', 'North East' ]
);

export const ks4SchoolAverageHeadlineScoresByPupilCharacteristics = createDataBlockWithChart(
  'Average headline scores by pupil characteristics',
  ['name', 'ebacc_entry', 'eng', 'attainment'],
  ['Name', 'Ebacc Entry', 'Eng & Maths (9-5)', 'Attainment 8'],
  SchoolType.Total,
  201819,
  [
    ['SEN', 12, 13, 27],
    ['No SEN', 12, 13, 27],
    [''],
    ['Disadvantage', 12, 13, 27],
    ['All other pupils', 12, 13, 27],
    [''],
    ['Boys', 12, 13, 27],
    ['Girls', 12, 13, 27],
  ],
  [
    createBasicChart(
      'horizontalbar',
      ['ebacc_entry', 'eng', 'attainment'],
      { title: '' },
      { title: '', key: 'name', size: 60 },
    ),
  ],
);

export const ks4TrendInDisavdantagePuilsAttainmentGapIndex = createDataBlockWithChart(
  'Disadvantage attainment gap index',
  ['value'],
  ['Value'],
  SchoolType.Total,
  201112,
  [
    ['4.07'],
    ['3.89'],
    ['3.81'],
    ['3.74'],
    ['3.80'],
    ['3.78'],
    ['3.66'],
    ['3.68'],
  ],
  [
    createBasicChart(
      'line',
      ['value'],
      { title: '' },
      { title: '', min: 0, max: 5 },
    ),
  ],
);

export const ks4AverageHeadlineScoresByPupilEthnicity = createDataBlockWithChart(
  'Average headline scores by pupil ethnicity',
  ['name', 'attainment', 'eng', 'ebacc_entry'],
  ['Name', 'Attainment 8', 'Eng & Maths (9-5)', 'Ebacc Entry'],
  SchoolType.Total,
  201819,
  [
    ['White', 45, 42, 38],
    ['Mixed', 48, 43, 41],
    ['Asian', 50, 50, 49],
    ['Black', 45, 39, 45],
    ['Chinese', 63, 75, 63],
  ],
  [
    createBasicChart(
      'verticalbar',
      ['attainment', 'eng', 'ebacc_entry'],
      { title: '', key: 'name' },
      { title: '' },
    ),
  ],
);

export const ks4PerformanceInMatsComparedToNationalAverage = createDataBlockWithChart(
  'Performance in MATs compared to national average',
  ['name', 'below_Average', 'average', 'above_average'],
  ['Name', 'Below Average', 'Average', 'Above Average'],
  SchoolType.Total,
  201819,
  [
    ['Progress8', 40, 25, 35],
    ['Ebacc APS', 63, 0, 37],
    ['Ebacc Entries', 57, 0, 43],
  ],
  [
    createBasicChart(
      'horizontalbar',
      ['below_Average', 'average', 'above_average'],
      { title: '', key: 'name' },
      { title: '', size: 40 },
      true,
    ),
  ],
);

export const ks4AverageAttainment8ScorePerPupilByLocalAuthority = createDataBlockWithChart(
  'Average Attainment 8 score per pupil by local authority',
  [],
  [],
  SchoolType.Total,
  201819,
  [[]],
  [
    {
      ...createBasicChart('map', [], { title: '' }, { title: '' }),
      geometry: OriginalData,
    },
  ],
);

export const ks4RevisedAttainmentData = createDataBlockWithChart(
  'Pupil subject entries are highest for science and humanities and continue to increase',
  ['label', 'label_0', 'label_1'],
  ['', '2017', '2018'],
  SchoolType.Total,
  201819,
  [
    ['Science', '91.3', '95.5'],
    ['Humanities', '76.8', '78.3'],
    ['Languages', '47.4', '46.1'],
    ['Art', '46.5', '44.3'],
  ],
  undefined,
  ['Year'],
);

export const ks4StateFundedSchoolsPerformance2 = createDataBlockWithChart(
  'Across state-funded schools performance is typically higher in converter academies, the most common school type',
  ['group', 'schools', 'end_of_ks4', 'attainment8', 'progress8'],
  [
    '',
    'Schools',
    'Pupils at end of KS4',
    'Average Attainment8',
    'Average Progress8',
  ],
  SchoolType.Total,
  201819,
  [
    ['LA Maintained Schools', '930', '151242', '46.5', '-0.03'],
    ['Academies/Free Schools', '2223', '360345', '47.9', '0.03'],
    ['Sponsored Academies', '643', '151242', '46.5', '-0.03'],
    ['Converter Academies', '1431', '151242', '46.5', '-0.03'],
    ['Free Schools', '930', '77', '46.5', '-0.03'],
    [
      'Studio Schools',
      'University Technical Colleges',
      '28',
      '151242',
      '46.5',
      '-0.03',
    ],
    ['Further Education Colleges', '19', '151242', '46.5', '-0.03'],
    ['All', '3175', '151242', '46.5', '-0.03'],
  ],
);

export const ks4StateFundedSchoolsPerformance = createDataBlockWithChart(
  'Across state-funded schools performance is typically higher in converter academies, the most common school type',
  [
    'group',
    'LA Maintained Schools',
    'Academies/Free Schools',
    'Sponsored Academies',
    'Converter Academies',
    'Free Schools',
    'Studio Schools',
    'Further Education Colleges',
    'All',
  ],
  [
    '',
    'LA Maintained Schools',
    'Academies/Free Schools',
    'Sponsored Academies',
    'Converter Academies',
    'Free Schools',
    'Studio Schools',
    'Further Education Colleges',
    'All',
  ],
  SchoolType.Total,
  201819,
  [
    ['Schools', '930', '2223', '643', '1431', '930', '77', '19', '3175'],
    [
      'Pupils at end of KS4',
      '151242',
      '360345',
      '151242',
      '151242',
      '77',
      '28',
      '151242',
      '151242',
    ],
    [
      'Average Attainment8',
      '46.5',
      '47.9',
      '46.5',
      '46.5',
      '46.5',
      '151242',
      '46.5',
      '46.5',
    ],
    [
      'Average Progress8',
      '-0.03',
      '0.03',
      '-0.03',
      '-0.03',
      '-0.03',
      '46.5',
      '-0.03',
      '-0.03',
    ],
  ],
);

export const testChartsVertical = createDataBlockWithChart(
  'GDHI per head (£) England, 2011',
  ['la_name', 'cost'],
  ['Region', 'Cost'],

  SchoolType.Total,
  201112,
  [
    ['London', '21'],
    ['South East', '18'],
    ['East', '17'],
    ['South West', '15'],
    ['East Midlands', '13'],
    ['North West', '13'],
    ['West Midlands', '12'],
    ['Yorkshire', '11'],
    ['North East', '10'],
  ],
  [
    createBasicChart(
      'horizontalbar',
      ['cost'],
      { title: '' },
      { title: '', key: 'la_name' },
      false,
    ),
  ],
);

export const testChartsVerticalWithReferenceLineAndAxisTitles = createDataBlockWithChart(
  'GDHI per head (£) England, 2011',
  ['la_name', 'cost'],
  ['Region', 'Cost'],

  SchoolType.Total,
  201112,
  [
    ['London', '21'],
    ['South East', '18'],
    ['East', '17'],
    ['South West', '15'],
    ['East Midlands', '13'],
    ['North West', '13'],
    ['West Midlands', '12'],
    ['Yorkshire', '11'],
    ['North East', '10'],
  ],
  [
    createBasicChart(
      'horizontalbar',
      ['cost'],
      { title: 'Cost' },
      { title: 'Local Authority', key: 'la_name', size: 60 },
      false,
      [{ x: 17, label: 'England' }],
    ),
  ],
);

export const testChartsVerticalOffset = createDataBlockWithChart(
  'GDHI per head index comparison with average (£) England, 2011',
  ['la_name', 'cost'],
  ['Region', '£(thousands)'],

  SchoolType.Total,
  201112,
  [
    ['London', '5'],
    ['South East', '2'],
    ['East', '0.5'],
    ['South West', '-0.1'],
    ['East Midlands', '-1.8'],
    ['North West', '-1.9'],
    ['West Midlands', '-2'],
    ['Yorkshire', '-2.1'],
    ['North East', '-2.2'],
  ],
  [
    createBasicChart(
      'horizontalbar',
      ['cost'],
      { title: '' },
      { title: '', key: 'la_name' },
      false,
      // [{ x: 17, label: 'England' }],
    ),
  ],
);

export const testTimeSeries = createDataBlockWithChart(
  'Time Series',
  ['year', 'percent'],
  ['year', 'Percent'],

  SchoolType.Total,
  200102,
  [
    ['1991', 50],
    ['1992', 70],
    ['1993', 40],
    ['1994', 90],
    ['1995', 10],
    ['1996', 30],
    ['1997', 70],
    ['1998', 80],
    ['1999', 50],
    ['2000', 85],
    ['2001', 50],
    ['2002', 70],
    ['2003', 40],
    ['2004', 90],
    ['2005', 10],
    ['2006', 30],
    ['2007', 70],
    ['2008', 80],
    ['2009', 50],
    ['2010', 85],
  ],
  [
    createBasicChart(
      'verticalbar',
      ['percent'],
      { title: '', key: 'year' },
      { title: '' },
      false,
    ),
  ],
);

export const testTimeSeriesWithLine = {
  ...testTimeSeries,
  charts: [
    createBasicChart(
      'line',
      ['percent'],
      { title: '', key: 'year' },
      { title: '' },
      false,
    ),
  ],
};

export const testDistribution = createDataBlockWithChart(
  'Distribution',
  ['year', 'percent'],
  ['year', 'Percent'],

  SchoolType.Total,
  200102,
  [...Array(101)].map((item, idx) => [
    idx % 10 === 0 ? 1900 + idx : '',
    Math.random() * 100,
  ]),
  [
    createBasicChart(
      'verticalbar',
      ['percent'],
      { title: '', key: 'year' },
      { title: '' },
      false,
    ),
  ],
);

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
        value: 1,
      },
    },

    indicators: {
      '23': {
        label: 'Unauthorised absence rate',
        unit: '%',
        value: 23,
      },
      '26': {
        label: 'Overall absence rate',
        unit: '%',
        value: 26,
      },
      '28': {
        label: 'Authorised absence rate',
        unit: '%',
        value: 28,
      },
    },

    timePeriods: {
      '2014': {
        label: '2014',
        value: 2014,
      },
      '2015': {
        label: '2015',
        value: 2015,
      },
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
