import { ToggleHandler } from '@common/components/AccordionSection';
import VisuallyHidden from '@common/components/VisuallyHidden';
import useMounted from '@common/hooks/useMounted';
import useToggle from '@common/hooks/useToggle';
import styles from '@frontend/modules/search-data/components/ExpandableFilterGroup.module.scss';
import classNames from 'classnames';
import React, { ReactNode, useEffect } from 'react';

interface Props {
  children: ReactNode;
  label: string;
  labelHiddenText?: string;
  labelAfter?: ReactNode;
  labelSub?: string;
  id: string;
  open?: boolean;
  testId?: string;
  onToggle?: ToggleHandler;
}

export default function ExpandableFilterGroup({
  children,
  label,
  labelAfter,
  labelHiddenText,
  labelSub,
  id,
  open: initialOpen = false,
  testId = 'expandable-filter-group',
  onToggle,
}: Props) {
  const contentId = `${id}-content`;

  const [open, toggleOpen] = useToggle(initialOpen);
  const { isMounted } = useMounted();

  useEffect(() => {
    toggleOpen(initialOpen);
  }, [initialOpen, toggleOpen]);

  return (
    <div
      className={classNames('govuk-!-margin-bottom-1 dfe-border-bottom', {
        [styles.expanded]: open,
      })}
      data-testid={testId}
      id={id}
    >
      <div className={styles.inner}>
        {isMounted ? (
          <button
            aria-controls={contentId}
            aria-expanded={open}
            className={classNames(
              'govuk-accordion__show-all govuk-!-margin-bottom-0',
              styles.toggleButton,
            )}
            data-testid="expandable-filter-group-button"
            type="button"
            onClick={() => {
              toggleOpen();
              onToggle?.(!open, id);
            }}
          >
            <span
              className={classNames('govuk-accordion-nav__chevron', {
                'govuk-accordion-nav__chevron--down': !open,
              })}
            />
            <span className="govuk-accordion__show-all-text">
              {label}
              {labelSub && (
                <span className="govuk-!-font-size-16 govuk-!-display-block">
                  {labelSub}
                </span>
              )}
              <VisuallyHidden>
                {labelHiddenText ??
                  ` - ${open ? 'hide options' : 'show options'}`}
              </VisuallyHidden>
            </span>
          </button>
        ) : (
          // The toggle can't work without JS, where the group is always
          // shown open, so render a plain label until mounted.
          <p className="govuk-heading-s govuk-!-margin-bottom-0">
            {label}
            {labelSub && (
              <span className="govuk-!-font-size-16 govuk-!-font-weight-regular govuk-!-display-block">
                {labelSub}
              </span>
            )}
          </p>
        )}

        {labelAfter}
      </div>

      <div className={styles.sectionContent} id={contentId}>
        {children}
      </div>
    </div>
  );
}
