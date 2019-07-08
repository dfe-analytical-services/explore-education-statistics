import { FormFieldset } from '@common/components/form';
import createErrorHelper from '@common/lib/validation/createErrorHelper';
import { Dictionary } from '@common/types';
import classNames from 'classnames';
import { Field, FieldProps } from 'formik';
import React from 'react';
import { Draggable, Droppable } from 'react-beautiful-dnd';
import FormFieldSortableList from './FormFieldSortableList';
import styles from './FormFieldSortableListGroup.module.scss';
import { SortableOption } from './FormSortableList';

interface Props<FormValues> {
  name: keyof FormValues & string;
  legend: string;
  groupLegend: string;
}

const FormFieldSortableListGroup = <T extends Dictionary<SortableOption[][]>>({
  name,
  legend,
  groupLegend,
}: Props<T>) => {
  return (
    <Field name={name}>
      {({ form }: FieldProps<T>) => {
        const { getError } = createErrorHelper(form);

        return (
          <Droppable droppableId={name} direction="horizontal">
            {(droppableProvided, droppableSnapshot) => (
              <div
                {...droppableProvided.droppableProps}
                ref={droppableProvided.innerRef}
                className={classNames(styles.groupsFieldset, {
                  [styles.groupsFieldsetDraggingOver]:
                    droppableSnapshot.isDraggingOver,
                })}
              >
                <FormFieldset
                  id={`sortableListGroup-${name}`}
                  legend={legend}
                  error={getError(name)}
                >
                  <div className={styles.listsContainer}>
                    {form.values[name].length === 0 && (
                      <div className="govuk-inset-text govuk-!-margin-0">
                        Add groups by dragging them here
                      </div>
                    )}

                    {form.values[name].map((_, index: number) => (
                      <Draggable
                        draggableId={`${name}-${index}`}
                        index={index}
                        // eslint-disable-next-line react/no-array-index-key
                        key={index}
                      >
                        {(draggableProvided, draggableSnapshot) => (
                          <div className={styles.listContainer}>
                            <div
                              {...draggableProvided.draggableProps}
                              {...draggableProvided.dragHandleProps}
                              className={classNames(
                                styles.list,
                                styles.isDraggable,
                                {
                                  [styles.isDragging]:
                                    draggableSnapshot.isDragging,
                                },
                              )}
                              ref={draggableProvided.innerRef}
                            >
                              <FormFieldSortableList<T>
                                name={`${name}[${index}]`}
                                id={`sortableList-${name}-${index}`}
                                legend={`${groupLegend} ${index + 1}`}
                                legendSize="s"
                              />
                            </div>
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
      }}
    </Field>
  );
};

export default FormFieldSortableListGroup;
