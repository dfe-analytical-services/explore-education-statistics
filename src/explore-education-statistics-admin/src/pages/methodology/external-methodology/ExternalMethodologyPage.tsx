import Page from '@admin/components/Page';
import PageTitle from '@admin/components/PageTitle';
import LoadingSpinner from '@common/components/LoadingSpinner';
import { dashboardRoute } from '@admin/routes/routes';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import ExternalMethodologyForm from '@admin/pages/methodology/external-methodology/components/ExternalMethodologyForm';
import publicationService, {
  ExternalMethodology,
  Publication,
} from '@admin/services/publicationService';
import React from 'react';
import { RouteComponentProps } from 'react-router';

interface Model {
  publication: Publication;
  externalMethodology?: ExternalMethodology;
}

const ExternalMethodologyPage = ({
  history,
  match,
}: RouteComponentProps<{ publicationId: string }>) => {
  const { publicationId } = match.params;

  const { value: model, isLoading } = useAsyncHandledRetry<Model>(async () => {
    const [publication, externalMethodology] = await Promise.all([
      publicationService.getPublication(publicationId),
      publicationService.getExternalMethodology(publicationId),
    ]);

    return { publication, externalMethodology };
  }, [publicationId]);

  const handleExternalMethodologySubmit = async (
    values: ExternalMethodology,
  ) => {
    if (!model?.publication) {
      return;
    }
    const updatedExternalMethodology: ExternalMethodology = {
      title: values.title,
      url: values.url,
    };

    await publicationService.updateExternalMethodology(
      publicationId,
      updatedExternalMethodology,
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
                name: model?.externalMethodology
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
                model?.externalMethodology
                  ? 'Edit external methodology link'
                  : 'Link to an externally hosted methodology'
              }
              caption={model?.publication.title}
            />
            <ExternalMethodologyForm
              initialValues={model?.externalMethodology}
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
