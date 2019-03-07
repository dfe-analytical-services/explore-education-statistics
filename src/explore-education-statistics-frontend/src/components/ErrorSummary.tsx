import React, { Component, createRef } from 'react';

export interface ErrorSummaryMessage {
  id: string;
  message: string;
}

interface Props {
  errors: ErrorSummaryMessage[];
  id: string;
  title?: string;
}

class ErrorSummary extends Component<Props> {
  public static defaultProps = {
    title: 'There is a problem',
  };

  private ref = createRef<HTMLDivElement>();

  public componentDidMount(): void {
    if (this.ref.current) {
      import('govuk-frontend/components/error-summary/error-summary')
        .then(({ default: GovUkErrorSummary }) => {
          new GovUkErrorSummary(this.ref.current).init()
        });
    }
  }

  public render() {
    const { id, errors, title } = this.props;

    const idTitle = `${id}-title`;

    return (
      errors.length > 0 && (
        <div
          aria-labelledby={idTitle}
          className="govuk-error-summary"
          ref={this.ref}
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
      )
    );
  }
}

export default ErrorSummary;
