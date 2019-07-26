import useMounted from '@common/hooks/useMounted';
import useToggle from '@common/hooks/useToggle';
import findAllParents from '@common/lib/dom/findAllParents';
import classNames from 'classnames';
import React, { MouseEvent, ReactNode, useEffect, useRef } from 'react';

let hasNativeDetails: boolean;
let idCounter = 0;

export interface DetailsProps {
  className?: string;
  tag?: string;
  children: ReactNode;
  id?: string;
  onToggle?: (isOpen: boolean, event: MouseEvent<HTMLElement>) => void;
  analytics?: (eventGA: { category: string; action: string }) => void;
  open?: boolean;
  summary: string | ReactNode;
}

const Details = ({
  className,
  children,
  id = `details-content-${(idCounter += 1)}`,
  open = false,
  onToggle,
  analytics,
  summary,
  tag,
}: DetailsProps) => {
  const ref = useRef<HTMLElement>(null);

  const { onMounted } = useMounted();
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
            if (analytics) {
              analytics({
                category: 'Statistics summary',
                action: `${summary} - ${tag}`,
              });
            }

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
          className="govuk-details__summary-text"
          data-testid="details--expand"
        >
          {summary}
          {tag && (
            <span className="govuk-tag govuk-!-margin-left-2">{tag}</span>
          )}
        </span>
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
