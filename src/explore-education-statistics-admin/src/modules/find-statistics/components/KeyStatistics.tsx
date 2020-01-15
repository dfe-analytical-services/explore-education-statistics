import React, { useEffect, useState } from 'react';
import {
  AbstractRelease,
  Publication,
} from '@common/services/publicationService';
import { EditableContentBlock } from '@admin/services/publicationService';
import Button from '@common/components/Button';
import KeyIndicatorSelectForm from './KeyIndicatorSelectForm';

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
      {keyStats.content &&
        keyStats.content.map(stat => <div key={stat.id}>{stat.heading}</div>)}
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
    !!release.keyStatisticsSection.content.length &&
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
