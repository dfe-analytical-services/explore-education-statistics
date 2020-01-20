import React, { useContext, useState } from 'react';
import {
  AbstractRelease,
  Publication,
  ContentSection,
} from '@common/services/publicationService';
import { EditableContentBlock } from '@admin/services/publicationService';
import Button from '@common/components/Button';
import { EditingContext } from '@common/modules/find-statistics/util/wrapEditableComponent';
import { releaseContentService } from '@admin/services/release/edit-release/content/service';
import ModalConfirm from '@common/components/ModalConfirm';
import DatablockSelectForm from './DatablockSelectForm';

interface Props {
  release: AbstractRelease<EditableContentBlock, Publication>;
  setRelease: (
    newRelease: AbstractRelease<EditableContentBlock, Publication>,
  ) => void;
  isEditing?: boolean;
  updating?: boolean;
}

export function hasSecondaryStats(
  keyStatisticsSecondarySection:
    | ContentSection<EditableContentBlock>
    | undefined,
) {
  return !!(
    keyStatisticsSecondarySection &&
    keyStatisticsSecondarySection.content &&
    keyStatisticsSecondarySection.content.length
  );
}

export const AddSecondaryStats = ({
  release,
  setRelease,
  updating = false,
}: Props) => {
  const { isEditing, updateAvailableDataBlocks } = useContext(EditingContext);
  const [isFormOpen, setIsFormOpen] = useState<boolean>(false);
  const [showConfirmation, setShowConfirmation] = useState<boolean>(false);

  async function removeSecondarySectionBlock() {
    return new Promise(async resolve => {
      if (
        release.keyStatisticsSecondarySection &&
        release.keyStatisticsSecondarySection.id &&
        release.keyStatisticsSecondarySection.content
      ) {
        await Promise.all(
          release.keyStatisticsSecondarySection.content.map(
            (content: EditableContentBlock) => {
              return releaseContentService.deleteContentSectionBlock(
                release.id,
                // @ts-ignore will be defined, due to above if statement
                release.keyStatisticsSecondarySection.id,
                content.id,
              );
            },
          ),
        ).then(() => {
          if (updateAvailableDataBlocks) {
            updateAvailableDataBlocks();
          }

          setRelease({
            ...release,
            keyStatisticsSecondarySection: release.keyStatisticsSecondarySection
              ? {
                  ...release.keyStatisticsSecondarySection,
                  content: [] as EditableContentBlock[],
                }
              : undefined,
          });
          resolve();
        });
      }
      resolve();
    });
  }

  if (!isEditing) return null;
  if (!isFormOpen)
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
          onConfirm={async () => {
            await removeSecondarySectionBlock();
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

  return (
    <>
      <DatablockSelectForm
        label="Select a datablock to show beside the headline facts and figures as secondary statistics."
        onSelect={async selectedDataBlockId => {
          if (
            release.keyStatisticsSecondarySection &&
            release.keyStatisticsSecondarySection.id
          ) {
            await removeSecondarySectionBlock();
            await releaseContentService
              .attachContentSectionBlock(
                release.id,
                release.keyStatisticsSecondarySection &&
                  release.keyStatisticsSecondarySection.id,
                {
                  contentBlockId: selectedDataBlockId,
                  order: 0,
                },
              )
              .then(v => {
                if (updateAvailableDataBlocks) {
                  updateAvailableDataBlocks();
                }
                return v;
              });
            const keyStatisticsSecondarySection = await releaseContentService.getContentSection(
              release.id,
              release.keyStatisticsSecondarySection.id,
            );
            if (keyStatisticsSecondarySection) {
              setRelease({
                ...release,
                keyStatisticsSecondarySection,
              });
              setIsFormOpen(false);
            }
          }
        }}
        onCancel={() => {
          setIsFormOpen(false);
        }}
      />
    </>
  );
};
