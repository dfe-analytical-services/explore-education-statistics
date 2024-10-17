import DragIcon from '@common/components/DragIcon';
import styles from '@common/components/ReorderableItem.module.scss';
import VisuallyHidden from '@common/components/VisuallyHidden';
import { ArrowLeft, ArrowRight } from '@common/components/ArrowIcons';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import { mergeRefs } from '@common/utils/mergeRefs';
import classNames from 'classnames';
import React, { ReactNode, useEffect, useRef } from 'react';
import { DraggableProvided, DraggableStateSnapshot } from '@hello-pangea/dnd';

export interface ReorderableListItem {
  id: string;
  label: ReactNode | string;
}

interface Props {
  draggableProvided: DraggableProvided;
  draggableSnapshot: DraggableStateSnapshot;
  focusItem: boolean;
  index: number;
  isLastItem: boolean;
  item: ReorderableListItem;
  onMoveItem: ({
    prevIndex,
    nextIndex,
  }: {
    prevIndex: number;
    nextIndex: number;
  }) => void;
}

export default function ReorderableItem({
  draggableProvided,
  draggableSnapshot,
  focusItem = false,
  index,
  isLastItem,
  item,
  onMoveItem,
}: Props) {
  const itemRef = useRef<HTMLLIElement>(null);
  useEffect(() => {
    if (focusItem) {
      itemRef.current?.focus();
    }
  });

  return (
    <div className={styles.container}>
      <div
        // eslint-disable-next-line react/jsx-props-no-spreading
        {...draggableProvided.draggableProps}
        // eslint-disable-next-line react/jsx-props-no-spreading
        {...draggableProvided.dragHandleProps}
        className={classNames('govuk-!-margin-bottom-0', styles.draggable, {
          [styles.isDragging]: draggableSnapshot.isDragging,
          [styles.isDraggedOutside]:
            draggableSnapshot.isDragging && !draggableSnapshot.draggingOver,
        })}
        ref={mergeRefs(draggableProvided.innerRef, itemRef)}
        style={draggableProvided.draggableProps.style}
        data-testid="reorderable-item"
      >
        <div className={styles.itemLabel}>
          <DragIcon className={styles.dragIcon} />
          <span>{item.label}</span>
        </div>
      </div>
      {!draggableSnapshot.isDragging && (
        <ButtonGroup className={styles.controls}>
          {index !== 0 && (
            <Button
              className="govuk-!-margin-bottom-0"
              variant="secondary"
              onClick={() => {
                onMoveItem({ prevIndex: index, nextIndex: index - 1 });
              }}
            >
              <ArrowLeft className={styles.arrow} />
              <VisuallyHidden>Move {item.label} up</VisuallyHidden>
            </Button>
          )}
          {!isLastItem && (
            <Button
              className="govuk-!-margin-bottom-0"
              variant="secondary"
              onClick={() => {
                onMoveItem({ prevIndex: index, nextIndex: index + 1 });
              }}
            >
              <ArrowRight className={styles.arrow} />
              <VisuallyHidden>Move {item.label} down</VisuallyHidden>
            </Button>
          )}
        </ButtonGroup>
      )}
    </div>
  );
}
