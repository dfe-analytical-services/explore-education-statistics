import Button from '@common/components/Button';
import ContentHtml from '@common/components/ContentHtml';
import FormattedDate from '@common/components/FormattedDate';
import PageNav, { NavItem } from '@common/components/PageNav';
import SectionBreak from '@common/components/SectionBreak';
import Tag from '@common/components/Tag';
import useDebouncedCallback from '@common/hooks/useDebouncedCallback';
import useToggle from '@common/hooks/useToggle';
import { ApiDataSetVersionChanges } from '@common/services/types/apiDataSetChanges';
import { PublicationSummaryRedesign } from '@common/services/publicationService';
import { Dictionary } from '@common/types';
import Page from '@frontend/components/Page';
import SubscribeLink from '@frontend/components/SubscribeLink';
import withAxiosHandler from '@frontend/middleware/ssr/withAxiosHandler';
import DataSetFileApiChangelog from '@frontend/modules/data-catalogue/components/DataSetFileApiChangelog';
import DataSetFileApiQuickStart from '@frontend/modules/data-catalogue/components/DataSetFileApiQuickStart';
import DataSetFileApiVersionHistory from '@frontend/modules/data-catalogue/components/DataSetFileApiVersionHistory';
import DataSetFileDetails from '@frontend/modules/data-catalogue/components/DataSetFileDetails';
import DataSetFileFootnotes from '@frontend/modules/data-catalogue/components/DataSetFileFootnotes';
import DataSetFilePreview from '@frontend/modules/data-catalogue/components/DataSetFilePreview';
import DataSetFileUsage from '@frontend/modules/data-catalogue/components/DataSetFileUsage';
import DataSetFileVariables from '@frontend/modules/data-catalogue/components/DataSetFileVariables';
import styles from '@frontend/modules/data-catalogue/DataSetPage.module.scss';
import getDataSetFileMetaCSVW from '@frontend/modules/data-catalogue/utils/getDataSetFileMetaCSVW';
import DataSetFileContact from '@frontend/modules/data-catalogue/components/DataSetFileContact';
import NotFoundPage from '@frontend/modules/NotFoundPage';
import apiDataSetQueries from '@frontend/queries/apiDataSetQueries';
import dataSetFileQueries from '@frontend/queries/dataSetFileQueries';
import {
  ApiDataSet,
  ApiDataSetVersion,
} from '@frontend/services/apiDataSetService';
import downloadService from '@frontend/services/downloadService';
import { DataSetFile } from '@frontend/services/dataSetFileService';
import { logEvent } from '@frontend/services/googleAnalyticsService';
import publicationQueries from '@frontend/queries/publicationQueries';
import { dehydrate, QueryClient } from '@tanstack/react-query';
import classNames from 'classnames';
import { GetServerSideProps } from 'next';
import isPatchVersion from '@common/utils/isPatchVersion';
import React, { useEffect, useMemo, useState } from 'react';
import omit from 'lodash/omit';

export const pageBaseSections = {
  dataSetDetails: 'Data set details',
  dataSetPreview: 'Data set preview',
  dataSetVariables: 'Variables in this data set',
  dataSetFootnotes: 'Footnotes',
  dataSetUsage: 'Using this data',
} as const;

export const pageApiSections = {
  api: 'Using the API',
  apiVersionHistory: 'API data set version history',
  apiChangelog: 'API data set changelog',
} as const;

export const pageSections = {
  ...pageBaseSections,
  ...pageApiSections,
  dataSetContact: 'Contact us',
} as const;

export type PageSections = typeof pageSections;
export type PageSectionId = keyof PageSections;

interface Props {
  apiDataSet?: ApiDataSet;
  apiDataSetVersion?: ApiDataSetVersion;
  apiDataSetVersionChanges?: ApiDataSetVersionChanges | null;
  dataSetFile: DataSetFile;
  publicationSummary: PublicationSummaryRedesign;
}

