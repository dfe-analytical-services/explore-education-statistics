import ContentBlocks from '@admin/components/editable/EditableContentBlocks';
import Link from '@admin/components/Link';
import AdminPublicationReleaseHelpAndSupportSection from '@admin/modules/find-statistics/components/AdminPublicationReleaseHelpAndSupportSection';
import BasicReleaseSummary from '@admin/modules/find-statistics/components/BasicReleaseSummary';
import PrintThisPage from '@admin/modules/find-statistics/components/PrintThisPage';
import ReleaseContentAccordion from '@admin/modules/find-statistics/components/ReleaseContentAccordion';
import useReleaseActions from '@admin/pages/release/edit-release/content/useReleaseActions';
import { useReleaseState } from '@admin/pages/release/edit-release/content/ReleaseContext';
import { getTimePeriodCoverageDateRangeStringShort } from '@admin/pages/release/util/releaseSummaryUtil';
import editReleaseDataService from '@admin/services/release/edit-release/data/editReleaseDataService';
import Button from '@common/components/Button';
import ButtonText from '@common/components/ButtonText';
import Details from '@common/components/Details';
import PageSearchForm from '@common/components/PageSearchForm';
import RelatedAside from '@common/components/RelatedAside';
import { EditingContext } from '@common/modules/find-statistics/util/wrapEditableComponent';
import React, { useContext, useMemo, useCallback } from 'react';
import RelatedInformationSection from './components/RelatedInformationSection';
import ReleaseHeadlines from './components/ReleaseHeadlines';
import ReleaseNotesSection from './components/ReleaseNotesSection';

export interface RendererProps {
  contentId?: string;
  releaseId?: string;
}

const PublicationReleaseContent = () => {
  const { isEditing } = useContext(EditingContext);
  const { release } = useReleaseState();
  const {
    addContentSectionBlock,
    updateContentSectionBlock,
    deleteContentSectionBlock,
  } = useReleaseActions();

  const releaseCount = useMemo(() => {
    if (release) {
      return (
        release.publication.otherReleases.length +
        release.publication.legacyReleases.length
      );
    }
    return 0;
  }, [release]);

  const addSummaryBlock = useCallback(() => {
    if (release)
      addContentSectionBlock({
        releaseId: release.id,
        sectionId: release.summarySection.id,
        sectionKey: 'summarySection',
        block: {
          type: 'MarkdownBlock',
          order: 0,
          body: '',
        },
      });
  }, [release, addContentSectionBlock]);

  const summaryBlockUpdate = useCallback(
    (blockId, bodyContent) => {
      if (release)
        updateContentSectionBlock({
          releaseId: release.id,
          sectionId: release.summarySection.id,
          blockId,
          sectionKey: 'summarySection',
          bodyContent,
        });
    },
    [release, updateContentSectionBlock],
  );

  const summaryBlockDelete = useCallback(
    (blockId: string) => {
      if (release)
        deleteContentSectionBlock({
          releaseId: release.id,
          sectionId: release.summarySection.id,
          blockId,
          sectionKey: 'summarySection',
        });
    },
    [release, deleteContentSectionBlock],
  );

  if (release === undefined) return null;
  return (
    <>
      <h1 className="govuk-heading-l">
        <span className="govuk-caption-l">
          {`${
            release.coverageTitle
          } ${getTimePeriodCoverageDateRangeStringShort(
            release.releaseName,
            '/',
          )}`}
        </span>
        {release.publication.title}
      </h1>

      <div className="govuk-grid-row">
        <div className="govuk-grid-column-two-thirds">
          <div className="govuk-grid-row">
            <BasicReleaseSummary release={release} />
          </div>

          {release.summarySection && (
            <>
              <ContentBlocks
                sectionId={release.summarySection.id}
                publication={release.publication}
                id={release.summarySection.id as string}
                content={release.summarySection.content}
                onBlockContentChange={summaryBlockUpdate}
                onBlockDelete={summaryBlockDelete}
                allowComments
              />
              {release.summarySection.content?.length === 0 && (
                <div className="govuk-!-margin-bottom-8 dfe-align--center">
                  <Button variant="secondary" onClick={addSummaryBlock}>
                    Add a summary text block
                  </Button>
                </div>
              )}
            </>
          )}

          {release.downloadFiles && !isEditing && (
            <Details summary="Download data files">
              <ul className="govuk-list govuk-list--bullet">
                {release.downloadFiles.map(
                  ({ extension, name, path, size }) => (
                    <li key={path}>
                      <ButtonText
                        onClick={() =>
                          editReleaseDataService.downloadFile(path, name)
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
                          ({ id, title }) => [
                            title,
                            <li key={id} data-testid="other-release-item">
                              <Link to="#">{title}</Link>
                            </li>,
                          ],
                        ),
                        ...release.publication.legacyReleases.map(
                          ({ id, description, url }) => [
                            description,
                            <li key={id} data-testid="other-release-item">
                              {!isEditing ? (
                                <a href={url}>{description}</a>
                              ) : (
                                <a>{description}</a>
                              )}
                            </li>,
                          ],
                        ),
                      ]
                        .sort((a, b) =>
                          b[0].toString().localeCompare(a[0].toString()),
                        )
                        .map(([, link]) => link)}
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

      <ReleaseContentAccordion
        release={release}
        accordionId="contents-accordion"
        sectionName="Contents"
      />

      <AdminPublicationReleaseHelpAndSupportSection release={release} />
    </>
  );
};

export default PublicationReleaseContent;
