import EditableSectionBlocks from '@admin/components/editable/EditableSectionBlocks';
import Link from '@admin/components/Link';
import { useEditingContext } from '@admin/contexts/EditingContext';
import useConfig from '@admin/hooks/useConfig';
import AdminPublicationReleaseHelpAndSupportSection from '@admin/modules/find-statistics/components/AdminPublicationReleaseHelpAndSupportSection';
import BasicReleaseSummary from '@admin/modules/find-statistics/components/BasicReleaseSummary';
import PrintThisPage from '@admin/modules/find-statistics/components/PrintThisPage';
import ReleaseContentAccordion from '@admin/modules/find-statistics/components/ReleaseContentAccordion';
import { useReleaseState } from '@admin/pages/release/edit-release/content/ReleaseContext';
import useReleaseActions from '@admin/pages/release/edit-release/content/useReleaseActions';
import releaseDataFileService from '@admin/services/releaseDataFileService';
import Button from '@common/components/Button';
import ButtonText from '@common/components/ButtonText';
import Details from '@common/components/Details';
import PageSearchForm from '@common/components/PageSearchForm';
import RelatedAside from '@common/components/RelatedAside';
import React, { useCallback, useMemo } from 'react';
import { CommentsChangeHandler } from './components/Comments';
import RelatedInformationSection from './components/RelatedInformationSection';
import ReleaseHeadlines from './components/ReleaseHeadlines';
import ReleaseNotesSection from './components/ReleaseNotesSection';

const PublicationReleaseContent = () => {
  const { value: config } = useConfig();
  const { isEditing } = useEditingContext();
  const { release } = useReleaseState();
  const actions = useReleaseActions();

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
      <h2 className="govuk-heading-l dfe-print-break-before">
        <span className="govuk-caption-l">{release.title}</span>
        {release.publication.title}
      </h2>

      <div className="govuk-grid-row">
        <div className="govuk-grid-column-two-thirds">
          <div className="govuk-grid-row">
            <BasicReleaseSummary release={release} />
          </div>

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
              <dt className="govuk-caption-m">For {release.coverageTitle}:</dt>
              <dd data-testid="release-name">
                <strong>{release.yearTitle}</strong>
              </dd>
              {releaseCount > 0 && (
                <dd>
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
                </dd>
              )}
            </dl>
            <ReleaseNotesSection release={release} />
            <RelatedInformationSection release={release} />
          </RelatedAside>
        </div>
      </div>

      <hr />

      <ReleaseHeadlines release={release} />

      <ReleaseContentAccordion release={release} sectionName="Contents" />

      <AdminPublicationReleaseHelpAndSupportSection release={release} />
    </>
  );
};

export default PublicationReleaseContent;
