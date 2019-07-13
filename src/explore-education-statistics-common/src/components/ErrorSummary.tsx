import React, { useEffect, useRef, useState } from 'react';

export interface ErrorSummaryMessage {
  id: string;
  message: string;
}

interface Props {
  errors: ErrorSummaryMessage[];
  id: string;
  focusOnError?: boolean;
  title?: string;
}

const ErrorSummary = ({
  id,
  errors,
  focusOnError = false,
  title = 'There is a problem',
}: Props) => {
  const ref = useRef<HTMLDivElement>(null);

  // Only
  const [prevErrors, setPrevErrors] = useState(0);

  useEffect(() => {
    import('govuk-frontend/components/error-summary/error-summary').then(
      ({ default: GovUkErrorSummary }) => {
        new GovUkErrorSummary(ref.current).init();
      },
    );
  }, [ref]);

  useEffect(() => {
    if (errors.length > 0 && !prevErrors && ref.current) {
      if (focusOnError) {
        ref.current.scrollIntoView({
          behavior: 'smooth',
          block: 'start',
        });

        ref.current.focus();
      }
    }

    setPrevErrors(errors.length);
  }, [errors, prevErrors, focusOnError]);

  const idTitle = `${id}-title`;

  return errors.length > 0 ? (
    <div
      aria-labelledby={idTitle}
      className="govuk-error-summary"
      ref={ref}
      role="alert"
      tabIndex={-1}
    >
      <h2 className="govuk-error-summary__title" id={idTitle}>
        {title}
      </h2>

      <div className="govuk-error-summary__body">
        <ul className="govuk-list govuk-error-summary__list">
          {errors.map(error => (
            <li key={error.id}>
              <a href={`#${error.id}`}>{error.message}</a>
            </li>
          ))}
        </ul>
      </div>
    </div>
  ) : null;
};

export default ErrorSummary;
