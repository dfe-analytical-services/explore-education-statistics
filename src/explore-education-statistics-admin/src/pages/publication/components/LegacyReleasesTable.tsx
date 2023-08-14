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
  const [confirmAction, setConfirmAction] = useState<ConfirmAction>();
  const [deleteLegacyRelease, setDeleteLegacyRelease] =
    useState<LegacyRelease>();

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
