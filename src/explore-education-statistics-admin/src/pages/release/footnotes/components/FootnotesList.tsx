import DroppableArea from '@admin/components/DroppableArea';
import DraggableItem from '@admin/components/DraggableItem';
import Link from '@admin/components/Link';
import generateFootnoteMetaMap from '@admin/pages/release/footnotes/utils/generateFootnoteMetaMap';
import styles from '@admin/pages/release/footnotes/components/FootnotesList.module.scss';
import FootnoteSubjectSelection from '@admin/pages/release/footnotes/components/FootnoteSubjectSelection';
import {
  ReleaseFootnoteRouteParams,
  releaseFootnotesEditRoute,
} from '@admin/routes/releaseRoutes';
import { Footnote, FootnoteMeta } from '@admin/services/footnoteService';
import ButtonGroup from '@common/components/ButtonGroup';
import ButtonText from '@common/components/ButtonText';
import Details from '@common/components/Details';
import InsetText from '@common/components/InsetText';
import ModalConfirm from '@common/components/ModalConfirm';
import reorder from '@common/utils/reorder';
import classNames from 'classnames';
import React, { useMemo } from 'react';
import { generatePath } from 'react-router';
import { DragDropContext, Droppable } from '@hello-pangea/dnd';
import ContentHtml from '@common/components/ContentHtml';

interface Props {
  publicationId: string;
  releaseId: string;
  footnoteMeta: FootnoteMeta;
  footnotes: Footnote[];
  canUpdateRelease: boolean;
  isReordering: boolean;
  onDelete: (footnote: Footnote) => void;
  onReorder: (footnotes: Footnote[]) => void;
}

const FootnotesList = ({
  publicationId,
  releaseId,
  footnotes,
  footnoteMeta,
  canUpdateRelease,
  isReordering,
  onDelete,
  onReorder,
}: Props) => {
  const footnoteMetaGetters = useMemo(() => {
    return generateFootnoteMetaMap(footnoteMeta);
  }, [footnoteMeta]);

  if (footnotes.length === 0) {
    return <InsetText>No footnotes have been created.</InsetText>;
  }

  return (
    <DragDropContext
      onDragEnd={result => {
        if (!result.destination) {
          return;
        }
        const reorderedFootnotes = reorder(
          footnotes,
          result.source.index,
          result.destination.index,
        );
        onReorder(reorderedFootnotes);
      }}
    >
      <Droppable droppableId="footnotes" isDropDisabled={!isReordering}>
        {(droppableProvided, droppableSnapshot) => (
          <DroppableArea
            droppableProvided={droppableProvided}
            droppableSnapshot={droppableSnapshot}
          >
            {footnotes.map((footnote, index) => {
              const { id, content } = footnote;

              return (
                <DraggableItem
                  className={classNames({
                    [styles.itemContainer]: !isReordering,
                  })}
                  id={id}
                  index={index}
                  isReordering={isReordering}
                  key={id}
                  testId={`Footnote - ${content}`}
                >
                  <div className={styles.row}>
                    <ContentHtml className={styles.rowContent} html={content} />

                    {!isReordering && canUpdateRelease && (
                      <ButtonGroup className={styles.rowActions}>
                        <Link
                          to={generatePath<ReleaseFootnoteRouteParams>(
                            releaseFootnotesEditRoute.path,
                            {
                              publicationId,
                              releaseId,
                              footnoteId: id,
                            },
                          )}
                        >
                          Edit footnote
                        </Link>

                        <ModalConfirm
                          title="Delete footnote"
                          triggerButton={
                            <ButtonText variant="warning">
                              Delete footnote
                            </ButtonText>
                          }
                          onConfirm={async () => {
                            await onDelete(footnote);
                          }}
                        >
                          <p>
                            Are you sure you want to delete the following
                            footnote:
                          </p>

                          <InsetText>
                            <p>{footnote.content}</p>
                          </InsetText>
                        </ModalConfirm>
                      </ButtonGroup>
                    )}
                  </div>
                  {!isReordering && (
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
                  )}
                </DraggableItem>
              );
            })}
          </DroppableArea>
        )}
      </Droppable>
    </DragDropContext>
  );
};

export default FootnotesList;
