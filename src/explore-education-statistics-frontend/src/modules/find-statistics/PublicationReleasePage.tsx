import AccordionSection from '@common/components/AccordionSection';
import Details from '@common/components/Details';
import FormattedDate from '@common/components/FormattedDate';
import RelatedAside from '@common/components/RelatedAside';
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
import AccordionWithAnalytics from '@frontend/components/AccordionWithAnalytics';
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
          <div className="govuk-grid-row">
            <div className="govuk-grid-column-three-quarters">
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
              <dl className="dfe-meta-content govuk-!-margin-top-3 govuk-!-margin-bottom-1">
                <dt className="govuk-caption-m">Published: </dt>
                <dd data-testid="published-date">
                  <strong>
                    <FormattedDate>{data.published}</FormattedDate>
                  </strong>
                </dd>
                {isValidPartialDate(data.nextReleaseDate) && (
                  <div>
                    <dt className="govuk-caption-m">Next update: </dt>
                    <dd data-testid="next-update">
                      <strong>
                        <time>{formatPartialDate(data.nextReleaseDate)}</time>
                      </strong>
                    </dd>
                  </div>
                )}
              </dl>
              <Link
                className="dfe-print-hidden"
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
            </div>
            <div className="govuk-grid-column-one-quarter">
              {data.type &&
                data.type.title === ReleaseType.NationalStatistics && (
                  <img
                    src="/assets/images/UKSA-quality-mark.jpg"
                    alt="UK statistics authority quality mark"
                    height="120"
                    width="120"
                  />
                )}
            </div>
          </div>

          {data.summarySection.content.map(block => (
            <ContentBlockRenderer key={block.id} block={block} />
          ))}

          {data.downloadFiles && (
            <Details
              summary="Download associated files"
              onToggle={open => {
                if (open) {
                  logEvent(
                    'Downloads',
                    'Release page download associated files dropdown opened',
                    window.location.pathname,
                  );
                }
              }}
            >
              <ul className="govuk-list govuk-list--bullet">
                {data.downloadFiles.map(({ extension, name, path, size }) => (
                  <li key={path}>
                    <Link
                      to={`${process.env.DATA_API_BASE_URL}/download/${path}`}
                      analytics={{
                        category: 'Downloads',
                        action: `Release page ${name} file downloaded`,
                        label: `File URL: /api/download/${path}`,
                      }}
                    >
                      {name}
                    </Link>

                    {` (${extension}, ${size})`}
                  </li>
                ))}
                {data.preReleaseAccessList && (
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
              </ul>
            </Details>
          )}
          <PageSearchFormWithAnalytics
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
              <div className="govuk-!-margin-bottom-2">
                <dt className="govuk-caption-m">For {data.coverageTitle}: </dt>
                <dd data-testid="release-name">
                  <strong>{data.yearTitle}</strong>
                  {!!releaseCount && (
                    <Details
                      summary={`See ${releaseCount} other releases`}
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
                  )}
                </dd>
              </div>

              {data.updates && data.updates.length > 0 && (
                <>
                  <dt className="govuk-caption-m">Last updated: </dt>
                  <dd id="releaseLastUpdated">
                    <strong>
                      <FormattedDate>{data.updates[0].on}</FormattedDate>
                    </strong>
                    <Details
                      id="releaseNotes"
                      onToggle={open => {
                        if (open) {
                          logEvent(
                            'Last Updates',
                            'Release page last updates dropdown opened',
                            window.location.pathname,
                          );
                        }
                      }}
                      summary={`See all ${data.updates.length} updates`}
                    >
                      <ol className="govuk-list">
                        {data.updates.map(note => (
                          <li key={note.id}>
                            <FormattedDate className="govuk-body govuk-!-font-weight-bold">
                              {note.on}
                            </FormattedDate>
                            <p>{note.reason}</p>
                          </li>
                        ))}
                      </ol>
                    </Details>
                  </dd>
                </>
              )}
            </dl>

            {(data.publication.methodology ||
              data.publication.externalMethodology ||
              data.relatedInformation.length !== 0) && (
              <>
                <h2
                  className="govuk-heading-m govuk-!-margin-top-6"
                  id="related-content"
                >
                  Related guidance
                </h2>
                <nav role="navigation" aria-labelledby="related-content">
                  <ul className="govuk-list">
                    {data.publication.methodology && (
                      <li>
                        <Link
                          to="/methodology/[methodology]"
                          as={`/methodology/${data.publication.methodology.slug}`}
                        >
                          {`${data.publication.title}: methodology`}
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

      {data.content.length > 0 && (
        <AccordionWithAnalytics
          publicationTitle={data.publication.title}
          id="content"
        >
          {data.content.map(({ heading, caption, order, content }) => {
            return (
              <AccordionSection heading={heading} caption={caption} key={order}>
                <PublicationSectionBlocks
                  content={content}
                  release={data}
                  onToggle={(section: { id: string; title: string }) => {
                    logEvent(
                      'Publication Release Data Tabs',
                      `${section.title} (${section.id}) tab opened`,
                      window.location.pathname,
                    );
                  }}
                />
              </AccordionSection>
            );
          })}
        </AccordionWithAnalytics>
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
        Create your own tables online
      </h2>
      <p>
        Use our tool to build tables using our range of national and regional
        data.
      </p>
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
