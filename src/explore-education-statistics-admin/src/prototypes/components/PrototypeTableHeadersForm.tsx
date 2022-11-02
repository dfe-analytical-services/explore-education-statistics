import FormFieldSortableListGroup from '@admin/prototypes/components/PrototypeFormFieldSortableListGroup';
import styles from '@admin/prototypes/components/PrototypeTableHeadersForm.module.scss';
import Button from '@admin/prototypes/components/PrototypeButton';
import { FormGroup } from '@common/components/form';
import useMounted from '@common/hooks/useMounted';
import useToggle from '@common/hooks/useToggle';
import { Filter } from '@common/modules/table-tool/types/filters';
import { TableHeadersConfig } from '@common/modules/table-tool/types/tableHeaders';
import { PickByType } from '@common/types';
import reorder from '@common/utils/reorder';
import Yup from '@common/validation/yup';
import classNames from 'classnames';
import { Form, Formik, FormikProps } from 'formik';
import compact from 'lodash/compact';
import last from 'lodash/last';
import React, { useCallback, useState } from 'react';
import { DragDropContext, DropResult } from 'react-beautiful-dnd';

interface FormValues {
  rowGroups: Filter[][];
  columnGroups: Filter[][];
}

interface Props {
  showTableHeadersForm: boolean;
  initialValues?: TableHeadersConfig;
  onSubmit: (values: TableHeadersConfig) => void;
  id?: string;
}

const TableHeadersForm = ({
  showTableHeadersForm,
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
  const [readOnly, toggleReadOnly] = useToggle(false);

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

  const moveGroupToAxis = ({
    destinationId,
    destinationIndex,
    form,
    sourceId,
    sourceIndex,
  }: {
    destinationId: keyof FormValues;
    destinationIndex: number;
    form: FormikProps<FormValues>;
    sourceId: keyof FormValues;
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
    sourceId: keyof FormValues,
    groupIndex: number,
    form: FormikProps<FormValues>,
  ) => {
    const destinationId: keyof FormValues =
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

  const handleDragEnd = (form: FormikProps<FormValues>, result: DropResult) => {
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
    moveGroupToAxis({
      destinationId,
      destinationIndex: destination.index,
      form,
      sourceId,
      sourceIndex: source.index,
    });
  };

  const handleSwitchReorderingType = (
    axisName: string,
    reorderingGroups: boolean,
  ) => {
    const message = reorderingGroups
      ? `Reorder ${axisName} selected, click to change to items`
      : 'Reorder items selected, click to change to groups';

    // Clear the message then repopulate to ensure the new message is read,
    // and only read once.
    setScreenReaderMessage('');
    setTimeout(() => {
      setScreenReaderMessage(message);
    }, 200);
  };

  if (!isMounted) {
    return null;
  }

  return (
    <>
      <div
        className={classNames(styles.formContainer, {
          [styles.hide]: !showTableHeadersForm,
        })}
        id={id}
      >
        <div className="govuk-width-container govuk-!-margin-2 govuk-!-margin-bottom-4">
          <h3>Move and reorder table headers</h3>
          <h4>Using a mouse, track pad, touch screen and a keyboard</h4>
          <p className="govuk-hint">
            Drag and drop or use the keyboard to reorder headers within or
            between columns and rows. Click the Reorder button on a header group
            to reorder the items within that group. Hold the Ctrl key and click
            to select multiple items to drag and drop.{' '}
          </p>
          <h4>Using only a keyboard</h4>
          <p className="govuk-hint">
            For keyboard users, use the Tab key to navigate to items or groups,
            select and deselect a draggable item with Space and use the arrow
            keys to move a selected item. To move multiple items, you can press
            Ctrl and Enter to add an item to your selected items.
          </p>
          <p className="govuk-visually-hidden">
            If you are using a screen reader disable scan mode.
          </p>
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
              <Form id={`${id}-form`}>
                <DragDropContext
                  onDragEnd={result => handleDragEnd(form, result)}
                  onDragStart={toggleIsDraggingGroup.on}
                >
                  <FormGroup className="govuk-!-margin-bottom-4">
                    <FormFieldSortableListGroup<
                      PickByType<TableHeadersConfig, Filter[][]>
                    >
                      isDraggingGroup={isDraggingGroup}
                      legend="Move column headers"
                      name="columnGroups"
                      readOnly={readOnly}
                      onChangeReorderingType={handleSwitchReorderingType}
                      onMoveGroupToOtherAxis={groupIndex =>
                        handleMoveGroupToOtherAxis(
                          'columnGroups',
                          groupIndex,
                          form,
                        )
                      }
                      onReorderingList={toggleReadOnly}
                    />

                    <FormFieldSortableListGroup<
                      PickByType<TableHeadersConfig, Filter[][]>
                    >
                      isDraggingGroup={isDraggingGroup}
                      legend="Move row headers"
                      name="rowGroups"
                      readOnly={readOnly}
                      onChangeReorderingType={handleSwitchReorderingType}
                      onMoveGroupToOtherAxis={groupIndex =>
                        handleMoveGroupToOtherAxis(
                          'rowGroups',
                          groupIndex,
                          form,
                        )
                      }
                      onReorderingList={toggleReadOnly}
                    />
                  </FormGroup>
                </DragDropContext>

                <Button
                  ariaControls={id}
                  ariaExpanded={showTableHeadersForm}
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
  );
};

export default TableHeadersForm;
