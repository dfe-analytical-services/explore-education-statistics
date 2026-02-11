import classNames from 'classnames';
import React, { ReactNode } from 'react';

interface Props {
  children: ReactNode;
  className?: string;
  icon?: string;
  testId?: string;
}

const WarningMessage = ({ children, className, icon = '!', testId }: Props) => {
  return (
    <div
      className={classNames('govuk-warning-text', className)}
      data-testid={testId}
    >
      <span className="govuk-warning-text__icon" aria-hidden="true">
        {icon}
      </span>
      <strong className="govuk-warning-text__text">
        <span className="govuk-visually-hidden">Warning</span>
        {children}
      </strong>
    </div>
  );
};

export default WarningMessage;
