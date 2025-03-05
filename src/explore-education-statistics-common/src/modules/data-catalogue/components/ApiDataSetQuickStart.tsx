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
  publicApiVersion?: 'v1';
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
  publicApiVersion = 'v1',
  renderLink,
}: Props) {
  const publicApiVersionUrl = `${publicApiBaseUrl}/${publicApiVersion}`;
  const publicApiDocsVersionUrl = `${publicApiDocsUrl}/reference-${publicApiVersion}`;

  return (
    <>
      <Heading>API data set details</Heading>

      <SummaryList compact noBorder className="govuk-!-margin-bottom-8">
        <SummaryListItem term="API data set name">
          {dataSetName}
        </SummaryListItem>
        <SummaryListItem term="API data set ID">{dataSetId}</SummaryListItem>
        <SummaryListItem term="API data set version">
          {dataSetVersion}
        </SummaryListItem>
      </SummaryList>

      <Heading>Download data set as CSV</Heading>
      <p>
        Get familiar with the data. The CSV response will render its categories
        and column headers in a human-readable format (instead of
        machine-readable IDs).
      </p>

      <UrlContainer
        className="govuk-!-margin-bottom-2"
        id="data-set-download-csv-endpoint"
        label={
          <>
            GET<VisuallyHidden> data set CSV URL</VisuallyHidden>
          </>
        }
        labelHidden={false}
        url={`${publicApiVersionUrl}/data-sets/${dataSetId}/csv?dataSetVersion=${dataSetVersion}`}
      />
      <p className="govuk-!-margin-bottom-8">
        {renderLink({
          to: `${publicApiDocsVersionUrl}/endpoints/DownloadDataSetCsv/`,
          children: 'Guidance: Download data set as CSV',
        })}
      </p>

      <Heading>Data set metadata</Heading>
      <p>
        Look up human-readable labels and their corresponding machine-readable
        IDs to help you create data set queries.
      </p>

      <UrlContainer
        className="govuk-!-margin-bottom-2"
        id="data-set-meta-endpoint"
        label={
          <>
            GET<VisuallyHidden> data set metadata URL</VisuallyHidden>
          </>
        }
        labelHidden={false}
        url={`${publicApiVersionUrl}/data-sets/${dataSetId}/meta?dataSetVersion=${dataSetVersion}`}
      />
      <p className="govuk-!-margin-bottom-8">
        {renderLink({
          to: `${publicApiDocsVersionUrl}/endpoints/GetDataSetMeta/`,
          children: 'Guidance: Get data set metadata',
        })}
      </p>

      <Heading>Query data set using GET</Heading>
      <p>Quickly test the machine-readable IDs in a range of basic queries.</p>

      <UrlContainer
        className="govuk-!-margin-bottom-2"
        id="data-set-get-query-endpoint"
        label={
          <>
            GET<VisuallyHidden> data set query URL</VisuallyHidden>
          </>
        }
        labelHidden={false}
        url={`${publicApiVersionUrl}/data-sets/${dataSetId}/query?dataSetVersion=${dataSetVersion}`}
      />
      <p className="govuk-!-margin-bottom-8">
        {renderLink({
          to: `${publicApiDocsVersionUrl}/endpoints/QueryDataSetGet/`,
          children: 'Guidance: Query data set (GET)',
        })}
      </p>

      <Heading>Query data set using POST</Heading>
      <p>
        The POST endpoint is recommended for production level pipeline queries.
        POST requires a query to be built and supplied in JSON format using
        third party software such as Postman and Insomnia or programming
        languages such as Python, R, etc.
      </p>

      <UrlContainer
        className="govuk-!-margin-bottom-2"
        id="data-set-post-query-endpoint"
        label={
          <>
            POST<VisuallyHidden> data set query URL</VisuallyHidden>
          </>
        }
        labelHidden={false}
        url={`${publicApiVersionUrl}/data-sets/${dataSetId}/query?dataSetVersion=${dataSetVersion}`}
      />
      <p className="govuk-!-margin-bottom-8">
        {renderLink({
          to: `${publicApiDocsVersionUrl}/endpoints/QueryDataSetPost/`,
          children: 'Guidance: Query data set (POST)',
        })}
      </p>

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
        url={`${publicApiVersionUrl}/data-sets/${dataSetId}`}
      />
      <p>
        {renderLink({
          to: `${publicApiDocsVersionUrl}/endpoints/GetDataSet/`,
          children: 'Guidance: Get data set summary',
        })}
      </p>
    </>
  );
}
