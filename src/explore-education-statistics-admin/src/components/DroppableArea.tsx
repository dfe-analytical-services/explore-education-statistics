import styles from '@admin/components/DroppableArea.module.scss';
import classNames from 'classnames';
import React, { ReactNode } from 'react';
import { DroppableProvided, DroppableStateSnapshot } from 'react-beautiful-dnd';

interface Props {
  children: ReactNode;
  className?: string;
  droppableProvided: DroppableProvided;
  droppableSnapshot: DroppableStateSnapshot;
  tag?: 'div' | 'ol' | 'tbody';
  testId?: string;
}

const DroppableArea = ({
  children,
  className,
  droppableProvided,
  droppableSnapshot,
  tag: Element = 'div',
  testId,
}: Props) => {
  return (
    <Element
      {...droppableProvided.droppableProps}
      className={classNames(className, styles.dropArea, {
        [styles.dropAreaActive]: droppableSnapshot.isDraggingOver,
      })}
      data-testid={testId}
      ref={droppableProvided.innerRef}
    >
      {children}
      {droppableProvided.placeholder}
    </Element>
  );
};

export default DroppableArea;
