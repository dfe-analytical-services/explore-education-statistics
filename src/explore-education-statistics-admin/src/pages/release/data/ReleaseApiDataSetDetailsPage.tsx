import Link from '@admin/components/Link';
import { useConfig } from '@admin/contexts/ConfigContext';
import ApiDataSetVersionSummaryList from '@admin/pages/release/data/components/ApiDataSetVersionSummaryList';
import DeleteDraftVersionButton from '@admin/pages/release/data/components/DeleteDraftVersionButton';
import ApiDataSetCreateModal from '@admin/pages/release/data/components/ApiDataSetCreateModal';
import ApiDataSetFinaliseBanner from '@admin/pages/release/data/components/ApiDataSetFinaliseBanner';
import { useReleaseVersionContext } from '@admin/pages/release/contexts/ReleaseVersionContext';
import apiDataSetQueries from '@admin/queries/apiDataSetQueries';
import {
  releaseApiDataSetChangelogRoute,
  releaseApiDataSetFiltersMappingRoute,
  releaseApiDataSetVersionHistoryRoute,
  releaseApiDataSetLocationsMappingRoute,
  releaseApiDataSetPreviewRoute,
  releaseApiDataSetPreviewTokenLogRoute,
  releaseApiDataSetsRoute,
  ReleaseDataSetChangelogRouteParams,
  ReleaseDataSetRouteParams,
  ReleaseRouteParams,
  releaseDataFileReplaceRoute,
  ReleaseDataFileReplaceRouteParams,
} from '@admin/routes/releaseRoutes';
import { ApiDataSet, DataSetStatus } from '@admin/services/apiDataSetService';
import apiDataSetVersionService from '@admin/services/apiDataSetVersionService';
import ContentHtml from '@common/components/ContentHtml';
import LoadingSpinner from '@common/components/LoadingSpinner';
import SummaryCard from '@common/components/SummaryCard';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import Tag, { TagProps } from '@common/components/Tag';
import TaskList from '@common/components/TaskList';
import TaskListItem from '@common/components/TaskListItem';
import { useQuery } from '@tanstack/react-query';
import React, { useEffect, useState } from 'react';
import { generatePath, useHistory, useParams } from 'react-router-dom';
import isPatchVersion from '@common/utils/isPatchVersion';
import InsetText from '@common/components/InsetText';
import shouldShowDraftActions from '@admin/pages/release/data/utils/shouldShowDraftActions';

export type DataSetFinalisingStatus = 'finalising' | 'finalised' | undefined;

