import ChevronCard from '@common/components/ChevronCard';
import ChevronGrid from '@common/components/ChevronGrid';
import Link from '@frontend/components/Link';
import ApiDataSetQuickStart from '@common/modules/data-catalogue/components/ApiDataSetQuickStart';
import DataSetFilePageSection from '@frontend/modules/data-catalogue/components/DataSetFilePageSection';
import { pageSections } from '@frontend/modules/data-catalogue/DataSetFilePage';
import React from 'react';

interface Props {
  id: string;
  name: string;
  version: string;
}

export default function DataSetFileApiQuickStart({ id, name, version }: Props) {
  return (
    <DataSetFilePageSection heading={pageSections.api} id="api">
      <ChevronGrid>
        <ChevronCard
          cardSize="l"
          description="This data set is available via an API, follow the link to get all the information to get started. 
          The documentation provides full guidance and examples on how to make the most from our data sets."
          headingSize="m"
          link={
            <Link to={process.env.PUBLIC_API_DOCS_URL}>API documentation</Link>
          }
        />
        <ChevronCard
          cardSize="l"
          description="To support building integrations on top of the explore education statistics API, software development 
          kits (SDKs) are provided to streamline common tasks and communication with the API."
          headingSize="m"
          link={
            <Link
              to={`${process.env.PUBLIC_API_DOCS_URL}/getting-started/building-api-integrations/`}
            >
              Building API integrations
            </Link>
          }
        />
      </ChevronGrid>
      <ApiDataSetQuickStart
        dataSetId={id}
        dataSetName={name}
        dataSetVersion={version}
        publicApiBaseUrl={process.env.PUBLIC_API_BASE_URL}
        publicApiDocsUrl={process.env.PUBLIC_API_DOCS_URL}
        renderLink={linkProps => <Link {...linkProps} />}
      />
    </DataSetFilePageSection>
  );
}
