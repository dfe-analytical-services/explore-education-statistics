import Link from '@admin/components/Link';
import { useConfig } from '@admin/contexts/ConfigContext';
import ReleasePageTabPanel from '@admin/pages/release/content/components/ReleasePageTabPanel';
import { useReleaseContentState } from '@admin/pages/release/content/contexts/ReleaseContentContext';
import downloadReleaseFileSecurely from '@admin/pages/release/data/components/utils/downloadReleaseFileSecurely';
import releaseContentQueries from '@admin/queries/releaseContentQueries';
import {
  preReleaseTableToolRoute,
  PreReleaseTableToolRouteParams,
} from '@admin/routes/preReleaseRoutes';
import releaseFileService from '@admin/services/releaseFileService';
import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import AccordionToggleButton from '@common/components/AccordionToggleButton';
import ButtonText from '@common/components/ButtonText';
import ContentHtml from '@common/components/ContentHtml';
import InsetText from '@common/components/InsetText';
import LoadingSpinner from '@common/components/LoadingSpinner';
import VisuallyHidden from '@common/components/VisuallyHidden';
import WarningMessage from '@common/components/WarningMessage';
import { useMobileMedia } from '@common/hooks/useMedia';
import useToggle from '@common/hooks/useToggle';
import ContactUsSection, {
  contactUsNavItem,
} from '@common/modules/find-statistics/components/ContactUsSectionRedesign';
import ReleaseDataList from '@common/modules/find-statistics/components/ReleaseDataList';
import ReleaseDataListItem from '@common/modules/find-statistics/components/ReleaseDataListItem';
import ReleasePageContentSection from '@common/modules/find-statistics/components/ReleasePageContentSection';
import ReleaseDataPageCardLink, {
  ReleaseDataPageCardLinkGrid,
} from '@common/modules/release/components/ReleaseDataPageCardLink';
import ReleaseDataSetFileSummary from '@common/modules/release/components/ReleaseDataSetFileSummary';
import ReleasePageLayout from '@common/modules/release/components/ReleasePageLayout';
import pageSections from '@common/modules/release/data/releaseExploreDataPageSections';
import { useQuery } from '@tanstack/react-query';
import React, { useMemo } from 'react';
import { generatePath } from 'react-router-dom';

interface Props {
  hidden: boolean;
  isPra?: boolean;
  handleFeaturedTableItemClick?: (id: string) => void;
}

