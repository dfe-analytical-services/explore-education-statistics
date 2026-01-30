import { ReleasePageTabSectionKey } from '@admin/pages/release/content/components/ReleaseContentRedesign';
import styles from '@admin/pages/release/content/components/ReleasePageTabPanel.module.scss';
import React, { ReactNode } from 'react';

interface Props {
  children: ReactNode;
  tabKey: ReleasePageTabSectionKey;
}

const ReleasePageTabPanel = ({ children, tabKey }: Props) => {
  return (
    <div
      aria-labelledby={`tab-${tabKey}-tab`}
      id={`tab-${tabKey}`}
      role="tabpanel"
      data-testid="release-page-tab-panel"
      className={styles.panel}
    >
      {children}
    </div>
  );
};
export default ReleasePageTabPanel;
