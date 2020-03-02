import React, { cloneElement, ReactElement, ReactNode } from 'react';
import { EditableAccordionSectionProps } from '@admin/components/EditableAccordionSection';
import { Draggable } from 'react-beautiful-dnd';
import styles from '@admin/components/DraggableAccordionSection.module.scss';

interface DraggableAccordionSectionProps {
  id: string;
  isReordering: boolean;
  index: number;
  openAll: boolean;
  section: ReactElement<EditableAccordionSectionProps>;
  headingButtons?: ReactNode[];
}

const DraggableAccordionSection = ({
  id,
  isReordering,
  index,
  openAll,
  section,
  headingButtons,
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
            {cloneElement<EditableAccordionSectionProps>(section, {
              index,
              open: false,
              canToggle: false,
              canEditHeading: section.props.canEditHeading && !isReordering,
              headingButtons: [
                ...(headingButtons || []),
                <span
                  key="drag_handle"
                  className={styles.dragHandle}
                  {...draggableProvided.dragHandleProps}
                />,
              ],
            })}
          </div>
        )}
      </Draggable>
    );
  }

  return cloneElement<EditableAccordionSectionProps>(section, {
    index,
    open: openAll,
    headingButtons,
  });
};

export default DraggableAccordionSection;
