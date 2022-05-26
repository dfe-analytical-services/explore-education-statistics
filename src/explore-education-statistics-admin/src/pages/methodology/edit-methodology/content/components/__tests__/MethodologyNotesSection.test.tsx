import MethodologyNotesSection from '@admin/pages/methodology/edit-methodology/content/components/MethodologyNotesSection';
import { MethodologyContent } from '@admin/services/methodologyContentService';
import { EditingContextProvider } from '@admin/contexts/EditingContext';
import _methodologyNoteService, {
  MethodologyNote,
} from '@admin/services/methodologyNoteService';
import { render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import React from 'react';

jest.mock('@admin/services/methodologyNoteService');
const methodologyNoteService = _methodologyNoteService as jest.Mocked<
  typeof _methodologyNoteService
>;

describe('MethodologyNotesSection', () => {
  const testMethodologyContentWithoutNotes: MethodologyContent = {
    id: 'methodology-1',
    title: 'Methodology title 1',
    slug: 'methodology-title-1',
    status: 'Draft',
    annexes: [],
    content: [],
    notes: [],
  };
  const testMethodologyContentWithNotes: MethodologyContent = {
    ...testMethodologyContentWithoutNotes,
    notes: [
      {
        id: 'note-1',
        content: 'Note 1',
        displayDate: new Date('2021-09-10T00:00:00'),
      },
      {
        id: 'note-2',
        content: 'Note 2',
        displayDate: new Date('2021-08-11T00:00:00'),
      },
      {
        id: 'note-3',
        content: 'Note 3',
        displayDate: new Date('2021-08-10T00:00:00'),
      },
    ],
  };

  describe('displaying notes', () => {
    test('renders correctly when there are no notes', () => {
      render(
        <EditingContextProvider editingMode="edit">
          <MethodologyNotesSection
            methodology={testMethodologyContentWithoutNotes}
          />
        </EditingContextProvider>,
      );
      expect(screen.getByText('Last updated')).toBeInTheDocument();
      expect(screen.getByTestId('Last updated-value')).toHaveTextContent('TBA');
      expect(
        screen.getByRole('button', { name: 'Add note' }),
      ).toBeInTheDocument();
    });

    test('renders correctly with notes', () => {
      render(
        <EditingContextProvider editingMode="edit">
          <MethodologyNotesSection
            methodology={testMethodologyContentWithNotes}
          />
        </EditingContextProvider>,
      );
      expect(screen.getByText('Last updated')).toBeInTheDocument();
      expect(screen.getByTestId('Last updated date')).toHaveTextContent(
        '10 September 2021',
      );

      const notes = screen.getAllByRole('listitem');
      expect(notes).toHaveLength(3);

      expect(
        within(notes[0]).getByText('10 September 2021'),
      ).toBeInTheDocument();
      expect(within(notes[0]).getByText('Note 1')).toBeInTheDocument();
      expect(
        within(notes[0]).getByRole('button', { name: 'Edit note' }),
      ).toBeInTheDocument();
      expect(
        within(notes[0]).getByRole('button', { name: 'Remove note' }),
      ).toBeInTheDocument();

      expect(within(notes[1]).getByText('11 August 2021')).toBeInTheDocument();
      expect(within(notes[1]).getByText('Note 2')).toBeInTheDocument();
      expect(
        within(notes[1]).getByRole('button', { name: 'Edit note' }),
      ).toBeInTheDocument();
      expect(
        within(notes[1]).getByRole('button', { name: 'Remove note' }),
      ).toBeInTheDocument();

      expect(within(notes[2]).getByText('10 August 2021')).toBeInTheDocument();
      expect(within(notes[2]).getByText('Note 3')).toBeInTheDocument();
      expect(
        within(notes[2]).getByRole('button', { name: 'Edit note' }),
      ).toBeInTheDocument();
      expect(
        within(notes[2]).getByRole('button', { name: 'Remove note' }),
      ).toBeInTheDocument();

      expect(
        screen.getByRole('button', { name: 'Add note' }),
      ).toBeInTheDocument();
    });

    test('renders correctly when there are no notes', () => {
      render(
        <EditingContextProvider editingMode="edit">
          <MethodologyNotesSection
            methodology={testMethodologyContentWithoutNotes}
          />
        </EditingContextProvider>,
      );
      expect(screen.getByText('Last updated')).toBeInTheDocument();
      expect(screen.getByTestId('Last updated-value')).toHaveTextContent('TBA');
      expect(
        screen.getByRole('button', { name: 'Add note' }),
      ).toBeInTheDocument();
    });

    test('shows the most recent date for Last Updated', () => {
      const oldNote: MethodologyNote = {
        id: 'note-old',
        content: 'Note Old',
        displayDate: new Date('2020-01-01T00:00:00'),
      };
      const testMethodologyContentWithOldNote: MethodologyContent = {
        ...testMethodologyContentWithNotes,
        notes: [oldNote, ...testMethodologyContentWithNotes.notes],
      };
      render(
        <EditingContextProvider editingMode="edit">
          <MethodologyNotesSection
            methodology={testMethodologyContentWithOldNote}
          />
        </EditingContextProvider>,
      );
      expect(screen.getByText('Last updated')).toBeInTheDocument();
      expect(screen.getByTestId('Last updated date')).toHaveTextContent(
        '10 September 2021',
      );
    });
  });

  describe('adding a note', () => {
    test('shows validation error if no note given and does not submit', async () => {
      render(
        <EditingContextProvider editingMode="edit">
          <MethodologyNotesSection
            methodology={testMethodologyContentWithNotes}
          />
        </EditingContextProvider>,
      );

      userEvent.click(screen.getByRole('button', { name: 'Add note' }));

      await waitFor(() => {
        expect(
          screen.getByLabelText('New methodology note'),
        ).toBeInTheDocument();
      });

      userEvent.click(screen.getByLabelText('New methodology note'));
      userEvent.tab();
      await waitFor(() => {
        expect(screen.getByText('There is a problem')).toBeInTheDocument();
        expect(
          screen.getByRole('link', {
            name: 'Methodology note must be provided',
          }),
        ).toBeInTheDocument();
      });

      userEvent.click(screen.getByRole('button', { name: 'Save note' }));
      expect(methodologyNoteService.create).not.toHaveBeenCalled();
    });

    test('submits successfully with a note and displays the new note', async () => {
      const realNow = Date.now;
      global.Date.now = jest.fn(() => new Date('2031-09-20').getTime());

      const newNote = {
        content: 'New note',
        displayDate: new Date('2031-09-20'),
      };
      methodologyNoteService.create.mockResolvedValue({
        ...newNote,
        id: 'note-4',
      });
      render(
        <EditingContextProvider editingMode="edit">
          <MethodologyNotesSection
            methodology={testMethodologyContentWithNotes}
          />
        </EditingContextProvider>,
      );

      userEvent.click(screen.getByRole('button', { name: 'Add note' }));

      await waitFor(() => {
        expect(
          screen.getByLabelText('New methodology note'),
        ).toBeInTheDocument();
      });

      await userEvent.type(
        screen.getByLabelText('New methodology note'),
        'New note',
      );
      userEvent.click(screen.getByRole('button', { name: 'Save note' }));

      await waitFor(() => {
        expect(methodologyNoteService.create).toHaveBeenCalledWith(
          'methodology-1',
          newNote,
        );
      });
      const notes = screen.getAllByRole('listitem');
      expect(notes).toHaveLength(4);

      expect(
        within(notes[0]).getByText('20 September 2031'),
      ).toBeInTheDocument();
      expect(within(notes[0]).getByText('New note')).toBeInTheDocument();
      expect(
        within(notes[0]).getByRole('button', { name: 'Edit note' }),
      ).toBeInTheDocument();
      expect(
        within(notes[0]).getByRole('button', { name: 'Remove note' }),
      ).toBeInTheDocument();

      // Reset Date.now
      global.Date.now = realNow;
    });
  });

  describe('editing a note', () => {
    test('shows validation error if note or date removed and does not submit', async () => {
      render(
        <EditingContextProvider editingMode="edit">
          <MethodologyNotesSection
            methodology={testMethodologyContentWithNotes}
          />
        </EditingContextProvider>,
      );

      const notes = screen.getAllByRole('listitem');
      userEvent.click(
        within(notes[0]).getByRole('button', { name: 'Edit note' }),
      );

      await waitFor(() => {
        expect(
          screen.getByLabelText('Edit methodology note'),
        ).toBeInTheDocument();
      });

      userEvent.clear(screen.getByLabelText('Edit methodology note'));
      userEvent.tab();
      await waitFor(() => {
        expect(screen.getByText('There is a problem')).toBeInTheDocument();
        expect(
          screen.getByRole('link', {
            name: 'Methodology note must be provided',
          }),
        ).toBeInTheDocument();
      });

      userEvent.clear(screen.getByLabelText('Day'));
      userEvent.clear(screen.getByLabelText('Month'));
      userEvent.clear(screen.getByLabelText('Year'));

      userEvent.tab();
      await waitFor(() => {
        expect(
          screen.getByRole('link', {
            name: 'Enter a valid edit date',
          }),
        ).toBeInTheDocument();
      });

      userEvent.click(screen.getByRole('button', { name: 'Update note' }));

      expect(methodologyNoteService.edit).not.toHaveBeenCalled();
    });

    test('submits successfully the updated note and updates it in the list', async () => {
      const updatedNote = {
        content: 'Note 1 edited',
        displayDate: new Date('2022-12-31'),
      };
      methodologyNoteService.edit.mockResolvedValue({
        ...updatedNote,
        id: 'note-1',
      });
      render(
        <EditingContextProvider editingMode="edit">
          <MethodologyNotesSection
            methodology={testMethodologyContentWithNotes}
          />
        </EditingContextProvider>,
      );

      const notes = screen.getAllByRole('listitem');
      userEvent.click(
        within(notes[0]).getByRole('button', { name: 'Edit note' }),
      );

      await waitFor(() => {
        expect(
          screen.getByLabelText('Edit methodology note'),
        ).toBeInTheDocument();
      });

      await userEvent.type(
        screen.getByLabelText('Edit methodology note'),
        ' edited',
      );
      userEvent.clear(screen.getByLabelText('Day'));
      await userEvent.type(screen.getByLabelText('Day'), '31');
      userEvent.clear(screen.getByLabelText('Month'));
      await userEvent.type(screen.getByLabelText('Month'), '12');
      userEvent.clear(screen.getByLabelText('Year'));
      await userEvent.type(screen.getByLabelText('Year'), '2022');

      userEvent.click(screen.getByRole('button', { name: 'Update note' }));
      await waitFor(() => {
        expect(methodologyNoteService.edit).toHaveBeenCalledWith(
          'note-1',
          'methodology-1',
          updatedNote,
        );
      });

      expect(
        within(notes[0]).getByText('31 December 2022'),
      ).toBeInTheDocument();
      expect(within(notes[0]).getByText('Note 1 edited')).toBeInTheDocument();
    });
  });

  describe('removing notes', () => {
    test('shows the confirm modal when clicking the Remove button', async () => {
      render(
        <EditingContextProvider editingMode="edit">
          <MethodologyNotesSection
            methodology={testMethodologyContentWithNotes}
          />
        </EditingContextProvider>,
      );

      const notes = screen.getAllByRole('listitem');
      userEvent.click(
        within(notes[0]).getByRole('button', { name: 'Remove note' }),
      );

      await waitFor(() => {
        expect(
          screen.getByText('Confirm deletion of methodology note'),
        ).toBeInTheDocument();

        expect(
          screen.getByText(
            'This methodology note will be removed from this methodology',
          ),
        ).toBeInTheDocument();
      });
      expect(
        screen.getByRole('button', { name: 'Confirm' }),
      ).toBeInTheDocument();
    });

    test('successfully removes a note', async () => {
      render(
        <EditingContextProvider editingMode="edit">
          <MethodologyNotesSection
            methodology={testMethodologyContentWithNotes}
          />
        </EditingContextProvider>,
      );

      const notes = screen.getAllByRole('listitem');
      userEvent.click(
        within(notes[0]).getByRole('button', { name: 'Remove note' }),
      );

      userEvent.click(screen.getByRole('button', { name: 'Confirm' }));
      await waitFor(() => {
        expect(methodologyNoteService.delete).toHaveBeenCalledWith(
          'note-1',
          'methodology-1',
        );

        expect(screen.getAllByRole('listitem')).toHaveLength(2);
      });
    });
  });
});
