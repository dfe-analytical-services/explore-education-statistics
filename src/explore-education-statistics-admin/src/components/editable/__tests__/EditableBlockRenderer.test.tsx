import EditableBlockRenderer from '@admin/components/editable/EditableBlockRenderer';
import { render, fireEvent, wait } from '@testing-library/react';
import React from 'react';

describe('EditableBlockRenderer', () => {
  describe('MarkDownBlock', () => {
    test('renders non-editable version correctly', () => {
      const { container, queryByText } = render(
        <EditableBlockRenderer
          releaseId=""
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

      expect(queryByText('Edit block')).toBeNull();
      expect(queryByText('Remove block')).toBeNull();
      expect(container.innerHTML).toMatchSnapshot();
    });

    test('renders editable version correctly', () => {
      const { container, queryByText } = render(
        <EditableBlockRenderer
          releaseId=""
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

      expect(queryByText('Edit block')).not.toBeNull();
      expect(queryByText('Remove block')).not.toBeNull();
      expect(container.innerHTML).toMatchSnapshot();
    });

    test('clicking "Edit block" and "Save" calls save callback', () => {
      const handleContentSave = jest.fn();

      const { getByText } = render(
        <EditableBlockRenderer
          releaseId=""
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

      fireEvent.click(getByText('Edit block'));
      fireEvent.click(getByText('Save'));

      expect(handleContentSave).toHaveBeenCalledWith('block-000', '');
    });

    test('clicking "Remove block" and "Confirm" calls delete callback', async () => {
      const handleDelete = jest.fn();

      const { getByText } = render(
        <EditableBlockRenderer
          releaseId=""
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

      fireEvent.click(getByText('Remove block'));

      await wait();

      fireEvent.click(getByText('Confirm'));

      expect(handleDelete).toHaveBeenCalledWith('block-000');
    });
  });
});
