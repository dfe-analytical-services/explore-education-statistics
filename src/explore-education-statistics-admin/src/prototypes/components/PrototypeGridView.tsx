import classNames from 'classnames';
import React, { ReactNode } from 'react';
import styles from './PrototypeGridView.module.scss';

interface Props {
  children: ReactNode;
  className?: string;
  view?: string;
}

const GridView = ({ children, className, view }: Props) => {
  return (
    <ul
      className={classNames(
        className,
        'govuk-list',
        view === 'list' ? styles.list : styles.grid,
      )}
    >
      {children}
    </ul>
  );
};

export default GridView;
