import EditableKeyStat from '@admin/components/editable/EditableKeyStat';
import KeyStatDataBlockSelectForm from '@admin/pages/release/content/components/KeyStatDataBlockSelectForm';
import useReleaseContentActions from '@admin/pages/release/content/contexts/useReleaseContentActions';
import { EditableRelease } from '@admin/services/releaseContentService';
import BlockDroppable from '@admin/components/editable/BlockDroppable';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import WarningMessage from '@common/components/WarningMessage';
import { KeyStatContainer } from '@common/modules/find-statistics/components/KeyStat';
import styles from '@admin/pages/release/content/components/KeyStatistics.module.scss';
import keyStatStyles from '@common/modules/find-statistics/components/KeyStat.module.scss';
import useToggle from '@common/hooks/useToggle';
import React, { useCallback, useState, useEffect } from 'react';
import { Draggable, DragDropContext, DropResult } from 'react-beautiful-dnd';
import classNames from 'classnames';
import reorder from '@common/utils/reorder';
import { Dictionary } from '@admin/types';
import {
  KeyStatistic,
  KeyStatisticDataBlock,
  KeyStatisticText,
} from '@common/services/publicationService';

export interface KeyStatisticsProps {
  release: EditableRelease;
  isEditing?: boolean;
}

const KeyStatistics = ({ release, isEditing }: KeyStatisticsProps) => {
  const {
    deleteContentSectionBlock,
    updateSectionBlockOrder,
  } = useReleaseContentActions();

  const [keyStatisticsBlocks, setKeyStatisticsBlocks] = useState(
    release.keyStatistics,
  );

  useEffect(() => {
    setKeyStatisticsBlocks(release.keyStatistics);
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

  const reorderKeyStatistics = useCallback(
    async (keyStatistics: KeyStatistic[]) => {
      const order = keyStatistics.reduce<Dictionary<number>>(
        (acc, keyStat, index) => {
          acc[keyStat.id] = index;
          return acc;
        },
        {},
      );
      await updateSectionBlockOrder({
        releaseId: release.id,
        sectionId: release.keyStatisticsSection.id,
        sectionKey: 'keyStatisticsSection',
        order,
      });
      // @MarkFix reorder KeyStatisticBase here
    },
    [release.id, release.keyStatisticsSection.id, updateSectionBlockOrder],
  );

  const saveOrder = useCallback(async () => {
    if (reorderKeyStatistics) {
      await reorderKeyStatistics(keyStatisticsBlocks);
      toggleReordering.off();
    }
  }, [reorderKeyStatistics, keyStatisticsBlocks, toggleReordering]);

  const handleDragEnd = useCallback(
    ({ source, destination }: DropResult) => {
      if (source && destination) {
        setKeyStatisticsBlocks(
          reorder(keyStatisticsBlocks, source.index, destination.index),
        );
      }
    },
    [keyStatisticsBlocks],
  );

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
          <ButtonGroup className={styles.buttons}>
            <AddKeyStatistics release={release} />
            {keyStatisticsBlocks.length > 1 && <ReorderKeyStatisticsButton />}
          </ButtonGroup>
        </>
      )}
      <DragDropContext onDragEnd={handleDragEnd}>
        <BlockDroppable
          droppable={isReordering}
          droppableId="keyStatisticsDroppable"
        >
          <div className="govuk-!-margin-bottom-9">
            <KeyStatContainer>
              {keyStatisticsBlocks.map((keyStat, index) => {
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
                          releaseId={release.id}
                          keyStatId={keyStat.id}
                          dataBlockId={
                            (keyStat as KeyStatisticDataBlock).dataBlockId
                          }
                          title={(keyStat as KeyStatisticText).title}
                          statistic={(keyStat as KeyStatisticText).statistic}
                          trend={keyStat.trend}
                          guidanceTitle={keyStat.guidanceTitle}
                          guidanceText={keyStat.guidanceText}
                          isEditing={isEditing}
                          isReordering={isReordering}
                          onRemove={async () => {
                            await deleteContentSectionBlock({
                              releaseId: release.id,
                              sectionId: release.keyStatisticsSection.id,
                              blockId: keyStat.id,
                              sectionKey: 'keyStatisticsSection',
                            });
                            // @MarkFix call keyStatisticService.Delete here
                            // call useReleaseContentActions#updateAvailableDataBlocks too
                          }}
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
      // @MarkFix call keyStatisticService.Create here
      // call useReleaseContentActions#updateAvailableDataBlocks too
      setIsFormOpen(false);
    },
    [release.id, release.keyStatisticsSection, attachContentSectionBlock],
  );

  return (
    <>
      {isFormOpen ? (
        <div className={styles.formContainer}>
          <KeyStatDataBlockSelectForm
            releaseId={release.id}
            onSelect={addKeyStatToSection}
            onCancel={() => setIsFormOpen(false)}
          />
        </div>
      ) : (
        <Button
          onClick={() => {
            setIsFormOpen(true);
          }}
        >
          {`Add ${
            keyStatisticsSection.content.length > 0 ? 'another ' : ''
          }key statistic`}
        </Button>
      )}
    </>
  );
};

export default KeyStatistics;
