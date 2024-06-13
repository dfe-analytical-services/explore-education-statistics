import Link from '@admin/components/Link';
import DeleteDraftVersionButton from '@admin/pages/release/api-data-sets/components/DeleteDraftVersionButton';
import getVersionStatusTagColour from '@admin/pages/release/api-data-sets/utils/getVersionStatusColour';
import getVersionStatusText from '@admin/pages/release/api-data-sets/utils/getVersionStatusText';
import {
  releaseApiDataSetDetailsRoute,
  ReleaseDataSetRouteParams,
} from '@admin/routes/releaseRoutes';
import {
  ApiDataSetDraftVersionSummary,
  ApiDataSetSummary,
} from '@admin/services/apiDataSetService';
import ButtonGroup from '@common/components/ButtonGroup';
import ButtonText from '@common/components/ButtonText';
import InsetText from '@common/components/InsetText';
import Tag from '@common/components/Tag';
import VisuallyHidden from '@common/components/VisuallyHidden';
import classNames from 'classnames';
import orderBy from 'lodash/orderBy';
import React from 'react';
import { generatePath } from 'react-router-dom';
import styles from './DraftApiDataSetsTable.module.scss';

export interface DraftApiDataSetSummary extends ApiDataSetSummary {
  draftVersion: ApiDataSetDraftVersionSummary;
}

interface Props {
  dataSets: DraftApiDataSetSummary[];
  publicationId: string;
  releaseId: string;
}

export default function DraftApiDataSetsTable({
  dataSets,
  publicationId,
  releaseId,
}: Props) {
  if (!dataSets.length) {
    return <InsetText>No draft API data sets for this publication.</InsetText>;
  }

  const hasLiveDataSets = dataSets.some(dataSet => dataSet.latestLiveVersion);
  const orderedDataSets = orderBy(dataSets, dataSet => dataSet.title);

  return (
    <table className={styles.table} data-testid="draft-api-data-sets">
      <thead>
        <tr>
          <th scope="col" style={{ width: '5%' }}>
            Draft version
          </th>
          {hasLiveDataSets && <th style={{ width: '5%' }}>Live version</th>}
          <th scope="col">Name</th>
          <th scope="col">Status</th>
          <th scope="col">Actions</th>
        </tr>
      </thead>
      <tbody>
        {orderedDataSets.map(
          ({ draftVersion, latestLiveVersion, ...dataSet }) => (
            <tr
              key={dataSet.id}
              className={classNames({
                [styles.rowHighlight]:
                  draftVersion.status === 'Failed' ||
                  draftVersion.status === 'Cancelled' ||
                  draftVersion.status === 'Mapping',
              })}
            >
              <td className="govuk-!-padding-left-1">
                <Tag
                  className={styles.versionTag}
                  colour={getVersionStatusTagColour(draftVersion.status)}
                >
                  {`v${draftVersion.version}`}
                </Tag>
              </td>
              {hasLiveDataSets ? (
                <td>
                  <Tag
                    className={styles.versionTag}
                    colour={latestLiveVersion ? 'blue' : 'grey'}
                  >
                    {latestLiveVersion?.version
                      ? `v${latestLiveVersion?.version}`
                      : 'N/A'}
                  </Tag>
                </td>
              ) : null}
              <td>{dataSet.title}</td>
              <td>
                <Tag colour={getVersionStatusTagColour(draftVersion.status)}>
                  {getVersionStatusText(draftVersion.status)}
                </Tag>
              </td>
              <td>
                <ButtonGroup
                  className="govuk-!-margin-bottom-0"
                  horizontalSpacing="m"
                >
                  {draftVersion.status !== 'Processing' && (
                    <DeleteDraftVersionButton
                      dataSet={dataSet}
                      dataSetVersion={draftVersion}
                    >
                      Delete draft
                      <VisuallyHidden> for {dataSet.title}</VisuallyHidden>
                    </DeleteDraftVersionButton>
                  )}
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
                    {draftVersion.version === '1.0'
                      ? 'View details'
                      : 'View details / edit draft'}
                    <VisuallyHidden>for {dataSet.title}</VisuallyHidden>
                  </Link>
                </ButtonGroup>
              </td>
            </tr>
          ),
        )}
      </tbody>
    </table>
  );
}
