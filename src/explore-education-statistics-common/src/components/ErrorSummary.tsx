import classNames from 'classnames';
import React, {
  forwardRef,
  MouseEventHandler,
  ReactNode,
  useEffect,
  useRef,
} from 'react';

export interface ErrorSummaryMessage {
  id: string;
  message: string;
}

interface BaseErrorSummaryProps {
  children: ReactNode;
  testId?: string;
  title: string;
  visuallyHidden?: boolean;
}

export const BaseErrorSummary = forwardRef<
  HTMLDivElement,
  BaseErrorSummaryProps
>((props, ref) => {
  const { children, testId = 'errorSummary', title, visuallyHidden } = props;

  useEffect(() => {
    document.title = `ERROR: ${document.title.replace(/ERROR: /g, '')}`;

    return () => {
      document.title = document.title.replace(/ERROR: /g, '');
    };
  }, []);

  return (
    <div
      className={classNames('govuk-error-summary', {
        'govuk-visually-hidden': visuallyHidden,
      })}
      ref={ref}
      tabIndex={-1}
      data-testid={testId}
    >
      <div role="alert">
        <h2 className="govuk-error-summary__title">{title}</h2>
        <div className="govuk-error-summary__body">{children}</div>
      </div>
    </div>
  );
});
BaseErrorSummary.displayName = 'BaseErrorSummary';

interface ErrorSummaryProps {
  errors: ErrorSummaryMessage[];
  focusOnError?: boolean;
  title?: string;
  visuallyHidden?: boolean;
  onFocus?: () => void;
  onErrorClick?: MouseEventHandler<HTMLAnchorElement>;
}

const ErrorSummary = ({
  errors,
  focusOnError = false,
  title = 'There is a problem',
  visuallyHidden = false,
  onFocus,
  onErrorClick,
}: ErrorSummaryProps) => {
  const ref = useRef<HTMLDivElement>(null);

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

  return errors.length > 0 ? (
    <BaseErrorSummary ref={ref} title={title} visuallyHidden={visuallyHidden}>
      <ul className="govuk-list govuk-error-summary__list">
        {errors.map((error, index) => (
          // eslint-disable-next-line react/no-array-index-key
          <li key={index}>
            {!visuallyHidden ? (
              <a href={`#${error.id}`} onClick={onErrorClick}>
                {error.message}
              </a>
            ) : (
              `${error.message}`
            )}
          </li>
        ))}
      </ul>
    </BaseErrorSummary>
  ) : null;
};

export default ErrorSummary;
