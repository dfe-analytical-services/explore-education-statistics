import LocationStep from '@common/modules/table-tool/components/LocationStep';
import { render, screen, within } from '@testing-library/react';
import {
  testLocationsFlat,
  testLocationsNested,
} from '@common/modules/table-tool/components/__tests__/__data__/testLocationFilters.data';
import { testWizardStepProps } from '@common/modules/table-tool/components/__tests__/__data__/testWizardStepProps';
import noop from 'lodash/noop';
import React from 'react';

describe('LocationStep', () => {
  const testWizardStepPropsInActive = {
    ...testWizardStepProps,
    isActive: false,
  };

  test('renders the form when active', () => {
    render(
      <LocationStep
        {...testWizardStepProps}
        options={testLocationsFlat}
        stepTitle="Choose locations"
        onSubmit={noop}
      />,
    );

    expect(
      screen.getByRole('group', {
        name: 'Step 1 Choose locations',
      }),
    ).toBeInTheDocument();
  });

  test('renders a read-only view of selected options from a flat list when not active', async () => {
    render(
      <LocationStep
        {...testWizardStepPropsInActive}
        options={testLocationsFlat}
        initialValues={['country-1', 'region-1', 'region-2']}
        stepTitle="Choose locations"
        onSubmit={noop}
      />,
    );

    expect(screen.queryAllByRole('checkbox')).toHaveLength(0);

    const countryChoices = within(screen.getByTestId('Country')).getAllByRole(
      'listitem',
    );

    expect(countryChoices).toHaveLength(1);
    expect(countryChoices[0]).toHaveTextContent('Country 1');

    const regionChoices = within(screen.getByTestId('Region')).getAllByRole(
      'listitem',
    );

    expect(regionChoices).toHaveLength(2);
    expect(regionChoices[0]).toHaveTextContent('Region 1');
    expect(regionChoices[1]).toHaveTextContent('Region 2');
  });

  test('renders a read-only view of selected options from nested groups when not active', async () => {
    render(
      <LocationStep
        {...testWizardStepPropsInActive}
        options={testLocationsNested}
        initialValues={['country-1', 'local-authority-1', 'local-authority-3']}
        stepTitle="Choose locations"
        onSubmit={noop}
      />,
    );

    expect(screen.queryAllByRole('checkbox')).toHaveLength(0);

    const countryChoices = within(screen.getByTestId('Country')).getAllByRole(
      'listitem',
    );

    expect(countryChoices).toHaveLength(1);
    expect(countryChoices[0]).toHaveTextContent('Country 1');

    const localAuthorityChoices = within(
      screen.getByTestId('Local authority'),
    ).getAllByRole('listitem');

    expect(localAuthorityChoices).toHaveLength(2);
    expect(localAuthorityChoices[0]).toHaveTextContent('Local authority 1');
    expect(localAuthorityChoices[1]).toHaveTextContent('Local authority 3');
  });
});
