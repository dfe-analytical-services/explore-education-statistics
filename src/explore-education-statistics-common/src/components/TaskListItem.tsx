import classNames from 'classnames';
import React, { ReactNode } from 'react';
import styles from './TaskListItem.module.scss';

export interface TaskListItemRenderProps {
  'aria-describedby': string;
}

export interface TaskListItemProps {
  children: (props: TaskListItemRenderProps) => ReactNode;
  className?: string;
  hint?: string;
  id: string;
  status: ReactNode;
}

export default function TaskListItem({
  children,
  className,
  hint,
  id,
  status,
}: TaskListItemProps) {
  const statusId = `${id}-status`;
  const hintId = `${id}-hint`;

  return (
    <li
      className={classNames('govuk-task-list__item', styles.item, className)}
      data-testid={id}
    >
      <div
        className={classNames(
          'govuk-task-list__name-and-hint',
          styles.nameAndHint,
        )}
      >
        {children({
          'aria-describedby': classNames(statusId, {
            [hintId]: !!hint,
          }),
        })}

        {hint && (
          <div className="govuk-task-list__hint" id={hintId}>
            {hint}
          </div>
        )}
      </div>
      <div className="govuk-task-list__status" id={statusId}>
        {status}
      </div>
    </li>
  );
}
