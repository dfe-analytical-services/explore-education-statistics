import classNames from 'classnames';
import React, { ReactNode } from 'react';

export interface TaskListProps {
  children: ReactNode;
  className?: string;
  testId?: string;
}

export default function TaskList({
  children,
  className,
  testId,
}: TaskListProps) {
  return (
    <ul
      className={classNames('govuk-task-list', className)}
      data-testid={testId}
    >
      {children}
    </ul>
  );
}
