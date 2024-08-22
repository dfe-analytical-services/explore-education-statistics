import Link from '@admin/components/Link';
import ApiDataSetNotesForm, {
  ApiDataSetNotesFormValues,
} from '@admin/pages/release/data/components/ApiDataSetNotesForm';
import {
  releaseApiDataSetDetailsRoute,
  ReleaseDataSetChangelogRouteParams,
  ReleaseDataSetRouteParams,
} from '@admin/routes/releaseRoutes';
import apiDataSetQueries from '@admin/queries/apiDataSetQueries';
import apiDataSetVersionQueries from '@admin/queries/apiDataSetVersionQueries';
import apiDataSetVersionService from '@admin/services/apiDataSetVersionService';
import LoadingSpinner from '@common/components/LoadingSpinner';
import useToggle from '@common/hooks/useToggle';
import Button from '@common/components/Button';
import ApiDataSetChangelog from '@common/modules/data-catalogue/components/ApiDataSetChangelog';
import Tag from '@common/components/Tag';
import { useQuery } from '@tanstack/react-query';
import { generatePath, useParams } from 'react-router-dom';
import React, { useEffect } from 'react';

export default function ReleaseApiDataSetChangelogPage() {
  const { dataSetId, dataSetVersionId, releaseId, publicationId } =
    useParams<ReleaseDataSetChangelogRouteParams>();

  const {
    data: dataSet,
    isLoading: isLoadingDataSet,
    refetch: refetchDataSet,
  } = useQuery(apiDataSetQueries.get(dataSetId));

  const { data: changes, isLoading: isLoadingChanges } = useQuery(
    apiDataSetVersionQueries.getChanges(dataSetVersionId),
  );

  const isDraft = dataSet?.draftVersion?.id === dataSetVersionId;

  const [showForm, toggleShowForm] = useToggle(false);

  const dataSetVersion = isDraft
    ? dataSet?.draftVersion
    : dataSet?.latestLiveVersion;

  useEffect(() => {
    if (isDraft && !dataSetVersion?.notes) {
      toggleShowForm.on();
    }
  }, [dataSetVersion?.notes, isDraft, toggleShowForm]);

  const handleUpdateNotes = async ({ notes }: ApiDataSetNotesFormValues) => {
    await apiDataSetVersionService.updateNotes(dataSetVersionId, {
      notes,
    });
    refetchDataSet();
    toggleShowForm.off();
  };

  return (
    <>
      <Link
        back
        className="govuk-!-margin-bottom-6"
        to={generatePath<ReleaseDataSetRouteParams>(
          releaseApiDataSetDetailsRoute.path,
          {
            publicationId,
            releaseId,
            dataSetId,
          },
        )}
      >
        Back to API data set details
      </Link>
      <LoadingSpinner loading={isLoadingDataSet || isLoadingChanges}>
        <div className="govuk-grid-row">
          <div className="govuk-grid-column-three-quarters">
            <span className="govuk-caption-l">API data set changelog</span>
            <h2>{dataSet?.title}</h2>
          </div>
        </div>
        <p>
          <Tag colour={isDraft ? 'green' : 'blue'}>{`${
            isDraft ? 'Ready' : 'Published'
          } v${dataSetVersion?.version}`}</Tag>
        </p>
        <p>
          <Tag
            colour={dataSetVersion?.type === 'Major' ? 'blue' : 'grey'}
          >{`${dataSetVersion?.type} update`}</Tag>
        </p>

        {isDraft && showForm ? (
          <ApiDataSetNotesForm
            notes={dataSetVersion?.notes}
            onSubmit={handleUpdateNotes}
          />
        ) : (
          <>
            <h3>Public guidance notes</h3>
            <p>
              {dataSetVersion?.notes
                ? dataSetVersion.notes
                : 'No notes have been added for this API data set.'}
            </p>
            {isDraft && (
              <Button onClick={toggleShowForm.on}>
                Edit public guidance notes
              </Button>
            )}
          </>
        )}

        {changes && dataSetVersion && (
          <ApiDataSetChangelog
            majorChanges={changes.majorChanges}
            minorChanges={changes.minorChanges}
            version={dataSetVersion.version}
          />
        )}
      </LoadingSpinner>
    </>
  );
}
