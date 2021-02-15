import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import Details from '@common/components/Details';
import FormattedDate from '@common/components/FormattedDate';
import RelatedAside from '@common/components/RelatedAside';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import ContentBlockRenderer from '@common/modules/find-statistics/components/ContentBlockRenderer';
import publicationService, {
  Release,
  ReleaseType,
} from '@common/services/publicationService';
import {
  formatPartialDate,
  isValidPartialDate,
} from '@common/utils/date/partialDate';
import ButtonLink from '@frontend/components/ButtonLink';
import Link from '@frontend/components/Link';
import Page from '@frontend/components/Page';
import PageSearchFormWithAnalytics from '@frontend/components/PageSearchFormWithAnalytics';
import PrintThisPage from '@frontend/components/PrintThisPage';
import PublicationSectionBlocks from '@frontend/modules/find-statistics/components/PublicationSectionBlocks';
import PublicationReleaseHelpAndSupportSection from '@frontend/modules/find-statistics/PublicationReleaseHelpAndSupportSection';
import { logEvent } from '@frontend/services/googleAnalyticsService';
import classNames from 'classnames';
import { GetServerSideProps, NextPage } from 'next';
import React from 'react';
import PublicationReleaseHeadlinesSection from './components/PublicationReleaseHeadlinesSection';
import styles from './PublicationReleasePage.module.scss';

interface Props {
  data: Release;
}

