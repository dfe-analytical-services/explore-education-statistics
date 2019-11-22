import React, { cloneElement, ReactElement } from 'react';
import { EditableAccordionSectionProps } from '@admin/components/EditableAccordionSection';
import { Draggable } from 'react-beautiful-dnd';
import styles from '@admin/components/DraggableAccordionSection.module.scss';

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
      <Draggable draggableId={id} type="section" index={index}>
        {draggableProvided => (
          <div
            {...draggableProvided.draggableProps}
            ref={draggableProvided.innerRef}
            className={styles.dragContainer}
          >
            {cloneElement<EditableAccordionSectionProps>(section, {
              index,
              open: false,
              canToggle: false,
              headingButtons: (
                <span
                  className={styles.dragHandle}
                  {...draggableProvided.dragHandleProps}
                />
              ),
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
