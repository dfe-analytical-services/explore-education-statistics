import classNames from 'classnames';
import React, { ReactNode } from 'react';
import styles from './ButtonGroup.module.scss';

interface Props {
  children: ReactNode;
  className?: string;
  horizontalSpacing?: 'l' | 'm' | 's';
  verticalSpacing?: 'l' | 'm' | 's';
}

export default function ButtonGroup({
  children,
  className,
  horizontalSpacing = 's',
  verticalSpacing = 's',
}: Props) {
  return (
    <div
      className={classNames(
        styles.group,
        styles[`horizontalSpacing--${horizontalSpacing}`],
        styles[`verticalSpacing--${verticalSpacing}`],
        className,
      )}
    >
      {children}
    </div>
  );
}
