import { useAuthContext } from '@admin/contexts/AuthContext';
import { useReleaseContext } from '@admin/pages/release/contexts/ReleaseContext';
import ApiDataSetCreateModal from '@admin/pages/release/data/components/ApiDataSetCreateModal';
import DraftApiDataSetsTable, {
  DraftApiDataSetSummary,
} from '@admin/pages/release/data/components/DraftApiDataSetsTable';
import LiveApiDataSetsTable, {
  LiveApiDataSetSummary,
} from '@admin/pages/release/data/components/LiveApiDataSetsTable';
import apiDataSetQueries from '@admin/queries/apiDataSetQueries';
import apiDataSetService from '@admin/services/apiDataSetService';
import InsetText from '@common/components/InsetText';
import LoadingSpinner from '@common/components/LoadingSpinner';
import WarningMessage from '@common/components/WarningMessage';
import { useQuery } from '@tanstack/react-query';
import React from 'react';

export default function ReleaseApiDataSetsSection() {
  const { release } = useReleaseContext();
  const { user } = useAuthContext();

  const {
    data: dataSets = [],
    isLoading: hasDataSetsLoading,
    refetch,
  } = useQuery({
    ...apiDataSetQueries.list(release.publicationId),
    refetchInterval: 20_000,
  });

  const draftDataSets = dataSets.filter(
    dataSet => dataSet.draftVersion,
  ) as DraftApiDataSetSummary[];

  const liveDataSets = dataSets.filter(
    dataSet => dataSet.latestLiveVersion && !dataSet.draftVersion,
  ) as LiveApiDataSetSummary[];

  const canUpdateRelease = release.approvalStatus !== 'Approved';

  return (
    <>
      <h2>API data sets</h2>

      <InsetText className="govuk-!-width-three-quarters">
        <h3>Before you start</h3>

        <p>
          API data sets are data sets that can be consumed by third-party
          applications via the platform's public API.
        </p>

        <p>
          An API data set should ideally be a long-lived data series where the
          data structure is expected to remain stable between each release. New
          versions of the API data set containing any updates can be published
          with future releases.
        </p>

        <p>
          If the structure of your data set will not be stable with future
          releases, you are advised to avoid using an API data set. Users can
          still download and explore your data by creating their own tables.
        </p>

        {!release.published && (
          <strong>
            Changes will not be made in the public API until this release has
            been published.
          </strong>
        )}
      </InsetText>

      <LoadingSpinner loading={hasDataSetsLoading}>
        {/* TODO: Update when non-BAU users can create API data sets  */}
        {user?.permissions.isBauUser && (
          <>
            {canUpdateRelease ? (
              <ApiDataSetCreateModal
                publicationId={release.publicationId}
                releaseId={release.id}
                onSubmit={async ({ releaseFileId }) => {
                  const dataSet = await apiDataSetService.createDataSet({
                    releaseFileId,
                  });
                  refetch();
                  return dataSet;
                }}
              />
            ) : (
              <WarningMessage>
                This release has been approved and API data sets can no longer
                be created for it.
              </WarningMessage>
            )}
          </>
        )}

        {dataSets.length > 0 ? (
          <>
            {draftDataSets.length > 0 && (
              <>
                <h3>Draft API data sets</h3>

                <DraftApiDataSetsTable
                  canUpdateRelease={canUpdateRelease}
                  dataSets={draftDataSets}
                  publicationId={release.publicationId}
                  releaseId={release.id}
                />
              </>
            )}

            {liveDataSets.length > 0 && (
              <>
                <h3>Current live API data sets</h3>

                <LiveApiDataSetsTable
                  canUpdateRelease={canUpdateRelease}
                  dataSets={liveDataSets}
                  publicationId={release.publicationId}
                  releaseVersionId={release.id}
                  releaseId={release.releaseId}
                />
              </>
            )}
          </>
        ) : (
          <InsetText>
            No API data sets have been created for this publication.
          </InsetText>
        )}
      </LoadingSpinner>
    </>
  );
}
