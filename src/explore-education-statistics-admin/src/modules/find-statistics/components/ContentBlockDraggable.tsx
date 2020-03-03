import React, { ReactNode } from 'react';
import { Draggable } from 'react-beautiful-dnd';
import styles from '@admin/modules/editable-components/EditableContentBlock.module.scss';

const ContentBlockDraggable = ({
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
      <Draggable draggableId={draggableId} index={index} type="contentBlock">
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

export default ContentBlockDraggable;