const ReleasePageTabExploreData = ({
  hidden,
  isPra = false,
  handleFeaturedTableItemClick,
}: Props) => {
  const { release } = useReleaseContentState();
  const { publication, publishingOrganisations } = release;

  const { isMedia: isMobileMedia } = useMobileMedia();
  const [showAllDataSetDetails, toggleAllDataSetDetails] = useToggle(false);
  const { publicAppUrl } = useConfig();

  const {
    data: dataContent,
    isError: isErrorDataContent,
    isLoading: isLoadingDataContent,
  } = useQuery(releaseContentQueries.getDataContent(release.id));

  const hasDataSets = dataContent && dataContent.dataSets.length > 0;
  const hasSupportingFiles =
    dataContent && dataContent.supportingFiles.length > 0;
  const hasFeaturedTables =
    dataContent && dataContent.featuredTables.length > 0;
  const hasDataDashboards =
    !!dataContent?.dataDashboards && dataContent.dataDashboards.length > 0;

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
      heading={`${dataContent.featuredTables.length} featured table${
        dataContent.featuredTables.length > 1 ? 's' : ''
      }`}
    >
      {dataContent.featuredTables.map(featuredTable => (
        <ReleaseDataListItem
          key={featuredTable.featuredTableId}
          title={featuredTable.title}
          description={featuredTable.summary}
          actions={
            // We need to handle this link differently depending on context:
            // in PRA, navigate to Pre-Release Table Tool route
            // in published releases, navigate to public fast track URL
            // in normal content preview set context to table preview mode
            handleFeaturedTableItemClick ? (
              <ButtonText
                onClick={() => {
                  handleFeaturedTableItemClick?.(featuredTable.dataBlockId);
                }}
              >
                View, edit or download{' '}
                <VisuallyHidden>{featuredTable.title}</VisuallyHidden>
              </ButtonText>
            ) : (
              <Link
                to={
                  isPra
                    ? generatePath<PreReleaseTableToolRouteParams>(
                        preReleaseTableToolRoute.path,
                        {
                          publicationId: publication.id,
                          releaseVersionId: release.id,
                          dataBlockId: featuredTable.dataBlockId,
                        },
                      )
                    : `${publicAppUrl}/data-tables/fast-track/${featuredTable.dataBlockParentId}`
                }
              >
                View, edit or download{' '}
                <VisuallyHidden>{featuredTable.title}</VisuallyHidden>
              </Link>
            )
          }
        />
      ))}
    </ReleaseDataList>
  ) : undefined;

  const dataSetsContent = hasDataSets ? (
    <ReleaseDataList
      heading={`${dataContent?.dataSets.length || 0} data set${
        dataContent?.dataSets?.length === 1 ? '' : 's'
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
      toggle={
        <AccordionToggleButton
          expanded={showAllDataSetDetails}
          label={
            showAllDataSetDetails
              ? 'Hide all details'
              : 'Show expanded view for all data sets'
          }
          onClick={toggleAllDataSetDetails}
        />
      }
    >
      {dataContent?.dataSets.map(dataset => (
        <ReleaseDataListItem
          key={dataset.fileId}
          title={dataset.title}
          description={dataset.summary}
          metaInfo={dataset.meta.geographicLevels.join(', ')}
          actions={
            <>
              <span>
                Create table{' '}
                <VisuallyHidden>using {dataset.title}</VisuallyHidden>
              </span>
              <ButtonText
                onClick={() => {
                  releaseFileService.downloadFilesAsZip(release.id, [
                    dataset.fileId,
                  ]);
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
            renderLink={
              <span>
                Data set information page{' '}
                <VisuallyHidden>for {dataset.title}</VisuallyHidden>
              </span>
            }
          />
        </ReleaseDataListItem>
      ))}
    </ReleaseDataList>
  ) : (
    <InsetText>No data sets added for this release yet.</InsetText>
  );

  const supportingFilesContent = (
    <ReleaseDataList
      heading={`${dataContent?.supportingFiles.length} supporting data file${
        dataContent?.supportingFiles.length === 1 ? '' : 's'
      }`}
    >
      {dataContent?.supportingFiles.map(file => (
        <ReleaseDataListItem
          key={file.fileId}
          title={file.title}
          description={file.summary}
          actions={
            <ButtonText
              onClick={() =>
                downloadReleaseFileSecurely({
                  releaseVersionId: release.id,
                  fileId: file.fileId,
                  fileName: file.filename,
                })
              }
            >
              Download <VisuallyHidden>{file.title}</VisuallyHidden>{' '}
              {`(${file.extension}, ${file.size})`}
            </ButtonText>
          }
        />
      ))}
    </ReleaseDataList>
  );

  const dataGuidanceContent = !dataContent?.dataGuidance ? (
    <InsetText>No data guidance available for this release yet.</InsetText>
  ) : (
    <ContentHtml
      html={dataContent?.dataGuidance}
      testId="dataGuidance-content"
    />
  );

  return (
    <ReleasePageTabPanel tabKey="explore" hidden={hidden}>
      <ReleasePageLayout navItems={navItems}>
        <LoadingSpinner loading={isLoadingDataContent}>
          {isErrorDataContent ? (
            <WarningMessage>Could not load release data</WarningMessage>
          ) : (
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
                          <Link
                            to={`#${pageSections.featuredTables.id}`}
                            unvisited
                          >
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
                          <Link
                            to={`#${pageSections.supportingFiles.id}`}
                            unvisited
                          >
                            {pageSections.supportingFiles.text}
                          </Link>
                        }
                        caption={pageSections.supportingFiles.caption}
                      />
                    )}

                    {hasDataDashboards && (
                      <ReleaseDataPageCardLink
                        renderLink={
                          <Link
                            to={`#${pageSections.dataDashboards.id}`}
                            unvisited
                          >
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
                      <ContentHtml
                        html={dataContent.dataDashboards || ''}
                        testId="dataDashboards-content"
                      />
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
                      <ContentHtml
                        html={dataContent.dataDashboards || ''}
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
                    {dataGuidanceContent}
                  </ReleasePageContentSection>
                </>
              )}

              <ContactUsSection
                publicationContact={publication.contact}
                publicationTitle={publication.title}
                publishingOrganisations={publishingOrganisations}
              />
            </>
          )}
        </LoadingSpinner>
      </ReleasePageLayout>
    </ReleasePageTabPanel>
  );
};

export default ReleasePageTabExploreData;