export default function DataSetFilePage({
  apiDataSet,
  apiDataSetVersion,
  apiDataSetVersionChanges,
  dataSetFile,
  publicationSummary,
}: Props) {
  const [activeSection, setActiveSection] =
    useState<PageSectionId>('dataSetDetails');
  const [fullScreenPreview, toggleFullScreenPreview] = useToggle(false);

  const handleDownload = async () => {
    await downloadService.downloadZip(release.id, 'DataCatalogue', file.id);

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

  const navItems = useMemo<NavItem[]>(() => {
    let sections: Partial<Record<PageSectionId, string>> = apiDataSet
      ? pageSections
      : pageBaseSections;

    if (!dataSetFile.footnotes.length) {
      sections = omit(sections, 'dataSetFootnotes');
    }

    const hasNoApiChangelog =
      !apiDataSetVersionChanges ||
      (!Object.keys(apiDataSetVersionChanges.majorChanges).length &&
        !Object.keys(apiDataSetVersionChanges.minorChanges).length);

    const hasNoPatchApiChangelog =
      !apiDataSetVersionChanges ||
      !apiDataSetVersionChanges.patchHistory ||
      apiDataSetVersionChanges.patchHistory.every(
        change => !change || !Object.keys(change.minorChanges).length,
      );

    if (hasNoApiChangelog && hasNoPatchApiChangelog) {
      sections = omit(sections, 'apiChangelog');
    }

    // Contact should be last for both nav versions
    sections.dataSetContact = 'Contact us';

    return Object.entries(sections).map<NavItem>(([id, text]) => {
      return {
        id,
        text,
      };
    });
  }, [apiDataSet, apiDataSetVersionChanges, dataSetFile.footnotes.length]);

  if (!dataSetFile) {
    return <NotFoundPage />;
  }
  const { file, release, summary, title } = dataSetFile;

  return (
    <Page
      title={title}
      caption={`Data set from ${release.publication.title}`}
      breadcrumbs={[{ name: 'Data catalogue', link: '/data-catalogue' }]}
      width={fullScreenPreview ? 'full' : undefined}
      pageMeta={{
        additionalMeta: [getDataSetFileMetaCSVW({ dataSetFile })],
      }}
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
                <span className={styles.infoSectionHeading}>
                  API data set version
                </span>{' '}
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

          <div className={styles.subscribeContainer}>
            <ContentHtml className="dfe-flex-grow--1" html={summary} />
            {apiDataSet && (
              <SubscribeLink
                url={`/api-subscriptions/new-subscription/${dataSetFile.id}`}
              />
            )}
          </div>

          <SectionBreak size="l" />

          <div className="govuk-grid-row">
            <PageNav
              activeSection={activeSection}
              items={navItems}
              onClickItem={sectionId => {
                setActiveSection(sectionId as PageSectionId);
              }}
            />

            <div className="govuk-grid-column-three-quarters">
              <DataSetFileDetails
                apiDataSetId={apiDataSet?.id}
                dataSetFile={dataSetFile}
              />

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
                dataSetFileId={dataSetFile.id}
                hasApiDataSet={!!apiDataSet}
                tableToolLink={`/data-tables/${release.publication.slug}/${release.slug}?subjectId=${file.subjectId}`}
                onDownload={handleDownload}
              />

              {apiDataSet && apiDataSetVersion && (
                <>
                  <DataSetFileApiQuickStart
                    id={apiDataSet.id}
                    name={apiDataSet.title}
                    version={apiDataSetVersion.version}
                  />

                  <DataSetFileApiVersionHistory
                    apiDataSetId={apiDataSet.id}
                    currentVersion={apiDataSetVersion.version}
                  />

                  {apiDataSetVersionChanges && (
                    <DataSetFileApiChangelog
                      changes={apiDataSetVersionChanges}
                      guidanceNotes={apiDataSetVersion.notes}
                      version={apiDataSetVersion.version}
                      patchHistory={apiDataSetVersionChanges.patchHistory || []}
                    />
                  )}
                </>
              )}

              <DataSetFileContact
                contact={publicationSummary.contact}
                dataSetTitle={dataSetFile.title}
              />
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

    const publicationSummary = await queryClient.fetchQuery(
      publicationQueries.getPublicationSummaryRedesign(
        dataSetFile.release.publication.slug,
      ),
    );

    const props: Props = {
      dataSetFile,
      publicationSummary,
    };

    if (dataSetFile.api) {
      await queryClient.prefetchQuery({
        ...apiDataSetQueries.listDataSetVersions(dataSetFile.api.id, {
          page: versionPage ? Number(versionPage) : 1,
        }),
        staleTime: Infinity,
      });

      const [apiDataSet, apiDataSetVersion, apiDataSetVersionChanges] =
        await Promise.all([
          queryClient.fetchQuery(
            apiDataSetQueries.getDataSet(dataSetFile.api.id),
          ),
          queryClient.fetchQuery(
            apiDataSetQueries.getDataSetVersion(
              dataSetFile.api.id,
              dataSetFile.api.version,
            ),
          ),
          dataSetFile.api.version !== '1.0'
            ? queryClient.fetchQuery(
                apiDataSetQueries.getDataSetVersionChanges(
                  dataSetFile.api.id,
                  dataSetFile.api.version,
                  isPatchVersion(dataSetFile.api.version),
                ),
              )
            : null,
        ]);

      props.apiDataSet = apiDataSet;
      props.apiDataSetVersion = apiDataSetVersion;
      props.apiDataSetVersionChanges = apiDataSetVersionChanges;
    }

    return {
      props: {
        ...props,
        dehydratedState: dehydrate(queryClient),
      },
    };
  },
);
