import React from 'react';
import { FullTableMeta } from '@common/modules/full-table/types/fullTable';
import CollapsibleList from '@common/components/CollapsibleList';

export interface FootnoteProps {
  content: FullTableMeta['footnotes'];
}

const Footnote = ({ content }: FootnoteProps) => {
  const footnoteList = content.map(function createFootnotes(footnotes) {
    return <li key={footnotes.id}>{footnotes.label}</li>;
  });

  return (
    <CollapsibleList ordered listStyle="number">
      {footnoteList}
    </CollapsibleList>
  );
};

export default Footnote;
