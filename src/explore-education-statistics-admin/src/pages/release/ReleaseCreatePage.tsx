import Page from '@admin/components/Page';
import PageTitle from '@admin/components/PageTitle';
import { useConfig } from '@admin/contexts/ConfigContext';
import useQueryParams from '@admin/hooks/useQueryParams';
import ReleaseSummaryForm, {
  ReleaseCreationMode,
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
import releaseVersionService from '@admin/services/releaseVersionService';
import { IdTitlePair, ValueLabelPair } from '@admin/services/types/common';
import LoadingSpinner from '@common/components/LoadingSpinner';
import useAsyncRetry from '@common/hooks/useAsyncRetry';
import { Organisation } from '@common/services/types/organisation';
import { ReleaseType } from '@common/services/types/releaseType';
import React, { useState } from 'react';
import { generatePath, useNavigate, useParams } from 'react-router';

export interface FormValues extends ReleaseSummaryFormValues {
  templateReleaseId: string;
}

type MatchProps = {
  publicationId: string;
};

type QueryParams = {
  sourceReleaseId?: string;
};

interface RolloverSource {
  releaseId: string;
  title: string;
  timePeriodCoverage: ValueLabelPair;
  year: number;
  yearTitle: string;
  type: ReleaseType;
  publishingOrganisations: Organisation[];
}

interface Model {
  publication: Publication;
  templateRelease?: IdTitlePair;
  rolloverSource?: RolloverSource;
  timePeriodCoverageGroups: TimePeriodCoverageGroup[];
}

const ReleaseCreatePage = () => {
  const { publicationId } = useParams<MatchProps>() as MatchProps;
  const { sourceReleaseId } = useQueryParams<QueryParams>();
  const { enableReleaseRollover } = useConfig();
  const navigate = useNavigate();

  const [creationMode, setCreationMode] = useState<ReleaseCreationMode>(
    sourceReleaseId ? 'WithData' : 'blank',
  );

  const { value: model, isLoading } = useAsyncRetry<Model>(async () => {
    if (!enableReleaseRollover) {
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
    }

    const [publication, timePeriodCoverageGroups] = await Promise.all([
      publicationService.getPublication(publicationId),
      metaService.getTimePeriodCoverageGroups(),
    ]);

    const sourceReleaseVersionId =
      sourceReleaseId ??
      (
        await publicationService
          .getPublicationReleaseTemplate(publicationId)
          .catch(() => undefined)
      )?.id;

    const sourceReleaseVersion = sourceReleaseVersionId
      ? await releaseVersionService
          .getReleaseVersion(sourceReleaseVersionId)
          .catch(() => undefined)
      : undefined;

    return {
      publication,
      rolloverSource: sourceReleaseVersion
        ? {
            releaseId: sourceReleaseVersion.releaseId,
            title: sourceReleaseVersion.title,
            timePeriodCoverage: sourceReleaseVersion.timePeriodCoverage,
            year: sourceReleaseVersion.year,
            yearTitle: sourceReleaseVersion.yearTitle,
            type: sourceReleaseVersion.type,
            publishingOrganisations:
              sourceReleaseVersion.publishingOrganisations,
          }
        : undefined,
      timePeriodCoverageGroups,
    } as Model;
  }, [publicationId, sourceReleaseId, enableReleaseRollover]);

  const isRolloverTitle =
    Boolean(model?.rolloverSource) && creationMode === 'WithData';
  const pageTitle = isRolloverTitle
    ? 'Create release rollover'
    : 'Create new release';

  const handleSubmit = async (values: ReleaseSummaryFormValues) => {
    if (
      model?.rolloverSource &&
      (values.creationMode === 'WithData' ||
        values.creationMode === 'StructureOnly')
    ) {
      const createdRollover = await releaseService.createReleaseRollover({
        timePeriodCoverage: {
          value: values.timePeriodCoverageCode,
        },
        year: Number(values.timePeriodCoverageStartYear),
        type: values.releaseType ?? 'AdHocStatistics',
        publicationId,
        rolloverSourceReleaseId: model.rolloverSource.releaseId,
        mode: values.creationMode,
        label: values.releaseLabel,
        publishingOrganisations: values.publishingOrganisations,
      });

      navigate(
        generatePath(releaseSummaryRoute.fullPath, {
          publicationId,
          releaseVersionId: createdRollover.id,
        }),
      );
      return;
    }

    const createdRelease = await releaseService.createRelease({
      timePeriodCoverage: {
        value: values.timePeriodCoverageCode,
      },
      year: Number(values.timePeriodCoverageStartYear),
      type: values.releaseType ?? 'AdHocStatistics',
      publicationId,
      templateReleaseId:
        values.templateReleaseId !== 'new' ? values.templateReleaseId : '',
      label: values.releaseLabel,
      publishingOrganisations: values.publishingOrganisations,
    });

    navigate(
      generatePath(releaseSummaryRoute.fullPath, {
        publicationId,
        releaseVersionId: createdRelease.id,
      }),
    );
  };

  const handleCancel = () => navigate(dashboardRoute.fullPath);

  return (
    <Page
      wide
      breadcrumbs={[
        {
          name: pageTitle,
        },
      ]}
    >
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-two-thirds">
          <PageTitle title={pageTitle} caption={model?.publication.title} />
        </div>
      </div>

      <LoadingSpinner loading={isLoading}>
        <ReleaseSummaryForm
          submitText="Create new release"
          initialValues={{
            timePeriodCoverageCode:
              model?.rolloverSource?.timePeriodCoverage.value ??
              model?.timePeriodCoverageGroups[0]?.timeIdentifiers[0]?.identifier
                .value ??
              '',
            timePeriodCoverageStartYear: model?.rolloverSource
              ? String(model.rolloverSource.year + 1)
              : '',
            templateReleaseId: '',
            creationMode: model?.rolloverSource ? creationMode : 'blank',
            releaseType: model?.rolloverSource?.type,
            releaseLabel: '',
            publishingOrganisations:
              model?.rolloverSource?.publishingOrganisations.map(
                org => org.id,
              ) ?? [],
          }}
          releaseVersion={0}
          templateRelease={model?.templateRelease}
          rolloverSource={
            model?.rolloverSource
              ? {
                  id: model.rolloverSource.releaseId,
                  title: model.rolloverSource.title,
                  timePeriodCoverage: model.rolloverSource.timePeriodCoverage,
                  yearTitle: model.rolloverSource.yearTitle,
                }
              : undefined
          }
          onCreationModeChange={setCreationMode}
          onSubmit={handleSubmit}
          onCancel={handleCancel}
        />
      </LoadingSpinner>
    </Page>
  );
};

export default ReleaseCreatePage;
