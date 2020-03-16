import EditableKeyStatTile from '@admin/components/editable/EditableKeyStatTile';
import useReleaseActions from '@admin/pages/release/edit-release/content/useReleaseActions';
import { useManageReleaseContext } from '@admin/pages/release/ManageReleaseContext';
import { EditableContentBlock } from '@admin/services/publicationService';
import Button from '@common/components/Button';
import WarningMessage from '@common/components/WarningMessage';
import styles from '@common/modules/find-statistics/components/SummaryRenderer.module.scss';
import {
  AbstractRelease,
  Publication,
} from '@common/services/publicationService';
import React, { useCallback, useEffect, useState } from 'react';
import KeyIndicatorSelectForm from './KeyIndicatorSelectForm';

export interface KeyStatisticsProps {
  release: AbstractRelease<EditableContentBlock, Publication>;
  isEditing?: boolean;
}

const KeyStatistics = ({ release, isEditing }: KeyStatisticsProps) => {
  const { releaseId } = useManageReleaseContext();
  const {
    deleteContentSectionBlock,
    updateContentSectionDataBlock,
  } = useReleaseActions();

  const [keyStats, setKeyStats] = useState<
    AbstractRelease<EditableContentBlock, Publication>['keyStatisticsSection']
  >();

  useEffect(() => {
    if (release.keyStatisticsSection) {
      setKeyStats(release.keyStatisticsSection);
    } else {
      setKeyStats(undefined);
    }
  }, [release]);

  if (!keyStats) return null;
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
        {keyStats.content &&
          keyStats.content.map(stat => {
            if (stat.dataBlockRequest !== undefined) {
              return stat.type === 'DataBlock' && stat.dataBlockRequest ? (
                // @ts-ignore
                <EditableKeyStatTile
                  key={stat.id}
                  {...stat}
                  isEditing={isEditing}
                  onRemove={() => {
                    deleteContentSectionBlock({
                      releaseId,
                      sectionId: release.keyStatisticsSection.id as string,
                      blockId: stat.id,
                      sectionKey: 'keyStatisticsSection',
                    });
                  }}
                  onSubmit={values =>
                    updateContentSectionDataBlock({
                      releaseId,
                      sectionId: release.keyStatisticsSection.id as string,
                      blockId: stat.id,
                      sectionKey: 'keyStatisticsSection',
                      values,
                    })
                  }
                />
              ) : null;
            }
            return null;
          })}
      </div>
    </>
  );
};

const AddKeyStatistics = ({ release }: KeyStatisticsProps) => {
  const [isFormOpen, setIsFormOpen] = useState<boolean>(false);
  const { attachContentSectionBlock } = useReleaseActions();

  const another =
    release.keyStatisticsSection.content &&
    release.keyStatisticsSection.content.length > 0 &&
    ' another ';

  const addKeyStatToSection = useCallback(
    (datablockId: string) => {
      attachContentSectionBlock({
        releaseId: release.id,
        sectionId: release.keyStatisticsSection.id,
        sectionKey: 'keyStatisticsSection',
        block: {
          contentBlockId: datablockId,
          order:
            (release.keyStatisticsSection.content &&
              release.keyStatisticsSection.content.length) ||
            0,
        },
      });
      setIsFormOpen(false);
    },
    [release.id, release.keyStatisticsSection, attachContentSectionBlock],
  );

  return (
    <>
      {isFormOpen ? (
        <KeyIndicatorSelectForm
          onSelect={addKeyStatToSection}
          onCancel={() => setIsFormOpen(false)}
        />
      ) : (
        <Button
          onClick={() => {
            setIsFormOpen(true);
          }}
        >
          Add {another} key statistic
        </Button>
      )}
    </>
  );
};

export default KeyStatistics;
