import { useEditingContext } from '@admin/contexts/EditingContext';
import { releaseNoteService } from '@admin/services/release/edit-release/content/service';
import { ManageContentPageViewModel } from '@admin/services/release/edit-release/content/types';
import Button from '@common/components/Button';
import ButtonText from '@common/components/ButtonText';
import Details from '@common/components/Details';
import { Form, FormFieldset } from '@common/components/form';
import FormFieldDateInput from '@common/components/form/FormFieldDateInput';
import FormFieldTextArea from '@common/components/form/FormFieldTextArea';
import FormattedDate from '@common/components/FormattedDate';
import ModalConfirm from '@common/components/ModalConfirm';
import { ReleaseNote } from '@common/services/publicationService';
import Yup from '@common/validation/yup';
import { Formik } from 'formik';
import React, { useState } from 'react';

interface Props {
  release: ManageContentPageViewModel['release'];
  logEvent?: (...params: string[]) => void;
}

interface AddFormValues {
  reason: string;
}

interface EditFormValues {
  on: Date;
  reason: string;
}

const nullLogEvent = () => {};

const emptyReleaseNote: ReleaseNote = {
  id: '',
  releaseId: '',
  on: new Date(),
  reason: '',
};

const ReleaseNotesSection = ({ release, logEvent = nullLogEvent }: Props) => {
  const [addFormOpen, setAddFormOpen] = useState<boolean>(false);
  const [editFormOpen, setEditFormOpen] = useState<boolean>(false);
  const [deletedReleaseNote, setDeletedReleaseNote] = useState<ReleaseNote>(
    emptyReleaseNote,
  );
  const [selectedReleaseNote, setSelectedReleaseNote] = useState<ReleaseNote>(
    emptyReleaseNote,
  );
  const [releaseNotes, setReleaseNotes] = useState<ReleaseNote[]>(
    release.updates,
  );
  const { isEditing } = useEditingContext();

  const addReleaseNote = (releaseNote: AddFormValues) => {
    return new Promise(resolve => {
      releaseNoteService
        .create(release.id, releaseNote)
        .then(newReleaseNotes => {
          setReleaseNotes(newReleaseNotes);
          resolve();
        });
    });
  };

  const editReleaseNote = (id: string, releaseNote: EditFormValues) => {
    return new Promise(resolve => {
      releaseNoteService
        .edit(id, release.id, {
          on: releaseNote.on,
          reason: releaseNote.reason,
        })
        .then(newReleaseNotes => {
          setReleaseNotes(newReleaseNotes);
          resolve();
        });
    });
  };

  const openAddForm = () => {
    setAddFormOpen(true);
    setEditFormOpen(false);
  };

  const openEditForm = (selected: ReleaseNote) => {
    setAddFormOpen(false);
    setEditFormOpen(true);
    setSelectedReleaseNote(selected);
  };

  const renderAddForm = () => {
    return !addFormOpen ? (
      <Button onClick={openAddForm}>Add note</Button>
    ) : (
      <Formik<AddFormValues>
        initialValues={{ reason: '' }}
        validationSchema={Yup.object<AddFormValues>({
          reason: Yup.string().required('Release note must be provided'),
        })}
        onSubmit={releaseNote =>
          addReleaseNote(releaseNote).then(() => {
            setAddFormOpen(false);
          })
        }
      >
        {form => {
          return (
            <Form {...form} id="create-new-release-note-form">
              <FormFieldset
                id="allFieldsFieldset"
                legend="Add new release note"
                legendSize="m"
              >
                <FormFieldTextArea
                  id="reason"
                  label="Release note"
                  name="reason"
                  rows={3}
                />
              </FormFieldset>
              <Button
                type="submit"
                className="govuk-button govuk-!-margin-right-1"
              >
                Add note
              </Button>
              <ButtonText
                className="govuk-button govuk-button--secondary"
                onClick={() => {
                  form.resetForm();
                  setAddFormOpen(false);
                }}
              >
                Cancel
              </ButtonText>
            </Form>
          );
        }}
      </Formik>
    );
  };

  const renderEditForm = () => {
    const formId = 'edit-release-note-form';

    return (
      <Formik<EditFormValues>
        initialValues={
          selectedReleaseNote
            ? {
                on: new Date(selectedReleaseNote.on),
                reason: selectedReleaseNote.reason,
              }
            : ({
                reason: '',
              } as EditFormValues)
        }
        validationSchema={Yup.object<EditFormValues>({
          on: Yup.date().required('Enter a valid edit date'),
          reason: Yup.string().required('Release note must be provided'),
        })}
        onSubmit={releaseNote =>
          editReleaseNote(
            selectedReleaseNote.id,
            releaseNote as EditFormValues,
          ).then(() => {
            setEditFormOpen(false);
          })
        }
      >
        {form => {
          return (
            <Form {...form} id={formId}>
              <FormFieldset
                id="allFieldsFieldset"
                legend="Edit release note"
                legendSize="m"
              >
                <FormFieldDateInput<EditFormValues>
                  id={`${formId}-on`}
                  name="on"
                  legend="Edit date"
                  legendSize="s"
                />
                <FormFieldTextArea<EditFormValues>
                  id="reason"
                  label="Edit release note"
                  name="reason"
                  rows={3}
                />
              </FormFieldset>
              <Button
                type="submit"
                className="govuk-button govuk-!-margin-right-1"
              >
                Update
              </Button>
              <ButtonText
                className="govuk-button govuk-button--secondary"
                onClick={() => {
                  form.resetForm();
                  setEditFormOpen(false);
                }}
              >
                Cancel
              </ButtonText>
            </Form>
          );
        }}
      </Formik>
    );
  };

  return (
    releaseNotes && (
      <>
        <dl className="dfe-meta-content">
          <dt className="govuk-caption-m">Last updated:</dt>
          <dd data-testid="last-updated">
            <strong>
              <FormattedDate>{releaseNotes[0]?.on}</FormattedDate>
            </strong>
            <Details
              onToggle={(open: boolean) =>
                open &&
                logEvent(
                  'Last Updates',
                  'Release page last updates dropdown opened',
                  window.location.pathname,
                )
              }
              summary={`See all ${releaseNotes.length} updates`}
            >
              {releaseNotes.map((elem, index) => (
                <React.Fragment key={elem.id}>
                  {isEditing &&
                  editFormOpen &&
                  selectedReleaseNote.id === elem.id ? (
                    renderEditForm()
                  ) : (
                    <div data-testid="last-updated-element" key={elem.id}>
                      <FormattedDate className="govuk-body govuk-!-font-weight-bold">
                        {elem.on}
                      </FormattedDate>
                      <p>{elem.reason}</p>

                      {isEditing && (
                        <>
                          <Button
                            className="govuk-button govuk-button--secondary govuk-!-margin-bottom-0 govuk-!-margin-right-6"
                            onClick={() => openEditForm(elem)}
                          >
                            Edit
                          </Button>
                          <ButtonText
                            className="govuk-button govuk-button--warning"
                            onClick={() => setDeletedReleaseNote(elem)}
                          >
                            Remove
                          </ButtonText>
                        </>
                      )}
                    </div>
                  )}
                  {index < releaseNotes.length - 1 && <hr />}
                </React.Fragment>
              ))}
            </Details>
          </dd>
          {isEditing && renderAddForm()}
        </dl>
        <ModalConfirm
          mounted={deletedReleaseNote.id.length > 0}
          title="Confirm deletion of release note"
          onExit={() => setDeletedReleaseNote(emptyReleaseNote)}
          onCancel={() => setDeletedReleaseNote(emptyReleaseNote)}
          onConfirm={async () => {
            await releaseNoteService
              .delete(deletedReleaseNote.id, release.id)
              .then(setReleaseNotes)
              .finally(() => setDeletedReleaseNote(emptyReleaseNote));
          }}
        >
          <p>This release note will be removed from this release</p>
        </ModalConfirm>
      </>
    )
  );
};

export default ReleaseNotesSection;
