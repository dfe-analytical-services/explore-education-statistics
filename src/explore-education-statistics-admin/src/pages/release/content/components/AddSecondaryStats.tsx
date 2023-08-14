import { useEditingContext } from '@admin/contexts/EditingContext';
import DataBlockSelectForm from '@admin/pages/release/content/components/DataBlockSelectForm';
import useReleaseContentActions from '@admin/pages/release/content/contexts/useReleaseContentActions';
import { EditableRelease } from '@admin/services/releaseContentService';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import ModalConfirm from '@common/components/ModalConfirm';
import React, { useState } from 'react';

interface Props {
  release: EditableRelease;
  updating?: boolean;
}

const AddSecondaryStats = ({ release, updating = false }: Props) => {
  const [isFormOpen, setIsFormOpen] = useState<boolean>(false);
  const [showConfirmation, setShowConfirmation] = useState<boolean>(false);
  const { editingMode } = useEditingContext();

  const { attachContentSectionBlock, deleteContentSectionBlock } =
    useReleaseContentActions();

  if (editingMode !== 'edit') {
    return null;
  }

  if (!isFormOpen) {
    return (
      <ButtonGroup>
        <Button
          className="govuk-!-margin-top-4 govuk-!-margin-bottom-4"
          onClick={() => {
            setIsFormOpen(true);
          }}
        >
          {`${updating ? 'Change' : 'Add'} secondary stats`}
        </Button>
        {updating && (
          <Button
            className="govuk-!-margin-top-4 govuk-!-margin-bottom-4 govuk-button--warning"
            onClick={() => {
              setShowConfirmation(true);
            }}
          >
            Remove secondary stats
          </Button>
        )}

        <ModalConfirm
          onConfirm={async () => {
            await Promise.all(
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
          open={showConfirmation}
        >
          <p>
            Are you sure you want to remove this this secondary stats section?
          </p>
        </ModalConfirm>
      </ButtonGroup>
    );
  }

  return (
    <DataBlockSelectForm
      id="secondaryStats-dataBlockSelectForm"
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
  );
};

export default AddSecondaryStats;
