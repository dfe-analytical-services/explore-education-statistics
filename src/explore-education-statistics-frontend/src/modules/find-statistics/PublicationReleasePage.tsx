import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import Details from '@common/components/Details';
import FormattedDate from '@common/components/FormattedDate';
import RelatedAside from '@common/components/RelatedAside';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import Tag from '@common/components/Tag';
import TagGroup from '@common/components/TagGroup';
import ContentBlockRenderer from '@common/modules/find-statistics/components/ContentBlockRenderer';
import ReleaseDataAndFiles from '@common/modules/release/components/ReleaseDataAndFiles';
import publicationService, {
  Release,
} from '@common/services/publicationService';
import { releaseTypes } from '@common/services/types/releaseType';
import { Dictionary } from '@common/types';
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
import {
  logEvent,
  logOutboundLink,
} from '@frontend/services/googleAnalyticsService';
import glossaryService from '@frontend/services/glossaryService';
import classNames from 'classnames';
import orderBy from 'lodash/orderBy';
import { GetServerSideProps, NextPage } from 'next';
import React from 'react';
import VisuallyHidden from '@common/components/VisuallyHidden';
import ScrollableContainer from '@common/components/ScrollableContainer';
import WarningMessage from '@common/components/WarningMessage';
import PublicationReleaseHeadlinesSection from './components/PublicationReleaseHeadlinesSection';
import styles from './PublicationReleasePage.module.scss';

interface Props {
  release: Release;
}

