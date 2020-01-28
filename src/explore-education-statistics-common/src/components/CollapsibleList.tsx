import React, { Children, ReactNode, useState } from 'react';
import ButtonText from './ButtonText';
import styles from './CollapsibleList.module.scss';

interface Props {
  children: ReactNode;
  collapseAfter?: number;
  isCollapsed?: boolean;
  cssDriven?: boolean;
}

const CollapsibleList = ({
  children,
  collapseAfter = 5,
  isCollapsed = true,
  cssDriven = false,
}: Props) => {
  const [collapsed, setCollapsed] = useState<boolean>(isCollapsed);

  if (Children.count(children) > collapseAfter) {
    if (cssDriven) {
      return (
        <div className={styles.footnotesContainer}>
          {React.Children.map(children, (child, i) => {
            if (i >= collapseAfter - 2) {
              if (React.isValidElement(child)) {
                return React.cloneElement(child, {
                  ...child.props,
                  'aria-hidden': false,
                  className: `${child.props.className} ${
                    collapsed ? 'govuk-visually-hidden' : ''
                  }`,
                });
              }
            }
            return child;
          })}
          <div className={styles.hidePrint} aria-hidden="true">
            {collapsed && Children.count(children) - (collapseAfter - 2) && (
              <strong>
                {`And ${React.Children.count(children) -
                  (collapseAfter - 2)} more...`}
                <br />
              </strong>
            )}
            <ButtonText onClick={() => setCollapsed(!collapsed)}>
              {collapsed ? 'Show All' : 'Collapse List'}
            </ButtonText>
          </div>
        </div>
      );
    }
    if (collapsed) {
      return (
        <>
          {React.Children.map(children, (child, i) => {
            if (i >= collapseAfter - 2) {
              return null;
            }
            return child;
          })}
          <div className={styles.hidePrint}>
            {Children.count(children) - (collapseAfter - 2) && (
              <strong>
                {`And ${React.Children.count(children) -
                  (collapseAfter - 2)} more...`}
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
    return (
      <>
        {children}
        <ButtonText onClick={() => setCollapsed(!collapsed)}>
          {collapsed ? 'Show All' : 'Collapse List'}
        </ButtonText>
      </>
    );
  }
  return <>{children}</>;
};

export default CollapsibleList;
