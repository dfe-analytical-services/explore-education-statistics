import classNames from 'classnames';
import React, { ReactNode } from 'react';
import styles from './TemporaryNotice.module.scss';
import { PageWidth } from './Page';

interface Props {
  children: ReactNode;
  start?: Date;
  end?: Date;
  width?: PageWidth;
}

const TemporaryNotice = ({ children, start, end, width }: Props) => {
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
          'dfe-width-container--wide': width === 'wide',
          'dfe-width-container--full': width === 'full',
        })}
      >
        {children}
      </div>
    </aside>
  );
};

export default TemporaryNotice;
