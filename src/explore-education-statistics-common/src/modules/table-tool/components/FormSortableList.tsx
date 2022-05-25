import { FormFieldset } from '@common/components/form';
import { FormFieldsetProps } from '@common/components/form/FormFieldset';
import DragIcon from '@common/modules/table-tool/components/DragIcon';
import { Filter } from '@common/modules/table-tool/types/filters';
import reorderMultiple from '@common/utils/reorderMultiple';
import classNames from 'classnames';
import React, {
  FocusEventHandler,
  MouseEventHandler,
  useEffect,
  useRef,
  useState,
} from 'react';
import {
  DragDropContext,
  Draggable,
  DraggableStateSnapshot,
  Droppable,
} from 'react-beautiful-dnd';
import styles from './FormSortableList.module.scss';

const primaryButton = 0; // https://developer.mozilla.org/en-US/docs/Web/API/MouseEvent/button

type SortableOptionChangeEventHandler = (value: Filter[]) => void;

export type FormSortableListProps = {
  isGroupDragDisabled: boolean;
  onBlur?: FocusEventHandler<HTMLDivElement>;
  onChange?: SortableOptionChangeEventHandler;
  onFocus?: FocusEventHandler<HTMLDivElement>;
  onMouseEnter?: MouseEventHandler<HTMLDivElement>;
  onMouseLeave?: MouseEventHandler<HTMLDivElement>;
  value: Filter[];
} & FormFieldsetProps;

const FormSortableList = ({
  id,
  isGroupDragDisabled,
  onBlur,
  onChange,
  onFocus,
  onMouseEnter,
  onMouseLeave,
  value,
  ...props
}: FormSortableListProps) => {
  const isFocusWithinRef = useRef(false);

  const [selectedIndices, setSelectedIndices] = useState<number[]>([]);
  const [draggingIndex, setDraggingIndex] = useState<number>();

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
    if (isGroupDragDisabled) {
      setSelectedIndices(prevIndices =>
        prevIndices.includes(index) ? [] : [index],
      );
    }
  };

  const toggleSelectionInGroup = (index: number) => {
    if (isGroupDragDisabled) {
      setSelectedIndices(prevIndices => {
        const indexPosition = prevIndices.indexOf(index);

        if (indexPosition === -1) {
          return [...prevIndices, index];
        }

        const nextIndices = [...prevIndices];
        nextIndices.splice(indexPosition, 1);

        return nextIndices;
      });
    }
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

  const handleContainerBlur: FocusEventHandler<HTMLDivElement> = event => {
    event.persist();

    // We need to use a timeout so that `document.activeElement`
    // is set if the focus is within the container (without the
    // timeout, it gets set to the document `body` for some reason).
    //
    // Unfortunately, this also means that the `currentTarget`
    // gets set to null (not sure why), so to workaround this
    // we set the element variable here to retain a reference
    // to use later.
    const element = event.currentTarget;

    setTimeout(() => {
      if (
        isFocusWithinRef.current &&
        !element.contains(document.activeElement)
      ) {
        isFocusWithinRef.current = false;

        // As `currentTarget` will be null at this point due
        // to being wrapped in a timeout, we need to assign it
        // back to the event so that consumers are not affected.
        // eslint-disable-next-line no-param-reassign
        event.currentTarget = element;

        onBlur?.(event);
      }
    });
  };

  const handleContainerFocus: FocusEventHandler<HTMLDivElement> = event => {
    if (
      !isFocusWithinRef.current &&
      event.currentTarget.contains(document.activeElement)
    ) {
      isFocusWithinRef.current = true;
      onFocus?.(event);
    }
  };

  return (
    <FormFieldset {...props} id={id}>
      <DragDropContext
        onDragStart={start => {
          if (!selectedIndices.includes(start.source.index)) {
            setSelectedIndices([]);
          }

          setDraggingIndex(start.source.index);
        }}
        onDragEnd={result => {
          if (result.destination?.index == null) {
            return;
          }

          const destinationIndex = result.destination.index;

          const selected = selectedIndices.length
            ? selectedIndices
            : [result.source.index];

          const nextValue = reorderMultiple({
            list: value,
            destinationIndex,
            selectedIndices: selected,
          });

          setDraggingIndex(undefined);

          const oldOptions = selected.map(index => value[index]);

          setSelectedIndices(
            nextValue.reduce<number[]>((acc, option, index) => {
              if (oldOptions.includes(option)) {
                acc.push(index);
              }

              return acc;
            }, []),
          );

          onChange?.(nextValue);
        }}
      >
        <Droppable droppableId={id}>
          {(droppableProvided, droppableSnapshot) => (
            <div
              // eslint-disable-next-line react/jsx-props-no-spreading
              {...droppableProvided.droppableProps}
              className={classNames(styles.list, {
                [styles.isDraggingOver]: droppableSnapshot.isDraggingOver,
                [styles.listIsDisabled]: !isGroupDragDisabled,
              })}
              ref={droppableProvided.innerRef}
              onBlur={handleContainerBlur}
              onFocus={handleContainerFocus}
              onMouseEnter={onMouseEnter}
              onMouseLeave={onMouseLeave}
              aria-hidden={!isGroupDragDisabled}
            >
              {value.map((option, index) => (
                <Draggable
                  isDragDisabled={!isGroupDragDisabled}
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
                      className={classNames(styles.option, {
                        [styles.isDragging]: draggableSnapshot.isDragging,
                        [styles.isSelected]: selectedIndices.includes(index),
                        [styles.isGhosted]:
                          selectedIndices.includes(index) &&
                          typeof draggingIndex === 'number' &&
                          draggingIndex !== index,
                        [styles.isDraggedOutside]:
                          draggableSnapshot.isDragging &&
                          !draggableSnapshot.draggingOver,
                      })}
                      ref={draggableProvided.innerRef}
                      role={isGroupDragDisabled ? 'button' : ''}
                      style={draggableProvided.draggableProps.style}
                      tabIndex={isGroupDragDisabled ? 0 : -1}
                      onClick={event => {
                        handleClick(event, index);
                      }}
                      onKeyDown={event =>
                        handleKeyDown(event, draggableSnapshot, index)
                      }
                      onTouchEnd={event => {
                        handleTouchEnd(event, index);
                      }}
                    >
                      <div className={styles.optionLabel}>
                        <span>{option.label}</span>
                        <DragIcon className={styles.dragIcon} />
                      </div>

                      {selectedIndices.length > 1 &&
                        draggingIndex === index &&
                        draggableSnapshot.isDragging && (
                          <div className={styles.selectedCount}>
                            {selectedIndices.length}{' '}
                            <span className="govuk-visually-hidden">
                              {`${selectedIndices.length} ${
                                selectedIndices.length === 1 ? 'item' : 'items'
                              } selected`}
                            </span>
                          </div>
                        )}
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
