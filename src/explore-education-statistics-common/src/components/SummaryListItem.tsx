import React, { ReactNode } from 'react';

interface Props {
  actions?: ReactNode;
  children?: ReactNode;
  term: string;
  testId?: string;
}

const SummaryListItem = ({ actions, children, term, testId }: Props) => {
  return (
    <div className="govuk-summary-list__row">
      <dt className="govuk-summary-list__key">{term}</dt>

      {children && (
        <dd className="govuk-summary-list__value" data-testid={testId}>
          {children}
        </dd>
      )}
      {actions && <dd className="govuk-summary-list__actions">{actions}</dd>}
    </div>
  );
};

export default SummaryListItem;
