import PageNavExpandable, {
  NavItem,
} from '@common/components/PageNavExpandable';
import { useMobileMedia } from '@common/hooks/useMedia';
import styles from '@common/modules/release/components/ReleasePageLayout.module.scss';
import React, { ReactNode } from 'react';

interface Props {
  children: ReactNode;
  navItems: NavItem[];
}

const ReleasePageLayout = ({ children, navItems }: Props) => {
  const { isMedia: isMobileMedia } = useMobileMedia();

  return (
    <div className={styles.wrapper}>
      {!isMobileMedia && (
        <div className={styles.sidebar}>
          {navItems.length > 0 && <PageNavExpandable items={navItems} />}
        </div>
      )}
      <div className={styles.content}>{children}</div>
    </div>
  );
};

export default ReleasePageLayout;
