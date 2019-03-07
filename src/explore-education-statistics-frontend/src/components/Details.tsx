import React, { Component, createRef, ReactNode } from 'react';

interface Props {
  children: ReactNode;
  id?: string;
  open?: boolean;
  summary: string;
}

class Details extends Component<Props> {
  private ref = createRef<HTMLElement>();

  public componentDidMount(): void {
    if (this.ref.current) {
      import('govuk-frontend/components/details/details')
        .then(({ default: GovUkDetails }) => {
          new GovUkDetails(this.ref.current).init()
        });
    }
  }

  public render() {
    const { children, id, open, summary } = this.props;

    return (
      <details className="govuk-details" open={open} ref={this.ref}>
        <summary className="govuk-details__summary">
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
  }
}

export default Details;
