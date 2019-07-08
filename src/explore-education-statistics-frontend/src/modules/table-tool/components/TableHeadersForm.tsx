import Button from '@common/components/Button';
import Details from '@common/components/Details';
import { FormGroup, Formik } from '@common/components/form';
import reorder from '@common/lib/utils/reorder';
import Yup from '@common/lib/validation/yup';
import { PickByType } from '@common/types';
import { Form, FormikProps } from 'formik';
import React from 'react';
import { DragDropContext } from 'react-beautiful-dnd';
import FormFieldSortableList from './FormFieldSortableList';
import FormFieldSortableListGroup from './FormFieldSortableListGroup';
import { SortableOption } from './FormSortableList';
import styles from './TableHeadersForm.module.scss';

interface Props {
  initialValues: TableHeadersFormValues;
  onSubmit: (values: TableHeadersFormValues) => void;
}

export interface TableHeadersFormValues {
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

      <Formik<TableHeadersFormValues>
        enableReinitialize
        initialValues={initialValues}
        validationSchema={Yup.object<TableHeadersFormValues>({
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
        render={(form: FormikProps<TableHeadersFormValues>) => {
          return (
            <Form>
              <FormGroup>
                <div className="govuk-grid-row">
                  <div className="govuk-grid-column-three-quarters-from-desktop">
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

                          const [sourceItem] = sourceClone.splice(
                            source.index,
                            1,
                          );
                          destinationClone.splice(
                            destination.index,
                            0,
                            sourceItem,
                          );

                          form.setFieldTouched(sourceId);
                          form.setFieldValue(sourceId, sourceClone);

                          form.setFieldTouched(destinationId);
                          form.setFieldValue(destinationId, destinationClone);
                        }
                      }}
                    >
                      <div className={styles.axisContainer}>
                        <FormFieldSortableListGroup<
                          PickByType<TableHeadersFormValues, SortableOption[][]>
                        >
                          name="rowGroups"
                          legend="Row groups"
                          groupLegend="Row group"
                        />
                      </div>

                      <div className={styles.axisContainer}>
                        <FormFieldSortableListGroup<
                          PickByType<TableHeadersFormValues, SortableOption[][]>
                        >
                          name="columnGroups"
                          legend="Column groups"
                          groupLegend="Column group"
                        />
                      </div>
                    </DragDropContext>
                  </div>
                  <div className="govuk-grid-column-one-quarter-from-desktop">
                    <div className={styles.rowColContainer}>
                      <div className={styles.list}>
                        <FormFieldSortableList<TableHeadersFormValues>
                          name="rows"
                          id="sort-rows"
                          legend="Rows"
                        />
                      </div>
                      <div className={styles.list}>
                        <FormFieldSortableList<TableHeadersFormValues>
                          name="columns"
                          id="sort-columns"
                          legend="Columns"
                        />
                      </div>
                    </div>
                  </div>
                </div>
              </FormGroup>

              <Button type="submit">Re-order table</Button>
            </Form>
          );
        }}
      />
    </Details>
  );
};

export default TableHeadersForm;
