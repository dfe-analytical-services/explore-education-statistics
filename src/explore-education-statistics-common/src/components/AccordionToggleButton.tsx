import classNames from 'classnames';
import React, { ReactNode } from 'react';

interface Props {
  ariaControls?: string;
  expanded?: boolean;
  label: string | ReactNode;
  onClick: () => void;
}

export default function AccordionToggleButton({
  ariaControls,
  expanded = false,
  label,
  onClick,
}: Props) {
  return (
    <button
      aria-controls={ariaControls}
      aria-expanded={expanded}
      type="button"
      className="govuk-accordion__show-all"
      onClick={onClick}
    >
      <span
        className={classNames('govuk-accordion-nav__chevron', {
          'govuk-accordion-nav__chevron--down': !expanded,
        })}
      />

      <span className="govuk-accordion__show-all-text">{label}</span>
    </button>
  );
}
