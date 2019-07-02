import { FormFieldset } from '@common/components/form';
import createErrorHelper from '@common/lib/validation/createErrorHelper';
import { Dictionary } from '@common/types';
import classNames from 'classnames';
import { Field, FieldProps } from 'formik';
import React from 'react';
import { Draggable, Droppable } from 'react-beautiful-dnd';
import FormFieldSortableList from './FormFieldSortableList';
import { SortableOption } from './FormSortableList';
import { FormValues } from './TableHeadersForm';
import styles from './TableHeadersForm.module.scss';

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
                  id={`tableHeaders-${name}`}
                  legend={legend}
                  error={getError(name)}
                >
                  <div className={classNames(styles.listsContainer)}>
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
                            <FormFieldSortableList<FormValues>
                              name={`${name}[${index}]`}
                              id={`sort-${name}-${index}`}
                              legend={`${groupLegend} ${index + 1}`}
                              legendSize="s"
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
      }}
    </Field>
  );
};

export default FormFieldSortableListGroup;
