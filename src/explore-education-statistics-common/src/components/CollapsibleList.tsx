import React, { useState, Children, ReactNode } from 'react';
import ButtonText from './ButtonText';

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
    if (collapsed) {
      return (
        <>
          {React.Children.map(children, (child, i) => {
            if (i >= collapseAfter - 2) {
              return null;
            }
            return child;
          })}
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
