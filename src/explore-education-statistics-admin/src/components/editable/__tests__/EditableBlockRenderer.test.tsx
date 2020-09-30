import EditableBlockRenderer from '@admin/components/editable/EditableBlockRenderer';
import { render, screen, waitFor } from '@testing-library/react';
import React from 'react';
import userEvent from '@testing-library/user-event';

describe('EditableBlockRenderer', () => {
  describe('MarkDownBlock', () => {
    test('renders non-editable version correctly', () => {
      const { container } = render(
        <EditableBlockRenderer
          releaseId="test-release-id"
          block={{
            id: 'block-000',
            order: 0,
            comments: [],
            type: 'MarkDownBlock',
            body: 'test',
          }}
          onContentSave={() => {}}
          onDelete={() => {}}
        />,
      );

      expect(screen.queryByText('Edit block')).toBeNull();
      expect(screen.queryByText('Remove block')).toBeNull();
      expect(container.innerHTML).toMatchSnapshot();
    });

    test('renders editable version correctly', () => {
      const { container } = render(
        <EditableBlockRenderer
          releaseId="test-release-id"
          editable
          block={{
            id: 'block-000',
            order: 0,
            comments: [],
            type: 'MarkDownBlock',
            body: 'test',
          }}
          onContentSave={() => {}}
          onDelete={() => {}}
        />,
      );

      expect(screen.queryByText('Edit block')).not.toBeNull();
      expect(screen.queryByText('Remove block')).not.toBeNull();
      expect(container.innerHTML).toMatchSnapshot();
    });

    test('clicking "Edit block" and "Save" calls save callback', () => {
      const handleContentSave = jest.fn();

      render(
        <EditableBlockRenderer
          releaseId="test-release-id"
          editable
          block={{
            id: 'block-000',
            order: 0,
            comments: [],
            type: 'MarkDownBlock',
            body: '',
          }}
          onContentSave={handleContentSave}
          onDelete={() => {}}
        />,
      );

      userEvent.click(screen.getByText('Edit block'));
      userEvent.click(screen.getByText('Save'));

      expect(handleContentSave).toHaveBeenCalledWith('block-000', '');
    });

    test('clicking "Remove block" and "Confirm" calls delete callback', async () => {
      const handleDelete = jest.fn();

      render(
        <EditableBlockRenderer
          releaseId="test-release-id"
          editable
          block={{
            id: 'block-000',
            order: 0,
            comments: [],
            type: 'MarkDownBlock',
            body: '',
          }}
          onContentSave={() => {}}
          onDelete={handleDelete}
        />,
      );

      userEvent.click(screen.getByText('Remove block'));

      await waitFor(() => {
        expect(screen.getByText('Confirm')).toBeInTheDocument();
      });

      userEvent.click(screen.getByText('Confirm'));

      expect(handleDelete).toHaveBeenCalledWith('block-000');
    });
  });
});
