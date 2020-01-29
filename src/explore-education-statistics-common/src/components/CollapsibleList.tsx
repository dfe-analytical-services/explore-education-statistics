import React, {
  Children,
  cloneElement,
  isValidElement,
  ReactNode,
  useState,
} from 'react';
import classNames from 'classnames';
import ButtonText from './ButtonText';
import styles from './CollapsibleList.module.scss';

interface Props {
  children: ReactNode;
  collapseAfter?: number;
  isCollapsed?: boolean;
}

const CollapsibleList = ({
  children,
  collapseAfter = 5,
  isCollapsed = true,
}: Props) => {
  const [collapsed, setCollapsed] = useState<boolean>(isCollapsed);

  if (Children.count(children) > collapseAfter) {
    return (
      <>
        {Children.map(children, (child, i) => {
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
        })}
        <div className={styles.hidePrint} aria-hidden="true">
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
