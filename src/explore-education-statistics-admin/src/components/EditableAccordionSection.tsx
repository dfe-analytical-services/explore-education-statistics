import AccordionSection, {
  AccordionSectionProps,
} from '@common/components/AccordionSection';
import Button from '@common/components/Button';
import { FormTextInput } from '@common/components/form';
import GoToTopLink from '@common/components/GoToTopLink';
import ModalConfirm from '@common/components/ModalConfirm';
import wrapEditableComponent from '@common/modules/find-statistics/util/wrapEditableComponent';
import classNames from 'classnames';
import React, { createElement, createRef, ReactNode, useState } from 'react';
import styles from './EditableAccordionSection.module.scss';

export interface EditableAccordionSectionProps extends AccordionSectionProps {
  sectionId: string;
  index?: number;
  headerButtons?: ReactNode;
  footerButtons?: ReactNode;
  canToggle?: boolean;
  onHeadingChange: (heading: string) => Promise<unknown>;
  canRemoveSection: boolean;
  onRemoveSection: () => Promise<unknown>;
  isReordering: boolean;
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
  canToggle = true,
  open = false,
  onToggle,
  onHeadingChange,
  onRemoveSection,
  headerButtons,
}: EditableAccordionSectionProps) => {
  const target = createRef<HTMLDivElement>();
  const [isOpen, setIsOpen] = useState(open);
  const [showRemoveModal, setShowRemoveModal] = useState(false);
  const [previousOpen, setPreviousOpen] = useState(open);
  const [isEditingHeading, setIsEditingHeading] = useState(false);

  const [newHeading, setNewHeading] = useState(heading);

  if (open !== previousOpen) {
    setPreviousOpen(open);
    setIsOpen(open);
  }

  const saveHeading = () => {
    if (isEditingHeading && onHeadingChange && newHeading !== heading) {
      onHeadingChange(newHeading);
    }
    setIsEditingHeading(false);
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
                defaultValue={newHeading}
                onChange={e => {
                  setNewHeading(e.target.value);
                }}
                onClick={e => {
                  e.stopPropagation();
                }}
                onKeyPress={e => {
                  if (e.key === 'Enter') saveHeading();
                  if (e.key === 'Esc') setIsEditingHeading(false);
                }}
              />
            ) : (
              heading
            )}
          </span>,
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
        <div>
          {onHeadingChange && isEditingHeading ? (
            <Button onClick={saveHeading}>Save title</Button>
          ) : (
            <Button
              type="button"
              onClick={() => setIsEditingHeading(!isEditingHeading)}
              variant="secondary"
            >
              Edit title
            </Button>
          )}
          {headerButtons}
          {!!onRemoveSection && (
            <Button onClick={() => setShowRemoveModal(true)} variant="warning">
              Remove section
              <ModalConfirm
                title="Are you sure?"
                mounted={showRemoveModal}
                onConfirm={onRemoveSection}
                onExit={() => setShowRemoveModal(false)}
                onCancel={() => setShowRemoveModal(false)}
              >
                <p>
                  Are you sure you want to remove the following section?
                  <br />
                  <strong>"{heading}"</strong>
                </p>
              </ModalConfirm>
            </Button>
          )}
        </div>

        {children && (
          <>
            {children}
            {goToTop && <GoToTopLink />}
          </>
        )}
      </div>
    </div>
  );
};

export default wrapEditableComponent(
  EditableAccordionSection,
  AccordionSection,
);
