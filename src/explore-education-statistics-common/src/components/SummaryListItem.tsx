import React, { ReactNode } from 'react';

interface Props {
  actions?: ReactNode;
  children?: ReactNode;
  term: string | ReactNode;
  testId?: string;
}

const SummaryListItem = ({
  actions,
  children,
  term,
  testId = typeof term === 'string' ? term : undefined,
}: Props) => {
  return (
    <div className="govuk-summary-list__row" data-testid={testId}>
      <dt className="govuk-summary-list__key" data-testid={`${testId}-key`}>
        {term}
      </dt>

      {typeof children !== 'undefined' && (
        <dd
          className="govuk-summary-list__value"
          data-testid={`${testId}-value`}
        >
          {children}
        </dd>
      )}

      {typeof actions !== 'undefined' && (
        <dd
          className="govuk-summary-list__actions"
          data-testid={`${testId}-actions`}
        >
          {actions}
        </dd>
      )}
    </div>
  );
};

export default SummaryListItem;
