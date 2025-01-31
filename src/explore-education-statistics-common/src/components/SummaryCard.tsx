import classNames from 'classnames';
import React, { ReactNode } from 'react';

export interface SummaryCardProps {
  actions?: ReactNode;
  children: ReactNode;
  className?: string;
  heading: ReactNode;
  headingTag: 'h2' | 'h3' | 'h4';
}

export default function SummaryCard({
  actions,
  children,
  className,
  heading,
  headingTag: Heading = 'h2',
}: SummaryCardProps) {
  return (
    <div className={classNames('govuk-summary-card', className)}>
      <div className="govuk-summary-card__title-wrapper">
        <Heading className="govuk-summary-card__title">{heading}</Heading>
        {actions && <ul className="govuk-summary-card__actions">{actions}</ul>}
      </div>
      <div className="govuk-summary-card__content">{children}</div>
    </div>
  );
}
