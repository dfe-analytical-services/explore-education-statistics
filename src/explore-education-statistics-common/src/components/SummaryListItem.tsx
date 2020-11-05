import React, { ReactNode } from 'react';

interface Props {
  actions?: ReactNode;
  children?: ReactNode;
  term: string;
  testId?: string;
}

const SummaryListItem = ({ actions, children, term, testId = term }: Props) => {
  return (
    <div className="govuk-summary-list__row" data-testid={testId}>
      <dt className="govuk-summary-list__key">{term}</dt>

      {typeof children !== 'undefined' && (
        <dd className="govuk-summary-list__value">{children}</dd>
      )}

      {typeof actions !== 'undefined' && (
        <dd className="govuk-summary-list__actions">{actions}</dd>
      )}
    </div>
  );
};

export default SummaryListItem;
