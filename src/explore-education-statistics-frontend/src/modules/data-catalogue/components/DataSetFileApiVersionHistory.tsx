import Tag from '@common/components/Tag';
import { PaginatedList } from '@common/services/types/pagination';
import Link from '@frontend/components/Link';
import DataSetFilePageSection from '@frontend/modules/data-catalogue/components/DataSetFilePageSection';
import { pageApiSections } from '@frontend/modules/data-catalogue/DataSetFilePage';
import { ApiDataSetVersion } from '@frontend/services/apiDataSetService';
import React from 'react';

interface Props {
  currentVersion: string;
  dataSetFileId: string;
  dataSetVersions: PaginatedList<ApiDataSetVersion>;
}

export default function DataSetFileApiVersionHistory({
  currentVersion,
  dataSetFileId,
  dataSetVersions,
}: Props) {
  return (
    <DataSetFilePageSection
      heading={pageApiSections.apiVersionHistory}
      id="apiVersionHistory"
    >
      <table>
        <thead>
          <tr>
            <th>Version</th>
            <th>Status</th>
          </tr>
        </thead>
        <tbody>
          {dataSetVersions.results.map((version, index) => (
            <tr key={`version-${index.toString()}`}>
              <td>
                {version.version !== currentVersion ? (
                  <Link
                    to={`/data-catalogue/data-set/${dataSetFileId}/${version.version}`}
                  >
                    {version.version}
                  </Link>
                ) : (
                  <>{version.version}</>
                )}
              </td>

              <td>
                <Tag
                  colour={version.status !== 'Published' ? 'orange' : 'blue'}
                >
                  {version.status}
                </Tag>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </DataSetFilePageSection>
  );
}
