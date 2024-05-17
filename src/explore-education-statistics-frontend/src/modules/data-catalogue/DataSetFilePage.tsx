import Tag from '@common/components/Tag';
import Button from '@common/components/Button';
import downloadService from '@common/services/downloadService';
import { Dictionary } from '@common/types';
import ContentHtml from '@common/components/ContentHtml';
import FormattedDate from '@common/components/FormattedDate';
import useToggle from '@common/hooks/useToggle';
import LoadingSpinner from '@common/components/LoadingSpinner';
import useDebouncedCallback from '@common/hooks/useDebouncedCallback';
import Page from '@frontend/components/Page';
import DataSetFilePageNav from '@frontend/modules/data-catalogue/components/DataSetFilePageNav';
import DataSetFileApiVersionHistory from '@frontend/modules/data-catalogue/components/DataSetFileApiVersionHistory';
import DataSetFilePreview from '@frontend/modules/data-catalogue/components/DataSetFilePreview';
import DataSetFileApiQuickStart from '@frontend/modules/data-catalogue/components/DataSetFileApiQuickStart';
import DataSetFileUsage from '@frontend/modules/data-catalogue/components/DataSetFileUsage';
import DataSetFileDetails from '@frontend/modules/data-catalogue/components/DataSetFileDetails';
import styles from '@frontend/modules/data-catalogue/DataSetPage.module.scss';
import NotFoundPage from '@frontend/modules/NotFoundPage';
import dataSetFileQueries from '@frontend/queries/dataSetFileQueries';
import apiDataSetQueries from '@frontend/queries/apiDataSetQueries';
import { logEvent } from '@frontend/services/googleAnalyticsService';
import withAxiosHandler from '@frontend/middleware/ssr/withAxiosHandler';
import React, { useEffect, useState } from 'react';
import { QueryClient, dehydrate, useQuery } from '@tanstack/react-query';
import { GetServerSideProps } from 'next';
import classNames from 'classnames';

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
  dataSetFileId: string;
}

export default function DataSetFilePage({ dataSetFileId }: Props) {
  const { data: dataSetFile, isLoading: isLoadingDataSetFile } = useQuery({
    ...dataSetFileQueries.get(dataSetFileId),
    keepPreviousData: true,
    staleTime: 60000,
    retry: false,
  });

  const apiDataSetId = dataSetFile?.api?.id ?? '';

  const { data: apiDataSetVersion, isLoading: isLoadingApiDataSetVersion } =
    useQuery({
      ...apiDataSetQueries.getDataSetVersion(
        apiDataSetId,
        dataSetFile?.api?.version ?? '',
      ),
      keepPreviousData: true,
      staleTime: 60000,
      retry: false,
      enabled: !!dataSetFile?.api,
    });

  const { data: apiDataSet, isLoading: isLoadingApiDataSet } = useQuery({
    ...apiDataSetQueries.getDataSet(apiDataSetId),
    keepPreviousData: true,
    staleTime: 60000,
    retry: false,
    enabled: !!dataSetFile?.api,
  });

  const { data: apiDataSetVersions, isLoading: isLoadingApiDataSetVersions } =
    useQuery({
      ...apiDataSetQueries.listDataSetVersions(apiDataSetId),
      keepPreviousData: true,
      staleTime: 60000,
      retry: false,
      enabled: !!dataSetFile?.api,
    });

  const [activeSection, setActiveSection] =
    useState<PageSectionId>('dataSetDetails');
  const [fullScreenPreview, toggleFullScreenPreview] = useToggle(false);
  const [showAllPreview, toggleShowAllPreview] = useToggle(false);

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
      if (!section) {
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
        window.history.pushState({}, '', `#${pageSectionId}`);
      }
    });
  }, 10);

  useEffect(() => {
    window.addEventListener('scroll', handleScroll);

    return () => {
      window.removeEventListener('scroll', handleScroll);
    };
  }, [handleScroll]);

  const isLoading = apiDataSet
    ? isLoadingDataSetFile ||
      isLoadingApiDataSetVersion ||
      isLoadingApiDataSet ||
      isLoadingApiDataSetVersions
    : isLoadingDataSetFile;

  if (isLoading) {
    return <LoadingSpinner />;
  }

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

          <hr className="govuk-!-margin-bottom-8 govuk-!-margin-top-6" />

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
                onClickDownload={handleDownload}
              />

              {apiDataSetVersions && apiDataSetVersion && (
                <DataSetFileApiVersionHistory
                  currentVersion={apiDataSetVersion.version}
                  dataSetFileId={dataSetFileId}
                  dataSetVersions={apiDataSetVersions}
                />
              )}
              {apiDataSet && apiDataSetVersion && (
                <DataSetFileApiQuickStart
                  id={apiDataSet.id}
                  name={apiDataSet.title}
                  version={apiDataSetVersion.version}
                />
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
    const { dataSetFileId, version } = context.query as Dictionary<string>;

    const queryClient = new QueryClient();

    const dataSet = await queryClient.fetchQuery(
      dataSetFileQueries.get(dataSetFileId),
    );

    if (dataSet.api) {
      const dataSetVersion = version ?? dataSet.api?.version;

      await Promise.all([
        queryClient.fetchQuery(apiDataSetQueries.getDataSet(dataSet.api.id)),
        queryClient.fetchQuery(
          apiDataSetQueries.getDataSetVersion(dataSet.api.id, dataSetVersion),
        ),
        queryClient.fetchQuery(
          apiDataSetQueries.listDataSetVersions(dataSet.api.id),
        ),
      ]);
    }

    const props: Props = {
      dataSetFileId,
    };

    return {
      props: { ...props, dehydratedState: dehydrate(queryClient) },
    };
  },
);
