import { FormFieldset } from '@common/components/form';
import { FormFieldsetProps } from '@common/components/form/FormFieldset';
import reorder from '@common/lib/utils/reorder';
import classNames from 'classnames';
import React from 'react';
import { DragDropContext, Draggable, Droppable } from 'react-beautiful-dnd';
import styles from './FormSortableList.module.scss';

export interface SortableOption {
  label: string;
  value: string;
}

type SortableOptionChangeEventHandler = (value: SortableOption[]) => void;

export type FormSortableListProps = {
  onChange?: SortableOptionChangeEventHandler;
  value: SortableOption[];
} & FormFieldsetProps;

const FormSortableList = (props: FormSortableListProps) => {
  const { id, onChange, value } = props;

  return (
    <FormFieldset {...props}>
      <DragDropContext
        onDragEnd={result => {
          if (!result.destination) {
            return;
          }

          const newValue = reorder(
            value,
            result.source.index,
            result.destination.index,
          );

          if (onChange) {
            onChange(newValue);
          }
        }}
      >
        <Droppable droppableId={id}>
          {(droppableProvided, droppableSnapshot) => (
            <div
              {...droppableProvided.droppableProps}
              className={classNames({
                [styles.listDraggingOver]: droppableSnapshot.isDraggingOver,
              })}
              ref={droppableProvided.innerRef}
            >
              {value.map((option, index) => (
                <Draggable
                  draggableId={option.value}
                  key={option.value}
                  index={index}
                >
                  {(draggableProvided, draggableSnapshot) => (
                    <div
                      {...draggableProvided.draggableProps}
                      {...draggableProvided.dragHandleProps}
                      className={classNames(styles.optionRow, {
                        [styles.optionCurrentDragging]:
                          draggableSnapshot.isDragging,
                      })}
                      ref={draggableProvided.innerRef}
                      style={draggableProvided.draggableProps.style}
                    >
                      <div className={styles.optionText}>
                        <strong>{option.label}</strong>
                        <span>⇅</span>
                      </div>
                    </div>
                  )}
                </Draggable>
              ))}
              {droppableProvided.placeholder}
            </div>
          )}
        </Droppable>
      </DragDropContext>
    </FormFieldset>
  );
};

export default FormSortableList;
