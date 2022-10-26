import classNames from 'classnames';
import React, { ReactNode } from 'react';
import styles from './PrototypeGridView.module.scss';

interface Props {
  children: ReactNode;
  className?: string;
}

const GridView = ({ children, className }: Props) => {
  return (
    <ul className={classNames(className, 'govuk-list', styles.grid)}>
      {children}
    </ul>
  );
};

export default GridView;
