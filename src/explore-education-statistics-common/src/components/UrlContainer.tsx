import React from 'react';
import styles from './UrlContainer.module.scss';

interface Props {
  'data-testid'?: string;
  url: string;
}

const UrlContainer = ({ url, 'data-testid': dataTestId }: Props) => (
  <span className={styles.url} data-testid={dataTestId}>
    {url}
  </span>
);

export default UrlContainer;
