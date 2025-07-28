import ApiDataSetMappingModal from '@admin/pages/release/data/components/ApiDataSetMappingModal';
import {
  LocationMappingWithKey,
  LocationCandidateWithKey,
} from '@admin/pages/release/data/utils/getApiDataSetLocationMappings';
import render from '@common-test/render';
import { screen, waitFor, within } from '@testing-library/react';

describe('ApiDataSetMappingModal', () => {
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

  test('clicking the edit button shows the modal', async () => {
    const { user } = render(
      <ApiDataSetMappingModal
        groupKey="region"
        itemLabel="location"
        mapping={testMapping}
        newItems={testNewLocations}
        onSubmit={Promise.resolve}
      />,
    );
    await user.click(
      screen.getByRole('button', { name: 'Map option for Location 1' }),
    );

    const modal = within(screen.getByRole('dialog'));
    expect(
      modal.getByRole('heading', { name: 'Map existing location' }),
    ).toBeInTheDocument();
    expect(
      modal.getByRole('heading', { name: 'Current data set location' }),
    ).toBeInTheDocument();
    expect(modal.getByTestId('Label')).toHaveTextContent('Location 1');
    expect(modal.getByTestId('Identifier')).toHaveTextContent(
      'location-1-public-id',
    );
    expect(modal.getByRole('group', { name: 'Next data set location' }));
    expect(modal.getByRole('button', { name: 'Update location mapping' }));
    expect(modal.getByRole('button', { name: 'Cancel' }));
  });

  test('submitting the form calls `onSubmit` and closes the modal', async () => {
    const handleSubmit = jest.fn();
    const { user } = render(
      <ApiDataSetMappingModal
        groupKey="region"
        itemLabel="location"
        mapping={testMapping}
        newItems={testNewLocations}
        onSubmit={handleSubmit}
      />,
    );
    await user.click(
      screen.getByRole('button', { name: 'Map option for Location 1' }),
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

    expect(screen.queryByRole('dialog')).not.toBeInTheDocument();
  });
});
