import { FormFieldset } from '@common/components/form';
import { FormFieldsetProps } from '@common/components/form/FormFieldset';
import { Filter } from '@common/modules/table-tool/types/filters';
import reorder from '@common/utils/reorder';
import classNames from 'classnames';
import React, { MouseEventHandler } from 'react';
import { DragDropContext, Draggable, Droppable } from 'react-beautiful-dnd';
import styles from './FormSortableList.module.scss';

type SortableOptionChangeEventHandler = (value: Filter[]) => void;

export type FormSortableListProps = {
  onChange?: SortableOptionChangeEventHandler;
  onMouseEnter?: MouseEventHandler<HTMLDivElement>;
  onMouseLeave?: MouseEventHandler<HTMLDivElement>;
  value: Filter[];
} & FormFieldsetProps;

const FormSortableList = ({
  id,
  onChange,
  onMouseEnter,
  onMouseLeave,
  value,
  ...props
}: FormSortableListProps) => {
  return (
    <FormFieldset {...props} id={id}>
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
              // eslint-disable-next-line react/jsx-props-no-spreading
              {...droppableProvided.droppableProps}
              className={classNames(styles.list, {
                [styles.listDraggingOver]: droppableSnapshot.isDraggingOver,
              })}
              ref={droppableProvided.innerRef}
              onMouseEnter={onMouseEnter}
              onMouseLeave={onMouseLeave}
            >
              {value.map((option, index) => (
                <Draggable
                  draggableId={option.value}
                  key={option.value}
                  index={index}
                >
                  {(draggableProvided, draggableSnapshot) => (
                    <div
                      // eslint-disable-next-line react/jsx-props-no-spreading
                      {...draggableProvided.draggableProps}
                      // eslint-disable-next-line react/jsx-props-no-spreading
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
                        <span
                          className={styles.optionDraggableIcon}
                          title="Reposition this element by dragging and dropping within this group"
                        >
                          ⋯
                        </span>
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
