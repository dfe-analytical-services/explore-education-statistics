import FormattedDate from '@common/components/FormattedDate';
import DownloadCsvButton from '@common/modules/table-tool/components/DownloadCsvButton';
import TimePeriodDataTable from '@common/modules/table-tool/components/TimePeriodDataTable';
import permalinkService from '@common/modules/table-tool/services/permalinkService';
import { Permalink } from '@common/modules/table-tool/types/permalink';
import mapPermalink from '@common/modules/table-tool/utils/mapPermalink';
import mapTableHeadersConfig from '@common/modules/table-tool/utils/mapTableHeadersConfig';
import getDefaultTableHeaderConfig from '@common/modules/table-tool/utils/tableHeaders';
import ButtonLink from '@frontend/components/ButtonLink';
import Page from '@frontend/components/Page';
import PrintThisPage from '@frontend/components/PrintThisPage';
import { NextPageContext } from 'next';
import React, { Component } from 'react';
import styles from './PermalinkPage.module.scss';

interface Props {
  permalink: string;
  data: Permalink;
}

class PermalinkPage extends Component<Props> {
  public static async getInitialProps({ query }: NextPageContext) {
    const { permalink } = query;

    const request = permalinkService.getPermalink(permalink as string);

    const data = await request;

    return {
      data,
      permalink,
    };
  }

  public render() {
    const { data } = this.props;
    const { fullTable, query } = mapPermalink(data);
    const { configuration } = query;

    return (
      <Page
        title={`'${fullTable.subjectMeta.subjectName}' from '${fullTable.subjectMeta.publicationName}'`}
        caption="Permanent data table"
        wide
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
            <PrintThisPage
              alignCentre
              noMargin
              analytics={{
                category: 'Page print',
                action: 'Print this page link selected',
              }}
            />
            {/* <RelatedAside>
              <h3>Related content</h3>
            </RelatedAside> */}
          </div>
        </div>
        <TimePeriodDataTable
          fullTable={fullTable}
          tableHeadersConfig={
            configuration.tableHeadersConfig
              ? mapTableHeadersConfig(
                  configuration.tableHeadersConfig,
                  fullTable.subjectMeta,
                )
              : getDefaultTableHeaderConfig(fullTable.subjectMeta)
          }
        />
        <div className={styles.hidePrint}>
          <DownloadCsvButton
            publicationSlug={`permalink-${data.created}-${data.title}`}
            fullTable={fullTable}
          />
          <p className="govuk-body-s">
            Source: DfE prototype example statistics
          </p>
          <h2 className="govuk-heading-m govuk-!-margin-top-9">
            Create your own tables online
          </h2>
          <p>
            Use our tool to build tables using our range of national and
            regional data.
          </p>
          <ButtonLink as="/data-tables/" href="/data-tables">
            Create tables
          </ButtonLink>
        </div>
      </Page>
    );
  }
}

export default PermalinkPage;
