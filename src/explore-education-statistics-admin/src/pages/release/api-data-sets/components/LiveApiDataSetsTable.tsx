import Link from '@admin/components/Link';
import {
  releaseApiDataSetDetailsRoute,
  ReleaseDataSetRouteParams,
} from '@admin/routes/releaseRoutes';
import {
  ApiDataSetLiveVersionSummary,
  ApiDataSetSummary,
} from '@admin/services/apiDataSetService';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import InsetText from '@common/components/InsetText';
import Tag from '@common/components/Tag';
import VisuallyHidden from '@common/components/VisuallyHidden';
import orderBy from 'lodash/orderBy';
import React from 'react';
import { generatePath } from 'react-router-dom';
import styles from './LiveApiDataSetsTable.module.scss';

export interface LiveApiDataSetSummary extends ApiDataSetSummary {
  latestLiveVersion: ApiDataSetLiveVersionSummary;
}

interface Props {
  // TODO: EES-4374 Remove when new versions can be created
  canCreateNewVersions?: boolean;
  canUpdateRelease?: boolean;
  dataSets: LiveApiDataSetSummary[];
  publicationId: string;
  releaseId: string;
}

export default function LiveApiDataSetsTable({
  canCreateNewVersions,
  canUpdateRelease,
  dataSets,
  publicationId,
  releaseId,
}: Props) {
  if (!dataSets.length) {
    return <InsetText>No live API data sets for this publication.</InsetText>;
  }

  const orderedDataSets = orderBy(dataSets, dataSet => dataSet.title);

  return (
    <table className={styles.table} data-testid="live-api-data-sets">
      <thead>
        <tr>
          <th scope="col">Version</th>
          <th scope="col">Name</th>
          <th scope="col">Actions</th>
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
                      releaseId,
                      dataSetId: dataSet.id,
                    },
                  )}
                >
                  View details
                  <VisuallyHidden> for {dataSet.title}</VisuallyHidden>
                </Link>
                {canUpdateRelease && canCreateNewVersions && (
                  <Button>
                    Create new version
                    <VisuallyHidden> for {dataSet.title}</VisuallyHidden>
                  </Button>
                )}
              </ButtonGroup>
            </td>
          </tr>
        ))}
      </tbody>
    </table>
  );
}
