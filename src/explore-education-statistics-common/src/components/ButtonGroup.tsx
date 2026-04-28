import classNames from 'classnames';
import React, { ReactNode } from 'react';
import styles from './ButtonGroup.module.scss';

interface Props {
  children: ReactNode;
  className?: string;
  alignment?: 'start';
  horizontalSpacing?: 'l' | 'm' | 's';
  verticalSpacing?: 'l' | 'm' | 's';
}

export default function ButtonGroup({
  alignment,
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
        {
          [styles.alignStart]: alignment === 'start',
        },
        className,
      )}
    >
      {children}
    </div>
  );
}
