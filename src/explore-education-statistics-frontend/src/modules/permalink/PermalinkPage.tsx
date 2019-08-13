import FormattedDate from '@common/components/FormattedDate';
import permalinkService, {
  Permalink,
} from '@frontend/services/permalinkService';
import ButtonLink from '@frontend/components/ButtonLink';
import Page from '@frontend/components/Page';
import PrintThisPage from '@frontend/components/PrintThisPage';
import { NextContext } from 'next';
import React, { Component } from 'react';
import TimePeriodDataTable from '../table-tool/components/TimePeriodDataTable';

interface Props {
  permalink: string;
  data: Permalink;
}

class PermalinkPage extends Component<Props> {
  public static async getInitialProps({
    query,
  }: NextContext<{
    permalink: string;
  }>) {
    const { permalink } = query;

    const request = permalinkService.getPermalink(permalink);

    const data = await request;

    return {
      data,
      permalink,
    };
  }

  public render() {
    const { data } = this.props;
    const { fullTable, configuration } = data;
    return (
      <Page
        title={data.title}
        caption="Permanent data table"
        breadcrumbs={[
          { name: 'Data tables', link: '/data-tables' },
          { name: 'Permanent link', link: '/data-tables' },
        ]}
      >
        <div className="govuk-grid-row">
          <div className="govuk-grid-column-two-thirds">
            <dl className="dfe-meta-content govuk-!-margin-bottom-9">
              <dt className="govuk-caption-m">Created:</dt>
              <dd data-testid="created-date">
                <strong>
                  {' '}
                  <FormattedDate>{data.created}</FormattedDate>{' '}
                </strong>
              </dd>
            </dl>
          </div>
          <div className="govuk-grid-column-one-third">
            {/* <RelatedAside>
              <h3>Related content</h3>
            </RelatedAside> */}
          </div>
        </div>

        <TimePeriodDataTable
          {...fullTable.subjectMeta}
          results={fullTable.results}
          tableHeadersConfig={configuration.tableHeadersConfig}
        />

        <p className="govuk-body-s">Source: DfE prototype example statistics</p>
        <h2 className="govuk-heading-m govuk-!-margin-top-9">
          Create your own tables online
        </h2>
        <p>
          Use our tool to build tables using our range of national and regional
          data.
        </p>
        <ButtonLink prefetch as="/data-tables/" href="/data-tables">
          Create tables
        </ButtonLink>

        <PrintThisPage
          analytics={{
            category: 'Page print',
            action: 'Print this page link selected',
          }}
        />
      </Page>
    );
  }
}

export default PermalinkPage;
