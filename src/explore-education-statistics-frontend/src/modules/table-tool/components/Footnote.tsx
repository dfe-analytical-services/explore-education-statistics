import React from 'react';
import { TableData } from '@common/services/tableBuilderService';

export interface FootnoteProps {
  content: TableData['footnotes'];
}

const Footnote = ({ content }: FootnoteProps) => {
  const footnoteList = content.map(function createFootnotes(footnotes) {
    return <li key={footnotes.id}>{footnotes.label}</li>;
  });
  return (
    <ol className="govuk-list govuk-list--number govuk-!-margin-bottom-8">
      {footnoteList}
    </ol>
  );
};

export default Footnote;
