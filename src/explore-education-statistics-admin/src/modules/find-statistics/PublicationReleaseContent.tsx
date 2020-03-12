import Link from '@admin/components/Link';
import AdminPublicationReleaseHelpAndSupportSection from '@admin/modules/find-statistics/components/AdminPublicationReleaseHelpAndSupportSection';
import BasicReleaseSummary from '@admin/modules/find-statistics/components/BasicReleaseSummary';
import PrintThisPage from '@admin/modules/find-statistics/components/PrintThisPage';
import ReleaseContentAccordion from '@admin/modules/find-statistics/components/ReleaseContentAccordion';
import { getTimePeriodCoverageDateRangeStringShort } from '@admin/pages/release/util/releaseSummaryUtil';
import { releaseContentService } from '@admin/services/release/edit-release/content/service';
import { ManageContentPageViewModel } from '@admin/services/release/edit-release/content/types';
import editReleaseDataService from '@admin/services/release/edit-release/data/editReleaseDataService';
import ButtonText from '@common/components/ButtonText';
import Details from '@common/components/Details';
import PageSearchForm from '@common/components/PageSearchForm';
import RelatedAside from '@common/components/RelatedAside';
import { EditingContext } from '@common/modules/find-statistics/util/wrapEditableComponent';
import { DataBlock as DataBlockModel } from '@common/services/dataBlockService';
import { Dictionary } from '@common/types';
import classNames from 'classnames';
import React, { useCallback, useState } from 'react';
import ContentBlocks from './components/EditableContentBlocks';
import RelatedInformationSection from './components/RelatedInformationSection';
import ReleaseHeadlines from './components/ReleaseHeadlines';
import ReleaseNotesSection from './components/ReleaseNotesSection';

export interface RendererProps {
  contentId?: string;
  releaseId?: string;
}

interface Props {
  editing: boolean;
  content: ManageContentPageViewModel;

  styles: Dictionary<string>;

  logEvent?: (...params: string[]) => void;

  onReleaseChange?: (content: ManageContentPageViewModel['release']) => void;

  availableDataBlocks: DataBlockModel[];
}

const nullLogEvent = () => {};

const PublicationReleaseContent = ({
  editing = true,
  content,
  styles,
  logEvent = nullLogEvent,
  onReleaseChange,
  availableDataBlocks: initialAvailableDataBlocks,
}: Props) => {
  const [release, setRelease] = useState(content.release);

  const setReleaseAndTriggerOnReleaseChange = useCallback(
    (newRelease: ManageContentPageViewModel['release']) => {
      if (onReleaseChange) onReleaseChange(newRelease);
      setRelease(newRelease);
    },
    [setRelease, onReleaseChange],
  );

  const [availableDataBlocks, setAvailableDataBlocks] = useState(
    initialAvailableDataBlocks,
  );

  const updateAvailableDataBlocks = () => {
    releaseContentService
      .getAvailableDataBlocks(release.id)
      .then(newAvailableDataBlocks => {
        setAvailableDataBlocks(newAvailableDataBlocks);
      });
  };

  const releaseCount = React.useMemo(
    () =>
      release.publication.otherReleases.length +
      release.publication.legacyReleases.length,
    [
      release.publication.legacyReleases.length,
      release.publication.otherReleases.length,
    ],
  );

  const publication = React.useMemo(() => release.publication, [release]);

  const onAccordionContentChange = useCallback(
    newContent => {
      setReleaseAndTriggerOnReleaseChange({
        ...release,
        content: newContent,
      });
    },
    [release, setReleaseAndTriggerOnReleaseChange],
  );

  const onSummaryContentChange = useCallback(
    newContent => {
      setReleaseAndTriggerOnReleaseChange({
        ...release,
        summarySection: {
          ...release.summarySection,
          content: newContent,
        },
      });
    },
    [release, setReleaseAndTriggerOnReleaseChange],
  );

  return (
    <EditingContext.Provider
      value={{
        isEditing: editing,
        releaseId: release.id,
        isReviewing: false,
        isCommenting: true,
        availableDataBlocks,
        updateAvailableDataBlocks,
      }}
    >
      <h1 className="govuk-heading-l">
        <span className="govuk-caption-l">
          {`${
            release.coverageTitle
          } ${getTimePeriodCoverageDateRangeStringShort(
            release.releaseName,
            '/',
          )}`}
        </span>
        {publication.title}
      </h1>

      <div className={classNames('govuk-grid-row', styles.releaseIntro)}>
        <div className="govuk-grid-column-two-thirds">
          <div className="govuk-grid-row">
            <BasicReleaseSummary release={release} />
          </div>

          {release.summarySection && (
            <ContentBlocks
              sectionId={release.summarySection.id}
              publication={publication}
              id={release.summarySection.id as string}
              addContentButtonText="Add summary content"
              content={release.summarySection.content}
              canAddSingleBlock
              textOnly
              onContentChange={onSummaryContentChange}
            />
          )}

          {release.downloadFiles && !editing && (
            <Details
              summary="Download data files"
              onToggle={(open: boolean) =>
                open &&
                logEvent(
                  'Downloads',
                  'Release page download data files dropdown opened',
                  window.location.pathname,
                )
              }
            >
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
          {!editing && (
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
                  <Details
                    summary={`See ${releaseCount} other releases`}
                    onToggle={(open: boolean) =>
                      open &&
                      logEvent(
                        'Other Releases',
                        'Release page other releases dropdown opened',
                        window.location.pathname,
                      )
                    }
                  >
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
                              {!editing ? (
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

      <ReleaseHeadlines
        release={release}
        setRelease={setReleaseAndTriggerOnReleaseChange}
      />

      <ReleaseContentAccordion
        releaseId={release.id}
        publication={publication}
        accordionId="contents-accordion"
        sectionName="Contents"
        onContentChange={onAccordionContentChange}
      />

      <AdminPublicationReleaseHelpAndSupportSection
        publication={publication}
        release={release}
      />
    </EditingContext.Provider>
  );
};

export default PublicationReleaseContent;
