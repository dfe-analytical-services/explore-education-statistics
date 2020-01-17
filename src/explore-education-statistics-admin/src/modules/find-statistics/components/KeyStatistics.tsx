import React, { useEffect, useState } from 'react';
import {
  AbstractRelease,
  Publication,
} from '@common/services/publicationService';
import { EditableContentBlock } from '@admin/services/publicationService';
import Button from '@common/components/Button';
import styles from '@common/modules/find-statistics/components/SummaryRenderer.module.scss';
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
  const [isAddingStat, setIsAddingStat] = useState<boolean>(false);

  const another =
    release.keyStatisticsSection.content &&
    release.keyStatisticsSection.content.length > 0 &&
    ' another ';
  return (
    <>
      {isAddingStat && (
        <KeyIndicatorSelectForm
          onSelect={datablock => console.log(datablock)}
          onCancel={() => setIsAddingStat(false)}
        />
      )}
      {!isAddingStat && (
        <Button
          onClick={() => {
            setIsAddingStat(true);
          }}
        >
          Add {another} key statistic
        </Button>
      )}
    </>
  );
};

export default KeyStatistics;
