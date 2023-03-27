import { createServerValidationErrorMock } from '@common-test/createAxiosErrorMock';
import FiltersForm, {
  TableQueryErrorCode,
} from '@common/modules/table-tool/components/FiltersForm';
import { InjectedWizardProps } from '@common/modules/table-tool/components/Wizard';
import { Subject, SubjectMeta } from '@common/services/tableBuilderService';
import { waitFor } from '@testing-library/dom';
import { render, screen, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import produce from 'immer';
import noop from 'lodash/noop';
import React from 'react';

describe('FiltersForm', () => {
  const testSubjectMeta: SubjectMeta = {
    filters: {
      SchoolType: {
        id: 'school-type',
        totalValue: '',
        hint: 'Filter by school type',
        legend: 'School type',
        options: {
          Default: {
            id: 'default',
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
            order: 0,
          },
        },
        name: 'school_type',
        order: 0,
      },
      Characteristic: {
        id: 'characteristic',
        totalValue: '',
        hint: 'Filter by pupil characteristic',
        legend: 'Characteristic',
        options: {
          EthnicGroupMajor: {
            id: 'ethnic-group-major',
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
            order: 0,
          },
          Gender: {
            id: 'Gender',
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
            order: 1,
          },
          Total: {
            id: 'total',
            label: 'Total',
            options: [
              {
                label: 'Total',
                value: 'total',
              },
            ],
            order: 2,
          },
        },
        name: 'characteristic',
        order: 1,
      },
    },
    indicators: {
      AbsenceByReason: {
        id: 'absence-by-reason',
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
        order: 0,
      },
      AbsenceFields: {
        id: 'absence-fields',
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
        order: 1,
      },
    },
    locations: {},
    timePeriod: {
      hint: '',
      legend: '',
      options: [],
    },
  };

  const testSubjectMetaSingleFilters: SubjectMeta = {
    ...testSubjectMeta,
    filters: {
      Characteristic: {
        id: 'characteristic',
        totalValue: '',
        hint: 'Filter by pupil characteristic',
        legend: 'Characteristic',
        options: {
          EthnicGroupMajor: {
            id: 'ethnic-group-major',
            label: 'Ethnic group major',
            options: [
              {
                label: 'Ethnicity Major Black Total',
                value: 'ethnicity-major-black-total',
              },
            ],
            order: 0,
          },
        },
        name: 'characteristic',
        order: 0,
      },
      SchoolType: {
        id: 'school-type',
        totalValue: '',
        hint: 'Filter by school type',
        legend: 'School type',
        options: {
          Default: {
            id: 'default',
            label: 'Default',
            options: [
              {
                label: 'State-funded secondary',
                value: 'state-funded-secondary',
              },
            ],
            order: 0,
          },
        },
        name: 'school_type',
        order: 1,
      },
      FilterWithMultipleOptions: {
        id: 'filter-with-multiple-options',
        totalValue: '',
        hint: 'Filter by Filter With Multiple Options',
        legend: 'Filter With Multiple Options',
        options: {
          OptionGroup1: {
            id: 'option-group-1',
            label: 'Option group 1',
            options: [
              {
                label: 'Option group 1 option 1',
                value: 'option-group-1-option-1',
              },
            ],
            order: 0,
          },
          OptionGroup2: {
            id: 'option-group-2',
            label: 'Option group 2',
            options: [
              {
                label: 'Option group 2 option 1',
                value: 'option-group-2-option-1',
              },
              {
                label: 'Option group 2 option 2',
                value: 'option-group-2-option-2',
              },
            ],
            order: 1,
          },
        },
        name: 'characteristic',
        order: 2,
      },
    },
  };

  const testSubjectMetaOneIndicator: SubjectMeta = {
    ...testSubjectMeta,
    indicators: {
      AbsenceByReason: {
        id: 'absence-by-reason',
        label: 'Absence by reason',
        options: [
          {
            label: 'Number of excluded sessions',
            unit: '',
            value: 'number-excluded-sessions',
            name: 'sess_auth_excluded',
          },
        ],
        order: 0,
      },
    },
  };

  const testWizardStepProps: InjectedWizardProps = {
    currentStep: 1,
    isActive: true,
    isEnabled: true,
    isLoading: false,
    stepNumber: 1,
    setCurrentStep: (step, task) => task?.(),
    goToNextStep: task => task?.(),
    goToPreviousStep: task => task?.(),
    shouldScroll: false,
  };

  const testSubject = {
    id: 'subject-1',
    name: 'Subject 1',
    file: {
      id: 'file-1',
      fileName: 'File 1',
      extension: 'csv',
      name: 'File 1',
      size: '100mb',
      type: 'Data',
    },
  } as Subject;

  test('renders indicators and filter group options correctly', () => {
    render(
      <FiltersForm
        {...testWizardStepProps}
        subject={testSubject}
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

  test('automatically selects checkbox when a filter has one group with one option', () => {
    render(
      <FiltersForm
        {...testWizardStepProps}
        subject={testSubject}
        subjectMeta={testSubjectMetaSingleFilters}
        onSubmit={noop}
      />,
    );

    expect(
      screen.getByRole('button', {
        name: 'School type - 1 selected',
      }),
    ).toBeInTheDocument();
    const filterGroup1 = screen.getByRole('group', {
      name: 'School type',
    });
    const filterCheckboxes1 = within(filterGroup1).getAllByRole('checkbox');
    expect(filterCheckboxes1).toHaveLength(1);
    expect(filterCheckboxes1[0]).toEqual(
      within(filterGroup1).getByLabelText('State-funded secondary'),
    );
    expect(filterCheckboxes1[0]).toBeChecked();

    expect(
      screen.getByRole('button', {
        name: 'Characteristic - 1 selected',
      }),
    ).toBeInTheDocument();
    const filterGroup2 = screen.getByRole('group', {
      name: 'Characteristic',
    });
    const filterCheckboxes2 = within(filterGroup2).getAllByRole('checkbox');
    expect(filterCheckboxes2).toHaveLength(1);
    expect(filterCheckboxes2[0]).toEqual(
      within(filterGroup2).getByLabelText('Ethnicity Major Black Total'),
    );
    expect(filterCheckboxes2[0]).toBeChecked();
  });

  test('does not automatically select checkbox when filter has multiple groups', () => {
    render(
      <FiltersForm
        {...testWizardStepProps}
        subject={testSubject}
        subjectMeta={testSubjectMetaSingleFilters}
        onSubmit={noop}
      />,
    );

    expect(
      screen.getByRole('button', {
        name: 'Filter With Multiple Options',
      }),
    ).toBeInTheDocument();
    const filterGroup1 = screen.getByRole('group', {
      name: 'Option group 1',
      hidden: true,
    });
    const filterCheckboxes1 = within(filterGroup1).getAllByRole('checkbox', {
      hidden: true,
    });
    expect(filterCheckboxes1).toHaveLength(1);
    expect(filterCheckboxes1[0]).toEqual(
      within(filterGroup1).getByLabelText('Option group 1 option 1'),
    );
    expect(filterCheckboxes1[0]).not.toBeChecked();

    const filterGroup2 = screen.getByRole('group', {
      name: 'Option group 2',
      hidden: true,
    });
    const filterCheckboxes2 = within(filterGroup2).getAllByRole('checkbox', {
      hidden: true,
    });
    expect(filterCheckboxes2).toHaveLength(2);
    expect(filterCheckboxes2[0]).toEqual(
      within(filterGroup2).getByLabelText('Option group 2 option 1'),
    );
    expect(filterCheckboxes2[0]).not.toBeChecked();
    expect(filterCheckboxes2[1]).toEqual(
      within(filterGroup2).getByLabelText('Option group 2 option 2'),
    );
    expect(filterCheckboxes2[1]).not.toBeChecked();
  });

  test('selecting options shows the number of selected options for each filter and indicator group', () => {
    render(
      <FiltersForm
        {...testWizardStepProps}
        subject={testSubject}
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

  test('shows validation errors if no options are selected from the filter and indicator groups', async () => {
    render(
      <FiltersForm
        {...testWizardStepProps}
        subject={testSubject}
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
        subject={testSubject}
        subjectMeta={testSubjectMeta}
        onSubmit={noop}
      />,
    );

    expect(screen.getByLabelText('State-funded secondary')).toBeChecked();
    expect(screen.getByLabelText('Ethnicity Major Asian Total')).toBeChecked();
    expect(screen.getByLabelText('Unauthorised absence rate')).toBeChecked();
  });

  test('other checkboxes are not selected from initial values', () => {
    render(
      <FiltersForm
        {...testWizardStepProps}
        initialValues={{
          filters: ['state-funded-secondary', 'ethnicity-major-asian-total'],
          indicators: ['unauthorised-absence-rate'],
        }}
        subject={testSubject}
        subjectMeta={testSubjectMeta}
        onSubmit={noop}
      />,
    );

    expect(screen.getByLabelText('Special')).not.toBeChecked();
    expect(
      screen.getByLabelText('Ethnicity Major Mixed Total'),
    ).not.toBeChecked();
    expect(
      screen.getByLabelText('Number of overall absence sessions'),
    ).not.toBeChecked();
  });

  test('automatically selects checkbox when there is only one indicator group with one option', () => {
    render(
      <FiltersForm
        {...testWizardStepProps}
        subject={testSubject}
        subjectMeta={testSubjectMetaOneIndicator}
        onSubmit={noop}
      />,
    );

    const filterGroup = screen.getByRole('group', {
      name: 'Indicators - 1 selected',
    });

    expect(within(filterGroup).getAllByRole('checkbox')).toHaveLength(1);

    expect(
      within(filterGroup).getByLabelText('Number of excluded sessions'),
    ).toBeChecked();
  });

  test('upon submit automatically selects Total checkbox if no other options in that filter group are checked', async () => {
    render(
      <FiltersForm
        {...testWizardStepProps}
        subject={testSubject}
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
        subject={testSubject}
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
        subject={testSubject}
        subjectMeta={testSubjectMeta}
        onSubmit={noop}
      />,
    );

    expect(screen.queryAllByRole('checkbox')).toHaveLength(0);

    expect(container.querySelector('dl')).toMatchSnapshot();
  });

  test('shows table size error when the correct error response is returned from the API', async () => {
    const errorResponse = createServerValidationErrorMock<TableQueryErrorCode>([
      'QueryExceedsMaxAllowableTableSize',
    ]);
    const onSubmit = jest.fn(() => Promise.reject(errorResponse));

    render(
      <FiltersForm
        {...testWizardStepProps}
        subject={testSubject}
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
        onSubmit={onSubmit}
      />,
    );

    userEvent.click(screen.getByLabelText('State-funded secondary'));
    userEvent.click(screen.getByLabelText('Number of excluded sessions'));
    userEvent.click(screen.getByLabelText('Total'));

    userEvent.click(screen.getByRole('button', { name: 'Create table' }));

    await waitFor(() => {
      expect(screen.getByText(/Could not create table/)).toBeInTheDocument();
    });

    expect(
      screen.getByText(/exceed the maximum allowed table size/),
    ).toBeInTheDocument();
    expect(
      screen.getByText(/Select different filters or download the subject data/),
    ).toBeInTheDocument();
    expect(screen.getByText(/Download Subject 1/)).toBeInTheDocument();
    expect(screen.getByText(/csv, 100mb/)).toBeInTheDocument();
    expect(
      screen.getByText(/available when the release is published/),
    ).toBeInTheDocument();
  });

  test('shows table timeout error when the correct error response is returned from the API', async () => {
    const errorResponse = createServerValidationErrorMock<TableQueryErrorCode>([
      'RequestCancelled',
    ]);
    const onSubmit = jest.fn(() => Promise.reject(errorResponse));

    render(
      <FiltersForm
        {...testWizardStepProps}
        subject={testSubject}
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
        onSubmit={onSubmit}
      />,
    );

    userEvent.click(screen.getByLabelText('State-funded secondary'));
    userEvent.click(screen.getByLabelText('Number of excluded sessions'));
    userEvent.click(screen.getByLabelText('Total'));

    userEvent.click(screen.getByRole('button', { name: 'Create table' }));

    await waitFor(() => {
      expect(screen.getByText(/Could not create table/)).toBeInTheDocument();
    });

    expect(screen.getByText(/took too long to respond/)).toBeInTheDocument();
    expect(
      screen.getByText(
        /Select different filters, try again later or download the subject data/,
      ),
    ).toBeInTheDocument();
    expect(screen.getByText(/Download Subject 1/)).toBeInTheDocument();
    expect(screen.getByText(/csv, 100mb/)).toBeInTheDocument();
    expect(
      screen.getByText(/available when the release is published/),
    ).toBeInTheDocument();
  });

  test('orders filters and filter groups by the order property', () => {
    const orderedTestSubjectMeta = produce(testSubjectMeta, draft => {
      draft.filters.SchoolType.order = 1;
      draft.filters.Characteristic.order = 0;
      draft.filters.Characteristic.options.EthnicGroupMajor.order = 2;
      draft.filters.Characteristic.options.Total.order = 0;
    });

    render(
      <FiltersForm
        {...testWizardStepProps}
        subject={testSubject}
        subjectMeta={orderedTestSubjectMeta}
        onSubmit={noop}
      />,
    );

    const filters = within(
      screen.getByRole('group', { name: 'Categories' }),
    ).getAllByRole('group');
    expect(within(filters[0]).getByRole('button', { name: 'Characteristic' }));
    // there are 4 groups within the characteristic group, so school type is the 6th group
    expect(
      within(filters[5]).getByRole('button', {
        name: 'School type',
      }),
    );

    const characteristicGroups = within(filters[0]).getAllByRole('group', {
      hidden: true,
    });
    expect(characteristicGroups[1]).toEqual(
      screen.getByRole('group', { name: 'Total', hidden: true }),
    );
    expect(characteristicGroups[2]).toEqual(
      screen.getByRole('group', { name: 'Gender', hidden: true }),
    );
    expect(characteristicGroups[3]).toEqual(
      screen.getByRole('group', { name: 'Ethnic group major', hidden: true }),
    );
  });

  test('orders indicator groups by the order property', () => {
    const orderedTestSubjectMeta = produce(testSubjectMeta, draft => {
      draft.indicators.AbsenceByReason.order = 1;
      draft.indicators.AbsenceFields.order = 0;
    });

    render(
      <FiltersForm
        {...testWizardStepProps}
        subject={testSubject}
        subjectMeta={orderedTestSubjectMeta}
        onSubmit={noop}
      />,
    );

    const indicators = within(
      screen.getByRole('group', { name: 'Indicators' }),
    ).getAllByRole('group');
    expect(indicators[0]).toEqual(
      screen.getByRole('group', { name: 'Absence fields', hidden: true }),
    );
    expect(indicators[1]).toEqual(
      screen.getByRole('group', { name: 'Absence by reason', hidden: true }),
    );
  });

  test('all filter groups are collapsed by default', () => {
    render(
      <FiltersForm
        {...testWizardStepProps}
        subject={testSubject}
        subjectMeta={testSubjectMeta}
        onSubmit={noop}
      />,
    );

    expect(
      screen.getByRole('group', {
        name: 'School type',
        hidden: true,
      }),
    ).not.toBeVisible();

    expect(
      screen.getByRole('group', {
        name: 'Ethnic group major',
        hidden: true,
      }),
    ).not.toBeVisible();

    expect(
      screen.getByRole('group', {
        name: 'Gender',
        hidden: true,
      }),
    ).not.toBeVisible();

    expect(
      screen.getByRole('group', {
        name: 'Total',
        hidden: true,
      }),
    ).not.toBeVisible();
  });

  test('shows the expand all button if there are multiple filter groups', () => {
    render(
      <FiltersForm
        {...testWizardStepProps}
        subject={testSubject}
        subjectMeta={testSubjectMeta}
        onSubmit={noop}
      />,
    );

    expect(
      screen.getByRole('button', { name: 'Expand all categories' }),
    ).toBeInTheDocument();
  });

  test('does not show the expand all button if there is only one filter group', () => {
    render(
      <FiltersForm
        {...testWizardStepProps}
        subject={testSubject}
        subjectMeta={{
          ...testSubjectMeta,
          filters: {
            SchoolType: {
              id: 'school-type',
              totalValue: '',
              hint: 'Filter by school type',
              legend: 'School type',
              options: {
                Default: {
                  id: 'default',
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
                  order: 0,
                },
              },
              name: 'school_type',
              order: 0,
            },
          },
        }}
        onSubmit={noop}
      />,
    );

    expect(
      screen.queryByRole('button', { name: 'Expand all categories' }),
    ).not.toBeInTheDocument();
  });

  test('toggles expanding and collapsing all filter groups when click the button', async () => {
    render(
      <FiltersForm
        {...testWizardStepProps}
        subject={testSubject}
        subjectMeta={testSubjectMeta}
        onSubmit={noop}
      />,
    );

    expect(
      screen.getByRole('group', {
        name: 'School type',
        hidden: true,
      }),
    ).not.toBeVisible();

    expect(
      screen.getByRole('group', {
        name: 'Ethnic group major',
        hidden: true,
      }),
    ).not.toBeVisible();

    expect(
      screen.getByRole('group', {
        name: 'Gender',
        hidden: true,
      }),
    ).not.toBeVisible();

    expect(
      screen.getByRole('group', {
        name: 'Total',
        hidden: true,
      }),
    ).not.toBeVisible();

    userEvent.click(
      screen.getByRole('button', { name: 'Expand all categories' }),
    );

    await waitFor(() => {
      expect(screen.getByText(/Collapse all/)).toBeInTheDocument();
    });

    expect(
      screen.getByRole('group', {
        name: 'School type',
        hidden: true,
      }),
    ).toBeVisible();

    expect(
      screen.getByRole('group', {
        name: 'Ethnic group major',
        hidden: true,
      }),
    ).toBeVisible();

    expect(
      screen.getByRole('group', {
        name: 'Gender',
        hidden: true,
      }),
    ).toBeVisible();

    expect(
      screen.getByRole('group', {
        name: 'Total',
        hidden: true,
      }),
    ).toBeVisible();

    userEvent.click(
      screen.getByRole('button', { name: 'Collapse all categories' }),
    );

    await waitFor(() => {
      expect(screen.getByText(/Expand all/)).toBeInTheDocument();
    });

    expect(
      screen.getByRole('group', {
        name: 'School type',
        hidden: true,
      }),
    ).not.toBeVisible();

    expect(
      screen.getByRole('group', {
        name: 'Ethnic group major',
        hidden: true,
      }),
    ).not.toBeVisible();

    expect(
      screen.getByRole('group', {
        name: 'Gender',
        hidden: true,
      }),
    ).not.toBeVisible();

    expect(
      screen.getByRole('group', {
        name: 'Total',
        hidden: true,
      }),
    ).not.toBeVisible();
  });
});
