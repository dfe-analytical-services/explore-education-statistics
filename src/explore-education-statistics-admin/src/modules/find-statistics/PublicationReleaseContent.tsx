import Link from '@admin/components/Link';
import BasicReleaseSummary from '@admin/modules/find-statistics/components/BasicReleaseSummary';
import ContentBlock from '@admin/modules/find-statistics/components/EditableContentBlock';
import DataBlock from '@admin/modules/find-statistics/components/EditableDataBlock';
import PrintThisPage from '@admin/modules/find-statistics/components/PrintThisPage';
import ReleaseContentAccordion from '@admin/modules/find-statistics/components/ReleaseContentAccordion';
import { getTimePeriodCoverageDateRangeStringShort } from '@admin/pages/release/util/releaseSummaryUtil';
import { ManageContentPageViewModel } from '@admin/services/release/edit-release/content/types';
import { generateIdList } from '@common/components/Accordion';
import Details from '@common/components/Details';
import FormattedDate from '@common/components/FormattedDate';
import PageSearchForm from '@common/components/PageSearchForm';
import RelatedAside from '@common/components/RelatedAside';
import { EditingContext } from '@common/modules/find-statistics/util/wrapEditableComponent';
import { Dictionary } from '@common/types';
import classNames from 'classnames';
import React from 'react';
import service from '@admin/services/release/edit-release/data/service';
import AdminPublicationReleaseHelpAndSupportSection from '@admin/modules/find-statistics/components/AdminPublicationReleaseHelpAndSupportSection';
import RelatedInformationSection from './components/RelatedInformationSection';

export interface RendererProps {
  contentId?: string;
  releaseId?: string;
}

interface Props {
  editing: boolean;
  content: ManageContentPageViewModel;

  styles: Dictionary<string>;

  logEvent?: (...params: string[]) => void;
}

const nullLogEvent = () => {};

const PublicationReleaseContent = ({
  editing = true,
  content,
  styles,
  logEvent = nullLogEvent,
}: Props) => {
  const { release } = content;

  const accId: string[] = generateIdList(2);

  const releaseCount =
    release.publication.releases.length +
    release.publication.legacyReleases.length;
  const { publication } = release;

  return (
    <EditingContext.Provider
      value={{ isEditing: editing, releaseId: release.id }}
    >
      <h1 className="govuk-heading-l">
        <span className="govuk-caption-l">
          {release.coverageTitle}{' '}
          {getTimePeriodCoverageDateRangeStringShort(release.releaseName, '/')}
        </span>
        {publication.title}
      </h1>

      <div className={classNames('govuk-grid-row', styles.releaseIntro)}>
        <div className="govuk-grid-column-two-thirds">
          <div className="govuk-grid-row">
            <BasicReleaseSummary release={release} />
          </div>

          {release.summarySection && (
            <ContentBlock
              sectionId={release.summarySection.id}
              publication={publication}
              id={release.summarySection.id as string}
              content={release.summarySection.content}
            />
          )}

          {release.downloadFiles && (
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
                      <Link
                        to="#"
                        onClick={() => service.downloadFile(path, name)}
                        className="govuk-link"
                      >
                        {name}
                      </Link>
                      {` (${extension}, ${size})`}
                    </li>
                  ),
                )}
              </ul>
            </Details>
          )}
          <PageSearchForm
            id="search-form"
            inputLabel="Search in this release page."
            className="govuk-!-margin-top-3 govuk-!-margin-bottom-3"
          />
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
              <dd>
                <Details
                  summary={`See previous ${releaseCount} releases`}
                  onToggle={(open: boolean) =>
                    open &&
                    logEvent(
                      'Previous Releases',
                      'Release page previous releases dropdown opened',
                      window.location.pathname,
                    )
                  }
                >
                  <ul className="govuk-list">
                    {[
                      ...release.publication.releases.map(
                        ({ id, slug, releaseName }) => [
                          releaseName,
                          <li key={id} data-testid="previous-release-item">
                            <Link
                              to={`/find-statistics/${release.publication.slug}/${slug}`}
                            >
                              {releaseName}
                            </Link>
                          </li>,
                        ],
                      ),
                      ...release.publication.legacyReleases.map(
                        ({ id, description, url }) => [
                          description,
                          <li key={id} data-testid="previous-release-item">
                            <a href={url}>{description}</a>
                          </li>,
                        ],
                      ),
                    ]
                      .sort((a, b) =>
                        b[0].toString().localeCompare(a[0].toString()),
                      )
                      .map(items => items[1])}
                  </ul>
                </Details>
              </dd>
            </dl>
            <dl className="dfe-meta-content">
              <dt className="govuk-caption-m">Last updated:</dt>
              <dd data-testid="last-updated">
                <strong>
                  <FormattedDate>{release.updates[0].on}</FormattedDate>
                </strong>
                <Details
                  onToggle={(open: boolean) =>
                    open &&
                    logEvent(
                      'Last Updates',
                      'Release page last updates dropdown opened',
                      window.location.pathname,
                    )
                  }
                  summary={`See all ${release.updates.length} updates`}
                >
                  {release.updates.map(elem => (
                    <div data-testid="last-updated-element" key={elem.on}>
                      <FormattedDate className="govuk-body govuk-!-font-weight-bold">
                        {elem.on}
                      </FormattedDate>
                      <p>{elem.reason}</p>
                    </div>
                  ))}
                </Details>
              </dd>
            </dl>
            <RelatedInformationSection release={release} />
          </RelatedAside>
        </div>
      </div>

      <hr />

      <h2 className="dfe-print-break-before">
        Headline facts and figures - {release.yearTitle}
      </h2>

      {release.keyStatisticsSection && (
        <DataBlock {...release.keyStatisticsSection.content[0]} id="keystats" />
      )}

      <ReleaseContentAccordion
        release={release}
        content={release.content}
        accordionId={accId[0]}
        sectionName="Contents"
      />

      <AdminPublicationReleaseHelpAndSupportSection
        publication={publication}
        release={release}
      />
    </EditingContext.Provider>
  );
};

export default PublicationReleaseContent;
