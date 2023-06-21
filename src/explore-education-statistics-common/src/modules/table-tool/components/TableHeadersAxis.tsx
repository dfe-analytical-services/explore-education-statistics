import { FormFieldset } from '@common/components/form';
import styles from '@common/modules/table-tool/components/TableHeadersAxis.module.scss';
import TableHeadersGroup from '@common/modules/table-tool/components/TableHeadersGroup';
import { TableHeadersFormValues } from '@common/modules/table-tool/components/TableHeadersForm';
import useTableHeadersContext from '@common/modules/table-tool/contexts/TableHeadersContext';
import {
  CategoryFilter,
  LocationFilter,
  TimePeriodFilter,
  Filter,
} from '@common/modules/table-tool/types/filters';
import classNames from 'classnames';
import { useField } from 'formik';
import { Droppable } from 'react-beautiful-dnd';
import React from 'react';

interface Props {
  id: string;
  legend: string;
  name: keyof TableHeadersFormValues;
  onMoveGroupToOtherAxis: (index: number) => void;
}

export default function TableHeadersAxis({
  id,
  legend,
  name,
  onMoveGroupToOtherAxis,
}: Props) {
  const { groupDraggingActive, groupDraggingEnabled } =
    useTableHeadersContext();
  const [field, meta] = useField(name);

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
              error={meta.error}
              legendSize="m"
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
                    <div
                      className={styles.groupContainer}
                      data-testid={`${name}-${index}`}
                      key={key}
                    >
                      <TableHeadersGroup
                        index={index}
                        legend={getGroupLegend(group)}
                        name={`${name}[${index}]`}
                        totalItems={group.length}
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
                {field.value.map((group: Filter[], index: number) => {
                  const key = `group-${index}`;
                  return (
                    <div
                      className={styles.groupContainer}
                      data-testid={`${name}-${index}`}
                      key={key}
                    >
                      <TableHeadersGroup
                        index={index}
                        legend={getGroupLegend(group)}
                        name={`${name}[${index}]`}
                        totalItems={group.length}
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
