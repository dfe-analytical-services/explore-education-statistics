import React from 'react';

interface Props {
  column: string;
}

export default function ApiDataSetColumnName({ column }: Props) {
  return (
    <span className="dfe-colour--dark-grey govuk-!-display-block">
      Column: <code>{column}</code>
    </span>
  );
}
