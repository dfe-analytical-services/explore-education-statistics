import EditableSectionBlocks from '@admin/components/editable/EditableSectionBlocks';
import Link from '@admin/components/Link';
import PrintThisPage from '@admin/components/PrintThisPage';
import RouteLeavingGuard from '@admin/components/RouteLeavingGuard';
import { useConfig } from '@admin/contexts/ConfigContext';
import { useEditingContext } from '@admin/contexts/EditingContext';
import RelatedPagesSection from '@admin/pages/release/content/components/RelatedPagesSection';
import ReleaseHelpAndSupportSection from '@common/modules/release/components/ReleaseHelpAndSupportSection';
import ReleaseBlock from '@admin/pages/release/content/components/ReleaseBlock';
import ReleaseContentAccordion from '@admin/pages/release/content/components/ReleaseContentAccordion';
import ReleaseEditableBlock from '@admin/pages/release/content/components/ReleaseEditableBlock';
import ReleaseHeadlines from '@admin/pages/release/content/components/ReleaseHeadlines';
import ReleaseNotesSection from '@admin/pages/release/content/components/ReleaseNotesSection';
import { useReleaseContentState } from '@admin/pages/release/content/contexts/ReleaseContentContext';
import useReleaseContentActions from '@admin/pages/release/content/contexts/useReleaseContentActions';
import { getReleaseApprovalStatusLabel } from '@admin/pages/release/utils/releaseSummaryUtil';
import { ReleaseRouteParams } from '@admin/routes/releaseRoutes';
import {
  preReleaseAccessListRoute,
  releaseDataGuidanceRoute,
} from '@admin/routes/routes';
import releaseDataFileService from '@admin/services/releaseDataFileService';
import releaseFileService from '@admin/services/releaseFileService';
import Button from '@common/components/Button';
import ButtonText from '@common/components/ButtonText';
import Details from '@common/components/Details';
import PageSearchForm from '@common/components/PageSearchForm';
import RelatedAside from '@common/components/RelatedAside';
import ScrollableContainer from '@common/components/ScrollableContainer';
import Tag from '@common/components/Tag';
import ReleaseSummarySection from '@common/modules/release/components/ReleaseSummarySection';
import ReleaseDataAndFiles from '@common/modules/release/components/ReleaseDataAndFiles';
import React, { useCallback, useMemo } from 'react';
import { generatePath, useLocation } from 'react-router';

interface MethodologyLink {
  key: string;
  title: string;
  url: string;
}

