import DragIcon from '@common/components/DragIcon';
import styles from '@common/modules/table-tool/components/TableHeadersReorderableItem.module.scss';
import { Filter } from '@common/modules/table-tool/types/filters';
import VisuallyHidden from '@common/components/VisuallyHidden';
import { ArrowLeft, ArrowRight } from '@common/components/ArrowIcons';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import classNames from 'classnames';
import React, { MouseEvent, KeyboardEvent, TouchEvent } from 'react';
import { DraggableProvided, DraggableStateSnapshot } from '@hello-pangea/dnd';

interface Props {
  activeItem?: string;
  draggableProvided: DraggableProvided;
  draggableSnapshot: DraggableStateSnapshot;
  index: number;
  isGhosted: boolean;
  isLastItem: boolean;
  isSelected: boolean;
  selectedIndicesLength: number;
  option: Filter;
  onClick: (event: MouseEvent<HTMLDivElement>, index: number) => void;
  onClickMoveDown: () => void;
  onClickMoveUp: () => void;
  onKeyDown: (
    event: KeyboardEvent<HTMLDivElement>,
    draggableSnapshot: DraggableStateSnapshot,
    index: number,
  ) => void;
  onSetActive: (id: string) => void;
  onTouchEnd: (event: TouchEvent<HTMLDivElement>, index: number) => void;
}

export default function TableHeadersReorderableItem({
  activeItem,
  draggableProvided,
  draggableSnapshot,
  index,
  isGhosted,
  isLastItem,
  isSelected,
  option,
  selectedIndicesLength,
  onClick,
  onClickMoveDown,
  onClickMoveUp,
  onKeyDown,
  onSetActive,
  onTouchEnd,
}: Props) {
  const isActive = activeItem === option.id;
  const isInActive = !!(activeItem && !isActive);
  return (
    <div className={styles.container} data-testid="reorderable-item">
      <div
        // eslint-disable-next-line react/jsx-props-no-spreading
        {...draggableProvided.draggableProps}
        // eslint-disable-next-line react/jsx-props-no-spreading
        {...draggableProvided.dragHandleProps}
        className={classNames(styles.option, {
          [styles.isDragging]: draggableSnapshot.isDragging,
          [styles.isSelected]: isSelected,
          [styles.isGhosted]: isGhosted || isInActive,
          [styles.isDraggedOutside]:
            draggableSnapshot.isDragging && !draggableSnapshot.draggingOver,
          [styles.isActive]: isActive,
        })}
        ref={draggableProvided.innerRef}
        role="button"
        style={draggableProvided.draggableProps.style}
        tabIndex={0}
        onClick={event => {
          if (!isActive) {
            onClick(event, index);
          }
        }}
        onKeyDown={event => onKeyDown(event, draggableSnapshot, index)}
        onTouchEnd={event => onTouchEnd(event, index)}
      >
        <div className={styles.optionLabel}>
          <DragIcon className={styles.dragIcon} />
          <span>{option.label}</span>
        </div>

        {selectedIndicesLength > 1 && draggableSnapshot.isDragging && (
          <div className={styles.selectedCount}>
            {selectedIndicesLength}{' '}
            <span className="govuk-visually-hidden">
              {`${selectedIndicesLength} ${
                selectedIndicesLength === 1 ? 'item' : 'items'
              } selected`}
            </span>
          </div>
        )}
      </div>
      {!draggableSnapshot.isDragging && (
        <div
          className={classNames({
            [styles.controls]: !isActive,
            [styles.controlsActive]: isActive,
          })}
        >
          {!isActive && !activeItem && (
            <Button variant="secondary" onClick={() => onSetActive(option.id)}>
              Move<VisuallyHidden> {option.label}</VisuallyHidden>
            </Button>
          )}
          {isActive && (
            <>
              <ButtonGroup className={styles.buttonGroup}>
                {index !== 0 && (
                  <Button
                    className="govuk-!-margin-bottom-0"
                    variant="secondary"
                    onClick={() => {
                      onClickMoveUp();
                    }}
                  >
                    <ArrowLeft className={styles.arrow} />
                    <VisuallyHidden>Move {option.label} up</VisuallyHidden>
                  </Button>
                )}
                {!isLastItem && (
                  <Button
                    className="govuk-!-margin-bottom-0"
                    variant="secondary"
                    onClick={() => {
                      onClickMoveDown();
                    }}
                  >
                    <ArrowRight className={styles.arrow} />
                    <VisuallyHidden>Move {option.label} down</VisuallyHidden>
                  </Button>
                )}
              </ButtonGroup>
              <Button
                className={`govuk-!-margin-bottom-0 ${styles.doneButton}`}
                onClick={() => onSetActive('')}
              >
                Done
              </Button>
            </>
          )}
        </div>
      )}
    </div>
  );
}
