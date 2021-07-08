import { FormFieldset } from '@common/components/form';
import { FormFieldsetProps } from '@common/components/form/FormFieldset';
import { Filter } from '@common/modules/table-tool/types/filters';
import reorderMulti from '@common/utils/reorderMulti';
import classNames from 'classnames';
import React, { MouseEventHandler, useState, useEffect } from 'react';
import {
  DragDropContext,
  Draggable,
  Droppable,
  DraggableStateSnapshot,
} from 'react-beautiful-dnd';
import styles from './FormSortableList.module.scss';

type SortableOptionChangeEventHandler = (value: Filter[]) => void;

export type FormSortableListProps = {
  onChange?: SortableOptionChangeEventHandler;
  onMouseEnter?: MouseEventHandler<HTMLDivElement>;
  onMouseLeave?: MouseEventHandler<HTMLDivElement>;
  value: Filter[];
} & FormFieldsetProps;

const FormSortableList = ({
  id,
  onChange,
  onMouseEnter,
  onMouseLeave,
  value,
  ...props
}: FormSortableListProps) => {
  const [selectedIds, setSelectedIds] = useState<string[]>([]);
  const [draggingTaskId, setDraggingTaskId] = useState<string>('');

  const primaryButton = 0; // https://developer.mozilla.org/en-US/docs/Web/API/MouseEvent/button

  const keyCodes = {
    enter: 13,
    escape: 27,
    arrowDown: 40,
    arrowUp: 38,
    tab: 9,
  };

  const toggleSelection = (selectedId: string) => {
    const wasSelected: boolean = selectedIds.includes(selectedId);

    const newTaskIds: string[] = (() => {
      if (!wasSelected || selectedIds.length > 1) {
        return [selectedId];
      }

      return [];
    })();

    setSelectedIds(newTaskIds);
  };

  const toggleSelectionInGroup = (selectedId: string) => {
    const index = selectedIds.indexOf(selectedId);

    if (index === -1) {
      setSelectedIds([...selectedIds, selectedId]);
      return;
    }

    const shallow = [...selectedIds];
    shallow.splice(index, 1);
    setSelectedIds(shallow);
  };

  const unselectAll = () => {
    setSelectedIds([]);
  };

  const onKeyDown = (
    event: React.KeyboardEvent,
    snapshot: DraggableStateSnapshot,
    itemId: string,
  ) => {
    if (
      event.defaultPrevented ||
      snapshot.isDragging ||
      event.keyCode !== keyCodes.enter
    ) {
      return;
    }
    event.preventDefault();
    performAction(event, itemId);
  };

  const onClick = (event: React.MouseEvent, itemId: string) => {
    if (event.defaultPrevented || event.button !== primaryButton) {
      return;
    }
    event.preventDefault();
    performAction(event, itemId);
  };

  const onTouchEnd = (event: React.TouchEvent) => {
    if (event.defaultPrevented) {
      return;
    }
    event.preventDefault();
    toggleSelectionInGroup(id);
  };

  // Determines if the platform specific toggle selection in group key was used
  const wasToggleInSelectionGroupKeyUsed = (
    event: React.MouseEvent | React.KeyboardEvent,
  ) => {
    const isUsingWindows = navigator.platform.indexOf('Win') >= 0;
    return isUsingWindows ? event.ctrlKey : event.metaKey;
  };

  const performAction = (
    event: React.MouseEvent | React.KeyboardEvent,
    itemId: string,
  ) => {
    if (wasToggleInSelectionGroupKeyUsed(event)) {
      toggleSelectionInGroup(itemId);
      return;
    }
    toggleSelection(itemId);
  };

  const onWindowKeyDown = (event: KeyboardEvent) => {
    if (event.defaultPrevented) {
      return;
    }

    if (event.key === 'Escape') {
      unselectAll();
    }
  };

  const onWindowClick = (event: MouseEvent) => {
    if (event.defaultPrevented) {
      return;
    }
    unselectAll();
  };

  const onWindowTouchEnd = (event: TouchEvent) => {
    if (event.defaultPrevented) {
      return;
    }
    unselectAll();
  };

  useEffect(() => {
    window.addEventListener('click', onWindowClick);
    window.addEventListener('keydown', onWindowKeyDown);
    window.addEventListener('touchend', onWindowTouchEnd);

    return function cleanup() {
      window.removeEventListener('click', onWindowClick);
      window.removeEventListener('keydown', onWindowKeyDown);
      window.removeEventListener('touchend', onWindowTouchEnd);
    };
  });

  return (
    <FormFieldset {...props} id={id}>
      <DragDropContext
        onDragStart={start => {
          const selected = selectedIds.includes(start.draggableId);
          if (!selected) {
            unselectAll();
          }
          setDraggingTaskId(start.draggableId);
        }}
        onDragEnd={result => {
          if (!result.destination) {
            return;
          }

          const newValue = reorderMulti({
            list: value,
            sourceIndex: result.source.index,
            destinationIndex: result.destination.index,
            selectedIds: selectedIds.length
              ? selectedIds
              : [result.draggableId],
          });

          setDraggingTaskId('');

          if (onChange) {
            onChange(newValue);
          }
        }}
      >
        <Droppable droppableId={id}>
          {(droppableProvided, droppableSnapshot) => (
            <div
              // eslint-disable-next-line react/jsx-props-no-spreading
              {...droppableProvided.droppableProps}
              className={classNames(styles.list, {
                [styles.listDraggingOver]: droppableSnapshot.isDraggingOver,
              })}
              ref={droppableProvided.innerRef}
              onMouseEnter={onMouseEnter}
              onMouseLeave={onMouseLeave}
            >
              {value.map((option, index) => (
                <Draggable
                  draggableId={option.value}
                  key={option.value}
                  index={index}
                >
                  {(draggableProvided, draggableSnapshot) => (
                    <div
                      // eslint-disable-next-line react/jsx-props-no-spreading
                      {...draggableProvided.draggableProps}
                      // eslint-disable-next-line react/jsx-props-no-spreading
                      {...draggableProvided.dragHandleProps}
                      className={classNames(styles.optionRow, {
                        [styles.optionCurrentDragging]:
                          draggableSnapshot.isDragging,
                        [styles.isSelected]: selectedIds.includes(option.value),
                        [styles.isGhosted]:
                          selectedIds.includes(option.value) &&
                          draggingTaskId &&
                          draggingTaskId !== option.value,
                      })}
                      ref={draggableProvided.innerRef}
                      style={draggableProvided.draggableProps.style}
                      onClick={event => {
                        onClick(event, option.value);
                      }}
                      onKeyDown={(event: React.KeyboardEvent) =>
                        onKeyDown(event, draggableSnapshot, option.value)
                      }
                      onTouchEnd={onTouchEnd}
                      role="button"
                      tabIndex={0}
                    >
                      <div className={styles.optionText}>
                        <strong>{option.label}</strong>
                        <span>⇅</span>
                      </div>
                    </div>
                  )}
                </Draggable>
              ))}
              {droppableProvided.placeholder}
            </div>
          )}
        </Droppable>
      </DragDropContext>
    </FormFieldset>
  );
};

export default FormSortableList;
