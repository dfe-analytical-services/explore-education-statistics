import styles from '@admin/pages/release/data/components/ReorderList.module.scss';
import ButtonText from '@common/components/ButtonText';
import reorder from '@common/utils/reorder';
import classNames from 'classnames';
import React, { useState } from 'react';
import { DragDropContext, Draggable, Droppable } from 'react-beautiful-dnd';

export interface FormattedOption {
  id: string;
  label: string;
}
export interface FormattedGroup {
  id: string;
  label: string;
  items: FormattedOption[];
}
export interface FormattedFilters {
  id: string;
  label: string;
  groups: FormattedGroup[];
}
export interface FormattedIndicators {
  id: string;
  label: string;
  items: FormattedOption[];
}
export interface ReorderProps {
  reordered: (
    | FormattedIndicators
    | FormattedFilters
    | FormattedOption
    | FormattedGroup
  )[];
  parentCategoryId?: string;
  parentGroupId?: string;
}

const getChildItems = (
  option:
    | FormattedFilters
    | FormattedGroup
    | FormattedOption
    | FormattedIndicators,
): {
  childOptions: (FormattedOption | FormattedGroup)[] | undefined;
  parentGroupId?: string | undefined;
} => {
  if ('groups' in option && option.groups.length > 1) {
    return { childOptions: option.groups };
  }
  if ('groups' in option && option.groups.length === 1) {
    return {
      childOptions: option.groups[0].items,
      parentGroupId: option.groups[0].id,
    };
  }
  if ('items' in option && option.items.length > 1) {
    return { childOptions: option.items, parentGroupId: option.id };
  }
  return { childOptions: undefined };
};

interface ReorderListProps {
  listItems: (
    | FormattedIndicators
    | FormattedFilters
    | FormattedOption
    | FormattedGroup
  )[];
  categoryId?: string;
  groupId?: string;
  onReorder: ({
    reordered,
    parentCategoryId,
    parentGroupId,
  }: ReorderProps) => void;
}
const ReorderList = ({
  listItems,
  categoryId,
  groupId,
  onReorder,
}: ReorderListProps) => {
  const [reorderingGroups, setReorderingGroups] = useState<string[]>([]);

  // If category has only one group, just show its options not the group.
  const options =
    listItems.length === 1 && 'items' in listItems[0]
      ? listItems[0].items
      : listItems;

  const parentCategoryId =
    listItems.length === 1 && 'items' in listItems[0]
      ? listItems[0].id
      : categoryId;

  return (
    <DragDropContext
      onDragEnd={result => {
        if (!result.destination) {
          return;
        }

        const reordered = reorder(
          options,
          result.source.index,
          result.destination.index,
        );

        // Update the order property if it's a category or group, don't if it's items
        onReorder({
          reordered: reordered.map((option, index) => {
            if ('order' in reordered[0]) {
              return {
                ...option,
                order: index,
              };
            }
            return option;
          }),
          parentCategoryId,
          parentGroupId: groupId,
        });
      }}
    >
      <Droppable droppableId="droppablegroups">
        {(droppableProvided, droppableSnapshot) => (
          <ol
            className={classNames(styles.dropArea, {
              [styles.dropAreaActive]: droppableSnapshot.isDraggingOver,
            })}
            data-testid="reorder-list"
            ref={droppableProvided.innerRef}
          >
            {options.map((option, index) => {
              const key = option.id || `key-${index}`;
              const isExpanded = reorderingGroups.includes(key);
              const { childOptions, parentGroupId } = getChildItems(option);
              return (
                <Draggable
                  draggableId={key}
                  isDragDisabled={reorderingGroups.length !== 0}
                  key={key}
                  index={index}
                >
                  {(draggableProvided, draggableSnapshot) => {
                    return (
                      <li
                        // eslint-disable-next-line react/jsx-props-no-spreading
                        {...draggableProvided.draggableProps}
                        // eslint-disable-next-line react/jsx-props-no-spreading
                        {...draggableProvided.dragHandleProps}
                        className={classNames(styles.draggable, {
                          [styles.isDisabled]:
                            reorderingGroups.length && !isExpanded,
                          [styles.isExpanded]: isExpanded,
                          [styles.isDragging]: draggableSnapshot.isDragging,
                          [styles.isDraggedOutside]:
                            draggableSnapshot.isDragging &&
                            !draggableSnapshot.draggingOver,
                        })}
                        ref={draggableProvided.innerRef}
                      >
                        <span className={styles.draggableInner}>
                          <span
                            className={classNames(styles.optionLabel, {
                              [styles.isExpanded]: isExpanded,
                            })}
                          >
                            {option.label}
                          </span>
                          {childOptions && (
                            <ButtonText
                              className="govuk-!-margin-bottom-0"
                              onClick={() => {
                                setReorderingGroups(
                                  isExpanded
                                    ? reorderingGroups.filter(
                                        item => item !== key,
                                      )
                                    : [...reorderingGroups, key],
                                );
                              }}
                            >
                              {isExpanded
                                ? 'Done'
                                : 'Reorder options within this group'}
                            </ButtonText>
                          )}
                        </span>
                        {childOptions && isExpanded && (
                          <ReorderList
                            listItems={childOptions}
                            categoryId={categoryId || option.id}
                            groupId={parentGroupId}
                            onReorder={onReorder}
                          />
                        )}
                      </li>
                    );
                  }}
                </Draggable>
              );
            })}
            {droppableProvided.placeholder}
          </ol>
        )}
      </Droppable>
    </DragDropContext>
  );
};
export default ReorderList;