const ReleaseContent = ({
  transformFeaturedTableLinks,
}: {
  transformFeaturedTableLinks?: (url: string, text: string) => void;
}) => {
  const config = useConfig();
  const location = useLocation();
  const { editingMode, unsavedBlocks, unsavedCommentDeletions } =
    useEditingContext();
  const { release } = useReleaseContentState();
  const { addContentSectionBlock } = useReleaseContentActions();

  const blockRouteChange = useMemo(() => {
    if (unsavedBlocks.length > 0) {
      return true;
    }

    const blocksWithCommentDeletions = Object.entries(unsavedCommentDeletions)
      .filter(blockWithDeletions => blockWithDeletions[1].length)
      .map(blockWithDeletions => blockWithDeletions[0]);

    return blocksWithCommentDeletions.length > 0;
  }, [unsavedBlocks, unsavedCommentDeletions]);

  const addSummaryBlock = useCallback(async () => {
    await addContentSectionBlock({
      releaseId: release.id,
      sectionId: release.summarySection.id,
      sectionKey: 'summarySection',
      block: {
        type: 'HtmlBlock',
        order: 0,
        body: '',
      },
    });
  }, [addContentSectionBlock, release.id, release.summarySection.id]);

  const addRelatedDashboardsBlock = useCallback(async () => {
    if (release.relatedDashboardsSection) {
      await addContentSectionBlock({
        releaseId: release.id,
        sectionId: release.relatedDashboardsSection.id,
        sectionKey: 'relatedDashboardsSection',
        block: {
          type: 'HtmlBlock',
          order: 0,
          body: '',
        },
      });
    }
  }, [addContentSectionBlock, release.id, release.relatedDashboardsSection]);

  const { publication } = release;

  const allMethodologies = useMemo<MethodologyLink[]>(() => {
    const methodologies = publication.methodologies.map(methodology => ({
      key: methodology.id,
      title: methodology.title,
      url: `/methodology/${methodology.id}/summary`,
    }));

    if (publication.externalMethodology) {
      methodologies.push({
        key: publication.externalMethodology.url,
        title: publication.externalMethodology.title,
        url: publication.externalMethodology.url,
      });
    }

    return methodologies;
  }, [publication.externalMethodology, publication.methodologies]);

  const hasAllFilesButton = release.downloadFiles.some(
    file =>
      file.type === 'Data' ||
      (file.type === 'Ancillary' && file.name !== 'All files'),
  );

  const releaseCount =
    release.publication.releases.length +
    release.publication.legacyReleases.length;

  return (
    <>
      <RouteLeavingGuard
        blockRouteChange={blockRouteChange}
        title="There are unsaved changes"
      >
        <p>
          Clicking away from this tab will result in the changes being lost.
        </p>
      </RouteLeavingGuard>

      <div className="govuk-grid-row">
        <div className="govuk-grid-column-two-thirds">
          <ReleaseSummarySection
            isEditing={editingMode === 'edit'}
            lastUpdated={release.updates[0]?.on}
            latestRelease={release.latestRelease}
            nextReleaseDate={release.nextReleaseDate}
            releaseDate={release.published ?? release.publishScheduled}
            releaseType={release.type}
            renderReleaseNotes={<ReleaseNotesSection release={release} />}
            renderStatusTags={
              <Tag className="govuk-!-margin-right-3 govuk-!-margin-bottom-3">
                {getReleaseApprovalStatusLabel(release.approvalStatus)}
              </Tag>
            }
            renderSubscribeLink={
              <a
                className="govuk-!-display-none-print govuk-!-font-weight-bold"
                href="#"
              >
                Sign up for email alerts
              </a>
            }
          />

          <div id="releaseSummary" data-testid="release-summary">
            {release.summarySection && (
              <>
                <EditableSectionBlocks
                  blocks={release.summarySection.content}
                  sectionId={release.summarySection.id}
                  renderBlock={block => (
                    <ReleaseBlock
                      block={block}
                      releaseId={release.id}
                      transformFeaturedTableLinks={transformFeaturedTableLinks}
                    />
                  )}
                  renderEditableBlock={block => (
                    <ReleaseEditableBlock
                      allowComments
                      block={block}
                      publicationId={release.publication.id}
                      releaseId={release.id}
                      sectionId={release.summarySection.id}
                      sectionKey="summarySection"
                    />
                  )}
                />
                {editingMode === 'edit' &&
                  release.summarySection.content?.length === 0 && (
                    <div className="govuk-!-margin-bottom-8 govuk-!-text-align-centre">
                      <Button variant="secondary" onClick={addSummaryBlock}>
                        Add a summary text block
                      </Button>
                    </div>
                  )}
              </>
            )}
          </div>

          {editingMode !== 'edit' && (
            <PageSearchForm
              id="search-form"
              inputLabel="Search in this release page."
              className="govuk-!-margin-top-3 govuk-!-margin-bottom-3"
            />
          )}
        </div>

        <div className="govuk-grid-column-one-third">
          <RelatedAside>
            <h2 className="govuk-heading-m" id="quick-links">
              Quick links
            </h2>
            <nav
              role="navigation"
              aria-labelledby="quick-links"
              data-testid="quick-links"
            >
              <ul className="govuk-list">
                {hasAllFilesButton && (
                  <li>
                    <Button
                      className="govuk-!-margin-bottom-3"
                      disableDoubleClick
                      onClick={() =>
                        releaseFileService.downloadAllFilesZip(release.id)
                      }
                    >
                      Download all data (ZIP)
                    </Button>
                  </li>
                )}
                {!!release.relatedDashboardsSection?.content.length && (
                  <li>
                    <a href="#related-dashboards">View related dashboard(s)</a>
                  </li>
                )}
                {(editingMode === 'edit' || !!release.content.length) && (
                  <li>
                    <a href="#releaseMainContent">Release contents</a>
                  </li>
                )}
                <li>
                  <a href="#explore-data-and-files">Explore data</a>
                </li>
                <li>
                  <a href="#help-and-support">Help and support</a>
                </li>
              </ul>
            </nav>

            <h2 className="govuk-heading-s">Related information</h2>
            <ul className="govuk-list" data-testid="related-information">
              <li>
                <Link
                  to={{
                    pathname: generatePath<ReleaseRouteParams>(
                      releaseDataGuidanceRoute.path,
                      {
                        publicationId: release.publication.id,
                        releaseId: release.id,
                      },
                    ),
                    state: {
                      backLink: location.pathname,
                    },
                  }}
                >
                  Data guidance
                </Link>
              </li>

              {release.hasPreReleaseAccessList && (
                <li>
                  <Link
                    to={{
                      pathname: generatePath<ReleaseRouteParams>(
                        preReleaseAccessListRoute.path,
                        {
                          publicationId: release.publication.id,
                          releaseId: release.id,
                        },
                      ),
                      state: {
                        backLink: location.pathname,
                      },
                    }}
                  >
                    Pre-release access list
                  </Link>
                </li>
              )}
              <li>
                <a href="#contact-us">Contact us</a>
              </li>
            </ul>

            {!!releaseCount && (
              <>
                <h3 className="govuk-heading-s" id="past-releases">
                  Releases in this series
                </h3>

                <Details
                  className="govuk-!-margin-bottom-4"
                  summary={`View releases (${releaseCount})`}
                >
                  {/* @MarkFix change to use publication.ReleaseSeries */}
                  <ScrollableContainer maxHeight={300}>
                    <ul className="govuk-list">
                      {[
                        ...release.publication.releases.map(
                          ({ id, title, slug }) => (
                            <li key={id} data-testid="other-release-item">
                              <Link
                                to={`${config?.publicAppUrl}/find-statistics/${release.publication.slug}/${slug}`}
                              >
                                {title}
                              </Link>
                            </li>
                          ),
                        ),
                        ...release.publication.legacyReleases.map(
                          ({ id, description, url }) => (
                            <li key={id} data-testid="other-release-item">
                              <Link to={url}>{description}</Link>
                            </li>
                          ),
                        ),
                      ]}
                    </ul>
                  </ScrollableContainer>
                </Details>
              </>
            )}

            {allMethodologies.length > 0 && (
              <>
                <h3
                  className="govuk-heading-s govuk-!-padding-top-0"
                  id="methodologies"
                >
                  Methodologies
                </h3>
                <ul className="govuk-list" data-testid="methodologies-list">
                  {allMethodologies.map(methodology => (
                    <li key={methodology.key}>
                      {editingMode === 'edit' ? (
                        <a>{methodology.title}</a>
                      ) : (
                        <Link to={methodology.url}>{methodology.title}</Link>
                      )}
                    </li>
                  ))}
                </ul>
              </>
            )}
            <RelatedPagesSection release={release} />
          </RelatedAside>
        </div>
      </div>

      <hr />

      <ReleaseHeadlines
        release={release}
        transformFeaturedTableLinks={transformFeaturedTableLinks}
      />

      {(release.downloadFiles ||
        release.hasPreReleaseAccessList ||
        !!release.relatedDashboardsSection?.content.length) && (
        <ReleaseDataAndFiles
          downloadFiles={release.downloadFiles}
          hasDataGuidance={release.hasDataGuidance}
          renderAllFilesLink={
            <ButtonText
              disableDoubleClick
              onClick={() => releaseFileService.downloadAllFilesZip(release.id)}
            >
              Download all data (ZIP)
            </ButtonText>
          }
          renderDownloadLink={file => (
            <ButtonText
              onClick={() =>
                releaseDataFileService.downloadFile(
                  release.id,
                  file.id,
                  file.fileName,
                )
              }
            >
              {`${file.name} (${file.extension}, ${file.size})`}
            </ButtonText>
          )}
          renderDataGuidanceLink={
            <Link
              to={{
                pathname: generatePath<ReleaseRouteParams>(
                  releaseDataGuidanceRoute.path,
                  {
                    publicationId: release.publication.id,
                    releaseId: release.id,
                  },
                ),
                state: {
                  backLink: location.pathname,
                },
              }}
            >
              Data guidance
            </Link>
          }
          renderDataCatalogueLink={
            <span>Data catalogue (public site only)</span>
          }
          renderCreateTablesLink={
            <span>View or create your own tables (public site only)</span>
          }
          showDownloadFilesList
          renderRelatedDashboards={
            release.relatedDashboardsSection?.content.length ? (
              <EditableSectionBlocks
                blocks={release.relatedDashboardsSection.content}
                sectionId={release.relatedDashboardsSection.id}
                renderBlock={block => (
                  <ReleaseBlock block={block} releaseId={release.id} />
                )}
                renderEditableBlock={block => (
                  <ReleaseEditableBlock
                    allowComments
                    block={block}
                    publicationId={release.publication.id}
                    releaseId={release.id}
                    // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
                    sectionId={release.relatedDashboardsSection!.id}
                    sectionKey="relatedDashboardsSection"
                  />
                )}
              />
            ) : null
          }
        />
      )}
      {editingMode === 'edit' &&
        !release.relatedDashboardsSection?.content.length && (
          <div className="govuk-!-margin-bottom-8 govuk-!-text-align-centre">
            <Button onClick={addRelatedDashboardsBlock}>
              Add dashboards section
            </Button>
          </div>
        )}

      <ReleaseContentAccordion
        release={release}
        sectionName="Contents"
        transformFeaturedTableLinks={transformFeaturedTableLinks}
      />

      <ReleaseHelpAndSupportSection
        publication={release.publication}
        releaseType={release.type}
        renderExternalMethodologyLink={externalMethodology => (
          <Link to={externalMethodology.url}>{externalMethodology.title}</Link>
        )}
        renderMethodologyLink={methodology => (
          <>
            {editingMode === 'edit' ? (
              <a>{`${methodology.title}`}</a>
            ) : (
              <Link to={`/methodology/${methodology.id}/summary`}>
                {methodology.title}
              </Link>
            )}
          </>
        )}
      />
      <PrintThisPage />
    </>
  );
};

export default ReleaseContent;
