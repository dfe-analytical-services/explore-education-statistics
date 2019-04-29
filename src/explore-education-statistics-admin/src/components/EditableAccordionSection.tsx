import GoToTopLink from '@common/components/GoToTopLink';
import classNames from 'classnames';
import React, { createElement, createRef, ReactNode } from 'react';

export interface EditableAccordionSectionProps {
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
  index: number;
  droppableIndex?: number;
}

const EditableAccordionSection = ({
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
}: EditableAccordionSectionProps) => {
  const target = createRef<HTMLDivElement>();

  return (
    <div
      ref={target}
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
      role="presentation"
    >
      <div className="govuk-accordion__section-header">
        {createElement(
          headingTag,
          {
            className: 'govuk-accordion__section-heading',
            onClick: () => {
              if (target.current) {
                target.current.classList.toggle(
                  'govuk-accordion__section--expanded',
                );
              }
            },
          },
          <button
            type="button"
            className="govuk-accordion__section-button"
            id={headingId}
          >
            {heading}
          </button>,
          <span className="govuk-accordion__icon" />,
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

export default EditableAccordionSection;
