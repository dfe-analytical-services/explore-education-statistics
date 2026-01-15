import { createServerValidationErrorMock } from '@common-test/createAxiosErrorMock';
import render from '@common-test/render';
import FiltersForm, {
  TableQueryErrorCode,
} from '@common/modules/table-tool/components/FiltersForm';
import { testWizardStepProps } from '@common/modules/table-tool/components/__tests__/__data__/testWizardStepProps';
import {
  testSubject,
  testSubjectMeta,
  testSubjectMetaOneIndicator,
  testSubjectMetaSingleFilters,
  testSubjectMetaWithFilterHierarchy,
} from '@common/modules/table-tool/components/__tests__/__data__/testFiltersData.data';
import { screen, within, waitFor } from '@testing-library/react';
import { produce } from 'immer';
import noop from 'lodash/noop';
import React from 'react';

describe('FiltersForm', () => {
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

    const schoolSection = within(
      screen.getByTestId('filters-SchoolType-accordion'),
    );
    expect(schoolSection.getByText('1 selected')).toBeInTheDocument();

    const filterGroup1 = screen.getByRole('group', {
      name: 'School type',
    });
    const filterCheckboxes1 = within(filterGroup1).getAllByRole('checkbox');
    expect(filterCheckboxes1).toHaveLength(1);
    expect(filterCheckboxes1[0]).toEqual(
      within(filterGroup1).getByLabelText('State-funded secondary'),
    );
    expect(filterCheckboxes1[0]).toBeChecked();

    const characteristicSection = within(
      screen.getByTestId('filters-Characteristic-accordion'),
    );
    expect(characteristicSection.getByText('1 selected')).toBeInTheDocument();

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
        name: 'Filter With Multiple Options - show options',
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
    const { user } = render(
      <FiltersForm
        {...testWizardStepProps}
        stepTitle="Choose your filters"
        subject={testSubject}
        subjectMeta={testSubjectMeta}
        onSubmit={noop}
      />,
    );

    await user.click(screen.getByLabelText('State-funded secondary'));

    const characteristicSection = within(
      screen.getByTestId('filters-Characteristic-accordion'),
    );
    const schoolSection = within(
      screen.getByTestId('filters-SchoolType-accordion'),
    );
    expect(schoolSection.getByText('1 selected')).toBeInTheDocument();

    await user.click(screen.getByLabelText('Ethnicity Major Mixed Total'));
    await user.click(screen.getByLabelText('Gender male'));
    await user.click(screen.getByLabelText('Total'));

    expect(characteristicSection.getByText('3 selected')).toBeInTheDocument();

    await user.click(screen.getByLabelText('Number of excluded sessions'));
    await user.click(screen.getByLabelText('Unauthorised absence rate'));

    expect(
      screen.getByRole('group', {
        name: 'Indicators - 2 selected',
      }),
    ).toBeInTheDocument();
  });

  test('shows validation errors if no options are selected from the filter and indicator groups', async () => {
    const { user } = render(
      <FiltersForm
        {...testWizardStepProps}
        stepTitle="Choose your filters"
        subject={testSubject}
        subjectMeta={testSubjectMeta}
        onSubmit={noop}
      />,
    );

    await user.click(
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
          filterHierarchies: {},
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
          filterHierarchies: {},
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
    const { user } = render(
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

    await user.click(screen.getByLabelText('State-funded secondary'));
    await user.click(screen.getByLabelText('Number of excluded sessions'));

    expect((screen.getByLabelText('Total') as HTMLInputElement).checked).toBe(
      false,
    );

    await user.click(
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
    const { container, rerender, user } = render(
      <FiltersForm
        {...testWizardStepProps}
        stepTitle="Choose your filters"
        subject={testSubject}
        subjectMeta={testSubjectMeta}
        onSubmit={noop}
      />,
    );

    await user.click(screen.getByLabelText('State-funded secondary'));
    await user.click(screen.getByLabelText('Ethnicity Major Black Total'));
    await user.click(screen.getByLabelText('Number of excluded sessions'));

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

  test('shows table timeout error when the correct error response is returned from the API', async () => {
    const errorResponse = createServerValidationErrorMock<TableQueryErrorCode>([
      { code: 'RequestCancelled', message: '' },
    ]);
    const onSubmit = jest.fn(() => Promise.reject(errorResponse));

    const { user } = render(
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

    await user.click(screen.getByLabelText('State-funded secondary'));
    await user.click(screen.getByLabelText('Number of excluded sessions'));
    await user.click(screen.getByLabelText('Total'));

    await user.click(screen.getByRole('button', { name: 'Create table' }));

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

    const filters = within(screen.getByRole('group', { name: 'Categories' }));

    const buttons = filters.getAllByRole('button');

    expect(
      screen.getByRole('button', {
        name: 'Characteristic - show options',
      }),
    ).toEqual(buttons[1]);

    expect(
      screen.getByRole('button', {
        name: 'School type - show options',
      }),
    ).toEqual(buttons[5]);

    const characteristicGroups = within(
      screen.getByTestId('filters-Characteristic-accordion'),
    ).getAllByRole('group');

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
      screen.getByRole('button', { name: 'School type - show options' }),
    ).toHaveAttribute('aria-expanded', 'false');
    expect(
      screen.getByRole('button', { name: 'Characteristic - show options' }),
    ).toHaveAttribute('aria-expanded', 'false');
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
    const { user } = render(
      <FiltersForm
        {...testWizardStepProps}
        stepTitle="Choose your filters"
        subject={testSubject}
        subjectMeta={testSubjectMeta}
        onSubmit={noop}
      />,
    );

    expect(
      screen.getByRole('button', { name: 'School type - show options' }),
    ).toHaveAttribute('aria-expanded', 'false');
    expect(
      screen.getByRole('button', { name: 'Characteristic - show options' }),
    ).toHaveAttribute('aria-expanded', 'false');

    await user.click(
      screen.getByRole('button', { name: 'Expand all categories' }),
    );

    expect(
      await screen.findByRole('button', { name: 'Collapse all categories' }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('button', { name: 'School type - hide options' }),
    ).toHaveAttribute('aria-expanded', 'true');
    expect(
      screen.getByRole('button', { name: 'Characteristic - hide options' }),
    ).toHaveAttribute('aria-expanded', 'true');

    await user.click(
      screen.getByRole('button', { name: 'Collapse all categories' }),
    );

    expect(
      await screen.findByRole('button', { name: 'Expand all categories' }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('button', { name: 'School type - show options' }),
    ).toHaveAttribute('aria-expanded', 'false');
    expect(
      screen.getByRole('button', { name: 'Characteristic - show options' }),
    ).toHaveAttribute('aria-expanded', 'false');
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
    test('filter hierarchies form submits with correct values ', async () => {
      const onSubmit = jest.fn();
      const { user } = render(
        <FiltersForm
          {...testWizardStepProps}
          stepTitle="Choose your filters"
          subject={testSubject}
          subjectMeta={testSubjectMetaWithFilterHierarchy}
          onSubmit={onSubmit}
        />,
      );

      // Expand filter
      await user.click(
        screen.getByRole('button', {
          name: 'Name of course being studied (3 tiers) - show options',
        }),
      );

      // Expand entry
      await user.click(
        screen.getByRole('button', {
          name: 'Show sub categories for entry level',
        }),
      );
      // Select some options
      await user.click(
        screen.getByRole('checkbox', {
          name: 'Entry level',
        }),
      );
      await user.click(
        screen.getByRole('checkbox', {
          name: 'Engineering',
        }),
      );

      // Expand engineering
      await user.click(
        screen.getByRole('button', {
          name: 'Show sub categories for engineering',
        }),
      );
      // Select an option
      await user.click(
        screen.getByRole('checkbox', {
          name: 'Civil engineering',
        }),
      );

      // Check count tag updated
      expect(
        within(
          screen.getByTestId('filterHierarchies.filter-3-count'),
        ).getByText('3 selected'),
      ).toBeInTheDocument();

      // Submit form
      expect(onSubmit).toHaveBeenCalledTimes(0);
      await user.click(screen.getByRole('button', { name: 'Create table' }));
      expect(onSubmit).toHaveBeenCalledTimes(1);
      expect(onSubmit).toHaveBeenCalledWith({
        filterHierarchies: {
          'filter-3': [
            'option-1,option-4,option-11',
            'option-1,option-2,option-11',
            'option-1,option-2,option-10',
          ],
        },
        filters: {},
        indicators: ['indicator-1'],
      });
    });

    test('filter hierarchy validation errors show and function', async () => {
      const { user } = render(
        <FiltersForm
          {...testWizardStepProps}
          stepTitle="Choose your filters"
          subject={testSubject}
          subjectMeta={testSubjectMetaWithFilterHierarchy}
          onSubmit={jest.fn}
        />,
      );
      // attempt to submit no filter hierarchy selectiosn
      await user.click(screen.getByRole('button', { name: 'Create table' }));
      // validation error kicks in
      const errorSummaryContainer = screen.getByTestId('errorSummary');
      expect(errorSummaryContainer).toBeInTheDocument();
      const fhErrorLink = within(errorSummaryContainer).getByRole('link', {
        name: 'Select at least one option from name of course being studied',
      });
      expect(fhErrorLink).toBeInTheDocument();
      // clicking error summary link focuses the fh area
      const fhFieldset = screen.getByText(
        'Browse all tiers of name of course being studied',
      );
      expect(fhFieldset.parentElement?.getAttribute('id')).toEqual(
        fhErrorLink.getAttribute('href')?.slice(1),
      );
    });
  });
});
