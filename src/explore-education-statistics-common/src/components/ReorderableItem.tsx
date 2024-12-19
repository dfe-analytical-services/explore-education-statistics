import DragIcon from '@common/components/DragIcon';
import styles from '@common/components/ReorderableItem.module.scss';
import VisuallyHidden from '@common/components/VisuallyHidden';
import { ArrowLeft, ArrowRight } from '@common/components/ArrowIcons';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import ButtonText from '@common/components/ButtonText';
import { mergeRefs } from '@common/utils/mergeRefs';
import classNames from 'classnames';
import React, { ReactNode, useEffect, useRef } from 'react';
import { DraggableProvided, DraggableStateSnapshot } from '@hello-pangea/dnd';

export interface ReorderableListItem {
  childOptions?: ReorderableListItem[];
  id: string;
  label: ReactNode | string;
  parentId?: string;
}

export interface ReorderResult {
  prevIndex: number;
  nextIndex: number;
}

interface Props {
  draggableProvided: DraggableProvided;
  draggableSnapshot: DraggableStateSnapshot;
  dropAreaActive: boolean;
  focusItem?: boolean;
  index: number;
  isLastItem?: boolean;
  item: ReorderableListItem;
  onExpandOptions?: (itemId: string, parentId?: string) => void;
  onMoveItem: ({ prevIndex, nextIndex }: ReorderResult) => void;
}

export default function ReorderableItem({
  draggableProvided,
  draggableSnapshot,
  dropAreaActive,
  focusItem = false,
  index,
  isLastItem = false,
  item,
  onExpandOptions,
  onMoveItem,
}: Props) {
  const itemRef = useRef<HTMLLIElement>(null);
  useEffect(() => {
    if (focusItem) {
      itemRef.current?.focus();
    }
  });
  const hasChildren = item.childOptions && item.childOptions.length > 1;
  const canReorderOptions =
    hasChildren ||
    (item.childOptions &&
      item.childOptions.length === 1 &&
      item.childOptions[0].childOptions &&
      item.childOptions[0].childOptions.length > 1);

  return (
    <li className={classNames(styles.container, 'govuk-!-margin-bottom-0')}>
      <div
        // eslint-disable-next-line react/jsx-props-no-spreading
        {...draggableProvided.draggableProps}
        // eslint-disable-next-line react/jsx-props-no-spreading
        {...draggableProvided.dragHandleProps}
        className={classNames('govuk-!-margin-bottom-0', styles.draggable, {
          [styles.isDragging]: draggableSnapshot.isDragging,
          [styles.isDraggedOutside]:
            draggableSnapshot.isDragging && !draggableSnapshot.draggingOver,
          [styles.notDragging]: dropAreaActive && !draggableSnapshot.isDragging,
        })}
        ref={mergeRefs(draggableProvided.innerRef, itemRef)}
        style={draggableProvided.draggableProps.style}
        data-testid="reorderable-item"
      >
        <div className={styles.itemLabel}>
          <DragIcon className={styles.dragIcon} />
          <div className={styles.labelInner}>{item.label}</div>
        </div>
      </div>
      {!dropAreaActive && !draggableSnapshot.isDragging && (
        <ButtonGroup
          className={classNames(styles.controls, 'govuk-!-margin-bottom-0')}
        >
          {canReorderOptions && (
            <ButtonText
              onClick={() => onExpandOptions?.(item.id, item.parentId)}
            >
              Reorder options
              <VisuallyHidden> within {item.label}</VisuallyHidden>
            </ButtonText>
          )}
          {index !== 0 && (
            <Button
              className="govuk-!-margin-bottom-0"
              variant="secondary"
              testId="move-up"
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
              testId="move-down"
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
    </li>
  );
}
