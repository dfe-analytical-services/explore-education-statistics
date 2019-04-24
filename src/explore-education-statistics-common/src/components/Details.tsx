import classNames from 'classnames';
import React, { Component, createRef, ReactNode } from 'react';

export interface DetailsProps {
  className?: string;
  children: ReactNode;
  id?: string;
  onToggle?: (isOpen: boolean) => void;
  open?: boolean;
  summary: string | ReactNode;
}

class Details extends Component<DetailsProps> {
  private ref = createRef<HTMLElement>();

  public componentDidMount(): void {
    if (this.ref.current) {
      import('govuk-frontend/components/details/details').then(
        ({ default: GovUkDetails }) => {
          new GovUkDetails(this.ref.current).init();
        },
      );
    }
  }

  public render() {
    const { className, children, id, open, onToggle, summary } = this.props;

    return (
      <details
        className={classNames('govuk-details', className)}
        open={open}
        ref={this.ref}
        data-testid={summary}
      >
        {/* eslint-disable-next-line jsx-a11y/click-events-have-key-events */}
        <summary
          className="govuk-details__summary"
          role="button"
          tabIndex={0}
          onClick={event => {
            if (onToggle) {
              onToggle(
                event.currentTarget.getAttribute('aria-expanded') === 'true',
              );
            }
          }}
        >
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