export default function ReleaseApiDataSetDetailsPage() {
  const { dataSetId } = useParams<ReleaseDataSetRouteParams>();
  const history = useHistory();
  const { publicAppUrl } = useConfig();
  const { releaseVersion } = useReleaseVersionContext();

  const [finalisingStatus, setFinalisingStatus] =
    useState<DataSetFinalisingStatus>(undefined);

  const {
    data: dataSet,
    isLoading,
    refetch,
  } = useQuery({
    ...apiDataSetQueries.get(dataSetId),
    refetchInterval: data => {
      return data?.draftVersion?.status === 'Processing' ||
        finalisingStatus === 'finalising'
        ? 3000
        : false;
    },
  });

  useEffect(() => {
    if (
      finalisingStatus === 'finalising' &&
      dataSet?.draftVersion?.status !== 'Mapping' &&
      dataSet?.draftVersion?.status !== 'Finalising'
    ) {
      setFinalisingStatus('finalised');
    }
  }, [finalisingStatus, dataSet?.draftVersion?.status, setFinalisingStatus]);

  const isPatch = isPatchVersion(dataSet?.draftVersion?.version);

  const shouldShowRejectedError = (
    apiDataSet: ApiDataSet | undefined,
    isPatchDataSetVersion: boolean,
  ): boolean => {
    return isPatchDataSetVersion
      ? !!(
          apiDataSet?.draftVersion?.mappingStatus &&
          apiDataSet.draftVersion.mappingStatus.isMajorVersionUpdate
        )
      : false;
  };

  const handleFinalise = async () => {
    if (dataSet?.draftVersion) {
      setFinalisingStatus('finalising');
      await apiDataSetVersionService.completeVersion({
        dataSetVersionId: dataSet?.draftVersion?.id,
      });
    }
  };

  const columnSizeClassName =
    dataSet?.latestLiveVersion && dataSet?.draftVersion
      ? 'govuk-grid-column-one-half-from-desktop'
      : 'govuk-grid-column-two-thirds-from-desktop';

  const canUpdateRelease = releaseVersion.approvalStatus !== 'Approved';

  const showDraftVersionActions = shouldShowDraftActions(
    isPatch,
    canUpdateRelease,
    dataSet,
  );

  const draftVersionSummary = dataSet?.draftVersion ? (
    <ApiDataSetVersionSummaryList
      dataSetVersion={dataSet.draftVersion}
      id="draft-version-summary"
      publicationId={releaseVersion.publicationId}
      collapsibleButtonHiddenText="for draft version"
      actions={
        showDraftVersionActions && (
          <ul className="govuk-list">
            {dataSet.draftVersion.status === 'Draft' && (
              <>
                {dataSet.draftVersion.version !== '1.0' && (
                  <li>
                    <Link
                      to={generatePath<ReleaseDataSetChangelogRouteParams>(
                        releaseApiDataSetChangelogRoute.path,
                        {
                          publicationId: releaseVersion.publicationId,
                          releaseVersionId: releaseVersion.id,
                          dataSetId,
                          dataSetVersionId: dataSet.draftVersion.id,
                        },
                      )}
                    >
                      View changelog and guidance notes
                    </Link>
                  </li>
                )}
                <li>
                  <Link
                    to={generatePath<ReleaseDataSetRouteParams>(
                      releaseApiDataSetPreviewRoute.path,
                      {
                        publicationId: releaseVersion.publicationId,
                        releaseVersionId: releaseVersion.id,
                        dataSetId,
                      },
                    )}
                  >
                    Preview API data set
                  </Link>
                </li>
                <li>
                  <Link
                    to={generatePath<ReleaseDataSetRouteParams>(
                      releaseApiDataSetPreviewTokenLogRoute.path,
                      {
                        publicationId: releaseVersion.publicationId,
                        releaseVersionId: releaseVersion.id,
                        dataSetId,
                      },
                    )}
                  >
                    View preview token log
                  </Link>
                </li>
              </>
            )}
            {canUpdateRelease &&
              !isPatch &&
              dataSet?.draftVersion?.status !== 'Finalising' && (
                <li>
                  <DeleteDraftVersionButton
                    dataSet={dataSet}
                    dataSetVersion={dataSet.draftVersion}
                    onDeleted={() =>
                      history.push(
                        generatePath<ReleaseRouteParams>(
                          releaseApiDataSetsRoute.path,
                          {
                            publicationId: releaseVersion.publicationId,
                            releaseVersionId: releaseVersion.id,
                          },
                        ),
                      )
                    }
                  >
                    Remove draft version
                  </DeleteDraftVersionButton>
                </li>
              )}
          </ul>
        )
      }
    />
  ) : null;

  const liveVersionSummary = dataSet?.latestLiveVersion ? (
    <ApiDataSetVersionSummaryList
      dataSetVersion={dataSet.latestLiveVersion}
      id="live-version-summary"
      publicationId={releaseVersion.publicationId}
      collapsibleButtonHiddenText="for latest live version"
      actions={
        <ul className="govuk-list">
          <li>
            <a
              href={`${publicAppUrl}/data-catalogue/data-set/${dataSet.latestLiveVersion.file.id}`}
              target="_blank"
              rel="noopener noreferrer"
            >
              View live data set (opens in new tab)
            </a>
          </li>
          {dataSet.latestLiveVersion.version !== '1.0' && (
            <li>
              <Link
                to={generatePath<ReleaseDataSetChangelogRouteParams>(
                  releaseApiDataSetChangelogRoute.path,
                  {
                    publicationId: releaseVersion.publicationId,
                    releaseVersionId:
                      dataSet.latestLiveVersion.releaseVersion.id,
                    dataSetId,
                    dataSetVersionId: dataSet.latestLiveVersion.id,
                  },
                )}
              >
                View changelog and guidance notes
              </Link>
            </li>
          )}
          {dataSet.latestLiveVersion.version !== '1.0' && (
            <li>
              <Link
                to={generatePath<ReleaseDataSetRouteParams>(
                  releaseApiDataSetVersionHistoryRoute.path,
                  {
                    publicationId: releaseVersion.publicationId,
                    releaseVersionId:
                      dataSet.latestLiveVersion.releaseVersion.id,
                    dataSetId,
                  },
                )}
              >
                View version history
              </Link>
            </li>
          )}
        </ul>
      }
    />
  ) : null;

  const showRejectedError = shouldShowRejectedError(dataSet, isPatch);

  const mappingComplete =
    dataSet?.draftVersion?.mappingStatus &&
    dataSet.draftVersion.mappingStatus.filtersComplete &&
    dataSet.draftVersion.mappingStatus.locationsComplete;

  const showDraftVersionTasks =
    dataSet?.draftVersion?.status !== 'Processing' &&
    dataSet?.draftVersion?.status !== 'Finalising' &&
    finalisingStatus !== 'finalising' &&
    dataSet?.draftVersion?.mappingStatus &&
    (dataSet?.draftVersion?.status === 'Draft' ||
      dataSet?.draftVersion?.status === 'Mapping');

  const replaceRouteParams = dataSet?.draftVersion?.originalFileId
    ? {
        publicationId: releaseVersion.publicationId,
        releaseVersionId: dataSet?.draftVersion?.releaseVersion.id,
        fileId: dataSet?.draftVersion?.originalFileId as string,
      }
    : undefined;
  const replaceTabRoute = replaceRouteParams
    ? `${generatePath<ReleaseDataFileReplaceRouteParams>(
        releaseDataFileReplaceRoute.path,
        replaceRouteParams,
      )}`
    : '';

  const incompletesFound =
    dataSet?.draftVersion?.mappingStatus &&
    (!dataSet.draftVersion.mappingStatus.filtersComplete ||
      !dataSet.draftVersion.mappingStatus.locationsComplete);

  const majorVersionErrorSummary = (
    <InsetText variant="error">
      <h2 className="govuk-error-summary__title" id="error-summary-title">
        {incompletesFound
          ? 'This API data set can not be published because location or filter mappings are not yet complete.'
          : 'This API data set can not be published because it has major changes that are not allowed.'}
      </h2>
      <div className="govuk-error-summary__body">
        <ul className="govuk-list govuk-error-summary__list">
          <li>
            {incompletesFound
              ? 'The data file uploaded has not been able to be fully auto mapped and as a result has incomplete location or filter manual mapping.'
              : 'The data file uploaded has resulted in a major version update which is not allowed in release amendments. Major version type changes can only be made as part of new releases.'}
          </li>
          <li>
            Please select a mapping configuration that does not result in a
            major version.
          </li>
          <li>
            Alternatively{' '}
            <Link to={replaceTabRoute} data-testid="cancel-replacement-link">
              {' '}
              cancel the ongoing replacement
            </Link>{' '}
            and upload a new data file which does not result in a major version
            update.
          </li>
          <li>For further guidance, contact the EES team.</li>
        </ul>
      </div>
    </InsetText>
  );
  return (
    <>
      <Link
        back
        className="govuk-!-margin-bottom-6"
        to={generatePath<ReleaseRouteParams>(releaseApiDataSetsRoute.path, {
          releaseVersionId: releaseVersion.id,
          publicationId: releaseVersion.publicationId,
        })}
      >
        Back to API data sets
      </Link>

      <LoadingSpinner loading={isLoading}>
        {dataSet && (
          <>
            <span className="govuk-caption-l">API data set details</span>
            <h2>{dataSet.title}</h2>

            {showRejectedError
              ? majorVersionErrorSummary
              : mappingComplete &&
                dataSet.draftVersion && (
                  <ApiDataSetFinaliseBanner
                    dataSetId={dataSetId}
                    dataSetVersionId={dataSet.draftVersion.id}
                    draftVersionStatus={dataSet.draftVersion.status}
                    finalisingStatus={finalisingStatus}
                    publicationId={releaseVersion.publicationId}
                    releaseVersionId={releaseVersion.id}
                    onFinalise={handleFinalise}
                  />
                )}

            <SummaryList
              className="govuk-!-margin-bottom-8"
              testId="data-set-summary"
            >
              <SummaryListItem term="Status">
                <Tag colour={getDataSetStatusColour(dataSet.status)}>
                  {dataSet.status}
                </Tag>
              </SummaryListItem>
              <SummaryListItem term="Summary">
                <ContentHtml html={dataSet.summary} />
              </SummaryListItem>
            </SummaryList>

            {showDraftVersionTasks && dataSet.draftVersion && (
              <div className="govuk-grid-row" data-testid="draft-version-tasks">
                <div className={columnSizeClassName}>
                  <h3>Draft version tasks</h3>

                  <p>
                    To publish the draft version of the API data set, the
                    following tasks need to be completed:
                  </p>

                  <TaskList className="govuk-!-margin-bottom-8">
                    <TaskListItem
                      id="map-locations-task"
                      status={
                        <Tag
                          colour={
                            (
                              dataSet.draftVersion.mappingStatus
                                ?.locationsComplete && isPatch
                                ? dataSet.draftVersion.mappingStatus
                                    ?.locationsHaveMajorChange
                                : !dataSet.draftVersion.mappingStatus
                                    ?.locationsComplete
                            )
                              ? 'red'
                              : 'blue'
                          }
                        >
                          {dataSet.draftVersion.mappingStatus?.locationsComplete
                            ? getCompleteText(
                                showRejectedError,
                                dataSet.draftVersion.mappingStatus
                                  ?.locationsHaveMajorChange,
                              )
                            : 'Incomplete'}
                        </Tag>
                      }
                      hint="Define the changes to locations in this version."
                    >
                      {props => (
                        <Link
                          {...props}
                          to={generatePath<ReleaseDataSetRouteParams>(
                            releaseApiDataSetLocationsMappingRoute.path,
                            {
                              publicationId: releaseVersion.publicationId,
                              releaseVersionId: releaseVersion.id,
                              dataSetId,
                            },
                          )}
                        >
                          Map locations
                        </Link>
                      )}
                    </TaskListItem>
                    <TaskListItem
                      id="map-filters-task"
                      status={
                        <Tag
                          colour={
                            (
                              dataSet.draftVersion.mappingStatus
                                ?.filtersComplete && isPatch
                                ? dataSet.draftVersion.mappingStatus
                                    ?.filtersHaveMajorChange
                                : !dataSet.draftVersion.mappingStatus
                                    ?.filtersComplete
                            )
                              ? 'red'
                              : 'blue'
                          }
                        >
                          {dataSet.draftVersion.mappingStatus?.filtersComplete
                            ? getCompleteText(
                                showRejectedError,
                                dataSet.draftVersion.mappingStatus
                                  ?.filtersHaveMajorChange,
                              )
                            : 'Incomplete'}
                        </Tag>
                      }
                      hint="Define the changes to filters in this version."
                    >
                      {props => (
                        <Link
                          {...props}
                          to={generatePath<ReleaseDataSetRouteParams>(
                            releaseApiDataSetFiltersMappingRoute.path,
                            {
                              publicationId: releaseVersion.publicationId,
                              releaseVersionId: releaseVersion.id,
                              dataSetId,
                            },
                          )}
                        >
                          Map filters
                        </Link>
                      )}
                    </TaskListItem>
                  </TaskList>
                </div>
              </div>
            )}

            <div className="govuk-grid-row">
              {draftVersionSummary && (
                <div className={columnSizeClassName}>
                  {dataSet.latestLiveVersion ? (
                    <SummaryCard
                      heading="Draft version details"
                      headingTag="h3"
                    >
                      {draftVersionSummary}
                    </SummaryCard>
                  ) : (
                    <>
                      <h3>Draft version details</h3>
                      {draftVersionSummary}
                    </>
                  )}
                </div>
              )}
              {liveVersionSummary && (
                <div className={columnSizeClassName}>
                  {dataSet.draftVersion ? (
                    <SummaryCard
                      heading="Latest live version details"
                      headingTag="h3"
                    >
                      {liveVersionSummary}
                    </SummaryCard>
                  ) : (
                    <>
                      <h3>Latest live version details</h3>
                      {liveVersionSummary}
                    </>
                  )}
                </div>
              )}
            </div>
            {canUpdateRelease &&
              !dataSet.draftVersion &&
              !dataSet.previousReleaseIds.includes(
                releaseVersion.releaseId,
              ) && (
                <ApiDataSetCreateModal
                  buttonText="Create a new version of this data set"
                  publicationId={releaseVersion.publicationId}
                  releaseVersionId={releaseVersion.id}
                  submitText="Confirm new data set version"
                  title="Create a new API data set version"
                  onSubmit={async ({ releaseFileId }) => {
                    await apiDataSetVersionService.createVersion({
                      dataSetId: dataSet.id,
                      releaseFileId,
                    });
                    await refetch();
                  }}
                />
              )}
          </>
        )}
      </LoadingSpinner>
    </>
  );

  function getCompleteText(
    rejectedErrorShown: boolean,
    majorVersionFound: boolean,
  ): React.ReactNode {
    return rejectedErrorShown && majorVersionFound
      ? 'Major Change'
      : 'Complete';
  }
}

function getDataSetStatusColour(status: DataSetStatus): TagProps['colour'] {
  switch (status) {
    case 'Deprecated':
      return 'purple';
    case 'Withdrawn':
      return 'red';
    default:
      return 'blue';
  }
}
