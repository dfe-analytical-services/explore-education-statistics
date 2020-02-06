import React from 'react';
import CollapsibleList from '@common/components/CollapsibleList';
import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';
import Details from '@common/components/Details';

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
