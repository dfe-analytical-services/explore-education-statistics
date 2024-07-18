import { useEditingContext } from '@admin/contexts/EditingContext';
import DataBlockSelectForm from '@admin/pages/release/content/components/DataBlockSelectForm';
import useReleaseContentActions from '@admin/pages/release/content/contexts/useReleaseContentActions';
import { EditableReleaseVersion } from '@admin/services/releaseContentService';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import ModalConfirm from '@common/components/ModalConfirm';
import React, { useState } from 'react';

interface Props {
  releaseVersion: EditableReleaseVersion;
  updating?: boolean;
}

const AddSecondaryStats = ({ releaseVersion, updating = false }: Props) => {
  const [isFormOpen, setIsFormOpen] = useState<boolean>(false);
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
          <ModalConfirm
            title="Remove secondary statistics section"
            triggerButton={
              <Button className="govuk-!-margin-top-4 govuk-!-margin-bottom-4 govuk-button--warning">
                Remove secondary stats
              </Button>
            }
            onConfirm={async () => {
              await Promise.all(
                releaseVersion.keyStatisticsSecondarySection.content.map(
                  async content => {
                    if (releaseVersion.keyStatisticsSecondarySection?.content) {
                      await deleteContentSectionBlock({
                        releaseVersionId: releaseVersion.id,
                        sectionId:
                          releaseVersion.keyStatisticsSecondarySection.id,
                        blockId: content.id,
                        sectionKey: 'keyStatisticsSecondarySection',
                      });
                    }
                  },
                ),
              );
            }}
          >
            <p>
              Are you sure you want to remove this this secondary stats section?
            </p>
          </ModalConfirm>
        )}
      </ButtonGroup>
    );
  }

  return (
    <DataBlockSelectForm
      id="secondaryStats-dataBlockSelectForm"
      releaseVersionId={releaseVersion.id}
      label="Select a data block to show alongside the headline facts and figures as secondary headline statistics."
      onSelect={async selectedDataBlockId => {
        await Promise.all(
          releaseVersion.keyStatisticsSecondarySection.content.map(
            async content => {
              await deleteContentSectionBlock({
                releaseVersionId: releaseVersion.id,
                sectionId: releaseVersion.keyStatisticsSecondarySection.id,
                blockId: content.id,
                sectionKey: 'keyStatisticsSecondarySection',
              });
            },
          ),
        );

        await attachContentSectionBlock({
          releaseVersionId: releaseVersion.id,
          sectionId: releaseVersion.keyStatisticsSecondarySection.id,
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
