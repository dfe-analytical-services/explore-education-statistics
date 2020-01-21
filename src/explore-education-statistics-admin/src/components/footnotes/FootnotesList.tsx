import {
  Footnote,
  FootnoteMeta,
  FootnoteMetaGetters,
} from '@admin/services/release/edit-release/footnotes/types';
import React from 'react';
import classNames from 'classnames';
import Button from '@common/components/Button';
import Details from '@common/components/Details';
import FootnoteForm, { FootnoteFormControls } from './form/FootnoteForm';
import styles from './FootnotesList.module.scss';
import FootnoteSubjectSelection from './FootnoteSubjectSelection';

interface Props {
  footnoteMeta: FootnoteMeta;
  footnoteMetaGetters: FootnoteMetaGetters;
  footnotes: Footnote[];
  footnoteFormControls: FootnoteFormControls;
  canUpdateRelease: boolean;
}

const FootnotesList = ({
  footnotes,
  footnoteMeta,
  footnoteMetaGetters,
  footnoteFormControls,
  canUpdateRelease,
}: Props) => {
  if (footnotes.length === 0) {
    return null;
  }

  const renderFootnoteRow = (footnote: Footnote) => {
    const { id, content } = footnote;
    const { footnoteForm } = footnoteFormControls;
    return (
      <div key={id} className={styles.itemContainer}>
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
            <div className={styles.row}>
              <div className={styles.row__footnoteContent}>{content}</div>
              {canUpdateRelease && (
                <div className={styles.row__footnoteActions}>
                  <Button
                    type="button"
                    className="govuk-button govuk-!-margin-right-3 govuk-!-margin-bottom-0"
                    onClick={() => footnoteFormControls.edit(footnote)}
                  >
                    Edit
                  </Button>
                  <Button
                    className="govuk-button govuk-button--secondary govuk-!-margin-bottom-0"
                    onClick={() => footnoteFormControls.delete(footnote)}
                  >
                    Delete
                  </Button>
                </div>
              )}
            </div>
            <Details
              summary="See matching criteria"
              className="govuk-!-margin-0"
            >
              <table
                className={classNames(
                  'govuk-table',
                  styles.footnoteSelectionTable,
                )}
              >
                <thead>
                  <tr>
                    <th>Subjects</th>
                    <th>Indicators</th>
                    <th>Filters</th>
                  </tr>
                </thead>
                <tbody className="govuk-body-s">
                  {Object.entries(footnote.subjects).map(
                    ([subjectId, selection]) => {
                      return (
                        <FootnoteSubjectSelection
                          key={subjectId}
                          subjectId={subjectId}
                          subject={selection}
                          footnoteMetaGetters={footnoteMetaGetters}
                        />
                      );
                    },
                  )}
                </tbody>
              </table>
            </Details>
          </>
        )}
      </div>
    );
  };

  return <div>{footnotes.map(renderFootnoteRow)}</div>;
};

export default FootnotesList;
