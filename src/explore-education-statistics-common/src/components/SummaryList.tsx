import SummaryListItem from '@common/components/SummaryListItem';
import isComponentType from '@common/lib/type-guards/components/isComponentType';
import classNames from 'classnames';
import React, { Children, ReactNode } from 'react';

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
      {Children.toArray(children).filter(child =>
        isComponentType(child, SummaryListItem),
      )}
    </dl>
  );
};

export default SummaryList;
