import Link from '@admin/components/Link';
import generateFootnoteMetaMap from '@admin/pages/release/footnotes/utils/generateFootnoteMetaMap';
import styles from '@admin/pages/release/footnotes/components/FootnotesList.module.scss';
import FootnoteSubjectSelection from '@admin/pages/release/footnotes/components/FootnoteSubjectSelection';
import {
  ReleaseFootnoteRouteParams,
  releaseFootnotesEditRoute,
} from '@admin/routes/releaseRoutes';
import { Footnote, FootnoteMeta } from '@admin/services/footnoteService';
import ReorderableList from '@common/components/ReorderableList';
import ButtonGroup from '@common/components/ButtonGroup';
import ButtonText from '@common/components/ButtonText';
import Details from '@common/components/Details';
import InsetText from '@common/components/InsetText';
import ModalConfirm from '@common/components/ModalConfirm';
import ContentHtml from '@common/components/ContentHtml';
import sanitizeHtml from '@common/utils/sanitizeHtml';
import reorder from '@common/utils/reorder';
import React, { useMemo } from 'react';
import { generatePath } from 'react-router';

interface Props {
  publicationId: string;
  releaseVersionId: string;
  footnoteMeta: FootnoteMeta;
  footnotes: Footnote[];
  canUpdateRelease: boolean;
  isReordering: boolean;
  onDelete: (footnote: Footnote) => void;
  onConfirmReordering: () => void;
  onReorder: (footnotes: Footnote[]) => void;
}

const FootnotesList = ({
  publicationId,
  releaseVersionId,
  footnotes,
  footnoteMeta,
  canUpdateRelease,
  isReordering,
  onConfirmReordering,
  onDelete,
  onReorder,
}: Props) => {
  const footnoteMetaGetters = useMemo(() => {
    return generateFootnoteMetaMap(footnoteMeta);
  }, [footnoteMeta]);

  if (footnotes.length === 0) {
    return <InsetText>No footnotes have been created.</InsetText>;
  }

  if (isReordering) {
    return (
      <ReorderableList
        heading="Reorder footnotes"
        id="footnotes"
        list={footnotes.map(footnote => ({
          id: footnote.id,
          label: sanitizeHtml(footnote.content, { allowedTags: [] }),
        }))}
        onConfirm={onConfirmReordering}
        onMoveItem={({ prevIndex, nextIndex }) => {
          const reordered = reorder(footnotes, prevIndex, nextIndex);
          onReorder(reordered);
        }}
        onReverse={() => {
          onReorder(footnotes.toReversed());
        }}
      />
    );
  }

  return (
    <>
      {footnotes.map(footnote => {
        const { id, content } = footnote;

        return (
          <div
            className={styles.itemContainer}
            id={id}
            key={id}
            data-testid={`Footnote - ${content}`}
          >
            <div className={styles.row}>
              <ContentHtml className={styles.rowContent} html={content} />

              {canUpdateRelease && (
                <ButtonGroup className={styles.rowActions}>
                  <Link
                    to={generatePath<ReleaseFootnoteRouteParams>(
                      releaseFootnotesEditRoute.path,
                      {
                        publicationId,
                        releaseVersionId,
                        footnoteId: id,
                      },
                    )}
                  >
                    Edit footnote
                  </Link>

                  <ModalConfirm
                    title="Delete footnote"
                    triggerButton={
                      <ButtonText variant="warning">Delete footnote</ButtonText>
                    }
                    onConfirm={async () => {
                      await onDelete(footnote);
                    }}
                  >
                    <p>
                      Are you sure you want to delete the following footnote:
                    </p>

                    <InsetText>
                      <p>{footnote.content}</p>
                    </InsetText>
                  </ModalConfirm>
                </ButtonGroup>
              )}
            </div>

            <Details
              summary="See matching criteria"
              className="govuk-!-margin-0"
            >
              <table className={styles.footnoteSelectionTable}>
                <thead>
                  <tr>
                    <th>Subjects</th>
                    <th>Indicators</th>
                    <th>Filters</th>
                  </tr>
                </thead>
                <tbody className="govuk-body-s">
                  {Object.entries(footnote.subjects).map(
                    ([subjectId, selection]) => (
                      <FootnoteSubjectSelection
                        key={subjectId}
                        subjectId={subjectId}
                        subject={selection}
                        footnoteMetaGetters={footnoteMetaGetters}
                      />
                    ),
                  )}
                </tbody>
              </table>
            </Details>
          </div>
        );
      })}
    </>
  );
};

export default FootnotesList;
