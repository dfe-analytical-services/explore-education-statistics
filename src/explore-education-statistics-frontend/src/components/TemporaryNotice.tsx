import classNames from 'classnames';
import React, { ReactNode } from 'react';
import styles from './TemporaryNotice.module.scss';

interface Props {
  children: ReactNode;
  start?: Date;
  end?: Date;
  wide?: boolean;
}

const TemporaryNotice = ({ children, start, end, wide }: Props) => {
  const now = new Date();

  if (start && now < start) {
    return null;
  }

  if (end && now > end) {
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
