import Button from '@common/components/Button';
import ContentHtml from '@common/components/ContentHtml';
import FormattedDate from '@common/components/FormattedDate';
import SectionBreak from '@common/components/SectionBreak';
import Tag from '@common/components/Tag';
import useDebouncedCallback from '@common/hooks/useDebouncedCallback';
import useToggle from '@common/hooks/useToggle';
import downloadService from '@common/services/downloadService';
import { Dictionary } from '@common/types';
import Page from '@frontend/components/Page';
import withAxiosHandler from '@frontend/middleware/ssr/withAxiosHandler';
import DataSetFileApiQuickStart from '@frontend/modules/data-catalogue/components/DataSetFileApiQuickStart';
import DataSetFileApiVersionHistory from '@frontend/modules/data-catalogue/components/DataSetFileApiVersionHistory';
import DataSetFileDetails from '@frontend/modules/data-catalogue/components/DataSetFileDetails';
import DataSetFilePageNav from '@frontend/modules/data-catalogue/components/DataSetFilePageNav';
import DataSetFilePreview from '@frontend/modules/data-catalogue/components/DataSetFilePreview';
import DataSetFileUsage from '@frontend/modules/data-catalogue/components/DataSetFileUsage';
import DataSetFileVariables from '@frontend/modules/data-catalogue/components/DataSetFileVariables';
import DataSetFileFootnotes from '@frontend/modules/data-catalogue/components/DataSetFileFootnotes';
import styles from '@frontend/modules/data-catalogue/DataSetPage.module.scss';
import NotFoundPage from '@frontend/modules/NotFoundPage';
import apiDataSetQueries from '@frontend/queries/apiDataSetQueries';
import dataSetFileQueries from '@frontend/queries/dataSetFileQueries';
import {
  ApiDataSet,
  ApiDataSetVersion,
} from '@frontend/services/apiDataSetService';
import { DataSetFile } from '@frontend/services/dataSetFileService';
import { logEvent } from '@frontend/services/googleAnalyticsService';
import { dehydrate, QueryClient } from '@tanstack/react-query';
import classNames from 'classnames';
import { GetServerSideProps } from 'next';
import React, { useEffect, useState } from 'react';
import omit from 'lodash/omit';

export const pageBaseSections = {
  dataSetDetails: 'Data set details',
  dataSetPreview: 'Data set preview',
  dataSetVariables: 'Variables in this data set',
  dataSetFootnotes: 'Footnotes',
  dataSetUsage: 'Using this data',
} as const;

export const pageApiSections = {
  apiVersionHistory: 'API data set version history',
  apiQuickStart: 'API data set quick start',
} as const;

export const pageSections = {
  ...pageBaseSections,
  ...pageApiSections,
} as const;

export type PageSection = typeof pageSections;
export type PageSectionId = keyof PageSection;

interface Props {
  apiDataSet?: ApiDataSet;
  apiDataSetVersion?: ApiDataSetVersion;
  dataSetFile: DataSetFile;
}

