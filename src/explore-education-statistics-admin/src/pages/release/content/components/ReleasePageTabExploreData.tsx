import Link from '@admin/components/Link';
import { useConfig } from '@admin/contexts/ConfigContext';
import ReleaseBlock from '@admin/pages/release/content/components/ReleaseBlock';
import ReleasePageTabPanel from '@admin/pages/release/content/components/ReleasePageTabPanel';
import { useReleaseContentState } from '@admin/pages/release/content/contexts/ReleaseContentContext';
import downloadReleaseFileSecurely from '@admin/pages/release/data/components/utils/downloadReleaseFileSecurely';
import dataGuidanceQueries from '@admin/queries/dataGuidanceQueries';
import releaseFileService from '@admin/services/releaseFileService';
import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import ButtonText from '@common/components/ButtonText';
import ContentHtml from '@common/components/ContentHtml';
import LoadingSpinner from '@common/components/LoadingSpinner';
import VisuallyHidden from '@common/components/VisuallyHidden';
import { useMobileMedia } from '@common/hooks/useMedia';
import ContactUsSection, {
  contactUsNavItem,
} from '@common/modules/find-statistics/components/ContactUsSectionRedesign';
import ReleaseDataList from '@common/modules/find-statistics/components/ReleaseDataList';
import ReleaseDataListItem from '@common/modules/find-statistics/components/ReleaseDataListItem';
import ReleasePageContentSection from '@common/modules/find-statistics/components/ReleasePageContentSection';
import ReleaseDataPageCardLink, {
  ReleaseDataPageCardLinkGrid,
} from '@common/modules/release/components/ReleaseDataPageCardLink';
import ReleasePageLayout from '@common/modules/release/components/ReleasePageLayout';
import pageSections from '@common/modules/release/data/releaseExploreDataPageSections';
import { useQuery } from '@tanstack/react-query';
import orderBy from 'lodash/orderBy';
import React, { useMemo } from 'react';

interface Props {
  hidden: boolean;
  isPra?: boolean;
}

