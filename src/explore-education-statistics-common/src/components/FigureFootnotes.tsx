import CollapsibleList from '@common/components/CollapsibleList';
import { Footnote } from '@common/services/types/footnotes';
import React from 'react';
import VisuallyHidden from './VisuallyHidden';

interface Props {
  footnotes: Footnote[];
  headingHiddenText?: string;
  id: string;
}

const FigureFootnotes = ({ footnotes, headingHiddenText, id }: Props) => {
  return footnotes.length > 0 ? (
    <>
      <h3 className="govuk-heading-m">
        Footnotes
        {headingHiddenText && (
          <VisuallyHidden>{` ${headingHiddenText}`}</VisuallyHidden>
        )}
      </h3>
      <CollapsibleList
        listStyle="number"
        collapseAfter={2}
        id={id}
        itemName="footnote"
        itemNamePlural="footnotes"
        testId="footnotes"
      >
        {footnotes.map(footnote => (
          <li key={footnote.id}>{footnote.label}</li>
        ))}
      </CollapsibleList>
    </>
  ) : null;
};

export default FigureFootnotes;
