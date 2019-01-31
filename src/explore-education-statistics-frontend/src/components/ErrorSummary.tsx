import GovUkErrorSummary from 'govuk-frontend/components/error-summary/error-summary';
import React, { Component, createRef } from 'react';

interface Props {
  errors: {
    id: string;
    message: string;
  }[];
  id: string;
  title?: string;
}

class ErrorSummary extends Component<Props> {
  public static defaultProps = {
    title: 'There was a problem',
  };

  private ref = createRef<HTMLDivElement>();

  public componentDidMount(): void {
    new GovUkErrorSummary(this.ref.current).init();
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
