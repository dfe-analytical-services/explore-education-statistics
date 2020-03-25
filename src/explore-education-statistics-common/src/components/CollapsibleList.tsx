import ButtonText from '@common/components/ButtonText';
import useToggle from '@common/hooks/useToggle';
import classNames from 'classnames';
import React, {
  Children,
  cloneElement,
  isValidElement,
  ReactNode,
} from 'react';
import styles from './CollapsibleList.module.scss';

interface Props {
  children: ReactNode;
  collapseAfter?: number;
  isCollapsed?: boolean;
  listStyle?: 'none' | 'number' | 'bullet';
}

const CollapsibleList = ({
  children,
  collapseAfter = 5,
  isCollapsed = true,
  listStyle = 'none',
}: Props) => {
  const [collapsed, toggleCollapsed] = useToggle(isCollapsed);

  const listClasses = classNames('govuk-list', styles.listContainer, {
    'govuk-list--number': listStyle === 'number',
    'govuk-list--bullet': listStyle === 'bullet',
  });

  const listItems = Children.map(children, (child, i) => {
    if (i > collapseAfter - 1) {
      if (isValidElement(child)) {
        return cloneElement(child, {
          ...child.props,
          className: classNames(child.props.className, {
            'govuk-visually-hidden': collapsed,
          }),
        });
      }
    }
    return child;
  });

  const collapsedCount =
    Children.count(children) - (collapseAfter > -1 ? collapseAfter : 0);

  return (
    <>
      {listStyle === 'number' ? (
        <ol className={listClasses}>{listItems}</ol>
      ) : (
        <ul className={listClasses}>{listItems}</ul>
      )}

      {collapsedCount > 0 && (
        <div
          className={classNames(styles.hidePrint, 'govuk-!-margin-bottom-4')}
          aria-hidden="true"
        >
          <ButtonText onClick={toggleCollapsed}>
            {collapsed
              ? `Show ${collapsedCount} ${
                  listItems && listItems.length > collapsedCount ? 'more' : ''
                } items`
              : 'Collapse list'}
          </ButtonText>
        </div>
      )}
    </>
  );
};

export default CollapsibleList;
