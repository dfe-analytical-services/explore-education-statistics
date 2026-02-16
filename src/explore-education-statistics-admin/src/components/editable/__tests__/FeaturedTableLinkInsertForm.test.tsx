import FeaturedTableLinkInsertForm from '@admin/components/editable/FeaturedTableLinkInsertForm';
import { TestConfigContextProvider } from '@admin/contexts/ConfigContext';
import { ReleaseContentProvider } from '@admin/pages/release/content/contexts/ReleaseContentContext';
import { FeaturedTable } from '@admin/services/featuredTableService';
import { generateEditableRelease } from '@admin-test/generators/releaseContentGenerators';
import baseRender from '@common-test/render';
import { screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import noop from 'lodash/noop';
import React, { ReactNode } from 'react';

describe('FeaturedTableLinkInsertForm ', () => {
  const testFeaturedTables: FeaturedTable[] = [
    {
      dataBlockId: 'data-block-id-1',
      dataBlockParentId: 'data-block-parent-id-1',
      description: 'description text 1',
      id: 'id-1',
      name: 'Featured table 1',
      order: 0,
    },
    {
      dataBlockId: 'data-block-id-2',
      dataBlockParentId: 'data-block-parent-id-2',
      description: 'description text 2',
      id: 'id-2',
      name: 'Featured table 2',
      order: 1,
    },
    {
      dataBlockId: 'data-block-id-3',
      dataBlockParentId: 'data-block-parent-id-3',
      description: 'description text 3',
      id: 'id-3',
      name: 'Something else',
      order: 1,
    },
  ];

  test('does not search if search term is less than 3 characters', async () => {
    render(<FeaturedTableLinkInsertForm onCancel={noop} onSubmit={noop} />);

    await waitFor(() => {
      expect(screen.getByText('Featured table')).toBeInTheDocument();
    });

    await userEvent.type(screen.getByLabelText('Featured table'), 'an');

    expect(screen.queryAllByRole('option')).toHaveLength(0);
  });

  test('shows search results', async () => {
    render(<FeaturedTableLinkInsertForm onCancel={noop} onSubmit={noop} />);

    await waitFor(() => {
      expect(screen.getByText('Featured table')).toBeInTheDocument();
    });

    await userEvent.type(screen.getByLabelText('Featured table'), 'fea');

    await waitFor(() => {
      expect(screen.getByText('Featured table 1')).toBeInTheDocument();
    });

    const options = screen.getAllByRole('option');
    expect(options).toHaveLength(2);
    expect(options[0]).toHaveTextContent('Featured table 1');
    expect(options[1]).toHaveTextContent('Featured table 2');

    expect(screen.queryByText('Something else')).not.toBeInTheDocument();
  });

  test('selecting a search result shows the edit form', async () => {
    render(<FeaturedTableLinkInsertForm onCancel={noop} onSubmit={noop} />);

    await waitFor(() => {
      expect(screen.getByText('Featured table')).toBeInTheDocument();
    });

    await userEvent.type(screen.getByLabelText('Featured table'), 'fea');

    await waitFor(() => {
      expect(screen.getByText('Featured table 1')).toBeInTheDocument();
    });

    const options = screen.getAllByRole('option');
    await userEvent.click(options[0]);

    expect(screen.getByLabelText('Link text')).toHaveValue('Featured table 1');
  });

  test('successfully submitting form', async () => {
    const handleSubmit = jest.fn();
    render(
      <FeaturedTableLinkInsertForm onCancel={noop} onSubmit={handleSubmit} />,
    );

    await waitFor(() => {
      expect(screen.getByText('Featured table')).toBeInTheDocument();
    });

    await userEvent.type(
      screen.getByLabelText('Featured table'),
      'Featured table 1',
    );

    await waitFor(() => {
      expect(screen.getByText('Featured table 1')).toBeInTheDocument();
    });

    const options = screen.getAllByRole('option');
    await userEvent.click(options[0]);

    expect(handleSubmit).not.toHaveBeenCalled();

    await userEvent.click(screen.getByRole('button', { name: 'Insert' }));

    await waitFor(() => {
      expect(handleSubmit).toHaveBeenCalledWith({
        text: 'Featured table 1',
        url: 'http://localhost/data-tables/fast-track/data-block-parent-id-1?featuredTable=true',
      });
    });
  });

  test('successfully submits the form after editing the link text', async () => {
    const handleSubmit = jest.fn();
    render(
      <FeaturedTableLinkInsertForm onCancel={noop} onSubmit={handleSubmit} />,
    );

    await waitFor(() => {
      expect(screen.getByText('Featured table')).toBeInTheDocument();
    });

    await userEvent.type(screen.getByLabelText('Featured table'), 'fea');

    await waitFor(() => {
      expect(screen.getByText('Featured table 1')).toBeInTheDocument();
    });

    const options = screen.getAllByRole('option');
    await userEvent.click(options[0]);

    await userEvent.type(screen.getByLabelText('Link text'), ' edited');

    expect(handleSubmit).not.toHaveBeenCalled();

    await userEvent.click(screen.getByRole('button', { name: 'Insert' }));

    await waitFor(() => {
      expect(handleSubmit).toHaveBeenCalledWith({
        text: 'Featured table 1 edited',
        url: 'http://localhost/data-tables/fast-track/data-block-parent-id-1?featuredTable=true',
      });
    });
  });

  test('shows a validation error if submit without selecting a featured table', async () => {
    const handleSubmit = jest.fn();
    render(
      <FeaturedTableLinkInsertForm onCancel={noop} onSubmit={handleSubmit} />,
    );

    await waitFor(() => {
      expect(screen.getByText('Featured table')).toBeInTheDocument();
    });

    await userEvent.click(screen.getByRole('button', { name: 'Insert' }));

    await waitFor(() => {
      expect(screen.getByText('Select a featured table')).toBeInTheDocument();
    });

    expect(handleSubmit).not.toHaveBeenCalled();
  });

  test('shows a validation error if submit without link text', async () => {
    const handleSubmit = jest.fn();
    render(
      <FeaturedTableLinkInsertForm onCancel={noop} onSubmit={handleSubmit} />,
    );

    await waitFor(() => {
      expect(screen.getByText('Featured table')).toBeInTheDocument();
    });

    await userEvent.type(screen.getByLabelText('Featured table'), 'fea');

    await waitFor(() => {
      expect(screen.getByText('Featured table 1')).toBeInTheDocument();
    });

    const options = screen.getAllByRole('option');
    await userEvent.click(options[0]);

    await userEvent.clear(screen.getByLabelText('Link text'));

    await userEvent.click(screen.getByRole('button', { name: 'Insert' }));

    await waitFor(() => {
      expect(screen.getByText('Enter link text')).toBeInTheDocument();
    });

    expect(handleSubmit).not.toHaveBeenCalled();
  });

  function render(element: ReactNode) {
    baseRender(
      <TestConfigContextProvider>
        <ReleaseContentProvider
          value={{
            canUpdateRelease: true,
            featuredTables: testFeaturedTables,
            release: generateEditableRelease({}),
            unattachedDataBlocks: [],
          }}
        >
          {element}
        </ReleaseContentProvider>
      </TestConfigContextProvider>,
    );
  }
});
