import {
  Footnote,
  FootnoteMeta,
  FootnoteMetaGetters,
} from '@admin/services/release/edit-release/footnotes/types';
import footnotesService from '@admin/services/release/edit-release/footnotes/service';
import React from 'react';
import Button from '@common/components/Button';
import CollapsibleList from '@common/components/CollapsibleList';
import FootnoteForm, { FootnoteFormControls } from './FootnoteForm';

interface Props {
  footnoteMeta: FootnoteMeta;
  footnoteMetaGetters: FootnoteMetaGetters;
  footnotes: Footnote[];
  footnoteFormControls: FootnoteFormControls;
}

const FootnotesList = ({
  footnotes,
  footnoteMeta,
  footnoteMetaGetters,
  footnoteFormControls,
}: Props) => {
  if (footnotes.length === 0) {
    return null;
  }

  const renderItems = (items: { label: string; value: number }[]) => {
    return (
      <CollapsibleList>
        {items.map(item => (
          <li key={item.value}>{item.label}</li>
        ))}
      </CollapsibleList>
    );
  };

  const renderFootnoteRow = (footnote: Footnote) => {
    const {
      id,
      content,
      subjects,
      indicators,
      filters,
      filterGroups,
      filterItems,
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
            footnoteMetaGetters={footnoteMetaGetters}
            onOpen={() => {}}
            onCancel={footnoteFormControls.cancel}
            onSubmit={footnoteFormControls.save}
          />
        ) : (
          <>
            <td>{content}</td>
            <td>{renderItems(subjects.map(footnoteMetaGetters.getSubject))}</td>
            <td>
              {renderItems(indicators.map(footnoteMetaGetters.getIndicator))}
            </td>
            <td>
              {renderItems([
                ...filters.map(footnoteMetaGetters.getFilter),
                ...filterGroups.map(footnoteMetaGetters.getFilterGroup),
                ...filterItems.map(footnoteMetaGetters.getFilterItem),
              ])}
            </td>
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
                onClick={() => footnoteFormControls.delete(footnote.id)}
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
