import React, { ReactNode } from 'react';
import classNames from 'classnames';

interface Props {
  actions?: ReactNode;
  children?: ReactNode;
  term: string;
  detailsNoMargin?: boolean;
}

const SummaryListItem = ({
  actions,
  children,
  term,
  detailsNoMargin,
}: Props) => {
  return (
    <div className="govuk-summary-list__row">
      <dt className="govuk-summary-list__key">{term}</dt>
      {children && (
        <dd
          className={classNames('govuk-summary-list__value', {
            'dfe-details-no-margin': detailsNoMargin,
          })}
        >
          {children}
        </dd>
      )}
      {actions && <dd className="govuk-summary-list__actions">{actions}</dd>}
      {!children && !actions && (
        <dd
          className={classNames('govuk-summary-list__value', {
            'dfe-details-no-margin': detailsNoMargin,
          })}
        />
      )}
    </div>
  );
};

export default SummaryListItem;
