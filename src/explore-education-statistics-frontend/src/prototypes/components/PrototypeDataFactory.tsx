import React from 'react';
import {
  DataBlock,
  DataBlockProps,
} from '../../modules/find-statistics/components/DataBlock';
import { Axis, Chart } from '../../services/publicationService';
import {
  CharacteristicsData,
  DataTableResult,
  PublicationMeta,
} from '../../services/tableBuilderService';
import SchoolType from '../../services/types/SchoolType';
import { data as OriginalData } from './PrototypeMapBoundaries';

function createDataValues(
  indicators: string[],
  labels: string[],
  schooltype: SchoolType,
  year: number,
  data: (string | number | undefined)[][],
) {
  return {
    data: {
      indicators,

      publicationId: '',
      releaseDate: '',
      releaseId: '',
      result: data.map((row: (string | number | undefined)[], rowIndex) => {
        return row.reduce(
          (
            result: DataTableResult,
            next: string | number | undefined,
            index,
          ) => {
            if (next) result.indicators[indicators[index]] = `${next}`;
            return result;
          },
          {
            characteristic: {
              description: null,
              label: '',
              name: 'Total',
              name2: null,
            },
            indicators: {},
            schoolType: schooltype,
            timePeriod: year + rowIndex * 101,
          },
        );
      }),
    } as CharacteristicsData,

    meta: {
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
        ],
      },

      publicationId: '',
    } as PublicationMeta,
  };
}

function createBasicChart(
  type: string,
  indicators: string[],
  xAxis: Axis,
  yAxis: Axis,
  stacked: boolean = false,
): Chart {
  return {
    indicators,
    stacked,
    type,
    xAxis,
    yAxis,
  };
}

function createFullChartWithData(
  type: string,
  xAxis: Axis,
  yAxis: Axis,
  indicators: string[],
  labels: string[],
  schooltype: SchoolType,
  year: number,
  data: (string | number | undefined)[][],
  stacked: boolean = false,
) {
  return {
    ...createBasicChart(type, indicators, xAxis, yAxis, stacked),

    ...createDataValues(
      indicators.filter(name => name !== xAxis.key && name !== yAxis.key),
      labels,
      schooltype,
      year,
      data,
    ),
  };
}

function createDataBlockWithChart(
  heading: string,
  indicators: string[],
  labels: string[],
  schooltype: SchoolType,
  year: number,
  data: (string | number | undefined)[][],
  charts?: Chart[],
): DataBlockProps {
  return {
    charts,

    heading,
    type: 'DataBlock',

    ...createDataValues(indicators, labels, schooltype, year, data),
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
  201819,
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
);

export const ks4SchoolAverageHeadlineScoresByPupilCharacteristics = createDataBlockWithChart(
  'Average headline scores by pupil characteristics',
  ['name', 'ebacc_entry', 'eng', 'attainment'],
  ['Name', 'Ebacc Entry', 'Eng & Maths (9-5)', 'Attainment 8'],
  SchoolType.Total,
  2001819,
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
      { title: '', key: 'name' },
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
      { title: '' },
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
