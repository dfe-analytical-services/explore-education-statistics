import React, { ReactNode } from 'react';

interface Props {
  children: ReactNode;
  summary: string;
}

const Details = ({ children, summary }: Props) => {
  return (
    <details className="govuk-details">
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
