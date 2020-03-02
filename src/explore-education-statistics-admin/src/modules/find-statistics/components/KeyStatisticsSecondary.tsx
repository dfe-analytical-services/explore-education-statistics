import { ErrorControlContext } from '@admin/components/ErrorBoundary';
import {
  deleteContentSectionBlock,
  attachContentSectionBlock,
} from '@admin/pages/release/edit-release/content/helpers';
import { useReleaseDispatch } from '@admin/pages/release/edit-release/content/ReleaseContext';
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
  const [isFormOpen, setIsFormOpen] = useState<boolean>(false);
  const [showConfirmation, setShowConfirmation] = useState<boolean>(false);

  const { isEditing } = useContext(EditingContext);
  const { handleApiErrors } = useContext(ErrorControlContext);
  const dispatch = useReleaseDispatch();

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
          onConfirm={() => {
            if (release.keyStatisticsSecondarySection?.content) {
              release.keyStatisticsSecondarySection.content.forEach(
                (content: EditableContentBlock) => {
                  deleteContentSectionBlock(
                    dispatch,
                    release.id,
                    // @ts-ignore will be defined, due to above if statement
                    release.keyStatisticsSecondarySection.id,
                    content.id,
                    'keyStatisticsSecondarySection',
                    handleApiErrors,
                  );
                },
              );
            }
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
            if (release.keyStatisticsSecondarySection?.content) {
              await Promise.all(
                release.keyStatisticsSecondarySection.content.map(
                  async (content: EditableContentBlock) => {
                    const response = await deleteContentSectionBlock(
                      dispatch,
                      release.id,
                      // @ts-ignore will be defined, due to above if statement
                      release.keyStatisticsSecondarySection.id,
                      content.id,
                      'keyStatisticsSecondarySection',
                      handleApiErrors,
                    );
                    return response;
                  },
                ),
              );
            }
            await attachContentSectionBlock(
              dispatch,
              release.id,
              release.keyStatisticsSecondarySection.id,
              'keyStatisticsSecondarySection',
              {
                contentBlockId: selectedDataBlockId,
                order: 0,
              },
            );
            setIsFormOpen(false);
          }
        }}
        onCancel={() => {
          setIsFormOpen(false);
        }}
      />
    </>
  );
};
