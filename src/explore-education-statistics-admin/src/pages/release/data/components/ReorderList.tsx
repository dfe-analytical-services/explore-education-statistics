import DroppableArea from '@admin/components/DroppableArea';
import DraggableItem from '@admin/components/DraggableItem';
import styles from '@admin/pages/release/data/components/ReorderList.module.scss';
import ButtonText from '@common/components/ButtonText';
import reorder from '@common/utils/reorder';
import classNames from 'classnames';
import React, { useState } from 'react';
import { DragDropContext, Droppable } from 'react-beautiful-dnd';

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
  testId?: string;
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
  testId,
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
          <DroppableArea
            className={styles.dropArea}
            droppableProvided={droppableProvided}
            droppableSnapshot={droppableSnapshot}
            tag="ol"
            testId={testId ? `${testId}-reorder-list` : 'reorder-list'}
          >
            {options.map((option, index) => {
              const key = option.id || `key-${index}`;
              const isExpanded = reorderingGroups.includes(key);
              const { childOptions, parentGroupId } = getChildItems(option);
              return (
                <DraggableItem
                  className={classNames({
                    [styles.isExpandedItem]: isExpanded,
                  })}
                  hideDragHandle={reorderingGroups.length > 0 && !isExpanded}
                  dragHandle={
                    isExpanded ? (
                      <span aria-hidden className={styles.dragHandle}>
                        ▼
                      </span>
                    ) : undefined
                  }
                  id={key}
                  index={index}
                  isDisabled={reorderingGroups.length !== 0}
                  isReordering
                  key={key}
                  tag="li"
                >
                  <div className={styles.inner}>
                    <span className={styles.draggableInner}>
                      <span
                        className={classNames(styles.optionLabel, {
                          [styles.isExpanded]: isExpanded,
                          [styles.hideDragHandle]:
                            reorderingGroups.length > 0 && !isExpanded,
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
                                ? reorderingGroups.filter(item => item !== key)
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
                        testId={option.label}
                        onReorder={onReorder}
                      />
                    )}
                  </div>
                </DraggableItem>
              );
            })}
          </DroppableArea>
        )}
      </Droppable>
    </DragDropContext>
  );
};
export default ReorderList;
