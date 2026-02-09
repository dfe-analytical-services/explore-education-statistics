import LoadingSpinner from '@common/components/LoadingSpinner';
import Tag from '@common/components/Tag';
import WarningMessage from '@common/components/WarningMessage';
import Link from '@frontend/components/Link';
import Pagination from '@frontend/components/Pagination';
import DataSetFilePageSection from '@frontend/modules/data-catalogue/components/DataSetFilePageSection';
import { pageApiSections } from '@frontend/modules/data-catalogue/DataSetFilePage';
import apiDataSetQueries from '@frontend/queries/apiDataSetQueries';
import { ApiDataSetVersion } from '@frontend/services/apiDataSetService';
import { logEvent } from '@frontend/services/googleAnalyticsService';
import { useQuery } from '@tanstack/react-query';
import { useRouter } from 'next/router';
import React from 'react';
import filterHighestPatchVersions from '../utils/filterHighestPatchVersions';

const sectionId = 'apiVersionHistory';

interface Props {
  apiDataSetId: string;
  currentVersion: string;
}

export default function DataSetFileApiVersionHistory({
  apiDataSetId,
  currentVersion,
}: Props) {
  const router = useRouter();

  const { data, isLoading } = useQuery({
    ...apiDataSetQueries.listDataSetVersions(apiDataSetId, {
      page: router.query.versionPage ? Number(router.query.versionPage) : 1,
      pageSize: 10,
    }),
    staleTime: Infinity,
  });

  const { page = 1, totalPages = 1 } = data?.paging ?? {};
  const currentVersionFileId = data?.results.find(
    version => version.version === currentVersion,
  )?.file.id;

  const filteredVersions = filterHighestPatchVersions(
    data?.results?.map((version: ApiDataSetVersion) => version.version) ?? [],
  );

  return (
    <DataSetFilePageSection heading={pageApiSections[sectionId]} id={sectionId}>
      <>
        <p>API data set updates come in 3 forms: Major, Minor and Patch.</p>
        <ul className="govuk-list govuk-list--bullet">
          <li className="govuk-!-margin-bottom-0">
            Major update (e.g. 1.x.x to 2.0.0): a breaking change meaning the
            removal of columns or rows from the data set.
          </li>
          <li className="govuk-!-margin-bottom-0">
            Minor update (e.g. 1.1.x to 1.2.0): non-breaking changes made as
            part of a new release (likely with new / additional data).
          </li>
          <li className="govuk-!-margin-bottom-0">
            Patch update (e.g. 1.1.0 to 1.1.1): a non-breaking change made as
            part of a release amendment (e.g. data corrections).
          </li>
        </ul>
        <p>
          More detailed information on versioning can be found in our{' '}
          <Link
            to="https://dfe-analytical-services.github.io/explore-education-statistics-api-docs/overview/versioning/#versioning"
            target="_blank"
            rel="noopener noreferrer"
          >
            Versioning - Explore education statistics API
          </Link>
          .
        </p>
        <LoadingSpinner loading={isLoading}>
          {data?.results ? (
            <>
              <table>
                <thead>
                  <tr>
                    <th>Version</th>
                    <th>Release</th>
                    <th>Status</th>
                  </tr>
                </thead>
                <tbody>
                  {data.results
                    .filter(version =>
                      filteredVersions.includes(version.version),
                    )
                    .map(version => (
                      <tr key={version.version}>
                        <td>
                          {version.version !== currentVersion &&
                          version.file.id !== currentVersionFileId ? (
                            <Link
                              to={`/data-catalogue/data-set/${version.file.id}`}
                            >
                              {version.version}
                            </Link>
                          ) : (
                            <strong>
                              {version.version}
                              {' (current)'}
                            </strong>
                          )}
                        </td>
                        <td>{version.release.title}</td>
                        <td>
                          <Tag
                            colour={
                              version.status !== 'Published' ? 'orange' : 'blue'
                            }
                          >
                            {version.status}
                          </Tag>
                        </td>
                      </tr>
                    ))}
                </tbody>
              </table>

              {totalPages > 1 && (
                <Pagination
                  currentPage={page}
                  label="Version history pagination"
                  pageParam="versionPage"
                  shallow
                  scroll={false}
                  totalPages={totalPages}
                  onClick={pageNumber => {
                    logEvent({
                      category: 'Data catalogue - data set page',
                      action: `API data set version history pagination clicked`,
                      label: `Page ${pageNumber}`,
                    });
                  }}
                />
              )}
            </>
          ) : (
            <WarningMessage>Could not load version history</WarningMessage>
          )}
        </LoadingSpinner>
      </>
    </DataSetFilePageSection>
  );
}
