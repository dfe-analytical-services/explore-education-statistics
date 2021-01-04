import EditableKeyStat from '@admin/components/editable/EditableKeyStat';
import KeyStatSelectForm from '@admin/pages/release/content/components/KeyStatSelectForm';
import useReleaseContentActions from '@admin/pages/release/content/contexts/useReleaseContentActions';
import { EditableRelease } from '@admin/services/releaseContentService';
import Button from '@common/components/Button';
import WarningMessage from '@common/components/WarningMessage';
import { KeyStatContainer } from '@common/modules/find-statistics/components/KeyStat';
import React, { useCallback, useState } from 'react';

export interface KeyStatisticsProps {
  release: EditableRelease;
  isEditing?: boolean;
}

const KeyStatistics = ({ release, isEditing }: KeyStatisticsProps) => {
  const {
    deleteContentSectionBlock,
    updateContentSectionDataBlock,
  } = useReleaseContentActions();

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
      <KeyStatContainer>
        {release.keyStatisticsSection.content
          .filter(block => block.type === 'DataBlock')
          .map(block => (
            <EditableKeyStat
              key={block.id}
              name={block.name}
              releaseId={release.id}
              dataBlockId={block.id}
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
          ))}
      </KeyStatContainer>
    </>
  );
};

const AddKeyStatistics = ({ release }: KeyStatisticsProps) => {
  const [isFormOpen, setIsFormOpen] = useState<boolean>(false);
  const { attachContentSectionBlock } = useReleaseContentActions();

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
          releaseId={release.id}
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
