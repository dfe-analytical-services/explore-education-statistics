import ButtonLink from '@admin/components/ButtonLink';
import Link from '@admin/components/Link';
import {
  MethodologyRouteParams,
  methodologySummaryRoute,
} from '@admin/routes/methodologyRoutes';
import {
  externalMethodologyEditRoute,
  methodologyAdoptRoute,
} from '@admin/routes/routes';
import methodologyService from '@admin/services/methodologyService';
import publicationService, {
  MyPublication,
} from '@admin/services/publicationService';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import Details from '@common/components/Details';
import FormattedDate from '@common/components/FormattedDate';
import ModalConfirm from '@common/components/ModalConfirm';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import Tag from '@common/components/Tag';
import TagGroup from '@common/components/TagGroup';
import useToggle from '@common/hooks/useToggle';
import React, { useState } from 'react';
import { generatePath, useHistory } from 'react-router';
import classNames from 'classnames';

export interface Props {
  publication: MyPublication;
  topicId: string;
  onChangePublication: () => void;
}

const MethodologySummary = ({
  publication,
  topicId,
  onChangePublication,
}: Props) => {
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
    contact,
    externalMethodology,
    methodologies,
    id: publicationId,
    title,
  } = publication;

  const handleRemoveExternalMethodology = async () => {
    const updatedPublication = {
      title,
      contact: {
        contactName: contact?.contactName ?? '',
        contactTelNo: contact?.contactTelNo ?? '',
        teamEmail: contact?.teamEmail ?? '',
        teamName: contact?.teamName ?? '',
      },
      topicId,
    };

    await publicationService.updatePublication(
      publicationId,
      updatedPublication,
    );
    toggleRemovingExternalMethodology.off();
    onChangePublication();
  };

  const canCreateAdoptOrManageExternal =
    publication.permissions.canCreateMethodologies ||
    publication.permissions.canManageExternalMethodology ||
    publication.permissions.canAdoptMethodologies;

  return (
    <>
      {methodologies.map(publicationMethodologyLink => {
        const { owner, permissions, methodology } = publicationMethodologyLink;

        const canEdit =
          methodology.permissions.canApproveMethodology ||
          methodology.permissions.canMarkMethodologyAsDraft ||
          methodology.permissions.canUpdateMethodology;

        const displayTitle = owner
          ? `${methodology.title} (Owned)`
          : `${methodology.title} (Adopted)`;

        return (
          <Details
            key={methodology.id}
            open={false}
            className="govuk-!-margin-bottom-0"
            summary={displayTitle}
            summaryAfter={
              <TagGroup className="govuk-!-margin-left-2">
                <Tag>{methodology.status}</Tag>
                {methodology.amendment && <Tag>Amendment</Tag>}
              </TagGroup>
            }
          >
            <SummaryList className="govuk-!-margin-bottom-3">
              <SummaryListItem term="Publish date">
                {methodology.published ? (
                  <FormattedDate>{methodology.published}</FormattedDate>
                ) : (
                  'Not yet published'
                )}
              </SummaryListItem>
              {methodology.latestInternalReleaseNote && (
                <SummaryListItem term="Internal release note">
                  {methodology.latestInternalReleaseNote}
                </SummaryListItem>
              )}
            </SummaryList>

            <div className="govuk-grid-row">
              <div className="govuk-grid-column-two-thirds">
                {methodology.amendment ? (
                  <>
                    <ButtonGroup>
                      <ButtonLink
                        to={generatePath(methodologySummaryRoute.path, {
                          publicationId,
                          methodologyId: methodology.id,
                        })}
                      >
                        {canEdit
                          ? 'Edit this amendment'
                          : 'View this amendment'}
                      </ButtonLink>
                      <ButtonLink
                        to={generatePath(methodologySummaryRoute.path, {
                          publicationId,
                          methodologyId: methodology.previousVersionId,
                        })}
                        className="govuk-!-margin-left-4"
                        variant="secondary"
                      >
                        View original methodology
                      </ButtonLink>
                    </ButtonGroup>
                  </>
                ) : (
                  <>
                    <ButtonGroup>
                      <ButtonLink
                        to={generatePath(methodologySummaryRoute.path, {
                          publicationId,
                          methodologyId: methodology.id,
                        })}
                      >
                        {canEdit
                          ? 'Edit this methodology'
                          : 'View this methodology'}
                      </ButtonLink>
                      {methodology.permissions
                        .canMakeAmendmentOfMethodology && (
                        <Button
                          type="button"
                          variant="secondary"
                          onClick={() => setAmendMethodologyId(methodology.id)}
                        >
                          Amend methodology
                        </Button>
                      )}
                    </ButtonGroup>
                  </>
                )}
              </div>
              <div className="govuk-grid-column-one-third dfe-align--right">
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
                    {methodology.amendment ? 'Cancel amendment' : 'Remove'}
                  </Button>
                )}
              </div>
            </div>

            {permissions.canDropMethodology && (
              <div className="govuk-grid-row">
                <div className="govuk-grid-column-two-thirds">
                  <Button
                    variant="warning"
                    onClick={() => {
                      setDropMethodologyId(methodology.methodologyId);
                    }}
                  >
                    Remove methodology
                  </Button>
                </div>
              </div>
            )}
          </Details>
        );
      })}

      {externalMethodology?.url && (
        <Details
          open={false}
          className="govuk-!-margin-bottom-0"
          summary={`${externalMethodology.title} (External)`}
        >
          <SummaryList className="govuk-!-margin-bottom-3">
            <SummaryListItem term="URL">
              <Link to={externalMethodology.url} unvisited>
                {externalMethodology.url}
              </Link>
            </SummaryListItem>
          </SummaryList>
          {publication.permissions.canManageExternalMethodology && (
            <div className="govuk-grid-row">
              <div className="govuk-grid-column-one-half">
                <ButtonLink
                  to={generatePath(externalMethodologyEditRoute.path, {
                    publicationId,
                  })}
                >
                  Edit external methodology
                </ButtonLink>
              </div>
              <div className="govuk-grid-column-one-half dfe-align--right">
                <Button
                  type="button"
                  variant="warning"
                  onClick={toggleRemovingExternalMethodology.on}
                >
                  Remove external methodology
                </Button>
              </div>
            </div>
          )}
        </Details>
      )}

      {methodologies.length === 0 && !canCreateAdoptOrManageExternal && (
        <>No methodologies added.</>
      )}

      {canCreateAdoptOrManageExternal && (
        <ButtonGroup
          className={classNames('govuk-!-margin-bottom-2', {
            'govuk-!-margin-top-2': methodologies.length > 0,
          })}
        >
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

          {publication.permissions.canManageExternalMethodology &&
            !externalMethodology && (
              <ButtonLink
                to={generatePath(externalMethodologyEditRoute.path, {
                  publicationId,
                })}
                variant="secondary"
              >
                Link to an externally hosted methodology
              </ButtonLink>
            )}

          {publication.permissions.canAdoptMethodologies && (
            <ButtonLink
              to={generatePath(methodologyAdoptRoute.path, {
                publicationId,
              })}
              variant="secondary"
            >
              Adopt a methodology
            </ButtonLink>
          )}
        </ButtonGroup>
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