const ReleasePageTabExploreData = ({ hidden, isPra = false }: Props) => {
  const { featuredTables, release } = useReleaseContentState();
  const { publication, downloadFiles } = release;

  const { isMedia: isMobileMedia } = useMobileMedia();
  const { publicAppUrl } = useConfig();

  const { data: dataGuidance, isLoading: isLoadingDataGuidance } = useQuery(
    dataGuidanceQueries.getDataGuidance(release.id),
  );

  const dataFiles = orderBy(
    downloadFiles.filter(file => file.type === 'Data'),
    ['name'],
  );

  const ancillaryFiles = orderBy(
    downloadFiles.filter(
      file => file.type === 'Ancillary' && file.name !== 'All files',
    ),
    ['name'],
  );

  const hasSupportingFiles = ancillaryFiles.length > 0;
  const hasFeaturedTables = featuredTables && featuredTables.length > 0;
  const hasDataDashboards =
    release.relatedDashboardsSection &&
    release.relatedDashboardsSection.content.length > 0;

  const navItems = useMemo(
    () =>
      [
        pageSections.explore,
        hasFeaturedTables && pageSections.featuredTables,
        pageSections.datasets,
        hasSupportingFiles && pageSections.supportingFiles,
        hasDataDashboards && pageSections.dataDashboards,
        pageSections.dataGuidance,
        contactUsNavItem,
      ].filter(item => !!item),
    [hasDataDashboards, hasFeaturedTables, hasSupportingFiles],
  );

  const featuredTablesContent = hasFeaturedTables ? (
    <ReleaseDataList
      heading={`${featuredTables.length} featured table${
        featuredTables.length > 1 ? 's' : ''
      }`}
    >
      {featuredTables.map(featuredTable => (
        <ReleaseDataListItem
          key={featuredTable.id}
          title={featuredTable.name}
          description={featuredTable.description}
          actions={
            isPra ? (
              <span>
                View, edit or download{' '}
                <VisuallyHidden>{featuredTable.name}</VisuallyHidden>
              </span>
            ) : (
              <Link
                to={
                  release.published
                    ? `${publicAppUrl}/data-tables/fast-track/${featuredTable.dataBlockParentId}`
                    : `/publication/${publication.id}/release/${release.id}/data-blocks/${featuredTable.dataBlockId}`
                }
              >
                View, edit or download{' '}
                <VisuallyHidden>{featuredTable.name}</VisuallyHidden>
              </Link>
            )
          }
        />
      ))}
    </ReleaseDataList>
  ) : undefined;

  const dataSetsContent = (
    <ReleaseDataList
      heading={`${dataFiles.length} data set${
        dataFiles.length > 1 ? 's' : ''
      } available for download`}
      actions={
        <ButtonText
          className="govuk-!-font-weight-bold"
          preventDoubleClick
          onClick={() => releaseFileService.downloadFilesAsZip(release.id)}
        >
          Download all (ZIP)
        </ButtonText>
      }
    >
      {dataFiles.map(dataset => (
        <ReleaseDataListItem
          key={dataset.id}
          title={dataset.name}
          description={dataset.summary}
          actions={
            <>
              <span>
                Create table{' '}
                <VisuallyHidden>using {dataset.name}</VisuallyHidden>
              </span>
              <ButtonText
                onClick={() => {
                  releaseFileService.downloadFilesAsZip(release.id, [
                    dataset.id,
                  ]);
                }}
              >
                Download <VisuallyHidden>{dataset.name}</VisuallyHidden> (ZIP)
              </ButtonText>
            </>
          }
        />
      ))}
    </ReleaseDataList>
  );

  const supportingFilesContent = (
    <ReleaseDataList
      heading={`${ancillaryFiles.length} supporting data file${
        ancillaryFiles.length > 1 ? 's' : ''
      }`}
    >
      {ancillaryFiles.map(file => (
        <ReleaseDataListItem
          key={file.id}
          title={file.name}
          description={file.summary}
          actions={
            <ButtonText
              onClick={() =>
                downloadReleaseFileSecurely({
                  releaseVersionId: release.id,
                  fileId: file.id,
                  fileName: file.fileName,
                })
              }
            >
              Download <VisuallyHidden>{file.name}</VisuallyHidden>{' '}
              {`(${file.extension}, ${file.size})`}
            </ButtonText>
          }
        />
      ))}
    </ReleaseDataList>
  );

  const relatedDashboardsContent =
    release.relatedDashboardsSection?.content.map(block => (
      <ReleaseBlock
        key={block.id}
        block={block}
        releaseVersionId={release.id}
        visible
      />
    ));

  const dataGuidanceContent = (
    <LoadingSpinner loading={isLoadingDataGuidance}>
      {!dataGuidance ? (
        <p>No data guidance available for this release.</p>
      ) : (
        <ContentHtml
          html={dataGuidance.content}
          testId="dataGuidance-content"
        />
      )}
    </LoadingSpinner>
  );

  return (
    <ReleasePageTabPanel tabKey="explore" hidden={hidden}>
      <ReleasePageLayout navItems={navItems}>
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
                  <ButtonText
                    className="govuk-!-font-weight-bold"
                    underline={false}
                    preventDoubleClick
                    onClick={() =>
                      releaseFileService.downloadFilesAsZip(release.id)
                    }
                  >
                    Download all data from this release (ZIP)
                  </ButtonText>
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
                  release.published ? (
                    <Link
                      to={`${publicAppUrl}/data-catalogue?publicationId=${release.publicationId}&releaseVersionId=${release.id}`}
                    >
                      Data catalogue
                    </Link>
                  ) : (
                    <span>
                      Data catalogue (available when release is published)
                    </span>
                  )
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
                {relatedDashboardsContent}
              </AccordionSection>
            )}

            <AccordionSection
              heading={pageSections.dataGuidance.text}
              id={pageSections.dataGuidance.id}
              caption={pageSections.dataGuidance.caption}
            >
              {dataGuidanceContent}
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
                {relatedDashboardsContent}
              </ReleasePageContentSection>
            )}

            <ReleasePageContentSection
              heading={pageSections.dataGuidance.text}
              id={pageSections.dataGuidance.id}
              testId={pageSections.dataGuidance.id}
              caption={pageSections.dataGuidance.caption}
            >
              {dataGuidanceContent}
            </ReleasePageContentSection>
          </>
        )}

        <ContactUsSection
          publicationContact={publication.contact}
          publicationTitle={publication.title}
        />
      </ReleasePageLayout>
    </ReleasePageTabPanel>
  );
};

export default ReleasePageTabExploreData;
