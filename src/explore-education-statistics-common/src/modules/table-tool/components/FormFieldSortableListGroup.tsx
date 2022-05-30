import Button from '@common/components/Button';
import { FormFieldset } from '@common/components/form';
import { useFormContext } from '@common/components/form/contexts/FormContext';
import DragIcon from '@common/modules/table-tool/components/DragIcon';
import FormFieldSortableList from '@common/modules/table-tool/components/FormFieldSortableList';
import styles from '@common/modules/table-tool/components/FormFieldSortableListGroup.module.scss';
import {
  CategoryFilter,
  LocationFilter,
  TimePeriodFilter,
  Filter,
} from '@common/modules/table-tool/types/filters';
import useToggle from '@common/hooks/useToggle';
import classNames from 'classnames';
import { useField } from 'formik';
import React from 'react';
import { Draggable, Droppable } from 'react-beautiful-dnd';

const getGroupLegend = (group: Filter[]) => {
  if (group[0] instanceof CategoryFilter) {
    return group[0].category;
  }
  if (group[0] instanceof LocationFilter) {
    return 'Locations';
  }
  if (group[0] instanceof TimePeriodFilter) {
    return 'Time periods';
  }
  return 'Indicators';
};

interface Props<FormValues> {
  id?: string;
  name: FormValues extends Record<string, unknown> ? keyof FormValues : string;
  legend: string;
  isDraggingGroup: boolean;
  onChangeReorderingType: (
    axisName: string,
    isReorderingGroups: boolean,
  ) => void;
  onMoveGroupToOtherAxis: (index: number) => void;
}

function FormFieldSortableListGroup<FormValues>({
  id: customId,
  name,
  legend,
  isDraggingGroup = false,
  onChangeReorderingType,
  onMoveGroupToOtherAxis,
}: Props<FormValues>) {
  const { prefixFormId, fieldId } = useFormContext();
  const id = customId ? prefixFormId(customId) : fieldId(name as string);

  const [field, meta] = useField(name as string);
  const [isDragDisabled, toggleDragDisabled] = useToggle(true);

  return (
    <Droppable droppableId={name as string} direction="horizontal">
      {(droppableProvided, droppableSnapshot) => (
        <div
          // eslint-disable-next-line react/jsx-props-no-spreading
          {...droppableProvided.droppableProps}
          ref={droppableProvided.innerRef}
          className={classNames(styles.container, {
            [styles.isDraggingOver]: droppableSnapshot.isDraggingOver,
            [styles.isDragDisabled]: isDragDisabled,
          })}
          data-testid={id}
        >
          <FormFieldset
            id={id}
            legend={legend}
            error={meta.error}
            legendSize="m"
          >
            <Button
              className={styles.toggleReorderGroups}
              variant="secondary"
              onClick={() => {
                onChangeReorderingType(legend, isDragDisabled);
                toggleDragDisabled();
              }}
            >
              {isDragDisabled
                ? `Re-order ${legend.toLowerCase()}`
                : 'Re-order items'}
            </Button>
            <div
              className={classNames(styles.groupsContainer, {
                [styles.isActive]: !isDragDisabled,
              })}
            >
              {field.value.length === 0 && (
                <div className="govuk-inset-text govuk-!-margin-0">
                  Add groups by dragging them here
                </div>
              )}

              {field.value.map((group: Filter[], index: number) => (
                <div
                  className={styles.groupContainer}
                  // eslint-disable-next-line react/no-array-index-key
                  key={index}
                >
                  <Draggable
                    // eslint-disable-next-line react/no-array-index-key
                    key={index}
                    draggableId={`${name}-${index}`}
                    isDragDisabled={isDragDisabled}
                    index={index}
                  >
                    {(draggableProvided, draggableSnapshot) => (
                      <div
                        // eslint-disable-next-line react/jsx-props-no-spreading
                        {...draggableProvided.draggableProps}
                        // eslint-disable-next-line react/jsx-props-no-spreading
                        {...draggableProvided.dragHandleProps}
                        className={classNames(styles.group, {
                          [styles.isDragging]: draggableSnapshot.isDragging,
                          [styles.isDraggedOutside]:
                            draggableSnapshot.isDragging &&
                            !draggableSnapshot.draggingOver,
                          [styles.groupIsActive]: !isDragDisabled,
                        })}
                        ref={draggableProvided.innerRef}
                        role={isDragDisabled ? '' : 'button'}
                        tabIndex={isDragDisabled ? -1 : 0}
                      >
                        <FormFieldSortableList
                          isGroupDragDisabled={isDragDisabled}
                          name={`${name}[${index}]`}
                          legend={
                            <>
                              {getGroupLegend(group)}
                              <DragIcon className={styles.dragIcon} />
                            </>
                          }
                          legendSize="s"
                        />
                      </div>
                    )}
                  </Draggable>
                  {!isDragDisabled && !isDraggingGroup && (
                    <Button
                      className={styles.moveButton}
                      onClick={e => {
                        e.preventDefault();
                        onMoveGroupToOtherAxis(index);
                      }}
                    >
                      {`Move ${getGroupLegend(group)} group to ${
                        name === 'rowGroups' ? 'columns' : 'rows'
                      }`}
                    </Button>
                  )}
                </div>
              ))}
              {droppableProvided.placeholder}
            </div>
          </FormFieldset>
        </div>
      )}
    </Droppable>
  );
}

export default FormFieldSortableListGroup;
