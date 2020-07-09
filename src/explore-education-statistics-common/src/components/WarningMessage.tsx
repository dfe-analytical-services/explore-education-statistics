import classNames from 'classnames';
import React, { ReactNode } from 'react';
import styles from './WarningMessage.module.scss';

interface Props {
  children: ReactNode;
  className?: string;
  icon?: string;
}

const WarningMessage = ({ children, className, icon = '!' }: Props) => {
  return (
    <div
      className={classNames('govuk-warning-text', styles.warning, className)}
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
