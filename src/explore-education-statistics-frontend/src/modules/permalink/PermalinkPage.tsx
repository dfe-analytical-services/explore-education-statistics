import FormattedDate from '@common/components/FormattedDate';
import DownloadCsvButton from '@common/modules/table-tool/components/DownloadCsvButton';
import DownloadExcelButton from '@common/modules/table-tool/components/DownloadExcelButton';
import TimePeriodDataTable from '@common/modules/table-tool/components/TimePeriodDataTable';
import permalinkService, {
  UnmappedPermalink,
} from '@common/services/permalinkService';
import mapPermalink from '@common/modules/table-tool/utils/mapPermalink';
import mapTableHeadersConfig from '@common/modules/table-tool/utils/mapTableHeadersConfig';
import getDefaultTableHeaderConfig from '@common/modules/table-tool/utils/tableHeaders';
import ButtonLink from '@frontend/components/ButtonLink';
import Page from '@frontend/components/Page';
import PrintThisPage from '@frontend/components/PrintThisPage';
import { NextPage, NextPageContext } from 'next';
import React, { useRef } from 'react';
import styles from './PermalinkPage.module.scss';

interface Props {
  data: UnmappedPermalink;
}

const PermalinkPage: NextPage<Props> = ({ data }) => {
  const tableRef = useRef<HTMLDivElement>(null);
  const { fullTable, query } = mapPermalink(data);
  const { configuration } = query;

  const publicationSlug = `permalink-${data.created}-${data.title}`;

  return (
    <Page
      title={`'${fullTable.subjectMeta.subjectName}' from '${fullTable.subjectMeta.publicationName}'`}
      caption="Permanent data table"
      className={styles.permalinkPage}
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
            className="dfe-align--centre govuk-!-margin-top-0"
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
      <div ref={tableRef}>
        <TimePeriodDataTable
          fullTable={fullTable}
          source="DfE prototype example statistics"
          tableHeadersConfig={
            configuration.tableHeadersConfig
              ? mapTableHeadersConfig(
                  configuration.tableHeadersConfig,
                  fullTable.subjectMeta,
                )
              : getDefaultTableHeaderConfig(fullTable.subjectMeta)
          }
        />
      </div>

      <div className={styles.hidePrint}>
        <ul className="govuk-list">
          <li>
            <DownloadCsvButton
              publicationSlug={publicationSlug}
              fullTable={fullTable}
            />
          </li>
          <li>
            <DownloadExcelButton
              tableRef={tableRef}
              publicationSlug={publicationSlug}
              subjectMeta={fullTable.subjectMeta}
            />
          </li>
        </ul>

        <h2 className="govuk-heading-m govuk-!-margin-top-9">
          Create your own tables online
        </h2>
        <p>
          Use our tool to build tables using our range of national and regional
          data.
        </p>
        <ButtonLink as="/data-tables/" href="/data-tables">
          Create tables
        </ButtonLink>
      </div>
    </Page>
  );
};

PermalinkPage.getInitialProps = async ({ query }: NextPageContext) => {
  const { permalink } = query;
  const request = permalinkService.getPermalink(permalink as string);

  const data = await request;

  return {
    data,
  };
};

export default PermalinkPage;
