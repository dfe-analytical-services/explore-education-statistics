import ApiDataSetLocationGroupOptionsModal from '@admin/pages/release/data/components/ApiDataSetLocationGroupOptionsModal';
import { LocationOptionSource } from '@admin/services/apiDataSetVersionService';
import render from '@common-test/render';
import { screen, within } from '@testing-library/react';

describe('ApiDataSetLocationGroupOptionsModal', () => {
  const testOptions: LocationOptionSource[] = [
    {
      label: 'Location 1',
      code: 'location-1-code',
    },
    {
      label: 'Location 2',
      code: 'location-2-code',
    },
    {
      label: 'Location 3',
      code: 'location-3-code',
    },
  ];

  test('renders correctly', async () => {
    const { user } = render(
      <ApiDataSetLocationGroupOptionsModal
        level="region"
        options={testOptions}
      />,
    );

    await user.click(
      screen.getByRole('button', { name: 'View group options for Regions' }),
    );

    const modal = within(screen.getByRole('dialog'));

    expect(
      modal.getByRole('heading', { name: 'View location group options' }),
    ).toBeInTheDocument();

    expect(
      modal.getByRole('heading', { name: 'Location group' }),
    ).toBeInTheDocument();

    expect(modal.getByTestId('Label')).toHaveTextContent('Regions');

    const items = within(
      modal.getByTestId('location-group-options'),
    ).getAllByRole('listitem');

    expect(items).toHaveLength(3);

    expect(items[0]).toHaveTextContent('Location 1');
    expect(items[0]).toHaveTextContent('Code: location-1-code');

    expect(items[1]).toHaveTextContent('Location 2');
    expect(items[1]).toHaveTextContent('Code: location-2-code');

    expect(items[2]).toHaveTextContent('Location 3');
    expect(items[2]).toHaveTextContent('Code: location-3-code');
  });

  test('pagination works correctly', async () => {
    const { user } = render(
      <ApiDataSetLocationGroupOptionsModal
        level="region"
        options={testOptions}
        pageSize={2}
      />,
    );

    await user.click(
      screen.getByRole('button', { name: 'View group options for Regions' }),
    );

    const modal = within(screen.getByRole('dialog'));

    expect(
      modal.getByRole('heading', { name: 'View location group options' }),
    ).toBeInTheDocument();

    expect(
      modal.getByRole('heading', { name: 'Location group' }),
    ).toBeInTheDocument();

    expect(modal.getByTestId('Label')).toHaveTextContent('Regions');

    const page1Items = within(
      modal.getByTestId('location-group-options'),
    ).getAllByRole('listitem');

    expect(page1Items).toHaveLength(2);

    expect(page1Items[0]).toHaveTextContent('Location 1');
    expect(page1Items[0]).toHaveTextContent('Code: location-1-code');

    expect(page1Items[1]).toHaveTextContent('Location 2');
    expect(page1Items[1]).toHaveTextContent('Code: location-2-code');

    const pagination = within(modal.getByRole('navigation'));

    const pageButtons = pagination.getAllByRole('button');

    expect(pageButtons).toHaveLength(3);

    expect(pageButtons[0]).toEqual(
      pagination.getByRole('button', { name: 'Page 1' }),
    );
    expect(pageButtons[1]).toEqual(
      pagination.getByRole('button', { name: 'Page 2' }),
    );
    expect(pageButtons[2]).toEqual(
      pagination.getByRole('button', { name: 'Next page' }),
    );

    await user.click(pagination.getByRole('button', { name: 'Next page' }));

    await modal.findByText('Previous');

    const page2Items = within(
      modal.getByTestId('location-group-options'),
    ).getAllByRole('listitem');

    expect(page2Items).toHaveLength(1);

    expect(page2Items[0]).toHaveTextContent('Location 3');
    expect(page2Items[0]).toHaveTextContent('Code: location-3-code');
  });
});
