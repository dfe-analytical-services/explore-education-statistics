import ButtonText from '@common/components/ButtonText';
import styles from '@common/components/CollapsibleList.module.scss';
import VisuallyHidden from '@common/components/VisuallyHidden';
import useToggle from '@common/hooks/useToggle';
import classNames from 'classnames';
import React, {
  Children,
  cloneElement,
  createElement,
  HTMLAttributes,
  ReactElement,
  ReactNode,
  useRef,
} from 'react';

interface BaseProps {
  buttonClassName?: string;
  buttonHiddenText?: string;
  children: ReactNode;
  collapseAfter?: number;
  id: string;
  isCollapsed?: boolean;
  listClassName?: string;
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
  buttonClassName,
  buttonHiddenText,
  children,
  collapseAfter = 5,
  id,
  itemName = 'item',
  itemNamePlural = 'items',
  isCollapsed = true,
  listClassName,
  listStyle = 'none',
  testId,
}: Props) => {
  const [collapsed, toggleCollapsed] = useToggle(isCollapsed);

  const firstItemRef = useRef<HTMLLIElement>(null);
  const firstHiddenItemRef = useRef<HTMLLIElement>(null);

  const getItemProperties = (index: number) => {
    if (index === 0) {
      return {
        className: styles.focusableItem,
        ref: firstItemRef,
        tabIndex: -1,
      };
    }
    if (collapseAfter === index) {
      return {
        className: styles.focusableItem,
        ref: firstHiddenItemRef,
        tabIndex: -1,
      };
    }
    return undefined;
  };

  const listItems = Children.toArray(children).map((item, index) => {
    if (!React.isValidElement(item)) {
      return item;
    }

    return cloneElement(
      item as ReactElement<HTMLAttributes<HTMLLIElement>>,
      getItemProperties(index),
    );
  });

  const renderedListItems = collapsed
    ? listItems.slice(0, collapseAfter > 0 ? collapseAfter : 0)
    : listItems;

  const listTag = listStyle === 'number' ? 'ol' : 'ul';

  const listClasses = classNames('govuk-list', listClassName, {
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
          className={classNames('govuk-!-display-none-print', buttonClassName, {
            'govuk-!-margin-bottom-4': !buttonClassName,
          })}
          onClick={() => {
            if (collapsed) {
              toggleCollapsed.off();
              // Timeout to make sure the list item is shown before focusing it.
              setTimeout(() => {
                firstHiddenItemRef.current?.focus();
              }, 0);
            } else {
              toggleCollapsed.on();
              firstItemRef.current?.focus();
            }
          }}
        >
          {collapsed
            ? `Show ${collapsedCount} ${collapseAfter > 0 ? 'more' : ''} ${
                collapsedCount > 1 ? itemNamePlural : itemName
              }`
            : `Hide ${collapsedCount} ${
                collapsedCount > 1 ? itemNamePlural : itemName
              }`}
          {buttonHiddenText && (
            <VisuallyHidden> {buttonHiddenText}</VisuallyHidden>
          )}
        </ButtonText>
      )}
    </>
  );
};

export default CollapsibleList;
