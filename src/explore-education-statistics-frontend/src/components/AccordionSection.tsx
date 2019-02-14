import classNames from 'classnames';
import React, { createElement, ReactNode } from 'react';
import GoToTopLink from './GoToTopLink';

export interface AccordionSectionProps {
  caption?: string;
  children: ReactNode;
  className?: string;
  contentId?: string;
  goToTop?: boolean;
  heading: string;
  headingId?: string;
  // Only for accessibility/semantic markup,
  // does not change the actual styling
  headingTag?: 'h2' | 'h3' | 'h4';
  id?: string;
  open?: boolean;
  onToggle?: (open: boolean) => void;
}

const AccordionSection = ({
  caption,
  className,
  children,
  contentId,
  goToTop = true,
  heading,
  headingId,
  headingTag = 'h2',
  open = false,
  onToggle,
}: AccordionSectionProps) => {
  return (
    <div
      onClick={event => {
        if (onToggle) {
          onToggle(
            event.currentTarget.classList.contains(
              'govuk-accordion__section--expanded',
            ),
          );
        }
      }}
      className={classNames('govuk-accordion__section', className, {
        'govuk-accordion__section--expanded': open,
      })}
    >
      <div className="govuk-accordion__section-header">
        {createElement(
          headingTag,
          {
            className: 'govuk-accordion__section-heading',
          },
          <span className="govuk-accordion__section-button" id={headingId}>
            {heading}
          </span>,
        )}
        {caption && (
          <span className="govuk-accordion__section-summary">{caption}</span>
        )}
      </div>

      <div
        className="govuk-accordion__section-content"
        aria-labelledby={headingId}
        id={contentId}
      >
        {children}

        {goToTop && <GoToTopLink />}
      </div>
    </div>
  );
};

export default AccordionSection;
