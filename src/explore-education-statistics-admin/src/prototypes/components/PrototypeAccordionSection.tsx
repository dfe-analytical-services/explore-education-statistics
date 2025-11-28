import CopyLinkModal from '@common/components/CopyLinkModal';
import useMounted from '@common/hooks/useMounted';
import findAllParents from '@common/utils/dom/findAllParents';
import classNames from 'classnames';
import React, { createElement, memo, ReactNode } from 'react';
import styles from '@common/components/AccordionSection.module.scss';
import stylesProt from '@admin/prototypes/PrototypePublicPage.module.scss';
import BackToTopLink from '@common/components/BackToTopLink';

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
  anchorLinkUrl?: string;
  backToTop?: boolean;
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
  anchorLinkUrl,
  caption,
  className,
  children,
  backToTop = true,
  header,
  heading,
  headingTag = 'h2',
  id = 'accordionSection',
  open = false,
  onToggle,
}: AccordionSectionProps) => {
  const { isMounted, onMounted } = useMounted();

  const headingId = `${id}-heading`;
  const contentId = `${id}-content`;

  return (
    <div
      className={classNames(
        classes.section,
        styles.section,
        stylesProt.accordionSection,
        className,
        {
          [classes.expanded]: open,
        },
      )}
      data-testid="accordionSection"
      id={id}
    >
      <div className="govuk-accordion__section-header">
        {anchorLinkUrl && (
          <CopyLinkModal
            buttonClassName={styles.copyLinkButton}
            url={anchorLinkUrl}
          />
        )}
        {header ??
          createElement(
            headingTag,
            {
              className: classes.sectionHeading,
              id: heading.toLowerCase().split(' ').join('-'),
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
                <span style={{ color: 'black' }}>{heading}</span>
                <div className={stylesProt.accordionShow}>
                  <span
                    className={classNames(
                      stylesProt.accordionChevron,
                      open && stylesProt.accordionChevronDown,
                    )}
                  >
                    {' '}
                  </span>
                  {open ? 'Hide' : 'Show'}
                </div>
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

        {backToTop && <BackToTopLink />}
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
