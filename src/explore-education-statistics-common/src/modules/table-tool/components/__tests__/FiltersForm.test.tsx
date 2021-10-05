import FiltersForm from '@common/modules/table-tool/components/FiltersForm';
import { InjectedWizardProps } from '@common/modules/table-tool/components/Wizard';
import { SubjectMeta } from '@common/services/tableBuilderService';
import { waitFor } from '@testing-library/dom';
import { render, screen, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import noop from 'lodash/noop';
import React from 'react';

describe('FiltersForm', () => {
  const testSubjectMeta: SubjectMeta = {
    filters: {
      SchoolType: {
        totalValue: '',
        hint: 'Filter by school type',
        legend: 'School type',
        options: {
          Default: {
            label: 'Default',
            options: [
              {
                label: 'State-funded secondary',
                value: 'state-funded-secondary',
              },
              {
                label: 'Special',
                value: 'special',
              },
            ],
          },
        },
        name: 'school_type',
      },
      Characteristic: {
        totalValue: '',
        hint: 'Filter by pupil characteristic',
        legend: 'Characteristic',
        options: {
          EthnicGroupMajor: {
            label: 'Ethnic group major',
            options: [
              {
                label: 'Ethnicity Major Black Total',
                value: 'ethnicity-major-black-total',
              },
              {
                label: 'Ethnicity Major Mixed Total',
                value: 'ethnicity-major-mixed-total',
              },
              {
                label: 'Ethnicity Major Asian Total',
                value: 'ethnicity-major-asian-total',
              },
            ],
          },
          Gender: {
            label: 'Gender',
            options: [
              {
                label: 'Gender female',
                value: 'gender-female',
              },
              {
                label: 'Gender male',
                value: 'gender-male',
              },
            ],
          },
          Total: {
            label: 'Total',
            options: [
              {
                label: 'Total',
                value: 'total',
              },
            ],
          },
        },
        name: 'characteristic',
      },
    },
    indicators: {
      AbsenceByReason: {
        label: 'Absence by reason',
        options: [
          {
            label: 'Number of excluded sessions',
            unit: '',
            value: 'number-excluded-sessions',
            name: 'sess_auth_excluded',
          },
          {
            label: 'Number of unauthorised reasons sessions',
            unit: '',
            value: 'number-unauthorised-reasons-sessions',
            name: 'sess_unauth_totalreasons',
          },
        ],
      },
      AbsenceFields: {
        label: 'Absence fields',
        options: [
          {
            label: 'Authorised absence rate',
            unit: '%',
            value: 'authorised-absence-rate',
            name: 'sess_authorised_percent',
          },
          {
            label: 'Number of overall absence sessions',
            unit: '',
            value: 'number-overall-absence-sessions',
            name: 'sess_overall',
          },
          {
            label: 'Unauthorised absence rate',
            unit: '%',
            value: 'unauthorised-absence-rate',
            name: 'sess_unauthorised_percent',
          },
        ],
      },
    },
    locations: {},
    timePeriod: {
      hint: '',
      legend: '',
      options: [],
    },
  };

  const testSubjectMetaNoFiltersOneIndicator: SubjectMeta = {
    filters: {},
    indicators: {
      AbsenceByReason: {
        label: 'Absence by reason',
        options: [
          {
            label: 'Number of excluded sessions',
            unit: '',
            value: 'number-excluded-sessions',
            name: 'sess_auth_excluded',
          },
        ],
      },
    },
    locations: {},
    timePeriod: {
      hint: '',
      legend: '',
      options: [],
    },
  };

  const testWizardStepProps: InjectedWizardProps = {
    currentStep: 1,
    isActive: true,
    isLoading: false,
    stepNumber: 1,
    setCurrentStep: noop,
    goToNextStep: noop,
    goToPreviousStep: noop,
    shouldScroll: false,
  };

  test('renders filter group options correctly', () => {
    render(
      <FiltersForm
        {...testWizardStepProps}
        subjectMeta={testSubjectMeta}
        onSubmit={noop}
      />,
    );

    expect(
      within(
        screen.getByRole('group', {
          name: 'School type',
          hidden: true,
        }),
      ).getAllByRole('checkbox', {
        hidden: true,
      }),
    ).toHaveLength(2);

    expect(
      within(
        screen.getByRole('group', {
          name: 'Ethnic group major',
          hidden: true,
        }),
      ).getAllByRole('checkbox', {
        hidden: true,
      }),
    ).toHaveLength(3);

    expect(
      within(
        screen.getByRole('group', {
          name: 'Gender',
          hidden: true,
        }),
      ).getAllByRole('checkbox', {
        hidden: true,
      }),
    ).toHaveLength(2);

    expect(
      within(
        screen.getByRole('group', {
          name: 'Total',
          hidden: true,
        }),
      ).getAllByRole('checkbox', {
        hidden: true,
      }),
    ).toHaveLength(1);

    expect(
      within(
        screen.getByRole('group', {
          name: 'Absence by reason',
        }),
      ).getAllByRole('checkbox'),
    ).toHaveLength(2);

    expect(
      within(
        screen.getByRole('group', {
          name: 'Absence fields',
        }),
      ).getAllByRole('checkbox'),
    ).toHaveLength(3);
  });

  test('automatically selects checkbox for a `Default` filter group with a single option', () => {
    render(
      <FiltersForm
        {...testWizardStepProps}
        subjectMeta={{
          ...testSubjectMeta,
          filters: {
            ...testSubjectMeta.filters,
            SchoolType: {
              ...testSubjectMeta.filters.SchoolType,
              totalValue: 'total',
              options: {
                Default: {
                  label: 'Default',
                  options: [
                    {
                      label: 'State-funded secondary',
                      value: 'state-funded-secondary',
                    },
                  ],
                },
              },
            },
          },
        }}
        onSubmit={noop}
      />,
    );

    expect(
      screen.getByRole('button', {
        name: 'School type - 1 selected',
      }),
    ).toBeInTheDocument();

    expect(
      screen.getAllByRole('checkbox', {
        hidden: true,
        name: (_, element) => (element as HTMLInputElement).checked,
      }),
    ).toHaveLength(1);

    expect(screen.getByLabelText('State-funded secondary')).toHaveAttribute(
      'checked',
    );
  });

  test('selecting options shows the number of selected options for each filter group', () => {
    render(
      <FiltersForm
        {...testWizardStepProps}
        subjectMeta={testSubjectMeta}
        onSubmit={noop}
      />,
    );

    userEvent.click(screen.getByLabelText('State-funded secondary'));

    expect(
      screen.getByRole('button', {
        name: 'School type - 1 selected',
      }),
    ).toBeInTheDocument();

    userEvent.click(screen.getByLabelText('Ethnicity Major Mixed Total'));
    userEvent.click(screen.getByLabelText('Gender male'));
    userEvent.click(screen.getByLabelText('Total'));

    expect(
      screen.getByRole('button', {
        name: 'Characteristic - 3 selected',
      }),
    ).toBeInTheDocument();

    userEvent.click(screen.getByLabelText('Number of excluded sessions'));
    userEvent.click(screen.getByLabelText('Unauthorised absence rate'));

    expect(
      screen.getByRole('group', {
        name: 'Indicators - 2 selected',
      }),
    ).toBeInTheDocument();
  });

  test('shows validation errors if no options are selected from the filter groups', async () => {
    render(
      <FiltersForm
        {...testWizardStepProps}
        subjectMeta={testSubjectMeta}
        onSubmit={noop}
      />,
    );

    userEvent.click(
      screen.getByRole('button', {
        name: 'Create table',
      }),
    );

    await waitFor(() => {
      expect(
        screen.getByRole('link', {
          name: 'Select at least one option from indicators',
        }),
      ).toBeInTheDocument();

      expect(
        screen.getByRole('link', {
          name: 'Select at least one option from characteristic',
        }),
      ).toBeInTheDocument();

      expect(
        screen.getByRole('link', {
          name: 'Select at least one option from school type',
        }),
      ).toBeInTheDocument();
    });
  });

  test('correct checkboxes are selected from initial values', () => {
    render(
      <FiltersForm
        {...testWizardStepProps}
        initialValues={{
          filters: ['state-funded-secondary', 'ethnicity-major-asian-total'],
          indicators: ['unauthorised-absence-rate'],
        }}
        subjectMeta={testSubjectMeta}
        onSubmit={noop}
      />,
    );

    expect(screen.getByLabelText('State-funded secondary')).toHaveAttribute(
      'checked',
    );
    expect(
      screen.getByLabelText('Ethnicity Major Asian Total'),
    ).toHaveAttribute('checked');
    expect(screen.getByLabelText('Unauthorised absence rate')).toHaveAttribute(
      'checked',
    );
  });

  test('other checkboxes are not selected from initial values', () => {
    render(
      <FiltersForm
        {...testWizardStepProps}
        initialValues={{
          filters: ['state-funded-secondary', 'ethnicity-major-asian-total'],
          indicators: ['unauthorised-absence-rate'],
        }}
        subjectMeta={testSubjectMeta}
        onSubmit={noop}
      />,
    );

    expect(screen.getByLabelText('Special')).not.toHaveAttribute('checked');
    expect(
      screen.getByLabelText('Ethnicity Major Mixed Total'),
    ).not.toHaveAttribute('checked');
    expect(
      screen.getByLabelText('Number of overall absence sessions'),
    ).not.toHaveAttribute('checked');
  });

  test('automatically selects checkbox when there is only one indicator and no filters', () => {
    render(
      <FiltersForm
        {...testWizardStepProps}
        subjectMeta={testSubjectMetaNoFiltersOneIndicator}
        onSubmit={noop}
      />,
    );

    expect(
      within(
        screen.getByRole('group', {
          name: 'Indicators - 1 selected',
        }),
      ).getAllByRole('checkbox'),
    ).toHaveLength(1);

    expect(
      screen.getByLabelText('Number of excluded sessions'),
    ).toHaveAttribute('checked');
  });

  test('upon submit automatically selects Total checkbox if no other options in that group are checked', async () => {
    render(
      <FiltersForm
        {...testWizardStepProps}
        subjectMeta={{
          ...testSubjectMeta,
          filters: {
            ...testSubjectMeta.filters,
            Characteristic: {
              ...testSubjectMeta.filters.Characteristic,
              totalValue: 'total',
            },
          },
        }}
        onSubmit={noop}
      />,
    );

    userEvent.click(screen.getByLabelText('State-funded secondary'));
    userEvent.click(screen.getByLabelText('Number of excluded sessions'));

    expect((screen.getByLabelText('Total') as HTMLInputElement).checked).toBe(
      false,
    );

    userEvent.click(
      screen.getByRole('button', {
        name: 'Create table',
      }),
    );

    await waitFor(() => {
      expect((screen.getByLabelText('Total') as HTMLInputElement).checked).toBe(
        true,
      );
    });
  });

  test('renders a read-only view of selected options when no longer the current step', async () => {
    const { container, rerender } = render(
      <FiltersForm
        {...testWizardStepProps}
        subjectMeta={testSubjectMeta}
        onSubmit={noop}
      />,
    );

    userEvent.click(screen.getByLabelText('State-funded secondary'));
    userEvent.click(screen.getByLabelText('Ethnicity Major Black Total'));
    userEvent.click(screen.getByLabelText('Number of excluded sessions'));

    await rerender(
      <FiltersForm
        {...testWizardStepProps}
        isActive={false}
        subjectMeta={testSubjectMeta}
        onSubmit={noop}
      />,
    );

    expect(screen.queryAllByRole('checkbox')).toHaveLength(0);

    expect(container.querySelector('dl')).toMatchSnapshot();
  });
});
