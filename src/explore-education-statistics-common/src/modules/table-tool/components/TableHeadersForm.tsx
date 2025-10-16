import Button from '@common/components/Button';
import { FormGroup } from '@common/components/form';
import FormProvider from '@common/components/form/FormProvider';
import Form from '@common/components/form/Form';
import ScreenReaderMessage from '@common/components/ScreenReaderMessage';
import useToggle from '@common/hooks/useToggle';
import useMounted from '@common/hooks/useMounted';
import TableHeadersAxis from '@common/modules/table-tool/components/TableHeadersAxis';
import { TableHeadersContextProvider } from '@common/modules/table-tool/contexts/TableHeadersContext';
import { Filter } from '@common/modules/table-tool/types/filters';
import { TableHeadersConfig } from '@common/modules/table-tool/types/tableHeaders';
import styles from '@common/modules/table-tool/components/TableHeadersForm.module.scss';
import reorder from '@common/utils/reorder';
import Yup from '@common/validation/yup';
import compact from 'lodash/compact';
import last from 'lodash/last';
import React, { useCallback, useMemo, useState } from 'react';
import { DragDropContext, DropResult } from '@hello-pangea/dnd';
import { UseFormReturn } from 'react-hook-form';
import { ObjectSchema } from 'yup';

export interface TableHeadersFormValues {
  rowGroups: Filter[][];
  columnGroups: Filter[][];
}

interface Props {
  initialValues: TableHeadersConfig;
  onSubmit: (values: TableHeadersConfig) => void | Promise<void>;
}

