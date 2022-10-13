import { useEditingContext } from '@admin/contexts/EditingContext';
import methodologyNoteService, {
  MethodologyNote,
} from '@admin/services/methodologyNoteService';
import { MethodologyContent } from '@admin/services/methodologyContentService';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import Details from '@common/components/Details';
import { Form } from '@common/components/form';
import FormFieldDateInput from '@common/components/form/FormFieldDateInput';
import FormFieldTextArea from '@common/components/form/FormFieldTextArea';
import FormattedDate from '@common/components/FormattedDate';
import ModalConfirm from '@common/components/ModalConfirm';
import SummaryListItem from '@common/components/SummaryListItem';
import Yup from '@common/validation/yup';
import orderBy from 'lodash/orderBy';
import { Formik } from 'formik';
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
  const [addFormOpen, setAddFormOpen] = useState<boolean>(false);
  const [editFormOpen, setEditFormOpen] = useState<boolean>(false);
  const [deletedMethodologyNoteId, setDeletedMethodologyNoteId] = useState<
    string
  >('');
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
    setAddFormOpen(false);
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
    setEditFormOpen(false);
    setMethodologyNotes(updatedNotes);
  };

  const deleteMethodologyNote = async (id: string) => {
    await methodologyNoteService.delete(id, methodology.id);
    const updatedNotes = methodologyNotes.filter(note => note.id !== id);
    setMethodologyNotes(updatedNotes);
    setDeletedMethodologyNoteId('');
  };

  const openAddForm = () => {
    setAddFormOpen(true);
    setEditFormOpen(false);
  };

  const openEditForm = (selected: MethodologyNote) => {
    setAddFormOpen(false);
    setEditFormOpen(true);
    setSelectedMethodologyNote(selected);
  };

  const renderAddForm = () => {
    const formId = 'createMethodologyNoteForm';

    return !addFormOpen ? (
      <Button onClick={openAddForm}>Add note</Button>
    ) : (
      <Formik<CreateFormValues>
        initialValues={{ content: '' }}
        validationSchema={Yup.object<CreateFormValues>({
          content: Yup.string().required('Methodology note must be provided'),
        })}
        onSubmit={methodologyNote => addMethodologyNote(methodologyNote)}
      >
        {form => {
          return (
            <Form id={formId}>
              <FormFieldTextArea<CreateFormValues>
                label="New methodology note"
                name="content"
                rows={3}
              />

              <ButtonGroup>
                <Button type="submit">Save note</Button>
                <Button
                  variant="secondary"
                  onClick={() => {
                    form.resetForm();
                    setAddFormOpen(false);
                  }}
                >
                  Cancel
                </Button>
              </ButtonGroup>
            </Form>
          );
        }}
      </Formik>
    );
  };

  const renderEditForm = () => {
    const formId = 'editMethodologyNoteForm';

    return (
      <Formik<EditFormValues>
        initialValues={
          selectedMethodologyNote
            ? {
                id: selectedMethodologyNote.id,
                displayDate: new Date(selectedMethodologyNote.displayDate),
                content: selectedMethodologyNote.content,
              }
            : ({
                content: '',
              } as EditFormValues)
        }
        validationSchema={Yup.object<Omit<EditFormValues, 'id'>>({
          displayDate: Yup.date().required('Enter a valid edit date'),
          content: Yup.string().required('Methodology note must be provided'),
        })}
        onSubmit={methodologyNote => editMethodologyNote(methodologyNote)}
      >
        {form => {
          return (
            <Form id={formId}>
              <FormFieldDateInput<EditFormValues>
                name="displayDate"
                legend="Edit date"
                legendSize="s"
              />
              <FormFieldTextArea<EditFormValues>
                label="Edit methodology note"
                name="content"
                rows={3}
              />

              <ButtonGroup>
                <Button type="submit">Update note</Button>
                <Button
                  variant="secondary"
                  onClick={() => {
                    form.resetForm();
                    setEditFormOpen(false);
                  }}
                >
                  Cancel
                </Button>
              </ButtonGroup>
            </Form>
          );
        }}
      </Formik>
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
                    renderEditForm()
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
                          <Button
                            variant="warning"
                            onClick={() => setDeletedMethodologyNoteId(note.id)}
                          >
                            Remove note
                          </Button>
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

      <ModalConfirm
        open={deletedMethodologyNoteId !== ''}
        title="Confirm deletion of methodology note"
        onExit={() => setDeletedMethodologyNoteId('')}
        onCancel={() => setDeletedMethodologyNoteId('')}
        onConfirm={() => deleteMethodologyNote(deletedMethodologyNoteId)}
      >
        <p>This methodology note will be removed from this methodology</p>
      </ModalConfirm>
    </SummaryListItem>
  );
};

export default MethodologyNotesSection;
