import React, { ReactNode } from 'react';
import classNames from 'classnames';
import styles from './SummaryListItem.module.scss';

interface Props {
  actions?: ReactNode;
  children?: ReactNode;
  term: string;
  smallKey?: boolean;
  showActions?: boolean;
}

const SummaryListItem = ({
  actions,
  children,
  term,
  smallKey = false,
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
      {children && <dd className="govuk-summary-list__value">{children}</dd>}
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
