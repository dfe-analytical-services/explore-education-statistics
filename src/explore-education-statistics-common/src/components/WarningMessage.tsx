import classNames from 'classnames';
import React, { ReactNode } from 'react';
import styles from './WarningMessage.module.scss';

interface Props {
  children: ReactNode;
  className?: string;
  error?: boolean;
  icon?: string;
  testId?: string;
}

const WarningMessage = ({
  children,
  className,
  error,
  icon = '!',
  testId,
}: Props) => {
  return (
    <div
      className={classNames('govuk-warning-text', styles.warning, className, {
        [styles.error]: error,
      })}
      data-testid={testId}
    >
      <span
        className={classNames('govuk-warning-text__icon', styles.icon)}
        aria-hidden="true"
      >
        {icon}
      </span>
      <strong className={classNames('govuk-warning-text__text', styles.text)}>
        <span className="govuk-warning-text__assistive">Warning</span>
        {children}
      </strong>
    </div>
  );
};

export default WarningMessage;
