import { ReleaseNote } from '@common/services/publicationService';
import React, { useState, useContext } from 'react';
import { FormikProps } from 'formik';
import { ManageContentPageViewModel } from '@admin/services/release/edit-release/content/types';
import Details from '@common/components/Details';
import FormattedDate from '@common/components/FormattedDate';
import Link from '@admin/components/Link';
import releaseNoteService from '@admin/services/release/edit-release/content/service';
import { EditingContext } from '@common/modules/find-statistics/util/wrapEditableComponent';
import Button from '@common/components/Button';
import {
  Formik,
  FormFieldset,
  Form,
  FormFieldTextInput,
} from '@common/components/form';
import Yup from '@common/lib/validation/yup';

interface Props {
  release: ManageContentPageViewModel['release'];
  logEvent?: (...params: string[]) => void;
}

const nullLogEvent = () => {};

const ReleaseNotesSection = ({ release, logEvent = nullLogEvent }: Props) => {
  const [releaseNotes, setReleaseNotes] = useState<ReleaseNote[]>(
    release.updates,
  );
  const [formOpen, setFormOpen] = useState<boolean>(false);

  const { isEditing } = useContext(EditingContext);

  const addReleaseNote = (releaseNote: Omit<ReleaseNote, 'id'>) => {
    return new Promise(resolve => {
      releaseNoteService.releaseNote
        .create(release.id, releaseNote)
        .then(newReleaseNotes => {
          setReleaseNotes(newReleaseNotes);
          resolve();
        });
    });
  };

  const removeReleaseNote = (releaseNoteId: string) => {
    releaseNoteService.releaseNote
      .delete(release.id, releaseNoteId)
      .then(setReleaseNotes);
  };

  const renderAddForm = () => {
    return !formOpen ? (
      <Button onClick={() => setFormOpen(true)}>Add note</Button>
    ) : (
      <Formik<Omit<ReleaseNote, 'id'>>
        initialValues={{ reason: '', releaseId: '', on: '' }}
        validationSchema={Yup.object({
          description: Yup.string().required('Release note must be provided'),
        })}
        onSubmit={releaseNote =>
          addReleaseNote(releaseNote).then(() => {
            setFormOpen(false);
          })
        }
        render={(form: FormikProps<Omit<ReleaseNote, 'id'>>) => {
          return (
            <Form {...form} id="create-new-release-note-form">
              <FormFieldset
                id="allFieldsFieldset"
                legend="Add new release note"
                legendSize="m"
              >
                <FormFieldTextInput
                  id="reason"
                  label="Release Note"
                  name="reason"
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
                  setFormOpen(false);
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
          {releaseNotes.map((elem, index) => (
            <div data-testid="last-updated-element" key={elem.on}>
              <FormattedDate className="govuk-body govuk-!-font-weight-bold">
                {elem.on}
              </FormattedDate>
              <p>{elem.reason}</p>

              {isEditing && (
                <>
                  <Link
                    to="#"
                    className="govuk-button govuk-button--secondary govuk-!-margin-right-6"
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
          ))}
        </Details>
      </dd>
      {isEditing && renderAddForm()}
    </dl>
  );
};

export default ReleaseNotesSection;
