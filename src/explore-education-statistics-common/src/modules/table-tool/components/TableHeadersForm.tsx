import Button from '@common/components/Button';
import Details from '@common/components/Details';
import { FormGroup } from '@common/components/form';
import { Filter } from '@common/modules/table-tool/types/filters';
import { TableHeadersConfig } from '@common/modules/table-tool/types/tableHeaders';
import { PickByType } from '@common/types';
import reorder from '@common/utils/reorder';
import Yup from '@common/validation/yup';
import { Form, Formik } from 'formik';
import compact from 'lodash/compact';
import last from 'lodash/last';
import React, { useCallback } from 'react';
import { DragDropContext } from 'react-beautiful-dnd';
import FormFieldSortableListGroup from './FormFieldSortableListGroup';

interface FormValues {
  rowGroups: Filter[][];
  columnGroups: Filter[][];
}

interface Props {
  id?: string;
  initialValues?: TableHeadersConfig;
  onSubmit: (values: TableHeadersConfig) => void;
}

const TableHeadersForm = ({
  onSubmit,
  id = 'tableHeadersForm',
  initialValues = {
    columnGroups: [],
    columns: [],
    rowGroups: [],
    rows: [],
  },
}: Props) => {
  const handleSubmit = useCallback(
    (values: FormValues) => {
      onSubmit({
        columnGroups:
          values.columnGroups.length > 1
            ? values.columnGroups.slice(0, -1)
            : [],
        rowGroups:
          values.rowGroups.length > 1 ? values.rowGroups.slice(0, -1) : [],
        columns: last(values.columnGroups) as Filter[],
        rows: last(values.rowGroups) as Filter[],
      });
    },
    [onSubmit],
  );

  return (
    <Details summary="Re-order table headers" className="dfe-custom-details">
      <div className="dfe-custom-details--content">
        <p className="govuk-hint">
          Drag and drop the options below to re-order the table headers. For
          keyboard users, select and deselect a draggable item with space and
          use the arrow keys to move a selected item.
        </p>
        <div className="govuk-visually-hidden">
          To move a draggable item, select and deselect the item with space and
          use the arrow keys to move a selected item. If you are using a screen
          reader disable scan mode.
        </div>

        <Formik<FormValues>
          enableReinitialize
          initialValues={{
            columnGroups: compact([
              ...(initialValues?.columnGroups ?? []),
              initialValues?.columns,
            ]),
            rowGroups: compact([
              ...(initialValues?.rowGroups ?? []),
              initialValues?.rows,
            ]),
          }}
          validationSchema={Yup.object<FormValues>({
            rowGroups: Yup.array()
              .of(Yup.array().of<Filter>(Yup.object()).ensure())
              .min(1, 'Must have at least one row group'),
            columnGroups: Yup.array()
              .of(Yup.array().of<Filter>(Yup.object()).ensure())
              .min(1, 'Must have at least one column group'),
          })}
          onSubmit={handleSubmit}
        >
          {form => {
            return (
              <Form>
                <DragDropContext
                  onDragEnd={result => {
                    const { source, destination } = result;

                    if (!destination) {
                      return;
                    }

                    const destinationId = destination.droppableId as keyof FormValues;
                    const sourceId = source.droppableId as keyof FormValues;

                    if (destinationId === sourceId) {
                      form.setFieldTouched(destinationId);
                      form.setFieldValue(
                        destinationId,
                        reorder(
                          form.values[destinationId] as unknown[],
                          source.index,
                          destination.index,
                        ),
                      );

                      return;
                    }

                    const nextSourceValue = [...form.values[sourceId]];
                    const nextDestinationValue = [
                      ...form.values[destinationId],
                    ];

                    const [sourceItem] = nextSourceValue.splice(
                      source.index,
                      1,
                    );
                    nextDestinationValue.splice(
                      destination.index,
                      0,
                      sourceItem,
                    );

                    form.setFieldValue(sourceId, nextSourceValue);
                    form.setFieldValue(destinationId, nextDestinationValue);

                    form.setFieldTouched(sourceId);
                    form.setFieldTouched(destinationId);
                  }}
                >
                  <FormGroup>
                    <div className="govuk-!-margin-bottom-2 column-group">
                      <FormFieldSortableListGroup<
                        PickByType<TableHeadersConfig, Filter[][]>
                      >
                        name="columnGroups"
                        legend="Column groups"
                        groupLegend="Column group"
                      />
                    </div>
                    <div className="govuk-!-margin-bottom-2 row-group">
                      <FormFieldSortableListGroup<
                        PickByType<TableHeadersConfig, Filter[][]>
                      >
                        name="rowGroups"
                        legend="Row groups"
                        groupLegend="Row group"
                      />
                    </div>
                  </FormGroup>
                </DragDropContext>

                <Button type="submit">Re-order table</Button>
              </Form>
            );
          }}
        </Formik>
      </div>
    </Details>
  );
};

export default TableHeadersForm;
