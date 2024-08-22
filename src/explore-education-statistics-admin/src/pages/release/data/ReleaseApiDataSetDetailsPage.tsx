import Link from '@admin/components/Link';
import { useConfig } from '@admin/contexts/ConfigContext';
import ApiDataSetVersionSummaryList from '@admin/pages/release/data/components/ApiDataSetVersionSummaryList';
import DeleteDraftVersionButton from '@admin/pages/release/data/components/DeleteDraftVersionButton';
import ApiDataSetCreateModal from '@admin/pages/release/data/components/ApiDataSetCreateModal';
import ApiDataSetFinaliseBanner from '@admin/pages/release/data/components/ApiDataSetFinaliseBanner';
import { useReleaseContext } from '@admin/pages/release/contexts/ReleaseContext';
import apiDataSetQueries from '@admin/queries/apiDataSetQueries';
import {
  releaseApiDataSetChangelogRoute,
  releaseApiDataSetFiltersMappingRoute,
  releaseApiDataSetHistoryPageRoute,
  releaseApiDataSetLocationsMappingRoute,
  releaseApiDataSetPreviewRoute,
  releaseApiDataSetPreviewTokenLogRoute,
  releaseApiDataSetsRoute,
  ReleaseDataSetChangelogRouteParams,
  ReleaseDataSetRouteParams,
  ReleaseRouteParams,
} from '@admin/routes/releaseRoutes';
import { DataSetStatus } from '@admin/services/apiDataSetService';
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

export type DataSetFinalisingStatus = 'finalising' | 'finalised' | undefined;

export default function ReleaseApiDataSetDetailsPage() {
  const { dataSetId } = useParams<ReleaseDataSetRouteParams>();
  const history = useHistory();

  const { publicAppUrl } = useConfig();
  const { release } = useReleaseContext();

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

  const canUpdateRelease = release.approvalStatus !== 'Approved';

  const columnSizeClassName =
    dataSet?.latestLiveVersion && dataSet?.draftVersion
      ? 'govuk-grid-column-one-half-from-desktop'
      : 'govuk-grid-column-two-thirds-from-desktop';

  const handleFinalise = async () => {
    if (dataSet?.draftVersion) {
      setFinalisingStatus('finalising');
      await apiDataSetVersionService.completeVersion({
        dataSetVersionId: dataSet?.draftVersion?.id,
      });
    }
  };

  useEffect(() => {
    if (
      finalisingStatus === 'finalising' &&
      dataSet?.draftVersion?.status !== 'Mapping'
    ) {
      setFinalisingStatus('finalised');
    }
  }, [finalisingStatus, dataSet?.draftVersion?.status, setFinalisingStatus]);

  const draftVersionSummary = dataSet?.draftVersion ? (
    <ApiDataSetVersionSummaryList
      dataSetVersion={dataSet?.draftVersion}
      id="draft-version-summary"
      publicationId={release.publicationId}
      collapsibleButtonHiddenText="for draft version"
      actions={
        <ul className="govuk-list">
          {dataSet.draftVersion.status === 'Draft' && (
            <>
              <li>
                <Link
                  to={generatePath<ReleaseDataSetChangelogRouteParams>(
                    releaseApiDataSetChangelogRoute.path,
                    {
                      publicationId: release.publicationId,
                      releaseId: release.id,
                      dataSetId,
                      dataSetVersionId: dataSet.draftVersion.id,
                    },
                  )}
                >
                  View changelog and guidance notes
                </Link>
              </li>
              <li>
                <Link
                  to={generatePath<ReleaseDataSetRouteParams>(
                    releaseApiDataSetPreviewRoute.path,
                    {
                      publicationId: release.publicationId,
                      releaseId: release.id,
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
                      publicationId: release.publicationId,
                      releaseId: release.id,
                      dataSetId,
                    },
                  )}
                >
                  View API data set token log
                </Link>
              </li>
            </>
          )}
          {canUpdateRelease && dataSet.draftVersion.status !== 'Processing' && (
            <li>
              <DeleteDraftVersionButton
                dataSet={dataSet}
                dataSetVersion={dataSet.draftVersion}
                onDeleted={() =>
                  history.push(
                    generatePath<ReleaseRouteParams>(
                      releaseApiDataSetsRoute.path,
                      {
                        publicationId: release.publicationId,
                        releaseId: release.id,
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
      }
    />
  ) : null;

  const liveVersionSummary = dataSet?.latestLiveVersion ? (
    <ApiDataSetVersionSummaryList
      dataSetVersion={dataSet.latestLiveVersion}
      id="live-version-summary"
      publicationId={release.publicationId}
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
                    publicationId: release.publicationId,
                    releaseId: release.id,
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
                  releaseApiDataSetHistoryPageRoute.path,
                  {
                    publicationId: release.publicationId,
                    releaseId: release.id,
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

  const mappingComplete =
    dataSet?.draftVersion?.mappingStatus &&
    dataSet.draftVersion.mappingStatus.filtersComplete &&
    dataSet.draftVersion.mappingStatus.locationsComplete;

  const showDraftVersionTasks =
    finalisingStatus !== 'finalising' &&
    dataSet?.draftVersion?.mappingStatus &&
    (dataSet?.draftVersion?.status === 'Draft' ||
      dataSet?.draftVersion?.status === 'Mapping');

  return (
    <>
      <Link
        back
        className="govuk-!-margin-bottom-6"
        to={generatePath<ReleaseRouteParams>(releaseApiDataSetsRoute.path, {
          releaseId: release.id,
          publicationId: release.publicationId,
        })}
      >
        Back to API data sets
      </Link>

      <LoadingSpinner loading={isLoading}>
        {dataSet && (
          <>
            <span className="govuk-caption-l">API data set details</span>
            <h2>{dataSet.title}</h2>

            {mappingComplete && dataSet.draftVersion && (
              <ApiDataSetFinaliseBanner
                changelogPath={generatePath<ReleaseDataSetChangelogRouteParams>(
                  releaseApiDataSetChangelogRoute.path,
                  {
                    publicationId: release.publicationId,
                    releaseId: release.id,
                    dataSetId,
                    dataSetVersionId: dataSet.draftVersion.id,
                  },
                )}
                draftVersionStatus={dataSet.draftVersion.status}
                finalisingStatus={finalisingStatus}
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
              <div className="govuk-grid-row">
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
                            dataSet.draftVersion.mappingStatus
                              ?.locationsComplete
                              ? 'blue'
                              : 'red'
                          }
                        >
                          {dataSet.draftVersion.mappingStatus?.locationsComplete
                            ? 'Complete'
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
                              publicationId: release.publicationId,
                              releaseId: release.id,
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
                            dataSet.draftVersion.mappingStatus?.filtersComplete
                              ? 'blue'
                              : 'red'
                          }
                        >
                          {dataSet.draftVersion.mappingStatus?.filtersComplete
                            ? 'Complete'
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
                              publicationId: release.publicationId,
                              releaseId: release.id,
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
              !dataSet.previousReleaseIds.includes(release.releaseId) && (
                <ApiDataSetCreateModal
                  buttonText="Create a new version of this data set"
                  publicationId={release.publicationId}
                  releaseId={release.id}
                  submitText="Confirm new data set version"
                  title="Create a new API data set version"
                  onSubmit={async ({ releaseFileId }) => {
                    await apiDataSetVersionService.createVersion({
                      dataSetId: dataSet.id,
                      releaseFileId,
                    });
                    refetch();
                  }}
                />
              )}
          </>
        )}
      </LoadingSpinner>
    </>
  );
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
