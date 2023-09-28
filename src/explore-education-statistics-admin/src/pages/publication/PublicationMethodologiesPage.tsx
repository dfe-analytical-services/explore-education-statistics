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
import LoadingSpinner from '@common/components/LoadingSpinner';
import ModalConfirm from '@common/components/ModalConfirm';
import Tag from '@common/components/Tag';
import VisuallyHidden from '@common/components/VisuallyHidden';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import React from 'react';
import { generatePath, useHistory } from 'react-router';
import getMethodologyApprovalStatusLabel from '@admin/pages/methodology/utils/getMethodologyApprovalStatusLabel';

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
      methodologyService.listLatestMethodologyVersions(publication.id),
    ]);

    return {
      externalMethodology,
      methodologyVersions,
    };
  }, [publication.id]);

  const handleRemoveExternalMethodology = async () => {
    if (!publication) {
      return;
    }
    await publicationService.removeExternalMethodology(publication.id);
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
                Type <MethodologyTypeGuidanceModal />
              </th>
              <th>
                Status <MethodologyStatusGuidanceModal />
              </th>
              <th>Published date</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {methodologyVersions.map(methodology => {
              const canEdit =
                methodology.permissions.canApproveMethodology ||
                methodology.permissions.canSubmitMethodologyForHigherReview ||
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
                          : getMethodologyApprovalStatusLabel(
                              methodology.status,
                            )
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
                            <ModalConfirm
                              title="Confirm you want to amend this published methodology"
                              triggerButton={
                                <ButtonText data-testid="amend">
                                  Amend
                                  <VisuallyHidden>
                                    {` ${methodology.title}`}
                                  </VisuallyHidden>
                                </ButtonText>
                              }
                              onConfirm={async () => {
                                const amendment =
                                  await methodologyService.createMethodologyAmendment(
                                    methodology.id,
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
                            >
                              <p>
                                Please note, any changes made to this published
                                methodology must be approved before updates can
                                be published.
                              </p>
                            </ModalConfirm>
                          )}

                        {methodology.permissions.canDeleteMethodology && (
                          <ModalConfirm
                            title={
                              methodology.amendment
                                ? 'Confirm you want to cancel this amended methodology'
                                : 'Confirm you want to delete this draft methodology'
                            }
                            triggerButton={
                              <ButtonText
                                data-testid={
                                  methodology.amendment
                                    ? 'cancel-amendment'
                                    : 'delete-draft'
                                }
                                variant="warning"
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
                            }
                            onConfirm={async () => {
                              await methodologyService.deleteMethodology(
                                methodology?.id,
                              );
                              onReload();
                            }}
                          >
                            <p>
                              {methodology.amendment ? (
                                <>
                                  By cancelling the amendment you will lose any
                                  changes made, and the original methodology
                                  will remain unchanged.
                                </>
                              ) : (
                                <>
                                  By deleting this draft methodology you will
                                  lose any changes made.
                                </>
                              )}
                            </p>
                          </ModalConfirm>
                        )}
                      </>
                    )}

                    {methodology.permissions.canRemoveMethodologyLink && (
                      <ModalConfirm
                        title="Remove methodology"
                        triggerButton={
                          <ButtonText variant="warning">
                            Remove
                            <VisuallyHidden>
                              {` ${methodology.title}`}
                            </VisuallyHidden>
                          </ButtonText>
                        }
                        onConfirm={async () => {
                          await publicationService.dropMethodology(
                            publication.id,
                            methodology.methodologyId,
                          );

                          onReload();
                        }}
                      >
                        <p>
                          Are you sure you want to remove this adopted
                          methodology?
                        </p>
                      </ModalConfirm>
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
                    <ModalConfirm
                      title="Remove external methodology"
                      triggerButton={
                        <ButtonText variant="warning">
                          Remove
                          <VisuallyHidden>
                            {` ${externalMethodology.title}`}
                          </VisuallyHidden>
                        </ButtonText>
                      }
                      onConfirm={async () => {
                        await handleRemoveExternalMethodology();
                      }}
                    >
                      <p>
                        Are you sure you want to remove this external
                        methodology?
                      </p>
                    </ModalConfirm>
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
    </>
  );
};

export default PublicationMethodologiesPage;
