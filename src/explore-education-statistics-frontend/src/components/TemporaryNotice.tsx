import classNames from 'classnames';
import React, { ReactNode } from 'react';
import styles from './TemporaryNotice.module.scss';

interface Props {
  children: ReactNode;
  expires?: Date;
  wide?: boolean;
}

const TemporaryNotice = ({ children, expires, wide }: Props) => {
  if (expires && new Date() > expires) {
    return null;
  }

  return (
    <aside className={styles.notice}>
      <div
        className={classNames('govuk-width-container', {
          'dfe-width-container--wide': wide,
        })}
      >
        {children}
      </div>
    </aside>
  );
};

export default TemporaryNotice;
