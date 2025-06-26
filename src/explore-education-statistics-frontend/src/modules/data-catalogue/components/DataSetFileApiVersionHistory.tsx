import LoadingSpinner from '@common/components/LoadingSpinner';
import Tag from '@common/components/Tag';
import WarningMessage from '@common/components/WarningMessage';
import Link from '@frontend/components/Link';
import Pagination from '@frontend/components/Pagination';
import DataSetFilePageSection from '@frontend/modules/data-catalogue/components/DataSetFilePageSection';
import { pageApiSections } from '@frontend/modules/data-catalogue/DataSetFilePage';
import apiDataSetQueries from '@frontend/queries/apiDataSetQueries';
import { logEvent } from '@frontend/services/googleAnalyticsService';
import { useQuery } from '@tanstack/react-query';
import { useRouter } from 'next/router';
import React from 'react';

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
  });

  const { page = 1, totalPages = 1 } = data?.paging ?? {};
  const currentVersionFileId = data?.results.find(
    version => version.version === currentVersion,
  )?.file.id;
  const patchVersionHighestPatchDictionary = data?.results.reduce(
    (acc, version) => {
      const [major, minor, patch] = version.version.split('.').map(Number);
      if (patch === undefined) {
        return acc;
      }
      const key = `${major}.${minor}`;
      if (!acc[key] || patch > acc[key]) {
        acc[key] = patch;
      }
      return acc;
    },
    {} as Record<string, number>,
  );

  return (
    <DataSetFilePageSection heading={pageApiSections[sectionId]} id={sectionId}>
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
                {data?.results.map(version => (
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
                          {version.version}{' '}
                          {showCurrentTag(
                            version.version,
                            patchVersionHighestPatchDictionary,
                          )}
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
    </DataSetFilePageSection>
  );
  function showCurrentTag(
    version: string,
    latestPatchLookup: Record<string, number> | undefined,
  ): string {
    const versionParts = version.split('.');
    let isCurrent = false;
    if (versionParts.length === 3) {
      const patch = Number(versionParts[2]);
      const key = versionParts.slice(0, 2).join('.');
      if (patch === latestPatchLookup?.[key]) {
        isCurrent = true;
      }
    } else {
      // `latestPatchLookup` only contains patch versions, so in this else, we treat everything not in `latestPatchLookup` as current.
      isCurrent = latestPatchLookup?.[version] === undefined;
    }
    return isCurrent ? ' (current)' : '';
  }
}
