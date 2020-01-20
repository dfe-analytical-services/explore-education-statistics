import { EditableContentBlock } from '@admin/services/publicationService';
import { releaseContentService } from '@admin/services/release/edit-release/content/service';
import Button from '@common/components/Button';
import styles from '@common/modules/find-statistics/components/SummaryRenderer.module.scss';
import { EditingContext } from '@common/modules/find-statistics/util/wrapEditableComponent';
import {
  AbstractRelease,
  Publication,
} from '@common/services/publicationService';
import React, { useContext, useEffect, useState } from 'react';
import KeyIndicatorSelectForm from './KeyIndicatorSelectForm';
import KeyStat from './KeyStat';

interface Props {
  release: AbstractRelease<EditableContentBlock, Publication>;
  setRelease: (
    newRelease: AbstractRelease<EditableContentBlock, Publication>,
  ) => void;
  isEditing?: boolean;
}

const KeyStatistics = ({ release, setRelease, isEditing }: Props) => {
  const [keyStats, setKeyStats] = useState<
    | AbstractRelease<EditableContentBlock, Publication>['keyStatisticsSection']
    | undefined
  >(undefined);

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
      <div className={styles.keyStatsContainer}>
        {keyStats.content &&
          keyStats.content.map(stat => <KeyStat key={stat.id} {...stat} />)}
      </div>
      {isEditing && (
        <AddKeyStatistics release={release} setRelease={setRelease} />
      )}
    </>
  );
};

const AddKeyStatistics = ({ release, setRelease }: Props) => {
  const { updateAvailableDataBlocks } = useContext(EditingContext);
  const [isFormOpen, setIsFormOpen] = useState<boolean>(false);

  const another =
    release.keyStatisticsSection.content &&
    release.keyStatisticsSection.content.length > 0 &&
    ' another ';
  return (
    <>
      {isFormOpen && (
        <KeyIndicatorSelectForm
          onSelect={async datablockId => {
            if (
              release.keyStatisticsSection &&
              release.keyStatisticsSection.id
            ) {
              await releaseContentService
                .attachContentSectionBlock(
                  release.id,
                  release.keyStatisticsSection &&
                    release.keyStatisticsSection.id,
                  {
                    contentBlockId: datablockId,
                    order:
                      (release.keyStatisticsSection.content &&
                        release.keyStatisticsSection.content.length) ||
                      0,
                  },
                )
                .then(v => {
                  if (updateAvailableDataBlocks) {
                    updateAvailableDataBlocks();
                  }
                  return v;
                });

              const keyStatisticsSection = await releaseContentService.getContentSection(
                release.id,
                release.keyStatisticsSection.id,
              );
              if (keyStatisticsSection) {
                setRelease({
                  ...release,
                  keyStatisticsSection,
                });
                setIsFormOpen(false);
              }
            }
          }}
          onCancel={() => setIsFormOpen(false)}
        />
      )}
      {!isFormOpen && (
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
