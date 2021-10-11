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

    const countryGroup = screen.getByRole('group', {
      name: 'Country',
      hidden: true,
    });
    const countryCheckboxes = within(countryGroup).getAllByRole('checkbox', {
      hidden: true,
    });
    expect(countryCheckboxes).toHaveLength(1);
    expect(countryCheckboxes[0]).toHaveAttribute('value', 'country-1');
    expect(countryCheckboxes[0]).toEqual(
      within(countryGroup).getByLabelText('Country 1'),
    );
    expect(countryCheckboxes[0]).not.toBeChecked();

    const localAuthorityGroup = screen.getByRole('group', {
      name: 'Local authority',
      hidden: true,
    });
    const localAuthorityCheckboxes = within(localAuthorityGroup).getAllByRole(
      'checkbox',
      {
        hidden: true,
      },
    );
    expect(localAuthorityCheckboxes).toHaveLength(3);
    expect(localAuthorityCheckboxes[0]).toHaveAttribute(
      'value',
      'local-authority-1',
    );
    expect(localAuthorityCheckboxes[0]).toEqual(
      within(localAuthorityGroup).getByLabelText('Local authority 1'),
    );
    expect(localAuthorityCheckboxes[0]).not.toBeChecked();
    expect(localAuthorityCheckboxes[1]).toHaveAttribute(
      'value',
      'local-authority-2',
    );
    expect(localAuthorityCheckboxes[1]).toEqual(
      within(localAuthorityGroup).getByLabelText('Local authority 2'),
    );
    expect(localAuthorityCheckboxes[1]).not.toBeChecked();
    expect(localAuthorityCheckboxes[2]).toHaveAttribute(
      'value',
      'local-authority-3',
    );
    expect(localAuthorityCheckboxes[2]).toEqual(
      within(localAuthorityGroup).getByLabelText('Local authority 3'),
    );
    expect(localAuthorityCheckboxes[2]).not.toBeChecked();

    const regionGroup = screen.getByRole('group', {
      name: 'Region',
      hidden: true,
    });
    const regionCheckboxes = within(regionGroup).getAllByRole('checkbox', {
      hidden: true,
    });
    expect(regionCheckboxes).toHaveLength(2);
    expect(regionCheckboxes[0]).toHaveAttribute('value', 'region-1');
    expect(regionCheckboxes[0]).toEqual(
      within(regionGroup).getByLabelText('Region 1'),
    );
    expect(regionCheckboxes[0]).not.toBeChecked();
    expect(regionCheckboxes[1]).toHaveAttribute('value', 'region-2');
    expect(regionCheckboxes[1]).toEqual(
      within(regionGroup).getByLabelText('Region 2'),
    );
    expect(regionCheckboxes[1]).not.toBeChecked();
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

    expect(screen.getByLabelText('Local authority 1')).toBeChecked();
    expect(screen.getByLabelText('Local authority 3')).toBeChecked();
    expect(screen.getByLabelText('Region 2')).toBeChecked();

    expect(screen.getByLabelText('Country 1')).not.toBeChecked();
    expect(screen.getByLabelText('Local authority 2')).not.toBeChecked();
    expect(screen.getByLabelText('Region 1')).not.toBeChecked();
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

    expect(screen.getByLabelText('Country 1')).toBeChecked();
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
