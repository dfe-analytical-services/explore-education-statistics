import ButtonLink from '@admin/components/ButtonLink';
import Link from '@admin/components/Link';
import {
  MethodologyRouteParams,
  methodologySummaryEditRoute,
  methodologySummaryRoute,
} from '@admin/routes/methodologyRoutes';
import methodologyService from '@admin/services/methodologyService';
import publicationService, { ExternalMethodology, MyPublication } from '@admin/services/publicationService';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import Details from '@common/components/Details';
import FormattedDate from '@common/components/FormattedDate';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import Tag from '@common/components/Tag';
import TagGroup from '@common/components/TagGroup';
import React, { useState } from 'react';
import { generatePath, useHistory } from 'react-router';
import MethodologyExternalLinkForm from './MethodologyExternalLinkForm';

export interface Props {
  canAmendMethodology?: boolean;
  publication: MyPublication;
  topicId: string;
  onChangePublication: () => void;
}

const MethodologySummary = ({
  canAmendMethodology = false, // TO DO replace with real permission check
  publication,
  topicId,
  onChangePublication,
}: Props) => {
  const [
    showAddExternalMethodologyForm,
    setShowAddExternalMethodologyForm,
  ] = useState<boolean>();
  const [
    showEditExternalMethodologyForm,
    setShowEditExternalMethodologyForm,
  ] = useState<boolean>();

  const {
    contact,
    externalMethodology,
    methodology,
    id: publicationId,
    title,
  } = publication;

  const history = useHistory();

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

  return (
    <>
      {methodology ? (
        <Details
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
          <ButtonGroup>
            <ButtonLink
              to={generatePath(methodologySummaryRoute.path, {
                publicationId,
                methodologyId: methodology.id,
              })}
            >
              View methodology
            </ButtonLink>
            {canAmendMethodology && (
              <ButtonLink
                to={generatePath(methodologySummaryEditRoute.path, {
                  publicationId,
                  methodologyId: methodology.id,
                })}
                variant="secondary"
              >
                Amend methodology
              </ButtonLink>
            )}
          </ButtonGroup>
        </Details>
      ) : (
        <>
          {externalMethodology?.url ? (
            <>
              {!showEditExternalMethodologyForm && (
                <>
                  <Link to={externalMethodology.url} unvisited>
                    {externalMethodology.title} (external methodology)
                  </Link>
                  <ButtonGroup className="govuk-!-margin-bottom-2 govuk-!-margin-top-2">
                    <Button
                      type="button"
                      onClick={() => setShowEditExternalMethodologyForm(true)}
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
                </>
              )}
              {showEditExternalMethodologyForm && (
                <MethodologyExternalLinkForm
                  initialValues={externalMethodology}
                  onCancel={() => setShowEditExternalMethodologyForm(false)}
                  onSubmit={values => {
                    handleExternalMethodologySubmit(values);
                    setShowEditExternalMethodologyForm(false);
                  }}
                />
              )}
            </>
          ) : (
            <>
              {!showAddExternalMethodologyForm && (
                <ButtonGroup className="govuk-!-margin-bottom-2">
                  <Button
                    onClick={async () => {
                      const {
                        id: methodologyId,
                      } = await methodologyService.createMethodology(
                        publicationId,
                      );
                      history.push(
                        generatePath<MethodologyRouteParams>(
                          methodologySummaryRoute.path,
                          {
                            publicationId,
                            methodologyId,
                          },
                        ),
                      );
                    }}
                    data-testid={`Create methodology for ${title}`}
                  >
                    Create methodology
                  </Button>
                  <Button
                    type="button"
                    data-testid={`Link methodology for ${title}`}
                    variant="secondary"
                    onClick={() => setShowAddExternalMethodologyForm(true)}
                  >
                    Link to an externally hosted methodology
                  </Button>
                </ButtonGroup>
              )}
              {showAddExternalMethodologyForm && (
                <MethodologyExternalLinkForm
                  onCancel={() => setShowAddExternalMethodologyForm(false)}
                  onSubmit={values => {
                    handleExternalMethodologySubmit(values);
                    setShowAddExternalMethodologyForm(false);
                  }}
                />
              )}
            </>
          )}
        </>
      )}
    </>
  );
};

export default MethodologySummary;
