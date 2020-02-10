import React, {
  Children,
  cloneElement,
  isValidElement,
  ReactNode,
  useState,
} from 'react';
import classNames from 'classnames';
import ButtonText from '@common/components/ButtonText';
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
  const [collapsed, setCollapsed] = useState<boolean>(isCollapsed);

  if (Children.count(children) > collapseAfter) {
    const listItems = Children.map(children, (child, i) => {
      if (i >= collapseAfter - 2) {
        if (isValidElement(child)) {
          return cloneElement(child, {
            ...child.props,
            'aria-hidden': false,
            className: classNames(
              child.props.className,
              collapsed && 'govuk-visually-hidden',
            ),
          });
        }
      }
      return child;
    });

    const listClasses = classNames('govuk-list', styles.listContainer, {
      'govuk-list--number': listStyle === 'number',
      'govuk-list--bullet': listStyle === 'bullet',
    });
    return (
      <>
        {listStyle === 'number' ? (
          <ol className={listClasses}>{listItems}</ol>
        ) : (
          <ul className={listClasses}>{listItems}</ul>
        )}
        <div
          className={classNames(styles.hidePrint, 'govuk-!-margin-bottom-4')}
          aria-hidden="true"
        >
          {collapsed && Children.count(children) - (collapseAfter - 2) && (
            <strong>
              {`And ${Children.count(children) - (collapseAfter - 2)} more...`}
              <br />
            </strong>
          )}
          <ButtonText onClick={() => setCollapsed(!collapsed)}>
            {collapsed ? 'Show All' : 'Collapse List'}
          </ButtonText>
        </div>
      </>
    );
  }
  return <>{children}</>;
};

export default CollapsibleList;
