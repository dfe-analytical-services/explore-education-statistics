import classNames from 'classnames';
import React, { ReactNode, useState } from 'react';
import styles from '@common/components/Accordion.module.scss';

interface Props {
  children: ReactNode;
  className?: string;
  listCompact?: boolean;
}

const ToggleMoreDetails = ({ children, className, listCompact }: Props) => {
  const [listCompactView, setListCompact] = useState(listCompact);

  return (
    <>
      <div
        className={classNames({
          'govuk-visually-hidden': listCompactView && listCompact,
        })}
      >
        {children}
      </div>
      <div className="govuk-accordion__controls govuk-!-margin-top-2">
        <button
          type="button"
          className="govuk-accordion__show-all"
          onClick={() => {
            setListCompact(!listCompactView);
          }}
        >
          <span
            className={classNames('govuk-accordion-nav__chevron', {
              'govuk-accordion-nav__chevron--down': listCompactView,
            })}
          />
          <span className="govuk-accordion__show-all-text">
            {`${
              listCompactView && listCompact ? 'Show more' : 'Hide'
            } details `}
          </span>
        </button>
      </div>
    </>
  );
};

export default ToggleMoreDetails;
