import { CommentsChangeHandler } from '@admin/components/editable/Comments';
import EditableSectionBlocks from '@admin/components/editable/EditableSectionBlocks';
import Link from '@admin/components/Link';
import PrintThisPage from '@admin/components/PrintThisPage';
import { useConfig } from '@admin/contexts/ConfigContext';
import { useEditingContext } from '@admin/contexts/EditingContext';
import BasicReleaseSummary from '@admin/pages/release/content/components/BasicReleaseSummary';
import RelatedInformationSection from '@admin/pages/release/content/components/RelatedInformationSection';
import ReleaseContentAccordion from '@admin/pages/release/content/components/ReleaseContentAccordion';
import ReleaseHeadlines from '@admin/pages/release/content/components/ReleaseHeadlines';
import ReleaseHelpAndSupportSection from '@admin/pages/release/content/components/ReleaseHelpAndSupportSection';
import ReleaseNotesSection from '@admin/pages/release/content/components/ReleaseNotesSection';
import { useReleaseContentState } from '@admin/pages/release/content/contexts/ReleaseContentContext';
import useReleaseContentActions from '@admin/pages/release/content/contexts/useReleaseContentActions';
import releaseDataFileService from '@admin/services/releaseDataFileService';
import Button from '@common/components/Button';
import ButtonText from '@common/components/ButtonText';
import Details from '@common/components/Details';
import PageSearchForm from '@common/components/PageSearchForm';
import RelatedAside from '@common/components/RelatedAside';
import React, { useCallback, useMemo } from 'react';

const ReleaseContent = () => {
  const config = useConfig();
  const { isEditing } = useEditingContext();
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
                  releaseId={release.id}
                  allowComments
                  sectionId={release.summarySection.id}
                  content={release.summarySection.content}
                  onBlockContentSave={updateBlock}
                  onBlockDelete={removeBlock}
                  onBlockCommentsChange={updateBlockComments}
                />
                {isEditing && release.summarySection.content?.length === 0 && (
                  <div className="govuk-!-margin-bottom-8 dfe-align--centre">
                    <Button variant="secondary" onClick={addBlock}>
                      Add a summary text block
                    </Button>
                  </div>
                )}
              </>
            )}
          </div>

          {release.downloadFiles && !isEditing && (
            <Details summary="Download associated files">
              <ul className="govuk-list govuk-list--bullet">
                {release.downloadFiles.map(
                  ({ extension, name, path, size }) => (
                    <li key={path}>
                      <ButtonText
                        onClick={() =>
                          releaseDataFileService.downloadFile(path)
                        }
                        className="govuk-link"
                      >
                        {name}
                      </ButtonText>
                      {` (${extension}, ${size})`}
                    </li>
                  ),
                )}
              </ul>
            </Details>
          )}
          {!isEditing && (
            <PageSearchForm
              id="search-form"
              inputLabel="Search in this release page."
              className="govuk-!-margin-top-3 govuk-!-margin-bottom-3"
            />
          )}
        </div>

        <div className="govuk-grid-column-one-third">
          <PrintThisPage
            analytics={{
              category: 'Page print',
              action: 'Print this page link selected',
            }}
          />
          <RelatedAside>
            <h2 className="govuk-heading-m">About these statistics</h2>

            <dl className="dfe-meta-content">
              <dt className="govuk-caption-m">For {release.coverageTitle}: </dt>
              <dd data-testid="release-name">
                <strong>{release.yearTitle}</strong>

                {releaseCount > 0 && (
                  <Details summary={`See ${releaseCount} other releases`}>
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
            <ReleaseNotesSection release={release} />
            <RelatedInformationSection release={release} />
          </RelatedAside>
        </div>
      </div>

      <hr />

      <ReleaseHeadlines release={release} />

      <ReleaseContentAccordion release={release} sectionName="Contents" />

      <ReleaseHelpAndSupportSection release={release} />
    </>
  );
};

export default ReleaseContent;
