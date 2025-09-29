import PageNavExpandable, {
  NavItem,
} from '@common/components/PageNavExpandable';
import { useMobileMedia } from '@common/hooks/useMedia';
import styles from '@common/modules/release/components/ReleasePageLayout.module.scss';
import React, { ReactNode } from 'react';

interface Props {
  activeSection?: string;
  children: ReactNode;
  navItems: NavItem[];
  onChangeSection?: (id: string) => void;
  onClickNavItem?: (id: string) => void;
}

const ReleasePageLayout = ({
  activeSection,
  children,
  navItems,
  onChangeSection,
  onClickNavItem,
}: Props) => {
  const { isMedia: isMobileMedia } = useMobileMedia();

  return (
    <div className={styles.wrapper}>
      {!isMobileMedia && (
        <div className={styles.sidebar}>
          <PageNavExpandable
            activeSection={activeSection}
            items={navItems}
            onChangeSection={onChangeSection}
            onClickItem={onClickNavItem}
          />
        </div>
      )}
      <div className={styles.content}>{children}</div>
    </div>
  );
};

export default ReleasePageLayout;
