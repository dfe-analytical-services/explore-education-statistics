import ReleaseNote from '@admin/pages/release/content/components/ReleaseNote';
import { ReleaseNote as ReleaseNoteData } from '@common/services/publicationService';
import { render, screen, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import noop from 'lodash/noop';
import React from 'react';

describe('ReleaseNote', () => {
  const testReleaseNote: ReleaseNoteData = {
    id: 'test-id',
    on: new Date('2024-01-01'),
    reason: 'Test note',
  };

  describe('is editable', () => {
    test('renders correctly', () => {
      render(
        <ReleaseNote
          isEditable
          releaseNote={testReleaseNote}
          onDelete={noop}
          onSubmit={noop}
        />,
      );

      expect(screen.getByText('1 January 2024')).toBeInTheDocument();
      expect(screen.getByText('Test note')).toBeInTheDocument();
      expect(
        screen.getByRole('button', { name: 'Edit note' }),
      ).toBeInTheDocument();
      expect(
        screen.getByRole('button', { name: 'Remove note' }),
      ).toBeInTheDocument();
    });

    test('clicking the edit button shows the edit form', async () => {
      const user = userEvent.setup();
      render(
        <ReleaseNote
          isEditable
          releaseNote={testReleaseNote}
          onDelete={noop}
          onSubmit={noop}
        />,
      );

      await user.click(screen.getByRole('button', { name: 'Edit note' }));

      expect(
        screen.getByRole('group', { name: 'Edit date' }),
      ).toBeInTheDocument();
      expect(screen.getByLabelText('Edit release note')).toHaveValue(
        'Test note',
      );
      expect(
        screen.getByRole('button', { name: 'Update note' }),
      ).toBeInTheDocument();
      expect(
        screen.getByRole('button', { name: 'Cancel' }),
      ).toBeInTheDocument();
    });

    test('clicking the cancel editing button hides the edit form', async () => {
      const user = userEvent.setup();
      render(
        <ReleaseNote
          isEditable
          releaseNote={testReleaseNote}
          onDelete={noop}
          onSubmit={noop}
        />,
      );

      await user.click(screen.getByRole('button', { name: 'Edit note' }));

      expect(
        screen.getByRole('group', { name: 'Edit date' }),
      ).toBeInTheDocument();

      await user.click(screen.getByRole('button', { name: 'Cancel' }));

      expect(
        screen.queryByRole('group', { name: 'Edit date' }),
      ).not.toBeInTheDocument();
    });

    test('submitting the edit form calls onSubmit', async () => {
      const user = userEvent.setup();
      const handleSubmit = jest.fn();
      render(
        <ReleaseNote
          isEditable
          releaseNote={testReleaseNote}
          onDelete={noop}
          onSubmit={handleSubmit}
        />,
      );

      await user.click(screen.getByRole('button', { name: 'Edit note' }));

      expect(handleSubmit).not.toHaveBeenCalled();

      await user.click(screen.getByRole('button', { name: 'Update note' }));

      expect(handleSubmit).toHaveBeenCalledTimes(1);
      expect(handleSubmit).toHaveBeenCalledWith('test-id', {
        on: new Date('2024-01-01'),
        reason: 'Test note',
      });
    });

    test('clicking the remove button shows a modal and calls onDelete when it is confirmed', async () => {
      const user = userEvent.setup();
      const handleDelete = jest.fn();
      render(
        <ReleaseNote
          isEditable
          releaseNote={testReleaseNote}
          onDelete={handleDelete}
          onSubmit={noop}
        />,
      );

      expect(handleDelete).not.toHaveBeenCalled();

      await user.click(screen.getByRole('button', { name: 'Remove note' }));

      const modal = within(screen.getByRole('dialog'));
      expect(
        modal.getByRole('heading', {
          name: 'Confirm deletion of release note',
        }),
      ).toBeInTheDocument();

      await user.click(
        modal.getByRole('button', {
          name: 'Confirm',
        }),
      );

      expect(handleDelete).toHaveBeenCalledTimes(1);
      expect(handleDelete).toHaveBeenCalledWith('test-id');
    });
  });

  describe('is not editable', () => {
    test('renders correctly', () => {
      render(
        <ReleaseNote
          isEditable={false}
          releaseNote={testReleaseNote}
          onDelete={noop}
          onSubmit={noop}
        />,
      );

      expect(screen.getByText('1 January 2024')).toBeInTheDocument();
      expect(screen.getByText('Test note')).toBeInTheDocument();

      expect(
        screen.queryByRole('button', { name: 'Edit note' }),
      ).not.toBeInTheDocument();
      expect(
        screen.queryByRole('button', { name: 'Remove note' }),
      ).not.toBeInTheDocument();
    });
  });
});
