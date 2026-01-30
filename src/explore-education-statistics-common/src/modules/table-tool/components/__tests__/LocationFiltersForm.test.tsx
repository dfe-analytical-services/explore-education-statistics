import LocationFiltersForm from '@common/modules/table-tool/components/LocationFiltersForm';
import { SubjectMeta } from '@common/services/tableBuilderService';
import {
  testLocationsFlat,
  testLocationsNested,
  testSchools,
} from '@common/modules/table-tool/components/__tests__/__data__/testLocationFilters.data';
import { testWizardStepProps } from '@common/modules/table-tool/components/__tests__/__data__/testWizardStepProps';
import { waitFor } from '@testing-library/dom';
import { render, screen, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import noop from 'lodash/noop';
import React from 'react';

describe('LocationFiltersForm', () => {
  test('renders flat location group options correctly', async () => {
    render(
      <LocationFiltersForm
        {...testWizardStepProps}
        stepHeading="Choose locations"
        options={testLocationsFlat}
        onSubmit={noop}
      />,
    );

    const countryGroup = await screen.findByRole('group', {
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

    const localAuthorityGroup = await screen.findByRole('group', {
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

    const regionGroup = await screen.findByRole('group', {
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

  test('renders nested location group options correctly', async () => {
    render(
      <LocationFiltersForm
        {...testWizardStepProps}
        stepHeading="Choose locations"
        options={testLocationsNested}
        onSubmit={noop}
      />,
    );

    const countryGroup = await screen.findByRole('group', {
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

    const localAuthorityGroup = within(
      await screen.findByRole('group', {
        name: 'Local authority',
        hidden: true,
      }),
    );

    const region1Group = within(
      localAuthorityGroup.getByRole('group', {
        name: 'Region 1',
        hidden: true,
      }),
    );

    const region1Checkboxes = region1Group.getAllByRole('checkbox', {
      hidden: true,
    });
    expect(region1Checkboxes).toHaveLength(2);
    expect(region1Checkboxes[0]).toHaveAttribute('value', 'local-authority-1');
    expect(region1Checkboxes[0]).toEqual(
      region1Group.getByLabelText('Local authority 1'),
    );
    expect(region1Checkboxes[0]).not.toBeChecked();

    expect(region1Checkboxes[1]).toHaveAttribute('value', 'local-authority-2');
    expect(region1Checkboxes[1]).toEqual(
      region1Group.getByLabelText('Local authority 2'),
    );
    expect(region1Checkboxes[1]).not.toBeChecked();

    const region2Group = within(
      localAuthorityGroup.getByRole('group', {
        name: 'Region 2',
        hidden: true,
      }),
    );

    const region2Checkboxes = region2Group.getAllByRole('checkbox', {
      hidden: true,
    });
    expect(region2Checkboxes).toHaveLength(2);
    expect(region2Checkboxes[0]).toHaveAttribute('value', 'local-authority-3');
    expect(region2Checkboxes[0]).toEqual(
      region2Group.getByLabelText('Local authority 3'),
    );
    expect(region2Checkboxes[0]).not.toBeChecked();

    expect(region2Checkboxes[1]).toHaveAttribute('value', 'local-authority-4');
    expect(region2Checkboxes[1]).toEqual(
      region2Group.getByLabelText('Local authority 4'),
    );
  });

  test('does not render school location group options by default', async () => {
    render(
      <LocationFiltersForm
        {...testWizardStepProps}
        stepHeading="Choose locations"
        options={testSchools}
        onSubmit={noop}
      />,
    );

    const schoolCheckboxes = screen.queryAllByRole('checkbox', {});
    expect(schoolCheckboxes).toHaveLength(0);

    expect(
      await screen.findByText(
        'Search by school name or unique reference number (URN), and select at least one option before continuing to the next step.',
      ),
    ).toBeInTheDocument();
  });

  test('renders school location group options as a flat list with urn and LA when there are search results', async () => {
    render(
      <LocationFiltersForm
        {...testWizardStepProps}
        stepHeading="Choose locations"
        options={testSchools}
        onSubmit={noop}
      />,
    );

    const searchInput = await screen.findByLabelText(/Search options/);

    await userEvent.type(searchInput, 'school');

    expect(await screen.findByText('Select all 3 options')).toBeInTheDocument();

    const schoolGroup = within(
      await screen.findByRole('group', {
        name: 'Schools',
      }),
    );

    const schoolCheckboxes = schoolGroup.getAllByRole('checkbox');
    expect(schoolCheckboxes).toHaveLength(3);

    expect(schoolCheckboxes[0]).toHaveAttribute('value', 'school-id-1');
    expect(schoolCheckboxes[0]).toEqual(schoolGroup.getByLabelText('School 1'));
    expect(
      schoolGroup.getByText('URN: 000001; Local Authority: LA 1'),
    ).toHaveAttribute(
      'id',
      'locationFiltersForm-locations-school-options-school-id-1-item-hint',
    );
    expect(schoolCheckboxes[0]).not.toBeChecked();

    expect(schoolCheckboxes[1]).toHaveAttribute('value', 'school-id-2');
    expect(schoolCheckboxes[1]).toEqual(schoolGroup.getByLabelText('School 2'));
    expect(
      schoolGroup.getByText('URN: 000002; Local Authority: LA 1'),
    ).toHaveAttribute(
      'id',
      'locationFiltersForm-locations-school-options-school-id-2-item-hint',
    );
    expect(schoolCheckboxes[1]).not.toBeChecked();

    expect(schoolCheckboxes[2]).toHaveAttribute('value', 'school-id-3');
    expect(schoolCheckboxes[2]).toEqual(schoolGroup.getByLabelText('School 3'));
    expect(
      schoolGroup.getByText('URN: 000003; Local Authority: LA 2'),
    ).toHaveAttribute(
      'id',
      'locationFiltersForm-locations-school-options-school-id-3-item-hint',
    );
    expect(schoolCheckboxes[2]).not.toBeChecked();
  });

  test('selecting options shows the number of selected options for each location group', async () => {
    render(
      <LocationFiltersForm
        {...testWizardStepProps}
        stepHeading="Choose locations"
        options={testLocationsFlat}
        onSubmit={noop}
      />,
    );

    await userEvent.click(await screen.findByLabelText('Local authority 2'));

    const countrySection = within(
      await screen.findByTestId('locations-country-accordion'),
    );
    const localAuthoritySection = within(
      await screen.findByTestId('locations-localAuthority-accordion'),
    );
    const regionSection = within(
      await screen.findByTestId('locations-region-accordion'),
    );

    expect(localAuthoritySection.getByText('1 selected')).toBeInTheDocument();

    await userEvent.click(await screen.findByLabelText('Local authority 3'));
    await userEvent.click(await screen.findByLabelText('Country 1'));
    await userEvent.click(await screen.findByLabelText('Region 2'));

    expect(localAuthoritySection.getByText('2 selected')).toBeInTheDocument();
    expect(countrySection.getByText('1 selected')).toBeInTheDocument();
    expect(regionSection.getByText('1 selected')).toBeInTheDocument();
  });

  test('selecting options shows the number of selected options for each location group (with nested groups)', async () => {
    render(
      <LocationFiltersForm
        {...testWizardStepProps}
        stepHeading="Choose locations"
        options={testLocationsNested}
        onSubmit={noop}
      />,
    );

    const countrySection = within(
      await screen.findByTestId('locations-country-accordion'),
    );
    const localAuthoritySection = within(
      await screen.findByTestId('locations-localAuthority-accordion'),
    );

    await userEvent.click(await screen.findByLabelText('Local authority 2'));

    expect(localAuthoritySection.getByText('1 selected')).toBeInTheDocument();

    await userEvent.click(await screen.findByLabelText('Local authority 3'));
    await userEvent.click(await screen.findByLabelText('Country 1'));

    expect(localAuthoritySection.getByText('2 selected')).toBeInTheDocument();
    expect(countrySection.getByText('1 selected')).toBeInTheDocument();
  });

  test('shows validation errors if no location is selected', async () => {
    render(
      <LocationFiltersForm
        {...testWizardStepProps}
        stepHeading="Choose locations"
        options={testLocationsFlat}
        onSubmit={noop}
      />,
    );

    await userEvent.click(
      await screen.findByRole('button', {
        name: 'Next step',
      }),
    );

    expect(
      await screen.findByRole('link', {
        name: 'Select at least one location',
      }),
    ).toBeInTheDocument();
  });

  test('only the correct checkboxes are selected from initial values', async () => {
    render(
      <LocationFiltersForm
        {...testWizardStepProps}
        stepHeading="Choose locations"
        options={testLocationsFlat}
        onSubmit={noop}
        initialValues={['region-2', 'local-authority-1', 'local-authority-3']}
      />,
    );

    expect(await screen.findByLabelText('Local authority 1')).toBeChecked();
    expect(await screen.findByLabelText('Local authority 3')).toBeChecked();
    expect(await screen.findByLabelText('Region 2')).toBeChecked();

    expect(await screen.findByLabelText('Country 1')).not.toBeChecked();
    expect(await screen.findByLabelText('Local authority 2')).not.toBeChecked();
    expect(await screen.findByLabelText('Region 1')).not.toBeChecked();
  });

  test('only the correct checkboxes (in nested groups) are selected from initial values', async () => {
    render(
      <LocationFiltersForm
        {...testWizardStepProps}
        stepHeading="Choose locations"
        options={testLocationsNested}
        onSubmit={noop}
        initialValues={['local-authority-1', 'local-authority-3']}
      />,
    );

    expect(await screen.findByLabelText('Local authority 1')).toBeChecked();
    expect(await screen.findByLabelText('Local authority 3')).toBeChecked();

    expect(await screen.findByLabelText('Country 1')).not.toBeChecked();
    expect(await screen.findByLabelText('Local authority 2')).not.toBeChecked();
    expect(await screen.findByLabelText('Local authority 4')).not.toBeChecked();
  });

  test('automatically selects the option and expands the group if only one location available', async () => {
    const testSingleLocation: SubjectMeta['locations'] = {
      country: {
        legend: 'Country',
        options: [
          {
            id: 'country-1',
            label: 'Country 1',
            value: 'country-1',
          },
        ],
      },
    };

    render(
      <LocationFiltersForm
        {...testWizardStepProps}
        stepHeading="Choose locations"
        options={testSingleLocation}
        onSubmit={noop}
      />,
    );

    const countrySection = within(
      await screen.findByTestId('locations-country-accordion'),
    );
    expect(countrySection.getByText('1 selected')).toBeInTheDocument();

    const country = within(
      await screen.findByRole('group', { name: 'Country' }),
    );

    const checkboxes = country.getAllByRole('checkbox');

    expect(checkboxes).toHaveLength(1);

    expect(country.getByLabelText('Country 1')).toEqual(checkboxes[0]);
    expect(country.getByLabelText('Country 1')).toBeVisible();
    expect(country.getByLabelText('Country 1')).toBeChecked();
  });

  test('automatically selects the option and expands the nested group if only one location available', async () => {
    const testSingleLocationNested: SubjectMeta['locations'] = {
      localAuthority: {
        legend: 'Local authority',
        options: [
          {
            label: 'Region 1',
            value: 'region-1',
            level: 'Region',
            options: [
              {
                id: 'local-authority-1',
                label: 'Local authority 1',
                value: 'local-authority-1',
              },
            ],
          },
        ],
      },
    };

    render(
      <LocationFiltersForm
        {...testWizardStepProps}
        stepHeading="Choose locations"
        options={testSingleLocationNested}
        onSubmit={noop}
      />,
    );

    const localAuthoritySection = within(
      await screen.findByTestId('locations-localAuthority-accordion'),
    );

    expect(
      await localAuthoritySection.findByText('1 selected'),
    ).toBeInTheDocument();

    const localAuthority = within(
      await screen.findByRole('group', { name: 'Local authority' }),
    );

    const checkboxes = await localAuthority.findAllByRole('checkbox');

    expect(checkboxes).toHaveLength(1);

    expect(await localAuthority.findByLabelText('Local authority 1')).toEqual(
      checkboxes[0],
    );
    expect(
      await localAuthority.findByLabelText('Local authority 1'),
    ).toBeChecked();
  });

  test('clicking `Next step` calls `onSubmit` with correct values', async () => {
    const handleSubmit = jest.fn();
    render(
      <LocationFiltersForm
        {...testWizardStepProps}
        stepHeading="Choose locations"
        options={testLocationsFlat}
        onSubmit={handleSubmit}
      />,
    );

    await userEvent.click(await screen.findByLabelText('Local authority 3'));
    await userEvent.click(await screen.findByLabelText('Region 1'));

    await userEvent.click(
      await screen.findByRole('button', { name: 'Next step' }),
    );

    const expected = {
      locationIds: ['local-authority-3', 'region-1'],
    };

    await waitFor(() => {
      expect(handleSubmit).toHaveBeenCalledTimes(1);
      expect(handleSubmit).toHaveBeenCalledWith(expected);
    });
  });

  test('clicking `Next step` calls `onSubmit` with correct values for nested groups', async () => {
    const handleSubmit = jest.fn();
    render(
      <LocationFiltersForm
        {...testWizardStepProps}
        stepHeading="Choose locations"
        options={testLocationsNested}
        onSubmit={handleSubmit}
      />,
    );

    await userEvent.click(await screen.findByLabelText('Local authority 1'));
    await userEvent.click(await screen.findByLabelText('Local authority 3'));
    await userEvent.click(await screen.findByLabelText('Country 1'));

    await userEvent.click(
      await screen.findByRole('button', { name: 'Next step' }),
    );

    const expected = {
      locationIds: ['country-1', 'local-authority-1', 'local-authority-3'],
    };

    await waitFor(() => {
      expect(handleSubmit).toHaveBeenCalledTimes(1);
      expect(handleSubmit).toHaveBeenCalledWith(expected);
    });
  });
});
