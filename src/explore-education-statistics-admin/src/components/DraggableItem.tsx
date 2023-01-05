import styles from '@admin/components/DraggableItem.module.scss';
import classNames from 'classnames';
import React, { ReactNode } from 'react';
import { Draggable } from 'react-beautiful-dnd';

export const DragHandle = ({ className }: { className?: string }) => (
  <span aria-hidden className={className ?? styles.dragHandle}>
    ☰
  </span>
);

interface Props {
  children: ReactNode;
  className?: string;
  // draggableClassName replaces the default draggable style
  draggableClassName?: string;
  dragHandle?: ReactNode;
  // dragHandleClassName replaces the default dragHandle style
  dragHandleClassName?: string;
  hideDragHandle?: boolean;
  id: string;
  index: number;
  isDisabled?: boolean;
  isReordering: boolean;
  tag?: 'div' | 'li' | 'tr';
  testId?: string;
}

const DraggableItem = ({
  children,
  className,
  draggableClassName,
  dragHandleClassName,
  dragHandle: overrideDragHandle,
  hideDragHandle = false,
  id,
  index,
  isDisabled = false,
  isReordering,
  tag: Element = 'div',
  testId,
}: Props) => {
  return (
    <Draggable
      draggableId={id}
      index={index}
      isDragDisabled={isDisabled || !isReordering}
    >
      {(draggableProvided, draggableSnapshot) => (
        <Element
          {...draggableProvided.draggableProps}
          {...draggableProvided.dragHandleProps}
          className={classNames(className, draggableClassName, {
            [styles.draggable]: isReordering && !draggableClassName,
            [styles.isDragging]: draggableSnapshot.isDragging,
            [styles.isDraggedOutside]:
              draggableSnapshot.isDragging && !draggableSnapshot.draggingOver,
            [styles.hideDragHandle]: hideDragHandle,
          })}
          data-testid={testId}
          ref={draggableProvided.innerRef}
        >
          {!hideDragHandle && (
            <>
              {overrideDragHandle ??
                DragHandle({ className: dragHandleClassName })}
            </>
          )}
          {children}
        </Element>
      )}
    </Draggable>
  );
};

export default DraggableItem;
