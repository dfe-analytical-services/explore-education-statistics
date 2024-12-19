import styles from '@common/components/ReorderableNestedList.module.scss';
import ReorderableList, {
  ReorderableListProps,
} from '@common/components/ReorderableList';
import VisuallyHidden from '@common/components/VisuallyHidden';
import ButtonText from '@common/components/ButtonText';
import { ReorderResult } from '@common/components/ReorderableItem';
import classNames from 'classnames';
import React, { useState } from 'react';
import { OmitStrict } from '@common/types';

interface NestedReorderResult extends ReorderResult {
  expandedItemId?: string;
  expandedItemParentId?: string;
}

interface Props extends OmitStrict<ReorderableListProps, 'onMoveItem'> {
  expandedItemId?: string;
  expandedItemParentId?: string;
  onMoveItem: (opts: NestedReorderResult) => void;
}

export default function ReorderableNestedList({
  expandedItemId,
  expandedItemParentId,
  heading,
  id,
  list,
  onMoveItem,
  ...props
}: Props) {
  const [expandedItem, setExpandedItem] = useState<{
    id: string;
    parentId?: string;
  }>();

  if (expandedItem?.id) {
    return (
      <>
        {heading && <h3>{heading}</h3>}
        <ol className={`govuk-list ${styles.list}`} data-testid={id}>
          {list.map(item => (
            <li
              key={item.id}
              className={classNames(styles.item, 'govuk-!-margin-bottom-0', {
                [styles.itemExpanded]:
                  item.id === expandedItem.id && item.childOptions,
                [styles.itemNotExpanded]: item.id !== expandedItem.id,
              })}
            >
              {item.id === expandedItem.id && item.childOptions ? (
                <>
                  <span className="dfe-flex dfe-justify-content--space-between dfe-align-items--center">
                    {item.label}
                    <ButtonText
                      className="govuk-!-margin-bottom-0"
                      onClick={() => setExpandedItem(undefined)}
                    >
                      Close<VisuallyHidden> {item.label}</VisuallyHidden>
                    </ButtonText>
                  </span>
                  <ReorderableNestedList
                    expandedItemId={expandedItem.id}
                    expandedItemParentId={expandedItem.parentId}
                    id={`${id}-children`}
                    testId={`${id}-children`}
                    list={
                      item.childOptions.length === 1 &&
                      item.childOptions[0].childOptions
                        ? item.childOptions[0].childOptions
                        : item.childOptions
                    }
                    onMoveItem={onMoveItem}
                  />
                </>
              ) : (
                item.label
              )}
            </li>
          ))}
        </ol>
      </>
    );
  }

  return (
    <ReorderableList
      heading={heading}
      id={id}
      list={list}
      onExpandOptions={(itemId, parentId) => {
        setExpandedItem({ id: itemId, parentId });
      }}
      onMoveItem={({ prevIndex, nextIndex }) => {
        onMoveItem({
          prevIndex,
          nextIndex,
          expandedItemId,
          expandedItemParentId,
        });
      }}
      {...props}
    />
  );
}
