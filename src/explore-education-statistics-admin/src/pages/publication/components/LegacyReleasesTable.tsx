import ButtonLink from '@admin/components/ButtonLink';
import {
  legacyReleaseCreateRoute,
  legacyReleaseEditRoute,
} from '@admin/routes/legacyReleaseRoutes';
import { PublicationRouteParams } from '@admin/routes/routes';
import legacyReleaseService, {
  LegacyRelease,
} from '@admin/services/legacyReleaseService';
import publicationService, {
  BasicPublicationDetails,
} from '@admin/services/publicationService';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import ModalConfirm from '@common/components/ModalConfirm';
import useToggle from '@common/hooks/useToggle';
import reorder from '@common/utils/reorder';
import styles from '@admin/pages/publication/components/LegacyReleasesTable.module.scss';
import classNames from 'classnames';
import React, { useState } from 'react';
import { DragDropContext, Draggable, Droppable } from 'react-beautiful-dnd';
import { generatePath } from 'react-router';

interface Props {
  publication: BasicPublicationDetails;
}
const LegacyReleasesTable = ({ publication }: Props) => {
  const [isReordering, toggleReordering] = useToggle(false);
  const [deleteLegacyRelease, setDeleteLegacyRelease] = useState<
    LegacyRelease
  >();

  const [legacyReleases, setLegacyReleases] = useState(
    publication.legacyReleases,
  );

  return (
    <>
      <p>
        Legacy releases will be displayed in descending order on the
        publication.
      </p>

      {legacyReleases.length > 0 ? (
        <DragDropContext
          onDragEnd={result => {
            if (!result.destination) {
              return;
            }

            const nextLegacyReleases = reorder(
              legacyReleases,
              result.source.index,
              result.destination.index,
            ).map((release, index) => ({
              ...release,
              order: legacyReleases.length - index,
            }));
            setLegacyReleases(nextLegacyReleases);
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
                  {legacyReleases.map((release, index) => (
                    <Draggable
                      draggableId={release.id}
                      isDragDisabled={!isReordering}
                      key={release.id}
                      index={index}
                    >
                      {(draggableProvided, draggableSnapshot) => (
                        <tr
                          // eslint-disable-next-line react/jsx-props-no-spreading
                          {...draggableProvided.draggableProps}
                          // eslint-disable-next-line react/jsx-props-no-spreading
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
                              <ButtonGroup className="govuk-!-margin-bottom-0">
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
        open={!!deleteLegacyRelease}
        onConfirm={async () => {
          if (deleteLegacyRelease) {
            await legacyReleaseService.deleteLegacyRelease(
              deleteLegacyRelease?.id,
            );

            const nextLegacyReleases = legacyReleases.filter(
              release => release.id !== deleteLegacyRelease.id,
            );
            setLegacyReleases(nextLegacyReleases);
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

          {legacyReleases.length > 0 && (
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
                legacyReleases.map(release => ({
                  id: release.id,
                  order: release.order,
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
              setLegacyReleases(publication.legacyReleases);
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
