import { useEditingContext } from '@admin/contexts/EditingContext';
import useReleaseActions from '@admin/pages/release/edit-release/content/useReleaseActions';
import { EditableRelease } from '@admin/services/releaseContentService';
import Button from '@common/components/Button';
import ModalConfirm from '@common/components/ModalConfirm';
import React, { useState } from 'react';
import DataBlockSelectForm from './DataBlockSelectForm';

interface Props {
  release: EditableRelease;
  isEditing?: boolean;
  updating?: boolean;
}

const AddSecondaryStats = ({ release, updating = false }: Props) => {
  const [isFormOpen, setIsFormOpen] = useState<boolean>(false);
  const [showConfirmation, setShowConfirmation] = useState<boolean>(false);
  const { isEditing } = useEditingContext();

  const {
    attachContentSectionBlock,
    deleteContentSectionBlock,
  } = useReleaseActions();

  if (!isEditing) {
    return null;
  }

  if (!isFormOpen) {
    return (
      <>
        <Button
          className="govuk-!-margin-top-4 govuk-!-margin-bottom-4"
          onClick={() => {
            setIsFormOpen(true);
          }}
        >
          {updating ? 'Change' : 'Add'} Secondary Stats
        </Button>
        {updating && (
          <Button
            className="govuk-!-margin-top-4 govuk-!-margin-bottom-4 govuk-button--warning"
            onClick={() => {
              setShowConfirmation(true);
            }}
          >
            Remove Secondary Stats
          </Button>
        )}

        <ModalConfirm
          onConfirm={() => {
            Promise.all(
              release.keyStatisticsSecondarySection.content.map(
                async content => {
                  if (release.keyStatisticsSecondarySection?.content) {
                    await deleteContentSectionBlock({
                      releaseId: release.id,
                      sectionId: release.keyStatisticsSecondarySection.id,
                      blockId: content.id,
                      sectionKey: 'keyStatisticsSecondarySection',
                    });
                  }
                },
              ),
            );
            setShowConfirmation(false);
          }}
          onExit={() => {
            setShowConfirmation(false);
          }}
          onCancel={() => {
            setShowConfirmation(false);
          }}
          title="Remove secondary statistics section"
          mounted={showConfirmation}
        >
          <p>
            Are you sure you want to remove this this secondary stats section?
          </p>
        </ModalConfirm>
      </>
    );
  }

  return (
    <>
      <DataBlockSelectForm
        releaseId={release.id}
        label="Select a data block to show alongside the headline facts and figures as secondary headline statistics."
        onSelect={async selectedDataBlockId => {
          await Promise.all(
            release.keyStatisticsSecondarySection.content.map(async content => {
              await deleteContentSectionBlock({
                releaseId: release.id,
                sectionId: release.keyStatisticsSecondarySection.id,
                blockId: content.id,
                sectionKey: 'keyStatisticsSecondarySection',
              });
            }),
          );

          await attachContentSectionBlock({
            releaseId: release.id,
            sectionId: release.keyStatisticsSecondarySection.id,
            sectionKey: 'keyStatisticsSecondarySection',
            block: {
              contentBlockId: selectedDataBlockId,
              order: 0,
            },
          });

          setIsFormOpen(false);
        }}
        onCancel={() => {
          setIsFormOpen(false);
        }}
      />
    </>
  );
};

export default AddSecondaryStats;
