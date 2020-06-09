import classNames from 'classnames';
import React, { ReactNode } from 'react';
import styles from './TagGroup.module.scss';

interface Props {
  children: ReactNode;
  className?: string;
}

const TagGroup = ({ children, className }: Props) => {
  return <div className={classNames(styles.group, className)}>{children}</div>;
};

export default TagGroup;
