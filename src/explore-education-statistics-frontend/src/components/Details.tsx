import React, { ReactNode } from 'react';

interface Props {
  children: ReactNode;
  open?: boolean;
  summary: string;
}

const Details = ({ children, summary, open }: Props) => {
  return (
    <details className="govuk-details" open={open}>
      <summary className="govuk-details__summary">
        <span
          className="govuk-details__summary-text"
          data-testid="details--expand"
        >
          {summary}
        </span>
      </summary>
      <div className="govuk-details__text">{children}</div>
    </details>
  );
};

export default Details;
