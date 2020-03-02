import { EditableContentBlock } from '@admin/services/publicationService';
import { releaseContentService } from '@admin/services/release/edit-release/content/service';
import Button from '@common/components/Button';
import ModalConfirm from '@common/components/ModalConfirm';
import { EditingContext } from '@common/modules/find-statistics/util/wrapEditableComponent';
import {
  AbstractRelease,
  ContentSection,
  Publication,
} from '@common/services/publicationService';
import React, { useContext, useState } from 'react';
import DatablockSelectForm from './DatablockSelectForm';

interface Props {
  release: AbstractRelease<EditableContentBlock, Publication>;
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

export const AddSecondaryStats = ({ release, updating = false }: Props) => {
  const { isEditing } = useContext(EditingContext);
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
        label="Select a data block to show alongside the headline facts and figures as secondary headline statistics."
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
                return v;
              });
            const keyStatisticsSecondarySection = await releaseContentService.getContentSection(
              release.id,
              release.keyStatisticsSecondarySection.id,
            );
            if (keyStatisticsSecondarySection) {
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
