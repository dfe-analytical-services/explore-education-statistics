import Link from '@admin/components/Link';
import ApiDataSetGuidanceNotesForm, {
  ApiDataSetGuidanceNotesFormValues,
} from '@admin/pages/release/data/components/ApiDataSetGuidanceNotesForm';
import {
  releaseApiDataSetDetailsRoute,
  ReleaseDataSetChangelogRouteParams,
  ReleaseDataSetRouteParams,
} from '@admin/routes/releaseRoutes';
import apiDataSetQueries from '@admin/queries/apiDataSetQueries';
import apiDataSetVersionQueries from '@admin/queries/apiDataSetVersionQueries';
import apiDataSetVersionService from '@admin/services/apiDataSetVersionService';
import LoadingSpinner from '@common/components/LoadingSpinner';
import TagGroup from '@common/components/TagGroup';
import useToggle from '@common/hooks/useToggle';
import Button from '@common/components/Button';
import ApiDataSetChangelog from '@common/modules/data-catalogue/components/ApiDataSetChangelog';
import Tag from '@common/components/Tag';
import { useQuery } from '@tanstack/react-query';
import { generatePath, useParams } from 'react-router-dom';
import React, { useEffect } from 'react';
import WarningMessage from '@common/components/WarningMessage';
import { DataSetVersionStatus } from '@admin/services/apiDataSetService';

const dataSetVersionIsDraft = (dataSetVersionStatus: DataSetVersionStatus) =>
  dataSetVersionStatus === 'Draft';

export default function ReleaseApiDataSetChangelogPage() {
  const { dataSetId, dataSetVersionId, releaseVersionId, publicationId } =
    useParams<ReleaseDataSetChangelogRouteParams>();

  const {
    data: dataSet,
    isLoading: isLoadingDataSet,
    refetch: refetchDataSet,
  } = useQuery(apiDataSetQueries.get(dataSetId));

  const { data: dataSetVersion, isLoading: isLoadingDataSetVersion } = useQuery(
    apiDataSetVersionQueries.getVersion(dataSetVersionId),
  );

  const { data: changes, isLoading: isLoadingChanges } = useQuery(
    apiDataSetVersionQueries.getChanges(dataSetVersionId),
  );

  const isDraft = dataSetVersion
    ? dataSetVersionIsDraft(dataSetVersion.status)
    : false;

  const [showForm, toggleShowForm] = useToggle(false);

  useEffect(() => {
    if (isDraft && !dataSetVersion?.notes) {
      toggleShowForm.on();
    }
  }, [dataSetVersion?.notes, isDraft, toggleShowForm]);

  const handleUpdateNotes = async ({
    notes,
  }: ApiDataSetGuidanceNotesFormValues) => {
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
            releaseVersionId,
            dataSetId,
          },
        )}
      >
        Back to API data set details
      </Link>

      <LoadingSpinner
        loading={
          isLoadingDataSet || isLoadingDataSetVersion || isLoadingChanges
        }
      >
        {dataSet && dataSetVersion ? (
          <>
            <div className="govuk-grid-row">
              <div className="govuk-grid-column-three-quarters">
                <span className="govuk-caption-l">API data set changelog</span>
                <h2>{dataSet.title}</h2>
              </div>
            </div>
            <TagGroup className="govuk-!-margin-bottom-7">
              <Tag colour={isDraft ? 'green' : 'blue'}>{`${
                isDraft ? 'Draft' : 'Published'
              } v${dataSetVersion.version}`}</Tag>
              <Tag
                colour={dataSetVersion?.type === 'Major' ? 'blue' : 'grey'}
              >{`${dataSetVersion.type} update`}</Tag>
            </TagGroup>

            {isDraft && showForm ? (
              <ApiDataSetGuidanceNotesForm
                notes={dataSetVersion.notes}
                onSubmit={handleUpdateNotes}
              />
            ) : (
              <>
                <h3>Public guidance notes</h3>
                <p>
                  {dataSetVersion?.notes ||
                    'No notes have been added for this API data set.'}
                </p>
                {isDraft && (
                  <Button onClick={toggleShowForm.on}>
                    Edit public guidance notes
                  </Button>
                )}
              </>
            )}

            {changes ? (
              <ApiDataSetChangelog
                majorChanges={changes.majorChanges}
                minorChanges={changes.minorChanges}
                version={dataSetVersion?.version ?? 'unknown'}
              />
            ) : (
              <WarningMessage>Could not load changelog</WarningMessage>
            )}
          </>
        ) : (
          <WarningMessage>Could not load API data set</WarningMessage>
        )}
      </LoadingSpinner>
    </>
  );
}
