import useMounted from '@common/hooks/useMounted';
import findAllParents from '@common/utils/dom/findAllParents';
import classNames from 'classnames';
import React, { createElement, memo, ReactNode } from 'react';
import styles from './AccordionSection.module.scss';
import GoToTopLink from './GoToTopLink';

export type ToggleHandler = (open: boolean, id: string) => void;

export interface AccordionSectionProps {
  caption?: string;
  children?:
    | ReactNode
    | ((props: {
        open: boolean;
        headingId: string;
        contentId: string;
      }) => ReactNode);
  className?: string;
  goToTop?: boolean;
  header?: ReactNode;
  heading: string;
  /**
   * Only for accessibility/semantic markup,
   * does not change the actual styling
   */
  headingTag?: 'h2' | 'h3' | 'h4';
  id?: string;
  open?: boolean;
  onToggle?: ToggleHandler;
}

const classes = {
  section: 'govuk-accordion__section',
  sectionButton: 'govuk-accordion__section-button',
  sectionContent: 'govuk-accordion__section-content',
  sectionHeading: 'govuk-accordion__section-heading',
  expanded: 'govuk-accordion__section--expanded',
};

export const accordionSectionClasses = classes;

const AccordionSection = ({
  caption,
  className,
  children,
  goToTop = true,
  header,
  heading,
  headingTag = 'h2',
  id = 'accordionSection',
  open = false,
  onToggle,
}: AccordionSectionProps) => {
  const { isMounted } = useMounted();

  const headingId = `${id}-heading`;
  const contentId = `${id}-content`;

  return (
    <div
      className={classNames(classes.section, styles.section, className, {
        [classes.expanded]: open,
        [styles.sectionExpanded]: open,
      })}
      role="presentation"
      id={id}
    >
      <div className="govuk-accordion__section-header">
        {header ??
          createElement(
            headingTag,
            {
              className: classes.sectionHeading,
            },
            isMounted ? (
              <>
                <button
                  aria-expanded={open}
                  className={classes.sectionButton}
                  id={headingId}
                  type="button"
                  onClick={() => {
                    if (onToggle) {
                      onToggle(!open, id);
                    }
                  }}
                >
                  {heading}
                  <span aria-hidden className="govuk-accordion__icon" />
                </button>
              </>
            ) : (
              <span id={headingId} className={classes.sectionButton}>
                {heading}
              </span>
            ),
          )}

        {caption && (
          <div className="govuk-accordion__section-summary govuk-body">
            {caption}
          </div>
        )}
      </div>
      <div
        className={classNames(classes.sectionContent, styles.sectionContent, {
          [styles.sectionContentExpanded]: open,
        })}
        aria-labelledby={headingId}
        id={contentId}
      >
        {typeof children === 'function'
          ? children({ open, contentId, headingId })
          : children}

        {goToTop && <GoToTopLink />}
      </div>
    </div>
  );
};

export default memo(AccordionSection);

export const openAllParentAccordionSections = (target: HTMLElement) => {
  const sections = findAllParents(target, `.${classes.section}`);

  sections.forEach(section => {
    const button = section.querySelector<HTMLElement>(
      `.${classes.sectionButton}`,
    );

    if (button && button.getAttribute('aria-expanded') === 'false') {
      button.click();
    }
  });
};
