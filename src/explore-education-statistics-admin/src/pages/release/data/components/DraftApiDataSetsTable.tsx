import isPatchVersion from '@common/utils/isPatchVersion';
import Link from '@admin/components/Link';
import DeleteDraftVersionButton from '@admin/pages/release/data/components/DeleteDraftVersionButton';
import getDataSetVersionStatusTagColour from '@admin/pages/release/data/components/utils/getDataSetVersionStatusColour';
import getDataSetVersionStatusText from '@admin/pages/release/data/components/utils/getDataSetVersionStatusText';
import {
  releaseApiDataSetDetailsRoute,
  ReleaseDataSetRouteParams,
} from '@admin/routes/releaseRoutes';
import {
  ApiDataSetDraftVersionSummary,
  ApiDataSetSummary,
} from '@admin/services/apiDataSetService';
import ButtonGroup from '@common/components/ButtonGroup';
import InsetText from '@common/components/InsetText';
import Tag from '@common/components/Tag';
import VisuallyHidden from '@common/components/VisuallyHidden';
import classNames from 'classnames';
import orderBy from 'lodash/orderBy';
import React from 'react';
import { generatePath } from 'react-router-dom';
import styles from './DraftApiDataSetsTable.module.scss';

export const columnWidth = 90;

export interface DraftApiDataSetSummary extends ApiDataSetSummary {
  draftVersion: ApiDataSetDraftVersionSummary;
}

interface Props {
  canUpdateRelease: boolean;
  dataSets: DraftApiDataSetSummary[];
  publicationId: string;
  releaseVersionId: string;
}

export default function DraftApiDataSetsTable({
  canUpdateRelease,
  dataSets,
  publicationId,
  releaseVersionId,
}: Props) {
  if (!dataSets.length) {
    return <InsetText>No draft API data sets for this publication.</InsetText>;
  }

  const hasLiveDataSets = dataSets.some(dataSet => dataSet.latestLiveVersion);
  const orderedDataSets = orderBy(dataSets, dataSet => dataSet.title);

  return (
    <table
      className={`${styles.table} dfe-table--row-highlights`}
      data-testid="draft-api-data-sets"
    >
      <caption className="govuk-visually-hidden">
        Table showing draft API data sets for this publication.
      </caption>
      <thead>
        <tr>
          <th
            scope="col"
            style={{
              width: hasLiveDataSets
                ? `${columnWidth}px`
                : `${columnWidth * 2}px`,
            }}
          >
            Draft version
          </th>
          {hasLiveDataSets && (
            <th style={{ width: `${columnWidth}px` }}>Live version</th>
          )}
          <th scope="col">Name</th>
          <th scope="col">Status</th>
          <th className="govuk-!-width-one-third" scope="col">
            Actions
          </th>
        </tr>
      </thead>
      <tbody>
        {orderedDataSets.map(
          ({ draftVersion, latestLiveVersion, ...dataSet }) => {
            const rowHighlight =
              draftVersion.status === 'Failed' ||
              draftVersion.status === 'Cancelled' ||
              draftVersion.status === 'Mapping';

            const isPatch = isPatchVersion(draftVersion?.version);

            return (
              <tr
                key={dataSet.id}
                className={classNames({
                  'rowHighlight--alert': rowHighlight,
                })}
              >
                <td>
                  <Tag
                    colour={getDataSetVersionStatusTagColour(
                      draftVersion.status,
                    )}
                  >
                    {`v${draftVersion.version}`}
                  </Tag>
                </td>
                {hasLiveDataSets ? (
                  <td>
                    <Tag colour={latestLiveVersion ? 'blue' : 'grey'}>
                      {latestLiveVersion?.version
                        ? `v${latestLiveVersion?.version}`
                        : 'N/A'}
                    </Tag>
                  </td>
                ) : null}
                <td>{dataSet.title}</td>
                <td>
                  <Tag
                    colour={getDataSetVersionStatusTagColour(
                      draftVersion.status,
                    )}
                  >
                    {getDataSetVersionStatusText(draftVersion.status)}
                  </Tag>
                </td>
                <td>
                  <ButtonGroup
                    className="govuk-!-margin-bottom-0"
                    horizontalSpacing="m"
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
                      {draftVersion.version === '1.0' || !canUpdateRelease
                        ? 'View details'
                        : 'View details / edit draft'}
                      <VisuallyHidden>for {dataSet.title}</VisuallyHidden>
                    </Link>
                    {draftVersion.status !== 'Processing' &&
                      draftVersion.status !== 'Finalising' &&
                      canUpdateRelease &&
                      !isPatch && (
                        <DeleteDraftVersionButton
                          dataSet={dataSet}
                          dataSetVersion={draftVersion}
                        >
                          Remove draft
                          <VisuallyHidden> for {dataSet.title}</VisuallyHidden>
                        </DeleteDraftVersionButton>
                      )}
                  </ButtonGroup>
                </td>
              </tr>
            );
          },
        )}
      </tbody>
    </table>
  );
}
