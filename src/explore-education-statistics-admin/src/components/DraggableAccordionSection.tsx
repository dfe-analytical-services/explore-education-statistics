import styles from '@admin/components/DraggableAccordionSection.module.scss';
import { EditableAccordionSectionProps } from '@admin/components/EditableAccordionSection';
import React, { cloneElement, ReactElement } from 'react';
import { Draggable } from 'react-beautiful-dnd';

interface DraggableAccordionSectionProps {
  id: string;
  isReordering: boolean;
  index: number;
  openAll: boolean;
  section: ReactElement<EditableAccordionSectionProps>;
}

const DraggableAccordionSection = ({
  id,
  isReordering,
  index,
  openAll,
  section,
}: DraggableAccordionSectionProps) => {
  if (isReordering) {
    return (
      <Draggable draggableId={id} index={index}>
        {draggableProvided => (
          <div
            {...draggableProvided.draggableProps}
            ref={draggableProvided.innerRef}
            className={styles.dragContainer}
          >
            <span
              {...draggableProvided.dragHandleProps}
              className={styles.dragHandle}
            />
            {cloneElement<EditableAccordionSectionProps>(section, {
              index,
              open: false,
              canToggle: false,
            })}
          </div>
        )}
      </Draggable>
    );
  }

  return cloneElement<EditableAccordionSectionProps>(section, {
    index,
    open: openAll,
  });
};

export default DraggableAccordionSection;
