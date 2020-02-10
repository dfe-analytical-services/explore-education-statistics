import CollapsibleList from '@common/components/CollapsibleList';
import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';
import React from 'react';

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
