import React from 'react';
import styles from './LinkContainer.module.scss';

interface Props {
  url: string;
  datatestid?: string;
}

const LinkContainer = ({ url, datatestid }: Props) => (
  <div className={styles.container}>
    <span className={styles.linkSelect} data-testid={datatestid}>
      {url}
    </span>
  </div>
);

export default LinkContainer;
