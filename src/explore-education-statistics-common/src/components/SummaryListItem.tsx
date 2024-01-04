import styles from '@common/components/SummaryList.module.scss';
import classNames from 'classnames';
import React, { ReactNode } from 'react';

interface Props {
  actions?: ReactNode;
  children?: ReactNode;
  className?: string;
  term: string | ReactNode;
  testId?: string;
}

const SummaryListItem = ({
  actions,
  children,
  className,
  term,
  testId = typeof term === 'string' ? term : undefined,
}: Props) => {
  return (
    <div
      className={classNames('govuk-summary-list__row', className)}
      data-testid={testId}
    >
      <dt
        className={`govuk-summary-list__key ${styles.key}`}
        data-testid={`${testId}-key`}
      >
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
