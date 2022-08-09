import styles from '@common/modules/table-tool/components/TableHeadersDraggableGroup.module.scss';
import TableHeadersGroupControls from '@common/modules/table-tool/components/TableHeadersGroupControls';
import TableHeadersReadOnlyList from '@common/modules/table-tool/components/TableHeadersReadOnlyList';
import getTableHeaderGroupId from '@common/modules/table-tool/components/utils/getTableHeaderGroupId';
import useTableHeadersContext from '@common/modules/table-tool/contexts/TableHeadersContext';
import classNames from 'classnames';
import React from 'react';
import { Draggable } from 'react-beautiful-dnd';

interface Props {
  index: number;
  legend: string;
  name: string;
  totalItems: number;
  onMoveGroupToOtherAxis: () => void;
}

const TableHeadersDraggableGroup = ({
  index,
  legend,
  name,
  totalItems,
  onMoveGroupToOtherAxis,
}: Props) => {
  const { groupDraggingActive } = useTableHeadersContext();
  const groupId = getTableHeaderGroupId(legend);

  return (
    <>
      <Draggable draggableId={name} index={index}>
        {(draggableProvided, draggableSnapshot) => (
          <div
            // eslint-disable-next-line react/jsx-props-no-spreading
            {...draggableProvided.draggableProps}
            // eslint-disable-next-line react/jsx-props-no-spreading
            {...draggableProvided.dragHandleProps}
            className={classNames(styles.group, {
              [styles.isDragging]: draggableSnapshot.isDragging,
              [styles.isDraggingActive]:
                !draggableSnapshot.isDragging && groupDraggingActive,
              [styles.isDraggedOutside]:
                draggableSnapshot.isDragging && !draggableSnapshot.draggingOver,
            })}
            ref={draggableProvided.innerRef}
            role="button"
            tabIndex={0}
          >
            <TableHeadersReadOnlyList
              id={groupId}
              legend={legend}
              name={name}
            />
          </div>
        )}
      </Draggable>
      {!groupDraggingActive && (
        <TableHeadersGroupControls
          axisName={name as string}
          id={groupId}
          legend={legend}
          totalItems={totalItems}
          onMove={onMoveGroupToOtherAxis}
        />
      )}
    </>
  );
};

export default TableHeadersDraggableGroup;
