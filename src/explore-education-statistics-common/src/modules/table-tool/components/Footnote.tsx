import React from 'react';
import { FullTableMeta } from '@common/modules/full-table/types/fullTable';
import CollapsibleList from '@common/components/CollapsibleList';

export interface FootnoteProps {
  content: FullTableMeta['footnotes'];
}

const Footnote = ({ content }: FootnoteProps) => {
  return (
    <CollapsibleList listStyle="number">
      {content.map(footnotes => {
        return <li key={footnotes.id}>{footnotes.label}</li>;
      })}
    </CollapsibleList>
  );
};

export default Footnote;
