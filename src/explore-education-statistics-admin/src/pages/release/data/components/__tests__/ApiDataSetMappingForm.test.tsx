import ApiDataSetMappingForm from '@admin/pages/release/data/components/ApiDataSetMappingForm';
import {
  LocationCandidateWithKey,
  LocationMappingWithKey,
} from '@admin/pages/release/data/utils/getApiDataSetLocationMappings';
import render from '@common-test/render';
import { screen, waitFor, within } from '@testing-library/react';

describe('ApiDataSetMappingForm', () => {
  const testMapping: LocationMappingWithKey = {
    publicId: 'location-1-public-id',
    sourceKey: 'Location1Key',
    source: {
      code: 'location-1-code',
      label: 'Location 1',
    },
    type: 'AutoNone',
  };

  const testNewLocations: LocationCandidateWithKey[] = [
    {
      key: 'Location2Key',
      label: 'Location 2',
    },
    {
      key: 'Location3Key',
      label: 'Location 3',
    },
    {
      key: 'Location4Key',
      label: 'Location 4',
    },
  ];

  test('renders the form', () => {
    render(
      <ApiDataSetMappingForm
        groupKey="region"
        itemLabel="location"
        mapping={testMapping}
        newItems={testNewLocations}
        onSubmit={Promise.resolve}
      />,
    );

    expect(screen.getByLabelText('Search locations')).toBeInTheDocument();

    const radioGroup = within(
      screen.getByRole('group', { name: 'Next data set location' }),
    );
    const radios = radioGroup.getAllByRole('radio');
    expect(radios).toHaveLength(4);
    expect(radios[0]).toBe(screen.getByLabelText('No mapping available'));
    expect(radios[0]).not.toBeChecked();
    expect(radios[1]).toBe(screen.getByLabelText('Location 2'));
    expect(radios[1]).not.toBeChecked();
    expect(radios[2]).toBe(screen.getByLabelText('Location 3'));
    expect(radios[2]).not.toBeChecked();
    expect(radios[3]).toBe(screen.getByLabelText('Location 4'));
    expect(radios[3]).not.toBeChecked();

    expect(screen.getByRole('button', { name: 'Update location mapping' }));
    expect(screen.getByRole('button', { name: 'Cancel' }));
  });

  test('renders the form correctly with a candidate previously selected', async () => {
    render(
      <ApiDataSetMappingForm
        candidate={{
          key: 'Location3Key',
          label: 'Location 3',
        }}
        groupKey="region"
        itemLabel="location"
        mapping={{
          ...testMapping,
          type: 'ManualMapped',
          candidateKey: 'Location3Key',
        }}
        newItems={[testNewLocations[0], testNewLocations[2]]}
        onSubmit={Promise.resolve}
      />,
    );

    const radioGroup = within(
      screen.getByRole('group', { name: 'Next data set location' }),
    );
    const radios = radioGroup.getAllByRole('radio');
    expect(radios).toHaveLength(4);

    expect(radios[0]).toBe(screen.getByLabelText('No mapping available'));
    expect(radios[0]).not.toBeChecked();

    expect(radios[1]).toBe(screen.getByLabelText('Location 3'));
    expect(radios[1]).toBeChecked();

    expect(radios[2]).toBe(screen.getByLabelText('Location 2'));
    expect(radios[2]).not.toBeChecked();
    expect(radios[3]).toBe(screen.getByLabelText('Location 4'));
    expect(radios[3]).not.toBeChecked();
  });

  test('renders the form correctly with no mapping available previously selected', async () => {
    render(
      <ApiDataSetMappingForm
        groupKey="region"
        itemLabel="location"
        mapping={{ ...testMapping, type: 'ManualNone' }}
        newItems={testNewLocations}
        onSubmit={Promise.resolve}
      />,
    );
    expect(screen.getAllByRole('radio')).toHaveLength(4);
    expect(screen.getByLabelText('No mapping available')).toBeChecked();
  });

  test('submitting the form with a mapping', async () => {
    const handleSubmit = jest.fn();
    const { user } = render(
      <ApiDataSetMappingForm
        groupKey="region"
        itemLabel="location"
        mapping={testMapping}
        newItems={testNewLocations}
        onSubmit={handleSubmit}
      />,
    );

    await user.click(screen.getByLabelText('Location 2'));

    expect(handleSubmit).not.toHaveBeenCalled();
    await user.click(
      screen.getByRole('button', { name: 'Update location mapping' }),
    );

    await waitFor(() => expect(handleSubmit).toHaveBeenCalledTimes(1));
    expect(handleSubmit).toHaveBeenCalledWith({
      candidateKey: 'Location2Key',
      groupKey: 'region',
      previousCandidate: undefined,
      previousMapping: {
        publicId: 'location-1-public-id',
        source: {
          code: 'location-1-code',
          label: 'Location 1',
        },
        sourceKey: 'Location1Key',
        type: 'AutoNone',
      },
      sourceKey: 'Location1Key',
      type: 'ManualMapped',
    });
  });

  test('submitting the form with no mapping available', async () => {
    const handleSubmit = jest.fn();
    const { user } = render(
      <ApiDataSetMappingForm
        groupKey="region"
        itemLabel="location"
        mapping={testMapping}
        newItems={testNewLocations}
        onSubmit={handleSubmit}
      />,
    );

    await user.click(screen.getByLabelText('No mapping available'));

    expect(handleSubmit).not.toHaveBeenCalled();
    await user.click(
      screen.getByRole('button', { name: 'Update location mapping' }),
    );

    await waitFor(() => expect(handleSubmit).toHaveBeenCalledTimes(1));
    expect(handleSubmit).toHaveBeenCalledWith({
      candidateKey: undefined,
      groupKey: 'region',
      previousCandidate: undefined,
      previousMapping: {
        publicId: 'location-1-public-id',
        source: {
          code: 'location-1-code',
          label: 'Location 1',
        },
        sourceKey: 'Location1Key',
        type: 'AutoNone',
      },
      sourceKey: 'Location1Key',
      type: 'ManualNone',
    });
  });

  test('shows validation error when submit with no selection', async () => {
    const { user } = render(
      <ApiDataSetMappingForm
        groupKey="region"
        itemLabel="location"
        mapping={testMapping}
        newItems={testNewLocations}
        onSubmit={Promise.resolve}
      />,
    );

    await user.click(
      screen.getByRole('button', { name: 'Update location mapping' }),
    );

    expect(
      await screen.findByText('Select the next data set location'),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('link', { name: 'Select the next data set location' }),
    ).toHaveAttribute('href', '#mapping-region-form-nextItem');
  });
});
