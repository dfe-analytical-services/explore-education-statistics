import InsetText from '@common/components/InsetText';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import UrlContainer from '@common/components/UrlContainer';
import VisuallyHidden from '@common/components/VisuallyHidden';
import React, { ReactNode } from 'react';

interface Props {
  dataSetId: string;
  dataSetName: string;
  dataSetVersion: string;
  headingsTag?: 'h3' | 'h4';
  publicApiBaseUrl: string;
  publicApiDocsUrl: string;
  renderLink: ({
    children,
    toString,
  }: {
    children: ReactNode;
    to: string;
  }) => ReactNode;
}

export default function ApiDataSetQuickStart({
  dataSetId,
  dataSetName,
  dataSetVersion,
  headingsTag: Heading = 'h3',
  publicApiBaseUrl,
  publicApiDocsUrl,
  renderLink,
}: Props) {
  return (
    <>
      <InsetText>
        <p>
          If you are unfamiliar with APIs, we suggest you first read our{' '}
          {renderLink({
            to: publicApiDocsUrl,
            children: 'API documentation',
          })}
        </p>
        <p>
          The documentation provides full guidance and examples on how to make
          the best use of API data sets.
        </p>
      </InsetText>

      <Heading>API data set details</Heading>

      <SummaryList>
        <SummaryListItem term="API data set name">
          {dataSetName}
        </SummaryListItem>
        <SummaryListItem term="API data set ID">{dataSetId}</SummaryListItem>
        <SummaryListItem term="API data set version">
          {dataSetVersion}
        </SummaryListItem>
      </SummaryList>

      <Heading>Get data set summary</Heading>

      <UrlContainer
        className="govuk-!-margin-bottom-2"
        id="data-set-summary-endpoint"
        label={
          <>
            GET<VisuallyHidden> data set summary URL</VisuallyHidden>
          </>
        }
        labelHidden={false}
        url={`${publicApiBaseUrl}/data-sets/${dataSetId}`}
      />
      <p>
        {renderLink({
          to: `${publicApiDocsUrl}/endpoints/GetDataSet`,
          children: 'Guidance: Get data set summary',
        })}
      </p>

      <Heading>Get data set metadata</Heading>

      <UrlContainer
        className="govuk-!-margin-bottom-2"
        id="data-set-meta-endpoint"
        label={
          <>
            GET<VisuallyHidden> data set metadata URL</VisuallyHidden>
          </>
        }
        labelHidden={false}
        url={`${publicApiBaseUrl}/data-sets/${dataSetId}/meta?dataSetVersion=${dataSetVersion}`}
      />
      <p>
        {renderLink({
          to: `${publicApiDocsUrl}/endpoints/GetDataSetMeta`,
          children: 'Guidance: Get data set metadata',
        })}
      </p>

      <Heading>Query data set (GET)</Heading>

      <UrlContainer
        className="govuk-!-margin-bottom-2"
        id="data-set-get-query-endpoint"
        label={
          <>
            GET<VisuallyHidden> data set query URL</VisuallyHidden>
          </>
        }
        labelHidden={false}
        url={`${publicApiBaseUrl}/data-sets/${dataSetId}/query?dataSetVersion=${dataSetVersion}`}
      />
      <p>
        {renderLink({
          to: `${publicApiDocsUrl}/endpoints/QueryDataSetGet`,
          children: 'Guidance: Query data set (GET)',
        })}
      </p>

      <Heading>Query data set (POST)</Heading>

      <UrlContainer
        className="govuk-!-margin-bottom-2"
        id="data-set-post-query-endpoint"
        label={
          <>
            POST<VisuallyHidden> data set query URL</VisuallyHidden>
          </>
        }
        labelHidden={false}
        url={`${publicApiBaseUrl}/data-sets/${dataSetId}/query?dataSetVersion=${dataSetVersion}`}
      />
      <p>
        {renderLink({
          to: `${publicApiDocsUrl}/endpoints/QueryDataSetPost`,
          children: 'Guidance: Query data set (POST)',
        })}
      </p>
    </>
  );
}