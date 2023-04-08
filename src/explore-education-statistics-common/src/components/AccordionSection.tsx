import CopyLinkButton from '@common/components/CopyLinkButton';
import useMounted from '@common/hooks/useMounted';
import findAllParents from '@common/utils/dom/findAllParents';
import classNames from 'classnames';
import kebabCase from 'lodash/kebabCase';
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
  anchorLinkUrl?: (id: string) => string;
  goToTop?: boolean;
  header?: ReactNode;
  heading: string;
  /**
   * Only for accessibility/semantic markup,
   * does not change the actual styling
   */
  headingTag?: 'h2' | 'h3' | 'h4';
  id?: string;
  anchorLinkIdPrefix?: string;
  open?: boolean;
  // eslint-disable-next-line react/no-unused-prop-types
  testId?: string;
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
  anchorLinkUrl,
  caption,
  className,
  children,
  goToTop = true,
  header,
  heading,
  headingTag = 'h2',
  id = 'accordionSection',
  anchorLinkIdPrefix = 'section',
  open = false,
  onToggle,
}: AccordionSectionProps) => {
  const { isMounted, onMounted } = useMounted();

  const headingId = `${id}-heading`;
  const contentId = `${id}-content`;
  const anchorId = `${anchorLinkIdPrefix}-${kebabCase(heading)}`;

  return (
    <div
      className={classNames(classes.section, styles.section, className, {
        [classes.expanded]: open,
      })}
      data-testid="accordionSection"
      id={id}
    >
      <div className="govuk-accordion__section-header">
        {anchorLinkUrl && (
          <CopyLinkButton
            className={styles.copyLinkButton}
            url={anchorLinkUrl(anchorId)}
          />
        )}
        {header ??
          createElement(
            headingTag,
            {
              className: classes.sectionHeading,
              id: anchorId,
            },
            isMounted ? (
              <button
                aria-controls={contentId}
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
        aria-labelledby={headingId}
        className={classNames(
          classes.sectionContent,
          onMounted({ [styles.sectionContentCollapsed]: !open }),
        )}
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
