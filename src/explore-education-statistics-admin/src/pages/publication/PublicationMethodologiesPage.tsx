import Link from '@admin/components/Link';
import {
  MethodologyStatusGuidanceModal,
  MethodologyTypeGuidanceModal,
} from '@admin/pages/publication/components/PublicationMethodologyGuidance';
import usePublicationContext from '@admin/pages/publication/contexts/PublicationContext';
import {
  MethodologyRouteParams,
  methodologySummaryRoute,
} from '@admin/routes/methodologyRoutes';
import {
  PublicationRouteParams,
  publicationAdoptMethodologyRoute,
  publicationExternalMethodologyRoute,
} from '@admin/routes/publicationRoutes';
import methodologyService, {
  MethodologyVersionSummary,
} from '@admin/services/methodologyService';
import publicationService, {
  ExternalMethodology,
} from '@admin/services/publicationService';
import Button from '@common/components/Button';
import ButtonText from '@common/components/ButtonText';
import FormattedDate from '@common/components/FormattedDate';
import InfoIcon from '@common/components/InfoIcon';
import LoadingSpinner from '@common/components/LoadingSpinner';
import ModalConfirm from '@common/components/ModalConfirm';
import Tag from '@common/components/Tag';
import VisuallyHidden from '@common/components/VisuallyHidden';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import useToggle from '@common/hooks/useToggle';
import React, { useState } from 'react';
import { generatePath, useHistory } from 'react-router';

interface Model {
  externalMethodology?: ExternalMethodology;
  methodologyVersions: MethodologyVersionSummary[];
}

