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
import { useRouter } from 'next/router';
import React, { useEffect, useState } from 'react';

// TODO EES-4856
export const pageHiddenSections = {
  dataSetPreview: 'Data set preview',
  dataSetVariables: 'Variables in this data set',
  footnotes: 'Footnotes',
} as const;

export const pageBaseSections = {
  dataSetDetails: 'Data set details',
  dataSetUsage: 'Using this data',
  // TODO EES-4856
  // dataSetPreview: 'Data set preview',
  // dataSetVariables: 'Variables in this data set',
  // footnotes: 'Footnotes',
} as const;

export const pageApiSections = {
  apiVersionHistory: 'API data set version history',
  apiQuickStart: 'API data set quick start',
} as const;

export const pageSections = {
  ...pageBaseSections,
  ...pageApiSections,
} as const;

export type PageHiddenSection = typeof pageHiddenSections;
export type PageSection = typeof pageSections;

export type PageHiddenSectionId = keyof PageHiddenSection;
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
  const [showAllPreview, toggleShowAllPreview] = useToggle(false);
  const router = useRouter();

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

        router.push(
          {
            pathname: `/data-catalogue/data-set/${dataSetFile.id}`,
            hash: pageSectionId,
          },
          undefined,
          {
            shallow: true,
          },
        );
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

  return (
    <Page
      title={title}
      caption={`Data set from ${release.publication.themeTitle}`}
      breadcrumbs={[{ name: 'Data catalogue', link: '/data-catalogue' }]}
      wide={fullScreenPreview}
    >
      {fullScreenPreview ? (
        <DataSetFilePreview
          fullScreen={fullScreenPreview}
          showAll={showAllPreview}
          onToggleFullScreen={toggleFullScreenPreview}
          onToggleShowAll={toggleShowAllPreview}
        />
      ) : (
        <>
          <div className={styles.info} data-testid="data-set-file-info">
            {release.isLatestPublishedRelease ? (
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
              sections={apiDataSet ? pageSections : pageBaseSections}
              onClickItem={setActiveSection}
            />

            <div className="govuk-grid-column-two-thirds">
              <DataSetFileDetails dataSetFile={dataSetFile} />

              {/* TODO EES-4856 */}
              {/* <DataSetFilePreview
                fullScreen={fullScreenPreview}
                showAll={showAllPreview}
                onToggleFullScreen={() => {
                  toggleFullScreenPreview();
                  window.scrollTo(0, 0);
                }}
                onToggleShowAll={toggleShowAllPreview}
              /> */}

              {/* TODO EES-4856 */}
              {/* <DataSetFileVariables /> */}

              {/* TODO EES-4856 */}
              {/* <DataSetFileFootnotes /> */}

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
