import FormattedDate from '@common/components/FormattedDate';
import DownloadCsvButton from '@common/modules/table-tool/components/DownloadCsvButton';
import DownloadExcelButton from '@common/modules/table-tool/components/DownloadExcelButton';
import TimePeriodDataTable from '@common/modules/table-tool/components/TimePeriodDataTable';
import mapFullTable from '@common/modules/table-tool/utils/mapFullTable';
import mapTableHeadersConfig from '@common/modules/table-tool/utils/mapTableHeadersConfig';
import permalinkService, { Permalink } from '@common/services/permalinkService';
import ButtonLink from '@frontend/components/ButtonLink';
import Page from '@frontend/components/Page';
import PrintThisPage from '@frontend/components/PrintThisPage';
import { GetServerSideProps, NextPage, NextPageContext } from 'next';
import React, { useRef } from 'react';
import styles from './PermalinkPage.module.scss';

interface Props {
  data: Permalink;
}

const PermalinkPage: NextPage<Props> = ({ data }) => {
  const tableRef = useRef<HTMLDivElement>(null);
  const fullTable = mapFullTable(data.fullTable);
  const tableHeadersConfig = mapTableHeadersConfig(
    data.configuration.tableHeaders,
    fullTable.subjectMeta,
  );

  const { subjectName, publicationName } = fullTable.subjectMeta;

  return (
    <Page
      title={`'${subjectName}' from '${publicationName}'`}
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
            <dt className="govuk-caption-m">Created: </dt>
            <dd data-testid="created-date">
              <strong>
                <FormattedDate>{data.created}</FormattedDate>
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
        </div>
      </div>
      <div ref={tableRef}>
        <TimePeriodDataTable
          fullTable={fullTable}
          source={`${publicationName}, ${subjectName}`}
          tableHeadersConfig={tableHeadersConfig}
        />
      </div>

      <div className={styles.hidePrint}>
        <ul className="govuk-list">
          <li>
            <DownloadCsvButton
              fileName={`permalink-${data.id}`}
              fullTable={fullTable}
            />
          </li>
          <li>
            <DownloadExcelButton
              tableRef={tableRef}
              fileName={`permalink-${data.id}`}
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
        <ButtonLink to="/data-tables">Create tables</ButtonLink>
      </div>
    </Page>
  );
};

export const getServerSideProps: GetServerSideProps<Props> = async ({
  query,
}) => {
  const { permalink } = query;
  const data = await permalinkService.getPermalink(permalink as string);

  return {
    props: {
      data,
    },
  };
};

export default PermalinkPage;