const PublicationMethodologiesPage = () => {
  const history = useHistory();
  const { publication, onReload } = usePublicationContext();
  const { permissions } = publication;

  const { value: model, isLoading } = useAsyncHandledRetry<Model>(async () => {
    const [externalMethodology, methodologyVersions] = await Promise.all([
      publicationService.getExternalMethodology(publication.id),
      methodologyService.listMethodologyVersions(publication.id),
    ]);

    return {
      externalMethodology,
      methodologyVersions,
    };
  }, [publication.id]);

  const [amendMethodologyId, setAmendMethodologyId] = useState<string>();
  const [deleteMethodologyDetails, setDeleteMethodologyDetails] = useState<{
    methodologyId: string;
    amendment: boolean;
  }>();
  const [dropMethodologyId, setDropMethodologyId] = useState<string>();
  const [removingExternalMethodology, toggleRemovingExternalMethodology] =
    useToggle(false);
  const [showMethodologyTypeGuidance, toggleMethodologyTypeGuidance] =
    useToggle(false);
  const [showMethodologyStatusGuidance, toggleMethodologyStatusGuidance] =
    useToggle(false);

  const handleRemoveExternalMethodology = async () => {
    if (!publication) {
      return;
    }
    await publicationService.removeExternalMethodology(publication.id);
    toggleRemovingExternalMethodology.off();
    onReload();
  };

  if (isLoading || !model) {
    return <LoadingSpinner />;
  }

  const { externalMethodology, methodologyVersions } = model;

  return (
    <>
      <h2>Manage methodologies</h2>
      <div className="govuk-grid-row  govuk-!-margin-bottom-6">
        <div className="govuk-grid-column-three-quarters">
          <p>
            Create a new methodology, view, edit or amend an existing
            methodology, select an existing methodology used in another
            publication or link to an external file that contains methodology
            details.
          </p>

          {permissions.canCreateMethodologies && (
            <Button
              className="govuk-!-margin-bottom-0"
              onClick={async () => {
                const { id: methodologyId } =
                  await methodologyService.createMethodology(publication.id);

                history.push(
                  generatePath<MethodologyRouteParams>(
                    methodologySummaryRoute.path,
                    {
                      methodologyId,
                    },
                  ),
                );
              }}
            >
              Create new methodology
            </Button>
          )}
        </div>
      </div>

      {methodologyVersions.length > 0 || externalMethodology ? (
        <table className="dfe-hide-empty-cells" data-testid="methodologies">
          <caption className="govuk-table__caption--m">
            Methodologies associated to this publication
          </caption>
          <thead>
            <tr>
              <th>Methodology</th>
              <th>
                Type{' '}
                <ButtonText onClick={toggleMethodologyTypeGuidance.on}>
                  <InfoIcon description="Guidance on methodology types" />
                </ButtonText>
              </th>
              <th>
                Status{' '}
                <ButtonText onClick={toggleMethodologyStatusGuidance.on}>
                  <InfoIcon description="Guidance on methodology statuses" />
                </ButtonText>
              </th>
              <th>Published date</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {methodologyVersions.map(methodology => {
              const canEdit =
                methodology.permissions.canApproveMethodology ||
                methodology.permissions.canMarkMethodologyAsDraft ||
                methodology.permissions.canUpdateMethodology;

              return (
                <tr key={methodology.id}>
                  <td>{methodology.title}</td>
                  <td>{methodology.owned ? 'Owned' : 'Adopted'}</td>
                  <td>
                    <Tag
                      colour={
                        methodology.status === 'Approved' &&
                        methodology.published
                          ? 'green'
                          : undefined
                      }
                    >
                      {`${
                        methodology.status === 'Approved' &&
                        methodology.published
                          ? 'Published'
                          : methodology.status
                      }${methodology.amendment ? ' Amendment' : ''}`}
                    </Tag>
                  </td>
                  <td>
                    {methodology.published ? (
                      <FormattedDate>{methodology.published}</FormattedDate>
                    ) : (
                      'Not yet published'
                    )}
                  </td>

                  <td>
                    <Link
                      className="govuk-!-margin-right-4 dfe-inline-block"
                      data-testid={canEdit ? 'edit' : 'view'}
                      to={generatePath<MethodologyRouteParams>(
                        methodologySummaryRoute.path,
                        {
                          methodologyId: methodology.id,
                        },
                      )}
                      unvisited
                    >
                      {canEdit ? 'Edit' : 'View'}
                      <VisuallyHidden>{` ${methodology.title}`}</VisuallyHidden>
                    </Link>

                    {methodology.owned && (
                      <>
                        {methodology.amendment &&
                          methodology.previousVersionId && (
                            <Link
                              className="govuk-!-margin-right-4 dfe-inline-block"
                              data-testid="view-existing-version"
                              to={generatePath<MethodologyRouteParams>(
                                methodologySummaryRoute.path,
                                {
                                  methodologyId: methodology.previousVersionId,
                                },
                              )}
                              unvisited
                            >
                              View existing version
                              <VisuallyHidden>
                                {` for ${methodology.title}`}
                              </VisuallyHidden>
                            </Link>
                          )}

                        {!methodology.amendment &&
                          methodology.permissions
                            .canMakeAmendmentOfMethodology && (
                            <ButtonText
                              data-testid="amend"
                              onClick={() =>
                                setAmendMethodologyId(methodology.id)
                              }
                            >
                              Amend
                              <VisuallyHidden>
                                {` ${methodology.title}`}
                              </VisuallyHidden>
                            </ButtonText>
                          )}

                        {methodology.permissions.canDeleteMethodology && (
                          <ButtonText
                            data-testid={
                              methodology.amendment
                                ? 'cancel-amendment'
                                : 'delete-draft'
                            }
                            variant="warning"
                            onClick={() =>
                              setDeleteMethodologyDetails({
                                methodologyId: methodology.id,
                                amendment: methodology.amendment,
                              })
                            }
                          >
                            {methodology.amendment ? (
                              <>
                                Cancel amendment
                                <VisuallyHidden>
                                  {` for ${methodology.title}`}
                                </VisuallyHidden>
                              </>
                            ) : (
                              <>
                                Delete draft
                                <VisuallyHidden>
                                  {` ${methodology.title}`}
                                </VisuallyHidden>
                              </>
                            )}
                          </ButtonText>
                        )}
                      </>
                    )}

                    {methodology.permissions.canRemoveMethodologyLink && (
                      <ButtonText
                        variant="warning"
                        onClick={() => {
                          setDropMethodologyId(methodology.methodologyId);
                        }}
                      >
                        Remove
                        <VisuallyHidden>
                          {` ${methodology.title}`}
                        </VisuallyHidden>
                      </ButtonText>
                    )}
                  </td>
                </tr>
              );
            })}
            {externalMethodology && (
              <tr>
                <td>{externalMethodology.title}</td>
                <td>External</td>
                <td />
                <td />
                <td>
                  {permissions.canManageExternalMethodology && (
                    <Link
                      className="govuk-!-margin-right-4"
                      to={generatePath<PublicationRouteParams>(
                        publicationExternalMethodologyRoute.path,
                        {
                          publicationId: publication.id,
                        },
                      )}
                      unvisited
                    >
                      Edit
                      <VisuallyHidden>
                        {` ${externalMethodology.title}`}
                      </VisuallyHidden>
                    </Link>
                  )}

                  <Link
                    className="govuk-!-margin-right-4"
                    to={externalMethodology.url}
                    unvisited
                  >
                    View
                    <VisuallyHidden>
                      {` ${externalMethodology.title}`}
                    </VisuallyHidden>
                  </Link>

                  {permissions.canManageExternalMethodology && (
                    <ButtonText
                      variant="warning"
                      onClick={toggleRemovingExternalMethodology.on}
                    >
                      Remove
                      <VisuallyHidden>
                        {` ${externalMethodology.title}`}
                      </VisuallyHidden>
                    </ButtonText>
                  )}
                </td>
              </tr>
            )}
          </tbody>
        </table>
      ) : (
        <p>There are no methodologies for this publication yet.</p>
      )}

      {((permissions.canManageExternalMethodology && !externalMethodology) ||
        permissions.canAdoptMethodologies) && (
        <>
          <h3>Other options</h3>

          {permissions.canManageExternalMethodology && !externalMethodology && (
            <>
              <Link
                to={generatePath<PublicationRouteParams>(
                  publicationExternalMethodologyRoute.path,
                  {
                    publicationId: publication.id,
                  },
                )}
              >
                Add external methodology
              </Link>
              <p>
                This is a link to an existing methodology that is hosted
                externally
              </p>
            </>
          )}

          {permissions.canAdoptMethodologies && (
            <>
              <Link
                to={generatePath<PublicationRouteParams>(
                  publicationAdoptMethodologyRoute.path,
                  {
                    publicationId: publication.id,
                  },
                )}
              >
                Adopt an existing methodology
              </Link>
              <p>This is a methodology that is owned by another publication</p>
            </>
          )}
        </>
      )}

      {amendMethodologyId && (
        <ModalConfirm
          open
          title="Confirm you want to amend this published methodology"
          onConfirm={async () => {
            const amendment =
              await methodologyService.createMethodologyAmendment(
                amendMethodologyId,
              );
            history.push(
              generatePath<MethodologyRouteParams>(
                methodologySummaryRoute.path,
                {
                  methodologyId: amendment.id,
                },
              ),
            );
          }}
          onExit={() => setAmendMethodologyId(undefined)}
          onCancel={() => setAmendMethodologyId(undefined)}
        >
          <p>
            Please note, any changes made to this published methodology must be
            approved before updates can be published.
          </p>
        </ModalConfirm>
      )}

      {deleteMethodologyDetails && (
        <ModalConfirm
          open
          title={
            deleteMethodologyDetails.amendment
              ? 'Confirm you want to cancel this amended methodology'
              : 'Confirm you want to delete this draft methodology'
          }
          onConfirm={async () => {
            await methodologyService.deleteMethodology(
              deleteMethodologyDetails?.methodologyId,
            );
            setDeleteMethodologyDetails(undefined);
            onReload();
          }}
          onCancel={() => setDeleteMethodologyDetails(undefined)}
          onExit={() => setDeleteMethodologyDetails(undefined)}
        >
          <p>
            {deleteMethodologyDetails.amendment ? (
              <>
                By cancelling the amendment you will lose any changes made, and
                the original methodology will remain unchanged.
              </>
            ) : (
              <>
                By deleting this draft methodology you will lose any changes
                made.
              </>
            )}
          </p>
        </ModalConfirm>
      )}

      {dropMethodologyId && (
        <ModalConfirm
          open
          title="Remove methodology"
          onConfirm={async () => {
            await publicationService.dropMethodology(
              publication.id,
              dropMethodologyId,
            );
            setDropMethodologyId(undefined);
            onReload();
          }}
          onCancel={() => setDropMethodologyId(undefined)}
          onExit={() => setDropMethodologyId(undefined)}
        >
          <p>Are you sure you want to remove this adopted methodology?</p>
        </ModalConfirm>
      )}

      <ModalConfirm
        open={removingExternalMethodology}
        title="Remove external methodology"
        onConfirm={async () => {
          await handleRemoveExternalMethodology();
        }}
        onCancel={toggleRemovingExternalMethodology.off}
        onExit={toggleRemovingExternalMethodology.off}
      >
        <p>Are you sure you want to remove this external methodology?</p>
      </ModalConfirm>

      <MethodologyStatusGuidanceModal
        open={showMethodologyStatusGuidance}
        onClose={toggleMethodologyStatusGuidance.off}
      />

      <MethodologyTypeGuidanceModal
        open={showMethodologyTypeGuidance}
        onClose={toggleMethodologyTypeGuidance.off}
      />
    </>
  );
};

export default PublicationMethodologiesPage;
