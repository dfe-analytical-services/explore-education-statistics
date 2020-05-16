import React, { MouseEventHandler, useEffect, useRef } from 'react';
import ErrorPrefixPageTitle from './ErrorPrefixPageTitle';

export interface ErrorSummaryMessage {
  id: string;
  message: string;
}

interface Props {
  errors: ErrorSummaryMessage[];
  id: string;
  focusOnError?: boolean;
  title?: string;
  onFocus?: () => void;
  onErrorClick?: MouseEventHandler<HTMLAnchorElement>;
}

const ErrorSummary = ({
  id,
  errors,
  focusOnError = false,
  title = 'There is a problem',
  onFocus,
  onErrorClick,
}: Props) => {
  const ref = useRef<HTMLDivElement>(null);

  useEffect(() => {
    import('govuk-frontend/govuk/components/error-summary/error-summary').then(
      ({ default: GovUkErrorSummary }) => {
        new GovUkErrorSummary(ref.current).init();
      },
    );
  }, [ref]);

  useEffect(() => {
    if (focusOnError && errors.length > 0 && ref.current) {
      ref.current.scrollIntoView({
        behavior: 'smooth',
        block: 'start',
      });

      ref.current.focus();

      if (onFocus) {
        onFocus();
      }
    }
  }, [errors, focusOnError, onFocus]);

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

      <ErrorPrefixPageTitle />

      <div className="govuk-error-summary__body">
        <ul className="govuk-list govuk-error-summary__list">
          {errors.map(error => (
            <li key={error.id}>
              <a href={`#${error.id}`} onClick={onErrorClick}>
                {error.message}
              </a>
            </li>
          ))}
        </ul>
      </div>
    </div>
  ) : null;
};

export default ErrorSummary;
