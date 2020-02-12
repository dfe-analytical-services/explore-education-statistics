import Page from '@admin/components/Page';
import service from '@admin/services/common/service.ts';
import withErrorControl, {
  ErrorControlProps,
} from '@admin/validation/withErrorControl';
import LoadingSpinner from '@common/components/LoadingSpinner';
import React, { useEffect, useState } from 'react';
import { RouteComponentProps } from 'react-router';
import { BasicPublicationDetails } from 'src/services/common/types';
import AssignMethodologyForm from './AssignMethodologyForm';

const PublicationAssignMethodologyPage = ({
  match,
  handleApiErrors,
}: RouteComponentProps<{
  themeId: string;
  topicId: string;
  publicationId: string;
}> &
  ErrorControlProps) => {
  const [publication, setPublication] = useState<BasicPublicationDetails>();

  const getPublication = () => {
    service
      .getBasicPublicationDetails(match.params.publicationId)
      .then(setPublication)
      .catch(handleApiErrors);
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
          <div className="govuk-grid-row">
            <div>
              <h1 className="govuk-heading-xl">
                <span className="govuk-caption-xl">{publication.title}</span>
                Assign methodology
              </h1>
            </div>
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

export default withErrorControl(PublicationAssignMethodologyPage);
