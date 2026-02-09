import PageNavExpandable, {
  NavItem,
} from '@common/components/PageNavExpandable';
import { useMobileMedia } from '@common/hooks/useMedia';
import styles from '@common/modules/release/components/ReleasePageLayout.module.scss';
import React, { ReactNode } from 'react';

interface Props {
  children: ReactNode;
  navItems: NavItem[];
  onClickNavItem?: (title: string) => void;
}

const ReleasePageLayout = ({ children, navItems, onClickNavItem }: Props) => {
  const { isMedia: isMobileMedia } = useMobileMedia();

  return (
    <div className={styles.wrapper}>
      {!isMobileMedia && (
        <div className={styles.sidebar}>
          {navItems.length > 0 && (
            <PageNavExpandable
              items={navItems}
              onClickNavItem={onClickNavItem}
            />
          )}
        </div>
      )}
      <div className={styles.content}>{children}</div>
    </div>
  );
};

export default ReleasePageLayout;
