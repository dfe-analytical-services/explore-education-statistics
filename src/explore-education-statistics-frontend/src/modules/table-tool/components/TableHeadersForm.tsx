import Button from '@common/components/Button';
import Details from '@common/components/Details';
import { FormFieldset, Formik } from '@common/components/form';
import reorder from '@common/lib/utils/reorder';
import createErrorHelper from '@common/lib/validation/createErrorHelper';
import Yup from '@common/lib/validation/yup';
import classNames from 'classnames';
import { Form, FormikProps } from 'formik';
import React from 'react';
import { DragDropContext, Draggable, Droppable } from 'react-beautiful-dnd';
import FormFieldSortableList from './FormFieldSortableList';
import { SortableOption } from './FormSortableList';
import styles from './TableHeadersForm.module.scss';

interface Props {
  initialValues: FormValues;
  onSubmit: (values: FormValues) => void;
}

export interface FormValues {
  columnGroups: SortableOption[][];
  columns: SortableOption[];
  rowGroups: SortableOption[][];
  rows: SortableOption[];
}

const TableHeadersForm = (props: Props) => {
  const { onSubmit, initialValues } = props;

  return (
    <Details summary="Re-order table headers">
      <p className="govuk-hint">
        Drag and drop the options below to re-order the table headers.
      </p>

      <Formik<FormValues>
        enableReinitialize
        initialValues={initialValues}
        validationSchema={Yup.object<FormValues>({
          rowGroups: Yup.array()
            .of(
              Yup.array()
                .of<SortableOption>(Yup.object())
                .ensure(),
            )
            .min(1, 'Must have at least one row group'),
          columnGroups: Yup.array()
            .of(
              Yup.array()
                .of<SortableOption>(Yup.object())
                .ensure(),
            )
            .min(1, 'Must have at least one column group'),
          columns: Yup.array()
            .of<SortableOption>(Yup.object())
            .ensure(),
          rows: Yup.array()
            .of<SortableOption>(Yup.object())
            .ensure(),
        })}
        onSubmit={onSubmit}
        render={(form: FormikProps<FormValues>) => {
          const { getError } = createErrorHelper(form);

          const renderDroppableFieldset = (
            group: 'rowGroups' | 'columnGroups',
            fieldsetLegend: string,
            groupLegend: string,
          ) => {
            return (
              <Droppable droppableId={group} direction="horizontal">
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
                      id={`tableHeaders-${group}`}
                      legend={fieldsetLegend}
                      error={getError(group)}
                    >
                      <div className={classNames(styles.listsContainer)}>
                        {form.values[group].length === 0 && (
                          <div className="govuk-inset-text govuk-!-margin-0">
                            Add groups by dragging them here
                          </div>
                        )}

                        {form.values[group].map((_, index) => (
                          <Draggable
                            draggableId={`${group}-${index}`}
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
                                  name={`${group}[${index}]`}
                                  id={`sort-${group}-${index}`}
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
          };

          return (
            <Form>
              <DragDropContext
                onDragEnd={result => {
                  if (!result.destination) {
                    return;
                  }

                  const { source, destination } = result;

                  const destinationId = destination.droppableId as
                    | 'columnGroups'
                    | 'rowGroups';

                  const sourceId = source.droppableId as
                    | 'columnGroups'
                    | 'rowGroups';

                  const destinationGroup = form.values[destinationId];

                  if (destinationId === sourceId) {
                    form.setFieldTouched(destinationId);
                    form.setFieldValue(
                      destinationId,
                      reorder(
                        destinationGroup,
                        source.index,
                        destination.index,
                      ),
                    );
                  } else {
                    const sourceClone = Array.from(form.values[sourceId]);
                    const destinationClone = Array.from(destinationGroup);

                    const [sourceItem] = sourceClone.splice(source.index, 1);
                    destinationClone.splice(destination.index, 0, sourceItem);

                    form.setFieldTouched(sourceId);
                    form.setFieldValue(sourceId, sourceClone);

                    form.setFieldTouched(destinationId);
                    form.setFieldValue(destinationId, destinationClone);
                  }
                }}
              >
                <div className={styles.axisContainer}>
                  {renderDroppableFieldset(
                    'rowGroups',
                    'Row groups',
                    'Row group',
                  )}

                  <div className={styles.rowColContainer}>
                    <div className={styles.list}>
                      <FormFieldSortableList<FormValues>
                        name="rows"
                        id="sort-rows"
                        legend="Rows"
                      />
                    </div>
                  </div>
                </div>

                <div className={styles.axisContainer}>
                  {renderDroppableFieldset(
                    'columnGroups',
                    'Column groups',
                    'Column group',
                  )}

                  <div className={styles.rowColContainer}>
                    <div className={styles.list}>
                      <FormFieldSortableList<FormValues>
                        name="columns"
                        id="sort-columns"
                        legend="Columns"
                      />
                    </div>
                  </div>
                </div>

                <Button type="submit">Re-order table</Button>
              </DragDropContext>
            </Form>
          );
        }}
      />
    </Details>
  );
};

export default TableHeadersForm;
