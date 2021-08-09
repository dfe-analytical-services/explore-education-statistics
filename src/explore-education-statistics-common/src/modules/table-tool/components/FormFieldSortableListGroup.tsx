import { FormFieldset } from '@common/components/form';
import { useFormContext } from '@common/components/form/contexts/FormContext';
import useToggle from '@common/hooks/useToggle';
import classNames from 'classnames';
import { useField } from 'formik';
import React from 'react';
import { Draggable, Droppable } from 'react-beautiful-dnd';
import FormFieldSortableList from './FormFieldSortableList';
import styles from './FormFieldSortableListGroup.module.scss';

interface Props<FormValues> {
  id?: string;
  name: FormValues extends Record<string, unknown> ? keyof FormValues : string;
  legend: string;
  groupLegend: string;
}

function FormFieldSortableListGroup<FormValues>({
  id: customId,
  name,
  legend,
  groupLegend,
}: Props<FormValues>) {
  const { prefixFormId, fieldId } = useFormContext();
  const id = customId ? prefixFormId(customId) : fieldId(name as string);

  const [field, meta] = useField(name as string);
  const [isDragDisabled, toggleDragDisabled] = useToggle(false);

  return (
    <Droppable droppableId={name as string} direction="horizontal">
      {(droppableProvided, droppableSnapshot) => (
        <div
          // eslint-disable-next-line react/jsx-props-no-spreading
          {...droppableProvided.droppableProps}
          ref={droppableProvided.innerRef}
          className={classNames(styles.groupsFieldset, {
            [styles.isDraggingOver]: droppableSnapshot.isDraggingOver,
          })}
        >
          <FormFieldset
            id={id}
            legend={legend}
            error={meta.error}
            legendSize="m"
          >
            <div className={styles.listsContainer}>
              {field.value.length === 0 && (
                <div className="govuk-inset-text govuk-!-margin-0">
                  Add groups by dragging them here
                </div>
              )}

              {field.value.map((_: string, index: number) => (
                <Draggable
                  // eslint-disable-next-line react/no-array-index-key
                  key={index}
                  draggableId={`${name}-${index}`}
                  isDragDisabled={isDragDisabled}
                  index={index}
                >
                  {(draggableProvided, draggableSnapshot) => (
                    <div
                      // eslint-disable-next-line react/jsx-props-no-spreading
                      {...draggableProvided.draggableProps}
                      // eslint-disable-next-line react/jsx-props-no-spreading
                      {...draggableProvided.dragHandleProps}
                      className={classNames(styles.list, {
                        [styles.isDragging]: draggableSnapshot.isDragging,
                        [styles.optionDraggedOutside]:
                          draggableSnapshot.isDragging &&
                          !draggableSnapshot.draggingOver,
                      })}
                      ref={draggableProvided.innerRef}
                      role="button"
                      tabIndex={0}
                    >
                      <FormFieldSortableList
                        name={`${name}[${index}]`}
                        legend={`${groupLegend} ${index + 1}`}
                        legendSize="s"
                        onBlur={toggleDragDisabled.off}
                        onFocus={toggleDragDisabled.on}
                        onMouseEnter={toggleDragDisabled.on}
                        onMouseLeave={toggleDragDisabled.off}
                      />
                    </div>
                  )}
                </Draggable>
              ))}
              {droppableProvided.placeholder}
            </div>
          </FormFieldset>
        </div>
      )}
    </Droppable>
  );
}

export default FormFieldSortableListGroup;
