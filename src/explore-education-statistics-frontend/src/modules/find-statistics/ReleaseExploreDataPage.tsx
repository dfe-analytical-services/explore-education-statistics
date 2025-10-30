import ContentHtml from '@common/components/ContentHtml';
import { NavItem } from '@common/components/PageNavExpandable';
import { useMobileMedia } from '@common/hooks/useMedia';
import ContactUsSection from '@common/modules/find-statistics/components/ContactUsSectionRedesign';
import ReleasePageContentSection from '@common/modules/find-statistics/components/ReleasePageContentSection';
import ReleaseDataPageCardLink, {
  ReleaseDataPageCardLinkGrid,
} from '@common/modules/release/components/ReleaseDataPageCardLink';
import {
  PublicationSummaryRedesign,
  ReleaseVersionDataContent,
  ReleaseVersionSummary,
} from '@common/services/publicationService';
import { Dictionary } from '@common/types';
import Link from '@frontend/components/Link';
import { logEvent } from '@frontend/services/googleAnalyticsService';
import React from 'react';

interface Props {
  dataContent: ReleaseVersionDataContent;
  publicationSummary: PublicationSummaryRedesign;
  releaseVersionSummary: ReleaseVersionSummary;
}

export const exploreDataPageSections = {
  explore: {
    id: 'explore-section',
    text: 'Explore data used in this release',
    caption:
      'This page provides a range of routes to access data from within this statistical release to suit different users.',
  },
  featuredTables: {
    id: 'featured-tables-section',
    text: 'Featured tables',
    caption:
      "Featured tables are pre-prepared tables created from a statistical release's data sets. They provide statistics that are regularly requested by some users (such as local councils, regional government or government policy teams) and can be adapted to switch between different categories (such as different geographies, time periods or characteristics where available).",
    linkCaption:
      "Featured tables are pre-prepared tables created from a statistical release's data sets. They provide statistics that are regularly requested by users and can be adapted to switch between different categories.",
  },
  datasets: {
    id: 'datasets-section',
    text: 'Data sets: download or create tables',
    caption:
      'Data sets present comprehensive open data from which users can create their own tables using the EES table tool or download a zipped CSV file.',
  },
  supportingFiles: {
    id: 'supporting-files-section',
    text: 'Supporting files',
    caption:
      'Supporting files provide an area for teams to supply non-standard files for download by users where required.',
  },
  dataDashboards: {
    id: 'data-dashboards-section',
    text: 'Data dashboards',
    caption:
      "Data dashboards provide an alternative route to explore a statistical release's data, presenting key statistics and further insights, often via graphical visualisations.",
  },
  dataGuidance: {
    id: 'data-guidance-section',
    text: 'Data guidance',
    caption:
      'Description of the data included in this release, this is a methodology document, providing information on data sources, their coverage and quality and how the data is produced.',
  },
} as const satisfies Dictionary<
  NavItem & { caption: string; linkCaption?: string }
>;

