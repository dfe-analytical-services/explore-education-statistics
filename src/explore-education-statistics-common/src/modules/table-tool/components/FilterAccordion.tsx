import { ToggleHandler } from '@common/components/AccordionSection';
import VisuallyHidden from '@common/components/VisuallyHidden';
import useToggle from '@common/hooks/useToggle';
import styles from '@common/modules/table-tool/components/FilterAccordion.module.scss';
import classNames from 'classnames';
import React, { ReactNode, useEffect } from 'react';

interface Props {
  children: ReactNode;
  label: string;
  labelHiddenText?: string;
  labelAfter?: ReactNode;
  id: string;
  open?: boolean;
  preventToggle?: boolean;
  testId?: string;
  onToggle?: ToggleHandler;
}

export default function FilterAccordion({
  children,
  label,
  labelAfter,
  labelHiddenText,
  id,
  open: initialOpen = false,
  preventToggle = false,
  testId = 'filter-accordion',
  onToggle,
}: Props) {
  const contentId = `${id}-content`;

  const [open, toggleOpen] = useToggle(initialOpen);

  useEffect(() => {
    toggleOpen(initialOpen);
  }, [initialOpen, toggleOpen]);

  return (
    <div
      className={classNames('govuk-!-margin-bottom-2', {
        [styles.expanded]: open,
      })}
      data-testid={testId}
      id={id}
    >
      <div className={styles.inner}>
        <button
          aria-controls={contentId}
          aria-expanded={open}
          className="govuk-accordion__show-all govuk-!-margin-bottom-0"
          type="button"
          onClick={() => {
            if (!preventToggle) {
              toggleOpen();
              onToggle?.(!open, id);
            }
          }}
        >
          <span
            className={classNames('govuk-accordion-nav__chevron', {
              'govuk-accordion-nav__chevron--down': !open,
            })}
          />
          <span className="govuk-accordion__show-all-text">
            {label}
            <VisuallyHidden>
              {labelHiddenText ??
                ` - ${open ? 'hide options' : 'show options'}`}
            </VisuallyHidden>
          </span>
        </button>

        {labelAfter}
      </div>

      <div className={styles.sectionContent} id={contentId}>
        {children}
      </div>
    </div>
  );
}
