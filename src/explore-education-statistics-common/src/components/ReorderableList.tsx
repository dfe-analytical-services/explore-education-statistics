import styles from '@common/components/ReorderableList.module.scss';
import ReorderableItem, {
  ReorderableListItem,
  ReorderResult,
} from '@common/components/ReorderableItem';
import ButtonGroup from '@common/components/ButtonGroup';
import Button from '@common/components/Button';
import classNames from 'classnames';
import React, { useState } from 'react';
import { DragDropContext, Draggable, Droppable } from '@hello-pangea/dnd';

export interface ReorderableListProps {
  heading?: string;
  id: string;
  list: ReorderableListItem[];
  testId?: string;
  onCancel?: () => void;
  onConfirm?: () => void;
  onExpandOptions?: (itemId: string, itemParentId?: string) => void;
  onMoveItem: ({ prevIndex, nextIndex }: ReorderResult) => void;
  onReverse?: () => void;
}

export default function ReorderableList({
  heading,
  id,
  list,
  testId,
  onCancel,
  onConfirm,
  onExpandOptions,
  onMoveItem,
  onReverse,
}: ReorderableListProps) {
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
                        onExpandOptions={onExpandOptions}
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
