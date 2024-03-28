import ButtonText from '@common/components/ButtonText';
import Tag from '@common/components/Tag';
import Button from '@common/components/Button';
import { releaseTypes } from '@common/services/types/releaseType';
import downloadService from '@common/services/downloadService';
import { Dictionary } from '@common/types';
import ContentHtml from '@common/components/ContentHtml';
import CollapsibleList from '@common/components/CollapsibleList';
import FormattedDate from '@common/components/FormattedDate';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import InfoIcon from '@common/components/InfoIcon';
import Modal from '@common/components/Modal';
import useToggle from '@common/hooks/useToggle';
import ReleaseTypeSection from '@common/modules/release/components/ReleaseTypeSection';
import getTimePeriodString from '@common/modules/table-tool/utils/getTimePeriodString';
import ChevronGrid from '@common/components/ChevronGrid';
import ChevronCard from '@common/components/ChevronCard';
import useDebouncedCallback from '@common/hooks/useDebouncedCallback';
import Link from '@frontend/components/Link';
import Page from '@frontend/components/Page';
import DataSetPageSection from '@frontend/modules/data-catalogue/components/DataSetPageSection';
import DataSetPageNav from '@frontend/modules/data-catalogue/components/DataSetPageNav';
import DataSetPreview from '@frontend/modules/data-catalogue/components/DataSetPreview';
import styles from '@frontend/modules/data-catalogue/DataSetPage.module.scss';
import NotFoundPage from '@frontend/modules/NotFoundPage';
import dataSetQueries from '@frontend/queries/dataSetQueries';
import { logEvent } from '@frontend/services/googleAnalyticsService';
import withAxiosHandler from '@frontend/middleware/ssr/withAxiosHandler';
import React, { useEffect, useState } from 'react';
import { QueryClient, dehydrate, useQuery } from '@tanstack/react-query';
import { GetServerSideProps } from 'next';
import { orderBy } from 'lodash';
import classNames from 'classnames';

export const pageSections: Dictionary<string> = {
  details: 'Data set details',
  using: 'Using this data',
  // TODO EES-4856
  // preview: 'Data set preview',
  // variables: 'Variables in this data set',
  // footnotes: 'Footnotes',
};

export type PageSection = keyof typeof pageSections & string;

interface Props {
  dataSetId: string;
  releaseId: string;
}

