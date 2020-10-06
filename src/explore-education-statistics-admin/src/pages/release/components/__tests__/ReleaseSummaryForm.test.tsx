import _metaService from '@admin/services/metaService';
import { render, screen, waitFor, within } from '@testing-library/react';
import noop from 'lodash/noop';
import React from 'react';
import ReleaseSummaryForm, {
  ReleaseSummaryFormValues,
} from '../ReleaseSummaryForm';

const metaService = _metaService as jest.Mocked<typeof _metaService>;

jest.mock('@admin/services/metaService');

describe('ReleaseSummaryForm', () => {
  const testReleaseTypes = [
    { id: 'a', title: 'Ad Hoc' },
    {
      id: 'b',
      title: 'National Statistics',
    },
    {
      id: 'c',
      title: 'Official Statistics',
    },
  ];

  const testTimeIdentifiers = [
    {
      category: { value: 'AcademicYear', label: 'Academic year' },
      timeIdentifiers: [
        { identifier: { value: 'AY', label: 'Academic Year' } },
        { identifier: { value: 'AYQ1', label: 'Academic Year Q1' } },
        { identifier: { value: 'AYQ2', label: 'Academic Year Q2' } },
        { identifier: { value: 'AYQ3', label: 'Academic Year Q3' } },
        { identifier: { value: 'AYQ4', label: 'Academic Year Q4' } },
      ],
    },
    // {
    //   category: { value: 'CalendarYear', label: 'Calendar year' },
    //   timeIdentifiers: [
    //     { identifier: { value: 'CY', label: 'Calendar Year' } },
    //     { identifier: { value: 'CYQ1', label: 'Calendar Year Q1' } },
    //     { identifier: { value: 'CYQ2', label: 'Calendar Year Q2' } },
    //     { identifier: { value: 'CYQ3', label: 'Calendar Year Q3' } },
    //     { identifier: { value: 'CYQ4', label: 'Calendar Year Q4' } },
    //   ],
    // },
    // {
    //   category: { value: 'FinancialYear', label: 'Financial Year' },
    //   timeIdentifiers: [
    //     { identifier: { value: 'FY', label: 'Financial Year' } },
    //     { identifier: { value: 'FYQ1', label: 'Financial Year Q1' } },
    //     { identifier: { value: 'FYQ2', label: 'Financial Year Q2' } },
    //     { identifier: { value: 'FYQ3', label: 'Financial Year Q3' } },
    //     { identifier: { value: 'FYQ4', label: 'Financial Year Q4' } },
    //   ],
    // },
  ];
  test('renders correctly', async () => {
    metaService.getReleaseTypes.mockResolvedValue(testReleaseTypes);
    metaService.getTimePeriodCoverageGroups.mockResolvedValue(
      testTimeIdentifiers,
    );

    render(
      <ReleaseSummaryForm<ReleaseSummaryFormValues>
        submitText="Create new release"
        initialValues={() => {
          return {
            timePeriodCoverageCode: '',
            timePeriodCoverageStartYear: '',
            releaseTypeId: '',
          };
        }}
        onSubmit={noop}
        onCancel={noop}
      />,
    );

    await waitFor(() => {
      expect(
        screen.getByText('Select time period coverage'),
      ).toBeInTheDocument();
    });

    const selectYearType = screen.getByLabelText('Type');
    expect(selectYearType).toBeInTheDocument();

    const inputYear = screen.getByLabelText(
      testTimeIdentifiers[0].category.label,
    );
    expect(inputYear).toBeInTheDocument();
  });
});
