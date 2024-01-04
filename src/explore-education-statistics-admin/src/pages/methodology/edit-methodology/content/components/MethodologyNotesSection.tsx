import { useEditingContext } from '@admin/contexts/EditingContext';
import methodologyNoteService, {
  MethodologyNote,
} from '@admin/services/methodologyNoteService';
import { MethodologyContent } from '@admin/services/methodologyContentService';
import MethodologyNotesAddForm from '@admin/pages/methodology/edit-methodology/content/components/MethodologyNotesAddForm';
import MethodologyNotesEditForm from '@admin/pages/methodology/edit-methodology/content/components/MethodologyNotesEditForm';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import Details from '@common/components/Details';
import FormattedDate from '@common/components/FormattedDate';
import ModalConfirm from '@common/components/ModalConfirm';
import SummaryListItem from '@common/components/SummaryListItem';
import useToggle from '@common/hooks/useToggle';
import orderBy from 'lodash/orderBy';
import React, { useState } from 'react';

interface Props {
  methodology: MethodologyContent;
}

interface CreateFormValues {
  content: string;
}

interface EditFormValues {
  id: string;
  content: string;
  displayDate: Date;
}

const MethodologyNotesSection = ({ methodology }: Props) => {
  const [addFormOpen, toggleAddFormOpen] = useToggle(false);
  const [editFormOpen, toggleEditFormOpen] = useToggle(false);
  const [selectedMethodologyNote, setSelectedMethodologyNote] = useState<
    MethodologyNote | undefined
  >();
  const [methodologyNotes, setMethodologyNotes] = useState<MethodologyNote[]>(
    methodology.notes,
  );
  const { editingMode } = useEditingContext();

  const addMethodologyNote = async (methodologyNote: CreateFormValues) => {
    const newNote = await methodologyNoteService.create(methodology.id, {
      content: methodologyNote.content,
      displayDate: new Date(Date.now()),
    });

    setMethodologyNotes([...methodologyNotes, newNote]);
    toggleAddFormOpen.off();
  };

  const editMethodologyNote = async (methodologyNote: EditFormValues) => {
    const updatedNote = await methodologyNoteService.edit(
      methodologyNote.id,
      methodology.id,
      {
        content: methodologyNote.content,
        displayDate: methodologyNote.displayDate,
      },
    );
    const updatedNotes = methodologyNotes.map(note =>
      note.id === updatedNote.id ? updatedNote : note,
    );
    setSelectedMethodologyNote(undefined);
    toggleEditFormOpen.off();
    setMethodologyNotes(updatedNotes);
  };

  const deleteMethodologyNote = async (id: string) => {
    await methodologyNoteService.delete(id, methodology.id);
    const updatedNotes = methodologyNotes.filter(note => note.id !== id);
    setMethodologyNotes(updatedNotes);
  };

  const openAddForm = () => {
    toggleAddFormOpen.on();
    toggleEditFormOpen.off();
  };

  const openEditForm = (selected: MethodologyNote) => {
    toggleAddFormOpen.off();
    toggleEditFormOpen.on();
    setSelectedMethodologyNote(selected);
  };

  const renderAddForm = () => {
    return !addFormOpen ? (
      <Button onClick={openAddForm}>Add note</Button>
    ) : (
      <MethodologyNotesAddForm
        onCancel={toggleAddFormOpen.off}
        onSubmit={addMethodologyNote}
      />
    );
  };

  const orderedNotes = orderBy(methodologyNotes, 'displayDate', 'desc');

  return (
    <SummaryListItem term="Last updated">
      {methodologyNotes.length > 0 ? (
        <>
          <FormattedDate testId="Last updated date">
            {orderedNotes[0].displayDate}
          </FormattedDate>
          <Details
            summary={`See all notes (${methodologyNotes.length})`}
            id="methodologyNotes"
            open={editingMode === 'edit'}
          >
            <ol className="govuk-list" data-testid="notes">
              {orderedNotes.map(note => (
                <li key={note.id}>
                  {editingMode === 'edit' &&
                  editFormOpen &&
                  selectedMethodologyNote?.id === note.id ? (
                    <MethodologyNotesEditForm
                      initialValues={{
                        id: selectedMethodologyNote.id,
                        displayDate: new Date(
                          selectedMethodologyNote.displayDate,
                        ),
                        content: selectedMethodologyNote.content,
                      }}
                      onCancel={toggleEditFormOpen.off}
                      onSubmit={editMethodologyNote}
                    />
                  ) : (
                    <>
                      <FormattedDate
                        className="govuk-body govuk-!-font-weight-bold"
                        data-testid="note-displayDate"
                      >
                        {note.displayDate}
                      </FormattedDate>
                      <p data-testid="note-content">{note.content}</p>

                      {editingMode === 'edit' && (
                        <ButtonGroup>
                          <Button
                            variant="secondary"
                            onClick={() => openEditForm(note)}
                          >
                            Edit note
                          </Button>

                          <ModalConfirm
                            title="Confirm deletion of methodology note"
                            triggerButton={
                              <Button variant="warning">Remove note</Button>
                            }
                            onConfirm={() => deleteMethodologyNote(note.id)}
                          >
                            <p>
                              This methodology note will be removed from this
                              methodology
                            </p>
                          </ModalConfirm>
                        </ButtonGroup>
                      )}
                    </>
                  )}
                </li>
              ))}
            </ol>
            {editingMode === 'edit' && renderAddForm()}
          </Details>
        </>
      ) : (
        <>
          <p>TBA</p>
          {editingMode === 'edit' && renderAddForm()}
        </>
      )}
    </SummaryListItem>
  );
};

export default MethodologyNotesSection;
