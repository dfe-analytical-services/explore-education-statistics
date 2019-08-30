import classNames from 'classnames';
import React, { ReactNode } from 'react';

interface Props {
  children: ReactNode;
  noBorder?: boolean;
  additionalClassName?: string;
}

const SummaryList = ({ children, noBorder, additionalClassName }: Props) => {
  return (
    <dl
      className={classNames('govuk-summary-list', {
        'govuk-summary-list--no-border': noBorder,
        [additionalClassName || '']: additionalClassName,
      })}
    >
      {children}
    </dl>
  );
};

export default SummaryList;
