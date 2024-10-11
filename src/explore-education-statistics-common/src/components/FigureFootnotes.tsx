import CollapsibleList from '@common/components/CollapsibleList';
import ContentHtml from '@common/components/ContentHtml';
import VisuallyHidden from '@common/components/VisuallyHidden';
import { Footnote } from '@common/services/types/footnotes';
import React, { createElement } from 'react';

interface Props {
  footnotes: Footnote[];
  headingHiddenText?: string;
  headingTag?: 'h2' | 'h3';
  id: string;
}

const FigureFootnotes = ({
  footnotes,
  headingHiddenText,
  headingTag = 'h3',
  id,
}: Props) => {
  return footnotes.length > 0 ? (
    <>
      {createElement(
        headingTag,
        {
          className: `govuk-heading-m`,
        },
        <>
          Footnotes
          {headingHiddenText && (
            <VisuallyHidden>{` ${headingHiddenText}`}</VisuallyHidden>
          )}
        </>,
      )}

      <CollapsibleList
        listStyle="number"
        collapseAfter={2}
        id={id}
        itemName="footnote"
        itemNamePlural="footnotes"
        testId="footnotes"
      >
        {footnotes.map(footnote => (
          <li key={footnote.id}>
            <ContentHtml html={footnote.label} />
          </li>
        ))}
      </CollapsibleList>
    </>
  ) : null;
};

export default FigureFootnotes;
