import { TableHighlight } from '@common/services/tableBuilderService';
import React, { ReactNode } from 'react';

interface Props {
  highlights: TableHighlight[];
  renderLink: (highlight: TableHighlight) => ReactNode;
}

const TableHighlightsList = ({ highlights = [], renderLink }: Props) => {
  return (
    <ul className="govuk-!-margin-bottom-6">
      {highlights.map(highlight => (
        <li key={highlight.id}>
          <p className="govuk-!-font-weight-bold govuk-!-margin-bottom-1">
            {renderLink(highlight)}
          </p>
          <p>{highlight.description}</p>
        </li>
      ))}
    </ul>
  );
};

export default TableHighlightsList;
