import React from 'react';
import { TableData } from '@common/services/tableBuilderService';

export interface FootnoteProps {
  content: TableData['footnotes'];
}

const Footnote = ({ content }: FootnoteProps) => {
  const footnoteList = content.map(function createFootnotes(footnotes) {
    return (
      <p key={footnotes.id} className="govuk-body-s govuk-!-margin-bottom-1">
        {footnotes.label}
      </p>
    );
  });
  return <div className="govuk-!-margin-bottom-8">{footnoteList}</div>;
};

export default Footnote;
