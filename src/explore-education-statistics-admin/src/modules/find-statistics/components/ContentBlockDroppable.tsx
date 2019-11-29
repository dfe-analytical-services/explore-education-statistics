import React, { ReactNode } from 'react';
import { Droppable } from 'react-beautiful-dnd';

const ContentBlockDroppable = ({
  draggable,
  droppableId,
  children,
}: {
  draggable: boolean;
  droppableId: string;
  children: ReactNode | ReactNode[];
}) => {
  return (
    <>
      {draggable ? (
        <Droppable droppableId={droppableId} type="content">
          {droppableProvided => (
            <div
              {...droppableProvided.droppableProps}
              ref={droppableProvided.innerRef}
            >
              {children}
              {droppableProvided.placeholder}
            </div>
          )}
        </Droppable>
      ) : (
        children
      )}
    </>
  );
};

export default ContentBlockDroppable;
