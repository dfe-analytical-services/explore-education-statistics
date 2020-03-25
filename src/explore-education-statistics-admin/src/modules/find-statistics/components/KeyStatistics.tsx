import EditableKeyStatTile from '@admin/components/editable/EditableKeyStatTile';
import useReleaseActions from '@admin/pages/release/edit-release/content/useReleaseActions';
import { useManageReleaseContext } from '@admin/pages/release/ManageReleaseContext';
import { EditableContentBlock } from '@admin/services/publicationService';
import Button from '@common/components/Button';
import WarningMessage from '@common/components/WarningMessage';
import styles from '@common/modules/find-statistics/components/SummaryRenderer.module.scss';
import { Release } from '@common/services/publicationService';
import React, { useCallback, useState } from 'react';
import KeyStatSelectForm from './KeyStatSelectForm';

export interface KeyStatisticsProps {
  release: Release<EditableContentBlock>;
  isEditing?: boolean;
}

const KeyStatistics = ({ release, isEditing }: KeyStatisticsProps) => {
  const { releaseId } = useManageReleaseContext();
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
      <div className={styles.keyStatsContainer}>
        {release.keyStatisticsSection.content.map(stat => {
          return stat.type === 'DataBlock' ? (
            <EditableKeyStatTile
              key={stat.id}
              {...stat}
              isEditing={isEditing}
              onRemove={() => {
                deleteContentSectionBlock({
                  releaseId,
                  sectionId: release.keyStatisticsSection.id,
                  blockId: stat.id,
                  sectionKey: 'keyStatisticsSection',
                });
              }}
              onSubmit={values =>
                updateContentSectionDataBlock({
                  releaseId,
                  sectionId: release.keyStatisticsSection.id,
                  blockId: stat.id,
                  sectionKey: 'keyStatisticsSection',
                  values,
                })
              }
            />
          ) : null;
        })}
      </div>
    </>
  );
};

const AddKeyStatistics = ({ release }: KeyStatisticsProps) => {
  const [isFormOpen, setIsFormOpen] = useState<boolean>(false);
  const { attachContentSectionBlock } = useReleaseActions();

  const { keyStatisticsSection } = release;

  const addKeyStatToSection = useCallback(
    (datablockId: string) => {
      attachContentSectionBlock({
        releaseId: release.id,
        sectionId: release.keyStatisticsSection.id,
        sectionKey: 'keyStatisticsSection',
        block: {
          contentBlockId: datablockId,
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