export default function DataSetFilePage({
  apiDataSet,
  apiDataSetVersion,
  dataSetFile,
}: Props) {
  const [activeSection, setActiveSection] =
    useState<PageSectionId>('dataSetDetails');
  const [fullScreenPreview, toggleFullScreenPreview] = useToggle(false);

  const handleDownload = async () => {
    await downloadService.downloadFiles(release.id, [file.id]);

    logEvent({
      category: 'Data catalogue - data set page',
      action: 'Data set file download',
      label: `Publication: ${release.publication.title}, Release: ${release.title}, Data set: ${title}`,
    });
  };

  const [handleScroll] = useDebouncedCallback(() => {
    const sections = document.querySelectorAll('[data-page-section]');

    // Set a section as active when it's in the top third of the page.
    const buffer = window.innerHeight / 3;
    const scrollPosition = window.scrollY + buffer;

    sections.forEach(section => {
      if (!section || section.id === activeSection) {
        return;
      }

      const { height } = section.getBoundingClientRect();
      const { offsetTop } = section as HTMLElement;
      const offsetBottom = offsetTop + height;

      const pageSectionId = section.id as PageSectionId;

      if (
        scrollPosition > offsetTop &&
        scrollPosition < offsetBottom &&
        pageSections[pageSectionId]
      ) {
        setActiveSection(pageSectionId);
      }
    });
  }, 10);

  useEffect(() => {
    window.addEventListener('scroll', handleScroll);

    return () => {
      window.removeEventListener('scroll', handleScroll);
    };
  }, [handleScroll]);

  if (!dataSetFile) {
    return <NotFoundPage />;
  }

  const { file, release, summary, title } = dataSetFile;

  const allNavSections = apiDataSet ? pageSections : pageBaseSections;
  const navSections = dataSetFile.footnotes.length
    ? allNavSections
    : omit(allNavSections, 'dataSetFootnotes');

  return (
    <Page
      title={title}
      caption={`Data set from ${release.publication.title}`}
      breadcrumbs={[{ name: 'Data catalogue', link: '/data-catalogue' }]}
      wide={fullScreenPreview}
    >
      {fullScreenPreview ? (
        <DataSetFilePreview
          dataCsvPreview={dataSetFile.file.dataCsvPreview}
          fullScreen={fullScreenPreview}
          onToggleFullScreen={toggleFullScreenPreview}
        />
      ) : (
        <>
          <div className={styles.info} data-testid="data-set-file-info">
            {release.isLatestPublishedRelease && !release.isSuperseded ? (
              <Tag className="govuk-!-margin-right-5">Latest data</Tag>
            ) : (
              <Tag className="govuk-!-margin-right-5" colour="orange">
                Not the latest data
              </Tag>
            )}
            {apiDataSetVersion && (
              <div className="govuk-!-font-size-16 govuk-!-margin-right-5">
                <span className={styles.infoSectionHeading}>API version</span>{' '}
                {apiDataSetVersion.version}
              </div>
            )}
            <div className="govuk-!-font-size-16 govuk-!-margin-right-5">
              <span className={styles.infoSectionHeading}>Published</span>{' '}
              <FormattedDate format="d MMMM yyyy">
                {release.published}
              </FormattedDate>
            </div>
            <div className="govuk-!-font-size-16 govuk-!-margin-right-5">
              <span className={styles.infoSectionHeading}>Last updated</span>{' '}
              <FormattedDate format="d MMMM yyyy">
                {release.lastUpdated}
              </FormattedDate>
            </div>

            <Button
              className={classNames(
                'govuk-!-margin-bottom-0',
                styles.infoDownloadButton,
              )}
              onClick={handleDownload}
            >
              Download data set (ZIP)
            </Button>
          </div>

          <ContentHtml html={summary} />

          <SectionBreak size="l" />

          <div className="govuk-grid-row">
            <DataSetFilePageNav
              activeSection={activeSection}
              sections={navSections}
              onClickItem={setActiveSection}
            />

            <div className="govuk-grid-column-two-thirds">
              <DataSetFileDetails dataSetFile={dataSetFile} />

              <DataSetFilePreview
                dataCsvPreview={dataSetFile.file.dataCsvPreview}
                fullScreen={fullScreenPreview}
                onToggleFullScreen={() => {
                  toggleFullScreenPreview();
                  window.scrollTo(0, 0);
                }}
              />

              <DataSetFileVariables variables={dataSetFile.file.variables} />

              {!!dataSetFile.footnotes.length && (
                <DataSetFileFootnotes footnotes={dataSetFile.footnotes} />
              )}

              <DataSetFileUsage
                hasApiDataSet={!!apiDataSet}
                tableToolLink={`/data-tables/${release.publication.slug}/${release.slug}?subjectId=${file.subjectId}`}
                onDownload={handleDownload}
              />

              {apiDataSet && apiDataSetVersion && (
                <>
                  <DataSetFileApiVersionHistory
                    apiDataSetId={apiDataSet?.id}
                    currentVersion={apiDataSetVersion.version}
                  />

                  <DataSetFileApiQuickStart
                    id={apiDataSet.id}
                    name={apiDataSet.title}
                    version={apiDataSetVersion.version}
                  />
                </>
              )}
            </div>
          </div>
        </>
      )}
    </Page>
  );
}

export const getServerSideProps: GetServerSideProps<Props> = withAxiosHandler(
  async context => {
    const { dataSetFileId, versionPage } = context.query as Dictionary<string>;

    const queryClient = new QueryClient();

    const dataSetFile = await queryClient.fetchQuery(
      dataSetFileQueries.get(dataSetFileId),
    );

    const props: Props = {
      dataSetFile,
    };

    if (dataSetFile.api) {
      const [apiDataSet, apiDataSetVersion] = await Promise.all([
        await queryClient.fetchQuery(
          apiDataSetQueries.getDataSet(dataSetFile.api.id),
        ),
        await queryClient.fetchQuery(
          apiDataSetQueries.getDataSetVersion(
            dataSetFile.api.id,
            dataSetFile.api.version,
          ),
        ),
        await queryClient.fetchQuery(
          apiDataSetQueries.listDataSetVersions(dataSetFile.api.id, {
            page: versionPage ? Number(versionPage) : 1,
          }),
        ),
      ]);

      props.apiDataSet = apiDataSet;
      props.apiDataSetVersion = apiDataSetVersion;
    }

    return {
      props: {
        ...props,
        dehydratedState: dehydrate(queryClient),
      },
    };
  },
);
