import LocationFiltersForm, {
  LocationFormValues,
} from '@common/modules/table-tool/components/LocationFiltersForm';
import { InjectedWizardProps } from '@common/modules/table-tool/components/Wizard';
import { SubjectMeta } from '@common/services/tableBuilderService';
import { waitFor } from '@testing-library/dom';
import { render, screen, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import noop from 'lodash/noop';
import React from 'react';

describe('LocationFiltersForm', () => {
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

  const testLocations: SubjectMeta['locations'] = {
    Country: {
      hint: '',
      legend: 'Country',
      options: [
        {
          label: 'Country 1',
          value: 'country-1',
        },
      ],
    },
    LocalAuthority: {
      hint: '',
      legend: 'Local authority',
      options: [
        {
          label: 'Local authority 1',
          value: 'local-authority-1',
        },
        {
          label: 'Local authority 2',
          value: 'local-authority-2',
        },
        {
          label: 'Local authority 3',
          value: 'local-authority-3',
        },
      ],
    },
    Region: {
      hint: '',
      legend: 'Region',
      options: [
        {
          label: 'Region 1',
          value: 'region-1',
        },
        {
          label: 'Region 2',
          value: 'region-2',
        },
      ],
    },
  };

  const testSingleLocation: SubjectMeta['locations'] = {
    Country: {
      hint: '',
      legend: 'Country',
      options: [
        {
          label: 'Country 1',
          value: 'country-1',
        },
      ],
    },
  };

  test('renders location group options correctly', () => {
    render(
      <LocationFiltersForm
        {...testWizardStepProps}
        options={testLocations}
        onSubmit={noop}
      />,
    );

    expect(
      within(
        screen.getByRole('group', {
          name: 'Country',
          hidden: true,
        }),
      ).getAllByRole('checkbox', {
        hidden: true,
      }),
    ).toHaveLength(1);

    expect(
      within(
        screen.getByRole('group', {
          name: 'Local authority',
          hidden: true,
        }),
      ).getAllByRole('checkbox', {
        hidden: true,
      }),
    ).toHaveLength(3);

    expect(
      within(
        screen.getByRole('group', {
          name: 'Region',
          hidden: true,
        }),
      ).getAllByRole('checkbox', {
        hidden: true,
      }),
    ).toHaveLength(2);
  });

  test('selecting options shows the number of selected options for each location group', () => {
    render(
      <LocationFiltersForm
        {...testWizardStepProps}
        options={testLocations}
        onSubmit={noop}
      />,
    );

    userEvent.click(screen.getByLabelText('Local authority 2'));

    expect(
      screen.getByRole('button', {
        name: 'Local authority - 1 selected',
      }),
    ).toBeInTheDocument();

    userEvent.click(screen.getByLabelText('Local authority 3'));
    userEvent.click(screen.getByLabelText('Country 1'));
    userEvent.click(screen.getByLabelText('Region 2'));

    expect(
      screen.getByRole('button', {
        name: 'Local authority - 2 selected',
      }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('button', {
        name: 'Country - 1 selected',
      }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('button', {
        name: 'Region - 1 selected',
      }),
    ).toBeInTheDocument();
  });

  test('shows validation errors if no location is selected', async () => {
    render(
      <LocationFiltersForm
        {...testWizardStepProps}
        options={testLocations}
        onSubmit={noop}
      />,
    );

    userEvent.click(
      screen.getByRole('button', {
        name: 'Next step',
      }),
    );

    await waitFor(() => {
      expect(
        screen.getByRole('link', {
          name: 'Select at least one location',
        }),
      ).toBeInTheDocument();
    });
  });

  test('only the correct checkboxes are selected from initial values', () => {
    render(
      <LocationFiltersForm
        {...testWizardStepProps}
        options={testLocations}
        onSubmit={noop}
        initialValues={{
          LocalAuthority: ['local-authority-1', 'local-authority-3'],
          Region: ['region-2'],
        }}
      />,
    );

    expect(screen.getByLabelText('Local authority 1')).toHaveAttribute(
      'checked',
    );
    expect(screen.getByLabelText('Local authority 3')).toHaveAttribute(
      'checked',
    );
    expect(screen.getByLabelText('Region 2')).toHaveAttribute('checked');

    expect(screen.getByLabelText('Country 1')).not.toHaveAttribute('checked');
    expect(screen.getByLabelText('Local authority 2')).not.toHaveAttribute(
      'checked',
    );
    expect(screen.getByLabelText('Region 1')).not.toHaveAttribute('checked');
  });

  test('automatically selects the option and expands the group if only one location available', () => {
    render(
      <LocationFiltersForm
        {...testWizardStepProps}
        options={testSingleLocation}
        onSubmit={noop}
      />,
    );

    expect(
      screen.getByRole('button', {
        name: 'Country - 1 selected',
      }),
    ).toBeInTheDocument();

    expect(
      within(
        screen.getByRole('group', {
          name: 'Country',
        }),
      ).getAllByRole('checkbox'),
    ).toHaveLength(1);

    expect(screen.getByLabelText('Country 1')).toBeVisible();

    expect(screen.getByLabelText('Country 1')).toHaveAttribute('checked');
  });

  test('renders a read-only view of selected options when no longer the current step', async () => {
    const { container, rerender } = render(
      <LocationFiltersForm
        {...testWizardStepProps}
        options={testLocations}
        onSubmit={noop}
      />,
    );

    userEvent.click(screen.getByLabelText('Region 1'));
    userEvent.click(screen.getByLabelText('Region 2'));
    userEvent.click(screen.getByLabelText('Country 1'));

    await rerender(
      <LocationFiltersForm
        {...testWizardStepProps}
        isActive={false}
        options={testLocations}
        onSubmit={noop}
      />,
    );

    expect(screen.queryAllByRole('checkbox')).toHaveLength(0);
    expect(container.querySelector('dl')).toMatchSnapshot();
  });

  test('clicking `Next step` calls `onSubmit` with correct values', async () => {
    const handleSubmit = jest.fn();
    render(
      <LocationFiltersForm
        {...testWizardStepProps}
        options={testLocations}
        onSubmit={handleSubmit}
      />,
    );

    userEvent.click(screen.getByLabelText('Local authority 3'));
    userEvent.click(screen.getByLabelText('Region 1'));

    userEvent.click(screen.getByRole('button', { name: 'Next step' }));

    const expected: LocationFormValues = {
      locations: {
        LocalAuthority: ['local-authority-3'],
        Region: ['region-1'],
      },
    };

    await waitFor(() => {
      expect(handleSubmit).toHaveBeenCalledTimes(1);
      expect(handleSubmit).toHaveBeenCalledWith(expected);
    });
  });
});
