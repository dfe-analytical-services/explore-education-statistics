import classNames from 'classnames';
import React, { ReactNode } from 'react';

interface Props {
  children: ReactNode;
  className?: string;
  noBorder?: boolean;
}

const SummaryList = ({ children, noBorder, className }: Props) => {
  return (
    <dl
      className={classNames('govuk-summary-list', className, {
        'govuk-summary-list--no-border': noBorder,
      })}
    >
      {children}
    </dl>
  );
};

export default SummaryList;