export default function TableHeadersForm({ onSubmit, initialValues }: Props) {
  const { isMounted } = useMounted();
  const [screenReaderMessage, setScreenReaderMessage] = useState('');
  const [showTableHeadersForm, toggleShowTableHeadersForm] = useToggle(false);
  const id = 'tableHeaderForm';

  const handleSubmit = useCallback(
    async (values: TableHeadersFormValues) => {
      await onSubmit({
        columnGroups:
          values.columnGroups.length > 1
            ? values.columnGroups.slice(0, -1)
            : [],
        rowGroups:
          values.rowGroups.length > 1 ? values.rowGroups.slice(0, -1) : [],
        columns: last(values.columnGroups) as Filter[],
        rows: last(values.rowGroups) as Filter[],
      });

      toggleShowTableHeadersForm.off();
    },
    [onSubmit, toggleShowTableHeadersForm],
  );

  const moveGroupToAxis = ({
    destinationId,
    destinationIndex,
    sourceId,
    sourceIndex,
    form,
  }: {
    destinationId: keyof TableHeadersFormValues;
    destinationIndex: number;
    sourceId: keyof TableHeadersFormValues;
    sourceIndex: number;
    form: UseFormReturn<TableHeadersFormValues>;
  }) => {
    const nextSourceValue = form.getValues(sourceId);
    const nextDestinationValue = form.getValues(destinationId);

    const [sourceItem] = nextSourceValue.splice(sourceIndex, 1);

    nextDestinationValue.splice(destinationIndex, 0, sourceItem);

    form.setValue(sourceId, nextSourceValue, { shouldTouch: true });
    form.setValue(destinationId, nextDestinationValue, { shouldTouch: true });

    form.trigger(destinationId);
  };

  const handleMoveGroupToOtherAxis = ({
    groupIndex,
    sourceId,
    form,
  }: {
    groupIndex: number;
    sourceId: keyof TableHeadersFormValues;
    form: UseFormReturn<TableHeadersFormValues>;
  }) => {
    const destinationId: keyof TableHeadersFormValues =
      sourceId === 'rowGroups' ? 'columnGroups' : 'rowGroups';
    const destinationIndex = form.getValues(destinationId).length;

    moveGroupToAxis({
      destinationId,
      destinationIndex,
      sourceIndex: groupIndex,
      sourceId,
      form,
    });

    const message =
      sourceId === 'rowGroups'
        ? 'You have moved the group from row headers to column headers'
        : 'You have moved the group from column headers to row headers';

    setScreenReaderMessage(message);
  };

  // Move group within axis via button controls
  const handleMoveGroup = ({
    groupIndex,
    sourceId,
    form,
    direction,
  }: {
    groupIndex: number;
    sourceId: keyof TableHeadersFormValues;
    form: UseFormReturn<TableHeadersFormValues>;
    direction: 'up' | 'down';
  }) => {
    const sourceValue = form.getValues(sourceId);
    const newIndex = direction === 'up' ? groupIndex - 1 : groupIndex + 1;
    if (
      (direction === 'up' && groupIndex === 0) ||
      (direction === 'down' && groupIndex === sourceValue.length - 1)
    ) {
      return;
    }
    const reordered = reorder(sourceValue, groupIndex, newIndex);

    form.setValue(sourceId, reordered);
    form.trigger(sourceId);
  };

  const handleDragEnd = ({
    result,
    form,
  }: {
    result: DropResult;
    form: UseFormReturn<TableHeadersFormValues>;
  }) => {
    const { source, destination } = result;

    if (!destination) {
      return;
    }

    const destinationId =
      destination.droppableId as keyof TableHeadersFormValues;
    const sourceId = source.droppableId as keyof TableHeadersFormValues;

    // Moving group within its axis
    if (destinationId === sourceId) {
      const values = form.getValues(destinationId);
      form.setValue(
        destinationId,
        reorder(values, source.index, destination.index),
      );

      return;
    }

    // Moving group to the other axis
    moveGroupToAxis({
      destinationId,
      destinationIndex: destination.index,
      sourceId,
      sourceIndex: source.index,
      form,
    });
  };

  const validationSchema = useMemo<ObjectSchema<TableHeadersFormValues>>(() => {
    return Yup.object({
      rowGroups: Yup.array()
        .required()
        .of(
          Yup.array()
            .required()
            .of<Filter>(
              Yup.object({
                id: Yup.string().required(),
                label: Yup.string().required('Label is required'),
                value: Yup.string().required('Value is required'),
              }),
            )
            .ensure(),
        )
        .min(1, 'Must have at least one row group'),
      columnGroups: Yup.array()
        .required()
        .of(
          Yup.array()
            .required()
            .of<Filter>(
              Yup.object({
                id: Yup.string().required(),
                label: Yup.string().required('Label is required'),
                value: Yup.string().required('Value is required'),
              }),
            )
            .ensure(),
        )
        .min(1, 'Must have at least one column group'),
    });
  }, []);

  const initialFormValues: TableHeadersFormValues = {
    columnGroups: compact([
      ...(initialValues?.columnGroups ?? []),
      initialValues?.columns,
    ]),
    rowGroups: compact([
      ...(initialValues?.rowGroups ?? []),
      initialValues?.rows,
    ]),
  };

  if (!isMounted) {
    return null;
  }

  return (
    <TableHeadersContextProvider>
      {({ toggleGroupDraggingActive }) => (
        <>
          {!showTableHeadersForm ? (
            <Button
              className="govuk-!-margin-bottom-2"
              ariaControls={id}
              ariaExpanded={showTableHeadersForm}
              onClick={toggleShowTableHeadersForm}
            >
              Move and reorder table headers
            </Button>
          ) : (
            <div className={styles.formContainer} id={id}>
              <div className="govuk-grid-row">
                <div className="govuk-grid-column-two-thirds">
                  <h3>Move and reorder table headers</h3>
                  <h4>Using a mouse, track pad, touch screen and a keyboard</h4>
                  <p className="govuk-hint">
                    Drag and drop or use the keyboard to reorder headers within
                    or between columns and rows. Click the Reorder button on a
                    header group to reorder the items within that group.
                  </p>
                  <p className="govuk-hint">
                    To move multiple items in one action, hold the Ctrl key
                    while you click the items you want to move, and then release
                    Ctrl before dragging and dropping the items into place.
                  </p>
                  <h4>Using only a keyboard</h4>
                  <p className="govuk-hint">
                    For keyboard users, use the Tab key to navigate to items or
                    groups, select and deselect a draggable item by pressing the
                    Spacebar once to start and once to finish the move. Use the
                    arrow keys to move a selected item.
                  </p>
                  <p className="govuk-hint">
                    To move multiple items in one action, press both Ctrl and
                    Enter keys together to add an item to your selected items.
                    Then press the Spacebar to start the move, use the arrow
                    keys to move the selected items and press the Spacebar again
                    to complete the move.
                  </p>
                  <div className="govuk-visually-hidden">
                    <p>
                      For screen reader users, you may need to change mode to
                      use the keyboard controls:
                    </p>
                    <ul>
                      <li>
                        If you are using NVDA, enable focus mode (Insert + Space
                        bar)
                      </li>
                      <li>
                        If you are using JAWS, enable the virtual cursor (Insert
                        + Z)
                      </li>
                      <li>
                        If you are using Windows Narrator, disable scan mode
                        (Caps lock + Space bar)
                      </li>
                    </ul>
                  </div>
                </div>
              </div>

              <FormProvider
                enableReinitialize
                initialValues={initialFormValues}
                validationSchema={validationSchema}
              >
                {form => {
                  return (
                    <Form
                      id={`${id}-form`}
                      showErrorSummary={false}
                      onSubmit={handleSubmit}
                    >
                      <DragDropContext
                        onDragEnd={result => {
                          handleDragEnd({ result, form });
                          toggleGroupDraggingActive(false);
                        }}
                        onDragStart={() => {
                          toggleGroupDraggingActive(true);
                        }}
                      >
                        <FormGroup className="govuk-!-margin-bottom-4">
                          <TableHeadersAxis
                            id="columnGroups"
                            legend="Move column headers"
                            name="columnGroups"
                            onMoveGroupDown={groupIndex => {
                              handleMoveGroup({
                                groupIndex,
                                sourceId: 'columnGroups',
                                form,
                                direction: 'down',
                              });
                            }}
                            onMoveGroupUp={groupIndex => {
                              handleMoveGroup({
                                groupIndex,
                                sourceId: 'columnGroups',
                                form,
                                direction: 'up',
                              });
                            }}
                            onMoveGroupToOtherAxis={groupIndex => {
                              handleMoveGroupToOtherAxis({
                                groupIndex,
                                sourceId: 'columnGroups',
                                form,
                              });
                            }}
                          />

                          <TableHeadersAxis
                            id="rowGroups"
                            legend="Move row headers"
                            name="rowGroups"
                            onMoveGroupDown={groupIndex => {
                              handleMoveGroup({
                                groupIndex,
                                sourceId: 'rowGroups',
                                form,
                                direction: 'down',
                              });
                            }}
                            onMoveGroupUp={groupIndex => {
                              handleMoveGroup({
                                groupIndex,
                                sourceId: 'rowGroups',
                                form,
                                direction: 'up',
                              });
                            }}
                            onMoveGroupToOtherAxis={groupIndex => {
                              handleMoveGroupToOtherAxis({
                                groupIndex,
                                sourceId: 'rowGroups',
                                form,
                              });
                            }}
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
              </FormProvider>
            </div>
          )}

          <ScreenReaderMessage message={screenReaderMessage} />
        </>
      )}
    </TableHeadersContextProvider>
  );
}
