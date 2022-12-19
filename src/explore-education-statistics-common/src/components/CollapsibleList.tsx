import ButtonText from '@common/components/ButtonText';
import useToggle from '@common/hooks/useToggle';
import classNames from 'classnames';
import React, { Children, createElement, ReactNode } from 'react';

interface BaseProps {
  children: ReactNode;
  collapseAfter?: number;
  id: string;
  isCollapsed?: boolean;
  listStyle?: 'none' | 'number' | 'bullet';
  testId?: string;
}

type Props = BaseProps &
  (
    | {
        itemName?: never;
        itemNamePlural?: never;
      }
    | {
        itemName: string;
        itemNamePlural: string;
      }
  );

const CollapsibleList = ({
  children,
  collapseAfter = 5,
  id,
  itemName = 'item',
  itemNamePlural = 'items',
  isCollapsed = true,
  listStyle = 'none',
  testId,
}: Props) => {
  const [collapsed, toggleCollapsed] = useToggle(isCollapsed);

  const listItems = Children.toArray(children);

  const renderedListItems = collapsed
    ? listItems.slice(0, collapseAfter > 0 ? collapseAfter : 0)
    : listItems;

  const listTag = listStyle === 'number' ? 'ol' : 'ul';

  const listClasses = classNames('govuk-list', {
    'govuk-list--number': listStyle === 'number',
    'govuk-list--bullet': listStyle === 'bullet',
  });

  const list = createElement(
    listTag,
    { id, className: listClasses, 'data-testid': testId },
    renderedListItems,
  );

  const collapsedCount =
    Children.count(children) - (collapseAfter > -1 ? collapseAfter : 0);

  return (
    <>
      {list}

      {collapsedCount > 0 && (
        <ButtonText
          ariaControls={id}
          ariaExpanded={!collapsed}
          className="dfe-hide-print govuk-!-margin-bottom-4"
          onClick={toggleCollapsed}
        >
          {collapsed
            ? `Show ${collapsedCount} ${collapseAfter > 0 ? 'more' : ''} ${
                collapsedCount > 1 ? itemNamePlural : itemName
              }`
            : `Hide ${collapsedCount} ${
                collapsedCount > 1 ? itemNamePlural : itemName
              }`}
        </ButtonText>
      )}
    </>
  );
};

export default CollapsibleList;
