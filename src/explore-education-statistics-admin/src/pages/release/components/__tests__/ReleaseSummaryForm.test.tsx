import _metaService, {
  TimePeriodCoverageGroup,
} from '@admin/services/metaService';
import { render, screen, waitFor, within } from '@testing-library/react';
import noop from 'lodash/noop';
import React from 'react';
import { IdTitlePair } from 'src/services/types/common';
import userEvent from '@testing-library/user-event';
import ReleaseSummaryForm, {
  ReleaseSummaryFormValues,
} from '../ReleaseSummaryForm';

const metaService = _metaService as jest.Mocked<typeof _metaService>;

jest.mock('@admin/services/metaService');

describe('ReleaseSummaryForm', () => {
  const testReleaseTypes: IdTitlePair[] = [
    { id: 'a', title: 'Ad Hoc Statistics' },
    {
      id: 'b',
      title: 'National Statistics',
    },
    {
      id: 'c',
      title: 'Official Statistics',
    },
  ];

  const testTimeIdentifiers: TimePeriodCoverageGroup[] = [
    {
      category: { label: 'Academic year' },
      timeIdentifiers: [
        { identifier: { value: 'AY', label: 'Academic Year' } },
        { identifier: { value: 'AYQ1', label: 'Academic Year Q1' } },
        { identifier: { value: 'AYQ2', label: 'Academic Year Q2' } },
        { identifier: { value: 'AYQ3', label: 'Academic Year Q3' } },
        { identifier: { value: 'AYQ4', label: 'Academic Year Q4' } },
      ],
    },
  ];
  test('renders correctly with empty initial values', async () => {
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

    expect(
      within(screen.getByRole('group', { name: 'Release Type' })).getAllByRole(
        'radio',
      ),
    ).toHaveLength(3);
    expect(screen.getByLabelText('Ad Hoc Statistics')).toBeInTheDocument();
    expect(screen.getByLabelText('National Statistics')).toBeInTheDocument();
    expect(screen.getByLabelText('Official Statistics')).toBeInTheDocument();

    const buttonCreate = screen.getByRole('button', {
      name: 'Create new release',
    });
    expect(buttonCreate).toBeInTheDocument();
    const buttonCancel = screen.getByRole('button', {
      name: 'Cancel',
    });
    expect(buttonCancel).toBeInTheDocument();
  });

  test('validation error when no year or release type selected', async () => {
    metaService.getReleaseTypes.mockResolvedValue(testReleaseTypes);
    metaService.getTimePeriodCoverageGroups.mockResolvedValue(
      testTimeIdentifiers,
    );

    const onSubmit = jest.fn();

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
        onSubmit={onSubmit}
        onCancel={noop}
      />,
    );

    await waitFor(() => {
      expect(
        screen.getByText('Select time period coverage'),
      ).toBeInTheDocument();
    });

    testReleaseTypes.map(type => screen.getByLabelText(type.title));
    const buttonCreate = screen.getByRole('button', {
      name: 'Create new release',
    });

    userEvent.click(buttonCreate);

    await waitFor(() => {
      expect(
        screen.getByText('Enter a year', { selector: 'a' }),
      ).toBeInTheDocument();
      expect(
        screen.getByText('Choose a release type', { selector: 'a' }),
      ).toBeInTheDocument();
    });
    expect(onSubmit).not.toHaveBeenCalled();
  });

  test('validation error when year "2"', async () => {
    metaService.getReleaseTypes.mockResolvedValue(testReleaseTypes);
    metaService.getTimePeriodCoverageGroups.mockResolvedValue(
      testTimeIdentifiers,
    );

    const onSubmit = jest.fn();

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
        onSubmit={onSubmit}
        onCancel={noop}
      />,
    );

    await waitFor(() => {
      expect(
        screen.getByText('Select time period coverage'),
      ).toBeInTheDocument();
    });

    const inputYear = screen.getByLabelText(
      testTimeIdentifiers[0].category.label,
    );
    testReleaseTypes.map(type => screen.getByLabelText(type.title));
    const buttonCreate = screen.getByRole('button', {
      name: 'Create new release',
    });

    userEvent.type(inputYear, '2');
    userEvent.click(screen.getByLabelText('Ad Hoc Statistics'));
    userEvent.click(buttonCreate);

    await waitFor(() => {
      expect(
        screen.getByText('Year must be exactly 4 characters', {
          selector: 'a',
        }),
      ).toBeInTheDocument();
      expect(
        screen.queryByText('Choose a release type', {
          selector: 'a',
        }),
      ).not.toBeInTheDocument();
    });
    expect(onSubmit).not.toHaveBeenCalled();
  });

  test('validation error when year "202021"', async () => {
    metaService.getReleaseTypes.mockResolvedValue(testReleaseTypes);
    metaService.getTimePeriodCoverageGroups.mockResolvedValue(
      testTimeIdentifiers,
    );

    const onSubmit = jest.fn();

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
        onSubmit={onSubmit}
        onCancel={noop}
      />,
    );

    await waitFor(() => {
      expect(
        screen.getByText('Select time period coverage'),
      ).toBeInTheDocument();
    });

    const inputYear = screen.getByLabelText(
      testTimeIdentifiers[0].category.label,
    );
    testReleaseTypes.map(type => screen.getByLabelText(type.title));
    const buttonCreate = screen.getByRole('button', {
      name: 'Create new release',
    });

    userEvent.type(inputYear, '202021');
    userEvent.click(screen.getByLabelText('Ad Hoc Statistics'));
    userEvent.click(buttonCreate);

    await waitFor(() => {
      expect(
        screen.getByText('Year must be exactly 4 characters', {
          selector: 'a',
        }),
      ).toBeInTheDocument();
      expect(
        screen.queryByText('Choose a release type', {
          selector: 'a',
        }),
      ).not.toBeInTheDocument();
    });
    expect(onSubmit).not.toHaveBeenCalled();
  });

  test('validation with valid year but no release type selected', async () => {
    metaService.getReleaseTypes.mockResolvedValue(testReleaseTypes);
    metaService.getTimePeriodCoverageGroups.mockResolvedValue(
      testTimeIdentifiers,
    );

    const onSubmit = jest.fn();

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
        onSubmit={onSubmit}
        onCancel={noop}
      />,
    );

    await waitFor(() => {
      expect(
        screen.getByText('Select time period coverage'),
      ).toBeInTheDocument();
      expect(
        screen.queryByText('Choose a release type', {
          selector: 'a',
        }),
      ).not.toBeInTheDocument();
    });

    const inputYear = screen.getByLabelText(
      testTimeIdentifiers[0].category.label,
    );
    testReleaseTypes.map(type => screen.getByLabelText(type.title));
    const buttonCreate = screen.getByRole('button', {
      name: 'Create new release',
    });

    userEvent.type(inputYear, '1966');
    userEvent.click(buttonCreate);

    await waitFor(() => {
      expect(
        screen.queryByText('Choose a release type', {
          selector: 'a',
        }),
      ).toBeInTheDocument();
    });
    expect(onSubmit).not.toHaveBeenCalled();
  });

  test('renders with provided initial values', async () => {
    metaService.getReleaseTypes.mockResolvedValue(testReleaseTypes);
    metaService.getTimePeriodCoverageGroups.mockResolvedValue(
      testTimeIdentifiers,
    );

    render(
      <ReleaseSummaryForm<ReleaseSummaryFormValues>
        submitText="Create new release"
        initialValues={() => {
          return {
            timePeriodCoverageCode: 'AYQ4',
            timePeriodCoverageStartYear: '1966',
            releaseTypeId: testReleaseTypes[1].id,
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
    expect(selectYearType).toHaveValue('AYQ4');

    const inputYear = screen.getByLabelText(
      testTimeIdentifiers[0].category.label,
    );
    expect(inputYear).toHaveValue(1966);

    const radioOptionsReleaseType = within(
      screen.getByRole('group', { name: 'Release Type' }),
    ).getAllByRole('radio');
    expect(radioOptionsReleaseType).toHaveLength(3);
    expect(radioOptionsReleaseType[0]).not.toBeChecked();
    expect(radioOptionsReleaseType[1]).toBeChecked();
    expect(radioOptionsReleaseType[2]).not.toBeChecked();
  });

  test('submits form with valid values', async () => {
    metaService.getReleaseTypes.mockResolvedValue(testReleaseTypes);
    metaService.getTimePeriodCoverageGroups.mockResolvedValue(
      testTimeIdentifiers,
    );

    const onSubmit = jest.fn();

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
        onSubmit={onSubmit}
        onCancel={noop}
      />,
    );

    await waitFor(() => {
      expect(
        screen.getByText('Select time period coverage'),
      ).toBeInTheDocument();
    });

    const selectYearType = screen.getByLabelText('Type', {
      selector: 'select',
    });
    userEvent.selectOptions(selectYearType, 'AY');
    const inputYear = screen.getByLabelText(
      testTimeIdentifiers[0].category.label,
    );
    userEvent.type(inputYear, '1966');

    const radioOptionsReleaseType = testReleaseTypes.map(type =>
      screen.getByLabelText(type.title),
    );
    userEvent.click(radioOptionsReleaseType[0]);

    const buttonCreate = screen.getByRole('button', {
      name: 'Create new release',
    });
    expect(onSubmit).not.toHaveBeenCalled();
    userEvent.click(buttonCreate);

    await waitFor(() => {
      expect(onSubmit).toHaveBeenCalledTimes(1);
    });
  });
});
