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
  data: (string | undefined)[][],
) {
  return {
    attributes,
    type,

    xAxis,
    yAxis,

    data: {
      publicationId: '',
      releaseDate: '',
      releaseId: '',
      result: data.map((row: (string | undefined)[], rowIndex) => {
        return row.reduce(
          (result: DataTableResult, next: string | undefined, index) => {
            if (next) result.attributes[attributes[index]] = next;
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

const createOnlyType = (type: string) =>
  create(
    type,
    { title: '' },
    { title: '' },
    [],
    [],
    SchoolType.Total,
    201819,
    [],
  );

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

export const ks4SchoolRevisedAttainmentChart = createOnlyType(
  'stackedbarvertical',
);

export const ks4SchoolAverageHeadlineScoresByPupilCharacteristics = createOnlyType(
  'stackedbarhorizontal',
);
