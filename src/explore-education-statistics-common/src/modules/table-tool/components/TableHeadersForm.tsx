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
  initialValues?: TableHeadersConfig;
  onSubmit: (values: TableHeadersConfig) => void;
  id?: string;
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
    <Details summary="Re-order table headers">
      <p className="govuk-hint">
        Drag and drop the options below to re-order the table headers. Hold the
        Ctrl key and click to select multiple items to drag and drop. For
        keyboard users, select and deselect a draggable item with space and use
        the arrow keys to move a selected item.
      </p>
      <p className="govuk-visually-hidden">
        To move an item with your keyboard, select or deselect the item with
        Space and use the Up and Down arrow keys to move the selected item. To
        move multiple items, you can press Ctrl and Enter to add an item to your
        selected items. If you are using a screen reader disable scan mode.
      </p>

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
            <Form id={id}>
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
                  const nextDestinationValue = [...form.values[destinationId]];

                  const [sourceItem] = nextSourceValue.splice(source.index, 1);
                  nextDestinationValue.splice(destination.index, 0, sourceItem);

                  form.setFieldValue(sourceId, nextSourceValue);
                  form.setFieldValue(destinationId, nextDestinationValue);

                  form.setFieldTouched(sourceId);
                  form.setFieldTouched(destinationId);
                }}
              >
                <FormGroup>
                  <div className="govuk-!-margin-bottom-2">
                    <FormFieldSortableListGroup<
                      PickByType<TableHeadersConfig, Filter[][]>
                    >
                      name="rowGroups"
                      legend="Row groups"
                      groupLegend="Row group"
                    />
                  </div>

                  <div className="govuk-!-margin-bottom-2">
                    <FormFieldSortableListGroup<
                      PickByType<TableHeadersConfig, Filter[][]>
                    >
                      name="columnGroups"
                      legend="Column groups"
                      groupLegend="Column group"
                    />
                  </div>
                </FormGroup>
              </DragDropContext>

              <Button type="submit">Re-order table</Button>
            </Form>
          );
        }}
      </Formik>
    </Details>
  );
};

export default TableHeadersForm;
