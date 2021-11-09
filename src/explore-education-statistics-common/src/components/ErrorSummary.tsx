import React, {
  forwardRef,
  MouseEventHandler,
  ReactNode,
  useEffect,
  useRef,
} from 'react';
import ErrorPrefixPageTitle from './ErrorPrefixPageTitle';

export interface ErrorSummaryMessage {
  id: string;
  message: string;
}

interface BaseErrorSummaryProps {
  id: string;
  children: ReactNode;
  title: string;
}

export const BaseErrorSummary = forwardRef<
  HTMLDivElement,
  BaseErrorSummaryProps
>((props, ref) => {
  const { id, children, title } = props;
  const idTitle = `${id}-title`;

  return (
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

      <div className="govuk-error-summary__body">{children}</div>
    </div>
  );
});
BaseErrorSummary.displayName = 'BaseErrorSummary';

interface ErrorSummaryProps {
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
    <BaseErrorSummary id={id} title={title} ref={ref}>
      <ul className="govuk-list govuk-error-summary__list">
        {errors.map(error => (
          <li key={error.id}>
            <a href={`#${error.id}`} onClick={onErrorClick}>
              {error.message}
            </a>
          </li>
        ))}
      </ul>
    </BaseErrorSummary>
  ) : null;
};

export default ErrorSummary;
