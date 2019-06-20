/* eslint-disable @typescript-eslint/camelcase */
import { Dictionary } from '@common/types';
import {
  DataBlockResponse,
  GeographicLevel,
  DataBlockLocation,
  Result,
  Country,
} from '@common/services/dataBlockService';

import { ChartType as PublicationChartType } from '@common/services/publicationService';

export interface ChartType {
  type: PublicationChartType;
  title: string;

  axis: string[];
  hasLegend: boolean;
}

const chartTypes: ChartType[] = [
  {
    type: 'line',
    title: 'Line Chart',
    axis: ['X Axis', 'Group fields'],
    hasLegend: true,
  },
  {
    type: 'horizontalbar',
    title: 'Horizontal bar',
    axis: ['Y Axis', 'Group fields'],
    hasLegend: true,
  },
  {
    type: 'verticalbar',
    title: 'Vertical bar',
    axis: ['X Axis', 'Group fields'],
    hasLegend: true,
  },
  {
    type: 'map',
    title: 'Map',
    axis: [],
    hasLegend: false,
  },
];

export interface PrototypeTable {
  label: string;
  value: string;

  title?: string;
}

const tables: Dictionary<PrototypeTable> = {
  absence_highlights_panel: {
    value: 'absence_highlights_panel',
    label: 'Table for absence highlights panel',

    title:
      "Table showing 'Absence by characteristic' from 'Pupil absence' in England between 2012/13 and 2016/17",
  },
  example_table_2: {
    value: 'example_table_2',
    label: 'Example Table 2',

    title:
      "Table showing 'Example 2' from 'Example table 2' in England between 2012/13 and 2016/17",
  },
  example_table_3: {
    value: 'example_table_3',
    label: 'Example Table 3',

    title:
      "Table showing 'Example 3' from 'Example table 3' in England between 2012/13 and 2016/17",
  },
  example_table_4: {
    value: 'example_table_4',
    label: 'Example Table 4',

    title:
      "Table showing 'Example 4' from 'Example table 4' in England between 2012/13 and 2016/17",
  },
};

const indicators = {
  number_of_pupil_enrolments: {
    value: 'number_of_pupil_enrolments',
    label: 'Number of pupil enrolments',
    unit: '',
  },
  authorised_absence_rate: {
    value: 'authorised_absence_rate',
    label: 'Authorised absence rate',
    unit: '%',
  },
  unauthorised_absence_rate: {
    value: 'unauthorised_absence_rate',
    label: 'Unuthorised absence rate',
    unit: '%',
  },
  overall_absence_rate: {
    value: 'overall_absence_rate',
    label: 'Overall absence rate',
    unit: '%',
  },
};

const countries: Dictionary<Country> = {
  e: { country_code: 'e', country_name: 'England' },
};

const emptylocation: DataBlockLocation = {
  country: { country_name: '', country_code: '' },
  region: { region_code: '', region_name: '' },
  localAuthority: { new_la_code: '', old_la_code: '', la_name: '' },
  localAuthorityDistrict: { sch_lad_code: '', sch_lad_name: '' },
};

const locations = {
  e: {
    label: 'England',
    value: 'e',
    geoJson: [],
  },
};

const filters = {
  academic_year: {
    value: 'academic_year',
    label: 'Academic Year',
  },
  school_type: {
    value: 'school_type',
    label: 'School Type',
  },
};

const createResult = (
  country: string,
  year: number,
  ...measureValues: number[]
): Result => {
  return {
    location: {
      ...emptylocation,
      country: countries[country],
    },
    year,
    timeIdentifier: 'HT6',
    filters: Object.keys(filters).map(key => +key),
    measures: Object.keys(indicators).reduce((measures, measureName, index) => {
      return {
        ...measures,
        [measureName]: `${measureValues[index]}`,
      };
    }, {}),
  };
};

const responseData: DataBlockResponse = {
  metaData: {
    indicators,
    locations,
    timePeriods: {
      '2012_HT6': { label: '2012/13', value: '2012' },
      '2013_HT6': { label: '2013/14', value: '2013' },
      '2014_HT6': { label: '2014/15', value: '2014' },
      '2015_HT6': { label: '2015/16', value: '2015' },
      '2016_HT6': { label: '2016/17', value: '2016' },
    },
    filters,
  },

  publicationId: 'prototype Id',
  geographicLevel: GeographicLevel.National,
  releaseId: 1,
  subjectId: 1,
  releaseDate: new Date('2017-01-01'),
  result: [
    createResult('e', 2012, 6477725, 4.2, 1.1, 5.3),
    createResult('e', 2013, 6553005, 3.5, 1.1, 4.5),
    createResult('e', 2014, 6642755, 3.5, 1.1, 4.6),
    createResult('e', 2015, 6737190, 3.4, 1.1, 4.6),
    createResult('e', 2016, 6899770, 3.4, 1.3, 4.7),
  ],
};

export default {
  chartTypes,
  tables,
  indicators,
  filters,
  responseData,
};
