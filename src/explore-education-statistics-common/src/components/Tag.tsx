import styles from '@common/components/Tag.module.scss';
import classNames from 'classnames';
import React, { ReactNode } from 'react';

export interface TagProps {
  children: ReactNode;
  colour?:
    | 'grey'
    | 'green'
    | 'turquoise'
    | 'blue'
    | 'light-blue'
    | 'purple'
    | 'pink'
    | 'red'
    | 'orange'
    | 'yellow';
  className?: string;
  id?: string;
  testId?: string;
}

export default function Tag({
  children,
  className,
  colour,
  id,
  testId,
}: TagProps) {
  return (
    <strong
      className={classNames('govuk-tag', styles.tag, className, {
        [`govuk-tag--${colour}`]: !!colour,
      })}
      id={id}
      data-testid={testId}
    >
      {children}
    </strong>
  );
}
