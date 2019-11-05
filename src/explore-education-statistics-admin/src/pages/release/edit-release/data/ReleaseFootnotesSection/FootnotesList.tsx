import React from 'react';
import { FootnoteMeta, Footnote } from '.';
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
            onOpen={() => {}}
            onCancel={footnoteFormControls.cancel}
            onSubmit={footnoteFormControls.save}
          />
        ) : (
          <>
            <td>{content}</td>
            <td>
              {subjects.map(subject => (
                <li key={subject}>{subject}</li>
              ))}
            </td>
            <td>
              {indicators.map(indicator => (
                <li key={indicator}>{indicator}</li>
              ))}
            </td>
            <td>
              {filters.map(filter => (
                <li key={filter}>{filter}</li>
              ))}
            </td>
            <td>
              <button
                type="button"
                onClick={() => footnoteFormControls.edit(footnote)}
              >
                edit
              </button>
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
