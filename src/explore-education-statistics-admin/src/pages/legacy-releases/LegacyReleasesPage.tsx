import ButtonLink from '@admin/components/ButtonLink';
import Page from '@admin/components/Page';
import usePublicationContext from '@admin/contexts/PublicationContext';
import {
  legacyReleaseCreateRoute,
  legacyReleaseEditRoute,
} from '@admin/routes/legacyReleaseRoutes';
import { dashboardRoute, PublicationRouteParams } from '@admin/routes/routes';
import legacyReleaseService, {
  LegacyRelease,
} from '@admin/services/legacyReleaseService';
import publicationService from '@admin/services/publicationService';
import appendQuery from '@admin/utils/url/appendQuery';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import LoadingSpinner from '@common/components/LoadingSpinner';
import ModalConfirm from '@common/components/ModalConfirm';
import useToggle from '@common/hooks/useToggle';
import reorder from '@common/utils/reorder';
import classNames from 'classnames';
import React, { useState } from 'react';
import { DragDropContext, Draggable, Droppable } from 'react-beautiful-dnd';
import { generatePath } from 'react-router';
import styles from './LegacyReleasesPage.module.scss';

const LegacyReleasesPage = () => {
  const {
    value: publication,
    setState: setPublication,
    isLoading,
    retry: reloadPublication,
  } = usePublicationContext();

  const [isReordering, toggleReordering] = useToggle(false);
  const [deleteLegacyRelease, setDeleteLegacyRelease] = useState<
    LegacyRelease
  >();

  if (!publication) {
    return <LoadingSpinner loading={isLoading} />;
  }

  return (
    <Page
      title="Legacy releases"
      caption={publication.title}
      breadcrumbs={[{ name: 'Legacy releases' }]}
      backLink={appendQuery(dashboardRoute.path, {
        themeId: publication.themeId,
        topicId: publication.topicId,
      })}
    >
      <p>
        Legacy releases will be displayed in descending order on the
        publication.
      </p>

      {publication.legacyReleases.length > 0 ? (
        <DragDropContext
          onDragEnd={result => {
            if (!result.destination) {
              return;
            }

            const nextLegacyReleases = reorder(
              publication.legacyReleases,
              result.source.index,
              result.destination.index,
            ).map((release, index) => ({
              ...release,
              order: publication.legacyReleases.length - index,
            }));

            setPublication({
              isLoading: false,
              value: {
                ...publication,
                legacyReleases: nextLegacyReleases,
              },
            });
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
                    <th>Order</th>
                    <th>Description</th>
                    <th>URL</th>
                    {!isReordering && (
                      <th className="govuk-!-width-one-third">Actions</th>
                    )}
                  </tr>
                </thead>
                <tbody>
                  {publication.legacyReleases.map((release, index) => (
                    <Draggable
                      draggableId={release.id}
                      isDragDisabled={!isReordering}
                      key={release.id}
                      index={index}
                    >
                      {(draggableProvided, draggableSnapshot) => (
                        <tr
                          {...draggableProvided.draggableProps}
                          {...draggableProvided.dragHandleProps}
                          ref={draggableProvided.innerRef}
                          className={classNames({
                            [styles.reorderingRow]: isReordering,
                            [styles.reorderingRowDragging]:
                              draggableSnapshot.isDragging,
                          })}
                        >
                          {isReordering && (
                            <td className={styles.dragHandle}>⬍</td>
                          )}

                          <td>{release.order}</td>

                          <td>{release.description}</td>
                          <td
                            className={classNames({
                              'govuk-!-width-one-half': isReordering,
                            })}
                          >
                            <a
                              className="govuk-link--no-visited-state"
                              href={release.url}
                              target="_blank"
                              rel="noopener noreferrer"
                              tabIndex={isReordering ? -1 : undefined}
                            >
                              {release.url}
                            </a>
                          </td>

                          {!isReordering && (
                            <td>
                              <ButtonGroup>
                                <ButtonLink
                                  to={generatePath(
                                    legacyReleaseEditRoute.path,
                                    {
                                      publicationId: publication.id,
                                      legacyReleaseId: release.id,
                                    },
                                  )}
                                >
                                  Edit release
                                </ButtonLink>
                                <Button
                                  variant="warning"
                                  onClick={() => {
                                    setDeleteLegacyRelease(release);
                                  }}
                                >
                                  Delete release
                                </Button>
                              </ButtonGroup>
                            </td>
                          )}
                        </tr>
                      )}
                    </Draggable>
                  ))}

                  {droppableProvided.placeholder}
                </tbody>
              </table>
            )}
          </Droppable>
        </DragDropContext>
      ) : (
        <p>No legacy releases for this publication.</p>
      )}

      <ModalConfirm
        mounted={!!deleteLegacyRelease}
        onConfirm={async () => {
          if (deleteLegacyRelease) {
            await legacyReleaseService.deleteLegacyRelease(
              deleteLegacyRelease?.id,
            );

            await reloadPublication();
          }

          setDeleteLegacyRelease(undefined);
        }}
        onExit={() => setDeleteLegacyRelease(undefined)}
        title="Delete legacy release"
      >
        <p>Are you sure you want to delete this legacy release?</p>
      </ModalConfirm>

      {!isReordering ? (
        <ButtonGroup>
          <ButtonLink
            to={generatePath<PublicationRouteParams>(
              legacyReleaseCreateRoute.path,
              {
                publicationId: publication.id,
              },
            )}
          >
            Create legacy release
          </ButtonLink>

          {publication.legacyReleases.length > 0 && (
            <Button variant="secondary" onClick={toggleReordering.on}>
              Reorder legacy releases
            </Button>
          )}
        </ButtonGroup>
      ) : (
        <ButtonGroup>
          <Button
            onClick={async () => {
              await publicationService.partialUpdateLegacyReleases(
                publication.id,
                publication.legacyReleases.map(release => ({
                  id: release.id,
                  order: release.order,
                })),
              );

              toggleReordering.off();
            }}
          >
            Confirm order
          </Button>
          <Button variant="secondary" onClick={toggleReordering.off}>
            Cancel reordering
          </Button>
        </ButtonGroup>
      )}
    </Page>
  );
};

export default LegacyReleasesPage;
