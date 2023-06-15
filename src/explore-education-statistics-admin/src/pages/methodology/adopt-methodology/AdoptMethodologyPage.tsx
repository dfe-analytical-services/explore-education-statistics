import Page from '@admin/components/Page';
import PageTitle from '@admin/components/PageTitle';
import publicationService from '@admin/services/publicationService';
import LoadingSpinner from '@common/components/LoadingSpinner';
import { dashboardRoute } from '@admin/routes/routes';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import AdoptMethodologyForm from '@admin/pages/methodology/adopt-methodology/components/AdoptMethodologyForm';
import { useHistory, useParams } from 'react-router';
import React from 'react';

const AdoptMethodologyPage = () => {
  const { publicationId } = useParams<{ publicationId: string }>();
  const history = useHistory();

  const { value, isLoading } = useAsyncHandledRetry(async () => {
    const [adoptableMethodologies, publication] = await Promise.all([
      publicationService.getAdoptableMethodologies(publicationId),
      publicationService.getPublication(publicationId),
    ]);

    return {
      adoptableMethodologies,
      publication,
    };
  }, [publicationId]);

  const { adoptableMethodologies, publication } = value ?? {};

  return (
    <Page breadcrumbs={[{ name: 'Adopt a methodology' }]}>
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-two-thirds">
          <PageTitle title="Adopt a methodology" caption={publication?.title} />
          <LoadingSpinner loading={isLoading}>
            {adoptableMethodologies && adoptableMethodologies.length > 0 ? (
              <AdoptMethodologyForm
                methodologies={adoptableMethodologies}
                onCancel={() => history.push(dashboardRoute.path)}
                onSubmit={async values => {
                  await publicationService.adoptMethodology(
                    publicationId,
                    values.methodologyId,
                  );
                  history.push(dashboardRoute.path);
                }}
              />
            ) : (
              <p>No methodologies available.</p>
            )}
          </LoadingSpinner>
        </div>
      </div>
    </Page>
  );
};

export default AdoptMethodologyPage;
