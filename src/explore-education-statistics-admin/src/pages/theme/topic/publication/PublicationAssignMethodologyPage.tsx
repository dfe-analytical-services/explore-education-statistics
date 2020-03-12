import Page from '@admin/components/Page';
import service from '@admin/services/common/service.ts';
import { BasicPublicationDetails } from '@admin/services/common/types';
import LoadingSpinner from '@common/components/LoadingSpinner';
import React, { useEffect, useState } from 'react';
import { RouteComponentProps } from 'react-router';
import AssignMethodologyForm from './AssignMethodologyForm';

const PublicationAssignMethodologyPage = ({
  match,
}: RouteComponentProps<{
  themeId: string;
  topicId: string;
  publicationId: string;
}>) => {
  const [publication, setPublication] = useState<BasicPublicationDetails>();

  const getPublication = () => {
    service
      .getBasicPublicationDetails(match.params.publicationId)
      .then(setPublication);
  };
  useEffect(getPublication, [match.params.publicationId]);

  return (
    <Page
      wide
      breadcrumbs={[
        {
          name: 'Assign methodology',
        },
      ]}
    >
      {publication ? (
        <>
          <div>
            <h1 className="govuk-heading-xl">
              <span className="govuk-caption-xl">{publication.title}</span>
              Assign methodology
            </h1>
          </div>

          <AssignMethodologyForm
            methodology={publication.methodology}
            externalMethodology={publication.externalMethodology}
            publicationId={match.params.publicationId}
            refreshPublication={getPublication}
          />
        </>
      ) : (
        <LoadingSpinner />
      )}
    </Page>
  );
};

export default PublicationAssignMethodologyPage;
