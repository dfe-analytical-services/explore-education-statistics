import styles from '@common/modules/education-in-numbers/components/TileWrapper.module.scss';
import React, { ReactNode } from 'react';

export interface TileWrapperProps {
  children: ReactNode;
}

const TileWrapper = ({ children }: TileWrapperProps) => {
  return <div className={styles.wrapper}>{children}</div>;
};

export default TileWrapper;
