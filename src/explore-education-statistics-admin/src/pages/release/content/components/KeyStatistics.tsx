import BlockDroppable from '@admin/components/editable/BlockDroppable';
import EditableKeyStat from '@admin/pages/release/content/components/EditableKeyStat';
import KeyStatDataBlockSelectForm from '@admin/pages/release/content/components/KeyStatDataBlockSelectForm';
import styles from '@admin/pages/release/content/components/KeyStatistics.module.scss';
import useReleaseContentActions from '@admin/pages/release/content/contexts/useReleaseContentActions';
import { EditableRelease } from '@admin/services/releaseContentService';
import Button from '@common/components/Button';
import WarningMessage from '@common/components/WarningMessage';
import useToggle from '@common/hooks/useToggle';
import { KeyStatContainer } from '@common/modules/find-statistics/components/KeyStat';
import keyStatStyles from '@common/modules/find-statistics/components/KeyStat.module.scss';
import reorder from '@common/utils/reorder';
import classNames from 'classnames';
import React, { useCallback, useEffect, useState } from 'react';
import { DragDropContext, Draggable, DropResult } from 'react-beautiful-dnd';
import { KeyStatisticType } from '@common/services/publicationService';
import { KeyStatisticTextCreateRequest } from '@admin/services/keyStatisticService';
import EditableKeyStatTextForm from '@admin/pages/release/content/components/EditableKeyStatTextForm';

export interface KeyStatisticsProps {
  release: EditableRelease;
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

const AddKeyStatistics = ({ release }: KeyStatisticsProps) => {
  const [formOpenForType, setFormOpenForType] = useState<
    KeyStatisticType | undefined
  >(undefined);
  const {
    updateUnattachedDataBlocks,
    addKeyStatisticDataBlock,
    addKeyStatisticText,
  } = useReleaseContentActions();

  const addKeyStatDataBlock = useCallback(
    async (dataBlockId: string) => {
      await addKeyStatisticDataBlock({ releaseId: release.id, dataBlockId });
      await updateUnattachedDataBlocks({ releaseId: release.id });
      setFormOpenForType(undefined);
    },
    [release.id, addKeyStatisticDataBlock, updateUnattachedDataBlocks],
  );

  const addKeyStatText = useCallback(
    async (newKeyStatText: KeyStatisticTextCreateRequest) => {
      await addKeyStatisticText({
        releaseId: release.id,
        keyStatisticText: newKeyStatText,
      });
      setFormOpenForType(undefined);
    },
    [release.id, addKeyStatisticText],
  );

  switch (formOpenForType) {
    case KeyStatisticType.DATABLOCK:
      return (
        <div className={styles.keyStatDataBlockFormContainer}>
          <WarningMessage>
            In order to add a key statistic from a data block, you first need to
            create a data block with just one value.
            <br />
            Any data blocks with more than one value cannot be selected as a key
            statistic.
          </WarningMessage>
          <KeyStatDataBlockSelectForm
            releaseId={release.id}
            onSelect={addKeyStatDataBlock}
            onCancel={() => setFormOpenForType(undefined)}
          />
        </div>
      );
    case KeyStatisticType.TEXT:
      return (
        <div className={styles.keyStatTextFormContainer}>
          <EditableKeyStatTextForm
            onSubmit={values =>
              addKeyStatText(values as KeyStatisticTextCreateRequest)
            }
            onCancel={() => setFormOpenForType(undefined)}
            testId="keyStatText-createForm"
          />
        </div>
      );
    default:
      return (
        <>
          <ul className="govuk-list">
            <li>
              <Button
                onClick={() => {
                  setFormOpenForType(KeyStatisticType.DATABLOCK);
                }}
                className="govuk-!-margin-bottom-2"
              >
                Add key statistic from data block
              </Button>
            </li>
            <li>
              <Button
                onClick={() => {
                  setFormOpenForType(KeyStatisticType.TEXT);
                }}
                className="govuk-!-margin-bottom-2"
              >
                Add free text key statistic
              </Button>
            </li>
          </ul>
        </>
      );
  }
};

export default KeyStatistics;
