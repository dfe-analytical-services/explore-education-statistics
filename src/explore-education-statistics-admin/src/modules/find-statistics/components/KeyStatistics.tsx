import { ErrorControlContext } from '@admin/components/ErrorBoundary';
import {
  attachContentSectionBlock,
  deleteContentSectionBlock,
  updateContentSectionDataBlock,
} from '@admin/pages/release/edit-release/content/helpers';
import { useReleaseDispatch } from '@admin/pages/release/edit-release/content/ReleaseContext';
import ManageReleaseContext, {
  ManageRelease,
} from '@admin/pages/release/ManageReleaseContext';
import { EditableContentBlock } from '@admin/services/publicationService';
import Button from '@common/components/Button';
import styles from '@common/modules/find-statistics/components/SummaryRenderer.module.scss';
import {
  AbstractRelease,
  Publication,
} from '@common/services/publicationService';
import React, { useContext, useEffect, useState } from 'react';
import EditableKeyStatTile from './EditableKeyStatTile';
import KeyIndicatorSelectForm from './KeyIndicatorSelectForm';

export interface KeyStatisticsProps {
  release: AbstractRelease<EditableContentBlock, Publication>;
  isEditing?: boolean;
}

const KeyStatistics = ({ release, isEditing }: KeyStatisticsProps) => {
  const { releaseId } = useContext(ManageReleaseContext) as ManageRelease;
  const { handleApiErrors } = useContext(ErrorControlContext);
  const dispatch = useReleaseDispatch();

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
      {isEditing && <AddKeyStatistics release={release} />}
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
                    deleteContentSectionBlock(
                      dispatch,
                      releaseId,
                      release.keyStatisticsSection.id as string,
                      stat.id,
                      'keyStatisticsSection',
                      handleApiErrors,
                    );
                  }}
                  onSubmit={values =>
                    updateContentSectionDataBlock(
                      dispatch,
                      releaseId,
                      release.keyStatisticsSection.id as string,
                      stat.id,
                      'keyStatisticsSection',
                      values,
                      handleApiErrors,
                    )
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

  const { releaseId } = useContext(ManageReleaseContext) as ManageRelease;
  const { handleApiErrors } = useContext(ErrorControlContext);
  const dispatch = useReleaseDispatch();

  const another =
    release.keyStatisticsSection.content &&
    release.keyStatisticsSection.content.length > 0 &&
    ' another ';

  return (
    <>
      {isFormOpen ? (
        <KeyIndicatorSelectForm
          onSelect={async datablockId => {
            attachContentSectionBlock(
              dispatch,
              releaseId,
              release.keyStatisticsSection.id,
              'keyStatisticsSection',
              {
                contentBlockId: datablockId,
                order:
                  (release.keyStatisticsSection.content &&
                    release.keyStatisticsSection.content.length) ||
                  0,
              },
              handleApiErrors,
            );
            setIsFormOpen(false);
          }}
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
