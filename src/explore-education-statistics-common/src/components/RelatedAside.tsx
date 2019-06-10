import React, { ReactNode } from 'react';
import styles from './RelatedItems.module.scss';

interface Props {
  children: ReactNode;
}

const RelatedAside = ({ children }: Props) => {
  return <aside className={styles.container}>{children}</aside>;
};

export default RelatedAside;
