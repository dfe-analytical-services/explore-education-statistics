import styles from '@common/modules/table-tool/components/TableHeadersGroup.module.scss';
import TableHeadersGroupControls from '@common/modules/table-tool/components/TableHeadersGroupControls';
import TableHeadersReadOnlyList from '@common/modules/table-tool/components/TableHeadersReadOnlyList';
import TableHeadersReorderableList from '@common/modules/table-tool/components/TableHeadersReorderableList';
import useTableHeadersContext from '@common/modules/table-tool/contexts/TableHeadersContext';
import classNames from 'classnames';
import React, { useState } from 'react';
import { Draggable } from 'react-beautiful-dnd';

interface Props {
  id: string;
  index: number;
  isLastGroup: boolean;
  legend: string;
  name: string;
  totalItems: number;
  onMoveGroupToOtherAxis: () => void;
  onMoveGroupDown: () => void;
  onMoveGroupUp: () => void;
}

const TableHeadersGroup = ({
  id,
  index,
  isLastGroup,
  legend,
  name,
  totalItems,
  onMoveGroupToOtherAxis,
  onMoveGroupDown,
  onMoveGroupUp,
}: Props) => {
  const {
    activeGroup,
    groupDraggingActive,
    groupDraggingEnabled,
    moveControlsActive,
  } = useTableHeadersContext();
  const [focusedGroup, setFocusedGroup] = useState<string>();
  const defaultNumberOfItems = 2;
  const showMovingControls = moveControlsActive.includes(id);

  return (
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
            [styles.groupActive]: activeGroup === id,
            [styles.showMovingControls]: showMovingControls,
          })}
          ref={draggableProvided.innerRef}
        >
          {activeGroup === id ? (
            <TableHeadersReorderableList id={id} legend={legend} name={name} />
          ) : (
            <div
              // eslint-disable-next-line react/jsx-props-no-spreading
              {...draggableProvided.dragHandleProps}
              className={styles.groupDragHandle}
              data-testid={`draggable-${id}`}
              onBlur={() => setFocusedGroup(undefined)}
              onFocus={() => setFocusedGroup(name)}
            >
              <TableHeadersReadOnlyList
                defaultNumberOfItems={defaultNumberOfItems}
                id={id}
                legend={legend}
                name={name}
              />
            </div>
          )}
          <TableHeadersGroupControls
            defaultNumberOfItems={defaultNumberOfItems}
            groupName={name}
            id={id}
            index={index}
            isLastGroup={isLastGroup}
            legend={legend}
            showMovingControls={showMovingControls}
            totalItems={totalItems}
            onMoveAxis={onMoveGroupToOtherAxis}
            onMoveDown={onMoveGroupDown}
            onMoveUp={onMoveGroupUp}
          />
        </div>
      )}
    </Draggable>
  );
};

export default TableHeadersGroup;
