import Link from '@admin/components/Link';
import { useConfig } from '@admin/contexts/ConfigContext';
import ApiDataSetVersionSummaryList from '@admin/pages/release/api-data-sets/components/ApiDataSetVersionSummaryList';
import { useReleaseContext } from '@admin/pages/release/contexts/ReleaseContext';
import apiDataSetQueries from '@admin/queries/apiDataSetQueries';
import {
  releaseApiDataSetsRoute,
  ReleaseDataSetRouteParams,
  ReleaseRouteParams,
} from '@admin/routes/releaseRoutes';
import ButtonText from '@common/components/ButtonText';
import LoadingSpinner from '@common/components/LoadingSpinner';
import SummaryCard from '@common/components/SummaryCard';
import Tag from '@common/components/Tag';
import TaskList from '@common/components/TaskList';
import TaskListItem from '@common/components/TaskListItem';
import { useQuery } from '@tanstack/react-query';
import React from 'react';
import { generatePath, useParams } from 'react-router-dom';

// TODO: EES-4370
const showRemoveDraft = false;
// TODO: Version mapping
const showDraftVersionTasks = false;

// TODO: EES-4367
const showChangelog = false;
// TODO: EES-4382
const showVersionHistory = false;

export default function ReleaseApiDataSetDetailsPage() {
  const { dataSetId } = useParams<ReleaseDataSetRouteParams>();
  const { publicAppUrl } = useConfig();
  const { release } = useReleaseContext();

  const { data: dataSet, isLoading } = useQuery({
    ...apiDataSetQueries.get(dataSetId),
    refetchInterval: data => {
      return data?.draftVersion?.status === 'Processing' ? 3000 : false;
    },
  });

  const columnSizeClassName =
    dataSet?.latestLiveVersion && dataSet?.draftVersion
      ? 'govuk-grid-column-one-half-from-desktop'
      : 'govuk-grid-column-two-thirds-from-desktop';

  const draftVersionSummary = dataSet?.draftVersion ? (
    <ApiDataSetVersionSummaryList
      dataSetVersion={dataSet?.draftVersion}
      id="draft-version-summary"
      publicationId={release.publicationId}
      collapsibleButtonHiddenText="for draft version"
      actions={
        showRemoveDraft ? (
          <ul className="govuk-list">
            <li>
              <ButtonText variant="warning">Remove draft version</ButtonText>
            </li>
          </ul>
        ) : undefined
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
          {showChangelog && (
            <li>
              <Link to="/todo">View changelog and guidance notes</Link>
            </li>
          )}
          {showVersionHistory && (
            <li>
              <Link to="/todo">View version history</Link>
            </li>
          )}
        </ul>
      }
    />
  ) : null;

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

            {dataSet.draftVersion?.status === 'Mapping' &&
              showDraftVersionTasks && (
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
                        status={<Tag colour="red">Incomplete</Tag>}
                        hint="Define the changes to locations in this version."
                      >
                        {props => (
                          <Link {...props} to="/todo">
                            Map locations
                          </Link>
                        )}
                      </TaskListItem>
                      <TaskListItem
                        id="map-filters-task"
                        status={<Tag colour="blue">Complete</Tag>}
                        hint="Define the changes to filters in this version."
                      >
                        {props => (
                          <Link {...props} to="/todo">
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
          </>
        )}
      </LoadingSpinner>
    </>
  );
}
