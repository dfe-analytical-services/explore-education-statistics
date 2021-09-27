import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import Details from '@common/components/Details';
import FormattedDate from '@common/components/FormattedDate';
import RelatedAside from '@common/components/RelatedAside';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import Tag from '@common/components/Tag';
import ContentBlockRenderer from '@common/modules/find-statistics/components/ContentBlockRenderer';
import ReleaseDataAndFilesAccordion from '@common/modules/release/components/ReleaseDataAndFilesAccordion';
import publicationService, {
  Release,
  ReleaseType,
} from '@common/services/publicationService';
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
import { logEvent } from '@frontend/services/googleAnalyticsService';
import glossaryService from '@frontend/services/glossaryService';
import classNames from 'classnames';
import orderBy from 'lodash/orderBy';
import { GetServerSideProps, NextPage } from 'next';
import React from 'react';
import PublicationReleaseHeadlinesSection from './components/PublicationReleaseHeadlinesSection';
import styles from './PublicationReleasePage.module.scss';

interface Props {
  release: Release;
}

const PublicationReleasePage: NextPage<Props> = ({ release }) => {
  const releaseCount =
    release.publication.otherReleases.length +
    release.publication.legacyReleases.length;

  // Re-order updates in descending order in-case the cached
  // release from the content API has not been updated to
  // have the updates in the correct order.
  const updates = orderBy(release.updates, 'on', 'desc');

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
        <div className="govuk-grid-column-two-thirds">
          <div className="dfe-flex dfe-align-items--center dfe-justify-content--space-between">
            <div className="dfe-flex govuk-!-margin-bottom-3">
              {release.latestRelease ? (
                <Tag strong className="govuk-!-margin-right-6">
                  This is the latest data
                </Tag>
              ) : (
                <Link
                  className="dfe-print-hidden"
                  unvisited
                  to="/find-statistics/[publication]"
                  as={`/find-statistics/${release.publication.slug}`}
                >
                  View latest data:{' '}
                  <span className="govuk-!-font-weight-bold">
                    {release.publication.otherReleases[0].title}
                  </span>
                </Link>
              )}
            </div>
            <div className="dfe-flex">
              {release.type &&
                release.type.title === ReleaseType.NationalStatistics && (
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

          {release.summarySection.content.map(block => (
            <ContentBlockRenderer
              key={block.id}
              block={block}
              getGlossaryEntry={glossaryService.getEntry}
            />
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
                    onClick={() => {
                      logEvent({
                        category: `${release.publication.title} release page`,
                        action: `View data and files clicked`,
                        label: window.location.pathname,
                      });
                    }}
                  >
                    View data and files
                  </a>
                </li>
                {release.hasPreReleaseAccessList && (
                  <li>
                    <Link
                      to={
                        release.latestRelease
                          ? '/find-statistics/[publication]/prerelease-access-list'
                          : '/find-statistics/[publication]/[release]/prerelease-access-list'
                      }
                      as={
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
                {!!releaseCount && (
                  <>
                    <p className="govuk-!-margin-bottom-0">
                      {release.coverageTitle}{' '}
                      <strong>{release.yearTitle}</strong>
                    </p>
                    <Details
                      summary={`See other releases (${releaseCount})`}
                      onToggle={open =>
                        open &&
                        logEvent({
                          category: 'Other Releases',
                          action: 'Release page other releases dropdown opened',
                          label: window.location.pathname,
                        })
                      }
                    >
                      <ul className="govuk-list">
                        {[
                          ...release.publication.otherReleases.map(
                            ({ id, slug, title }) => (
                              <li key={id} data-testid="other-release-item">
                                <Link
                                  to="/find-statistics/[publication]/[release]"
                                  as={`/find-statistics/${release.publication.slug}/${slug}`}
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
                    </Details>
                  </>
                )}
              </ul>
            </nav>
            {release.publication.methodologies.length > 0 && (
              <>
                <h3
                  className="govuk-heading-s govuk-!-margin-top-6"
                  id="methodologies"
                >
                  Methodologies
                </h3>
                <ul className="govuk-list govuk-list--spaced govuk-!-margin-bottom-0">
                  {release.publication.methodologies.map(methodology => (
                    <li key={methodology.id}>
                      <Link
                        to="/methodology/[methodology]"
                        as={`/methodology/${methodology.slug}`}
                      >
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
                <h3
                  className="govuk-heading-s govuk-!-margin-top-6"
                  id="related-pages"
                >
                  Related pages
                </h3>
                <nav role="navigation" aria-labelledby="related-pages">
                  <ul className="govuk-list">
                    {release.relatedInformation &&
                      release.relatedInformation.map(link => (
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
        Headline facts and figures - {release.yearTitle}
      </h2>

      <PublicationReleaseHeadlinesSection release={release} />

      {release.downloadFiles && (
        <ReleaseDataAndFilesAccordion
          release={release}
          onSectionOpen={accordionSection => {
            logEvent({
              category: `${release.publication.title} release page`,
              action: `Content accordion opened`,
              label: `${accordionSection.title}`,
            });
          }}
          renderAllFilesButton={
            <ButtonLink
              className="govuk-!-width-full"
              to={`${process.env.CONTENT_API_BASE_URL}/releases/${release.id}/files/all`}
              onClick={() => {
                logEvent({
                  category: 'Downloads',
                  action: 'Release page all files downloaded',
                  label: `Publication: ${release.title}, File: All files`,
                });
              }}
            >
              Download all files
            </ButtonLink>
          }
          renderCreateTablesButton={
            <CreateTablesButton
              className="govuk-!-width-full"
              release={release}
            />
          }
          renderDataCatalogueLink={
            <Link
              to={
                release.latestRelease
                  ? '/data-catalogue/[publication]'
                  : '/data-catalogue/[publication]/[release]'
              }
              as={
                release.latestRelease
                  ? `/data-catalogue/${release.publication.slug}`
                  : `/data-catalogue/${release.publication.slug}/${release.slug}`
              }
            >
              data catalogue
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
                    label: `Publication: ${release.title}, File: ${file.fileName}`,
                  });
                }}
              >
                {file.name}
              </Link>
            );
          }}
          renderDataGuidanceLink={
            <Link
              to={
                release.latestRelease
                  ? '/find-statistics/[publication]/meta-guidance'
                  : '/find-statistics/[publication]/[release]/meta-guidance'
              }
              as={
                release.latestRelease
                  ? `/find-statistics/${release.publication.slug}/meta-guidance`
                  : `/find-statistics/${release.publication.slug}/${release.slug}/meta-guidance`
              }
            >
              data files guide
            </Link>
          }
        />
      )}

      {release.content.length > 0 && (
        <Accordion
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
                <PublicationSectionBlocks blocks={content} release={release} />
              </AccordionSection>
            );
          })}
        </Accordion>
      )}

      <PublicationReleaseHelpAndSupportSection
        accordionId="help-and-support"
        publicationTitle={release.publication.title}
        methodologies={release.publication.methodologies}
        externalMethodology={release.publication.externalMethodology}
        publicationContact={release.publication.contact}
        releaseType={release.type.title}
      />

      <h2 className="govuk-heading-m govuk-!-margin-top-9">
        Create your own tables
      </h2>
      <p>Explore our range of data and build your own tables from it.</p>
      <CreateTablesButton release={release} />

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

interface CreateTableButtonProps {
  release: Release;
  className?: string;
}

const CreateTablesButton = ({ release, className }: CreateTableButtonProps) => {
  return release.latestRelease ? (
    <ButtonLink
      className={className}
      to="/data-tables/[publication]"
      as={`/data-tables/${release.publication.slug}`}
    >
      Create tables
    </ButtonLink>
  ) : (
    <ButtonLink
      className={className}
      to="/data-tables/[publication]/[release]"
      as={`/data-tables/${release.publication.slug}/${release.slug}`}
    >
      Create tables
    </ButtonLink>
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
