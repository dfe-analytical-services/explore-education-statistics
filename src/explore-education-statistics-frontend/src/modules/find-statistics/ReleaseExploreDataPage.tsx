import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import AccordionToggleButton from '@common/components/AccordionToggleButton';
import ButtonText from '@common/components/ButtonText';
import ContentHtml from '@common/components/ContentHtml';
import VisuallyHidden from '@common/components/VisuallyHidden';
import { useMobileMedia } from '@common/hooks/useMedia';
import useToggle from '@common/hooks/useToggle';
import ContactUsSection from '@common/modules/find-statistics/components/ContactUsSectionRedesign';
import ReleaseDataList from '@common/modules/find-statistics/components/ReleaseDataList';
import ReleaseDataListItem from '@common/modules/find-statistics/components/ReleaseDataListItem';
import ReleasePageContentSection from '@common/modules/find-statistics/components/ReleasePageContentSection';
import ReleaseDataPageCardLink, {
  ReleaseDataPageCardLinkGrid,
} from '@common/modules/release/components/ReleaseDataPageCardLink';
import pageSections from '@common/modules/release/components/ReleaseExploreDataPageSections';
import {
  PublicationSummaryRedesign,
  ReleaseVersionDataContent,
  ReleaseVersionSummary,
} from '@common/services/publicationService';
import Link from '@frontend/components/Link';
import ReleaseDataSetFileSummary from '@frontend/modules/find-statistics/components/ReleaseDataSetFileSummary';
import downloadService from '@frontend/services/downloadService';
import { logEvent } from '@frontend/services/googleAnalyticsService';
import React from 'react';

