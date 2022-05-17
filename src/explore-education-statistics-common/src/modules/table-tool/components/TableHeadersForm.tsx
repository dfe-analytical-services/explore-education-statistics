import Button from '@common/components/Button';
import Details from '@common/components/Details';
import { FormGroup } from '@common/components/form';
import useMounted from '@common/hooks/useMounted';
import useToggle from '@common/hooks/useToggle';
import { Filter } from '@common/modules/table-tool/types/filters';
import { TableHeadersConfig } from '@common/modules/table-tool/types/tableHeaders';
import { PickByType } from '@common/types';
import reorder from '@common/utils/reorder';
import Yup from '@common/validation/yup';
import { Form, Formik, FormikProps } from 'formik';
import compact from 'lodash/compact';
import last from 'lodash/last';
import React, { useCallback, useMemo, useState } from 'react';
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
  const { isMounted } = useMounted();
  const [isDraggingGroup, toggleIsDraggingGroup] = useToggle(false);
  const [screenReaderMessage, setScreenReaderMessage] = useState('');

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

  const handleMoveGroupToOtherAxis = useMemo(
    () => (
      sourceId: keyof FormValues,
      groupIndex: number,
      form: FormikProps<FormValues>,
    ) => {
      const destinationId: keyof FormValues =
        sourceId === 'rowGroups' ? 'columnGroups' : 'rowGroups';

      const nextSourceValue = [...form.values[sourceId]];
      const sourceItem = nextSourceValue.splice(groupIndex, 1);
      const nextDestinationValue = [...form.values[destinationId]].concat(
        sourceItem,
      );

      form.setFieldValue(sourceId, nextSourceValue);
      form.setFieldValue(destinationId, nextDestinationValue);
      form.setFieldTouched(sourceId);
      form.setFieldTouched(destinationId);

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
    },
    [],
  );

  const handleSwitchReorderingType = (
    axisName: string,
    reorderingGroups: boolean,
  ) => {
    const message = reorderingGroups
      ? `Re-order ${axisName} selected, click to change to items`
      : 'Re-order items selected, click to change to groups';

    // Clear the message then repopulate to ensure the new message is read,
    // and only read once.
    setScreenReaderMessage('');
    setTimeout(() => {
      setScreenReaderMessage(message);
    }, 200);
  };

  return (
    <>
      <Details summary="Re-order table headers">
        <p className="govuk-hint">
          Drag and drop the options below to re-order the table headers. Hold
          the Ctrl key and click to select multiple items to drag and drop. For
          keyboard users, use the Tab key to navigate to items or groups, select
          and deselect a draggable item with Space and use the Up and Down arrow
          keys to move a selected item. To move multiple items, you can press
          Ctrl and Enter to add an item to your selected items.
        </p>
        <p className="govuk-visually-hidden">
          If you are using a screen reader disable scan mode.
        </p>
        {isMounted && (
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
                      toggleIsDraggingGroup.off();

                      const { source, destination } = result;

                      if (!destination) {
                        return;
                      }

                      const destinationId = destination.droppableId as keyof FormValues;
                      const sourceId = source.droppableId as keyof FormValues;

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
                    onDragStart={toggleIsDraggingGroup.on}
                  >
                    <FormGroup>
                      <div className="govuk-!-margin-bottom-2">
                        <FormFieldSortableListGroup<
                          PickByType<TableHeadersConfig, Filter[][]>
                        >
                          name="columnGroups"
                          legend="Column groups"
                          isDraggingGroup={isDraggingGroup}
                          onChangeReorderingType={handleSwitchReorderingType}
                          onMoveGroupToOtherAxis={(groupIndex: number) =>
                            handleMoveGroupToOtherAxis(
                              'columnGroups',
                              groupIndex,
                              form,
                            )
                          }
                        />
                      </div>

                      <div className="govuk-!-margin-bottom-2">
                        <FormFieldSortableListGroup<
                          PickByType<TableHeadersConfig, Filter[][]>
                        >
                          name="rowGroups"
                          legend="Row groups"
                          isDraggingGroup={isDraggingGroup}
                          onChangeReorderingType={handleSwitchReorderingType}
                          onMoveGroupToOtherAxis={(groupIndex: number) =>
                            handleMoveGroupToOtherAxis(
                              'rowGroups',
                              groupIndex,
                              form,
                            )
                          }
                        />
                      </div>
                    </FormGroup>
                  </DragDropContext>

                  <Button type="submit">Re-order table</Button>
                </Form>
              );
            }}
          </Formik>
        )}
      </Details>
      <div
        aria-live="assertive"
        aria-atomic="true"
        aria-relevant="additions"
        className="govuk-visually-hidden"
      >
        {screenReaderMessage}
      </div>
    </>
  );
};

export default TableHeadersForm;
