import React, { ReactNode } from 'react';
import { Droppable } from 'react-beautiful-dnd';

interface Props {
  droppable: boolean;
  droppableId: string;
  children: ReactNode | ReactNode[];
}

const BlockDroppable = ({ droppable, droppableId, children }: Props) => {
  return (
    <Droppable
      droppableId={droppableId}
      isDropDisabled={!droppable}
      type="content"
    >
      {droppableProvided => (
        <div
          // eslint-disable-next-line react/jsx-props-no-spreading
          {...droppableProvided.droppableProps}
          ref={droppableProvided.innerRef}
        >
          {children}
          {droppableProvided.placeholder}
        </div>
      )}
    </Droppable>
  );
};

export default BlockDroppable;
