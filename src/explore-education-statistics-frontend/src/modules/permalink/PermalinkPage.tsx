import FormattedDate from '@common/components/FormattedDate';
import WarningMessage from '@common/components/WarningMessage';
import DownloadTable from '@common/modules/table-tool/components/DownloadTable';
import permalinkService, {
  PermalinkSnapshot,
} from '@common/services/permalinkService';
import ButtonLink from '@frontend/components/ButtonLink';
import Page from '@frontend/components/Page';
import PrintThisPage from '@frontend/components/PrintThisPage';
import styles from '@frontend/modules/permalink/PermalinkPage.module.scss';
import { logEvent } from '@frontend/services/googleAnalyticsService';
import FixedMultiHeaderDataTable from '@common/modules/table-tool/components/FixedMultiHeaderDataTable';
import { GetServerSideProps, NextPage } from 'next';
import React, { useRef } from 'react';
import { Dictionary } from '@common/types';
import DataTableCaption from '@common/modules/table-tool/components/DataTableCaption';
import withAxiosHandler from '@frontend/middleware/ssr/withAxiosHandler';

const captionId = 'dataTableCaption';
const footnotesId = 'dataTableFootnotes';

interface Props {
  data: PermalinkSnapshot;
}

const PermalinkPage: NextPage<Props> = ({ data }) => {
  const tableRef = useRef<HTMLDivElement>(null);

  const { dataSetTitle, publicationTitle, table } = data as PermalinkSnapshot;

  const { caption, footnotes, json } = table;

  return (
    <Page
      title={`'${dataSetTitle}' from '${publicationTitle}'`}
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
        <FixedMultiHeaderDataTable
          caption={<DataTableCaption title={caption} id={captionId} />}
          captionId={captionId}
          footnotes={footnotes}
          footnotesClassName="govuk-!-width-two-thirds"
          footnotesHeadingTag="h2"
          footnotesId={footnotesId}
          source={`${publicationTitle}, ${dataSetTitle}`}
          tableJson={json}
          ref={tableRef}
        />
      </div>

      <div className="govuk-!-display-none-print">
        <DownloadTable
          fileName={`permalink-${data.id}`}
          footnotes={footnotes}
          headingSize="m"
          headingTag="h2"
          tableRef={tableRef}
          tableTitle={caption}
          onCsvDownload={() => permalinkService.getPermalinkCsv(data.id)}
          onSubmit={fileFormat => {
            logEvent({
              category: 'Permalink page',
              action:
                fileFormat === 'csv'
                  ? 'CSV download button clicked'
                  : 'ODS download button clicked',
              label: caption,
            });

            permalinkService.recordDownload({
              permalinkId: data.id,
              permalinkTitle: `${dataSetTitle}' from '${publicationTitle}`,
              downloadFormat: fileFormat,
            });
          }}
        />

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

export const getServerSideProps: GetServerSideProps<Props> = withAxiosHandler(
  async ({ query }) => {
    const { permalink } = query as Dictionary<string>;

    const data = await permalinkService.getPermalink(permalink);

    return {
      props: {
        data,
      },
    };
  },
);

export default PermalinkPage;
