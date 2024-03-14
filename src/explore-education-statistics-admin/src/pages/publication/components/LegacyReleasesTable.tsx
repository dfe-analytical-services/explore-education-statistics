import DraggableItem from '@admin/components/DraggableItem';
import DroppableArea from '@admin/components/DroppableArea';
import Link from '@admin/components/Link';
import {
  publicationCreateLegacyReleaseRoute,
  PublicationRouteParams,
  publicationEditLegacyReleaseRoute,
  PublicationEditLegacyReleaseRouteParams,
} from '@admin/routes/publicationRoutes';
import publicationService from '@admin/services/publicationService';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import ModalConfirm from '@common/components/ModalConfirm';
import WarningMessage from '@common/components/WarningMessage';
import useToggle from '@common/hooks/useToggle';
import reorder from '@common/utils/reorder';
import styles from '@admin/pages/publication/components/LegacyReleasesTable.module.scss';
import classNames from 'classnames';
import React, { useState } from 'react';
import { DragDropContext, Droppable } from 'react-beautiful-dnd';
import { generatePath } from 'react-router';
import { useHistory } from 'react-router-dom';
import ButtonText from '@common/components/ButtonText';
import VisuallyHidden from '@common/components/VisuallyHidden';
import { useConfig } from '@admin/contexts/ConfigContext';
import {ReleaseSeriesItem} from "@common/services/publicationService";

