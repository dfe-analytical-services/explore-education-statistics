import Link from '@frontend/components/Link';
import ApiDataSetQuickStart from '@common/modules/data-catalogue/components/ApiDataSetQuickStart';
import DataSetFilePageSection from '@frontend/modules/data-catalogue/components/DataSetFilePageSection';
import { pageSections } from '@frontend/modules/data-catalogue/DataSetFilePage';
import React from 'react';

const publicApiBaseUrl = process.env.PUBLIC_API_BASE_URL;
const publicApiDocsUrl = process.env.PUBLIC_API_DOCUMENTATION_URL;

interface Props {
  id: string;
  name: string;
  version: string;
}

export default function DataSetFileApiQuickStart({ id, name, version }: Props) {
  return (
    <DataSetFilePageSection
      heading={pageSections.apiQuickStart}
      id="apiQuickStart"
    >
      <ApiDataSetQuickStart
        dataSetId={id}
        dataSetName={name}
        dataSetVersion={version}
        publicApiBaseUrl={publicApiBaseUrl ?? ''}
        publicApiDocsUrl={publicApiDocsUrl ?? ''}
        renderLink={linkProps => <Link {...linkProps} />}
      />
    </DataSetFilePageSection>
  );
}
