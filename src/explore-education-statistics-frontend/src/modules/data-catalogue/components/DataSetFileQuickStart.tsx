import InsetText from '@common/components/InsetText';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import UrlContainer from '@common/components/UrlContainer';
import VisuallyHidden from '@common/components/VisuallyHidden';
import Link from '@frontend/components/Link';
import DataSetFilePageSection from '@frontend/modules/data-catalogue/components/DataSetFilePageSection';
import { apiPageSections } from '@frontend/modules/data-catalogue/DataSetFilePage';
import React from 'react';

interface Props {
  id: string;
  name: string;
  version: string;
}

export default function DataSetFileQuickStart({ id, name, version }: Props) {
  return (
    <DataSetFilePageSection
      className="govuk-!-padding-bottom-8"
      heading={apiPageSections.quickStart}
      id="quickStart"
    >
      <InsetText>
        <p>
          If you are unfamiliar with APIs, we suggest you first read our{' '}
          <Link to={process.env.PUBLIC_API_DOCUMENTATION_URL ?? ''}>
            API documentation
          </Link>
          .
        </p>
        <p>
          The documentation provides full guidance and examples on how to make
          the most from our data sets.
        </p>
      </InsetText>
      <h3>API data set details</h3>
      <SummaryList compact noBorder>
        <SummaryListItem term="API data set name">{name}</SummaryListItem>
        <SummaryListItem term="API data set ID">{id}</SummaryListItem>
        <SummaryListItem term="Latest API version">{version}</SummaryListItem>
      </SummaryList>
      <h3>Data set summary</h3>
      <UrlContainer
        className="govuk-!-margin-bottom-1"
        id="data-set-summary-endpoint"
        label={
          <>
            GET<VisuallyHidden> data set summary</VisuallyHidden>
          </>
        }
        labelHidden={false}
        url={`${process.env.PUBLIC_API_BASE_URL}/v${process.env.PUBLIC_API_VERSION}/data-sets/${id}`}
      />
      <Link to={`${process.env.PUBLIC_API_DOCUMENTATION_URL}/TODO`}>
        Guidance, get data set summary
      </Link>
      <h3 className="govuk-!-margin-top-5">Data set meta data</h3>
      <UrlContainer
        className="govuk-!-margin-bottom-1"
        id="data-set-meta-endpoint"
        label={
          <>
            GET<VisuallyHidden> data set meta data</VisuallyHidden>
          </>
        }
        labelHidden={false}
        url={`${process.env.PUBLIC_API_BASE_URL}/v${process.env.PUBLIC_API_VERSION}/data-sets/${id}/meta`}
      />
      <Link to={`${process.env.PUBLIC_API_DOCUMENTATION_URL}/TODO`}>
        Guidance, get data set meta data
      </Link>

      <h3 className="govuk-!-margin-top-5">Query data set using GET</h3>
      <UrlContainer
        className="govuk-!-margin-bottom-1"
        id="data-set-get-query-endpoint"
        label={
          <>
            GET<VisuallyHidden> data set</VisuallyHidden>
          </>
        }
        labelHidden={false}
        url={`${process.env.PUBLIC_API_BASE_URL}/v${process.env.PUBLIC_API_VERSION}/data-sets/${id}/query`}
      />
      <Link to={`${process.env.PUBLIC_API_DOCUMENTATION_URL}/TODO`}>
        Guidance, query data set (GET)
      </Link>

      <h3 className="govuk-!-margin-top-5">Query data set using POST</h3>
      <UrlContainer
        className="govuk-!-margin-bottom-1"
        id="data-set-post-query-endpoint"
        label={
          <>
            POST<VisuallyHidden> data set</VisuallyHidden>
          </>
        }
        labelHidden={false}
        url={`${process.env.PUBLIC_API_BASE_URL}/v${process.env.PUBLIC_API_VERSION}/data-sets/${id}/query`}
      />
      <Link to={`${process.env.PUBLIC_API_DOCUMENTATION_URL}/TODO`}>
        Guidance, query data set (POST)
      </Link>
    </DataSetFilePageSection>
  );
}
