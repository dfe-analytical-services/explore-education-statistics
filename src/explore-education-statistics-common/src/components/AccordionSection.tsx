import CopyLinkModal from '@common/components/CopyLinkModal';
import useMounted from '@common/hooks/useMounted';
import findAllParents from '@common/utils/dom/findAllParents';
import classNames from 'classnames';
import kebabCase from 'lodash/kebabCase';
import React, { createElement, memo, ReactNode } from 'react';
import styles from './AccordionSection.module.scss';
import GoToTopLink from './GoToTopLink';

export type ToggleHandler = (open: boolean, id: string) => void;

export interface AccordionSectionProps {
  anchorLinkIdPrefix?: string;
  anchorLinkUrl?: (id: string) => string;
  caption?: ReactNode | string;
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
  trackScroll?: boolean;
  onToggle?: ToggleHandler;
}

const classes = {
  section: 'govuk-accordion__section',
  sectionButton: 'govuk-accordion__section-button',
  sectionContent: 'govuk-accordion__section-content',
  sectionHeading: 'govuk-accordion__section-heading',
  sectionHeadingText: 'govuk-accordion__section-heading-text',
  expanded: 'govuk-accordion__section--expanded',
};

export const accordionSectionClasses = classes;

const AccordionSection = ({
  anchorLinkIdPrefix = 'section',
  anchorLinkUrl,
  caption,
  className,
  children,
  goToTop = true,
  header,
  heading,
  headingTag = 'h2',
  id = 'accordionSection',
  open = false,
  trackScroll = false,
  onToggle,
}: AccordionSectionProps) => {
  const { isMounted } = useMounted();

  const headingId = `${id}-heading`;
  const contentId = `${id}-content`;
  const anchorId = `${anchorLinkIdPrefix}-${kebabCase(heading)}`;

  return (
    <div
      className={classNames(classes.section, styles.section, className, {
        [classes.expanded]: open,
        [styles.hasAnchorLink]: anchorLinkUrl,
      })}
      data-testid="accordionSection"
      id={id}
    >
      <div className="govuk-accordion__section-header">
        {anchorLinkUrl && (
          <CopyLinkModal
            buttonClassName={styles.copyLinkButton}
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
                aria-label={`${heading}, ${
                  open ? 'hide' : 'show'
                } this section`}
                className={classes.sectionButton}
                data-scroll={trackScroll ? true : undefined}
                id={headingId}
                type="button"
                onClick={() => {
                  onToggle?.(!open, id);
                }}
              >
                <HeadingContent caption={caption} heading={heading} />
                <span className="govuk-visually-hidden govuk-accordion__section-heading-divider">
                  ,{' '}
                </span>
                <span className="govuk-accordion__section-toggle">
                  <span className="govuk-accordion__section-toggle-focus">
                    <span
                      className={classNames('govuk-accordion-nav__chevron', {
                        'govuk-accordion-nav__chevron--down': !open,
                      })}
                    />
                    <span className="govuk-accordion__section-toggle-text">
                      {open ? 'Hide' : 'Show'}
                    </span>
                  </span>
                </span>
              </button>
            ) : (
              <HeadingContent caption={caption} heading={heading} />
            ),
          )}
      </div>

      <div className={classNames(classes.sectionContent)} id={contentId}>
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

function HeadingContent({
  caption,
  heading,
}: {
  caption?: ReactNode | string;
  heading: string;
}) {
  return (
    <>
      <span className={classes.sectionHeadingText}>
        <span
          data-testid="accordionSection-heading"
          className="govuk-accordion__section-heading-text-focus"
        >
          {heading}
        </span>
      </span>
      {caption && (
        <span className="govuk-accordion__section-summary govuk-body">
          <span className="govuk-accordion__section-summary-focus">
            {caption}
          </span>
        </span>
      )}
    </>
  );
}
