import styles from '@admin/components/DraggableAccordionSection.module.scss';
import { EditableAccordionSectionProps } from '@admin/components/EditableAccordionSection';
import classNames from 'classnames';
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
  return (
    <Draggable draggableId={id} isDragDisabled={!isReordering} index={index}>
      {draggableProvided => (
        <div
          {...draggableProvided.draggableProps}
          ref={draggableProvided.innerRef}
          className={classNames({
            [styles.dragContainer]: isReordering,
          })}
        >
          <span
            {...draggableProvided.dragHandleProps}
            className={classNames({
              [styles.dragHandle]: isReordering,
            })}
          />
          {cloneElement<EditableAccordionSectionProps>(section, {
            index,
            open: isReordering ? false : openAll,
            canToggle: !isReordering,
          })}
        </div>
      )}
    </Draggable>
  );
};

export default DraggableAccordionSection;
