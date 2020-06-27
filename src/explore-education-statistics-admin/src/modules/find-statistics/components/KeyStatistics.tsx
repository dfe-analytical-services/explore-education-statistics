import EditableKeyStatTile from '@admin/components/editable/EditableKeyStatTile';
import useReleaseActions from '@admin/pages/release/edit-release/content/useReleaseActions';
import { EditableContentBlock } from '@admin/services/types/content';
import Button from '@common/components/Button';
import WarningMessage from '@common/components/WarningMessage';
import {
  KeyStatTileColumn,
  KeyStatTileContainer,
} from '@common/modules/find-statistics/components/KeyStatTile';
import { Release } from '@common/services/publicationService';
import React, { useCallback, useState } from 'react';
import KeyStatSelectForm from './KeyStatSelectForm';

export interface KeyStatisticsProps {
  release: Release<EditableContentBlock>;
  isEditing?: boolean;
}

const KeyStatistics = ({ release, isEditing }: KeyStatisticsProps) => {
  const {
    deleteContentSectionBlock,
    updateContentSectionDataBlock,
  } = useReleaseActions();

  return (
    <>
      {isEditing && (
        <>
          <WarningMessage>
            In order to add a key statistic you first need to create a data
            block with just one value.
            <br />
            Any data blocks with more than one value cannot be selected as a key
            statistic.
          </WarningMessage>
          <AddKeyStatistics release={release} />
        </>
      )}
      <KeyStatTileContainer>
        {release.keyStatisticsSection.content
          .filter(block => block.type === 'DataBlock')
          .map(block => (
            <KeyStatTileColumn key={block.id}>
              <EditableKeyStatTile
                id={block.id}
                name={block.name}
                query={block.dataBlockRequest}
                summary={block.summary}
                isEditing={isEditing}
                onRemove={async () => {
                  await deleteContentSectionBlock({
                    releaseId: release.id,
                    sectionId: release.keyStatisticsSection.id,
                    blockId: block.id,
                    sectionKey: 'keyStatisticsSection',
                  });
                }}
                onSubmit={async values => {
                  await updateContentSectionDataBlock({
                    releaseId: release.id,
                    sectionId: release.keyStatisticsSection.id,
                    blockId: block.id,
                    sectionKey: 'keyStatisticsSection',
                    values,
                  });
                }}
              />
            </KeyStatTileColumn>
          ))}
      </KeyStatTileContainer>
    </>
  );
};

const AddKeyStatistics = ({ release }: KeyStatisticsProps) => {
  const [isFormOpen, setIsFormOpen] = useState<boolean>(false);
  const { attachContentSectionBlock } = useReleaseActions();

  const { keyStatisticsSection } = release;

  const addKeyStatToSection = useCallback(
    async (dataBlockId: string) => {
      await attachContentSectionBlock({
        releaseId: release.id,
        sectionId: release.keyStatisticsSection.id,
        sectionKey: 'keyStatisticsSection',
        block: {
          contentBlockId: dataBlockId,
          order: release.keyStatisticsSection.content.length || 0,
        },
      });
      setIsFormOpen(false);
    },
    [release.id, release.keyStatisticsSection, attachContentSectionBlock],
  );

  return (
    <>
      {isFormOpen ? (
        <KeyStatSelectForm
          onSelect={addKeyStatToSection}
          onCancel={() => setIsFormOpen(false)}
        />
      ) : (
        <Button
          onClick={() => {
            setIsFormOpen(true);
          }}
        >
          {`Add ${
            keyStatisticsSection.content.length > 0 ? ' another ' : ''
          } key statistic`}
        </Button>
      )}
    </>
  );
};

export default KeyStatistics;
