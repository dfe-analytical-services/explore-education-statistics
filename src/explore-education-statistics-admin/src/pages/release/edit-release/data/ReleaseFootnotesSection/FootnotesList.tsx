import {
  Footnote,
  FootnoteMeta,
} from '@admin/services/release/edit-release/footnotes/types';
import footnotesService from '@admin/services/release/edit-release/footnotes/service';
import React from 'react';
import Button from '@common/components/Button';
import CollapsibleList from '@common/components/CollapsibleList';
import FootnoteForm, { FootnoteFormControls } from './FootnoteForm';

interface Props {
  footnoteMeta: FootnoteMeta;
  footnotes: Footnote[];
  footnoteFormControls: FootnoteFormControls;
}

const FootnotesList = ({
  footnotes,
  footnoteMeta,
  footnoteFormControls,
}: Props) => {
  if (footnotes.length === 0) {
    return null;
  }

  const renderItems = (items: number[]) => {
    return (
      <CollapsibleList>
        {items.map(item => (
          <li key={item}>{item}</li>
        ))}
      </CollapsibleList>
    );
  };

  const renderFootnoteRow = (footnote: Footnote) => {
    const {
      id,
      content = '',
      subjects = [],
      indicators = [],
      filters = [],
      filterGroups = [],
      filterItems = [],
    } = footnote;
    const { footnoteForm } = footnoteFormControls;
    return (
      <tr key={id}>
        {footnoteForm.state === 'edit' &&
        footnoteForm.footnote &&
        footnoteForm.footnote.id === id ? (
          <FootnoteForm
            state={footnoteForm.state}
            footnote={footnote}
            footnoteMeta={footnoteMeta}
            onOpen={() => {}}
            onCancel={footnoteFormControls.cancel}
            onSubmit={footnoteFormControls.save}
          />
        ) : (
          <>
            <td>{content}</td>
            <td>{renderItems(subjects)}</td>
            <td>{renderItems(indicators)}</td>
            <td>{renderItems(filters)}</td>
            <td>
              <Button
                type="button"
                className="govuk-button govuk-!-margin-right-3 govuk-!-margin-bottom-0"
                onClick={() => footnoteFormControls.edit(footnote)}
              >
                Edit
              </Button>
              <Button
                className="govuk-button govuk-button--secondary govuk-!-margin-bottom-0"
                onClick={() => footnotesService.deleteFootnote(footnote.id)}
              >
                Delete
              </Button>
            </td>
          </>
        )}
      </tr>
    );
  };

  return (
    <table className="govuk-table">
      <thead>
        <tr>
          <th>Footnote</th>
          <th>Subjects</th>
          <th>Indicators</th>
          <th>Filters</th>
          <th>Actions</th>
        </tr>
      </thead>
      <tbody className="govuk-body-s">{footnotes.map(renderFootnoteRow)}</tbody>
    </table>
  );
};

export default FootnotesList;
