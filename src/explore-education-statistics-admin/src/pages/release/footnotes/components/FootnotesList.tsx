import ButtonLink from '@admin/components/ButtonLink';
import generateFootnoteMetaMap from '@admin/pages/release/footnotes/utils/generateFootnoteMetaMap';
import styles from '@admin/pages/release/footnotes/components/FootnotesList.module.scss';
import FootnoteSubjectSelection from '@admin/pages/release/footnotes/components/FootnoteSubjectSelection';
import {
  ReleaseFootnoteRouteParams,
  releaseFootnotesEditRoute,
} from '@admin/routes/releaseRoutes';
import { Footnote, FootnoteMeta } from '@admin/services/footnoteService';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import Details from '@common/components/Details';
import InsetText from '@common/components/InsetText';
import ModalConfirm from '@common/components/ModalConfirm';
import React, { useMemo, useState } from 'react';
import { generatePath } from 'react-router';

interface Props {
  publicationId: string;
  releaseId: string;
  footnoteMeta: FootnoteMeta;
  footnotes: Footnote[];
  canUpdateRelease: boolean;
  onDelete: (footnote: Footnote) => void;
}

const FootnotesList = ({
  publicationId,
  releaseId,
  footnotes,
  footnoteMeta,
  canUpdateRelease,
  onDelete,
}: Props) => {
  const [deleteFootnote, setDeleteFootnote] = useState<Footnote>();

  const footnoteMetaGetters = useMemo(() => {
    return generateFootnoteMetaMap(footnoteMeta);
  }, [footnoteMeta]);

  if (footnotes.length === 0) {
    return <InsetText>No footnotes have been created.</InsetText>;
  }

  return (
    <>
      {footnotes.map(footnote => {
        const { id, content } = footnote;

        return (
          <div
            key={id}
            className={styles.itemContainer}
            data-testid={`Footnote - ${content}`}
          >
            <div className={styles.row}>
              <div className={styles.rowContent}>{content}</div>

              {canUpdateRelease && (
                <ButtonGroup className={styles.rowActions}>
                  <ButtonLink
                    to={generatePath<ReleaseFootnoteRouteParams>(
                      releaseFootnotesEditRoute.path,
                      {
                        publicationId,
                        releaseId,
                        footnoteId: footnote.id,
                      },
                    )}
                  >
                    Edit footnote
                  </ButtonLink>
                  <Button
                    variant="secondary"
                    onClick={() => setDeleteFootnote(footnote)}
                  >
                    Delete footnote
                  </Button>
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

      {deleteFootnote && (
        <ModalConfirm
          title="Delete footnote"
          open={!!deleteFootnote}
          onExit={() => setDeleteFootnote(undefined)}
          onConfirm={async () => {
            if (!deleteFootnote) {
              return;
            }

            await onDelete(deleteFootnote);
            setDeleteFootnote(undefined);
          }}
        >
          <p>Are you sure you want to delete the following footnote:</p>

          <InsetText>
            <p>{deleteFootnote.content}</p>
          </InsetText>
        </ModalConfirm>
      )}
    </>
  );
};

export default FootnotesList;
