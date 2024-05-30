import classNames from 'classnames';
import React, { ReactNode } from 'react';

interface SummaryCardActionProps {
  children: ReactNode;
  className?: string;
}

export default function SummaryCardAction({
  children,
  className,
}: SummaryCardActionProps) {
  return (
    <li className={classNames('govuk-summary-card__action', className)}>
      {children}
    </li>
  );
}
