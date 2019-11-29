import Accordion from '@admin/components/EditableAccordion';
import AccordionSection from '@admin/components/EditableAccordionSection';
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
import { ReleaseType } from '@common/services/publicationService';
import { Dictionary } from '@common/types';
import classNames from 'classnames';
import React from 'react';
import service from '@admin/services/release/edit-release/data/service';
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
  const { release, introductionSection } = content;

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

          <ContentBlock
            sectionId={introductionSection.id}
            publication={publication}
            id={introductionSection.id}
            content={introductionSection.content}
          />

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
            <RelatedInformationSection
              release={release}
              relatedInformation={release.relatedInformation}
            />
          </RelatedAside>
        </div>
      </div>

      <hr />

      <h2 className="dfe-print-break-before">
        Headline facts and figures - {release.yearTitle}
      </h2>

      <DataBlock {...release.keyStatistics} id="keystats" />

      {/* <editor-fold desc="Content blocks"> */}

      <ReleaseContentAccordion
        release={release}
        content={release.content}
        accordionId={accId[0]}
        sectionName="Contents"
      />

      {/* </editor-fold> */}

      {/* <editor-fold desc="Help and support"> */}
      <h2
        className="govuk-heading-m govuk-!-margin-top-9"
        data-testid="extra-information"
      >
        Help and support
      </h2>

      <Accordion
        // publicationTitle={publication.title}
        id="static-content-section"
      >
        <AccordionSection
          heading={`${publication.title}: methodology`}
          caption="Find out how and why we collect, process and publish these statistics"
          headingTag="h3"
        >
          <p>
            Read our{' '}
            <Link to={`/methodology/${release.publication.methodology.id}`}>
              {`${publication.title}: methodology`}
            </Link>{' '}
            guidance.
          </p>
        </AccordionSection>
        {release.type && release.type.title === ReleaseType.NationalStatistics && (
          <AccordionSection heading="National Statistics" headingTag="h3">
            <p className="govuk-body">
              The{' '}
              <a href="https://www.statisticsauthority.gov.uk/">
                United Kingdom Statistics Authority
              </a>{' '}
              designated these statistics as National Statistics in accordance
              with the{' '}
              <a href="https://www.legislation.gov.uk/ukpga/2007/18/contents">
                Statistics and Registration Service Act 2007
              </a>{' '}
              and signifying compliance with the Code of Practice for
              Statistics.
            </p>
            <p className="govuk-body">
              Designation signifying their compliance with the authority's{' '}
              <a href="https://www.statisticsauthority.gov.uk/code-of-practice/the-code/">
                Code of Practice for Statistics
              </a>{' '}
              which broadly means these statistics are:
            </p>
            <ul className="govuk-list govuk-list--bullet">
              <li>
                managed impartially and objectively in the public interest
              </li>
              <li>meet identified user needs</li>
              <li>produced according to sound methods</li>
              <li>well explained and readily accessible</li>
            </ul>
            <p className="govuk-body">
              Once designated as National Statistics it's a statutory
              requirement for statistics to follow and comply with the Code of
              Practice for Statistics to be observed.
            </p>
            <p className="govuk-body">
              Find out more about the standards we follow to produce these
              statistics through our{' '}
              <a href="https://www.gov.uk/government/publications/standards-for-official-statistics-published-by-the-department-for-education">
                Standards for official statistics published by DfE
              </a>{' '}
              guidance.
            </p>
          </AccordionSection>
        )}
        <AccordionSection heading="Contact us" headingTag="h3">
          <p>
            If you have a specific enquiry about {publication.topic.theme.title}{' '}
            statistics and data:
          </p>
          <h4 className="govuk-heading-s govuk-!-margin-bottom-0">
            {publication.contact && publication.contact.teamName}
          </h4>
          <p className="govuk-!-margin-top-0">
            Email <br />
            {publication.contact && (
              <a href={`mailto:${publication.contact.teamEmail}`}>
                {publication.contact.teamEmail}
              </a>
            )}
          </p>
          <p>
            {publication.contact && (
              <>
                Telephone: {publication.contact.contactName} <br />{' '}
                {publication.contact.contactTelNo}
              </>
            )}
          </p>
          <h4 className="govuk-heading-s govuk-!-margin-bottom-0">
            Press office
          </h4>
          <p className="govuk-!-margin-top-0">If you have a media enquiry:</p>
          <p>
            Telephone <br />
            020 7925 6789
          </p>
          <h4 className="govuk-heading-s govuk-!-margin-bottom-0">
            Public enquiries
          </h4>
          <p className="govuk-!-margin-top-0">
            If you have a general enquiry about the Department for Education
            (DfE) or education:
          </p>
          <p>
            Telephone <br />
            037 0000 2288
          </p>
        </AccordionSection>
      </Accordion>
      {/* </editor-fold> */}
    </EditingContext.Provider>
  );
};

export default PublicationReleaseContent;
