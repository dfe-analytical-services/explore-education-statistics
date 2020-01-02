import { Droppable } from 'react-beautiful-dnd';
import React, { ReactNode } from 'react';
import classnames from 'classnames';

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
  if (isReordering) {
    return (
      <Droppable droppableId={id} type="accordion">
        {(droppableProvided, snapshot) => (
          <div
            {...droppableProvided.droppableProps}
            ref={droppableProvided.innerRef}
            className={classnames({
              [styles.dragover]: snapshot.isDraggingOver,
            })}
          >
            {children}

            {droppableProvided.placeholder}
          </div>
        )}
      </Droppable>
    );
  }

  return <>{children}</>;
};

export default DroppableAccordion;
