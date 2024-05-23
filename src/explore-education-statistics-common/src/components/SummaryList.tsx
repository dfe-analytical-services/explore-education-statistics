import classNames from 'classnames';
import React, { ReactNode } from 'react';
import styles from './SummaryList.module.scss';

interface Props {
  ariaLabel?: string;
  children: ReactNode;
  className?: string;
  compact?: boolean;
  id?: string;
  noBorder?: boolean;
  smallKey?: boolean;
  testId?: string;
}

const SummaryList = ({
  ariaLabel,
  children,
  className,
  compact = false,
  id,
  noBorder,
  smallKey = false,
  testId,
}: Props) => {
  return (
    <dl
      aria-label={ariaLabel}
      className={classNames('govuk-summary-list', className, {
        [styles.compact]: compact,
        [styles.smallKey]: smallKey,
        'govuk-summary-list--no-border': noBorder,
      })}
      data-testid={testId}
      id={id}
    >
      {children}
    </dl>
  );
};

export default SummaryList;
