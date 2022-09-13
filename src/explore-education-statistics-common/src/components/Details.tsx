import useMounted from '@common/hooks/useMounted';
import useToggle from '@common/hooks/useToggle';
import findAllParents from '@common/utils/dom/findAllParents';
import { formatTestId } from '@common/utils/test-utils';
import classNames from 'classnames';
import React, {
  MouseEvent,
  ReactNode,
  useEffect,
  useRef,
  useState,
} from 'react';
import styles from './Details.module.scss';
import VisuallyHidden from './VisuallyHidden';

let hasNativeDetails: boolean;
let idCounter = 0;

export type DetailsToggleHandler = (
  isOpen: boolean,
  event: MouseEvent<HTMLElement>,
) => void;

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
  summary: string;
  summaryAfter?: ReactNode;
  hiddenText?: string;
}

const Details = ({
  className,
  children,
  id: propId = `details-content-${(idCounter += 1)}`,
  detailsId,
  jsRequired = false,
  open = false,
  onToggle,
  summary,
  summaryAfter,
  hiddenText,
}: DetailsProps) => {
  const [id] = useState(propId);
  const ref = useRef<HTMLElement>(null);

  const { onMounted } = useMounted(undefined, jsRequired);
  const [isOpened, setOpened] = useToggle(open);

  useEffect(() => {
    if (typeof hasNativeDetails === 'undefined') {
      hasNativeDetails =
        typeof document.createElement('details').open === 'boolean';
    }
  }, []);

  useEffect(() => {
    setOpened(open);
  }, [open, setOpened]);

  useEffect(() => {
    if (hasNativeDetails) {
      return;
    }

    if (ref.current) {
      // Don't really need to include this, but just
      // polyfills DOM behaviour for IE/Edge
      if (isOpened) {
        ref.current.setAttribute('open', '');
      } else {
        ref.current.removeAttribute('open');
      }
    }
  }, [isOpened]);

  return (
    <details
      id={detailsId ? `${detailsId}-details-${id}` : `details-tag-${id}`}
      className={classNames('govuk-details', className)}
      open={open}
      ref={ref}
      role={onMounted('group')}
    >
      {/* eslint-disable-next-line jsx-a11y/click-events-have-key-events,jsx-a11y/no-static-element-interactions */}
      <summary
        aria-controls={onMounted(id)}
        aria-expanded={onMounted(isOpened)}
        className="govuk-details__summary"
        role={onMounted('button')}
        tabIndex={onMounted(0)}
        onClick={event => {
          event.persist();

          if (onToggle) {
            onToggle(!isOpened, event);

            if (event.isDefaultPrevented()) {
              return;
            }
          }

          setOpened(!isOpened);
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
      <div
        aria-hidden={onMounted(!isOpened)}
        className="govuk-details__text"
        id={onMounted(id)}
        style={onMounted(
          !hasNativeDetails
            ? {
                display: !isOpened ? 'none' : undefined,
              }
            : undefined,
        )}
      >
        {children}
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
