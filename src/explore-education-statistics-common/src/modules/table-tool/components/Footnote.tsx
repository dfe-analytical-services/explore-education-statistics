import React from 'react';
import { FullTableMeta } from '@common/modules/full-table/types/fullTable';
import CollapsibleList from '@common/components/CollapsibleList';
import classNames from 'classnames';
import styles from './Footnote.module.scss';

export interface FootnoteProps {
  content: FullTableMeta['footnotes'];
}

const Footnote = ({ content }: FootnoteProps) => {
  const footnoteList = content.map(function createFootnotes(footnotes, index) {
    return (
      <li key={footnotes.id}>
        <span className="govuk-!-font-weight-bold">{index + 1}. </span>
        {footnotes.label}
      </li>
    );
  });

  return (
    <ol className={classNames('govuk-list', styles.footnotesContainer)}>
      <CollapsibleList>{footnoteList}</CollapsibleList>
    </ol>
  );
};

export default Footnote;
