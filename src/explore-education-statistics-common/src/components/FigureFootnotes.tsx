import CollapsibleList from '@common/components/CollapsibleList';
import React from 'react';

interface Props {
  footnotes: {
    id: string;
    label: string;
  }[];
}

const FigureFootnotes = ({ footnotes }: Props) => {
  return footnotes.length > 0 ? (
    <>
      <h3 className="govuk-heading-m">Footnotes</h3>

      <CollapsibleList listStyle="number" collapseAfter={2} testId="footnotes">
        {footnotes.map(footnote => (
          <li key={footnote.id}>{footnote.label}</li>
        ))}
      </CollapsibleList>
    </>
  ) : null;
};

export default FigureFootnotes;
