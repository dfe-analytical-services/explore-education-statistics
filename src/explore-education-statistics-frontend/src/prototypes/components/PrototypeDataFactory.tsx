import React from 'react';
import { Axis } from '../../services/publicationService';
import {
  CharacteristicsData,
  DataTableResult,
  PublicationMeta,
} from '../../services/tableBuilderService';
import SchoolType from '../../services/types/SchoolType';

function create(
  type: string,
  xAxis: Axis,
  yAxis: Axis,
  attributes: string[],
  labels: string[],
  schooltype: SchoolType,
  year: number,
  data: (string | number | undefined)[][],
) {
  return {
    type,

    xAxis,
    yAxis,

    attributes: attributes.filter(
      name => name !== xAxis.key && name !== yAxis.key,
    ),
    data: {
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
            if (next) result.attributes[attributes[index]] = `${next}`;
            return result;
          },
          {
            attributes: {},
            schoolType: schooltype,
            year: year + rowIndex * 101,
          },
        );
      }),
    } as CharacteristicsData,

    meta: {
      attributes: {
        Test: attributes.map((key: string, index: number) => ({
          label: labels[index],
          name: key,
        })),
      },

      characteristics: {},

      publicationId: '',
    } as PublicationMeta,
  };
}

export const kS4SchoolPerformanceChart = create(
  'line',
  { title: '' },
  { title: '' },
  ['attainment_english_maths', 'ebacc_entries', 'attainment_8'],
  ['Attainment in English and Maths', 'Ebacc Entries', 'Attainment 8'],
  SchoolType.Total,
  200910,
  [
    ['55', '22', undefined],
    ['55', '22', undefined],
    ['55', '22', undefined],
    ['55', '35', undefined],
    ['55', '35', undefined],
    ['55', '40', '48'],
    ['55', '40', '48'],
    ['42', '40', '48'],
    ['42', '40', '48'],
  ],
);

export const ks4SchoolRevisedAttainmentChart = create(
  'stackedbarvertical',
  { title: '', key: 'la_name' },
  { title: '' },
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
);

export const ks4SchoolAverageHeadlineScoresByPupilCharacteristics = create(
  'stackedbarhorizontal',
  { title: '' },
  { title: '', key: 'name' },
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
);

export const ks4TrendInDisavdantagePuilsAttainmentGapIndex = create(
  'line',
  { title: '' },
  { title: '', min: 0, max: 5 },
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
);

export const ks4AverageHeadlineScoresByPupilEthnicity = create(
  'stackedbarvertical',
  { title: '', key: 'name' },
  { title: '' },
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
);
