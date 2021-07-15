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
  const [
    cancelAmendmentMethodologyId,
    setCancelAmendmentMethodologyId,
  ] = useState<string>();

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
      {hasMethodologyPermissions && (
        <ButtonGroup className="govuk-!-margin-bottom-2">
          {publication.permissions.canCreateMethodologies && (
            <Button
              data-testid={`Create methodology for ${title}`}
              disabled={showAddEditExternalMethodologyForm}
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
              <Button
                type="button"
                data-testid={`Link methodology for ${title}`}
                variant="secondary"
                disabled={showAddEditExternalMethodologyForm}
                onClick={() => setShowAddEditExternalMethodologyForm(true)}
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
                Edit externally hosted methodology
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
                {methodology.permissions.canCancelMethodologyAmendment &&
                  methodology.amendment && (
                    <Button
                      className="govuk-button--warning"
                      onClick={() =>
                        setCancelAmendmentMethodologyId(methodology.id)
                      }
                    >
                      Cancel amendment
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
      {cancelAmendmentMethodologyId && (
        <ModalConfirm
          title="Confirm you want to cancel this amended methodology"
          onConfirm={async () => {
            await methodologyService.deleteMethodology(
              cancelAmendmentMethodologyId,
            );
            setCancelAmendmentMethodologyId(undefined);
            onChangePublication();
          }}
          onCancel={() => setCancelAmendmentMethodologyId(undefined)}
          onExit={() => setCancelAmendmentMethodologyId(undefined)}
          open
        >
          <p>
            By cancelling the amendments you will lose any changes made, and the
            original methodology will remain unchanged.
          </p>
        </ModalConfirm>
      )}
    </>
  );
};

export default MethodologySummary;
