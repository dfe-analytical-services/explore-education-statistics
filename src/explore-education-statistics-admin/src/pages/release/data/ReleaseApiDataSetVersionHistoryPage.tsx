import Link from '@admin/components/Link';
import { useConfig } from '@admin/contexts/ConfigContext';
import {
  releaseApiDataSetChangelogRoute,
  releaseApiDataSetDetailsRoute,
  ReleaseDataSetChangelogRouteParams,
  ReleaseDataSetPreviewTokenRouteParams,
  ReleaseDataSetRouteParams,
  ReleaseRouteParams,
  releaseSummaryRoute,
} from '@admin/routes/releaseRoutes';
import apiDataSetQueries from '@admin/queries/apiDataSetQueries';
import apiDataSetVersionQueries from '@admin/queries/apiDataSetVersionQueries';
import LoadingSpinner from '@common/components/LoadingSpinner';
import Tag from '@common/components/Tag';
import Pagination from '@common/components/Pagination';
import TagGroup from '@common/components/TagGroup';
import useQueryParams from '@admin/hooks/useQueryParams';
import parseNumber from '@common/utils/number/parseNumber';
import VisuallyHidden from '@common/components/VisuallyHidden';
import { useQuery } from '@tanstack/react-query';
import { generatePath, useParams } from 'react-router-dom';
import React, { useState } from 'react';

export default function ReleaseApiDataSetVersionHistoryPage() {
  const { publicAppUrl } = useConfig();
  const { page } = useQueryParams<{ page: string }>();
  const [currentPage, setCurrentPage] = useState<number>(
    parseNumber(page) ?? 1,
  );

  const { dataSetId, releaseVersionId, publicationId } =
    useParams<ReleaseDataSetPreviewTokenRouteParams>();

  const { data: dataSet, isLoading: isLoadingDataSet } = useQuery(
    apiDataSetQueries.get(dataSetId),
  );

  const { data: dataSetVersionsData, isLoading: isLoadingVersions } = useQuery({
    ...apiDataSetVersionQueries.list({
      dataSetId: dataSet?.id ?? '',
      page: currentPage,
    }),
    enabled: !!dataSet,
  });

  const { paging, results: dataSetVersions = [] } = dataSetVersionsData ?? {};

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
      <LoadingSpinner loading={isLoadingDataSet || isLoadingVersions}>
        <div className="govuk-grid-row">
          <div className="govuk-grid-column-three-quarters">
            <span className="govuk-caption-l">Version history</span>
            <h2>{dataSet?.title}</h2>
          </div>
        </div>

        {dataSetVersions?.length ? (
          <>
            <table>
              <caption className="govuk-visually-hidden">
                Table showing versions of the API data set
              </caption>
              <thead>
                <tr>
                  <th>Version</th>
                  <th>Related release</th>
                  <th>Status</th>
                  <th className="govuk-!-text-align-right">Action</th>
                </tr>
              </thead>
              <tbody>
                {dataSetVersions.map((dataSetVersion, index) => {
                  return (
                    <tr key={dataSetVersion.id}>
                      <td>{dataSetVersion.version}</td>
                      <td>
                        <Link
                          to={generatePath<ReleaseRouteParams>(
                            releaseSummaryRoute.path,
                            {
                              publicationId,
                              releaseVersionId:
                                dataSetVersion.releaseVersion.id,
                            },
                          )}
                        >
                          {dataSetVersion.releaseVersion.title}
                        </Link>
                      </td>

                      <td>
                        <TagGroup>
                          <Tag>{dataSetVersion.status}</Tag>
                          {currentPage === 1 && index === 0 && (
                            <Tag>Latest version</Tag>
                          )}
                        </TagGroup>
                      </td>

                      <td className="govuk-!-text-align-right">
                        {dataSetVersion.version !== '1.0' && (
                          <Link
                            to={generatePath<ReleaseDataSetChangelogRouteParams>(
                              releaseApiDataSetChangelogRoute.path,
                              {
                                publicationId,
                                releaseVersionId:
                                  dataSetVersion.releaseVersion.id,
                                dataSetId,
                                dataSetVersionId: dataSetVersion.id,
                              },
                            )}
                          >
                            View changelog
                            <VisuallyHidden>
                              {' '}
                              for version {dataSetVersion.version}
                            </VisuallyHidden>
                          </Link>
                        )}

                        <a
                          className="govuk-!-margin-left-2"
                          href={`${publicAppUrl}/data-catalogue/data-set/${dataSetVersion.file.id}`}
                          target="_blank"
                          rel="noopener noreferrer"
                        >
                          View live data set
                          <VisuallyHidden>
                            {' '}
                            for version {dataSetVersion.version}
                          </VisuallyHidden>{' '}
                          (opens in new tab)
                        </a>
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
            {paging && (
              <Pagination
                currentPage={currentPage}
                totalPages={paging.totalPages}
                renderLink={linkProps => <Link {...linkProps} />}
                onClick={setCurrentPage}
              />
            )}
          </>
        ) : (
          <p>No data set versions have been created.</p>
        )}
      </LoadingSpinner>
    </>
  );
}