const PublicationReleasePage: NextPage<Props> = ({ data }) => {
  const releaseCount =
    data.publication.otherReleases.length +
    data.publication.legacyReleases.length;

  let methodologyUrl = '';
  let methodologySummary = '';
  if (data.publication.methodology) {
    methodologyUrl = `/methodology/${data.publication.methodology.slug}`;
    methodologySummary = data.publication.methodology.summary;
  } else if (data.publication.externalMethodology) {
    methodologyUrl = data.publication.externalMethodology.url;
  }

  return (
    <Page
      title={data.publication.title}
      caption={data.title}
      description={
        data.summarySection.content && data.summarySection.content.length > 0
          ? data.summarySection.content[0].body
          : ''
      }
      breadcrumbs={[
        { name: 'Find statistics and data', link: '/find-statistics' },
      ]}
    >
      <div className={classNames('govuk-grid-row', styles.releaseIntro)}>
        <div className="govuk-grid-column-two-thirds">
          <div className="dfe-flex dfe-align-items--center dfe-justify-content--space-between">
            <div className="dfe-flex govuk-!-margin-bottom-3">
              {data.latestRelease ? (
                <strong className="govuk-tag govuk-!-margin-right-6">
                  This is the latest data
                </strong>
              ) : (
                <Link
                  className="dfe-print-hidden"
                  unvisited
                  to="/find-statistics/[publication]"
                  as={`/find-statistics/${data.publication.slug}`}
                >
                  View latest data:{' '}
                  <span className="govuk-!-font-weight-bold">
                    {data.publication.otherReleases[0].title}
                  </span>
                </Link>
              )}
            </div>
            <div className="dfe-flex">
              {data.type &&
                data.type.title === ReleaseType.NationalStatistics && (
                  <img
                    src="/assets/images/UKSA-quality-mark2.jpg"
                    alt="UK statistics authority quality mark"
                    height="60"
                    width="60"
                  />
                )}
            </div>
          </div>

          <SummaryList>
            <SummaryListItem term="Published">
              <FormattedDate>{data.published}</FormattedDate>
            </SummaryListItem>
            {isValidPartialDate(data.nextReleaseDate) && (
              <SummaryListItem term="Next update">
                <time>{formatPartialDate(data.nextReleaseDate)}</time>
              </SummaryListItem>
            )}
            {data.updates && data.updates.length > 0 && (
              <SummaryListItem term="Last updated">
                <FormattedDate>{data.updates[0].on}</FormattedDate>

                <Details
                  id="releaseLastUpdates"
                  onToggle={open => {
                    if (open) {
                      logEvent(
                        'Last Updates',
                        'Release page last updates dropdown opened',
                        window.location.pathname,
                      );
                    }
                  }}
                  summary={`See all updates (${data.updates.length})`}
                >
                  <ol className="govuk-list">
                    {data.updates.map(update => (
                      <li key={update.id}>
                        <FormattedDate className="govuk-body govuk-!-font-weight-bold">
                          {update.on}
                        </FormattedDate>
                        <p>{update.reason}</p>
                      </li>
                    ))}
                  </ol>
                </Details>
              </SummaryListItem>
            )}
            <SummaryListItem term="Receive updates">
              <Link
                className="dfe-print-hidden govuk-!-font-weight-bold"
                unvisited
                analytics={{
                  category: 'Subscribe',
                  action: 'Email subscription',
                }}
                to={`/subscriptions?slug=${data.publication.slug}`}
                data-testid={`subscription-${data.publication.slug}`}
              >
                Sign up for email alerts
              </Link>
            </SummaryListItem>
          </SummaryList>

          {data.summarySection.content.map(block => (
            <ContentBlockRenderer key={block.id} block={block} />
          ))}

          <PageSearchFormWithAnalytics
            inputLabel="Search in this release page."
            className="govuk-!-margin-top-3 govuk-!-margin-bottom-3"
          />
        </div>

        <div className="govuk-grid-column-one-third">
          <RelatedAside>
            <h2 className="govuk-heading-m" id="useful-information">
              Useful information
            </h2>
            <nav role="navigation" aria-labelledby="useful-information">
              <ul className="govuk-list govuk-list--spaced govuk-!-margin-bottom-0">
                <li>
                  <a
                    href="#dataDownloads-1"
                    className="govuk-button govuk-!-margin-bottom-3"
                  >
                    View data and files
                  </a>
                </li>
                {data.publication.methodology && (
                  <li>
                    <Link
                      to="/methodology/[methodology]"
                      as={`/methodology/${data.publication.methodology.slug}`}
                    >
                      Methodology
                    </Link>
                  </li>
                )}
                {data.publication.externalMethodology && (
                  <li>
                    <a
                      href={data.publication.externalMethodology.url}
                      target="_blank"
                      rel="noopener noreferrer"
                    >
                      {data.publication.externalMethodology.title}
                    </a>
                  </li>
                )}
                {data.hasMetaGuidance && (
                  <li>
                    <Link
                      to={
                        data.latestRelease
                          ? '/find-statistics/[publication]/meta-guidance'
                          : '/find-statistics/[publication]/[release]/meta-guidance'
                      }
                      as={
                        data.latestRelease
                          ? `/find-statistics/${data.publication.slug}/meta-guidance`
                          : `/find-statistics/${data.publication.slug}/${data.slug}/meta-guidance`
                      }
                    >
                      Metadata guidance document
                    </Link>
                  </li>
                )}
                {data.hasPreReleaseAccessList && (
                  <li>
                    <Link
                      to={
                        data.latestRelease
                          ? '/find-statistics/[publication]/prerelease-access-list'
                          : '/find-statistics/[publication]/[release]/prerelease-access-list'
                      }
                      as={
                        data.latestRelease
                          ? `/find-statistics/${data.publication.slug}/prerelease-access-list`
                          : `/find-statistics/${data.publication.slug}/${data.slug}/prerelease-access-list`
                      }
                    >
                      Pre-release access list
                    </Link>
                  </li>
                )}
                {!!releaseCount && (
                  <>
                    <p className="govuk-!-margin-bottom-0">
                      {data.coverageTitle} <strong>{data.yearTitle}</strong>
                    </p>
                    <Details
                      summary={`See other releases (${releaseCount})`}
                      onToggle={open =>
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
                          ...data.publication.otherReleases.map(
                            ({ id, slug, title }) => (
                              <li key={id} data-testid="other-release-item">
                                <Link
                                  to="/find-statistics/[publication]/[release]"
                                  as={`/find-statistics/${data.publication.slug}/${slug}`}
                                >
                                  {title}
                                </Link>
                              </li>
                            ),
                          ),
                          ...data.publication.legacyReleases.map(
                            ({ id, description, url }) => (
                              <li key={id} data-testid="other-release-item">
                                <a href={url}>{description}</a>
                              </li>
                            ),
                          ),
                        ]}
                      </ul>
                    </Details>
                  </>
                )}
              </ul>
            </nav>
            {data.relatedInformation.length !== 0 && (
              <>
                <h2
                  className="govuk-heading-s govuk-!-margin-top-6"
                  id="related-pages"
                >
                  Related pages
                </h2>
                <nav role="navigation" aria-labelledby="related-pages">
                  <ul className="govuk-list">
                    {data.relatedInformation &&
                      data.relatedInformation.map(link => (
                        <li key={link.id}>
                          <a href={link.url}>{link.description}</a>
                        </li>
                      ))}
                  </ul>
                </nav>
              </>
            )}
          </RelatedAside>
        </div>
      </div>
      <hr />

      <h2 className="dfe-print-break-before">
        Headline facts and figures - {data.yearTitle}
      </h2>

      <PublicationReleaseHeadlinesSection release={data} />

      {data.downloadFiles && (
        <div className={styles.downloadSection}>
          <Accordion
            id="dataDownloads"
            showOpenAll={false}
            onSectionOpen={accordionSection => {
              logEvent(
                `${data.publication.title} release page`,
                `Content accordion opened`,
                `${accordionSection.title}`,
              );
            }}
          >
            <AccordionSection heading="Download data and files">
              <p className="govuk-caption-m">
                Find and download files used in the production of this release.
              </p>
              <ul className="govuk-list govuk-!-width-full">
                {data.downloadFiles.map(
                  ({ id: fileId, fileName, extension, name, size }) => {
                    const isAllFiles = !fileId && name === 'All files';

                    const url = `${process.env.CONTENT_API_BASE_URL}/releases/${
                      data.id
                    }/files/${isAllFiles ? 'all' : fileId}`;

                    return (
                      <li key={isAllFiles ? 'all' : fileId}>
                        <Link
                          to={url}
                          analytics={{
                            category: 'Downloads',
                            action: `Release page ${
                              isAllFiles ? 'all files' : 'file'
                            } downloaded`,
                            label: `Publication: ${data.title}, File: ${fileName}`,
                          }}
                        >
                          {name}
                        </Link>
                        {` (${extension}, ${size})`}
                      </li>
                    );
                  },
                )}

                <li className="govuk-!-margin-top-9">
                  <div className="dfe-flex dfe-justify-content--space-between dfe-align-items--center">
                    <div>
                      <h2 className="govuk-heading-m">
                        Create your own tables online
                      </h2>
                      <p>
                        Explore our range of data and build your own tables from
                        it.
                      </p>
                    </div>
                    <p className="govuk-!-width-one-quarter dfe-flex-shrink--0">
                      <ButtonLink
                        className="govuk-!-width-full"
                        to="/data-tables/[publication]"
                        as={`/data-tables/${data.publication.slug}`}
                      >
                        Create tables
                      </ButtonLink>
                    </p>
                  </div>
                </li>
              </ul>
            </AccordionSection>
          </Accordion>
        </div>
      )}

      {data.content.length > 0 && (
        <Accordion
          id="content"
          onSectionOpen={accordionSection => {
            logEvent(
              `${data.publication.title} release page`,
              `Content accordion opened`,
              `${accordionSection.title}`,
            );
          }}
        >
          {data.content.map(({ heading, caption, order, content }) => {
            return (
              <AccordionSection heading={heading} caption={caption} key={order}>
                <PublicationSectionBlocks blocks={content} release={data} />
              </AccordionSection>
            );
          })}
        </Accordion>
      )}

      <PublicationReleaseHelpAndSupportSection
        accordionId="help-and-support"
        publicationTitle={data.publication.title}
        methodologyUrl={methodologyUrl}
        methodologySummary={methodologySummary}
        publicationContact={data.publication.contact}
        releaseType={data.type.title}
      />

      <h2 className="govuk-heading-m govuk-!-margin-top-9">
        Create your own tables
      </h2>
      <p>Explore our range of data and build your own tables from it.</p>
      <ButtonLink
        to="/data-tables/[publication]"
        as={`/data-tables/${data.publication.slug}`}
      >
        Create tables
      </ButtonLink>

      <PrintThisPage
        analytics={{
          category: 'Page print',
          action: 'Print this page link selected',
        }}
      />
    </Page>
  );
};

export const getServerSideProps: GetServerSideProps<Props> = async ({
  query,
}) => {
  const { publication, release } = query as {
    publication: string;
    release: string;
  };

  const data = await (release
    ? publicationService.getPublicationRelease(publication, release)
    : publicationService.getLatestPublicationRelease(publication));

  return {
    props: {
      data,
    },
  };
};

export default PublicationReleasePage;
