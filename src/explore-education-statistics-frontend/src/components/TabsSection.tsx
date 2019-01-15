import React, { FunctionComponent } from 'react';

export interface TabsSectionProps {
  id: string;
  title: string;
}

const TabsSection: FunctionComponent<TabsSectionProps> = ({
  children,
  title,
  id,
}) => {
  return (
    <div key={id} className="govuk-tabs__panel" id={id}>
      {children}
    </div>
  );
};

export default TabsSection;
