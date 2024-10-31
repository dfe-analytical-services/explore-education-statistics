import styles from '@common/components/ReorderableList.module.scss';
import ReorderableItem, {
  ReorderableListItem,
} from '@common/components/ReorderableItem';
import ButtonGroup from '@common/components/ButtonGroup';
import Button from '@common/components/Button';
import classNames from 'classnames';
import React, { useState } from 'react';
import { DragDropContext, Draggable, Droppable } from '@hello-pangea/dnd';

interface Props {
  heading?: string;
  id: string;
  list: ReorderableListItem[];
  testId?: string;
  onCancel?: () => void;
  onConfirm?: () => void;
  onMoveItem: ({
    prevIndex,
    nextIndex,
  }: {
    prevIndex: number;
    nextIndex: number;
  }) => void;
  onReverse?: () => void;
}

export default function ReorderableList({
  heading,
  id,
  list,
  testId,
  onCancel,
  onConfirm,
  onMoveItem,
  onReverse,
}: Props) {
  const [focusItem, setFocusItem] = useState<number>(0);

  return (
    <>
      {heading && <h3>{heading}</h3>}
      <DragDropContext
        onDragEnd={result => {
          if (!result.destination) {
            return;
          }

          onMoveItem({
            prevIndex: result.source.index,
            nextIndex: result.destination.index,
          });
          setFocusItem(result.destination.index);
        }}
      >
        <Droppable droppableId={id}>
          {(droppableProvided, droppableSnapshot) => (
            <ol
              // eslint-disable-next-line react/jsx-props-no-spreading
              {...droppableProvided.droppableProps}
              className={classNames('govuk-list', styles.dropArea, {
                [styles.dropAreaActive]: droppableSnapshot.isDraggingOver,
              })}
              data-testid={testId}
              ref={droppableProvided.innerRef}
            >
              {list.map((item, index) => {
                return (
                  <Draggable draggableId={item.id} key={item.id} index={index}>
                    {(draggableProvided, draggableSnapshot) => (
                      <ReorderableItem
                        draggableProvided={draggableProvided}
                        draggableSnapshot={draggableSnapshot}
                        dropAreaActive={droppableSnapshot.isDraggingOver}
                        focusItem={focusItem === index}
                        key={item.id}
                        index={index}
                        isLastItem={index === list.length - 1}
                        item={item}
                        onMoveItem={({ prevIndex, nextIndex }) => {
                          onMoveItem({ prevIndex, nextIndex });
                          setFocusItem(nextIndex);
                        }}
                      />
                    )}
                  </Draggable>
                );
              })}
              {droppableProvided.placeholder}
            </ol>
          )}
        </Droppable>
      </DragDropContext>
      {(onCancel || onConfirm || onReverse) && (
        <ButtonGroup>
          {onConfirm && <Button onClick={onConfirm}>Confirm order</Button>}
          {onReverse && (
            <Button variant="secondary" onClick={onReverse}>
              Reverse order
            </Button>
          )}
          {onCancel && (
            <Button variant="secondary" onClick={onCancel}>
              Cancel reordering
            </Button>
          )}
        </ButtonGroup>
      )}
    </>
  );
}
