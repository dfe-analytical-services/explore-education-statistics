import { ReleasePageTabSectionKey } from '@admin/pages/release/content/components/ReleaseContentRedesign';
import styles from '@admin/pages/release/content/components/ReleasePageTabPanel.module.scss';
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
      data-testid="release-page-tab-panel"
      className={styles.panel}
    >
      {children}
    </div>
  );
};
export default ReleasePageTabPanel;
