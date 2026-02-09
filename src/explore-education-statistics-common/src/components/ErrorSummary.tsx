import classNames from 'classnames';
import React, {
  createElement,
  MouseEventHandler,
  ReactNode,
  Ref,
  useEffect,
  useRef,
} from 'react';

export interface ErrorSummaryMessage {
  id: string;
  message: string;
}

interface BaseErrorSummaryProps {
  children: ReactNode;
  headingTag?: 'h2' | 'h3' | 'h4';
  ref?: Ref<HTMLDivElement>;
  testId?: string;
  title: string;
  updateDocumentTitle?: boolean;
  visuallyHidden?: boolean;
}

export const BaseErrorSummary = (props: BaseErrorSummaryProps) => {
  const {
    children,
    headingTag = 'h2',
    ref,
    testId = 'errorSummary',
    title,
    updateDocumentTitle = true,
    visuallyHidden,
  } = props;

  useEffect(() => {
    if (updateDocumentTitle) {
      document.title = `ERROR: ${document.title.replace(/ERROR: /g, '')}`;
    }
    return () => {
      document.title = document.title.replace(/ERROR: /g, '');
    };
  }, [updateDocumentTitle]);

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
        {createElement(
          headingTag,
          { className: 'govuk-error-summary__title' },
          title,
        )}
        <div className="govuk-error-summary__body">{children}</div>
      </div>
    </div>
  );
};

BaseErrorSummary.displayName = 'BaseErrorSummary';

interface ErrorSummaryProps {
  errors: ErrorSummaryMessage[];
  headingTag?: 'h2' | 'h3' | 'h4';
  focusOnError?: boolean;
  title?: string;
  updateDocumentTitle?: boolean;
  visuallyHidden?: boolean;
  onFocus?: () => void;
  onErrorClick?: MouseEventHandler<HTMLAnchorElement>;
}

const ErrorSummary = ({
  errors,
  focusOnError = false,
  headingTag = 'h2',
  title = 'There is a problem',
  updateDocumentTitle,
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
    <BaseErrorSummary
      ref={ref}
      headingTag={headingTag}
      title={title}
      updateDocumentTitle={updateDocumentTitle}
      visuallyHidden={visuallyHidden}
    >
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
