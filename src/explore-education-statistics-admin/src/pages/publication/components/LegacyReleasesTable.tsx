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
import { DragDropContext, Draggable, Droppable } from 'react-beautiful-dnd';
import { generatePath } from 'react-router';
import { useHistory } from 'react-router-dom';
import ButtonText from '@common/components/ButtonText';
import VisuallyHidden from '@common/components/VisuallyHidden';

interface ConfirmAction {
  type: 'create' | 'edit' | 'reordering';
  id?: string;
}

const getModalTitle = (confirmAction?: ConfirmAction): string => {
  switch (confirmAction?.type) {
    case 'create':
      return 'Create legacy release';
    case 'edit':
      return 'Edit legacy release';
    case 'reordering':
      return 'Reorder legacy releases';
    default:
      return '';
  }
};

interface Props {
  legacyReleases: LegacyRelease[];
  publicationId: string;
}
const LegacyReleasesTable = ({
  legacyReleases: initialLegacyReleases,
  publicationId,
}: Props) => {
  const history = useHistory();
  const [isReordering, toggleReordering] = useToggle(false);
  const [confirmAction, setConfirmAction] = useState<ConfirmAction>();
  const [deleteLegacyRelease, setDeleteLegacyRelease] = useState<
    LegacyRelease
  >();

  const [legacyReleases, setLegacyReleases] = useState(initialLegacyReleases);

  return (
    <>
      <p>
        Legacy releases will be displayed in descending order on the
        publication.
      </p>

      <h3>Legacy release order</h3>

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
                    {!isReordering && <th>Actions</th>}
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
                                <ButtonText
                                  onClick={() =>
                                    setConfirmAction({
                                      type: 'edit',
                                      id: release.id,
                                    })
                                  }
                                >
                                  Edit
                                  <VisuallyHidden>
                                    {' '}
                                    {release.description}
                                  </VisuallyHidden>
                                </ButtonText>
                                <ButtonText
                                  variant="warning"
                                  onClick={() => {
                                    setDeleteLegacyRelease(release);
                                  }}
                                >
                                  Delete
                                  <VisuallyHidden>
                                    {' '}
                                    {release.description}
                                  </VisuallyHidden>
                                </ButtonText>
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

      {!isReordering ? (
        <ButtonGroup>
          <Button onClick={() => setConfirmAction({ type: 'create' })}>
            Create legacy release
          </Button>

          {legacyReleases.length > 0 && (
            <Button
              variant="secondary"
              onClick={() => setConfirmAction({ type: 'reordering' })}
            >
              Reorder legacy releases
            </Button>
          )}
        </ButtonGroup>
      ) : (
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
        <p>
          All changes made to legacy releases appear immediately on the public
          website.
        </p>
      </ModalConfirm>

      <ModalConfirm
        confirmText="OK"
        open={!!confirmAction}
        title={getModalTitle(confirmAction)}
        onConfirm={() => {
          switch (confirmAction?.type) {
            case 'create': {
              history.push(
                generatePath<PublicationRouteParams>(
                  publicationCreateLegacyReleaseRoute.path,
                  {
                    publicationId,
                  },
                ),
              );
              break;
            }
            case 'edit': {
              if (confirmAction.id) {
                history.push(
                  generatePath<PublicationEditLegacyReleaseRouteParams>(
                    publicationEditLegacyReleaseRoute.path,
                    {
                      publicationId,
                      legacyReleaseId: confirmAction.id,
                    },
                  ),
                );
              }
              break;
            }
            case 'reordering':
              toggleReordering.on();
              break;
            default:
              break;
          }
          setConfirmAction(undefined);
        }}
        onExit={() => {
          setConfirmAction(undefined);
        }}
        onCancel={() => {
          setConfirmAction(undefined);
        }}
      >
        <WarningMessage>
          All changes made to legacy releases appear immediately on the public
          website.
        </WarningMessage>
      </ModalConfirm>
    </>
  );
};

export default LegacyReleasesTable;
