import GoToTopLink from '@common/components/GoToTopLink';
import classNames from 'classnames';
import React, { createElement, createRef, ReactNode, useState } from 'react';
// import PrototypeEditableContent from "@admin/pages/prototypes/components/PrototypeEditableContent";

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
  editable?: boolean;
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
  // editable = false,
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
        if (onToggle) {
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
              if (target.current) {
                setIsOpen(!isOpen);
              }
            },
          },
          <span className="govuk-accordion__section-button" id={headingId}>
            {heading}
          </span>,
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
