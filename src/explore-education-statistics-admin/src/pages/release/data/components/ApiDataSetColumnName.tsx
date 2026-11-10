import classNames from 'classnames';
import React from 'react';

interface Props {
  block?: boolean;
  column: string;
}

export default function ApiDataSetColumnName({ block = true, column }: Props) {
  return (
    <span
      className={classNames('dfe-colour--dark-grey', {
        'govuk-!-display-block': block,
      })}
    >
      Column: <code>{column}</code>
    </span>
  );
}
