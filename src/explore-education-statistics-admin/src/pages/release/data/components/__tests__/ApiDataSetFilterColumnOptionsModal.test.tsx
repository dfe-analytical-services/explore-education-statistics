import ApiDataSetMappableFilterColumnOptionsModal from '@admin/pages/release/data/components/ApiDataSetMappableFilterColumnOptionsModal';
import render from '@common-test/render';
import { screen, within } from '@testing-library/react';

describe('ApiDataSetMappableFilterColumnOptionsModal', () => {
  const testOptions: string[] = [
    'Filter option 1',
    'Filter option 2',
    'Filter option 3',
  ];

  test('renders correctly', async () => {
    const { user } = render(
      <ApiDataSetMappableFilterColumnOptionsModal
        column="filter_1_column"
        label="Filter 1"
        options={testOptions}
        publicId="filter-1-id"
      />,
    );

    await user.click(
      screen.getByRole('button', { name: 'View filter options for Filter 1' }),
    );

    const modal = within(screen.getByRole('dialog'));

    expect(
      modal.getByRole('heading', { name: 'View filter options' }),
    ).toBeInTheDocument();

    expect(
      modal.getByRole('heading', { name: 'Filter column' }),
    ).toBeInTheDocument();

    expect(modal.getByTestId('Label')).toHaveTextContent('Filter 1');
    expect(modal.getByTestId('Column')).toHaveTextContent('filter_1_column');
    expect(modal.getByTestId('Identifier')).toHaveTextContent('filter-1-id');

    const items = within(modal.getByTestId('filter-options')).getAllByRole(
      'listitem',
    );

    expect(items).toHaveLength(3);

    expect(items[0]).toHaveTextContent('Filter option 1');
    expect(items[1]).toHaveTextContent('Filter option 2');
    expect(items[2]).toHaveTextContent('Filter option 3');
  });

  test('does not render Identifier summary item if `publicId` is not provided', async () => {
    const { user } = render(
      <ApiDataSetMappableFilterColumnOptionsModal
        column="filter_1_column"
        label="Filter 1"
        options={testOptions}
      />,
    );

    await user.click(
      screen.getByRole('button', { name: 'View filter options for Filter 1' }),
    );

    const modal = within(screen.getByRole('dialog'));

    expect(modal.getByTestId('Label')).toHaveTextContent('Filter 1');
    expect(modal.getByTestId('Column')).toHaveTextContent('filter_1_column');
    expect(modal.queryByTestId('Identifier')).not.toBeInTheDocument();
  });

  test('pagination works correctly', async () => {
    const { user } = render(
      <ApiDataSetMappableFilterColumnOptionsModal
        column="filter_1_column"
        label="Filter 1"
        options={testOptions}
        pageSize={2}
        publicId="filter-1-id"
      />,
    );

    await user.click(
      screen.getByRole('button', { name: 'View filter options for Filter 1' }),
    );

    const modal = within(screen.getByRole('dialog'));

    expect(
      modal.getByRole('heading', { name: 'View filter options' }),
    ).toBeInTheDocument();

    expect(
      modal.getByRole('heading', { name: 'Filter column' }),
    ).toBeInTheDocument();

    const page1Items = within(modal.getByTestId('filter-options')).getAllByRole(
      'listitem',
    );

    expect(page1Items).toHaveLength(2);

    expect(page1Items[0]).toHaveTextContent('Filter option 1');
    expect(page1Items[1]).toHaveTextContent('Filter option 2');

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

    const page2Items = within(modal.getByTestId('filter-options')).getAllByRole(
      'listitem',
    );

    expect(page2Items).toHaveLength(1);

    expect(page2Items[0]).toHaveTextContent('Filter option 3');
  });
});
