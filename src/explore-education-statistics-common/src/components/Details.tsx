import useMounted from '@common/hooks/useMounted';
import useToggle from '@common/hooks/useToggle';
import findAllParents from '@common/utils/dom/findAllParents';
import { formatTestId } from '@common/utils/test-utils';
import classNames from 'classnames';
import React, { ReactNode, useEffect, useId, useRef, useState } from 'react';
import styles from './Details.module.scss';
import ButtonText from './ButtonText';
import VisuallyHidden from './VisuallyHidden';

export type DetailsToggleHandler = (isOpen: boolean) => void;

export interface DetailsProps {
  className?: string;
  children: ReactNode;
  id?: string;
  detailsId?: string;
  /**
   * When `jsRequired` is true, we assume that the browser has
   * JS and will skip waiting for the component to mount.
   * This means we render progressive enhancements automatically.
   *
   * This is a performance optimization and should only be used in
   * situations where JS is mandatory anyway (e.g. table tool).
   *
   * Setting this to true will make Details **unusable**
   * for anyone with JS disabled.
   */
  jsRequired?: boolean;
  onToggle?: DetailsToggleHandler;
  open?: boolean;
  showCloseButton?: boolean;
  summary: string;
  summaryAfter?: ReactNode;
  hiddenText?: string;
  preventToggle?: boolean;
  testId?: string;
}

const Details = ({
  className,
  children,
  id: propId,
  detailsId,
  jsRequired = false,
  open: defaultOpen = false,
  onToggle,
  showCloseButton = false,
  summary,
  summaryAfter,
  hiddenText,
  preventToggle = false,
  testId,
}: DetailsProps) => {
  const generatedId = useId();
  const [id] = useState(propId ?? `details-content-${generatedId}`);
  const ref = useRef<HTMLDetailsElement>(null);

  const { onMounted } = useMounted(undefined, jsRequired);
  const [open, toggleOpen] = useToggle(defaultOpen);

  useEffect(() => {
    if (preventToggle) {
      toggleOpen(true);
      return;
    }
    toggleOpen(defaultOpen);
  }, [defaultOpen, preventToggle, toggleOpen]);

  return (
    <details
      id={detailsId ? `${detailsId}-details-${id}` : `details-tag-${id}`}
      className={classNames('govuk-details', className)}
      open={open}
      ref={ref}
      data-testid={testId}
    >
      <summary
        aria-controls={onMounted(id)}
        aria-expanded={onMounted(open)}
        className="govuk-details__summary"
        role={onMounted('button')}
        onClick={event => {
          // preventDefault here to allow the open state to be controlled,
          // see https://github.com/facebook/react/issues/15486
          event.preventDefault();
          if (!preventToggle) {
            onToggle?.(!open);
            toggleOpen();
          }
        }}
        onKeyPress={event => {
          if (event.key === 'Enter' || event.key === ' ') {
            event.preventDefault();
            (event.target as HTMLElement).click();
          }
        }}
        onKeyUp={event => {
          if (event.key === ' ') {
            event.preventDefault();
          }
        }}
      >
        <span
          id={
            detailsId ? `${detailsId}-heading-${id}` : `details-heading-${id}`
          }
          className={classNames(
            styles.summaryText,
            'govuk-details__summary-text',
          )}
          data-testid={formatTestId(`Expand Details Section ${summary}`)}
        >
          {summary}
          {hiddenText && <VisuallyHidden>{` ${hiddenText}`}</VisuallyHidden>}
        </span>
        {summaryAfter}
      </summary>
      <div className="govuk-details__text" id={onMounted(id)}>
        {children}

        {showCloseButton && (
          <ButtonText
            onClick={ev => {
              ev.preventDefault();
              toggleOpen();
            }}
          >
            Close
          </ButtonText>
        )}
      </div>
    </details>
  );
};

export default Details;

export const openAllParentDetails = (target: HTMLElement) => {
  const textContainers = findAllParents(target, '.govuk-details__text');

  textContainers.forEach(textContainer => {
    const summary = document.querySelector<HTMLElement>(
      `summary[aria-controls="${textContainer.id}"]`,
    );

    if (summary && summary.getAttribute('aria-expanded') === 'false') {
      summary.click();
    }
  });
};
