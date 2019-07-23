import { IdLabelPair } from '@admin/services/common/types';

export enum DateType {
  DayMonthYear,
  Year,
}

export interface TimePeriodCoverageGroup {
  label: string;
  startDateLabel: string;
  startDateType: DateType;
  options: IdLabelPair[];
}

const createTimePeriodGroupWithQuarterlyPermutations = (
  timePeriodGroup: string,
  code: string,
  startDateLabel: string,
  startDateType: DateType,
) => {
  return {
    label: timePeriodGroup,
    startDateLabel,
    startDateType,
    options: [
      {
        id: code,
        label: timePeriodGroup,
      },
      {
        id: `${code}Q1`,
        label: `${timePeriodGroup} Q1`,
      },
      {
        id: `${code}Q1Q2`,
        label: `${timePeriodGroup} Q1 to Q2`,
      },
      {
        id: `${code}Q1Q3`,
        label: `${timePeriodGroup} Q1 to Q3`,
      },
      {
        id: `${code}Q1Q4`,
        label: `${timePeriodGroup} Q1 to Q4`,
      },
      {
        id: `${code}Q2`,
        label: `${timePeriodGroup} Q2`,
      },
      {
        id: `${code}Q2Q3`,
        label: `${timePeriodGroup} Q2 to Q3`,
      },
      {
        id: `${code}Q2Q4`,
        label: `${timePeriodGroup} Q2 to Q4`,
      },
      {
        id: `${code}Q3`,
        label: `${timePeriodGroup} Q3`,
      },
      {
        id: `${code}Q3Q4`,
        label: `${timePeriodGroup} Q3 to Q4`,
      },
      {
        id: `${code}Q4`,
        label: `${timePeriodGroup} Q4`,
      },
    ],
  };
};

const timePeriodCoverageGroups: TimePeriodCoverageGroup[] = [
  createTimePeriodGroupWithQuarterlyPermutations(
    'Academic year',
    'AY',
    'Year',
    DateType.Year,
  ),
  createTimePeriodGroupWithQuarterlyPermutations(
    'Calendar year',
    'CY',
    'Year',
    DateType.Year,
  ),
  createTimePeriodGroupWithQuarterlyPermutations(
    'Financial year',
    'FY',
    'Financial year start',
    DateType.DayMonthYear,
  ),
  createTimePeriodGroupWithQuarterlyPermutations(
    'Tax year',
    'TY',
    'Year',
    DateType.Year,
  ),
  {
    label: 'Term',
    startDateLabel: 'Year',
    startDateType: DateType.Year,
    options: [
      {
        id: 'T1',
        label: 'Autumn term',
      },
      {
        id: 'T1T2',
        label: 'Autumn and spring term',
      },
      {
        id: 'T2',
        label: 'Spring term',
      },
      {
        id: 'T3',
        label: 'Summer term',
      },
    ],
  },
  {
    label: 'Month',
    startDateLabel: 'Year',
    startDateType: DateType.Year,
    options: [
      'January',
      'February',
      'March',
      'April',
      'May',
      'June',
      'July',
      'August',
      'September',
      'October',
      'November',
      'December',
    ].map((month, index) => ({
      id: `M${index + 1}`,
      label: month,
    })),
  },
  {
    label: 'Other',
    startDateLabel: 'Year',
    startDateType: DateType.Year,
    options: [
      {
        id: 'EOM',
        label: 'Up until 31st March',
      },
    ],
  },
];

const releaseTypeOptions: IdLabelPair[] = [
  {
    id: 'national-stats',
    label: 'National statistics',
  },
  {
    id: 'adhoc-stats',
    label: 'Official / adhoc statistics',
  },
];

const findTimePeriodCoverageOption = (code: string) => {
  return (
    timePeriodCoverageGroups
      .flatMap(group => group.options)
      .find(option => option.id === code) ||
    timePeriodCoverageGroups[0].options[0]
  );
};

const findTimePeriodCoverageGroup = (code: string) => {
  return (
    timePeriodCoverageGroups.find(group =>
      group.options.map(option => option.id).some(id => id === code),
    ) || timePeriodCoverageGroups[0]
  );
};

const findReleaseType = (id: string) => {
  return (
    releaseTypeOptions.find(type => type.id === id) || releaseTypeOptions[0]
  );
};

export default {
  timePeriodCoverageGroups,
  releaseTypeOptions,
  findTimePeriodCoverageOption,
  findTimePeriodCoverageGroup,
  findReleaseType,
};
