import Button from '@common/components/Button';
import { FormFieldset } from '@common/components/form';
import { useFormIdContext } from '@common/components/form/contexts/FormIdContext';
import DragIcon from '@admin/prototypes/components/PrototypeDragIcon';
import FormFieldSortableList from '@admin/prototypes/components/PrototypeFormFieldSortableList';
import styles from '@admin/prototypes/components/PrototypeFormFieldSortableListGroup.module.scss';
import {
  CategoryFilter,
  LocationFilter,
  TimePeriodFilter,
  Filter,
} from '@common/modules/table-tool/types/filters';
import classNames from 'classnames';
import { useField } from 'formik';
import React, { useState } from 'react';
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

interface GroupButtonsProps {
  axisName: string;
  disabled?: boolean;
  legend: string;
  listActive?: boolean;
  onClickDone?: () => void;
  onClickMove?: () => void;
  onClickReorder?: () => void;
}

const GroupButtons = ({
  axisName,
  disabled = false,
  legend,
  listActive = false,
  onClickDone,
  onClickMove,
  onClickReorder,
}: GroupButtonsProps) => {
  return (
    <div className={styles.buttonsContainer}>
      {listActive ? (
        <Button
          onClick={e => {
            e.preventDefault();
            onClickDone?.();
          }}
        >
          Done
        </Button>
      ) : (
        <>
          <Button
            disabled={disabled}
            onClick={e => {
              e.preventDefault();
              onClickReorder?.();
            }}
          >
            Reorder
            <span className="govuk-visually-hidden">{` items in ${legend}`}</span>
          </Button>
          <Button
            className={styles.moveButton}
            disabled={disabled}
            onClick={e => {
              e.preventDefault();
              onClickMove?.();
            }}
          >
            Move <span className="govuk-visually-hidden">{legend} </span>
            to {axisName === 'rowGroups' ? 'columns' : 'rows'}
          </Button>
        </>
      )}
    </div>
  );
};

interface Props<FormValues> {
  id?: string;
  hint?: string;
  isDraggingGroup: boolean;
  legend: string;
  name: FormValues extends Record<string, unknown> ? keyof FormValues : string;
  readOnly: boolean;
  onChangeReorderingType: (
    axisName: string,
    isReorderingGroups: boolean,
  ) => void;
  onMoveGroupToOtherAxis: (index: number) => void;
  onReorderingList: () => void;
}

function FormFieldSortableListGroup<FormValues>({
  hint,
  id: customId,
  isDraggingGroup = false,
  legend,
  name,
  readOnly = false,
  onMoveGroupToOtherAxis,
  onReorderingList,
}: Props<FormValues>) {
  const { fieldId } = useFormIdContext();
  const id = fieldId(name as string, customId);
  const [field, meta] = useField(name as string);
  const [activeList, setActiveList] = useState<number | undefined>(undefined);

  if (readOnly) {
    return (
      <div className={classNames(styles.container)}>
        <h3>{legend}</h3>
        <div className={styles.groupsContainer}>
          {field.value.map((group: Filter[], index: number) => {
            const key = `group=${index}`;
            return (
              <div className={styles.groupContainer} key={key}>
                <div className={styles.group}>
                  <FormFieldSortableList
                    focus={activeList === index}
                    legend={
                      <>
                        {getGroupLegend(group)}
                        <DragIcon className={styles.dragIcon} />
                      </>
                    }
                    legendSize="s"
                    name={`${name as string}[${index}]`}
                    readOnly={activeList !== index}
                  />
                </div>

                <GroupButtons
                  axisName={name as string}
                  disabled={activeList !== index}
                  legend={getGroupLegend(group)}
                  listActive={activeList === index}
                  onClickDone={() => {
                    setActiveList(undefined);
                    onReorderingList();
                  }}
                />
              </div>
            );
          })}
        </div>
      </div>
    );
  }

  return (
    <Droppable droppableId={name as string} direction="horizontal">
      {droppableProvided => (
        <div
          // eslint-disable-next-line react/jsx-props-no-spreading
          {...droppableProvided.droppableProps}
          ref={droppableProvided.innerRef}
          className={classNames(styles.container, {
            [styles.dragActive]: isDraggingGroup,
          })}
          data-testid={id}
        >
          <FormFieldset
            id={id}
            legend={legend}
            legendWeight="regular"
            error={meta.error}
            legendSize="m"
            hint={hint}
          >
            <div className={styles.groupsContainer}>
              {field.value.length === 0 && (
                <div className="govuk-inset-text govuk-!-margin-0">
                  Add groups by dragging them here
                </div>
              )}

              {field.value.map((group: Filter[], index: number) => {
                const key = `group-${index}`;
                return (
                  <div className={styles.groupContainer} key={key}>
                    <Draggable
                      draggableId={`${name as string}-${index}`}
                      index={index}
                    >
                      {(draggableProvided, draggableSnapshot) => (
                        <div
                          // eslint-disable-next-line react/jsx-props-no-spreading
                          {...draggableProvided.draggableProps}
                          // eslint-disable-next-line react/jsx-props-no-spreading
                          {...draggableProvided.dragHandleProps}
                          className={classNames(
                            styles.group,
                            styles.groupIsActive,
                            {
                              [styles.isDragging]: draggableSnapshot.isDragging,
                              [styles.isDraggedOutside]:
                                draggableSnapshot.isDragging &&
                                !draggableSnapshot.draggingOver,
                            },
                          )}
                          ref={draggableProvided.innerRef}
                          role="button"
                          tabIndex={0}
                        >
                          <FormFieldSortableList
                            focus={activeList === index}
                            legend={
                              <>
                                {getGroupLegend(group)}
                                <DragIcon className={styles.dragIcon} />
                              </>
                            }
                            legendSize="s"
                            name={`${name as string}[${index}]`}
                            readOnly
                          />
                        </div>
                      )}
                    </Draggable>
                    {!isDraggingGroup && (
                      <GroupButtons
                        axisName={name as string}
                        legend={getGroupLegend(group)}
                        onClickMove={() => onMoveGroupToOtherAxis(index)}
                        onClickReorder={() => {
                          setActiveList(index);
                          onReorderingList();
                        }}
                      />
                    )}
                  </div>
                );
              })}
              {droppableProvided.placeholder}
            </div>
          </FormFieldset>
        </div>
      )}
    </Droppable>
  );
}

export default FormFieldSortableListGroup;
