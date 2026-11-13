import FormattedDate from '@common/components/FormattedDate';
import PageNav, { NavItem } from '@common/components/PageNav';
import Tag from '@common/components/Tag';
import WarningMessage from '@common/components/WarningMessage';
import useDebouncedCallback from '@common/hooks/useDebouncedCallback';
import { useMobileMedia } from '@common/hooks/useMedia';
import useToggle from '@common/hooks/useToggle';
import downloadService from '@common/services/downloadService';
import logger from '@common/services/logger';
import { PublicationSummary } from '@common/services/publicationService';
import { ApiDataSetVersionChanges } from '@common/services/types/apiDataSetChanges';
import { Dictionary } from '@common/types';
import isPatchVersion from '@common/utils/isPatchVersion';
import FullPageTitle from '@frontend/components/FullPageTitle';
import Page from '@frontend/components/Page';
import withAxiosHandler from '@frontend/middleware/ssr/withAxiosHandler';
import DataSetFileApiChangelog from '@frontend/modules/data-catalogue/components/DataSetFileApiChangelog';
import DataSetFileApiQuickStart from '@frontend/modules/data-catalogue/components/DataSetFileApiQuickStart';
import DataSetFileApiVersionHistory from '@frontend/modules/data-catalogue/components/DataSetFileApiVersionHistory';
import DataSetFileContact from '@frontend/modules/data-catalogue/components/DataSetFileContact';
import DataSetFileDetails from '@frontend/modules/data-catalogue/components/DataSetFileDetails';
import DataSetFileFootnotes from '@frontend/modules/data-catalogue/components/DataSetFileFootnotes';
import DataSetFilePreview from '@frontend/modules/data-catalogue/components/DataSetFilePreview';
import DataSetFileQuickLinks from '@frontend/modules/data-catalogue/components/DataSetFileQuickLinks';
import DataSetFileUsage from '@frontend/modules/data-catalogue/components/DataSetFileUsage';
import DataSetFileVariables from '@frontend/modules/data-catalogue/components/DataSetFileVariables';
import styles from '@frontend/modules/data-catalogue/DataSetPage.module.scss';
import getDataSetFileMetaCSVW from '@frontend/modules/data-catalogue/utils/getDataSetFileMetaCSVW';
import NotFoundPage from '@frontend/modules/NotFoundPage';
import apiDataSetQueries from '@frontend/queries/apiDataSetQueries';
import dataSetFileQueries from '@frontend/queries/dataSetFileQueries';
import publicationQueries from '@frontend/queries/publicationQueries';
import {
  ApiDataSet,
  ApiDataSetVersion,
} from '@frontend/services/apiDataSetService';
import { DataSetFile } from '@frontend/services/dataSetFileService';
import { logEvent } from '@frontend/services/googleAnalyticsService';
import { dehydrate, QueryClient } from '@tanstack/react-query';
import omit from 'lodash/omit';
import { GetServerSideProps } from 'next';
import { ReactNode, useEffect, useMemo, useState } from 'react';

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

interface InfoListItemProps {
  children: ReactNode;
  term: string;
}

const InfoListItem = ({ children, term }: InfoListItemProps) => {
  return (
    <div className={styles.infoListItem} data-testid={term}>
      <dt className={styles.infoListItemTerm} data-testid={`${term}-key`}>
        {term}
      </dt>

      <dd
        className={styles.infoListItemDefinition}
        data-testid={`${term}-value`}
      >
        {children}
      </dd>
    </div>
  );
};

interface Props {
  apiDataSet?: ApiDataSet;
  apiDataSetVersion?: ApiDataSetVersion;
  apiDataSetVersionChanges?: ApiDataSetVersionChanges | null;
  dataSetFile: DataSetFile;
  publicationSummary: PublicationSummary;
}

