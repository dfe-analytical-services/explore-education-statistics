import Accordion, { generateIdList } from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import Details from '@common/components/Details';
import FormattedDate from '@common/components/FormattedDate';
import PageSearchFormWithAnalytics from '@frontend/components/PageSearchFormWithAnalytics';
import RelatedAside from '@common/components/RelatedAside';
import DataBlockWithAnalytics from '@frontend/components/DataBlockWithAnalytics';
import { baseUrl } from '@common/services/api';
import publicationService, {
  Release,
  ReleaseType,
} from '@common/services/publicationService';
import ButtonLink from '@frontend/components/ButtonLink';
import HelpAndSupport from '@frontend/modules/find-statistics/PublicationReleaseHelpAndSupportSection';
import { logEvent } from '@frontend/services/googleAnalyticsService';
import Link from '@frontend/components/Link';
import Page from '@frontend/components/Page';
import PrintThisPage from '@frontend/components/PrintThisPage';
import classNames from 'classnames';
import { NextContext } from 'next';
import React, { Component } from 'react';
import ReactMarkdown from 'react-markdown';
import ContentBlock from '@common/modules/find-statistics/components/ContentBlocks';
import styles from './PublicationReleasePage.module.scss';

interface Props {
  publication: string;
  release: string;
  data: Release;
}

class PublicationReleasePage extends Component<Props> {
  private accId: string[] = generateIdList(2);

  public static async getInitialProps({
    query,
  }: NextContext<{
    publication: string;
    release: string;
  }>) {
    const { publication, release } = query;

    const request = release
      ? publicationService.getPublicationRelease(publication, release)
      : publicationService.getLatestPublicationRelease(publication);

    const data = await request;

    return {
      data,
      publication,
      release,
    };
  }

  public render() {
    const { data } = this.props;

    const releaseCount =
      data.publication.releases.length + data.publication.legacyReleases.length;

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
                    to={`/find-statistics/${data.publication.slug}`}
                  >
                    View latest data:{' '}
                    <span className="govuk-!-font-weight-bold">
                      {data.publication.releases.slice(-1)[0].title}
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
                  <div>
                    <dt className="govuk-caption-m">Next update: </dt>
                    <dd data-testid="next-update">
                      <strong>
                        <FormattedDate format="MMMM yyyy">
                          {data.publication.nextUpdate}
                        </FormattedDate>
                      </strong>
                    </dd>
                  </div>
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
                      src="/static/images/UKSA-quality-mark.jpg"
                      alt="UK statistics authority quality mark"
                      height="120"
                      width="120"
                    />
                  )}
              </div>
            </div>

            <ReactMarkdown
              className="govuk-body"
              source={
                data.summarySection.content &&
                data.summarySection.content.length > 0
                  ? data.summarySection.content[0].body
                  : ''
              }
            />
            {data.downloadFiles && (
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
                  {data.downloadFiles.map(({ extension, name, path, size }) => (
                    <li key={path}>
                      <Link
                        to={`${baseUrl.data}/download/${path}`}
                        className="govuk-link"
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
                <dt className="govuk-caption-m">For {data.coverageTitle}: </dt>
                <dd data-testid="release-name">
                  <strong>{data.yearTitle}</strong>
                </dd>
                {!!releaseCount && (
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
                          ...data.publication.releases.map(
                            ({ id, slug, releaseName }) => [
                              releaseName,
                              <li key={id} data-testid="previous-release-item">
                                <Link
                                  to={`/find-statistics/${data.publication.slug}/${slug}`}
                                >
                                  {releaseName}
                                </Link>
                              </li>,
                            ],
                          ),
                          ...data.publication.legacyReleases.map(
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
                )}
              </dl>
              {data.updates && data.updates.length > 0 && (
                <dl className="dfe-meta-content">
                  <dt className="govuk-caption-m">Last updated:</dt>
                  <dd data-testid="last-updated">
                    <strong>
                      <FormattedDate>{data.updates[0].on}</FormattedDate>
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
                      summary={`See all ${data.updates.length} updates`}
                    >
                      {data.updates.map(elem => (
                        <div data-testid="last-updated-element" key={elem.id}>
                          <FormattedDate className="govuk-body govuk-!-font-weight-bold">
                            {elem.on}
                          </FormattedDate>
                          <p>{elem.reason}</p>
                        </div>
                      ))}
                    </Details>
                  </dd>
                </dl>
              )}
              <h2
                className="govuk-heading-m govuk-!-margin-top-6"
                id="related-content"
              >
                Related guidance
              </h2>
              <nav role="navigation" aria-labelledby="related-content">
                <ul className="govuk-list">
                  <li>
                    <Link to={`/methodology/${data.publication.slug}`}>
                      {`${data.publication.title}: methodology`}
                    </Link>
                  </li>
                  {data.relatedInformation &&
                    data.relatedInformation.map(link => (
                      <li key={link.id}>
                        <a href={link.url}>{link.description}</a>
                      </li>
                    ))}
                </ul>
              </nav>
            </RelatedAside>
          </div>
        </div>
        <hr />

        <h2 className="dfe-print-break-before">
          Headline facts and figures - {data.yearTitle}
        </h2>

        {data.keyStatisticsSection && data.keyStatisticsSection.content && (
          <DataBlockWithAnalytics
            {...data.keyStatisticsSection.content[0]}
            id="keystats"
          />
        )}

        {data.content.length > 0 && (
          <Accordion id={this.accId[0]}>
            {data.content.map(({ heading, caption, order, content }) => {
              return (
                <AccordionSection
                  heading={heading}
                  caption={caption}
                  key={order}
                >
                  <ContentBlock
                    content={content}
                    id={`content_${order}`}
                    publication={data.publication}
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
          </Accordion>
        )}

        <HelpAndSupport
          accordionId={this.accId[1]}
          publicationTitle={data.publication.title}
          methodologyUrl={`/methodology/${data.publication.slug}`}
          themeTitle={data.publication.topic.theme.title}
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
          prefetch
          as={`/data-tables/${data.publication.slug}`}
          href={`/data-tables?publicationSlug=${data.publication.slug}`}
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
  }
}

export default PublicationReleasePage;