interface Props {
  canManageLegacyReleases: boolean;
  releaseSeries: ReleaseSeriesItem[];
  publicationId: string;
}
const LegacyReleasesTable = ({
  // @MarkFix rename
  canManageLegacyReleases,
  releaseSeries: initialReleaseSeries,
  publicationId,
}: Props) => {
  const history = useHistory();
  const [isReordering, toggleReordering] = useToggle(false);
  const [releaseSeries, setReleaseSeries] = useState(initialReleaseSeries);

  const config = useConfig();

  return (
    <>
      <p>Releases will be shown in the order below on the publication.</p>
      <p>
        Explore education statistics releases from this publication can also be
        reordered, including those in draft status or with a draft amendment,
        but cannot be edited or deleted. Only releases with a published version
        will be shown on the publication.
      </p>

      {canManageLegacyReleases && !isReordering && (
        <ButtonGroup>
          <ModalConfirm
            confirmText="OK"
            title="Create legacy release"
            triggerButton={<Button>Create legacy release</Button>}
            onConfirm={() => {
              history.push(
                generatePath<PublicationRouteParams>(
                  publicationCreateLegacyReleaseRoute.path,
                  {
                    publicationId,
                  },
                ),
              );
            }}
          >
            <WarningMessage>
              All changes made to legacy releases appear immediately on the
              public website.
            </WarningMessage>
          </ModalConfirm>

          {releaseSeries.length > 0 && (
            <ModalConfirm
              confirmText="OK"
              title="Reorder releases"
              triggerButton={
                <Button variant="secondary">Reorder releases</Button>
              }
              onConfirm={toggleReordering.on}
            >
              <WarningMessage>
                All changes made to releases appear immediately on the public
                website.
              </WarningMessage>
            </ModalConfirm>
          )}
        </ButtonGroup>
      )}

      {releaseSeries.length > 0 ? (
        <DragDropContext
          onDragEnd={seriesItem => {
            if (!seriesItem.destination) {
              return;
            }

            const nextReleaseSeries = reorder(
              releaseSeries,
              seriesItem.source.index,
              seriesItem.destination.index,
            );

            setReleaseSeries(nextReleaseSeries);
          }}
        >
          <Droppable droppableId="droppable" isDropDisabled={!isReordering}>
            {(droppableProvided, droppableSnapshot) => (
              <table
                ref={droppableProvided.innerRef}
                className={classNames({
                  [styles.tableDraggingOver]: droppableSnapshot.isDraggingOver,
                })}
              >
                <thead>
                  <tr>
                    {isReordering && <th>Sort</th>}
                    <th>Description</th>
                    <th>URL</th>
                    {canManageLegacyReleases && !isReordering && (
                      <th>Actions</th>
                    )}
                  </tr>
                </thead>
                <DroppableArea
                  droppableProvided={droppableProvided}
                  droppableSnapshot={droppableSnapshot}
                  tag="tbody"
                >
                  {releaseSeries.map((seriesItem, index) => (
                    <DraggableItem
                      hideDragHandle
                      id={seriesItem.id}
                      index={index}
                      isReordering={isReordering}
                      key={seriesItem.id}
                      tag="tr"
                    >
                      {isReordering && <td className={styles.dragHandle}>⬍</td>}

                      <td>
                        <span className="govuk-!-display-block">
                          {seriesItem.description}
                        </span>
                        {/* @MarkFix{seriesItem.isLatest && ( */}
                        {/*  <span className="govuk-tag govuk-!-display-inline-block govuk-!-margin-right-1 govuk-!-margin-bottom-1"> */}
                        {/*    Latest */}
                        {/*  </span> */}
                        {/* )} */}
                        {/* @MarkFix{seriesItem.isDraft && ( */}
                        {/*  <span className="govuk-tag govuk-!-display-inline-block govuk-!-margin-right-1 govuk-!-margin-bottom-1 dfe-white-space--nowrap"> */}
                        {/*    Draft{seriesItem.isAmendment && ' Amendment'} */}
                        {/*  </span> */}
                        {/* )} */}
                      </td>
                      <td
                        className={classNames({
                          'govuk-!-width-one-half': isReordering,
                        })}
                      >
                        {seriesItem.isLegacyLink && (
                          <a
                            className="govuk-link--no-visited-state"
                            href={seriesItem.legacyLinkUrl}
                            target="_blank"
                            rel="noopener noreferrer"
                            tabIndex={isReordering ? -1 : undefined}
                          >
                            {seriesItem.legacyLinkUrl}
                          </a>
                        )}

                        {!seriesItem.isLegacyLink && (
                          // seriesItem.isDraft && // @MarkFix
                          // seriesItem.isAmendment) ||
                          // (!seriesItem.isDraft && !seriesItem.isAmendment)) && (
                          <Link
                            to={`${config.publicAppUrl}/find-statistics/${seriesItem.publicationSlug}/${seriesItem.releaseSlug}`}
                            className="govuk-link--no-visited-state"
                            target="_blank"
                            rel="noopener noreferrer"
                            tabIndex={isReordering ? -1 : undefined}
                          >
                            {`${config.publicAppUrl}/find-statistics/${seriesItem.publicationSlug}/${seriesItem.releaseSlug}`}
                          </Link>
                        )}
                      </td>

                      {canManageLegacyReleases && !isReordering && (
                        <td>
                          {seriesItem.isLegacyLink && (
                            <ButtonGroup className="govuk-!-margin-bottom-0">
                              <ModalConfirm
                                confirmText="OK"
                                title="Edit legacy release"
                                triggerButton={
                                  <ButtonText>
                                    Edit{/* @MarkFix editing is broken */}
                                    <VisuallyHidden>
                                      {` ${seriesItem.description}`}
                                    </VisuallyHidden>
                                  </ButtonText>
                                }
                                onConfirm={() => {
                                  history.push(
                                    generatePath<PublicationEditLegacyReleaseRouteParams>(
                                      publicationEditLegacyReleaseRoute.path,
                                      {
                                        publicationId,
                                        legacyReleaseId: seriesItem.id,
                                      },
                                    ),
                                  );
                                }}
                              >
                                <WarningMessage>
                                  All changes made to legacy releases appear
                                  immediately on the public website.
                                </WarningMessage>
                              </ModalConfirm>

                              <ModalConfirm
                                title="Delete legacy release"
                                triggerButton={
                                  <ButtonText variant="warning">
                                    Delete
                                    <VisuallyHidden>
                                      {` ${seriesItem.description}`}
                                    </VisuallyHidden>
                                  </ButtonText>
                                }
                                onConfirm={async () => {
                                  const nextReleaseSeries =
                                    releaseSeries.filter(
                                      item => item.id !== seriesItem.id,
                                    );
                                  await publicationService.updateReleaseSeriesView(
                                    publicationId,
                                    nextReleaseSeries.map(item => ({
                                      // @MarkFix abstract this mapping out?
                                      id: item.id,
                                      releaseId: !item.isLegacyLink
                                        ? item.releaseId
                                        : undefined,
                                      legacyLinkDescription: item.isLegacyLink
                                        ? item.description
                                        : undefined,
                                      legacyLinkUrl: item.isLegacyLink
                                        ? item.legacyLinkUrl
                                        : undefined,
                                    })),
                                  );
                                  setReleaseSeries(nextReleaseSeries);
                                }}
                              >
                                <p>
                                  Are you sure you want to delete this legacy
                                  release?
                                </p>
                                <p>
                                  All changes made to legacy releases appear
                                  immediately on the public website.
                                </p>
                              </ModalConfirm>
                            </ButtonGroup>
                          )}
                        </td>
                      )}
                    </DraggableItem>
                  ))}
                </DroppableArea>
              </table>
            )}
          </Droppable>
        </DragDropContext>
      ) : (
        <p>No releases for this publication.</p>
      )}

      {isReordering && (
        <ButtonGroup>
          <Button
            onClick={async () => {
              await publicationService.updateReleaseSeriesView(
                publicationId,
                releaseSeries.map(seriesItem => ({
                  id: seriesItem.id,
                  releaseId: !seriesItem.isLegacyLink
                    ? seriesItem.releaseId
                    : undefined,
                  legacyLinkDescription: seriesItem.isLegacyLink
                    ? seriesItem.description
                    : undefined,
                  legacyLinkUrl: seriesItem.isLegacyLink
                    ? seriesItem.legacyLinkUrl
                    : undefined,
                })),
              );

              toggleReordering.off();
            }}
          >
            Confirm order
          </Button>
          <Button
            variant="secondary"
            onClick={() => {
              setReleaseSeries(releaseSeries);
              toggleReordering.off();
            }}
          >
            Cancel reordering
          </Button>
        </ButtonGroup>
      )}
    </>
  );
};

export default LegacyReleasesTable;
