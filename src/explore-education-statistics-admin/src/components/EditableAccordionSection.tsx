import AccordionSection, {
  AccordionSectionProps,
} from '@common/components/AccordionSection';
import GoToTopLink from '@common/components/GoToTopLink';
import classNames from 'classnames';
import React, { createElement, createRef, useState, ReactNode } from 'react';
import wrapEditableComponent from '@common/modules/find-statistics/util/wrapEditableComponent';

export interface EditableAccordionSectionProps extends AccordionSectionProps {
  index?: number;
  headingButtons?: ReactNode[];
  canToggle?: boolean;
}

const EditableAccordionSection = ({
  caption,
  className,
  children,
  contentId,
  goToTop = true,
  heading,
  headingButtons,
  headingId,
  headingTag = 'h2',
  canToggle = true,
  open = false,
  onToggle,
}: EditableAccordionSectionProps) => {
  const target = createRef<HTMLDivElement>();
  const [isOpen, setIsOpen] = useState(open);
  const [previousOpen, setPreviousOpen] = useState(open);

  if (open !== previousOpen) {
    setPreviousOpen(open);
    setIsOpen(open);
  }

  return (
    <div
      ref={target}
      onClick={() => {
        if (canToggle && onToggle) {
          onToggle(isOpen);
        }
      }}
      className={classNames('govuk-accordion__section', className, {
        'govuk-accordion__section--expanded': isOpen,
      })}
      role="presentation"
    >
      <div className="govuk-accordion__section-header">
        {createElement(
          headingTag,
          {
            className: 'govuk-accordion__section-heading',
            onClick: () => {
              if (canToggle && target.current) {
                setIsOpen(!isOpen);
              }
            },
          },
          <span className="govuk-accordion__section-button" id={headingId}>
            {heading}
          </span>,
          headingButtons,
          canToggle && <span className="govuk-accordion__icon" />,
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

export default wrapEditableComponent(
  EditableAccordionSection,
  AccordionSection,
);
