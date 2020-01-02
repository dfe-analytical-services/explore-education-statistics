import AccordionSection, {
  AccordionSectionProps,
} from '@common/components/AccordionSection';
import GoToTopLink from '@common/components/GoToTopLink';
import classNames from 'classnames';
import React, { createElement, createRef, ReactNode, useState } from 'react';
import wrapEditableComponent from '@common/modules/find-statistics/util/wrapEditableComponent';
import { FormTextInput } from '@common/components/form';
import styles from './EditableAccordionSection.module.scss';

export interface EditableAccordionSectionProps extends AccordionSectionProps {
  index?: number;
  headingButtons?: ReactNode[];
  canToggle?: boolean;
  onHeadingChange: (heading: string) => Promise<unknown>;
  canEditHeading: boolean;
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
  onHeadingChange,
  canEditHeading,
}: EditableAccordionSectionProps) => {
  const target = createRef<HTMLDivElement>();
  const [isOpen, setIsOpen] = useState(open);
  const [previousOpen, setPreviousOpen] = useState(open);

  const [currentHeading, setCurrentHeading] = useState(heading);

  const [isEditingHeading, setIsEditingHeading] = useState(false);

  if (open !== previousOpen) {
    setPreviousOpen(open);
    setIsOpen(open);
  }

  const editHeading = (e: React.MouseEvent | React.KeyboardEvent) => {
    e.stopPropagation();

    if (isEditingHeading && onHeadingChange && currentHeading !== heading) {
      onHeadingChange(currentHeading).then(() => {
        setIsEditingHeading(false);
      });
    } else {
      setIsEditingHeading(!isEditingHeading);
    }
  };

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
          <span
            className={classNames(
              'govuk-accordion__section-button',
              styles['editable-header'],
            )}
          >
            {isEditingHeading ? (
              <FormTextInput
                id="heading"
                name="heading"
                label="Edit Heading"
                defaultValue={currentHeading}
                onChange={e => setCurrentHeading(e.target.value)}
                onClick={e => {
                  e.stopPropagation();
                }}
              />
            ) : (
              currentHeading
            )}
          </span>,
          canEditHeading && (
            <a
              role="button"
              tabIndex={0}
              onClick={editHeading}
              onKeyPress={e => {
                if (e.charCode === 13) editHeading(e);
              }}
              className={styles.edit}
            >
              ({isEditingHeading ? 'Save' : 'Edit'} Section Title)
            </a>
          ),
          headingButtons,
          canToggle && <span className="govuk-accordion__icon" />,
        )}
        {caption && (
          <span className="govuk-accordion__section-summary">{caption}</span>
        )}
      </div>
      {children && (
        <div
          className="govuk-accordion__section-content"
          aria-labelledby={headingId}
          id={contentId}
        >
          {children}

          {goToTop && <GoToTopLink />}
        </div>
      )}
    </div>
  );
};

export default wrapEditableComponent(
  EditableAccordionSection,
  AccordionSection,
);
