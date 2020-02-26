import ContentBlockDroppable from '@admin/modules/find-statistics/components/ContentBlockDroppable';
import AccordionSection, {
  AccordionSectionProps,
} from '@common/components/AccordionSection';
import { FormTextInput } from '@common/components/form';
import GoToTopLink from '@common/components/GoToTopLink';
import ModalConfirm from '@common/components/ModalConfirm';
import wrapEditableComponent from '@common/modules/find-statistics/util/wrapEditableComponent';
import { Dictionary } from '@common/types/util';
import classNames from 'classnames';
import React, { createElement, createRef, ReactNode, useState } from 'react';
import { DragDropContext, DropResult } from 'react-beautiful-dnd';
import styles from './EditableAccordionSection.module.scss';

export interface EditableAccordionSectionProps extends AccordionSectionProps {
  sectionId: string;
  index?: number;
  footerButtons?: ReactNode;
  canToggle?: boolean;
  onHeadingChange: (heading: string) => Promise<unknown>;
  canRemoveSection: boolean;
  onRemoveSection: () => Promise<unknown>;
  onSaveOrder: (order: Dictionary<number>) => Promise<unknown>;
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
  onSaveOrder,
  sectionId,
  footerButtons,
}: EditableAccordionSectionProps) => {
  const target = createRef<HTMLDivElement>();
  const [isOpen, setIsOpen] = useState(open);
  const [isReordering, setIsReordering] = useState(false);
  const [showRemoveModal, setShowRemoveModal] = useState(false);
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

  const onDragEnd = React.useCallback(
    (result: DropResult) => {
      /* const { source, destination, type } = result;

      if (type === 'content' && destination) {
        const newContentBlocks = [...(contentBlocks || [])];
        const [removed] = newContentBlocks.splice(source.index, 1);
        newContentBlocks.splice(destination.index, 0, removed);
        setContentBlocks(newContentBlocks);
        if (onContentChange) onContentChange(newContentBlocks);
      } */
    },
    [
      /* contentBlocks, onContentChange */
    ],
  );

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
                onChange={e => {
                  setCurrentHeading(e.target.value);
                }}
                onClick={e => {
                  e.stopPropagation();
                }}
                onKeyPress={e => {
                  if (e.key === 'Enter') editHeading(e);
                }}
              />
            ) : (
              currentHeading
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
          {onHeadingChange && (
            <a
              role="button"
              tabIndex={0}
              onClick={editHeading}
              onKeyPress={e => {
                if (e.charCode === 13) editHeading(e);
              }}
              className={`govuk-button ${!isEditingHeading &&
                'govuk-button--secondary'} govuk-!-margin-right-2`}
            >
              {isEditingHeading ? 'Save' : 'Edit'} title
            </a>
          )}
          {!!onSaveOrder && (
            <a
              role="button"
              tabIndex={0}
              onClick={() => setIsReordering(!isReordering)}
              onKeyPress={e => {
                if (e.key === 'Enter') setIsReordering(!isReordering);
              }}
              className={`govuk-button ${!isReordering &&
                'govuk-button--secondary'} govuk-!-margin-right-2`}
            >
              {isReordering ? 'Save order' : 'Reorder'}
            </a>
          )}
          {!!onRemoveSection && (
            <a
              role="button"
              tabIndex={0}
              onClick={() => setShowRemoveModal(true)}
              onKeyPress={e => {
                if (e.key === 'Enter') setShowRemoveModal(true);
              }}
              className="govuk-button govuk-button--warning"
            >
              Remove section
              {showRemoveModal && (
                <ModalConfirm
                  title="Are you sure?"
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
              )}
            </a>
          )}
        </div>

        {children && (
          <>
            {onSaveOrder ? (
              <DragDropContext onDragEnd={onDragEnd}>
                <ContentBlockDroppable
                  draggable={isReordering}
                  droppableId={sectionId}
                >
                  {children}
                </ContentBlockDroppable>
              </DragDropContext>
            ) : (
              children
            )}
            {footerButtons}
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
