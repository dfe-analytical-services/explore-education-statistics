import React from 'react';
import { TableData } from '@common/services/tableBuilderService';

// eslint-disable-next-line @typescript-eslint/prefer-interface
export type FootnoteProps = {
  content: TableData['footnotes'];
};

const Footnote = ({ content }: FootnoteProps) => {
  const footnoteList = content.map(function createFootnotes(footnotes) {
    return (
      <p className="govuk-body-s govuk-!-margin-bottom-1">
        {footnotes.indicators} + ' ' +{footnotes.value}
      </p>
    );
  });
  return <div className="govuk-!-margin-bottom-8">{footnoteList}</div>;
};

export default Footnote;
