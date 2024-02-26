import DraggableItem from '@admin/components/DraggableItem';
import DroppableArea from '@admin/components/DroppableArea';
import Link from '@admin/components/Link';
import {
  publicationCreateLegacyReleaseRoute,
  PublicationRouteParams,
  publicationEditLegacyReleaseRoute,
  PublicationEditLegacyReleaseRouteParams,
} from '@admin/routes/publicationRoutes';
import legacyReleaseService, {
  ReleaseSeriesItem,
} from '@admin/services/legacyReleaseService';
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

interface Props {
  canManageLegacyReleases: boolean;
  releaseSeries: ReleaseSeriesItem[];
  publicationId: string;
}
const LegacyReleasesTable = ({
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
          onDragEnd={result => {
            if (!result.destination) {
              return;
            }

            const nextReleaseSeries = reorder(
              releaseSeries,
              result.source.index,
              result.destination.index,
            ).map((release, index) => ({
              ...release,
              order: releaseSeries.length - index,
            }));
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
                  {releaseSeries.map((release, index) => (
                    <DraggableItem
                      hideDragHandle
                      id={release.id}
                      index={index}
                      isReordering={isReordering}
                      key={release.id}
                      tag="tr"
                    >
                      {isReordering && <td className={styles.dragHandle}>⬍</td>}

                      <td>
                        <span className="govuk-!-display-block">
                          {release.description}
                        </span>
                        {release.isLatest && (
                          <span className="govuk-tag govuk-!-display-inline-block govuk-!-margin-right-1 govuk-!-margin-bottom-1">
                            Latest
                          </span>
                        )}
                        {release.isDraft && (
                          <span className="govuk-tag govuk-!-display-inline-block govuk-!-margin-right-1 govuk-!-margin-bottom-1 dfe-white-space--nowrap">
                            Draft{release.isAmendment && ' Amendment'}
                          </span>
                        )}
                      </td>
                      <td
                        className={classNames({
                          'govuk-!-width-one-half': isReordering,
                        })}
                      >
                        {release.isLegacy && (
                          <a
                            className="govuk-link--no-visited-state"
                            href={release.url}
                            target="_blank"
                            rel="noopener noreferrer"
                            tabIndex={isReordering ? -1 : undefined}
                          >
                            {release.url}
                          </a>
                        )}

                        {((!release.isLegacy &&
                          release.isDraft &&
                          release.isAmendment) ||
                          (!release.isDraft && !release.isAmendment)) && (
                          <Link
                            to={`${config.publicAppUrl}/find-statistics/${release.url}`}
                            className="govuk-link--no-visited-state"
                            target="_blank"
                            rel="noopener noreferrer"
                            tabIndex={isReordering ? -1 : undefined}
                          >
                            {`${config.publicAppUrl}/find-statistics/${release.url}`}
                          </Link>
                        )}
                      </td>

                      {canManageLegacyReleases && !isReordering && (
                        <td>
                          {release.isLegacy && (
                            <ButtonGroup className="govuk-!-margin-bottom-0">
                              <ModalConfirm
                                confirmText="OK"
                                title="Edit legacy release"
                                triggerButton={
                                  <ButtonText>
                                    Edit
                                    <VisuallyHidden>
                                      {` ${release.description}`}
                                    </VisuallyHidden>
                                  </ButtonText>
                                }
                                onConfirm={() => {
                                  history.push(
                                    generatePath<PublicationEditLegacyReleaseRouteParams>(
                                      publicationEditLegacyReleaseRoute.path,
                                      {
                                        publicationId,
                                        legacyReleaseId: release.id,
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
                                      {` ${release.description}`}
                                    </VisuallyHidden>
                                  </ButtonText>
                                }
                                onConfirm={async () => {
                                  await legacyReleaseService.deleteLegacyRelease(
                                    release?.id,
                                  );

                                  const nextReleaseSeries =
                                    releaseSeries.filter(
                                      legacyRelease =>
                                        legacyRelease.id !== release.id,
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
                releaseSeries.map(release => ({
                  id: release.id,
                  order: release.order,
                  isLegacy: release.isLegacy,
                  isAmendment: release.isAmendment,
                  isLatest: release.isLatest,
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
