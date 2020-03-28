import classnames from 'classnames';
import React, { ReactNode } from 'react';
import { Droppable } from 'react-beautiful-dnd';

import styles from './DroppableAccordion.module.scss';

interface DroppableAccordionProps {
  id: string;
  isReordering: boolean;
  children: ReactNode;
}

const DroppableAccordion = ({
  id,
  isReordering,
  children,
}: DroppableAccordionProps) => {
  return (
    <Droppable droppableId={id} isDropDisabled={!isReordering} type="accordion">
      {(droppableProvided, snapshot) => (
        <div
          {...droppableProvided.droppableProps}
          ref={droppableProvided.innerRef}
          className={classnames({
            [styles.dragover]: snapshot.isDraggingOver && isReordering,
          })}
        >
          {children}

          {droppableProvided.placeholder}
        </div>
      )}
    </Droppable>
  );
};

export default DroppableAccordion;
