import Link from '@admin/components/Link';
import styles from '@admin/pages/release/data/components/LiveApiDataSetsTable.module.scss';
import ApiDataSetCreateModal from '@admin/pages/release/data/components/ApiDataSetCreateModal';
import { columnWidth } from '@admin/pages/release/data/components/DraftApiDataSetsTable';
import {
  releaseApiDataSetDetailsRoute,
  ReleaseDataSetRouteParams,
} from '@admin/routes/releaseRoutes';
import {
  ApiDataSetLiveVersionSummary,
  ApiDataSetSummary,
} from '@admin/services/apiDataSetService';
import apiDataSetVersionService from '@admin/services/apiDataSetVersionService';
import ButtonGroup from '@common/components/ButtonGroup';
import InsetText from '@common/components/InsetText';
import Tag from '@common/components/Tag';
import VisuallyHidden from '@common/components/VisuallyHidden';
import orderBy from 'lodash/orderBy';
import React from 'react';
import { generatePath, useHistory } from 'react-router-dom';

export interface LiveApiDataSetSummary extends ApiDataSetSummary {
  latestLiveVersion: ApiDataSetLiveVersionSummary;
}

interface Props {
  canUpdateRelease?: boolean;
  dataSets: LiveApiDataSetSummary[];
  publicationId: string;
  releaseVersionId: string;
  releaseId: string;
}

export default function LiveApiDataSetsTable({
  canUpdateRelease,
  dataSets,
  publicationId,
  releaseVersionId,
  releaseId,
}: Props) {
  const history = useHistory();

  if (!dataSets.length) {
    return <InsetText>No live API data sets for this publication.</InsetText>;
  }

  const orderedDataSets = orderBy(dataSets, dataSet => dataSet.title);

  return (
    <table className={styles.table} data-testid="live-api-data-sets">
      <thead>
        <tr>
          <th scope="col" style={{ width: `${columnWidth * 2}px` }}>
            Version
          </th>
          <th scope="col">Name</th>
          <th className="govuk-!-width-one-third" scope="col">
            Actions
          </th>
        </tr>
      </thead>
      <tbody>
        {orderedDataSets.map(({ latestLiveVersion, ...dataSet }) => (
          <tr key={dataSet.id}>
            <td>
              <Tag>{`v${latestLiveVersion.version}`}</Tag>
            </td>
            <td>{dataSet.title}</td>
            <td>
              <ButtonGroup
                className="govuk-!-margin-bottom-0"
                horizontalSpacing="l"
              >
                <Link
                  to={generatePath<ReleaseDataSetRouteParams>(
                    releaseApiDataSetDetailsRoute.path,
                    {
                      publicationId,
                      releaseVersionId,
                      dataSetId: dataSet.id,
                    },
                  )}
                >
                  View details
                  <VisuallyHidden> for {dataSet.title}</VisuallyHidden>
                </Link>
                {canUpdateRelease &&
                  !dataSet.previousReleaseIds.includes(releaseId) && (
                    <ApiDataSetCreateModal
                      buttonText={
                        <>
                          Create new version
                          <VisuallyHidden> for {dataSet.title}</VisuallyHidden>
                        </>
                      }
                      publicationId={publicationId}
                      releaseVersionId={releaseVersionId}
                      submitText="Confirm new data set version"
                      title="Create a new API data set version"
                      onSubmit={async ({ releaseFileId }) => {
                        await apiDataSetVersionService.createVersion({
                          dataSetId: dataSet.id,
                          releaseFileId,
                        });
                        history.push(
                          generatePath<ReleaseDataSetRouteParams>(
                            releaseApiDataSetDetailsRoute.path,
                            {
                              publicationId,
                              releaseVersionId,
                              dataSetId: dataSet.id,
                            },
                          ),
                        );
                      }}
                    />
                  )}
              </ButtonGroup>
            </td>
          </tr>
        ))}
      </tbody>
    </table>
  );
}
