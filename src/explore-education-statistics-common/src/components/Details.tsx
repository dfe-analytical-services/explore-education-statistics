import classNames from 'classnames';
import DetailsModule from 'govuk-frontend/components/details/details';
import React, {
  MouseEvent,
  MutableRefObject,
  ReactNode,
  useEffect,
  useRef,
} from 'react';

export interface DetailsProps {
  className?: string;
  children: ReactNode;
  id?: string;
  onToggle?: (isOpen: boolean, event: MouseEvent<HTMLElement>) => void;
  open?: boolean;
  summary: string | ReactNode;
}

const Details = ({
  className,
  children,
  id,
  open = false,
  onToggle,
  summary,
}: DetailsProps) => {
  const ref = useRef<HTMLElement>(null);
  const module: MutableRefObject<DetailsModule | null> = useRef(null);

  useEffect(() => {
    if (ref.current) {
      import('govuk-frontend/components/details/details').then(
        ({ default: GovUkDetails }) => {
          module.current = new GovUkDetails(ref.current);
          module.current.init();
        },
      );
    }
  }, []);

  useEffect(() => {
    if (module.current) {
      module.current.setAttributes();
    }
  }, [open]);

  return (
    <details
      className={classNames('govuk-details', className)}
      open={open}
      ref={ref}
      data-testid={summary}
    >
      {/* eslint-disable-next-line jsx-a11y/click-events-have-key-events */}
      <summary
        className="govuk-details__summary"
        role="button"
        tabIndex={0}
        onClick={event => {
          event.persist();

          if (onToggle) {
            onToggle(
              event.currentTarget.getAttribute('aria-expanded') === 'true',
              event,
            );
          }
        }}
      >
        <span
          className="govuk-details__summary-text"
          data-testid="details--expand"
        >
          {summary}
        </span>
      </summary>
      <div className="govuk-details__text" id={id}>
        {children}
      </div>
    </details>
  );
};

export default Details;
