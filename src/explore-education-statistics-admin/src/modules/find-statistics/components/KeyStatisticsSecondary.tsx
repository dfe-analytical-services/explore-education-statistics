import React, { useContext, useState } from 'react';
import {
  AbstractRelease,
  Publication,
  ContentSection,
} from '@common/services/publicationService';
import TabsSection from '@common/components/TabsSection';
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

function renderSecondaryStats({
  release,
  setRelease,
  isEditing = false,
}: Props): JSX.Element[] {
  const { keyStatisticsSecondarySection } = release;
  if (!hasSecondaryStats(keyStatisticsSecondarySection)) {
    return [];
  }

  // @ts-ignore - it will not be undefined due to above check
  const secondaryStatsDatablock = keyStatisticsSecondarySection.content[0];

  return [
    <TabsSection key="table" id="headline-secondary-table" title="Table">
      <p>Table</p>

      {
        //@ts-ignore
        secondaryStatsDatablock.name
      }
    </TabsSection>,
    <TabsSection key="chart" id="headline-secondary-Chart" title="Chart">
      Chart
    </TabsSection>,
  ];
}

function getKeyStatisticsSecondaryTabs({
  release,
  setRelease,
  isEditing = false,
}: Props): JSX.Element[] {
  //returns a set of tabs based on what has previously added to the release
  const { keyStatisticsSecondarySection } = release;

  return hasSecondaryStats(keyStatisticsSecondarySection)
    ? renderSecondaryStats({ release, setRelease, isEditing })
    : [];
}

export const AddSecondaryStats = ({
  release,
  setRelease,
  updating = false,
}: Props) => {
  const {
    isEditing,
    availableDataBlocks,
    updateAvailableDataBlocks,
  } = useContext(EditingContext);
  const [isPicking, setIsPicking] = useState<boolean>(false);
  const [showConfirmation, setShowConfirmation] = useState<boolean>(false);

  async function removeSecondarySectionBlock() {
    return new Promise(async (resolve, reject) => {
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
                //@ts-ignore
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
  if (!isPicking)
    return (
      <>
        <Button
          className="govuk-!-margin-top-4 govuk-!-margin-bottom-4"
          onClick={() => {
            setIsPicking(true);
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
              setIsPicking(false);
            }
          }
        }}
        onCancel={() => {
          setIsPicking(false);
        }}
      />
    </>
  );
};

export default getKeyStatisticsSecondaryTabs;