export default function DataSetFilePage({
  apiDataSet,
  apiDataSetVersion,
  apiDataSetVersionChanges,
  dataSetFile,
  publicationSummary,
}: Props) {
  const { isMedia: isMobileMedia } = useMobileMedia();

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

  const tableToolLink = `/data-tables/${release.publication.slug}/${release.slug}?subjectId=${file.subjectId}`;

  return (
    <Page
      title={title}
      breadcrumbs={[{ name: 'Data catalogue', link: '/data-catalogue' }]}
      width={fullScreenPreview ? 'full' : 'wide'}
      pageMeta={{
        additionalMeta: [getDataSetFileMetaCSVW({ dataSetFile })],
      }}
      pageTitleComponent={
        <div className={styles.titleWrap}>
          <FullPageTitle
            caption={`Data set from ${release.publication.title}`}
            summary={summary}
            title={title}
          />

          {/* TODO needs test checking rendering */}
          {!isMobileMedia && (
            <DataSetFileQuickLinks
              subscribeFileId={apiDataSet ? dataSetFile.id : undefined}
              handleDownload={handleDownload}
              tableToolLink={tableToolLink}
            />
          )}
        </div>
      }
    >
      {fullScreenPreview ? (
        <DataSetFilePreview
          dataCsvPreview={dataSetFile.file.dataCsvPreview}
          fullScreen={fullScreenPreview}
          onToggleFullScreen={toggleFullScreenPreview}
        />
      ) : (
        <>
          <div className={styles.infoRow} data-testid="data-set-file-info-row">
            {release.isLatestPublishedRelease && !release.isSuperseded ? (
              <Tag>Latest data</Tag>
            ) : (
              <Tag colour="orange">Not the latest data</Tag>
            )}

            {/* TODO needs test adding */}
            {apiDataSet && (
              <a
                href="#apiVersionHistory"
                className="govuk-link govuk-link--no-visited-state govuk-!-display-none-print"
              >
                API data set versions
              </a>
            )}
          </div>

          <div
            data-testid="data-set-file-info-container"
            className={styles.infoContainer}
          >
            <dl className={styles.infoList}>
              {apiDataSetVersion && (
                <InfoListItem term="API data set version">
                  {apiDataSetVersion.version}
                </InfoListItem>
              )}
              <InfoListItem term="Published">
                <FormattedDate format="d MMMM yyyy">
                  {release.published}
                </FormattedDate>
              </InfoListItem>
              <InfoListItem term="Last updated">
                <FormattedDate format="d MMMM yyyy">
                  {release.lastUpdated}
                </FormattedDate>
              </InfoListItem>
            </dl>

            {isMobileMedia && (
              <DataSetFileQuickLinks
                subscribeFileId={apiDataSet ? dataSetFile.id : undefined}
                handleDownload={handleDownload}
                tableToolLink={tableToolLink}
              />
            )}
          </div>

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
                tableToolLink={tableToolLink}
                onDownload={handleDownload}
              />

              {dataSetFile.api && (
                <>
                  <DataSetFileApiQuickStart
                    id={apiDataSet?.id}
                    name={apiDataSet?.title}
                    version={apiDataSetVersion?.version}
                  />

                  {apiDataSet && apiDataSetVersion ? (
                    <>
                      <DataSetFileApiVersionHistory
                        apiDataSetId={apiDataSet.id}
                        currentVersion={apiDataSetVersion.version}
                      />

                      <DataSetFileApiChangelog
                        changes={apiDataSetVersionChanges}
                        guidanceNotes={apiDataSetVersion.notes}
                        version={apiDataSetVersion.version}
                        patchHistory={
                          apiDataSetVersionChanges?.patchHistory || []
                        }
                      />
                    </>
                  ) : (
                    <WarningMessage>
                      API data set version history is not currently available
                    </WarningMessage>
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
      publicationQueries.getPublicationSummary(
        dataSetFile.release.publication.slug,
      ),
    );

    const props: Props = {
      dataSetFile,
      publicationSummary,
    };

    let apiDataSet: ApiDataSet | undefined;
    let apiDataSetVersion: ApiDataSetVersion | undefined;
    let apiDataSetVersionChanges: ApiDataSetVersionChanges | null | undefined;

    if (dataSetFile.api) {
      try {
        await queryClient.fetchQuery({
          ...apiDataSetQueries.listDataSetVersions(dataSetFile.api.id, {
            page: versionPage ? Number(versionPage) : 1,
          }),
          staleTime: Infinity,
        });

        apiDataSet = await queryClient.fetchQuery(
          apiDataSetQueries.getDataSet(dataSetFile.api.id),
        );

        apiDataSetVersion = await queryClient.fetchQuery(
          apiDataSetQueries.getDataSetVersion(
            dataSetFile.api.id,
            dataSetFile.api.version,
          ),
        );

        apiDataSetVersionChanges =
          dataSetFile.api.version !== '1.0'
            ? await queryClient.fetchQuery(
                apiDataSetQueries.getDataSetVersionChanges(
                  dataSetFile.api.id,
                  dataSetFile.api.version,
                  isPatchVersion(dataSetFile.api.version),
                ),
              )
            : null;
      } catch (error) {
        logger.error(error);
      }
    }

    return {
      props: {
        ...props,
        ...(apiDataSet !== undefined ? { apiDataSet } : {}),
        ...(apiDataSetVersion !== undefined ? { apiDataSetVersion } : {}),
        ...(apiDataSetVersionChanges !== undefined
          ? { apiDataSetVersionChanges }
          : {}),
        dehydratedState: dehydrate(queryClient),
      },
    };
  },
);
