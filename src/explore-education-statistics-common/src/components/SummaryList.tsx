import classNames from 'classnames';
import React, { ReactNode } from 'react';
import styles from './SummaryList.module.scss';

interface Props {
  children: ReactNode;
  className?: string;
  compact?: boolean;
  noBorder?: boolean;
  smallKey?: boolean;
}

const SummaryList = ({
  children,
  className,
  compact = false,
  noBorder,
  smallKey = false,
}: Props) => {
  return (
    <dl
      className={classNames('govuk-summary-list', className, {
        [styles.compact]: compact,
        [styles.smallKey]: smallKey,
        'govuk-summary-list--no-border': noBorder,
      })}
    >
      {children}
    </dl>
  );
};

export default SummaryList;
