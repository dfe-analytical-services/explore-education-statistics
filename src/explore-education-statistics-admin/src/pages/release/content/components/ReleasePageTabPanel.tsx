import { ReleasePageTabSectionKey } from '@admin/pages/release/content/components/ReleaseContentRedesign';
import React, { ReactNode } from 'react';

interface Props {
  children: ReactNode;
  hidden: boolean;
  tabKey: ReleasePageTabSectionKey;
}

const ReleasePageTabPanel = ({ children, hidden, tabKey }: Props) => {
  return (
    <div
      aria-labelledby={`tab-${tabKey}-tab`}
      hidden={hidden}
      id={`tab-${tabKey}`}
      role="tabpanel"
    >
      {children}
    </div>
  );
};
export default ReleasePageTabPanel;
