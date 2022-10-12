import ButtonLink from '@admin/components/ButtonLink';
import Link from '@admin/components/Link';
import publicationSummaryStyles from '@admin/pages/admin-dashboard/components/PublicationSummary.module.scss';
import {
  MethodologyRouteParams,
  methodologySummaryRoute,
} from '@admin/routes/methodologyRoutes';
import {
  externalMethodologyEditRoute,
  methodologyAdoptRoute,
  PublicationRouteParams,
} from '@admin/routes/routes';
import methodologyService from '@admin/services/methodologyService';
import publicationService, {
  MyPublication,
} from '@admin/services/publicationService';
import Button from '@common/components/Button';
import Details from '@common/components/Details';
import FormattedDate from '@common/components/FormattedDate';
import ModalConfirm from '@common/components/ModalConfirm';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import Tag from '@common/components/Tag';
import TagGroup from '@common/components/TagGroup';
import WarningMessage from '@common/components/WarningMessage';
import useToggle from '@common/hooks/useToggle';
import React, { useState } from 'react';
import { generatePath, useHistory } from 'react-router';
import classNames from 'classnames';

export interface Props {
  publication: MyPublication;
  onChangePublication: () => void;
}

const MethodologySummary = ({ publication, onChangePublication }: Props) => {
  const history = useHistory();
  const [amendMethodologyId, setAmendMethodologyId] = useState<string>();
  const [deleteMethodologyDetails, setDeleteMethodologyDetails] = useState<{
    methodologyId: string;
    amendment: boolean;
  }>();
  const [dropMethodologyId, setDropMethodologyId] = useState<string>();
  const [
    removingExternalMethodology,
    toggleRemovingExternalMethodology,
  ] = useToggle(false);

  const {
    externalMethodology,
    methodologies,
    id: publicationId,
    title,
  } = publication;

  const handleRemoveExternalMethodology = async () => {
    await publicationService.removeExternalMethodology(publicationId);
    toggleRemovingExternalMethodology.off();
    onChangePublication();
  };

  const canAdoptOrManageExternal =
    publication.permissions.canManageExternalMethodology ||
    publication.permissions.canAdoptMethodologies;

  return (
    <>
      {methodologies.length > 0 || externalMethodology?.url ? (
        <ul className="govuk-list govuk-!-margin-top-2">
          {methodologies.map(methodology => {
            const canEdit =
              methodology.permissions.canApproveMethodology ||
              methodology.permissions.canMarkMethodologyAsDraft ||
              methodology.permissions.canUpdateMethodology;

            const displayTitle = methodology.owned
              ? `${methodology.title} (Owned)`
              : `${methodology.title} (Adopted)`;

            return (
              <li key={methodology.id}>
                <Details
                  open={false}
                  className="govuk-!-margin-bottom-0"
                  summary={displayTitle}
                  summaryAfter={
                    <TagGroup className="govuk-!-margin-left-2">
                      <Tag>
                        {methodology.published &&
                        methodology.status === 'Approved'
                          ? 'Published'
                          : methodology.status}
                      </Tag>
                      {methodology.amendment && <Tag>Amendment</Tag>}
                    </TagGroup>
                  }
                >
                  <div className={publicationSummaryStyles.detailsInner}>
                    <div className={publicationSummaryStyles.sectionContent}>
                      <SummaryList className="govuk-!-margin-bottom-3">
                        <SummaryListItem term="Publish date">
                          {methodology.published ? (
                            <FormattedDate>
                              {methodology.published}
                            </FormattedDate>
                          ) : (
                            'Not yet published'
                          )}
                        </SummaryListItem>
                        {methodology.internalReleaseNote && (
                          <SummaryListItem term="Internal release note">
                            {methodology.internalReleaseNote}
                          </SummaryListItem>
                        )}
                      </SummaryList>
                    </div>
                    <div
                      className={classNames(
                        publicationSummaryStyles.sectionActions,
                        publicationSummaryStyles.detailsActions,
                      )}
                    >
                      {methodology.amendment ? (
                        <>
                          <ButtonLink
                            to={generatePath<MethodologyRouteParams>(
                              methodologySummaryRoute.path,
                              {
                                methodologyId: methodology.id,
                              },
                            )}
                            variant="secondary"
                          >
                            {canEdit ? 'Edit amendment' : 'View amendment'}
                          </ButtonLink>

                          {methodology.previousVersionId && (
                            <ButtonLink
                              to={generatePath<MethodologyRouteParams>(
                                methodologySummaryRoute.path,
                                {
                                  methodologyId: methodology.previousVersionId,
                                },
                              )}
                              variant="secondary"
                            >
                              View existing version
                            </ButtonLink>
                          )}
                        </>
                      ) : (
                        <>
                          <ButtonLink
                            to={generatePath<MethodologyRouteParams>(
                              methodologySummaryRoute.path,
                              {
                                methodologyId: methodology.id,
                              },
                            )}
                            variant="secondary"
                          >
                            {canEdit ? 'Edit methodology' : 'View methodology'}
                          </ButtonLink>
                          {methodology.permissions
                            .canMakeAmendmentOfMethodology && (
                            <Button
                              variant="secondary"
                              onClick={() =>
                                setAmendMethodologyId(methodology.id)
                              }
                            >
                              Amend methodology
                            </Button>
                          )}
                        </>
                      )}
                      {methodology.permissions.canDeleteMethodology && (
                        <Button
                          variant="warning"
                          onClick={() =>
                            setDeleteMethodologyDetails({
                              methodologyId: methodology.id,
                              amendment: methodology.amendment,
                            })
                          }
                        >
                          {methodology.amendment
                            ? 'Cancel amendment'
                            : 'Remove'}
                        </Button>
                      )}
                      {methodology.permissions.canRemoveMethodologyLink && (
                        <Button
                          variant="warning"
                          onClick={() => {
                            setDropMethodologyId(methodology.methodologyId);
                          }}
                        >
                          Remove methodology
                        </Button>
                      )}
                    </div>
                  </div>
                </Details>
              </li>
            );
          })}

          {externalMethodology?.url && (
            <li>
              <Details
                open={false}
                className="govuk-!-margin-bottom-0"
                summary={`${externalMethodology.title} (External)`}
              >
                <div className={publicationSummaryStyles.detailsInner}>
                  <div className={publicationSummaryStyles.sectionContent}>
                    <SummaryList className="govuk-!-margin-bottom-3">
                      <SummaryListItem term="URL">
                        <Link to={externalMethodology.url} unvisited>
                          {externalMethodology.url}
                        </Link>
                      </SummaryListItem>
                    </SummaryList>
                  </div>
                  <div
                    className={classNames(
                      publicationSummaryStyles.sectionActions,
                      publicationSummaryStyles.detailsActions,
                    )}
                  >
                    {publication.permissions.canManageExternalMethodology && (
                      <>
                        <ButtonLink
                          to={generatePath<PublicationRouteParams>(
                            externalMethodologyEditRoute.path,
                            {
                              publicationId,
                            },
                          )}
                          variant="secondary"
                        >
                          Edit external methodology
                        </ButtonLink>

                        <Button
                          variant="warning"
                          onClick={toggleRemovingExternalMethodology.on}
                        >
                          Remove external methodology
                        </Button>
                      </>
                    )}
                  </div>
                </div>
              </Details>
            </li>
          )}
        </ul>
      ) : (
        <WarningMessage className="govuk-!-margin-bottom-2">
          No methodologies added
        </WarningMessage>
      )}

      {publication.permissions.canCreateMethodologies && (
        <Button
          data-testid={`Create methodology for ${title}`}
          onClick={async () => {
            const {
              id: methodologyId,
            } = await methodologyService.createMethodology(publicationId);
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
          Create methodology
        </Button>
      )}

      {canAdoptOrManageExternal && (
        <p className="govuk-!-margin-bottom-0">
          Or alternatively:{' '}
          {publication.permissions.canManageExternalMethodology &&
            !externalMethodology && (
              <Link
                to={generatePath<PublicationRouteParams>(
                  externalMethodologyEditRoute.path,
                  {
                    publicationId,
                  },
                )}
              >
                Use an external methodology
              </Link>
            )}
          {publication.permissions.canManageExternalMethodology &&
          publication.permissions.canAdoptMethodologies &&
          !publication.externalMethodology
            ? ' or '
            : null}
          {publication.permissions.canAdoptMethodologies && (
            <Link
              to={generatePath<PublicationRouteParams>(
                methodologyAdoptRoute.path,
                {
                  publicationId,
                },
              )}
            >
              Adopt an existing methodology
            </Link>
          )}
        </p>
      )}

      {amendMethodologyId && (
        <ModalConfirm
          open
          title="Confirm you want to amend this live methodology"
          onConfirm={async () => {
            const amendment = await methodologyService.createMethodologyAmendment(
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
            Please note, any changes made to this live methodology must be
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
              : 'Confirm you want to remove this methodology'
          }
          onConfirm={async () => {
            await methodologyService.deleteMethodology(
              deleteMethodologyDetails?.methodologyId,
            );
            setDeleteMethodologyDetails(undefined);
            onChangePublication();
          }}
          onCancel={() => setDeleteMethodologyDetails(undefined)}
          onExit={() => setDeleteMethodologyDetails(undefined)}
        >
          <p>
            {deleteMethodologyDetails.amendment ? (
              <>
                By cancelling the amendments you will lose any changes made, and
                the original methodology will remain unchanged.
              </>
            ) : (
              <>By removing this methodology you will lose any changes made.</>
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
              publicationId,
              dropMethodologyId,
            );
            setDropMethodologyId(undefined);
            onChangePublication();
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
    </>
  );
};

export default MethodologySummary;
