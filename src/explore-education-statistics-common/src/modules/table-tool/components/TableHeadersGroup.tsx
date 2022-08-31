import styles from '@common/modules/table-tool/components/TableHeadersGroup.module.scss';
import TableHeadersGroupControls from '@common/modules/table-tool/components/TableHeadersGroupControls';
import TableHeadersReadOnlyList from '@common/modules/table-tool/components/TableHeadersReadOnlyList';
import TableHeadersReorderableList from '@common/modules/table-tool/components/TableHeadersReorderableList';
import getTableHeaderGroupId from '@common/modules/table-tool/components/utils/getTableHeaderGroupId';
import useTableHeadersContext from '@common/modules/table-tool/contexts/TableHeadersContext';
import classNames from 'classnames';
import React, { useState } from 'react';
import { Draggable } from 'react-beautiful-dnd';

interface Props {
  index: number;
  legend: string;
  name: string;
  totalItems: number;
  onMoveGroupToOtherAxis: () => void;
}

const TableHeadersGroup = ({
  index,
  legend,
  name,
  totalItems,
  onMoveGroupToOtherAxis,
}: Props) => {
  const {
    activeGroup,
    groupDraggingActive,
    groupDraggingEnabled,
  } = useTableHeadersContext();
  const [focusedGroup, setFocusedGroup] = useState<string>();
  const groupId = getTableHeaderGroupId(legend);
  const defaultNumberOfItems = 2;

  return (
    <>
      <Draggable
        draggableId={name}
        index={index}
        isDragDisabled={!groupDraggingEnabled}
      >
        {(draggableProvided, draggableSnapshot) => (
          <div
            // eslint-disable-next-line react/jsx-props-no-spreading
            {...draggableProvided.draggableProps}
            // eslint-disable-next-line react/jsx-props-no-spreading
            className={classNames(styles.group, {
              [styles.isDragging]: draggableSnapshot.isDragging,
              [styles.isDraggingActive]:
                !draggableSnapshot.isDragging && groupDraggingActive,
              [styles.isDraggedOutside]:
                draggableSnapshot.isDragging && !draggableSnapshot.draggingOver,
              [styles.dragEnabled]: groupDraggingEnabled,
              [styles.focused]: focusedGroup === name,
            })}
            ref={draggableProvided.innerRef}
          >
            {activeGroup === groupId ? (
              <TableHeadersReorderableList
                id={groupId}
                legend={legend}
                name={name}
              />
            ) : (
              <div
                // eslint-disable-next-line react/jsx-props-no-spreading
                {...draggableProvided.dragHandleProps}
                className={styles.groupDragHandle}
                data-testid={`draggable-${groupId}`}
                onBlur={() => setFocusedGroup(undefined)}
                onFocus={() => setFocusedGroup(name)}
              >
                <TableHeadersReadOnlyList
                  defaultNumberOfItems={defaultNumberOfItems}
                  id={groupId}
                  legend={legend}
                  name={name}
                />
              </div>
            )}
            <TableHeadersGroupControls
              defaultNumberOfItems={defaultNumberOfItems}
              groupName={name}
              id={groupId}
              legend={legend}
              totalItems={totalItems}
              onMove={onMoveGroupToOtherAxis}
            />
          </div>
        )}
      </Draggable>
    </>
  );
};

export default TableHeadersGroup;
