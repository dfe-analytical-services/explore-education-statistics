import BlockDroppable from '@admin/components/editable/BlockDroppable';
import EditableKeyStat from '@admin/pages/release/content/components/EditableKeyStat';
import styles from '@admin/pages/release/content/components/KeyStatistics.module.scss';
import useReleaseContentActions from '@admin/pages/release/content/contexts/useReleaseContentActions';
import { EditableRelease } from '@admin/services/releaseContentService';
import Button from '@common/components/Button';
import useToggle from '@common/hooks/useToggle';
import { KeyStatContainer } from '@common/modules/find-statistics/components/KeyStat';
import keyStatStyles from '@common/modules/find-statistics/components/KeyStat.module.scss';
import reorder from '@common/utils/reorder';
import classNames from 'classnames';
import React, { useCallback, useEffect, useState } from 'react';
import { DragDropContext, Draggable, DropResult } from 'react-beautiful-dnd';
import AddKeyStatistics from '@admin/pages/release/content/components/AddKeyStatistics';

export interface KeyStatisticsProps {
  release: EditableRelease;
  // eslint-disable-next-line react/no-unused-prop-types
  isEditing?: boolean;
}

const KeyStatistics = ({ release, isEditing }: KeyStatisticsProps) => {
  const { reorderKeyStatistics } = useReleaseContentActions();
  const [keyStatistics, setKeyStatistics] = useState(release.keyStatistics);

  useEffect(() => {
    setKeyStatistics(release.keyStatistics);
  }, [release]);

  const [isReordering, toggleReordering] = useToggle(false);

  const ReorderKeyStatisticsButton = () => {
    return !isReordering ? (
      <Button variant="secondary" onClick={toggleReordering.on}>
        Reorder<span className="govuk-visually-hidden"> key statistics</span>
      </Button>
    ) : (
      <Button variant="secondary" onClick={saveOrder}>
        Save order
      </Button>
    );
  };

  const saveOrder = useCallback(async () => {
    if (reorderKeyStatistics) {
      await reorderKeyStatistics({
        releaseId: release.id,
        keyStatistics,
      });
      toggleReordering.off();
    }
  }, [reorderKeyStatistics, release.id, keyStatistics, toggleReordering]);

  const handleDragEnd = useCallback(
    ({ source, destination }: DropResult) => {
      if (source && destination) {
        setKeyStatistics(
          reorder(keyStatistics, source.index, destination.index),
        );
      }
    },
    [keyStatistics],
  );

  return (
    <>
      {isEditing && (
        <>
          <AddKeyStatistics release={release} />
          <hr />
          {keyStatistics.length > 1 && <ReorderKeyStatisticsButton />}
        </>
      )}
      <DragDropContext onDragEnd={handleDragEnd}>
        <BlockDroppable
          droppable={isReordering}
          droppableId="keyStatisticsDroppable"
        >
          <div className="govuk-!-margin-bottom-9">
            <KeyStatContainer>
              {keyStatistics.map((keyStat, index) => {
                return (
                  <Draggable
                    draggableId={keyStat.id}
                    index={index}
                    isDragDisabled={!isReordering}
                    key={keyStat.id}
                  >
                    {(draggableProvided, snapshot) => (
                      <div
                        // eslint-disable-next-line react/jsx-props-no-spreading
                        {...draggableProvided.draggableProps}
                        // eslint-disable-next-line react/jsx-props-no-spreading
                        {...draggableProvided.dragHandleProps}
                        ref={draggableProvided.innerRef}
                        className={classNames({
                          [styles.draggable]: isReordering,
                          [keyStatStyles.column]: !isReordering,
                          [styles.isDragging]: snapshot.isDragging,
                        })}
                        data-testid="keyStat"
                      >
                        {isReordering && (
                          <span
                            className={classNames({
                              [styles.dragHandle]: isReordering,
                            })}
                          />
                        )}
                        <EditableKeyStat
                          key={keyStat.id}
                          keyStat={keyStat}
                          releaseId={release.id}
                          isEditing={isEditing}
                          isReordering={isReordering}
                        />
                      </div>
                    )}
                  </Draggable>
                );
              })}
            </KeyStatContainer>
          </div>
        </BlockDroppable>
      </DragDropContext>
    </>
  );
};

export default KeyStatistics;
