import ReleaseNoteForm, {
  ReleaseNoteFormValues,
} from '@admin/pages/release/content/components/ReleaseNoteForm';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import FormattedDate from '@common/components/FormattedDate';
import ModalConfirm from '@common/components/ModalConfirm';
import useToggle from '@common/hooks/useToggle';
import { ReleaseNote as ReleaseNoteData } from '@common/services/publicationService';
import React from 'react';

interface Props {
  isEditable: boolean;
  releaseNote: ReleaseNoteData;
  onDelete: (id: string) => void;
  onSubmit: (id: string, values: ReleaseNoteFormValues) => Promise<void> | void;
}

export default function ReleaseNote({
  isEditable,
  releaseNote,
  onDelete,
  onSubmit,
}: Props) {
  const [editFormOpen, toggleEditForm] = useToggle(false);

  return (
    <li>
      {editFormOpen ? (
        <ReleaseNoteForm
          id={`edit-release-note-form-${releaseNote.id}`}
          initialValues={{
            on: new Date(releaseNote.on),
            reason: releaseNote.reason,
          }}
          onCancel={toggleEditForm.off}
          onSubmit={async values => {
            await onSubmit(releaseNote.id, values);
            toggleEditForm.off();
          }}
        />
      ) : (
        <>
          <FormattedDate className="govuk-body govuk-!-font-weight-bold">
            {releaseNote.on}
          </FormattedDate>
          <p>{releaseNote.reason}</p>

          {isEditable && (
            <ButtonGroup>
              <Button variant="secondary" onClick={toggleEditForm.on}>
                Edit note
              </Button>

              <ModalConfirm
                title="Confirm deletion of release note"
                triggerButton={<Button variant="warning">Remove note</Button>}
                onConfirm={() => onDelete(releaseNote.id)}
              >
                <p>This release note will be removed from this release</p>
              </ModalConfirm>
            </ButtonGroup>
          )}
        </>
      )}
    </li>
  );
}
