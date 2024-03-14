import { FormFieldset } from '@common/components/form';
import styles from '@common/modules/table-tool/components/TableHeadersAxis.module.scss';
import TableHeadersGroup from '@common/modules/table-tool/components/TableHeadersGroup';
import { TableHeadersFormValues } from '@common/modules/table-tool/components/TableHeadersForm';
import useTableHeadersContext from '@common/modules/table-tool/contexts/TableHeadersContext';
import createRHFErrorHelper from '@common/components/form/rhf/validation/createRHFErrorHelper';
import getTableHeaderGroupId from '@common/modules/table-tool/components/utils/getTableHeaderGroupId';
import {
  CategoryFilter,
  LocationFilter,
  TimePeriodFilter,
  Filter,
} from '@common/modules/table-tool/types/filters';
import classNames from 'classnames';
import React from 'react';
import { Droppable } from 'react-beautiful-dnd';
import { useFormContext } from 'react-hook-form';

interface Props {
  id: string;
  legend: string;
  name: keyof TableHeadersFormValues;
  onMoveGroupDown: (index: number) => void;
  onMoveGroupUp: (index: number) => void;
  onMoveGroupToOtherAxis: (index: number) => void;
}

export default function TableHeadersAxis({
  id,
  legend,
  name,
  onMoveGroupDown,
  onMoveGroupUp,
  onMoveGroupToOtherAxis,
}: Props) {
  const { groupDraggingActive, groupDraggingEnabled } =
    useTableHeadersContext();

  const { formState, getValues } = useFormContext<TableHeadersFormValues>();

  const values = getValues(name);

  const { getError } = createRHFErrorHelper({
    errors: formState.errors,
    touchedFields: formState.touchedFields,
  });

  return (
    <Droppable
      droppableId={name}
      direction="horizontal"
      isDropDisabled={!groupDraggingEnabled}
    >
      {droppableProvided => (
        <div
          // eslint-disable-next-line react/jsx-props-no-spreading
          {...droppableProvided.droppableProps}
          ref={droppableProvided.innerRef}
          className={classNames(styles.container, {
            [styles.isDraggingActive]: groupDraggingActive,
          })}
          data-testid={id}
        >
          {groupDraggingEnabled ? (
            <FormFieldset
              id={id}
              legend={legend}
              legendWeight="regular"
              error={getError(name)}
              legendSize="m"
            >
              <div className={styles.groupsContainer}>
                {values.length === 0 && (
                  <div className="govuk-inset-text govuk-!-margin-0">
                    Add groups by dragging them here
                  </div>
                )}

                {values.map((group: Filter[], index: number) => {
                  const key = `group-${index}`;
                  const groupName = `${name}[${index}]`;
                  const groupLegend = getGroupLegend(group);
                  const groupId = getTableHeaderGroupId(groupLegend);

                  return (
                    <div
                      className={styles.groupContainer}
                      data-testid={`${name}-${index}`}
                      key={key}
                    >
                      <TableHeadersGroup
                        id={groupId}
                        index={index}
                        isLastGroup={index === values.length - 1}
                        legend={groupLegend}
                        name={groupName}
                        totalItems={group.length}
                        onMoveGroupDown={() => {
                          onMoveGroupDown(index);
                        }}
                        onMoveGroupUp={() => {
                          onMoveGroupUp(index);
                        }}
                        onMoveGroupToOtherAxis={() => {
                          onMoveGroupToOtherAxis(index);
                        }}
                      />
                    </div>
                  );
                })}
                {droppableProvided.placeholder}
              </div>
            </FormFieldset>
          ) : (
            <>
              <h3 className="govuk-!-font-weight-regular govuk-!-margin-bottom-3">
                {legend}
              </h3>
              <div className={styles.groupsContainer}>
                {values.map((group: Filter[], index: number) => {
                  const key = `group-${index}`;
                  const groupName = `${name}[${index}]`;
                  const groupLegend = getGroupLegend(group);
                  const groupId = getTableHeaderGroupId(groupLegend);

                  return (
                    <div
                      className={styles.groupContainer}
                      data-testid={`${name}-${index}`}
                      key={key}
                    >
                      <TableHeadersGroup
                        id={groupId}
                        index={index}
                        isLastGroup={index === values.length - 1}
                        legend={groupLegend}
                        name={groupName}
                        totalItems={group.length}
                        onMoveGroupDown={() => {
                          onMoveGroupDown(index);
                        }}
                        onMoveGroupUp={() => {
                          onMoveGroupUp(index);
                        }}
                        onMoveGroupToOtherAxis={() => {
                          onMoveGroupToOtherAxis(index);
                        }}
                      />
                    </div>
                  );
                })}
              </div>
            </>
          )}
        </div>
      )}
    </Droppable>
  );
}

function getGroupLegend(group: Filter[]) {
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
}
