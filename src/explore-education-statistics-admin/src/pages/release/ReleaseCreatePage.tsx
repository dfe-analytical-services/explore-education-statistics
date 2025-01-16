import Page from '@admin/components/Page';
import PageTitle from '@admin/components/PageTitle';
import ReleaseSummaryForm, {
  ReleaseSummaryFormValues,
} from '@admin/pages/release/components/ReleaseSummaryForm';
import { releaseSummaryRoute } from '@admin/routes/releaseRoutes';
import { dashboardRoute } from '@admin/routes/routes';
import metaService, {
  TimePeriodCoverageGroup,
} from '@admin/services/metaService';
import publicationService, {
  Publication,
} from '@admin/services/publicationService';
import releaseService from '@admin/services/releaseService';
import { IdTitlePair } from '@admin/services/types/common';
import LoadingSpinner from '@common/components/LoadingSpinner';
import useAsyncRetry from '@common/hooks/useAsyncRetry';
import React from 'react';
import { generatePath, RouteComponentProps, withRouter } from 'react-router';

export interface FormValues extends ReleaseSummaryFormValues {
  templateReleaseId: string;
}

interface MatchProps {
  publicationId: string;
}

interface Model {
  publication: Publication;
  templateRelease: IdTitlePair;
  timePeriodCoverageGroups: TimePeriodCoverageGroup[];
}

const ReleaseCreatePage = ({
  match,
  history,
}: RouteComponentProps<MatchProps>) => {
  const { publicationId } = match.params;

  const { value: model, isLoading } = useAsyncRetry<Model>(async () => {
    const [publication, templateRelease, timePeriodCoverageGroups] =
      await Promise.all([
        publicationService.getPublication(publicationId),
        publicationService.getPublicationReleaseTemplate(publicationId),
        metaService.getTimePeriodCoverageGroups(),
      ]);

    return {
      publication,
      templateRelease,
      timePeriodCoverageGroups,
    } as Model;
  }, [publicationId]);

  const handleSubmit = async (values: ReleaseSummaryFormValues) => {
    const createdRelease = await releaseService.createReleaseVersion({
      timePeriodCoverage: {
        value: values.timePeriodCoverageCode,
      },
      year: Number(values.timePeriodCoverageStartYear),
      type: values.releaseType ?? 'AdHocStatistics',
      publicationId,
      templateReleaseId:
        values.templateReleaseId !== 'new' ? values.templateReleaseId : '',
      label: values.releaseLabel,
    });

    history.push(
      generatePath(releaseSummaryRoute.path, {
        publicationId,
        releaseVersionId: createdRelease.id,
      }),
    );
  };

  const handleCancel = () => history.push(dashboardRoute.path);

  return (
    <Page
      wide
      breadcrumbs={[
        {
          name: 'Create new release',
        },
      ]}
    >
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-two-thirds">
          <PageTitle
            title="Create new release"
            caption={model?.publication.title}
          />
        </div>
      </div>

      <LoadingSpinner loading={isLoading}>
        <ReleaseSummaryForm
          submitText="Create new release"
          initialValues={{
            timePeriodCoverageCode:
              model?.timePeriodCoverageGroups[0]?.timeIdentifiers[0]?.identifier
                .value ?? '',
            timePeriodCoverageStartYear: '',
            templateReleaseId: '',
            releaseType: undefined,
            releaseLabel: '',
          }}
          releaseVersion={0}
          templateRelease={model?.templateRelease}
          onSubmit={handleSubmit}
          onCancel={handleCancel}
        />
      </LoadingSpinner>
    </Page>
  );
};

export default withRouter(ReleaseCreatePage);
