import classNames from 'classnames';
import React, { ReactNode } from 'react';
import { Draggable } from 'react-beautiful-dnd';
import styles from './BlockDraggable.module.scss';

interface Props {
  draggable: boolean;
  draggableId: string;
  index: number;
  children: ReactNode | ReactNode[];
}

const BlockDraggable = ({ draggable, draggableId, index, children }: Props) => (
  <Draggable
    draggableId={draggableId}
    index={index}
    isDragDisabled={!draggable}
  >
    {draggableProvided => (
      <div
        {...draggableProvided.draggableProps}
        ref={draggableProvided.innerRef}
        className={classNames({
          [styles.isDragging]: draggable,
        })}
      >
        <span
          {...draggableProvided.dragHandleProps}
          className={classNames({
            [styles.dragHandle]: draggable,
          })}
        />
        {children}
      </div>
    )}
  </Draggable>
);

export default BlockDraggable;
