import React, { ReactNode } from 'react';
import classNames from 'classnames';
import CollapsibleList from './CollapsibleList';
import styles from './SummaryListItem.module.scss';

interface Props {
  actions?: ReactNode;
  children?: ReactNode;
  term: string;
  smallKey?: boolean;
  showActions?: boolean;
  shouldCollapse?: boolean;
  collapseAfter?: number;
}

const SummaryListItem = ({
  actions,
  children,
  term,
  smallKey = false,
  shouldCollapse = false,
  collapseAfter = 5,
}: Props) => {
  return (
    <div className="govuk-summary-list__row">
      <dt
        className={classNames('govuk-summary-list__key', {
          [styles.smallKey]: smallKey,
        })}
      >
        {term}
      </dt>
      {children && (
        <dd className="govuk-summary-list__value">
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
          <dd className="govuk-summary-list__value" />
          <dd className="govuk-summary-list__actions" />
        </>
      )}
    </div>
  );
};

export default SummaryListItem;
