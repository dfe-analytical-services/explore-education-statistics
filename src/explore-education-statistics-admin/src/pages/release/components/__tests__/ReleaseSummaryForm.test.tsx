import _metaService from '@admin/services/metaService';
import { render, screen, waitFor, within, wait } from '@testing-library/react';
import noop from 'lodash/noop';
import React from 'react';
import ReleaseSummaryForm, {
  ReleaseSummaryFormValues,
} from '../ReleaseSummaryForm';
import userEvent from '@testing-library/user-event';

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

    const selectYearType = screen.getByLabelText('Type', {
      selector: 'select',
    });
    expect(selectYearType).toBeInTheDocument();

    const inputYear = screen.getByLabelText(
      testTimeIdentifiers[0].category.label,
      { selector: 'input' },
    );
    expect(inputYear).toBeInTheDocument();

    testReleaseTypes.forEach(type => {
      const radioReleaseType = screen.getByLabelText(type.title, {
        selector: 'input',
      });
      expect(radioReleaseType).toBeInTheDocument();
    });

    const buttonCreate = screen.getByText('Create new release', {
      selector: 'button',
    });
    expect(buttonCreate).toBeInTheDocument();
    const buttonCancel = screen.getByText('Cancel', { selector: 'button' });
    expect(buttonCancel).toBeInTheDocument();
  });

  test('validation error render where appropriate', async () => {
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
      { selector: 'input' },
    );
    const radioOptionsReleaseType = testReleaseTypes.map(type =>
      screen.getByLabelText(type.title, {
        selector: 'input',
      }),
    );
    const buttonCreate = screen.getByText('Create new release', {
      selector: 'button',
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

    userEvent.type(inputYear, '2');

    await waitFor(() => {
      expect(
        screen.getByText('Year must be exactly 4 characters', {
          selector: 'a',
        }),
      ).toBeInTheDocument();
    });

    userEvent.type(inputYear, '202021');

    await waitFor(() => {
      expect(
        screen.getByText('Year must be exactly 4 characters', {
          selector: 'a',
        }),
      ).toBeInTheDocument();
    });

    userEvent.clear(inputYear);
    userEvent.type(inputYear, '1966');

    await waitFor(() => {
      expect(
        screen.queryByText('Year must be exactly 4 characters', {
          selector: 'a',
        }),
      ).not.toBeInTheDocument();
      expect(
        screen.queryByText('Enter a year', {
          selector: 'a',
        }),
      ).not.toBeInTheDocument();
    });

    userEvent.click(radioOptionsReleaseType[0]);

    await waitFor(() => {
      expect(
        screen.queryByText('Choose a release type', {
          selector: 'a',
        }),
      ).not.toBeInTheDocument();
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

    const selectYearType = screen.getByLabelText('Type', {
      selector: 'select',
    });
    expect(selectYearType).toHaveValue('AYQ4');

    const inputYear = screen.getByLabelText(
      testTimeIdentifiers[0].category.label,
      { selector: 'input' },
    );
    expect(inputYear).toHaveValue(1966);

    const radioOptionsReleaseType = testReleaseTypes.map(type =>
      screen.getByLabelText(type.title, {
        selector: 'input',
      }),
    );
    radioOptionsReleaseType.forEach((radio, index) => {
      if (index === 1) return expect(radio).toBeChecked();
      expect(radio).not.toBeChecked();
    });
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
      { selector: 'input' },
    );
    userEvent.type(inputYear, '1966');

    const radioOptionsReleaseType = testReleaseTypes.map(type =>
      screen.getByLabelText(type.title, {
        selector: 'input',
      }),
    );
    userEvent.click(radioOptionsReleaseType[0]);

    const buttonCreate = screen.getByText('Create new release', {
      selector: 'button',
    });
    expect(onSubmit).not.toHaveBeenCalled();
    userEvent.click(buttonCreate);

    await waitFor(() => {
      expect(onSubmit).toHaveBeenCalledTimes(1);
    });
  });
});