const PublicationReleasePage: NextPage<Props> = ({ release }) => {
  const otherPublicationReleases = release.publication.releases.filter(
    r => r.id !== release.id,
  );
  const otherReleasesCount =
    otherPublicationReleases.length + release.publication.legacyReleases.length;

  // Re-order updates in descending order in-case the cached
  // release from the content API has not been updated to
  // have the updates in the correct order.
  const updates = orderBy(release.updates, 'on', 'desc');

  const showAllFilesButton = release.downloadFiles.some(
    file =>
      file.type === 'Data' ||
      (file.type === 'Ancillary' && file.name !== 'All files'),
  );

  return (
    <Page
      title={release.publication.title}
      caption={release.title}
      description={
        release.summarySection.content &&
        release.summarySection.content.length > 0
          ? release.summarySection.content[0].body
          : ''
      }
      breadcrumbs={[
        { name: 'Find statistics and data', link: '/find-statistics' },
      ]}
    >
      <div className={classNames('govuk-grid-row', styles.releaseIntro)}>
        {release.publication?.isSuperseded ? (
          <WarningMessage testId="superseded-warning">
            This publication has been superseded by{' '}
            <Link
              testId="superseded-by-link"
              to={`/find-statistics/${release.publication.supersededBy?.slug}`}
            >
              {release.publication.supersededBy?.title}
            </Link>
          </WarningMessage>
        ) : null}
        <div className="govuk-grid-column-two-thirds">
          <div className="dfe-flex dfe-align-items--center dfe-justify-content--space-between govuk-!-margin-bottom-3">
            <div>
              {!release.publication.isSuperseded && !release.latestRelease && (
                <Link
                  className="dfe-print-hidden dfe-block govuk-!-margin-bottom-3"
                  unvisited
                  to={`/find-statistics/${release.publication.slug}`}
                >
                  View latest data:{' '}
                  <span className="govuk-!-font-weight-bold">
                    {release.publication.releases[0].title}
                  </span>
                </Link>
              )}
              <TagGroup>
                {!release.publication.isSuperseded && (
                  <>
                    {release.latestRelease ? (
                      <Tag>This is the latest data</Tag>
                    ) : (
                      <Tag colour="orange">This is not the latest data</Tag>
                    )}
                  </>
                )}
                {release.type && <Tag>{releaseTypes[release.type]}</Tag>}
              </TagGroup>
            </div>
            {release.type === 'NationalStatistics' && (
              <img
                src="/assets/images/UKSA-quality-mark.jpg"
                alt="UK statistics authority quality mark"
                height="60"
                width="60"
              />
            )}
          </div>
          <SummaryList>
            <SummaryListItem term="Published">
              <FormattedDate>{release.published}</FormattedDate>
            </SummaryListItem>
            {release.latestRelease &&
              isValidPartialDate(release.nextReleaseDate) && (
                <SummaryListItem
                  term="Next update"
                  testId="next-update-list-item"
                >
                  <time>{formatPartialDate(release.nextReleaseDate)}</time>
                </SummaryListItem>
              )}

            {updates.length > 0 ? (
              <SummaryListItem term="Last updated">
                <FormattedDate>{updates[0].on}</FormattedDate>

                <Details
                  id="releaseLastUpdates"
                  summary={`See all updates (${updates.length})`}
                  hiddenText={`for ${release.title}`}
                  onToggle={open => {
                    if (open) {
                      logEvent({
                        category: 'Last Updates',
                        action: 'Release page last updates dropdown opened',
                        label: window.location.pathname,
                      });
                    }
                  }}
                >
                  <ol className="govuk-list" data-testid="all-updates">
                    {updates.map(update => (
                      <li key={update.id}>
                        <FormattedDate
                          className="govuk-body govuk-!-font-weight-bold"
                          testId="update-on"
                        >
                          {update.on}
                        </FormattedDate>
                        <p data-testid="update-reason">{update.reason}</p>
                      </li>
                    ))}
                  </ol>
                </Details>
              </SummaryListItem>
            ) : null}

            <SummaryListItem term="Receive updates">
              <Link
                className="dfe-print-hidden govuk-!-font-weight-bold"
                unvisited
                to={`/subscriptions?slug=${release.publication.slug}`}
                data-testid={`subscription-${release.publication.slug}`}
                onClick={() => {
                  logEvent({
                    category: 'Subscribe',
                    action: 'Email subscription',
                  });
                }}
              >
                Sign up for email alerts
              </Link>
            </SummaryListItem>
          </SummaryList>
          <VisuallyHidden as="h2">
            {/** 
              Visually hidden h2 as currently the release intro editor only starts from h3
              meaning that this breaks sequential heading order.
              @see {@link https://dfedigital.atlassian.net/browse/EES-3541}
              */}
            Introduction
          </VisuallyHidden>
          {release.summarySection.content.map(block => (
            <ContentBlockRenderer
              key={block.id}
              block={block}
              getGlossaryEntry={glossaryService.getEntry}
              trackContentLinks={(url, newTab) => {
                logOutboundLink(
                  `Publication release summary link: ${url}`,
                  url,
                  newTab,
                );
              }}
              trackGlossaryLinks={glossaryEntrySlug =>
                logEvent({
                  category: `Publication Release Summary Glossary Link`,
                  action: `Glossary link clicked`,
                  label: glossaryEntrySlug,
                })
              }
            />
          ))}
          <PageSearchFormWithAnalytics
            inputLabel="Search in this release page."
            className="govuk-!-margin-top-3 govuk-!-margin-bottom-3"
          />
        </div>

        <div className="govuk-grid-column-one-third">
          <RelatedAside>
            <h2 className="govuk-heading-m" id="quick-links">
              Quick links
            </h2>
            <nav
              role="navigation"
              aria-labelledby="quick-links"
              data-testid="quick-links"
            >
              <ul className="govuk-list">
                {showAllFilesButton && (
                  <li>
                    <ButtonLink
                      className="govuk-button  govuk-!-margin-bottom-3"
                      to={`${process.env.CONTENT_API_BASE_URL}/releases/${release.id}/files`}
                      onClick={() => {
                        logEvent({
                          category: `${release.publication.title} release page - Useful information`,
                          action: 'Download all data button clicked',
                          label: `Publication: ${release.publication.title}, Release: ${release.title}, File: All files`,
                        });
                      }}
                    >
                      Download all data (zip)
                    </ButtonLink>
                  </li>
                )}
                {!!release.relatedDashboardsSection?.content.length && (
                  <li>
                    <a href="#related-dashboards">View related dashboard(s)</a>
                  </li>
                )}
                <li>
                  <a
                    href="#content"
                    onClick={() => {
                      logEvent({
                        category: `${release.publication.title} release page`,
                        action: `Release contents clicked`,
                        label: window.location.pathname,
                      });
                    }}
                  >
                    Release contents
                  </a>
                </li>
                <li>
                  <a
                    href="#explore-data-and-files"
                    onClick={() => {
                      logEvent({
                        category: `${release.publication.title} release page`,
                        action: `View data and files clicked`,
                        label: window.location.pathname,
                      });
                    }}
                  >
                    Explore data
                  </a>
                </li>

                <li>
                  <a
                    href="#help-and-support"
                    onClick={() => {
                      logEvent({
                        category: `${release.publication.title} release page`,
                        action: `Help and support clicked`,
                        label: window.location.pathname,
                      });
                    }}
                  >
                    Help and support
                  </a>
                </li>
              </ul>
            </nav>

            <h2 className="govuk-heading-s">Related information</h2>
            <ul className="govuk-list">
              <li>
                <Link
                  to={
                    release.latestRelease
                      ? `/find-statistics/${release.publication.slug}/data-guidance`
                      : `/find-statistics/${release.publication.slug}/${release.slug}/data-guidance`
                  }
                >
                  Data guidance
                </Link>
              </li>

              {release.hasPreReleaseAccessList && (
                <li>
                  <Link
                    to={
                      release.latestRelease
                        ? `/find-statistics/${release.publication.slug}/prerelease-access-list`
                        : `/find-statistics/${release.publication.slug}/${release.slug}/prerelease-access-list`
                    }
                  >
                    Pre-release access list
                  </Link>
                </li>
              )}
              <li>
                <a href="#contact-us">Contact us</a>
              </li>
            </ul>
            {!!otherReleasesCount && (
              <>
                <h3 className="govuk-heading-s" id="past-releases">
                  Releases in this series
                </h3>

                <Details
                  className="govuk-!-margin-bottom-4"
                  summary={`View releases (${otherReleasesCount})`}
                  hiddenText={`for ${release.publication.title}`}
                  onToggle={open =>
                    open &&
                    logEvent({
                      category: 'Other Releases',
                      action: 'Release page other releases dropdown opened',
                      label: window.location.pathname,
                    })
                  }
                >
                  <ScrollableContainer maxHeight={300}>
                    <ul className="govuk-list">
                      {[
                        ...otherPublicationReleases.map(
                          ({ id, slug, title }) => (
                            <li key={id} data-testid="other-release-item">
                              <Link
                                to={`/find-statistics/${release.publication.slug}/${slug}`}
                              >
                                {title}
                              </Link>
                            </li>
                          ),
                        ),
                        ...release.publication.legacyReleases.map(
                          ({ id, description, url }) => (
                            <li key={id} data-testid="other-release-item">
                              <a href={url}>{description}</a>
                            </li>
                          ),
                        ),
                      ]}
                    </ul>
                  </ScrollableContainer>
                </Details>
              </>
            )}

            {(release.publication.methodologies.length > 0 ||
              release.publication.externalMethodology) && (
              <>
                <h3
                  className="govuk-heading-s govuk-!-padding-top-0"
                  id="methodologies"
                >
                  Methodologies
                </h3>
                <ul className="govuk-list">
                  {release.publication.methodologies.map(methodology => (
                    <li key={methodology.id}>
                      <Link to={`/methodology/${methodology.slug}`}>
                        {methodology.title}
                      </Link>
                    </li>
                  ))}
                  {release.publication.externalMethodology && (
                    <li>
                      <Link
                        to={release.publication.externalMethodology.url}
                        target="_blank"
                        rel="noopener noreferrer"
                      >
                        {release.publication.externalMethodology.title}
                      </Link>
                    </li>
                  )}
                </ul>
              </>
            )}
            {release.relatedInformation.length > 0 && (
              <>
                <h3 className="govuk-heading-s" id="related-pages">
                  Related pages
                </h3>
                <nav role="navigation" aria-labelledby="related-pages">
                  <ul className="govuk-list">
                    {release.relatedInformation &&
                      release.relatedInformation.map(link => (
                        <li key={link.id}>
                          <a
                            href={link.url}
                            onClick={e => {
                              e.preventDefault();
                              logOutboundLink(
                                `Publication release related page link: ${link.url}`,
                                link.url,
                              );
                            }}
                          >
                            {link.description}
                          </a>
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
        Headline facts and figures - {release.yearTitle}
      </h2>

      <PublicationReleaseHeadlinesSection release={release} />

      {(release.downloadFiles ||
        !!release.relatedDashboardsSection?.content.length) && (
        <ReleaseDataAndFiles
          release={release}
          renderAllFilesLink={
            <Link
              to={`${process.env.CONTENT_API_BASE_URL}/releases/${release.id}/files`}
              onClick={() => {
                logEvent({
                  category: 'Downloads',
                  action: `Release page all files downloads.title}, Release: ${release.title}, File: All files`,
                });
              }}
            >
              Download all data (ZIP)
            </Link>
          }
          renderCreateTablesLink={
            <Link
              to={
                release.latestRelease
                  ? `/data-tables/${release.publication.slug}`
                  : `/data-tables/${release.publication.slug}/${release.slug}`
              }
            >
              View or create your own tables
            </Link>
          }
          renderDataCatalogueLink={
            <Link
              to={`/data-catalogue/${release.publication.slug}/${release.slug}`}
            >
              Data catalogue
            </Link>
          }
          renderDownloadLink={file => {
            return (
              <Link
                to={`${process.env.CONTENT_API_BASE_URL}/releases/${release.id}/files/${file.id}`}
                onClick={() => {
                  logEvent({
                    category: 'Downloads',
                    action: 'Release page file downloaded',
                    label: `Publication: ${release.publication.title}, Release: ${release.title}, File: ${file.fileName}`,
                  });
                }}
              >
                {`${file.name} (${file.extension}, ${file.size})`}
              </Link>
            );
          }}
          renderDataGuidanceLink={
            <Link
              to={
                release.latestRelease
                  ? `/find-statistics/${release.publication.slug}/data-guidance`
                  : `/find-statistics/${release.publication.slug}/${release.slug}/data-guidance`
              }
            >
              Data guidance
            </Link>
          }
          onSectionOpen={accordionSection => {
            logEvent({
              category: `${release.publication.title} release page`,
              action: `Data accordion opened`,
              label: accordionSection.title,
            });
          }}
          renderRelatedDashboards={
            release.relatedDashboardsSection?.content.length
              ? release.relatedDashboardsSection.content.map(block => (
                  <ContentBlockRenderer
                    key={block.id}
                    block={block}
                    getGlossaryEntry={glossaryService.getEntry}
                    trackContentLinks={(url, newTab) => {
                      logOutboundLink(
                        `Publication release related dashboards link: ${url}`,
                        url,
                        newTab,
                      );
                    }}
                    trackGlossaryLinks={glossaryEntrySlug =>
                      logEvent({
                        category: `Publication Release Related Dashboards Glossary Link`,
                        action: `Glossary link clicked`,
                        label: glossaryEntrySlug,
                      })
                    }
                  />
                ))
              : null
          }
        />
      )}

      {release.content.length > 0 && (
        <Accordion
          className="govuk-!-margin-top-9"
          id="content"
          onSectionOpen={accordionSection => {
            logEvent({
              category: `${release.publication.title} release page`,
              action: `Content accordion opened`,
              label: `${accordionSection.title}`,
            });
          }}
        >
          {release.content.map(({ heading, caption, order, content }) => {
            return (
              <AccordionSection heading={heading} caption={caption} key={order}>
                {({ open }) => (
                  <PublicationSectionBlocks
                    blocks={content}
                    release={release}
                    visible={open}
                  />
                )}
              </AccordionSection>
            );
          })}
        </Accordion>
      )}

      <PublicationReleaseHelpAndSupportSection
        publicationTitle={release.publication.title}
        methodologies={release.publication.methodologies}
        externalMethodology={release.publication.externalMethodology}
        publicationContact={release.publication.contact}
        releaseType={release.type}
      />

      <PrintThisPage
        onClick={() => {
          logEvent({
            category: 'Page print',
            action: 'Print this page link selected',
            label: window.location.pathname,
          });
        }}
      />
    </Page>
  );
};

export const getServerSideProps: GetServerSideProps<Props> = async ({
  query,
}) => {
  const {
    publication: publicationSlug,
    release: releaseSlug,
  } = query as Dictionary<string>;

  const release = await (releaseSlug
    ? publicationService.getPublicationRelease(publicationSlug, releaseSlug)
    : publicationService.getLatestPublicationRelease(publicationSlug));

  return {
    props: {
      release,
    },
  };
};

export default PublicationReleasePage;
