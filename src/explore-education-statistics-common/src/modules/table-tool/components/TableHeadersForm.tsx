import Button from '@common/components/Button';
import { FormGroup } from '@common/components/form';
import useMounted from '@common/hooks/useMounted';
import TableHeadersAxis from '@common/modules/table-tool/components/TableHeadersAxis';
import useTableHeadersContext, {
  TableHeadersContextProvider,
} from '@common/modules/table-tool/contexts/TableHeadersContext';
import { Filter } from '@common/modules/table-tool/types/filters';
import { TableHeadersConfig } from '@common/modules/table-tool/types/tableHeaders';
import styles from '@common/modules/table-tool/components/TableHeadersForm.module.scss';
import { PickByType } from '@common/types';
import reorder from '@common/utils/reorder';
import Yup from '@common/validation/yup';
import { Form, Formik, FormikProps } from 'formik';
import compact from 'lodash/compact';
import last from 'lodash/last';
import React, { useCallback, useState } from 'react';
import { DragDropContext, DropResult } from 'react-beautiful-dnd';

export interface TableHeadersFormValues {
  rowGroups: Filter[][];
  columnGroups: Filter[][];
}

interface TableHeadersFormProps {
  initialValues: TableHeadersConfig;
  onSubmit: (values: TableHeadersConfig) => void;
  id?: string;
}

const TableHeadersForm = ({
  onSubmit,
  id = 'tableHeadersForm',
  initialValues,
}: TableHeadersFormProps) => {
  const { isMounted } = useMounted();
  const [screenReaderMessage, setScreenReaderMessage] = useState('');

  const handleSubmit = useCallback(
    (values: TableHeadersFormValues) => {
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

  const moveGroupToAxis = ({
    destinationId,
    destinationIndex,
    form,
    sourceId,
    sourceIndex,
  }: {
    destinationId: keyof TableHeadersFormValues;
    destinationIndex: number;
    form: FormikProps<TableHeadersFormValues>;
    sourceId: keyof TableHeadersFormValues;
    sourceIndex: number;
  }) => {
    const nextSourceValue = [...form.values[sourceId]];
    const nextDestinationValue = [...form.values[destinationId]];
    const [sourceItem] = nextSourceValue.splice(sourceIndex, 1);
    nextDestinationValue.splice(destinationIndex, 0, sourceItem);

    form.setFieldValue(sourceId, nextSourceValue);
    form.setFieldValue(destinationId, nextDestinationValue);
    form.setFieldTouched(sourceId);
    form.setFieldTouched(destinationId);
  };

  const handleMoveGroupToOtherAxis = (
    sourceId: keyof TableHeadersFormValues,
    groupIndex: number,
    form: FormikProps<TableHeadersFormValues>,
  ) => {
    const destinationId: keyof TableHeadersFormValues =
      sourceId === 'rowGroups' ? 'columnGroups' : 'rowGroups';

    moveGroupToAxis({
      destinationId,
      destinationIndex: form.values[destinationId].length,
      form,
      sourceIndex: groupIndex,
      sourceId,
    });

    const message =
      sourceId === 'rowGroups'
        ? ' You have moved the group from row groups to column groups'
        : 'You have moved the group from column groups to row groups';

    // Clear the message then repopulate to ensure the new message is read,
    // and only read once.
    setScreenReaderMessage('');
    setTimeout(() => {
      setScreenReaderMessage(message);
    }, 200);
  };

  const handleDragEnd = (
    form: FormikProps<TableHeadersFormValues>,
    result: DropResult,
  ) => {
    const { source, destination } = result;

    if (!destination) {
      return;
    }

    const destinationId = destination.droppableId as keyof TableHeadersFormValues;
    const sourceId = source.droppableId as keyof TableHeadersFormValues;

    // Moving group within its axis
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

    // Moving group to the other axis
    moveGroupToAxis({
      destinationId,
      destinationIndex: destination.index,
      form,
      sourceId,
      sourceIndex: source.index,
    });
  };

  if (!isMounted) {
    return null;
  }

  return (
    <TableHeadersContextProvider>
      {({ toggleGroupDraggingActive }) => (
        <>
          <div className={styles.formContainer} id={id}>
            <div className="govuk-grid-row">
              <div className="govuk-grid-column-two-thirds">
                <h3>Move and reorder table headers</h3>
                <h4>Using a mouse, track pad, touch screen and a keyboard</h4>
                <p className="govuk-hint">
                  Drag and drop or use the keyboard to reorder headers within or
                  between columns and rows. Click the Reorder button on a header
                  group to reorder the items within that group. Hold the Ctrl
                  key and click to select multiple items to drag and drop.
                </p>
                <h4>Using only a keyboard</h4>
                <p className="govuk-hint">
                  For keyboard users, use the Tab key to navigate to items or
                  groups, select and deselect a draggable item with Space and
                  use the arrow keys to move a selected item. To move multiple
                  items, you can press Ctrl and Enter to add an item to your
                  selected items.
                </p>
                <p className="govuk-visually-hidden">
                  If you are using a screen reader disable scan mode.
                </p>
              </div>
            </div>

            <Formik<TableHeadersFormValues>
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
              validationSchema={Yup.object<TableHeadersFormValues>({
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
                  <Form id={`${id}-form`}>
                    <DragDropContext
                      onDragEnd={result => {
                        handleDragEnd(form, result);
                        toggleGroupDraggingActive();
                      }}
                      onDragStart={() => {
                        toggleGroupDraggingActive();
                      }}
                    >
                      <FormGroup className="govuk-!-margin-bottom-4">
                        <TableHeadersAxis<
                          PickByType<TableHeadersConfig, Filter[][]>
                        >
                          id="columnGroups"
                          legend="Move column headers"
                          name="columnGroups"
                          onMoveGroupToOtherAxis={groupIndex => {
                            handleMoveGroupToOtherAxis(
                              'columnGroups',
                              groupIndex,
                              form,
                            );
                          }}
                        />

                        <TableHeadersAxis<
                          PickByType<TableHeadersConfig, Filter[][]>
                        >
                          id="rowGroups"
                          legend="Move row headers"
                          name="rowGroups"
                          onMoveGroupToOtherAxis={groupIndex =>
                            handleMoveGroupToOtherAxis(
                              'rowGroups',
                              groupIndex,
                              form,
                            )
                          }
                        />
                      </FormGroup>
                    </DragDropContext>

                    <Button
                      ariaControls={id}
                      ariaExpanded
                      className="govuk-!-margin-left-5 govuk-!-margin-top-3"
                      type="submit"
                    >
                      Update and view reordered table
                    </Button>
                  </Form>
                );
              }}
            </Formik>
          </div>

          <div
            aria-live="assertive"
            aria-atomic="true"
            aria-relevant="additions"
            className="govuk-visually-hidden"
          >
            {screenReaderMessage}
          </div>
        </>
      )}
    </TableHeadersContextProvider>
  );
};

// const TableHeadersForm = (props: TableHeadersFormProps) => (
//   <TableHeadersContextProvider>
//     <TableHeadersFormInner {...props} />
//   </TableHeadersContextProvider>
// );

export default TableHeadersForm;
