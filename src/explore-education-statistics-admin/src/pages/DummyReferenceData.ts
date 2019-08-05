import { IdTitlePair } from '@admin/services/common/types';

export enum DateType {
  DayMonthYear,
  Year,
}

export interface TimePeriodCoverageGroup {
  title: string;
  startDateLabel: string;
  startDateType: DateType;
  options: IdTitlePair[];
}

const createTimePeriodGroupWithQuarterlyPermutations = (
  timePeriodGroup: string,
  code: string,
  startDateLabel: string,
  startDateType: DateType,
) => {
  return {
    title: timePeriodGroup,
    startDateLabel,
    startDateType,
    options: [
      {
        id: code,
        title: timePeriodGroup,
      },
      {
        id: `${code}Q1`,
        title: `${timePeriodGroup} Q1`,
      },
      {
        id: `${code}Q1Q2`,
        title: `${timePeriodGroup} Q1 to Q2`,
      },
      {
        id: `${code}Q1Q3`,
        title: `${timePeriodGroup} Q1 to Q3`,
      },
      {
        id: `${code}Q1Q4`,
        title: `${timePeriodGroup} Q1 to Q4`,
      },
      {
        id: `${code}Q2`,
        title: `${timePeriodGroup} Q2`,
      },
      {
        id: `${code}Q2Q3`,
        title: `${timePeriodGroup} Q2 to Q3`,
      },
      {
        id: `${code}Q2Q4`,
        title: `${timePeriodGroup} Q2 to Q4`,
      },
      {
        id: `${code}Q3`,
        title: `${timePeriodGroup} Q3`,
      },
      {
        id: `${code}Q3Q4`,
        title: `${timePeriodGroup} Q3 to Q4`,
      },
      {
        id: `${code}Q4`,
        title: `${timePeriodGroup} Q4`,
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
    title: 'Term',
    startDateLabel: 'Year',
    startDateType: DateType.Year,
    options: [
      {
        id: 'T1',
        title: 'Autumn term',
      },
      {
        id: 'T1T2',
        title: 'Autumn and spring term',
      },
      {
        id: 'T2',
        title: 'Spring term',
      },
      {
        id: 'T3',
        title: 'Summer term',
      },
    ],
  },
  {
    title: 'Month',
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
      title: month,
    })),
  },
  {
    title: 'Other',
    startDateLabel: 'Year',
    startDateType: DateType.Year,
    options: [
      {
        id: 'EOM',
        title: 'Up until 31st March',
      },
    ],
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

export default {
  timePeriodCoverageGroups,
  findTimePeriodCoverageOption,
  findTimePeriodCoverageGroup,
};
