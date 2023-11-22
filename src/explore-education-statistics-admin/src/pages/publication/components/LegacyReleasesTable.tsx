import DraggableItem from '@admin/components/DraggableItem';
import DroppableArea from '@admin/components/DroppableArea';
import {
  publicationCreateLegacyReleaseRoute,
  PublicationRouteParams,
  publicationEditLegacyReleaseRoute,
  PublicationEditLegacyReleaseRouteParams,
} from '@admin/routes/publicationRoutes';
import legacyReleaseService, {
  LegacyRelease,
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

interface Props {
  canManageLegacyReleases: boolean;
  legacyReleases: LegacyRelease[];
  publicationId: string;
}
const LegacyReleasesTable = ({
  canManageLegacyReleases,
  legacyReleases: initialLegacyReleases,
  publicationId,
}: Props) => {
  const history = useHistory();
  const [isReordering, toggleReordering] = useToggle(false);
  const [legacyReleases, setLegacyReleases] = useState(initialLegacyReleases);

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
                  {legacyReleases.map((release, index) => (
                    <DraggableItem
                      hideDragHandle
                      id={release.id}
                      index={index}
                      isReordering={isReordering}
                      key={release.id}
                      tag="tr"
                    >
                      {isReordering && <td className={styles.dragHandle}>⬍</td>}

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

                      {canManageLegacyReleases && !isReordering && (
                        <td>
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

                                const nextLegacyReleases =
                                  legacyReleases.filter(
                                    legacyRelease =>
                                      legacyRelease.id !== release.id,
                                  );
                                setLegacyReleases(nextLegacyReleases);
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
        <p>No legacy releases for this publication.</p>
      )}

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

          {legacyReleases.length > 0 && (
            <ModalConfirm
              confirmText="OK"
              title="Reorder legacy releases"
              triggerButton={
                <Button variant="secondary">Reorder legacy releases</Button>
              }
              onConfirm={toggleReordering.on}
            >
              <WarningMessage>
                All changes made to legacy releases appear immediately on the
                public website.
              </WarningMessage>
            </ModalConfirm>
          )}
        </ButtonGroup>
      )}

      {isReordering && (
        <ButtonGroup>
          <Button
            onClick={async () => {
              await publicationService.partialUpdateLegacyReleases(
                publicationId,
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
              setLegacyReleases(legacyReleases);
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
