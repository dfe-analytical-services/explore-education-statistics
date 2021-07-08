import ButtonLink from '@admin/components/ButtonLink';
import Link from '@admin/components/Link';
import MethodologyExternalLinkForm from '@admin/pages/admin-dashboard/components/MethodologyExternalLinkForm';
import {
  MethodologyRouteParams,
  methodologySummaryRoute,
} from '@admin/routes/methodologyRoutes';
import methodologyService from '@admin/services/methodologyService';
import publicationService, {
  ExternalMethodology,
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
import React, { useState } from 'react';
import { generatePath, useHistory } from 'react-router';

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
  const [
    showAddEditExternalMethodologyForm,
    setShowAddEditExternalMethodologyForm,
  ] = useState<boolean>();
  const [amendMethodologyId, setAmendMethodologyId] = useState<string>();
  const [deleteMethodologyDetails, setDeleteMethodologyDetails] = useState<{
    methodologyId: string;
    amendment: boolean;
  }>();

  const {
    contact,
    externalMethodology,
    methodologies,
    id: publicationId,
    title,
  } = publication;

  const handleExternalMethodologySubmit = async (
    values: ExternalMethodology,
  ) => {
    const updatedPublication = {
      title,
      contact: {
        contactName: contact?.contactName ?? '',
        contactTelNo: contact?.contactTelNo ?? '',
        teamEmail: contact?.teamEmail ?? '',
        teamName: contact?.teamName ?? '',
      },
      topicId,
      externalMethodology: {
        title: values.title,
        url: values.url,
      },
    };

    await publicationService.updatePublication(
      publicationId,
      updatedPublication,
    );
    onChangePublication();
  };

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
    onChangePublication();
  };

  const hasMethodologyPermissions =
    publication.permissions.canCreateMethodologies ||
    publication.permissions.canManageExternalMethodology;

  return (
    <>
      {publication.permissions.canCreateMethodologies && (
        <ButtonGroup className="govuk-!-margin-bottom-2">
          <Button
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
            data-testid={`Create methodology for ${title}`}
            disabled={showAddEditExternalMethodologyForm}
          >
            Create methodology
          </Button>

          {publication.permissions.canManageExternalMethodology && (
            <Button
              type="button"
              data-testid={`Link methodology for ${title}`}
              variant="secondary"
              onClick={() => setShowAddEditExternalMethodologyForm(true)}
              disabled={showAddEditExternalMethodologyForm}
            >
              Link to an externally hosted methodology
            </Button>
          )}
        </ButtonGroup>
      )}

      {externalMethodology?.url && (
        <>
          <Link to={externalMethodology.url} unvisited>
            {externalMethodology.title} (external methodology)
          </Link>
          {publication.permissions.canManageExternalMethodology && (
            <ButtonGroup className="govuk-!-margin-bottom-2 govuk-!-margin-top-2">
              <Button
                type="button"
                onClick={() => setShowAddEditExternalMethodologyForm(true)}
              >
                Edit
              </Button>
              <Button
                type="button"
                variant="warning"
                onClick={handleRemoveExternalMethodology}
              >
                Remove
              </Button>
            </ButtonGroup>
          )}
        </>
      )}

      {showAddEditExternalMethodologyForm && (
        <MethodologyExternalLinkForm
          initialValues={externalMethodology}
          onCancel={() => setShowAddEditExternalMethodologyForm(false)}
          onSubmit={values => {
            handleExternalMethodologySubmit(values);
            setShowAddEditExternalMethodologyForm(false);
          }}
        />
      )}

      {methodologies.length > 0 &&
        methodologies.map(methodology => (
          <Details
            key={methodology.id}
            open={false}
            className="govuk-!-margin-bottom-0"
            summary={methodology.title}
            summaryAfter={
              <TagGroup className="govuk-!-margin-left-2">
                <Tag>{methodology.status}</Tag>
                {methodology.amendment && <Tag>Amendment</Tag>}
              </TagGroup>
            }
          >
            <SummaryList className="govuk-!-margin-bottom-3">
              <SummaryListItem term="Publish date">
                <FormattedDate>{methodology.published || ''}</FormattedDate>
              </SummaryListItem>
              {methodology.internalReleaseNote && (
                <SummaryListItem term="Internal release note">
                  {methodology.internalReleaseNote}
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
                        {methodology.permissions.canUpdateMethodology
                          ? 'Edit this amendment'
                          : 'View this amendment'}
                      </ButtonLink>
                      <ButtonLink
                        to={generatePath(methodologySummaryRoute.path, {
                          publicationId,
                          methodologyId: methodology.previousVersionId,
                        })}
                        className="govuk-button--secondary govuk-!-margin-left-4"
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
                        {methodology.permissions.canUpdateMethodology
                          ? 'Edit this methodology'
                          : 'View this methodology'}
                      </ButtonLink>
                      {methodology.permissions
                        .canMakeAmendmentOfMethodology && (
                        <Button
                          type="button"
                          onClick={() => setAmendMethodologyId(methodology.id)}
                          variant="secondary"
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
                    onClick={() =>
                      setDeleteMethodologyDetails({
                        methodologyId: methodology.id,
                        amendment: methodology.amendment,
                      })
                    }
                    className="govuk-button--warning"
                  >
                    {methodology.amendment ? 'Cancel amendment' : 'Remove'}
                  </Button>
                )}
              </div>
            </div>
          </Details>
        ))}

      {methodologies.length === 0 && !hasMethodologyPermissions && (
        <>No methodologies added.</>
      )}

      {amendMethodologyId && (
        <ModalConfirm
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
          open
        >
          <p>
            Please note, any changes made to this live methodology must be
            approved before updates can be published.
          </p>
        </ModalConfirm>
      )}
      {deleteMethodologyDetails && (
        <ModalConfirm
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
          open
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
    </>
  );
};

export default MethodologySummary;
