import ErrorPrefixPageTitle from '@common/components/ErrorPrefixPageTitle';
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

        <ErrorPrefixPageTitle />

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
    import('govuk-frontend/govuk/components/error-summary/error-summary').then(
      ({ default: GovUkErrorSummary }) => {
        if (ref.current) {
          const { handleClick } = new GovUkErrorSummary(ref.current);
          ref.current.addEventListener('click', handleClick);
        }
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

  return errors.length > 0 ? (
    <BaseErrorSummary ref={ref} title={title} visuallyHidden={visuallyHidden}>
      <ul className="govuk-list govuk-error-summary__list">
        {errors.map(error => (
          <li key={error.id}>
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
