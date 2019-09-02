import React from 'react';
import styles from './LinkContainer.module.scss';

interface Props {
  url: string;
}

const LinkContainer = ({ url }: Props) => (
  <div className={styles.container}>
    <span className={styles.linkSelect}>{url}</span>
  </div>
);

export default LinkContainer;
