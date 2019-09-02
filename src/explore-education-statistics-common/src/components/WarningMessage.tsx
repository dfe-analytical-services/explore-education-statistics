import React, { ReactNode } from 'react';

interface Props {
  children: ReactNode;
  icon?: string;
}

const WarningMessage = ({ icon = '!', children }: Props) => {
  return (
    <div className="govuk-warning-text">
      <span className="govuk-warning-text__icon" aria-hidden="true">
        {icon}
      </span>
      <strong className="govuk-warning-text__text">
        <span className="govuk-warning-text__assistive">Warning</span>
        {children}
      </strong>
    </div>
  );
};

export default WarningMessage;
