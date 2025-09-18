import styles from '@common/modules/education-in-numbers/components/FreeTextStatTileWrapper.module.scss';
import React, { ReactNode } from 'react';

export interface FreeTextStatTileWrapperProps {
  children: ReactNode;
}

const FreeTextStatTileWrapper = ({
  children,
}: FreeTextStatTileWrapperProps) => {
  return <div className={styles.wrapper}>{children}</div>;
};

export default FreeTextStatTileWrapper;