interface Props {
  dataContent: ReleaseVersionDataContent;
  publicationSummary: PublicationSummaryRedesign;
  releaseVersionSummary: ReleaseVersionSummary;
}

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

  const [showAllDataSetDetails, toggleAllDataSetDetails] = useToggle(false);

  const featuredTablesContent = (
    <ReleaseDataList
      heading={`${featuredTables.length} featured table${
        featuredTables.length > 1 ? 's' : ''
      }`}
    >
      {featuredTables.map(featuredTable => (
        <ReleaseDataListItem
          key={featuredTable.featuredTableId}
          title={featuredTable.title}
          description={featuredTable.summary}
          actions={
            <Link
              to={`/data-tables/fast-track/${featuredTable.dataBlockParentId}`}
            >
              View, edit or download{' '}
              <VisuallyHidden>{featuredTable.title}</VisuallyHidden>
            </Link>
          }
        />
      ))}
    </ReleaseDataList>
  );

  const dataSetsContent = (
    <ReleaseDataList
      heading={`${dataSets.length} data set${
        dataSets.length > 1 ? 's' : ''
      } available for download`}
      actions={
        <Link
          to={`${process.env.CONTENT_API_BASE_URL}/releases/${releaseVersionSummary.id}/files?fromPage=ReleaseDownloads`}
          onClick={() => {
            logEvent({
              category: 'Downloads',
              action: `Release page all files, Release: ${releaseVersionSummary.title}, File: All files`,
            });
          }}
          className="govuk-!-font-weight-bold"
          unvisited
        >
          Download all (ZIP)
        </Link>
      }
      toggle={
        <AccordionToggleButton
          expanded={showAllDataSetDetails}
          label={
            showAllDataSetDetails
              ? 'Hide all details'
              : 'Show expanded view for all data sets'
          }
          onClick={() => {
            toggleAllDataSetDetails();
            logEvent({
              category: 'Data catalogue',
              action: 'All data set details toggled',
            });
          }}
        />
      }
    >
      {dataSets.map(dataset => (
        <ReleaseDataListItem
          key={dataset.fileId}
          title={dataset.title}
          description={dataset.summary}
          metaInfo={dataset.meta.geographicLevels.join(', ')}
          actions={
            <>
              <Link
                to={`/data-tables/${publicationSummary.slug}/${releaseVersionSummary.slug}?subjectId=${dataset.subjectId}`}
              >
                Create table{' '}
                <VisuallyHidden>using {dataset.title}</VisuallyHidden>
              </Link>
              <ButtonText
                onClick={async () => {
                  await downloadService.downloadZip(
                    releaseVersionSummary.id,
                    'ReleaseDownloads',
                    dataset.fileId,
                  );

                  logEvent({
                    category: 'Downloads',
                    action: 'Release page data set file download',
                    label: `Publication: ${publicationSummary.title}, Release: ${releaseVersionSummary.title}, Data set: ${dataset.title}`,
                  });
                }}
              >
                Download <VisuallyHidden>{dataset.title}</VisuallyHidden> (ZIP)
              </ButtonText>
            </>
          }
        >
          <ReleaseDataSetFileSummary
            dataSetFile={dataset}
            expanded={showAllDataSetDetails}
          />
        </ReleaseDataListItem>
      ))}
    </ReleaseDataList>
  );

  const supportingFilesContent = (
    <ReleaseDataList
      heading={`${supportingFiles.length} supporting data file${
        supportingFiles.length > 1 ? 's' : ''
      }`}
    >
      {supportingFiles.map(file => (
        <ReleaseDataListItem
          key={file.fileId}
          title={file.title}
          description={file.summary}
          actions={
            <Link
              to={`${process.env.CONTENT_API_BASE_URL}/releases/${releaseVersionSummary.id}/files/${file.fileId}`}
              onClick={() => {
                logEvent({
                  category: 'Downloads',
                  action: 'Release page file downloaded',
                  label: `Publication: ${publicationSummary.title}, Release: ${releaseVersionSummary.title}, File: ${file.title}`,
                });
              }}
            >
              Download <VisuallyHidden>{file.title}</VisuallyHidden>{' '}
              {`(${file.extension}, ${file.size})`}
            </Link>
          }
        />
      ))}
    </ReleaseDataList>
  );

  return (
    <>
      <ReleasePageContentSection
        heading={pageSections.explore.text}
        id={pageSections.explore.id}
        testId={pageSections.explore.id}
        caption={pageSections.explore.caption}
        includeBackToTopLink={false}
        includeSectionBreak={!isMobileMedia}
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
                  <Link to={`#${pageSections.featuredTables.id}`} unvisited>
                    {pageSections.featuredTables.text}
                  </Link>
                }
                caption={pageSections.featuredTables.shortCaption}
              />
            )}

            <ReleaseDataPageCardLink
              renderLink={
                <Link to={`#${pageSections.datasets.id}`} unvisited>
                  {pageSections.datasets.text}
                </Link>
              }
              caption={pageSections.datasets.caption}
            />

            {hasSupportingFiles && (
              <ReleaseDataPageCardLink
                renderLink={
                  <Link to={`#${pageSections.supportingFiles.id}`} unvisited>
                    {pageSections.supportingFiles.text}
                  </Link>
                }
                caption={pageSections.supportingFiles.caption}
              />
            )}

            {hasDataDashboards && (
              <ReleaseDataPageCardLink
                renderLink={
                  <Link to={`#${pageSections.dataDashboards.id}`} unvisited>
                    {pageSections.dataDashboards.text}
                  </Link>
                }
                caption={pageSections.dataDashboards.caption}
              />
            )}

            <ReleaseDataPageCardLink
              renderLink={
                <Link to={`#${pageSections.dataGuidance.id}`} unvisited>
                  {pageSections.dataGuidance.text}
                </Link>
              }
              caption={pageSections.dataGuidance.caption}
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

      {isMobileMedia ? (
        <Accordion
          className="govuk-!-margin-top-9"
          id="accordion-content"
          onSectionOpen={accordionSection => {
            logEvent({
              category: `${publicationSummary.title} release data page`,
              action: `Content accordion opened`,
              label: `${accordionSection.title}`,
            });
          }}
          showOpenAll={false}
        >
          {hasFeaturedTables && (
            <AccordionSection
              heading={pageSections.featuredTables.text}
              id={pageSections.featuredTables.id}
              caption={pageSections.featuredTables.shortCaption}
            >
              {featuredTablesContent}
            </AccordionSection>
          )}

          <AccordionSection
            heading={pageSections.datasets.text}
            id={pageSections.datasets.id}
            caption={pageSections.datasets.caption}
          >
            {dataSetsContent}
          </AccordionSection>

          {hasSupportingFiles && (
            <AccordionSection
              heading={pageSections.supportingFiles.text}
              id={pageSections.supportingFiles.id}
              caption={pageSections.supportingFiles.caption}
            >
              {supportingFilesContent}
            </AccordionSection>
          )}

          {hasDataDashboards && (
            <AccordionSection
              heading={pageSections.dataDashboards.text}
              id={pageSections.dataDashboards.id}
              caption={pageSections.dataDashboards.caption}
            >
              <ContentHtml
                html={dataDashboards}
                testId="dataDashboards-content"
              />
            </AccordionSection>
          )}

          <AccordionSection
            heading={pageSections.dataGuidance.text}
            id={pageSections.dataGuidance.id}
            caption={pageSections.dataGuidance.caption}
          >
            <ContentHtml html={dataGuidance} testId="dataGuidance-content" />
          </AccordionSection>
        </Accordion>
      ) : (
        <>
          {hasFeaturedTables && (
            <ReleasePageContentSection
              heading={pageSections.featuredTables.text}
              id={pageSections.featuredTables.id}
              testId={pageSections.featuredTables.id}
              caption={pageSections.featuredTables.caption}
            >
              {featuredTablesContent}
            </ReleasePageContentSection>
          )}

          <ReleasePageContentSection
            heading={pageSections.datasets.text}
            id={pageSections.datasets.id}
            testId={pageSections.datasets.id}
            caption={pageSections.datasets.caption}
          >
            {dataSetsContent}
          </ReleasePageContentSection>

          {hasSupportingFiles && (
            <ReleasePageContentSection
              heading={pageSections.supportingFiles.text}
              id={pageSections.supportingFiles.id}
              testId={pageSections.supportingFiles.id}
              caption={pageSections.supportingFiles.caption}
            >
              {supportingFilesContent}
            </ReleasePageContentSection>
          )}

          {hasDataDashboards && (
            <ReleasePageContentSection
              heading={pageSections.dataDashboards.text}
              id={pageSections.dataDashboards.id}
              testId={pageSections.dataDashboards.id}
              caption={pageSections.dataDashboards.caption}
            >
              <ContentHtml
                html={dataDashboards}
                testId="dataDashboards-content"
              />
            </ReleasePageContentSection>
          )}

          <ReleasePageContentSection
            heading={pageSections.dataGuidance.text}
            id={pageSections.dataGuidance.id}
            testId={pageSections.dataGuidance.id}
            caption={pageSections.dataGuidance.caption}
          >
            <ContentHtml html={dataGuidance} testId="dataGuidance-content" />
          </ReleasePageContentSection>
        </>
      )}

      <ContactUsSection
        publicationContact={publicationSummary.contact}
        publicationTitle={publicationSummary.title}
      />
    </>
  );
};

export default ReleaseExploreDataPage;
