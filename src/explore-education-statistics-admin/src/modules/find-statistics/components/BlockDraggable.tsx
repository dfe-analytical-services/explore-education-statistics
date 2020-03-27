import styles from '@admin/components/editable/EditableBlock.module.scss';
import React, { ReactNode } from 'react';
import { Draggable } from 'react-beautiful-dnd';

const BlockDraggable = ({
  draggable,
  draggableId,
  index,
  children,
}: {
  draggable: boolean;
  draggableId: string;
  index: number;
  children: ReactNode | ReactNode[];
}) => (
  <>
    {draggable ? (
      <Draggable draggableId={draggableId} index={index}>
        {draggableProvided => (
          <div
            {...draggableProvided.draggableProps}
            ref={draggableProvided.innerRef}
            className={styles.isDragging}
          >
            <span
              {...draggableProvided.dragHandleProps}
              className={styles.dragHandle}
            />
            {children}
          </div>
        )}
      </Draggable>
    ) : (
      children
    )}
  </>
);

export default BlockDraggable;
