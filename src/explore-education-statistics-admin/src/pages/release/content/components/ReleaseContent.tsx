import { CommentsChangeHandler } from '@admin/components/editable/Comments';
import EditableSectionBlocks from '@admin/components/editable/EditableSectionBlocks';
import Link from '@admin/components/Link';
import PrintThisPage from '@admin/components/PrintThisPage';
import { useConfig } from '@admin/contexts/ConfigContext';
import { useEditingContext } from '@admin/contexts/EditingContext';
import BasicReleaseSummary from '@admin/pages/release/content/components/BasicReleaseSummary';
import RelatedPagesSection from '@admin/pages/release/content/components/RelatedPagesSection';
import ReleaseBlock from '@admin/pages/release/content/components/ReleaseBlock';
import ReleaseContentAccordion from '@admin/pages/release/content/components/ReleaseContentAccordion';
import ReleaseEditableBlock from '@admin/pages/release/content/components/ReleaseEditableBlock';
import ReleaseHeadlines from '@admin/pages/release/content/components/ReleaseHeadlines';
import ReleaseHelpAndSupportSection from '@admin/pages/release/content/components/ReleaseHelpAndSupportSection';
import { useReleaseContentState } from '@admin/pages/release/content/contexts/ReleaseContentContext';
import useReleaseContentActions from '@admin/pages/release/content/contexts/useReleaseContentActions';
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
import ReleaseDataAndFilesAccordion from '@common/modules/release/components/ReleaseDataAndFilesAccordion';
import React, { useCallback, useMemo } from 'react';
import { generatePath, useLocation } from 'react-router';

interface MethodologyLink {
  key: string;
  title: string;
  url: string;
}

const ReleaseContent = () => {
  const config = useConfig();
  const location = useLocation();

  const { editingMode } = useEditingContext();
  const { release } = useReleaseContentState();
  const actions = useReleaseContentActions();

  const releaseCount = useMemo(() => {
    if (release) {
      return (
        release.publication.otherReleases.length +
        release.publication.legacyReleases.length
      );
    }
    return 0;
  }, [release]);

  const addBlock = useCallback(async () => {
    if (release)
      await actions.addContentSectionBlock({
        releaseId: release.id,
        sectionId: release.summarySection.id,
        sectionKey: 'summarySection',
        block: {
          type: 'HtmlBlock',
          order: 0,
          body: '',
        },
      });
  }, [release, actions]);

  const updateBlock = useCallback(
    async (blockId, bodyContent) => {
      if (release)
        await actions.updateContentSectionBlock({
          releaseId: release.id,
          sectionId: release.summarySection.id,
          blockId,
          sectionKey: 'summarySection',
          bodyContent,
        });
    },
    [release, actions],
  );

  const removeBlock = useCallback(
    async (blockId: string) => {
      if (release)
        await actions.deleteContentSectionBlock({
          releaseId: release.id,
          sectionId: release.summarySection.id,
          blockId,
          sectionKey: 'summarySection',
        });
    },
    [release, actions],
  );

  const updateBlockComments: CommentsChangeHandler = useCallback(
    async (blockId, comments) => {
      await actions.updateBlockComments({
        sectionId: release.summarySection.id,
        blockId,
        sectionKey: 'summarySection',
        comments,
      });
    },
    [actions, release.summarySection.id],
  );

  if (!release) {
    return null;
  }

  const { publication } = release;

  const allMethodologies: MethodologyLink[] = publication.methodologies.map(
    methodology => ({
      key: methodology.id,
      title: methodology.title,
      url: `/methodology/${methodology.id}/summary`,
    }),
  );

  if (publication.externalMethodology) {
    allMethodologies.push({
      key: publication.externalMethodology.url,
      title: publication.externalMethodology.title,
      url: publication.externalMethodology.url,
    });
  }

  return (
    <>
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-two-thirds">
          <div className="govuk-grid-row">
            <BasicReleaseSummary release={release} />
          </div>

          <div id="releaseSummary">
            {release.summarySection && (
              <>
                <EditableSectionBlocks
                  allowComments
                  blocks={release.summarySection.content}
                  sectionId={release.summarySection.id}
                  onBlockCommentsChange={updateBlockComments}
                  renderBlock={block => (
                    <ReleaseBlock block={block} releaseId={release.id} />
                  )}
                  renderEditableBlock={block => (
                    <ReleaseEditableBlock
                      block={block}
                      releaseId={release.id}
                      sectionId={release.summarySection.id}
                      onSave={updateBlock}
                      onDelete={removeBlock}
                    />
                  )}
                />
                {editingMode === 'edit' &&
                  release.summarySection.content?.length === 0 && (
                    <div className="govuk-!-margin-bottom-8 dfe-align--centre">
                      <Button variant="secondary" onClick={addBlock}>
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
            <h2 className="govuk-heading-m">Useful information</h2>

            <ul className="govuk-list govuk-list--spaced govuk-!-margin-bottom-0">
              <li>
                <a
                  href="#dataDownloads-1"
                  className="govuk-button govuk-!-margin-bottom-3"
                >
                  View data and files
                </a>
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

            <dl className="dfe-meta-content">
              <dt className="govuk-caption-m">For {release.coverageTitle}: </dt>
              <dd data-testid="release-name">
                <strong>{release.yearTitle}</strong>

                {releaseCount > 0 && (
                  <Details summary={`See other releases (${releaseCount})`}>
                    <ul className="govuk-list">
                      {[
                        ...release.publication.otherReleases.map(
                          ({ id, title, slug }) => (
                            <li key={id} data-testid="other-release-item">
                              <Link
                                to={`${config?.PublicAppUrl}/find-statistics/${release.publication.slug}/${slug}`}
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
                  </Details>
                )}
              </dd>
            </dl>
            {allMethodologies.length > 0 && (
              <>
                <h3
                  className="govuk-heading-s govuk-!-margin-top-6"
                  id="methodologies"
                >
                  Methodologies
                </h3>
                <ul className="govuk-list govuk-list--spaced govuk-!-margin-bottom-0">
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

      <ReleaseHeadlines release={release} />

      {(release.downloadFiles || release.hasPreReleaseAccessList) && (
        <ReleaseDataAndFilesAccordion
          release={release}
          renderAllFilesButton={
            <Button
              className="govuk-!-width-full"
              disableDoubleClick
              onClick={() => releaseFileService.downloadAllFilesZip(release.id)}
            >
              Download all files
            </Button>
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
              {file.name}
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
              data files guide
            </Link>
          }
          renderPreReleaseAccessLink={
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
              View pre-release access list
            </Link>
          }
        />
      )}

      <ReleaseContentAccordion release={release} sectionName="Contents" />

      <ReleaseHelpAndSupportSection release={release} />
      <PrintThisPage />
    </>
  );
};

export default ReleaseContent;
