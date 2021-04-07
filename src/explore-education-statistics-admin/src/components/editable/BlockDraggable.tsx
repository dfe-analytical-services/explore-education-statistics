import classNames from 'classnames';
import React, { ReactNode } from 'react';
import { Draggable } from 'react-beautiful-dnd';
import styles from './BlockDraggable.module.scss';

interface Props {
  draggable: boolean;
  draggableId: string;
  index: number;
  children: ReactNode | ReactNode[];
  modifierClass?: string;
}

const BlockDraggable = ({ draggable, draggableId, index, children, modifierClass }: Props) => (
  <Draggable
    draggableId={draggableId}
    index={index}
    isDragDisabled={!draggable}
  >
    {(draggableProvided, snapshot) => (
      <div
        // eslint-disable-next-line react/jsx-props-no-spreading
        {...draggableProvided.draggableProps}
        // eslint-disable-next-line react/jsx-props-no-spreading
        {...draggableProvided.dragHandleProps}
        ref={draggableProvided.innerRef}
        className={classNames({
          [styles.draggable]: draggable,
          [styles.isDragging]: snapshot.isDragging,
          [styles[`${modifierClass}`]]: modifierClass
        })}
      >
        <span
          className={classNames({
            [styles.dragHandle]: draggable
          })}
        />
        {children}
      </div>
    )}
  </Draggable>
);

export default BlockDraggable;
