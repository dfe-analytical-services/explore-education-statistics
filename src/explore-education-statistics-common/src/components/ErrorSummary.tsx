import React, { useEffect, useRef, useState } from 'react';

export interface ErrorSummaryMessage {
  id: string;
  message: string;
}

interface Props {
  errors: ErrorSummaryMessage[];
  id: string;
  title?: string;
}

const ErrorSummary = ({ id, errors, title = 'There is a problem' }: Props) => {
  const ref = useRef<HTMLDivElement>(null);

  const [prevErrors, setPrevErrors] = useState<ErrorSummaryMessage[]>([]);

  useEffect(() => {
    import('govuk-frontend/components/error-summary/error-summary').then(
      ({ default: GovUkErrorSummary }) => {
        new GovUkErrorSummary(ref.current).init();
      },
    );
  }, [ref]);

  useEffect(() => {
    if (errors.length > 0 && !prevErrors.length && ref.current) {
      ref.current.scrollIntoView({
        behavior: 'smooth',
        block: 'start',
      });
      ref.current.focus();
    }

    setPrevErrors(errors);
  }, [errors, prevErrors]);

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
