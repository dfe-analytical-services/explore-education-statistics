import ReleaseNotesSection from '@admin/pages/release/content/components/ReleaseNotesSection';
import { EditingContextProvider } from '@admin/contexts/EditingContext';
import _releaseNoteService from '@admin/services/releaseNoteService';
import { generateEditableRelease } from '@admin-test/generators/releaseContentGenerators';
import { ReleaseNote as ReleaseNoteData } from '@common/services/publicationService';
import { render, screen, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import React from 'react';

jest.mock('@admin/services/releaseNoteService');
const releaseNoteService = _releaseNoteService as jest.Mocked<
  typeof _releaseNoteService
>;

describe('ReleaseNote', () => {
  const testReleaseNotes: ReleaseNoteData[] = [
    {
      id: 'test-id-2',
      on: new Date('2024-02-02'),
      reason: 'Test note 2',
    },
    {
      id: 'test-id-1',
      on: new Date('2024-01-01'),
      reason: 'Test note 1',
    },
  ];
  const testRelease = generateEditableRelease({ updates: testReleaseNotes });

  describe('is editable', () => {
    test('renders correctly', () => {
      render(
        <EditingContextProvider editingMode="edit">
          <ReleaseNotesSection release={testRelease} />
        </EditingContextProvider>,
      );

      expect(
        screen.getByRole('button', { name: 'See all updates (2)' }),
      ).toBeInTheDocument();

      const updates = screen.getAllByRole('listitem');
      expect(updates).toHaveLength(2);

      expect(
        within(updates[0]).getByText('2 February 2024'),
      ).toBeInTheDocument();
      expect(within(updates[0]).getByText('Test note 2')).toBeInTheDocument();
      expect(
        within(updates[0]).getByRole('button', { name: 'Edit note' }),
      ).toBeInTheDocument();
      expect(
        within(updates[0]).getByRole('button', { name: 'Remove note' }),
      ).toBeInTheDocument();

      expect(
        within(updates[1]).getByText('1 January 2024'),
      ).toBeInTheDocument();
      expect(within(updates[1]).getByText('Test note 1')).toBeInTheDocument();
      expect(
        within(updates[1]).getByRole('button', { name: 'Edit note' }),
      ).toBeInTheDocument();
      expect(
        within(updates[1]).getByRole('button', { name: 'Remove note' }),
      ).toBeInTheDocument();

      expect(
        screen.getByRole('button', { name: 'Add note' }),
      ).toBeInTheDocument();
    });

    test('clicking the add note button shows the form', async () => {
      const user = userEvent.setup();
      render(
        <EditingContextProvider editingMode="edit">
          <ReleaseNotesSection release={testRelease} />
        </EditingContextProvider>,
      );

      await user.click(screen.getByRole('button', { name: 'Add note' }));

      expect(screen.getByLabelText('New release note')).toBeInTheDocument();
      expect(
        screen.getByRole('button', { name: 'Save note' }),
      ).toBeInTheDocument();
      expect(
        screen.getByRole('button', { name: 'Cancel' }),
      ).toBeInTheDocument();
    });

    test('adding a note calls the releaseNoteService and updates the notes list', async () => {
      const user = userEvent.setup();
      releaseNoteService.create.mockResolvedValue([
        { id: 'test-id-3', on: new Date('2024-03-04'), reason: 'Test note 3' },
        ...testReleaseNotes,
      ]);
      render(
        <EditingContextProvider editingMode="edit">
          <ReleaseNotesSection release={testRelease} />
        </EditingContextProvider>,
      );

      await user.click(screen.getByRole('button', { name: 'Add note' }));

      expect(releaseNoteService.create).not.toHaveBeenCalled();

      await user.type(screen.getByLabelText('New release note'), 'Test note 3');
      await user.click(screen.getByRole('button', { name: 'Save note' }));

      expect(releaseNoteService.create).toHaveBeenCalledTimes(1);
      expect(releaseNoteService.create).toHaveBeenCalledWith(
        'Release-title-id',
        {
          reason: 'Test note 3',
        },
      );

      const updates = screen.getAllByRole('listitem');
      expect(updates).toHaveLength(3);

      expect(within(updates[0]).getByText('4 March 2024')).toBeInTheDocument();
      expect(within(updates[0]).getByText('Test note 3')).toBeInTheDocument();
      expect(
        within(updates[0]).getByRole('button', { name: 'Edit note' }),
      ).toBeInTheDocument();
      expect(
        within(updates[0]).getByRole('button', { name: 'Remove note' }),
      ).toBeInTheDocument();
    });

    test('editing a note calls the releaseNoteService and updates the notes list', async () => {
      const user = userEvent.setup();
      const updatedReleaseNote: ReleaseNoteData = {
        id: 'test-id-1',
        on: new Date('2024-01-02'),
        reason: 'Test note 1 edited',
      };
      releaseNoteService.edit.mockResolvedValue([
        testReleaseNotes[0],
        updatedReleaseNote,
      ]);
      render(
        <EditingContextProvider editingMode="edit">
          <ReleaseNotesSection release={testRelease} />
        </EditingContextProvider>,
      );

      await user.click(
        within(screen.getAllByRole('listitem')[1]).getByRole('button', {
          name: 'Edit note',
        }),
      );

      expect(releaseNoteService.edit).not.toHaveBeenCalled();

      await user.clear(screen.getByLabelText('Day'));
      await user.type(screen.getByLabelText('Day'), '2');

      await user.clear(screen.getByLabelText('Edit release note'));
      await user.type(
        screen.getByLabelText('Edit release note'),
        'Test note 1 edited',
      );
      await user.click(screen.getByRole('button', { name: 'Update note' }));

      expect(releaseNoteService.edit).toHaveBeenCalledTimes(1);
      expect(releaseNoteService.edit).toHaveBeenCalledWith(
        'test-id-1',
        'Release-title-id',
        { on: updatedReleaseNote.on, reason: updatedReleaseNote.reason },
      );

      const updates = screen.getAllByRole('listitem');
      expect(updates).toHaveLength(2);

      expect(
        within(updates[1]).getByText('2 January 2024'),
      ).toBeInTheDocument();
      expect(
        within(updates[1]).getByText('Test note 1 edited'),
      ).toBeInTheDocument();
      expect(
        within(updates[1]).getByRole('button', { name: 'Edit note' }),
      ).toBeInTheDocument();
      expect(
        within(updates[1]).getByRole('button', { name: 'Remove note' }),
      ).toBeInTheDocument();
    });

    test('removing a note calls the releaseNoteService and updates the notes list', async () => {
      const user = userEvent.setup();

      releaseNoteService.delete.mockResolvedValue([testReleaseNotes[0]]);
      render(
        <EditingContextProvider editingMode="edit">
          <ReleaseNotesSection release={testRelease} />
        </EditingContextProvider>,
      );

      expect(releaseNoteService.delete).not.toHaveBeenCalled();

      await user.click(
        within(screen.getAllByRole('listitem')[1]).getByRole('button', {
          name: 'Remove note',
        }),
      );
      await user.click(screen.getByRole('button', { name: 'Confirm' }));

      expect(releaseNoteService.delete).toHaveBeenCalledTimes(1);
      expect(releaseNoteService.delete).toHaveBeenCalledWith(
        'test-id-1',
        'Release-title-id',
      );

      const updates = screen.getAllByRole('listitem');
      expect(updates).toHaveLength(1);

      expect(screen.queryByText('1 January 2024')).not.toBeInTheDocument();
      expect(screen.queryByText('Test note 1')).not.toBeInTheDocument();
    });
  });

  describe('is not editable', () => {
    test('renders correctly', () => {
      render(
        <EditingContextProvider editingMode="preview">
          <ReleaseNotesSection release={testRelease} />
        </EditingContextProvider>,
      );

      expect(
        screen.getByRole('button', { name: 'See all updates (2)' }),
      ).toBeInTheDocument();

      const updates = screen.getAllByRole('listitem');
      expect(updates).toHaveLength(2);

      expect(
        within(updates[0]).getByText('2 February 2024'),
      ).toBeInTheDocument();
      expect(within(updates[0]).getByText('Test note 2')).toBeInTheDocument();
      expect(
        within(updates[0]).queryByRole('button', { name: 'Edit note' }),
      ).not.toBeInTheDocument();
      expect(
        within(updates[0]).queryByRole('button', { name: 'Remove note' }),
      ).not.toBeInTheDocument();

      expect(
        within(updates[1]).getByText('1 January 2024'),
      ).toBeInTheDocument();
      expect(within(updates[1]).getByText('Test note 1')).toBeInTheDocument();
      expect(
        within(updates[1]).queryByRole('button', { name: 'Edit note' }),
      ).not.toBeInTheDocument();
      expect(
        within(updates[1]).queryByRole('button', { name: 'Remove note' }),
      ).not.toBeInTheDocument();

      expect(
        screen.queryByRole('button', { name: 'Add note' }),
      ).not.toBeInTheDocument();
    });
  });
});
