import { IdLabelPair } from '@admin/services/publicationService';

interface TimePeriodCoverageGroup {
  label: string;
  options: IdLabelPair[];
}

const createTimePeriodGroupWithQuarterlyPermutations = (
  timePeriodGroup: string,
  code: string,
) => {
  return {
    label: timePeriodGroup,
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

const timePeriodCoverageOptions: TimePeriodCoverageGroup[] = [
  createTimePeriodGroupWithQuarterlyPermutations('Academic year', 'AY'),
  createTimePeriodGroupWithQuarterlyPermutations('Calendar year', 'CY'),
  createTimePeriodGroupWithQuarterlyPermutations('Financial year', 'FY'),
  createTimePeriodGroupWithQuarterlyPermutations('Tax year', 'TY'),
  {
    label: 'Term',
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
    ].map((month, index) => {
      return {
        id: `M${index + 1}`,
        label: month,
      };
    }),
  },
  {
    label: 'Other',
    options: [
      {
        id: 'EOM',
        label: 'Up until 31st March',
      },
    ],
  },
];

export default {
  timePeriodCoverageGroups: timePeriodCoverageOptions,
};
