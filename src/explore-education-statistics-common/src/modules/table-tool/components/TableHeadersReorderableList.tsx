import styles from '@common/modules/table-tool/components/TableHeadersReorderableList.module.scss';
import TableHeadersReorderableItem from '@common/modules/table-tool/components/TableHeadersReorderableItem';
import { Filter } from '@common/modules/table-tool/types/filters';
import { FormFieldset } from '@common/components/form';
import reorderMultiple from '@common/utils/reorderMultiple';
import reorder from '@common/utils/reorder';
import classNames from 'classnames';
import React, { useEffect, useRef, useState } from 'react';
import {
  DragDropContext,
  Draggable,
  DraggableStateSnapshot,
  Droppable,
} from '@hello-pangea/dnd';
import { useFormContext } from 'react-hook-form';

const primaryButton = 0; // https://developer.mozilla.org/en-US/docs/Web/API/MouseEvent/button

interface Props {
  id: string;
  legend: string;
  name: string;
}

const TableHeadersReorderableList = ({ id, legend, name }: Props) => {
  const [selectedIndices, setSelectedIndices] = useState<number[]>([]);
  const [draggingIndex, setDraggingIndex] = useState<number>();
  const [activeItem, setActiveItem] = useState<string>();

  const { getValues, setValue } = useFormContext();
  /**
   * Focus the list when it's shown to help with keyboard navigation.
   */
  const listRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    listRef.current?.focus();
  }, []);

  useEffect(() => {
    const resetState = () => {
      setDraggingIndex(undefined);
      setSelectedIndices([]);
    };

    const handleWindowKeyDown = (event: KeyboardEvent) => {
      if (event.defaultPrevented) {
        return;
      }

      if (event.key === 'Escape') {
        resetState();
      }
    };

    const handleWindowClick = (event: MouseEvent) => {
      if (event.defaultPrevented) {
        return;
      }

      resetState();
    };

    const handleWindowTouchEnd = (event: TouchEvent) => {
      if (event.defaultPrevented) {
        return;
      }

      resetState();
    };

    // Add event handlers to reset the state if
    // the user clicks outside of the component.
    window.addEventListener('click', handleWindowClick);
    window.addEventListener('keydown', handleWindowKeyDown);
    window.addEventListener('touchend', handleWindowTouchEnd);

    return () => {
      window.removeEventListener('click', handleWindowClick);
      window.removeEventListener('keydown', handleWindowKeyDown);
      window.removeEventListener('touchend', handleWindowTouchEnd);
    };
  }, []);

  const toggleSelection = (index: number) => {
    setSelectedIndices(prevIndices =>
      prevIndices.includes(index) ? [] : [index],
    );
  };

  const toggleSelectionInGroup = (index: number) => {
    setSelectedIndices(prevIndices => {
      const indexPosition = prevIndices.indexOf(index);

      if (indexPosition === -1) {
        return [...prevIndices, index];
      }

      const nextIndices = [...prevIndices];
      nextIndices.splice(indexPosition, 1);

      return nextIndices;
    });
  };

  const performAction = (
    event:
      | React.MouseEvent<HTMLDivElement>
      | React.KeyboardEvent<HTMLDivElement>,
    index: number,
  ) => {
    if (isGroupKeyUsed(event)) {
      toggleSelectionInGroup(index);
      return;
    }

    toggleSelection(index);
  };

  const handleKeyDown = (
    event: React.KeyboardEvent<HTMLDivElement>,
    snapshot: DraggableStateSnapshot,
    index: number,
  ) => {
    if (
      event.defaultPrevented ||
      snapshot.isDragging ||
      event.key !== 'Enter'
    ) {
      return;
    }

    event.preventDefault();
    performAction(event, index);
  };

  const handleClick = (
    event: React.MouseEvent<HTMLDivElement>,
    index: number,
  ) => {
    if (event.defaultPrevented || event.button !== primaryButton) {
      return;
    }

    event.preventDefault();
    performAction(event, index);
  };

  const handleTouchEnd = (
    event: React.TouchEvent<HTMLDivElement>,
    index: number,
  ) => {
    if (event.defaultPrevented) {
      return;
    }

    event.preventDefault();
    toggleSelectionInGroup(index);
  };

  const list: Filter[] = getValues(name);

  const handleMoveItem = ({
    direction,
    index,
  }: {
    direction: 'up' | 'down';
    index: number;
  }) => {
    const newIndex = direction === 'up' ? index - 1 : index + 1;
    const reordered = reorder(list, index, newIndex);
    setValue(name, reordered);
  };

  return (
    <FormFieldset legend={legend} legendSize="s" id={id}>
      <div className={styles.focusContainer} ref={listRef} tabIndex={-1}>
        <DragDropContext
          onDragStart={start => {
            if (!selectedIndices.includes(start.source.index)) {
              setSelectedIndices([]);
            }

            setDraggingIndex(start.source.index);
          }}
          onDragEnd={result => {
            const destinationIndex = result.destination?.index;

            if (destinationIndex === null || destinationIndex === undefined) {
              return;
            }

            const selected = selectedIndices.length
              ? selectedIndices
              : [result.source.index];

            const nextValue = reorderMultiple({
              list,
              destinationIndex,
              selectedIndices: selected,
            });

            setDraggingIndex(undefined);

            const oldOptions = selected.map(index => list[index]);

            setSelectedIndices(
              nextValue.reduce<number[]>((acc, option, index) => {
                if (oldOptions.includes(option)) {
                  acc.push(index);
                }

                return acc;
              }, []),
            );

            setValue(name, nextValue);
          }}
        >
          <Droppable droppableId={id}>
            {(droppableProvided, droppableSnapshot) => (
              <div
                // eslint-disable-next-line react/jsx-props-no-spreading
                {...droppableProvided.droppableProps}
                className={classNames(styles.list, {
                  [styles.isDraggingOver]: droppableSnapshot.isDraggingOver,
                })}
                ref={droppableProvided.innerRef}
              >
                {list.map((option, index) => {
                  const isInActive = !!(activeItem && activeItem !== option.id);

                  return (
                    <Draggable
                      draggableId={option.value}
                      key={option.value}
                      index={index}
                      isDragDisabled={isInActive}
                    >
                      {(draggableProvided, draggableSnapshot) => (
                        <TableHeadersReorderableItem
                          activeItem={activeItem}
                          draggableProvided={draggableProvided}
                          draggableSnapshot={draggableSnapshot}
                          key={option.value}
                          index={index}
                          isGhosted={
                            !!(
                              selectedIndices.includes(index) &&
                              draggingIndex &&
                              draggingIndex !== index
                            )
                          }
                          isLastItem={index === list.length - 1}
                          isSelected={selectedIndices.includes(index)}
                          option={option}
                          selectedIndicesLength={selectedIndices.length}
                          onClick={handleClick}
                          onClickMoveDown={() => {
                            handleMoveItem({ index, direction: 'down' });
                          }}
                          onClickMoveUp={() => {
                            handleMoveItem({ index, direction: 'up' });
                          }}
                          onKeyDown={handleKeyDown}
                          onSetActive={setActiveItem}
                          onTouchEnd={handleTouchEnd}
                        />
                      )}
                    </Draggable>
                  );
                })}
                {droppableProvided.placeholder}
              </div>
            )}
          </Droppable>
        </DragDropContext>
      </div>
    </FormFieldset>
  );
};

export default TableHeadersReorderableList;

/**
 * Determines if the platform-specific grouping key was used
 * e.g. Ctrl for Linux/Windows and the Meta key for Mac.
 */
function isGroupKeyUsed(
  event: React.MouseEvent | React.KeyboardEvent,
): boolean {
  return Boolean(
    navigator.platform.includes('Mac') ? event.metaKey : event.ctrlKey,
  );
}
