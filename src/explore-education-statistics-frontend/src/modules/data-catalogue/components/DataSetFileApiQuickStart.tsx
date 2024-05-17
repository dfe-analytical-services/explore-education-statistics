import InsetText from '@common/components/InsetText';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import UrlContainer from '@common/components/UrlContainer';
import VisuallyHidden from '@common/components/VisuallyHidden';
import Link from '@frontend/components/Link';
import DataSetFilePageSection from '@frontend/modules/data-catalogue/components/DataSetFilePageSection';
import { pageSections } from '@frontend/modules/data-catalogue/DataSetFilePage';
import React from 'react';

const apiBaseUrl = `${process.env.PUBLIC_API_BASE_URL}/v${process.env.PUBLIC_API_VERSION}`;
const apiDocsUrl = process.env.PUBLIC_API_DOCUMENTATION_URL;

interface Props {
  id: string;
  name: string;
  version: string;
}

export default function DataSetFileApiQuickStart({ id, name, version }: Props) {
  return (
    <DataSetFilePageSection
      className="govuk-!-padding-bottom-8"
      heading={pageSections.apiQuickStart}
      id="apiQuickStart"
    >
      <InsetText>
        <p>
          If you are unfamiliar with APIs, we suggest you first read our{' '}
          <Link to={apiDocsUrl ?? ''}>API documentation</Link>.
        </p>
        <p>
          The documentation provides full guidance and examples on how to make
          the best use of API data sets.
        </p>
      </InsetText>

      <h3>API data set details</h3>

      <SummaryList>
        <SummaryListItem term="API data set name">{name}</SummaryListItem>
        <SummaryListItem term="API data set ID">{id}</SummaryListItem>
        <SummaryListItem term="API data set version">{version}</SummaryListItem>
      </SummaryList>

      <h3>Get data set summary</h3>

      <UrlContainer
        className="govuk-!-margin-bottom-2"
        id="data-set-summary-endpoint"
        label={
          <>
            GET<VisuallyHidden> data set summary URL</VisuallyHidden>
          </>
        }
        labelHidden={false}
        url={`${apiBaseUrl}/data-sets/${id}`}
      />
      <p>
        <Link to={`${apiDocsUrl}/endpoints/GetDataSet`}>
          Guidance: Get data set summary
        </Link>
      </p>

      <h3>Get data set metadata</h3>

      <UrlContainer
        className="govuk-!-margin-bottom-2"
        id="data-set-meta-endpoint"
        label={
          <>
            GET<VisuallyHidden> data set metadata URL</VisuallyHidden>
          </>
        }
        labelHidden={false}
        url={`${apiBaseUrl}/data-sets/${id}/meta?dataSetVersion=${version}`}
      />
      <p>
        <Link to={`${apiDocsUrl}/endpoints/GetDataSetMeta`}>
          Guidance: Get data set metadata
        </Link>
      </p>

      <h3>Query data set (GET)</h3>

      <UrlContainer
        className="govuk-!-margin-bottom-2"
        id="data-set-get-query-endpoint"
        label={
          <>
            GET<VisuallyHidden> data set query URL</VisuallyHidden>
          </>
        }
        labelHidden={false}
        url={`${apiBaseUrl}/data-sets/${id}/query?dataSetVersion=${version}`}
      />
      <p>
        <Link to={`${apiDocsUrl}/endpoints/QueryDataSetGet`}>
          Guidance: Query data set (GET)
        </Link>
      </p>

      <h3>Query data set (POST)</h3>

      <UrlContainer
        className="govuk-!-margin-bottom-2"
        id="data-set-post-query-endpoint"
        label={
          <>
            POST<VisuallyHidden> data set query URL</VisuallyHidden>
          </>
        }
        labelHidden={false}
        url={`${apiBaseUrl}/data-sets/${id}/query?dataSetVersion=${version}`}
      />
      <p>
        <Link to={`${apiDocsUrl}/endpoints/QueryDataSetPost`}>
          Guidance: Query data set (POST)
        </Link>
      </p>
    </DataSetFilePageSection>
  );
}
