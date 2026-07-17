import TimePeriodForm from '@common/modules/table-tool/components/TimePeriodForm';
import { InjectedWizardProps } from '@common/modules/table-tool/components/Wizard';
import { SubjectMeta } from '@common/services/tableBuilderService';
import { waitFor } from '@testing-library/dom';
import { render, screen, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import noop from 'lodash/noop';
import React from 'react';

describe('TimePeriodForm', () => {
  const testTimePeriodOptions: SubjectMeta['timePeriod']['options'] = [
    {
      year: 2018,
      code: 'AY',
      label: '2018/19',
    },
    {
      year: 2019,
      code: 'AY',
      label: '2019/20',
    },
    {
      year: 2020,
      code: 'AY',
      label: '2020/21',
    },
    {
      year: 2021,
      code: 'AY',
      label: '2021/22',
    },
  ];

  const singleTimePeriodOption: SubjectMeta['timePeriod']['options'] = [
    {
      year: 2020,
      code: 'AY',
      label: '2020/21',
    },
  ];

  const wizardProps: InjectedWizardProps = {
    shouldScroll: true,
    stepNumber: 1,
    currentStep: 1,
    isActive: true,
    isEnabled: true,
    isLoading: false,
    setCurrentStep: (step, task) => task?.(),
    goToNextStep: task => task?.(),
    goToPreviousStep: task => task?.(),
  };

  describe('with multiple time period options', () => {
    test('renders the form with start and end date selects when active', () => {
      render(
        <TimePeriodForm
          {...wizardProps}
          stepTitle="Select time period"
          options={testTimePeriodOptions}
          onSubmit={noop}
        />,
      );

      expect(
        screen.getByRole('heading', {
          name: 'Step 1 Select time period',
        }),
      ).toBeInTheDocument();

      expect(
        screen.getByLabelText('Start date', {
          selector: 'select',
        }),
      ).toBeInTheDocument();

      expect(
        screen.getByLabelText('End date', {
          selector: 'select',
        }),
      ).toBeInTheDocument();

      expect(
        screen.getByRole('button', {
          name: 'Next step',
        }),
      ).toBeInTheDocument();
    });

    test('pre-selects the earliest option for start date and latest for end date', () => {
      render(
        <TimePeriodForm
          {...wizardProps}
          stepTitle="Select time period"
          options={testTimePeriodOptions}
          onSubmit={noop}
        />,
      );

      const startSelect = screen.getByLabelText('Start date', {
        selector: 'select',
      }) as HTMLSelectElement;
      const endSelect = screen.getByLabelText('End date', {
        selector: 'select',
      }) as HTMLSelectElement;

      expect(startSelect.value).toBe('2018_AY');
      expect(endSelect.value).toBe('2021_AY');
    });

    test('displays all time period options in both selects', () => {
      render(
        <TimePeriodForm
          {...wizardProps}
          stepTitle="Select time period"
          options={testTimePeriodOptions}
          onSubmit={noop}
        />,
      );

      const startSelect = screen.getByLabelText('Start date', {
        selector: 'select',
      });
      const endSelect = screen.getByLabelText('End date', {
        selector: 'select',
      });

      const startOptions = within(startSelect as HTMLElement).getAllByRole(
        'option',
      );
      const endOptions = within(endSelect as HTMLElement).getAllByRole(
        'option',
      );

      // 1 placeholder + 4 time periods
      expect(startOptions).toHaveLength(5);
      expect(endOptions).toHaveLength(5);

      expect(startOptions[1]).toHaveTextContent('2018/19');
      expect(startOptions[2]).toHaveTextContent('2019/20');
      expect(startOptions[3]).toHaveTextContent('2020/21');
      expect(startOptions[4]).toHaveTextContent('2021/22');
    });

    test('allows changing start and end date selections', async () => {
      render(
        <TimePeriodForm
          {...wizardProps}
          stepTitle="Select time period"
          options={testTimePeriodOptions}
          onSubmit={noop}
        />,
      );

      const startSelect = screen.getByLabelText('Start date', {
        selector: 'select',
      });
      const endSelect = screen.getByLabelText('End date', {
        selector: 'select',
      });

      await userEvent.selectOptions(startSelect, '2019_AY');
      await userEvent.selectOptions(endSelect, '2020_AY');

      expect((startSelect as HTMLSelectElement).value).toBe('2019_AY');
      expect((endSelect as HTMLSelectElement).value).toBe('2020_AY');
    });

    test('calls onSubmit with correct values when form is submitted', async () => {
      const handleSubmit = jest.fn();

      render(
        <TimePeriodForm
          {...wizardProps}
          stepTitle="Select time period"
          options={testTimePeriodOptions}
          onSubmit={handleSubmit}
        />,
      );

      const startSelect = screen.getByLabelText('Start date', {
        selector: 'select',
      });
      const endSelect = screen.getByLabelText('End date', {
        selector: 'select',
      });

      await userEvent.selectOptions(startSelect, '2019_AY');
      await userEvent.selectOptions(endSelect, '2020_AY');

      await userEvent.click(
        screen.getByRole('button', {
          name: 'Next step',
        }),
      );

      await waitFor(() => {
        expect(handleSubmit).toHaveBeenCalledWith({
          start: '2019_AY',
          end: '2020_AY',
        });
      });
    });

    test('shows error when start date is not selected', async () => {
      render(
        <TimePeriodForm
          {...wizardProps}
          stepTitle="Select time period"
          options={testTimePeriodOptions}
          onSubmit={noop}
        />,
      );

      const startSelect = screen.getByLabelText('Start date', {
        selector: 'select',
      });

      await userEvent.selectOptions(startSelect, '');

      await userEvent.click(
        screen.getByRole('button', {
          name: 'Next step',
        }),
      );

      await waitFor(() => {
        expect(
          screen.getByTestId('timePeriodForm-start-error'),
        ).toHaveTextContent('Start date required');
      });
    });

    test('shows error when end date is not selected', async () => {
      render(
        <TimePeriodForm
          {...wizardProps}
          stepTitle="Select time period"
          options={testTimePeriodOptions}
          onSubmit={noop}
        />,
      );

      const endSelect = screen.getByLabelText('End date', {
        selector: 'select',
      });

      await userEvent.selectOptions(endSelect, '');

      await userEvent.click(
        screen.getByRole('button', {
          name: 'Next step',
        }),
      );

      await waitFor(() => {
        expect(
          screen.getByTestId('timePeriodForm-end-error'),
        ).toHaveTextContent('End date required');
      });
    });

    test('shows error when start date is after end date', async () => {
      render(
        <TimePeriodForm
          {...wizardProps}
          stepTitle="Select time period"
          options={testTimePeriodOptions}
          onSubmit={noop}
        />,
      );

      const startSelect = screen.getByLabelText('Start date', {
        selector: 'select',
      });
      const endSelect = screen.getByLabelText('End date', {
        selector: 'select',
      });

      await userEvent.selectOptions(startSelect, '2021_AY');
      await userEvent.selectOptions(endSelect, '2019_AY');

      await userEvent.click(
        screen.getByRole('button', {
          name: 'Next step',
        }),
      );

      await waitFor(() => {
        expect(
          screen.getByTestId('timePeriodForm-start-error'),
        ).toHaveTextContent('Start date must be before or same as end date');
      });
    });

    test('shows error when end date is before start date', async () => {
      render(
        <TimePeriodForm
          {...wizardProps}
          stepTitle="Select time period"
          options={testTimePeriodOptions}
          onSubmit={noop}
        />,
      );

      const startSelect = screen.getByLabelText('Start date', {
        selector: 'select',
      });
      const endSelect = screen.getByLabelText('End date', {
        selector: 'select',
      });

      await userEvent.selectOptions(startSelect, '2020_AY');
      await userEvent.selectOptions(endSelect, '2018_AY');

      await userEvent.click(
        screen.getByRole('button', {
          name: 'Next step',
        }),
      );

      await waitFor(() => {
        expect(
          screen.getByTestId('timePeriodForm-end-error'),
        ).toHaveTextContent('End date must be after or same as start date');
      });
    });

    test('allows start and end date to be the same', async () => {
      const handleSubmit = jest.fn();

      render(
        <TimePeriodForm
          {...wizardProps}
          stepTitle="Select time period"
          options={testTimePeriodOptions}
          onSubmit={handleSubmit}
        />,
      );

      const startSelect = screen.getByLabelText('Start date', {
        selector: 'select',
      });
      const endSelect = screen.getByLabelText('End date', {
        selector: 'select',
      });

      await userEvent.selectOptions(startSelect, '2019_AY');
      await userEvent.selectOptions(endSelect, '2019_AY');

      await userEvent.click(
        screen.getByRole('button', {
          name: 'Next step',
        }),
      );

      await waitFor(() => {
        expect(handleSubmit).toHaveBeenCalledWith({
          start: '2019_AY',
          end: '2019_AY',
        });
      });
    });
  });

  describe('with single time period option', () => {
    test('pre-selects the same start and end date', () => {
      render(
        <TimePeriodForm
          {...wizardProps}
          stepTitle="Select time period"
          options={singleTimePeriodOption}
          onSubmit={noop}
        />,
      );

      const startSelect = screen.getByLabelText('Start date', {
        selector: 'select',
      }) as HTMLSelectElement;
      const endSelect = screen.getByLabelText('End date', {
        selector: 'select',
      }) as HTMLSelectElement;

      expect(startSelect.value).toBe('2020_AY');
      expect(endSelect.value).toBe('2020_AY');
    });

    test('shows hint that only one time period is available', () => {
      render(
        <TimePeriodForm
          {...wizardProps}
          stepTitle="Select time period"
          options={singleTimePeriodOption}
          onSubmit={noop}
        />,
      );

      expect(
        screen.getByText('Only one time period available.'),
      ).toBeInTheDocument();
    });

    test('submits with matching start and end dates', async () => {
      const handleSubmit = jest.fn();

      render(
        <TimePeriodForm
          {...wizardProps}
          stepTitle="Select time period"
          options={singleTimePeriodOption}
          onSubmit={handleSubmit}
        />,
      );

      await userEvent.click(
        screen.getByRole('button', {
          name: 'Next step',
        }),
      );

      await waitFor(() => {
        expect(handleSubmit).toHaveBeenCalledWith({
          start: '2020_AY',
          end: '2020_AY',
        });
      });
    });
  });

  describe('with pre-existing selection', () => {
    test('maintains pre-existing selection for start and end dates', () => {
      render(
        <TimePeriodForm
          {...wizardProps}
          stepTitle="Select time period"
          options={testTimePeriodOptions}
          initialValues={{
            startYear: 2019,
            startCode: 'AY',
            endYear: 2020,
            endCode: 'AY',
          }}
          onSubmit={noop}
        />,
      );

      const startSelect = screen.getByLabelText('Start date', {
        selector: 'select',
      }) as HTMLSelectElement;
      const endSelect = screen.getByLabelText('End date', {
        selector: 'select',
      }) as HTMLSelectElement;

      expect(startSelect.value).toBe('2019_AY');
      expect(endSelect.value).toBe('2020_AY');
    });

    test('displays pre-existing selection in summary view when not active', () => {
      render(
        <TimePeriodForm
          {...wizardProps}
          isActive={false}
          stepTitle="Select time period"
          options={testTimePeriodOptions}
          initialValues={{
            startYear: 2019,
            startCode: 'AY',
            endYear: 2020,
            endCode: 'AY',
          }}
          onSubmit={noop}
        />,
      );

      expect(
        screen.getByText('2019/20 to 2020/21', {
          exact: false,
        }),
      ).toBeInTheDocument();
    });

    test('allows changing pre-existing selection', async () => {
      render(
        <TimePeriodForm
          {...wizardProps}
          stepTitle="Select time period"
          options={testTimePeriodOptions}
          initialValues={{
            startYear: 2019,
            startCode: 'AY',
            endYear: 2020,
            endCode: 'AY',
          }}
          onSubmit={noop}
        />,
      );

      const startSelect = screen.getByLabelText('Start date', {
        selector: 'select',
      });
      const endSelect = screen.getByLabelText('End date', {
        selector: 'select',
      });

      await userEvent.selectOptions(startSelect, '2018_AY');
      await userEvent.selectOptions(endSelect, '2021_AY');

      expect((startSelect as HTMLSelectElement).value).toBe('2018_AY');
      expect((endSelect as HTMLSelectElement).value).toBe('2021_AY');
    });

    test('submits with updated values when pre-existing selection is changed', async () => {
      const handleSubmit = jest.fn();

      render(
        <TimePeriodForm
          {...wizardProps}
          stepTitle="Select time period"
          options={testTimePeriodOptions}
          initialValues={{
            startYear: 2019,
            startCode: 'AY',
            endYear: 2020,
            endCode: 'AY',
          }}
          onSubmit={handleSubmit}
        />,
      );

      const startSelect = screen.getByLabelText('Start date', {
        selector: 'select',
      });
      const endSelect = screen.getByLabelText('End date', {
        selector: 'select',
      });

      await userEvent.selectOptions(startSelect, '2018_AY');
      await userEvent.selectOptions(endSelect, '2021_AY');

      await userEvent.click(
        screen.getByRole('button', {
          name: 'Next step',
        }),
      );

      await waitFor(() => {
        expect(handleSubmit).toHaveBeenCalledWith({
          start: '2018_AY',
          end: '2021_AY',
        });
      });
    });
  });

  describe('read-only summary view', () => {
    test('renders summary view when step is not active', () => {
      render(
        <TimePeriodForm
          {...wizardProps}
          isActive={false}
          stepTitle="Select time period"
          options={testTimePeriodOptions}
          initialValues={{
            startYear: 2019,
            startCode: 'AY',
            endYear: 2020,
            endCode: 'AY',
          }}
          onSubmit={noop}
        />,
      );

      expect(
        screen.queryByRole('button', {
          name: 'Next step',
        }),
      ).not.toBeInTheDocument();

      expect(
        screen.queryByLabelText('Start date', {
          selector: 'select',
        }),
      ).not.toBeInTheDocument();

      expect(
        screen.queryByLabelText('End date', {
          selector: 'select',
        }),
      ).not.toBeInTheDocument();
    });

    test('displays time period range in summary when start and end dates are different', () => {
      render(
        <TimePeriodForm
          {...wizardProps}
          isActive={false}
          stepTitle="Select time period"
          options={testTimePeriodOptions}
          initialValues={{
            startYear: 2018,
            startCode: 'AY',
            endYear: 2021,
            endCode: 'AY',
          }}
          onSubmit={noop}
        />,
      );

      expect(screen.getByText('2018/19 to 2021/22')).toBeInTheDocument();
    });

    test('displays single time period in summary when start and end dates are the same', () => {
      render(
        <TimePeriodForm
          {...wizardProps}
          isActive={false}
          stepTitle="Select time period"
          options={testTimePeriodOptions}
          initialValues={{
            startYear: 2019,
            startCode: 'AY',
            endYear: 2019,
            endCode: 'AY',
          }}
          onSubmit={noop}
        />,
      );

      expect(screen.getByText('2019/20')).toBeInTheDocument();
      expect(screen.queryByText(/to/)).not.toBeInTheDocument();
    });

    test('provides button to edit time period', () => {
      render(
        <TimePeriodForm
          {...wizardProps}
          isActive={false}
          stepTitle="Select time period"
          options={testTimePeriodOptions}
          initialValues={{
            startYear: 2019,
            startCode: 'AY',
            endYear: 2020,
            endCode: 'AY',
          }}
          onSubmit={noop}
        />,
      );

      expect(
        screen.getByRole('button', {
          name: /Edit time period/,
        }),
      ).toBeInTheDocument();
    });
  });

  describe('form re-initialization', () => {
    test('updates form when initialValues prop changes', async () => {
      const { rerender } = render(
        <TimePeriodForm
          {...wizardProps}
          stepTitle="Select time period"
          options={testTimePeriodOptions}
          initialValues={{
            startYear: 2018,
            startCode: 'AY',
            endYear: 2019,
            endCode: 'AY',
          }}
          onSubmit={noop}
        />,
      );

      let startSelect = screen.getByLabelText('Start date', {
        selector: 'select',
      }) as HTMLSelectElement;
      let endSelect = screen.getByLabelText('End date', {
        selector: 'select',
      }) as HTMLSelectElement;

      expect(startSelect.value).toBe('2018_AY');
      expect(endSelect.value).toBe('2019_AY');

      rerender(
        <TimePeriodForm
          {...wizardProps}
          stepTitle="Select time period"
          options={testTimePeriodOptions}
          initialValues={{
            startYear: 2020,
            startCode: 'AY',
            endYear: 2021,
            endCode: 'AY',
          }}
          onSubmit={noop}
        />,
      );

      startSelect = screen.getByLabelText('Start date', {
        selector: 'select',
      }) as HTMLSelectElement;
      endSelect = screen.getByLabelText('End date', {
        selector: 'select',
      }) as HTMLSelectElement;

      expect(startSelect.value).toBe('2020_AY');
      expect(endSelect.value).toBe('2021_AY');
    });
  });

  describe('wizard step integration', () => {
    test('does not render form when step is not active', () => {
      render(
        <TimePeriodForm
          {...wizardProps}
          isActive={false}
          stepTitle="Select time period"
          options={testTimePeriodOptions}
          onSubmit={noop}
        />,
      );

      expect(
        screen.queryByLabelText('Start date', {
          selector: 'select',
        }),
      ).not.toBeInTheDocument();

      expect(
        screen.queryByLabelText('End date', {
          selector: 'select',
        }),
      ).not.toBeInTheDocument();

      expect(
        screen.queryByRole('button', {
          name: 'Next step',
        }),
      ).not.toBeInTheDocument();
    });

    test('renders form when step is active', () => {
      render(
        <TimePeriodForm
          {...wizardProps}
          isActive
          stepTitle="Select time period"
          options={testTimePeriodOptions}
          onSubmit={noop}
        />,
      );

      expect(
        screen.getByLabelText('Start date', {
          selector: 'select',
        }),
      ).toBeInTheDocument();

      expect(
        screen.getByLabelText('End date', {
          selector: 'select',
        }),
      ).toBeInTheDocument();

      expect(
        screen.getByRole('button', {
          name: 'Next step',
        }),
      ).toBeInTheDocument();
    });
  });
});
