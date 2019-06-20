/* eslint-disable @typescript-eslint/camelcase */
import { Dictionary } from '@common/types';
import AdminDashboardPublications from '@admin/components/AdminDashboardPublications';
import React from 'react';
import { Methodology, Theme, Topic } from '@common/services/publicationService';

export interface ChartType {
  type: string;
  title: string;

  axis?: string;
}

const chartTypes: ChartType[] = [
  {
    type: 'line',
    title: 'Line Chart',
    axis: 'X axis',
  },
  {
    type: 'horizontal',
    title: 'Horizontal bar',
    axis: 'Y axis',
  },
  {
    type: 'vertical',
    title: 'Vertical bar',
    axis: 'X axis',
  },
  {
    type: 'map',
    title: 'Map',
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
  },
  authorised_absence_rate: {
    value: 'authorised_absence_rate',
    label: 'Authorised absence rate',
  },
  unauthorised_absence_rate: {
    value: 'unauthorised_absence_rate',
    label: 'Unuthorised absence rate',
  },
  overall_absence_rate: {
    value: 'overall_absence_rate',
    label: 'Overall absence rate',
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

const methodologies: Methodology[] = [
  {
    id: 'methodology-1',
    title: 'A guide to absence statistics',
  },
];

const themes: Theme[] = [
  {
    id: 'theme-1',
    title: 'Pupils and schools',
  },
];

const topics: Topic[] = [
  {
    id: 'topic-1',
    title: 'pupil absence',
    theme: themes[0],
  },
];

const publications = [
  {
    id: 'publication-1',
    slug: 'pupil-absence-statistics-and-data-for-schools-in-england',
    title: 'Pupil absence statistics and data for schools in England',
    description: '',
    dataSource: '',
    summary: '',
    nextUpdate: '',
    releases: [],
    legacyReleases: [],
    topic: topics[0],
    contact: {
      teamName: '',
      teamEmail: '',
      contactName: '',
      contactTelNo: '',
    },
    methodology: methodologies[0],
  },
];

export default {
  chartTypes,
  tables,
  indicators,
  filters,
  publications,
};
