import EditableKeyStat from '@admin/pages/release/content/components/EditableKeyStat';
import EditableKeyStatDataBlock from '@admin/pages/release/content/components/EditableKeyStatDataBlock';
import AddKeyStatistics from '@admin/pages/release/content/components/AddKeyStatistics';
import useReleaseContentActions from '@admin/pages/release/content/contexts/useReleaseContentActions';
import { EditableRelease } from '@admin/services/releaseContentService';
import Button from '@common/components/Button';
import ReorderableList from '@common/components/ReorderableList';
import useToggle from '@common/hooks/useToggle';
import { KeyStatContainer } from '@common/modules/find-statistics/components/KeyStat';
import keyStatStyles from '@common/modules/find-statistics/components/KeyStat.module.scss';
import reorder from '@common/utils/reorder';
import React, { useEffect, useState } from 'react';

export interface KeyStatisticsProps {
  release: EditableRelease;
}

export default function EditableKeyStatistics({ release }: KeyStatisticsProps) {
  const { reorderKeyStatistics } = useReleaseContentActions();
  const [keyStatistics, setKeyStatistics] = useState(release.keyStatistics);

  useEffect(() => {
    setKeyStatistics(release.keyStatistics);
  }, [release]);

  const [isReordering, toggleIsReordering] = useToggle(false);

  return (
    <>
      <AddKeyStatistics release={release} />
      <hr />
      {keyStatistics.length > 1 && !isReordering && (
        <Button variant="secondary" onClick={toggleIsReordering.on}>
          Reorder
          <span className="govuk-visually-hidden"> key statistics</span>
        </Button>
      )}

      {isReordering ? (
        <ReorderableList
          heading="Reorder key statistics"
          id="reorder-key-statistics"
          list={keyStatistics.map(keyStat => {
            if (keyStat.type === 'KeyStatisticText') {
              return { id: keyStat.id, label: keyStat.title };
            }

            return {
              id: keyStat.id,
              label: (
                <EditableKeyStatDataBlock
                  isReordering
                  keyStat={keyStat}
                  keyStats={[]}
                  releaseVersionId={release.id}
                />
              ),
            };
          })}
          onCancel={() => {
            setKeyStatistics(release.keyStatistics);
            toggleIsReordering.off();
          }}
          onConfirm={async () => {
            await reorderKeyStatistics({
              releaseVersionId: release.id,
              keyStatistics,
            });
            toggleIsReordering.off();
          }}
          onMoveItem={({ prevIndex, nextIndex }) => {
            const reorderedKeyStatistics = reorder(
              keyStatistics,
              prevIndex,
              nextIndex,
            );
            setKeyStatistics(reorderedKeyStatistics);
          }}
          onReverse={() => {
            setKeyStatistics(keyStatistics.toReversed());
          }}
        />
      ) : (
        <div className="govuk-!-margin-bottom-9">
          <KeyStatContainer>
            {release.keyStatistics.map(keyStat => (
              <div
                className={keyStatStyles.wrapper}
                data-testid="keyStat"
                key={keyStat.id}
              >
                <EditableKeyStat
                  keyStat={keyStat}
                  keyStats={release.keyStatistics}
                  releaseVersionId={release.id}
                />
              </div>
            ))}
          </KeyStatContainer>
        </div>
      )}
    </>
  );
}