const ReleaseExploreDataPage = ({
  dataContent,
  publicationSummary,
  releaseVersionSummary,
}: Props) => {
  const {
    dataGuidance,
    dataDashboards,
    dataSets,
    supportingFiles,
    featuredTables,
  } = dataContent;

  const hasSupportingFiles = supportingFiles.length > 0;
  const hasFeaturedTables = featuredTables.length > 0;
  const hasDataDashboards = dataDashboards && dataDashboards.length > 0;

  const { isMedia: isMobileMedia } = useMobileMedia();

  return (
    <>
      <ReleasePageContentSection
        heading={exploreDataPageSections.explore.text}
        id={exploreDataPageSections.explore.id}
        caption={exploreDataPageSections.explore.caption}
      >
        {!isMobileMedia && (
          <ReleaseDataPageCardLinkGrid>
            <ReleaseDataPageCardLink
              renderLink={
                <Link
                  to={`${process.env.CONTENT_API_BASE_URL}/releases/${releaseVersionSummary.id}/files?fromPage=ReleaseDownloads`}
                  onClick={() => {
                    logEvent({
                      category: 'Downloads',
                      action: `Release page all files, Release: ${releaseVersionSummary.title}, File: All files`,
                    });
                  }}
                  unvisited
                >
                  Download all data from this release (ZIP)
                </Link>
              }
              caption="This includes all data sets, guidance files and any supporting files"
              isHighlightVariant
            />

            {hasFeaturedTables && (
              <ReleaseDataPageCardLink
                renderLink={
                  <Link
                    to={`#${exploreDataPageSections.featuredTables.id}`}
                    unvisited
                  >
                    {exploreDataPageSections.featuredTables.text}
                  </Link>
                }
                caption={exploreDataPageSections.featuredTables.linkCaption}
              />
            )}

            <ReleaseDataPageCardLink
              renderLink={
                <Link to={`#${exploreDataPageSections.datasets.id}`} unvisited>
                  {exploreDataPageSections.datasets.text}
                </Link>
              }
              caption={exploreDataPageSections.datasets.caption}
            />

            {hasSupportingFiles && (
              <ReleaseDataPageCardLink
                renderLink={
                  <Link
                    to={`#${exploreDataPageSections.supportingFiles.id}`}
                    unvisited
                  >
                    {exploreDataPageSections.supportingFiles.text}
                  </Link>
                }
                caption={exploreDataPageSections.supportingFiles.caption}
              />
            )}

            {hasDataDashboards && (
              <ReleaseDataPageCardLink
                renderLink={
                  <Link
                    to={`#${exploreDataPageSections.dataDashboards.id}`}
                    unvisited
                  >
                    {exploreDataPageSections.dataDashboards.text}
                  </Link>
                }
                caption={exploreDataPageSections.dataDashboards.caption}
              />
            )}

            <ReleaseDataPageCardLink
              renderLink={
                <Link
                  to={`#${exploreDataPageSections.dataGuidance.id}`}
                  unvisited
                >
                  {exploreDataPageSections.dataGuidance.text}
                </Link>
              }
              caption={exploreDataPageSections.dataGuidance.caption}
            />

            <ReleaseDataPageCardLink
              renderLink={
                <Link
                  to={`/data-catalogue?themeId=${publicationSummary.theme.id}&publicationId=${publicationSummary.id}&releaseVersionId=${releaseVersionSummary.id}`}
                  unvisited
                >
                  Data catalogue
                </Link>
              }
              caption="Alternatively use our data catalogue to search and filter for specific data sets from this release or our entire library, providing full data summaries, data previews and access to API data sets."
            />
          </ReleaseDataPageCardLinkGrid>
        )}
      </ReleasePageContentSection>

      {hasFeaturedTables && (
        <ReleasePageContentSection
          heading={exploreDataPageSections.featuredTables.text}
          id={exploreDataPageSections.featuredTables.id}
          caption={exploreDataPageSections.featuredTables.caption}
        >
          {featuredTables.length} featured tables
        </ReleasePageContentSection>
      )}

      <ReleasePageContentSection
        heading={exploreDataPageSections.datasets.text}
        id={exploreDataPageSections.datasets.id}
        caption={exploreDataPageSections.datasets.caption}
      >
        <p>{dataSets.length} datasets</p>
      </ReleasePageContentSection>

      {hasSupportingFiles && (
        <ReleasePageContentSection
          heading={exploreDataPageSections.supportingFiles.text}
          id={exploreDataPageSections.supportingFiles.id}
          caption={exploreDataPageSections.supportingFiles.caption}
        >
          <p>{supportingFiles.length} supporting files</p>
        </ReleasePageContentSection>
      )}

      {hasDataDashboards && (
        <ReleasePageContentSection
          heading={exploreDataPageSections.dataDashboards.text}
          id={exploreDataPageSections.dataDashboards.id}
          caption={exploreDataPageSections.dataDashboards.caption}
        >
          <ContentHtml html={dataDashboards} testId="dataDashboards-content" />
        </ReleasePageContentSection>
      )}

      <ReleasePageContentSection
        heading={exploreDataPageSections.dataGuidance.text}
        id={exploreDataPageSections.dataGuidance.id}
        caption={exploreDataPageSections.dataGuidance.caption}
      >
        <ContentHtml html={dataGuidance} testId="dataGuidance-content" />
      </ReleasePageContentSection>

      <ContactUsSection
        publicationContact={publicationSummary.contact}
        publicationTitle={publicationSummary.title}
      />
    </>
  );
};

export default ReleaseExploreDataPage;
