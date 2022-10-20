import classNames from 'classnames';
import React, { ReactNode } from 'react';
//import styles from './SummaryList.module.scss';

interface Props {
  children: ReactNode;
  className?: string;
}

const GridView = ({ children, className }: Props) => {
  return (
    <ul
      className={classNames(className, 'govuk-list')}
      style={{ display: 'grid', gridTemplateColumns: '1fr 1fr' }}
    >
      {children}
    </ul>
  );
};

export default GridView;
