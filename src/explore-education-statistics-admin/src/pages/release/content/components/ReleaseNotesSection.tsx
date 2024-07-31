import { useEditingContext } from '@admin/contexts/EditingContext';
import ReleaseNoteForm, {
  ReleaseNoteFormValues,
} from '@admin/pages/release/content/components/ReleaseNoteForm';
import ReleaseNote from '@admin/pages/release/content/components/ReleaseNote';
import { EditableReleaseVersion } from '@admin/services/releaseContentService';
import releaseNoteService from '@admin/services/releaseNoteService';
import Button from '@common/components/Button';
import Details from '@common/components/Details';
import useToggle from '@common/hooks/useToggle';
import { ReleaseNote as ReleaseNoteData } from '@common/services/publicationService';
import React, { useState } from 'react';

interface Props {
  release: EditableReleaseVersion;
}

export default function ReleaseNotesSection({ release }: Props) {
  const [addFormOpen, toggleAddForm] = useToggle(false);
  const [releaseNotes, setReleaseNotes] = useState<ReleaseNoteData[]>(
    release.updates,
  );
  const { editingMode } = useEditingContext();

  const handleAddReleaseNote = async (values: ReleaseNoteFormValues) => {
    const updatedReleaseNotes = await releaseNoteService.create(
      release.id,
      values,
    );
    setReleaseNotes(updatedReleaseNotes);
    toggleAddForm.off();
  };

  const handleEditReleaseNote = async (
    id: string,
    values: ReleaseNoteFormValues,
  ) => {
    if (!values.on) {
      return;
    }
    const updatedReleaseNotes = await releaseNoteService.edit(id, release.id, {
      on: values.on,
      reason: values.reason,
    });

    setReleaseNotes(updatedReleaseNotes);
  };

  const handleDeleteReleaseNote = async (id: string) => {
    const updatedReleaseNotes = await releaseNoteService.delete(id, release.id);
    setReleaseNotes(updatedReleaseNotes);
  };

  return (
    releaseNotes && (
      <Details
        summary={`See all updates (${releaseNotes.length})`}
        id="release-notes"
        open={editingMode === 'edit'}
      >
        <ol className="govuk-list">
          {releaseNotes.map(releaseNote => (
            <ReleaseNote
              key={releaseNote.id}
              isEditable={editingMode === 'edit'}
              releaseNote={releaseNote}
              onDelete={handleDeleteReleaseNote}
              onSubmit={handleEditReleaseNote}
            />
          ))}
        </ol>
        {editingMode === 'edit' && (
          <>
            {addFormOpen ? (
              <ReleaseNoteForm
                id="create-release-note-form"
                initialValues={{ reason: '' }}
                onCancel={toggleAddForm.off}
                onSubmit={handleAddReleaseNote}
              />
            ) : (
              <Button onClick={toggleAddForm.on}>Add note</Button>
            )}
          </>
        )}
      </Details>
    )
  );
}
