import { createServerValidationErrorMock } from '@common-test/createAxiosErrorMock';
import FiltersForm, {
  TableQueryErrorCode,
} from '@common/modules/table-tool/components/FiltersForm';
import { testWizardStepProps } from '@common/modules/table-tool/components/__tests__/__data__/testWizardStepProps';
import { Subject, SubjectMeta } from '@common/services/tableBuilderService';
import { render, screen, within, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { produce } from 'immer';
import noop from 'lodash/noop';
import React from 'react';

describe('FiltersForm', () => {
  const testSubjectMeta: SubjectMeta = {
    filters: {
      SchoolType: {
        id: 'school-type',
        autoSelectFilterItemId: '',
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
        autoSelectFilterItemId: '',
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
        autoSelectFilterItemId: '',
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
        autoSelectFilterItemId: '',
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
        autoSelectFilterItemId: '',
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
        stepTitle="Choose your filters"
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
        stepTitle="Choose your filters"
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
        stepTitle="Choose your filters"
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

  test('selecting options shows the number of selected options for each filter and indicator group', async () => {
    render(
      <FiltersForm
        {...testWizardStepProps}
        stepTitle="Choose your filters"
        subject={testSubject}
        subjectMeta={testSubjectMeta}
        onSubmit={noop}
      />,
    );

    await userEvent.click(screen.getByLabelText('State-funded secondary'));

    expect(
      screen.getByRole('button', {
        name: 'School type - 1 selected',
      }),
    ).toBeInTheDocument();

    await userEvent.click(screen.getByLabelText('Ethnicity Major Mixed Total'));
    await userEvent.click(screen.getByLabelText('Gender male'));
    await userEvent.click(screen.getByLabelText('Total'));

    expect(
      screen.getByRole('button', {
        name: 'Characteristic - 3 selected',
      }),
    ).toBeInTheDocument();

    await userEvent.click(screen.getByLabelText('Number of excluded sessions'));
    await userEvent.click(screen.getByLabelText('Unauthorised absence rate'));

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
        stepTitle="Choose your filters"
        subject={testSubject}
        subjectMeta={testSubjectMeta}
        onSubmit={noop}
      />,
    );

    await userEvent.click(
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
        stepTitle="Choose your filters"
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
        stepTitle="Choose your filters"
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
        stepTitle="Choose your filters"
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
        stepTitle="Choose your filters"
        subject={testSubject}
        subjectMeta={{
          ...testSubjectMeta,
          filters: {
            ...testSubjectMeta.filters,
            Characteristic: {
              ...testSubjectMeta.filters.Characteristic,
              autoSelectFilterItemId: 'total',
            },
          },
        }}
        onSubmit={noop}
      />,
    );

    await userEvent.click(screen.getByLabelText('State-funded secondary'));
    await userEvent.click(screen.getByLabelText('Number of excluded sessions'));

    expect((screen.getByLabelText('Total') as HTMLInputElement).checked).toBe(
      false,
    );

    await userEvent.click(
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
        stepTitle="Choose your filters"
        subject={testSubject}
        subjectMeta={testSubjectMeta}
        onSubmit={noop}
      />,
    );

    await userEvent.click(screen.getByLabelText('State-funded secondary'));
    await userEvent.click(screen.getByLabelText('Ethnicity Major Black Total'));
    await userEvent.click(screen.getByLabelText('Number of excluded sessions'));

    await rerender(
      <FiltersForm
        {...testWizardStepProps}
        stepTitle="Choose your filters"
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
      { code: 'QueryExceedsMaxAllowableTableSize', message: '' },
    ]);
    const onSubmit = jest.fn(() => Promise.reject(errorResponse));

    render(
      <FiltersForm
        {...testWizardStepProps}
        stepTitle="Choose your filters"
        subject={testSubject}
        subjectMeta={{
          ...testSubjectMeta,
          filters: {
            ...testSubjectMeta.filters,
            Characteristic: {
              ...testSubjectMeta.filters.Characteristic,
              autoSelectFilterItemId: 'total',
            },
          },
        }}
        onSubmit={onSubmit}
      />,
    );

    await userEvent.click(screen.getByLabelText('State-funded secondary'));
    await userEvent.click(screen.getByLabelText('Number of excluded sessions'));
    await userEvent.click(screen.getByLabelText('Total'));

    await userEvent.click(screen.getByRole('button', { name: 'Create table' }));

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
      { code: 'RequestCancelled', message: '' },
    ]);
    const onSubmit = jest.fn(() => Promise.reject(errorResponse));

    render(
      <FiltersForm
        {...testWizardStepProps}
        stepTitle="Choose your filters"
        subject={testSubject}
        subjectMeta={{
          ...testSubjectMeta,
          filters: {
            ...testSubjectMeta.filters,
            Characteristic: {
              ...testSubjectMeta.filters.Characteristic,
              autoSelectFilterItemId: 'total',
            },
          },
        }}
        onSubmit={onSubmit}
      />,
    );

    await userEvent.click(screen.getByLabelText('State-funded secondary'));
    await userEvent.click(screen.getByLabelText('Number of excluded sessions'));
    await userEvent.click(screen.getByLabelText('Total'));

    await userEvent.click(screen.getByRole('button', { name: 'Create table' }));

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
        stepTitle="Choose your filters"
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
        stepTitle="Choose your filters"
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
        stepTitle="Choose your filters"
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
        stepTitle="Choose your filters"
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
        stepTitle="Choose your filters"
        subject={testSubject}
        subjectMeta={{
          ...testSubjectMeta,
          filters: {
            SchoolType: {
              id: 'school-type',
              autoSelectFilterItemId: '',
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
        stepTitle="Choose your filters"
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

    await userEvent.click(
      screen.getByRole('button', { name: 'Expand all categories' }),
    );

    expect(
      await screen.findByRole('button', { name: 'Collapse all categories' }),
    ).toBeInTheDocument();

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

    await userEvent.click(
      screen.getByRole('button', { name: 'Collapse all categories' }),
    );

    expect(
      await screen.findByRole('button', { name: 'Expand all categories' }),
    ).toBeInTheDocument();

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

  test('shows additional hint text if any filters contain a "Total"', async () => {
    render(
      <FiltersForm
        {...testWizardStepProps}
        stepTitle="Choose your filters"
        subject={testSubject}
        subjectMeta={{
          ...testSubjectMeta,
          filters: {
            ...testSubjectMeta.filters,
            Characteristic: {
              ...testSubjectMeta.filters.Characteristic,
              autoSelectFilterItemId: 'total',
            },
          },
        }}
        onSubmit={noop}
      />,
    );

    expect(
      screen.getByText(/Select at least one option from all categories./),
    ).toBeInTheDocument();
    expect(
      screen.getByText(
        /If no options are selected from a category then a default option \(often 'Total'\) may be selected automatically when creating a table./,
      ),
    ).toBeInTheDocument();
    expect(
      screen.getByText(
        /Where present, the 'Total' option is usually an aggregate of all other options within a category./,
      ),
    ).toBeInTheDocument();
  });

  test('does not show additional hint text if no filters contain a "Total"', async () => {
    render(
      <FiltersForm
        {...testWizardStepProps}
        stepTitle="Choose your filters"
        subject={testSubject}
        subjectMeta={testSubjectMeta}
        onSubmit={noop}
      />,
    );

    expect(
      screen.getByText('Select at least one option from all categories.'),
    ).toBeInTheDocument();

    expect(
      screen.queryByText(
        /If no options are selected from a category then a default option \(often 'Total'\) may be selected automatically when creating a table./,
      ),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByText(
        /Where present, the 'Total' option is usually an aggregate of all other options within a category./,
      ),
    ).not.toBeInTheDocument();
  });

  describe('FilterHierarchies', () => {
    const subjectMetaWithFilterHierarchy: SubjectMeta = {
      filters: {
        LevelOfQualification: {
          id: 'filter-660d764b',
          legend: 'Level of qualification',
          options: {
            Default: {
              id: '174bf78d-e667-4407-937d-e2edd0c10546',
              label: 'Default',
              options: [
                {
                  label: 'Total',
                  value: 'option-99890dde',
                },
                {
                  label: 'Entry level',
                  value: 'option-ae8fc558',
                },
                {
                  label: 'Higher',
                  value: 'option-a84c4bd9',
                },
              ],
              order: 0,
            },
          },
          name: 'qualification_level',
          autoSelectFilterItemId: 'option-99890dde',
          order: 0,
        },
        NameOfCourseBeingStudied: {
          id: 'filter-5709a7b4',
          legend: 'Name of course being studied',
          options: {
            Default: {
              id: '96be2505-40fb-4b31-9ec4-8163a829272c',
              label: 'Default',
              options: [
                {
                  label: 'Total',
                  value: 'option-9f53e999',
                },
                {
                  label: 'Biochemistry',
                  value: 'option-8949eb4e',
                },
                {
                  label: 'Bricklaying',
                  value: 'option-482ac7e3',
                },
                {
                  label: 'Bricklaying01',
                  value: 'option-f5db81a9',
                },
                {
                  label: 'Bricklaying02',
                  value: 'option-12321f4d',
                },
                {
                  label: 'Bricklaying03',
                  value: 'option-952a34be',
                },
                {
                  label: 'Bricklaying04',
                  value: 'option-f1cb3b00',
                },
                {
                  label: 'Bricklaying05',
                  value: 'option-1195279c',
                },
                {
                  label: 'Bricklaying06',
                  value: 'option-5aa58bd1',
                },
                {
                  label: 'Bricklaying07',
                  value: 'option-5a44c859',
                },
                {
                  label: 'Civil engineering',
                  value: 'option-99087931',
                },
                {
                  label: 'Electrical engineering',
                  value: 'option-8387bb38',
                },
                {
                  label: 'Forestry',
                  value: 'option-d467c799',
                },
                {
                  label: 'Hedge rows',
                  value: 'option-76b2bc7a',
                },
                {
                  label: 'Physics',
                  value: 'option-437f2fd2',
                },
                {
                  label: 'Plastering',
                  value: 'option-17eabb0d',
                },
              ],
              order: 0,
            },
          },
          name: 'course_title',
          autoSelectFilterItemId: 'option-9f53e999',
          order: 1,
        },
        SectorSubjectArea: {
          id: 'filter-949b438b',
          legend: 'Sector subject area',
          options: {
            Default: {
              id: '79c98ff8-0d40-4071-b7e1-8b23c8f7e96d',
              label: 'Default',
              options: [
                {
                  label: 'Total',
                  value: 'option-dcbf7d26',
                },
                {
                  label: 'Construction',
                  value: 'option-4f110129',
                },
                {
                  label: 'Engineering',
                  value: 'option-5687872a',
                },
                {
                  label: 'Horticulture',
                  value: 'option-ef6f22e5',
                },
                {
                  label: 'Science',
                  value: 'option-7b4cc4f3',
                },
              ],
              order: 0,
            },
          },
          name: 'subject_area',
          autoSelectFilterItemId: 'option-dcbf7d26',
          order: 2,
        },
      },
      indicators: {
        Default: {
          id: '6c23bee1',
          label: 'Default',
          options: [
            {
              label: 'Number of students enrolled',
              unit: '',
              value: 'indicator-76d5e064',
              name: 'enrollment_count',
            },
          ],
          order: 0,
        },
      },
      locations: {},
      timePeriod: {
        hint: '',
        legend: '',
        options: [],
      },
      filterHierarchies: [
        [
          {
            level: 0,
            filterId: 'filter-660d764b',
            childFilterId: 'filter-949b438b',
            hierarchy: {
              'option-ae8fc558': [
                'option-5687872a',
                'option-7b4cc4f3',
                'option-dcbf7d26',
              ],
              'option-99890dde': ['option-dcbf7d26'],
              'option-a84c4bd9': [
                'option-4f110129',
                'option-dcbf7d26',
                'option-ef6f22e5',
              ],
            },
          },
          {
            level: 1,
            filterId: 'filter-949b438b',
            childFilterId: 'filter-5709a7b4',
            hierarchy: {
              'option-5687872a': [
                'option-8387bb38',
                'option-99087931',
                'option-9f53e999',
              ],
              'option-4f110129': [
                'option-1195279c',
                'option-17eabb0d',
                'option-f1cb3b00',
                'option-952a34be',
                'option-5a44c859',
                'option-5aa58bd1',
                'option-9f53e999',
                'option-12321f4d',
                'option-f5db81a9',
                'option-482ac7e3',
              ],
              'option-7b4cc4f3': [
                'option-8949eb4e',
                'option-437f2fd2',
                'option-9f53e999',
              ],
              'option-dcbf7d26': ['option-9f53e999'],
              'option-ef6f22e5': [
                'option-76b2bc7a',
                'option-d467c799',
                'option-9f53e999',
              ],
            },
          },
        ],
      ],
    };
    test('renders as expected', async () => {
      render(
        <FiltersForm
          {...testWizardStepProps}
          stepTitle="Choose your filters"
          subject={testSubject}
          subjectMeta={subjectMetaWithFilterHierarchy}
          onSubmit={noop}
          showFilterHierarchies
        />,
      );

      expect(
        screen.getByText('Select at least one indicator below'),
      ).toBeInTheDocument();

      const hierarchyMenu = screen.getByRole('button', {
        name: 'Name of course being studied (3 tiers)',
      });
      expect(hierarchyMenu).toBeInTheDocument();
      await userEvent.click(hierarchyMenu);

      const searchBar = screen.getByLabelText(
        'Search all tiers and name of course being studied',
      );
      expect(searchBar).toBeInTheDocument();

      const modalButton = screen.getByRole('button', {
        name: 'What are name of course being studied tiers?',
      });
      expect(modalButton).toBeInTheDocument();
      await userEvent.click(modalButton);

      const modal = screen.getByRole('dialog');
      expect(modal).toBeInTheDocument();

      expect(
        within(modal).getAllByTestId('modal-tier-description-section'),
      ).toHaveLength(3);
      await userEvent.click(screen.getByText('Close'));
      expect(modal).not.toBeInTheDocument();

      const optionsContainer = screen.getByTestId('filter-hierarchy-options');
      expect(optionsContainer).toBeInTheDocument();
      expect(
        within(optionsContainer).getAllByRole('checkbox', {
          hidden: false,
        }),
      ).toHaveLength(22);

      expect(
        within(optionsContainer).getByRole('checkbox', {
          name: 'Total',
          description: 'Level of qualification',
        }),
      ).toBeInTheDocument();

      expect(
        within(optionsContainer).getByRole('checkbox', {
          name: 'Higher',
          description: 'Level of qualification',
        }),
      ).toBeInTheDocument();

      const entryLevelOption = within(optionsContainer).getByRole('checkbox', {
        name: 'Entry level',
        description: 'Level of qualification',
      });
      expect(entryLevelOption).toBeInTheDocument();
      const entryLevelOptionContainer = within(optionsContainer).getByTestId(
        `filter-hierarchy-options-option-ae8fc558`,
      );
      expect(entryLevelOptionContainer).toBeInTheDocument();
      const entryLevelOptionsMenu = within(entryLevelOptionContainer).getByText(
        'Show sector subject area',
      );

      expect(entryLevelOptionsMenu).toBeInTheDocument();
      await userEvent.click(entryLevelOptionsMenu);
      expect(entryLevelOptionsMenu).toHaveTextContent(
        'Close sector subject area',
      );

      expect(
        within(entryLevelOptionContainer).getByRole('checkbox', {
          name: 'Science',
          description: 'Sector subject area',
        }),
      ).toBeInTheDocument();
      const engineeringOption = within(entryLevelOptionContainer).getByRole(
        'checkbox',
        {
          name: 'Engineering',
          description: 'Sector subject area',
        },
      );
      expect(engineeringOption).toBeInTheDocument();
      const engineeringOptionContainer = within(optionsContainer).getByTestId(
        `filter-hierarchy-options-option-5687872a`,
      );
      expect(engineeringOptionContainer).toBeInTheDocument();
      const engineeringOptionsMenu = within(
        engineeringOptionContainer,
      ).getByText('Show name of course being studied');
      expect(engineeringOptionsMenu).toBeInTheDocument();
      await userEvent.click(engineeringOptionsMenu);
      expect(engineeringOptionsMenu).toHaveTextContent(
        'Close name of course being studied',
      );

      expect(
        within(engineeringOptionContainer).getByRole('checkbox', {
          name: 'Civil engineering',
        }),
      ).toBeInTheDocument();
      expect(
        within(engineeringOptionContainer).getByRole('checkbox', {
          name: 'Electrical engineering',
        }),
      ).toBeInTheDocument();
    });
    test('filter hierarchies form submit with correct values ', async () => {
      const onSubmit = jest.fn();
      render(
        <FiltersForm
          {...testWizardStepProps}
          stepTitle="Choose your filters"
          subject={testSubject}
          subjectMeta={subjectMetaWithFilterHierarchy}
          onSubmit={onSubmit}
          showFilterHierarchies
        />,
      );

      // open details
      const hierarchyMenu = screen.getByRole('button', {
        name: 'Name of course being studied (3 tiers)',
      });
      await userEvent.click(hierarchyMenu);

      // open some options
      const optionsContainer = screen.getByTestId('filter-hierarchy-options');
      const entryLevelOption = within(optionsContainer).getByRole('checkbox', {
        name: 'Entry level',
        description: 'Level of qualification',
      });
      expect(entryLevelOption).toBeInTheDocument();
      const entryLevelOptionContainer = within(optionsContainer).getByTestId(
        `filter-hierarchy-options-option-ae8fc558`,
      );

      const entryLevelOptionsMenu = within(entryLevelOptionContainer).getByText(
        'Show sector subject area',
      );
      await userEvent.click(entryLevelOptionsMenu);
      const engineeringOption = within(entryLevelOptionContainer).getByRole(
        'checkbox',
        {
          name: 'Engineering',
          description: 'Sector subject area',
        },
      );
      const engineeringOptionContainer = within(optionsContainer).getByTestId(
        `filter-hierarchy-options-option-5687872a`,
      );
      const engineeringOptionsMenu = within(
        engineeringOptionContainer,
      ).getByText('Show name of course being studied');
      await userEvent.click(engineeringOptionsMenu);

      const civilEngineering = within(engineeringOptionContainer).getByRole(
        'checkbox',
        {
          name: 'Civil engineering',
        },
      );
      // select some options
      await userEvent.click(entryLevelOption);
      await userEvent.click(engineeringOption);
      await userEvent.click(civilEngineering);
      // expect '3 items selected' tag on screen
      const selectionCounter = within(hierarchyMenu).getByText('3 selected');
      expect(selectionCounter).toBeInTheDocument();
      // expect for to be submitted with some option's values
      await userEvent.click(
        screen.getByRole('button', { name: 'Create table' }),
      );
      expect(onSubmit).toHaveBeenCalledTimes(1);
      expect(onSubmit).toHaveBeenCalledWith({
        filterHierarchies: {
          'filter-5709a7b4': [
            entryLevelOption.getAttribute('value'), // 'option-ae8fc558',
            engineeringOption.getAttribute('value'), // 'option-5687872a',
            civilEngineering.getAttribute('value'), // 'option-99087931',
          ],
        },
        filters: {},
        indicators: ['indicator-76d5e064'],
      });
    });
    test('filter hierarchy validation errors show and function', async () => {
      render(
        <FiltersForm
          {...testWizardStepProps}
          stepTitle="Choose your filters"
          subject={testSubject}
          subjectMeta={subjectMetaWithFilterHierarchy}
          onSubmit={jest.fn}
          showFilterHierarchies
        />,
      );
      // attempt to submit no filter hierarchy selectiosn
      await userEvent.click(
        screen.getByRole('button', { name: 'Create table' }),
      );
      // validation error kicks in
      const errorSummaryContainer = screen.getByTestId('errorSummary');
      expect(errorSummaryContainer).toBeInTheDocument();
      const fhErrorLink = within(errorSummaryContainer).getByRole('link', {
        name: 'Select at least one option from name of course being studied',
      });
      expect(fhErrorLink).toBeInTheDocument();
      // clicking error summary link focuses the fh area
      const fhFieldset = screen.getByText(
        'Browse all name of course being studied',
      );
      expect(fhFieldset.parentElement?.getAttribute('id')).toEqual(
        fhErrorLink.getAttribute('href')?.slice(1),
      );
    });
    test('filter hierarchy search filters the list and emboldeneds the option labels', async () => {
      render(
        <FiltersForm
          {...testWizardStepProps}
          stepTitle="Choose your filters"
          subject={testSubject}
          subjectMeta={subjectMetaWithFilterHierarchy}
          onSubmit={noop}
          showFilterHierarchies
        />,
      );

      // open details
      const hierarchyMenu = screen.getByRole('button', {
        name: 'Name of course being studied (3 tiers)',
      });
      await userEvent.click(hierarchyMenu);

      const optionsContainer = screen.getByTestId('filter-hierarchy-options');
      const options = within(optionsContainer).getAllByRole('checkbox');
      expect(options).toHaveLength(22);

      // type a term into the search
      const hierarchySearch = screen.getByLabelText(
        'Search all tiers and name of course being studied',
      );
      expect(hierarchySearch).toBeInTheDocument();
      await userEvent.type(hierarchySearch, 'engineering');
      await userEvent.click(screen.getByRole('button', { name: 'Search' }));
      expect(
        screen.getByText(
          `Search 'engineering' in all tiers and name of course being studied`,
        ),
      ).toBeInTheDocument();
      const clearSearchButton = screen.getByRole('button', {
        name: 'Clear search',
      });
      expect(clearSearchButton).toBeInTheDocument();

      // see options list has changed
      const filteredOptions = within(optionsContainer).getAllByRole('checkbox');
      expect(filteredOptions).toHaveLength(4);
      expect(filteredOptions.length).not.toEqual(options.length);

      const entryLevelOption =
        within(optionsContainer).getByLabelText('Entry level');
      expect(filteredOptions).toContain(entryLevelOption);
      const engineeringOption =
        within(optionsContainer).getByLabelText('Engineering');
      expect(filteredOptions).toContain(engineeringOption);
      const civilOption =
        within(optionsContainer).getByLabelText('Civil engineering');

      expect(filteredOptions).toContain(civilOption);
      const electricalOption = within(optionsContainer).getByLabelText(
        'Electrical engineering',
      );
      expect(filteredOptions).toContain(electricalOption);

      // see options contain search term or children with search term
      const entryLevelOptionContainer = within(optionsContainer).getByTestId(
        `filter-hierarchy-options-option-ae8fc558`,
      );
      expect(entryLevelOptionContainer).toContainElement(engineeringOption);
      expect(entryLevelOptionContainer).toContainElement(civilOption);
      expect(entryLevelOptionContainer).toContainElement(electricalOption);

      const searchHighlights =
        within(optionsContainer).getAllByTestId('search-highlight');
      expect(searchHighlights).toHaveLength(3);

      // see labels with search term contain the matching term text in bold
      searchHighlights.forEach(highlight =>
        expect(highlight.nodeName).toEqual('STRONG'),
      );

      expect(within(optionsContainer).getAllByRole('checkbox')).toHaveLength(4);
      // clear search
      await userEvent.click(clearSearchButton);
      expect(within(optionsContainer).getAllByRole('checkbox')).toHaveLength(
        22,
      );
    });
    test('filter hierarchy clearing search resets the UI', async () => {
      render(
        <FiltersForm
          {...testWizardStepProps}
          stepTitle="Choose your filters"
          subject={testSubject}
          subjectMeta={subjectMetaWithFilterHierarchy}
          onSubmit={noop}
          showFilterHierarchies
        />,
      );

      // open details
      const hierarchyMenu = screen.getByRole('button', {
        name: 'Name of course being studied (3 tiers)',
      });
      await userEvent.click(hierarchyMenu);

      const optionsContainer = screen.getByTestId('filter-hierarchy-options');
      expect(within(optionsContainer).getAllByRole('checkbox')).toHaveLength(
        22,
      );

      // type a term into the search
      const hierarchySearch = screen.getByLabelText(
        'Search all tiers and name of course being studied',
      );
      await userEvent.type(hierarchySearch, 'bricklaying');
      await userEvent.click(screen.getByRole('button', { name: 'Search' }));

      const searchDescription = screen.getByText(
        `Search 'bricklaying' in all tiers and name of course being studied`,
      );
      expect(searchDescription).toBeInTheDocument();

      // see options list has changed
      expect(within(optionsContainer).getAllByRole('checkbox')).toHaveLength(
        10,
      );

      // submit empty search term

      await userEvent.clear(hierarchySearch);
      await userEvent.type(hierarchySearch, ' ');
      await userEvent.click(screen.getByRole('button', { name: 'Search' }));

      // expect UI to be resetted
      expect(searchDescription).toHaveTextContent(
        'Browse all name of course being studied',
      );
      expect(within(optionsContainer).getAllByRole('checkbox')).toHaveLength(
        22,
      );

      // type a term into the search
      await userEvent.type(hierarchySearch, 'engineering');
      await userEvent.click(screen.getByRole('button', { name: 'Search' }));

      // see options list has changed
      expect(searchDescription).toHaveTextContent(
        `Search 'engineering' in all tiers and name of course being studied`,
      );
      expect(within(optionsContainer).getAllByRole('checkbox')).toHaveLength(4);

      // click clear search
      const clearSearchButton = screen.getByRole('button', {
        name: 'Clear search',
      });
      await userEvent.click(clearSearchButton);

      // expect UI to be resetted
      expect(searchDescription).toHaveTextContent(
        'Browse all name of course being studied',
      );
      expect(within(optionsContainer).getAllByRole('checkbox')).toHaveLength(
        22,
      );
    });
  });
});