export default function DataSetPage({ dataSetId, releaseId }: Props) {
  const { data: dataSet } = useQuery({
    ...dataSetQueries.get(dataSetId),
    keepPreviousData: true,
    staleTime: 60000,
    retry: false,
  });

  const [activeSection, setActiveSection] = useState<PageSection>('details');
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
    const sections = document.querySelectorAll('[data-scroll]');

    // Set a section as active when it's in the top third of the page.
    const buffer = window.innerHeight / 3;
    const scrollPosition = window.scrollY + buffer;

    sections.forEach(section => {
      if (section) {
        const { height } = section.getBoundingClientRect();
        const { offsetTop } = section as HTMLElement;
        const offsetBottom = offsetTop + height;

        if (scrollPosition > offsetTop && scrollPosition < offsetBottom) {
          setActiveSection(section.id);
          window.history.pushState({}, '', `#${section.id}`);
        }
      }
    });
  }, 10);

  useEffect(() => {
    window.addEventListener('scroll', handleScroll);

    return () => {
      window.removeEventListener('scroll', handleScroll);
    };
  }, [handleScroll]);

  if (!dataSet) {
    return <NotFoundPage />;
  }

  const {
    file,
    filters,
    geographicLevels,
    indicators,
    release,
    summary,
    timePeriods,
    title,
  } = dataSet;

  return (
    <Page
      title={title}
      caption={`Data set from ${release.publication.themeTitle}`}
      breadcrumbs={[{ name: 'Data catalogue', link: '/data-catalogue' }]}
      wide={fullScreenPreview}
    >
      {fullScreenPreview ? (
        <DataSetPreview
          fullScreen={fullScreenPreview}
          showAll={showAllPreview}
          onToggleFullScreen={toggleFullScreenPreview}
          onToggleShowAll={toggleShowAllPreview}
        />
      ) : (
        <>
          <div className={styles.info}>
            {release.isLatestPublishedRelease ? (
              <Tag className="govuk-!-margin-right-5">Latest data</Tag>
            ) : (
              <Tag className="govuk-!-margin-right-5" colour="orange">
                Not the latest data
              </Tag>
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
            <DataSetPageNav
              activeSection={activeSection}
              onClickItem={setActiveSection}
            />

            <div className="govuk-grid-column-two-thirds">
              <DataSetPageSection heading={pageSections.details} id="details">
                <SummaryList
                  ariaLabel={`Details list for ${title}`}
                  className="govuk-!-margin-bottom-4 govuk-!-margin-top-4"
                  noBorder
                >
                  <SummaryListItem term="Theme">
                    {release.publication.themeTitle}
                  </SummaryListItem>
                  <SummaryListItem term="Publication">
                    {release.publication.title}
                  </SummaryListItem>
                  <SummaryListItem term="Release">
                    <Link
                      to={`/find-statistics/${release.publication.slug}/${release.slug}`}
                    >
                      {release.title}
                    </Link>
                  </SummaryListItem>
                  <SummaryListItem term="Release type">
                    <Modal
                      showClose
                      title={releaseTypes[release.type]}
                      triggerButton={
                        <ButtonText>
                          {releaseTypes[release.type]}{' '}
                          <InfoIcon
                            description={`Information on ${
                              releaseTypes[release.type]
                            }`}
                          />
                        </ButtonText>
                      }
                    >
                      <ReleaseTypeSection
                        showHeading={false}
                        type={release.type}
                      />
                    </Modal>
                  </SummaryListItem>

                  {geographicLevels && geographicLevels.length > 0 && (
                    <SummaryListItem term="Geographic levels">
                      {orderBy(geographicLevels).join(', ')}
                    </SummaryListItem>
                  )}
                  {indicators && indicators.length > 0 && (
                    <SummaryListItem term="Indicators">
                      <CollapsibleList
                        buttonClassName="govuk-!-margin-bottom-1"
                        buttonHiddenText={`for ${title}`}
                        collapseAfter={3}
                        id="indicators"
                        itemName="indicator"
                        itemNamePlural="indicators"
                        listClassName="govuk-!-margin-top-0 govuk-!-margin-bottom-1"
                        testId="indicators"
                      >
                        {indicators.map((indicator, index) => (
                          <li key={`indicator-${index.toString()}`}>
                            {indicator}
                          </li>
                        ))}
                      </CollapsibleList>
                    </SummaryListItem>
                  )}
                  {filters && filters.length > 0 && (
                    <SummaryListItem term="Filters">
                      <CollapsibleList
                        buttonClassName="govuk-!-margin-bottom-1"
                        buttonHiddenText={`for ${title}`}
                        collapseAfter={3}
                        id="filters"
                        itemName="filter"
                        itemNamePlural="filters"
                        listClassName="govuk-!-margin-top-0 govuk-!-margin-bottom-1"
                        testId="filters"
                      >
                        {filters.map((filter, index) => (
                          <li key={`filter-${index.toString()}`}>{filter}</li>
                        ))}
                      </CollapsibleList>
                    </SummaryListItem>
                  )}
                  {timePeriods && (timePeriods.from || timePeriods.to) && (
                    <SummaryListItem term="Time period">
                      {getTimePeriodString(timePeriods)}
                    </SummaryListItem>
                  )}
                </SummaryList>
              </DataSetPageSection>

              {/* TODO EES-4856 */}
              {/* <DataSetPreview
                fullScreen={fullScreenPreview}
                showAll={showAllPreview}
                onToggleFullScreen={() => {
                  toggleFullScreenPreview();
                  window.scrollTo(0, 0);
                }}
                onToggleShowAll={toggleShowAllPreview}
              /> */}

              {/* TODO EES-4856 */}
              {/* <DataSetVariables /> */}

              {/* TODO EES-4856 */}
              {/* <DataSetFootnotes /> */}

              <DataSetPageSection heading={pageSections.using} id="using">
                <ChevronGrid>
                  <ChevronCard
                    cardSize="l"
                    description="Download the underlying data as a compressed ZIP file"
                    link={
                      <ButtonText onClick={handleDownload}>
                        Download this data set (ZIP)
                      </ButtonText>
                    }
                    noBorder
                    noChevron
                  />
                  <ChevronCard
                    cardSize="l"
                    description="View tables that we have built for you, or create your own tables from open data using our table tool"
                    link={
                      <Link
                        to={`/data-tables/${release.publication.slug}/${release.slug}`}
                      >
                        View or create your own tables
                      </Link>
                    }
                  />
                </ChevronGrid>
              </DataSetPageSection>
            </div>
          </div>
        </>
      )}
    </Page>
  );
}

export const getServerSideProps: GetServerSideProps<Props> = withAxiosHandler(
  async context => {
    const { dataSetId = '', releaseId = '' } =
      context.query as Dictionary<string>;

    const queryClient = new QueryClient();
    await queryClient.prefetchQuery(dataSetQueries.get(dataSetId));

    const props: Props = {
      dataSetId,
      releaseId,
    };

    return {
      props: { ...props, dehydratedState: dehydrate(queryClient) },
    };
  },
);
