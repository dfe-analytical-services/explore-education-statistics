import {
  ReleaseNote,
  DayMonthYearInputs,
  dayMonthYearInputsToDate,
  dateToDayMonthYear,
} from '@common/services/publicationService';
import React, { useState, useContext } from 'react';
import { FormikProps } from 'formik';
import merge from 'lodash/merge';
import { ManageContentPageViewModel } from '@admin/services/release/edit-release/content/types';
import Details from '@common/components/Details';
import FormattedDate from '@common/components/FormattedDate';
import Link from '@admin/components/Link';
import releaseNoteService from '@admin/services/release/edit-release/content/service';
import { EditingContext } from '@common/modules/find-statistics/util/wrapEditableComponent';
import Button from '@common/components/Button';
import { Formik, FormFieldset, Form } from '@common/components/form';
import Yup from '@common/lib/validation/yup';
import FormFieldDayMonthYear from '@common/components/form/FormFieldDayMonthYear';
import { validateMandatoryDayMonthYearField } from '@admin/validation/validation';
import FormFieldTextArea from '@common/components/form/FormFieldTextArea';

interface Props {
  release: ManageContentPageViewModel['release'];
  logEvent?: (...params: string[]) => void;
}

export interface AddFormValues {
  reason: string;
}

export interface EditFormValues {
  on: DayMonthYearInputs;
  reason: string;
}

const nullLogEvent = () => {};

const ReleaseNotesSection = ({ release, logEvent = nullLogEvent }: Props) => {
  const [addFormOpen, setAddFormOpen] = useState<boolean>(false);
  const [editFormOpen, setEditFormOpen] = useState<boolean>(false);
  const [selectedReleaseNote, setSelectedReleaseNote] = useState<ReleaseNote>();
  const [releaseNotes, setReleaseNotes] = useState<ReleaseNote[]>(
    release.updates,
  );
  const { isEditing } = useContext(EditingContext);

  const addReleaseNote = (releaseNote: AddFormValues) => {
    return new Promise(resolve => {
      releaseNoteService.releaseNote
        .create(release.id, releaseNote)
        .then(newReleaseNotes => {
          setReleaseNotes(newReleaseNotes);
          resolve();
        });
    });
  };

  const editReleaseNote = (id: string, releaseNote: EditFormValues) => {
    return new Promise(resolve => {
      releaseNoteService.releaseNote
        .edit(id, release.id, {
          on: dayMonthYearInputsToDate(releaseNote.on),
          reason: releaseNote.reason,
        })
        .then(newReleaseNotes => {
          setReleaseNotes(newReleaseNotes);
          resolve();
        });
    });
  };

  const removeReleaseNote = (releaseNoteId: string) => {
    releaseNoteService.releaseNote
      .delete(releaseNoteId, release.id)
      .then(setReleaseNotes);
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
        render={(form: FormikProps<AddFormValues>) => {
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
              <Link
                to="#"
                className="govuk-button govuk-button--secondary"
                onClick={() => {
                  form.resetForm();
                  setAddFormOpen(false);
                }}
              >
                Cancel
              </Link>
            </Form>
          );
        }}
      />
    );
  };

  const renderEditForm = () => {
    const formId = 'edit-release-note-form';

    // TODO
    if (!selectedReleaseNote) {
      return null;
    }

    const generateInitialValues = () => {
      const initialValue: EditFormValues = {
        on: {
          day: '',
          month: '',
          year: '',
        },
        reason: '',
      };

      return merge(initialValue, {
        on: dateToDayMonthYear(new Date(selectedReleaseNote.on)),
        reason: selectedReleaseNote.reason,
      });
    };

    return (
      <Formik<EditFormValues>
        initialValues={generateInitialValues()}
        validationSchema={Yup.object<EditFormValues>({
          on: validateMandatoryDayMonthYearField,
          reason: Yup.string().required('Release note must be provided'),
        })}
        onSubmit={releaseNote =>
          editReleaseNote(selectedReleaseNote.id, releaseNote).then(() => {
            setEditFormOpen(false);
          })
        }
        render={(form: FormikProps<EditFormValues>) => {
          return (
            <Form {...form} id={formId}>
              <FormFieldset
                id="allFieldsFieldset"
                legend="Edit release note"
                legendSize="m"
              >
                <FormFieldDayMonthYear<EditFormValues>
                  formId={formId}
                  fieldName="on"
                  fieldsetLegend="Edit date"
                  fieldsetLegendSize="s"
                  day={form.values.on.day}
                  month={form.values.on.month}
                  year={form.values.on.year}
                />
                <FormFieldTextArea
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
              <Link
                to="#"
                className="govuk-button govuk-button--secondary"
                onClick={() => {
                  form.resetForm();
                  setEditFormOpen(false);
                }}
              >
                Cancel
              </Link>
            </Form>
          );
        }}
      />
    );
  };

  return (
    <dl className="dfe-meta-content">
      <dt className="govuk-caption-m">Last updated:</dt>
      <dd data-testid="last-updated">
        <strong>
          <FormattedDate>{release.updates[0].on}</FormattedDate>
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
          summary={`See all ${release.updates.length} updates`}
        >
          {releaseNotes.map((elem, index) =>
            isEditing &&
            editFormOpen &&
            selectedReleaseNote &&
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
                    <Link
                      to="#"
                      className="govuk-button govuk-button--secondary govuk-!-margin-right-6"
                      onClick={() => openEditForm(elem)}
                    >
                      Edit
                    </Link>
                    <Link
                      to="#"
                      className="govuk-button govuk-button--warning"
                      onClick={() => removeReleaseNote(elem.id)}
                    >
                      Remove
                    </Link>
                  </>
                )}
                {index < release.updates.length - 1 && <hr />}
              </div>
            ),
          )}
        </Details>
      </dd>
      {isEditing && renderAddForm()}
    </dl>
  );
};

export default ReleaseNotesSection;
