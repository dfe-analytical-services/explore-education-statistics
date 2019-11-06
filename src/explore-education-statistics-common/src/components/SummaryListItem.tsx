import React, { ReactNode, useState } from 'react';
import classNames from 'classnames';
import ButtonText from './ButtonText';
import CollapsibleList from './CollapsibleList';

interface Props {
  actions?: ReactNode;
  children?: ReactNode;
  term: string;
  detailsNoMargin?: boolean;
  smallKey?: boolean;
  showActions?: boolean;
  shouldCollapse?: boolean;
  collapseAfter?: number;
}

const SummaryListItem = ({
  actions,
  children,
  term,
  detailsNoMargin,
  smallKey = false,
  shouldCollapse = false,
  collapseAfter = 5,
}: Props) => {
  return (
    <div className="govuk-summary-list__row">
      <dt
        className={classNames('govuk-summary-list__key', {
          'dfe-details-no-margin': detailsNoMargin,
          'dfe-summary-list__key--small': smallKey,
        })}
      >
        {term}
      </dt>
      {children && (
        <dd
          className={classNames('govuk-summary-list__value', {
            'dfe-details-no-margin': detailsNoMargin,
          })}
        >
          {shouldCollapse && children ? (
            <CollapsibleList collapseAfter={collapseAfter}>
              {children}
            </CollapsibleList>
          ) : (
            children
          )}
        </dd>
      )}
      {actions && <dd className="govuk-summary-list__actions">{actions}</dd>}
      {!children && !actions && (
        <>
          <dd
            className={classNames('govuk-summary-list__value', {
              'dfe-details-no-margin': detailsNoMargin,
            })}
          />
          <dd className="govuk-summary-list__actions" />
        </>
      )}
    </div>
  );
};

export default SummaryListItem;
