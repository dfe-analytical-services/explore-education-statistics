import classNames from 'classnames';
import React from 'react';
import styles from './UrlContainer.module.scss';

interface Props {
  'data-testid'?: string;
  className?: string;
  url: string;
}

const UrlContainer = ({ 'data-testid': dataTestId, className, url }: Props) => (
  <span className={classNames(styles.url, className)} data-testid={dataTestId}>
    {url}
  </span>
);

export default UrlContainer;
