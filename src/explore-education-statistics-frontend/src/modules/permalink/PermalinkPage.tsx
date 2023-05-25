import FormattedDate from '@common/components/FormattedDate';
import WarningMessage from '@common/components/WarningMessage';
import useToggle from '@common/hooks/useToggle';
import DownloadTable from '@common/modules/table-tool/components/DownloadTable';
import TimePeriodDataTable from '@common/modules/table-tool/components/TimePeriodDataTable';
import mapFullTable from '@common/modules/table-tool/utils/mapFullTable';
import mapTableHeadersConfig from '@common/modules/table-tool/utils/mapTableHeadersConfig';
import permalinkService, { Permalink } from '@common/services/permalinkService';
import ButtonLink from '@frontend/components/ButtonLink';
import Page from '@frontend/components/Page';
import PrintThisPage from '@frontend/components/PrintThisPage';
import styles from '@frontend/modules/permalink/PermalinkPage.module.scss';
import { logEvent } from '@frontend/services/googleAnalyticsService';
import { GetServerSideProps, NextPage } from 'next';
import React, { useRef } from 'react';

interface Props {
  data: Permalink;
}

const PermalinkPage: NextPage<Props> = ({ data }) => {
  const [hasTableError, toggleHasTableError] = useToggle(false);
  const tableRef = useRef<HTMLDivElement>(null);
  const fullTable = mapFullTable(data.fullTable);
  const tableHeadersConfig = mapTableHeadersConfig(
    data.configuration.tableHeaders,
    fullTable,
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
      <div className="dfe-flex dfe-justify-content--space-between">
        <dl className="dfe-meta-content">
          <dt className="govuk-caption-m">Created: </dt>
          <dd data-testid="created-date">
            <strong>
              <FormattedDate>{data.created}</FormattedDate>
            </strong>
          </dd>
        </dl>

        <PrintThisPage
          onClick={() =>
            logEvent({
              category: 'Page print',
              action: 'Print this page link selected',
              label: window.location.pathname,
            })
          }
        />
      </div>

      {data.status === 'SubjectRemoved' && (
        <WarningMessage error testId="permalink-warning">
          WARNING - The data used in this table is no longer valid.
        </WarningMessage>
      )}
      {(data.status === 'NotForLatestRelease' ||
        data.status === 'PublicationSuperseded') && (
        <WarningMessage error testId="permalink-warning">
          WARNING - The data used in this table may now be out-of-date as a new
          release has been published since its creation.
        </WarningMessage>
      )}
      {data.status === 'SubjectReplacedOrRemoved' && (
        <WarningMessage error testId="permalink-warning">
          WARNING - The data used in this table may be invalid as the subject
          file has been amended or removed since its creation.
        </WarningMessage>
      )}

      <div ref={tableRef}>
        <TimePeriodDataTable
          footnotesClassName="govuk-!-width-two-thirds"
          fullTable={fullTable}
          source={`${publicationName}, ${subjectName}`}
          tableHeadersConfig={tableHeadersConfig}
          onError={message => {
            toggleHasTableError.on();
            logEvent({
              category: 'Permalink page',
              action: 'Table rendering error',
              label: message,
            });
          }}
        />
      </div>

      <div className="dfe-hide-print">
        {!hasTableError && (
          <DownloadTable
            fullTable={fullTable}
            fileName={`permalink-${data.id}`}
            onCsvDownload={() => permalinkService.getPermalinkCsv(data.id)}
            headingSize="m"
            headingTag="h2"
            tableRef={tableRef}
            onSubmit={fileFormat =>
              logEvent({
                category: 'Permalink page',
                action:
                  fileFormat === 'csv'
                    ? 'CSV download button clicked'
                    : 'ODS download button clicked',
                label: `${fullTable.subjectMeta.publicationName} between ${
                  fullTable.subjectMeta.timePeriodRange[0].label
                } and ${
                  fullTable.subjectMeta.timePeriodRange[
                    fullTable.subjectMeta.timePeriodRange.length - 1
                  ].label
                }`,
              })
            }
          />
        )}

        <h2 className="govuk-heading-m govuk-!-margin-top-9">
          Create your own tables
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
