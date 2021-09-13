import Page from '@admin/components/Page';
import PageTitle from '@admin/components/PageTitle';
import LoadingSpinner from '@common/components/LoadingSpinner';
import { dashboardRoute } from '@admin/routes/routes';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import ExternalMethodologyForm from '@admin/pages/methodology/external-methodology/components/ExternalMethodologyForm';
import publicationService, {
  ExternalMethodology,
  UpdatePublicationRequest,
} from '@admin/services/publicationService';
import React from 'react';
import { RouteComponentProps } from 'react-router';

const ExternalMethodologyPage = ({
  history,
  match,
}: RouteComponentProps<{ publicationId: string }>) => {
  const { publicationId } = match.params;

  const { value: publication, isLoading } = useAsyncHandledRetry(
    async () => publicationService.getPublication(publicationId),
    [publicationId],
  );

  const handleExternalMethodologySubmit = async (
    values: ExternalMethodology,
  ) => {
    if (!publication) {
      return;
    }
    const updatedPublication: UpdatePublicationRequest = {
      title: publication.title,
      contact: {
        contactName: publication.contact?.contactName ?? '',
        contactTelNo: publication.contact?.contactTelNo ?? '',
        teamEmail: publication.contact?.teamEmail ?? '',
        teamName: publication.contact?.teamName ?? '',
      },
      topicId: publication.topicId,
      externalMethodology: {
        title: values.title,
        url: values.url,
      },
    };

    await publicationService.updatePublication(
      publicationId,
      updatedPublication,
    );
    history.push(dashboardRoute.path);
  };

  return (
    <Page
      breadcrumbs={
        isLoading
          ? []
          : [
              {
                name: publication?.externalMethodology
                  ? 'Edit external methodology link'
                  : 'Link to an externally hosted methodology',
              },
            ]
      }
    >
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-two-thirds">
          <LoadingSpinner loading={isLoading}>
            <PageTitle
              title={
                publication?.externalMethodology
                  ? 'Edit external methodology link'
                  : 'Link to an externally hosted methodology'
              }
              caption={publication?.title}
            />
            <ExternalMethodologyForm
              initialValues={publication?.externalMethodology}
              onCancel={() => history.push(dashboardRoute.path)}
              onSubmit={handleExternalMethodologySubmit}
            />
          </LoadingSpinner>
        </div>
      </div>
    </Page>
  );
};

export default ExternalMethodologyPage;
