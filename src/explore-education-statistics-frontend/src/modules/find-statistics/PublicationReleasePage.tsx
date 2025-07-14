import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import Details from '@common/components/Details';
import FormattedDate from '@common/components/FormattedDate';
import RelatedContent from '@common/components/RelatedContent';
import Tag from '@common/components/Tag';
import VisuallyHidden from '@common/components/VisuallyHidden';
import ScrollableContainer from '@common/components/ScrollableContainer';
import WarningMessage from '@common/components/WarningMessage';
import ReleaseSummarySection from '@common/modules/release/components/ReleaseSummarySection';
import ContentBlockRenderer from '@common/modules/find-statistics/components/ContentBlockRenderer';
import ReleaseDataAndFiles from '@common/modules/release/components/ReleaseDataAndFiles';
import ReleaseHelpAndSupportSection from '@common/modules/release/components/ReleaseHelpAndSupportSection';
import publicationService, {
  ReleaseVersion,
} from '@common/services/publicationService';
import { Dictionary } from '@common/types';
import Link from '@frontend/components/Link';
import Page from '@frontend/components/Page';
import PageSearchFormWithAnalytics from '@frontend/components/PageSearchFormWithAnalytics';
import PrintThisPage from '@frontend/components/PrintThisPage';
import PublicationSectionBlocks from '@frontend/modules/find-statistics/components/PublicationSectionBlocks';
import { logEvent } from '@frontend/services/googleAnalyticsService';
import glossaryService from '@frontend/services/glossaryService';
import withAxiosHandler from '@frontend/middleware/ssr/withAxiosHandler';
import PublicationReleaseHeadlinesSection from '@frontend/modules/find-statistics/components/PublicationReleaseHeadlinesSection';
import styles from '@frontend/modules/find-statistics/PublicationReleasePage.module.scss';
import classNames from 'classnames';
import orderBy from 'lodash/orderBy';
import { GetServerSideProps, NextPage } from 'next';
import React, { Fragment } from 'react';
import Button from '@common/components/Button';
import downloadService from '@frontend/services/downloadService';

interface Props {
  releaseVersion: ReleaseVersion;
}

const PublicationReleasePage: NextPage<Props> = ({ releaseVersion }) => {
  const releaseSeries = releaseVersion.publication.releaseSeries.filter(
    rsi => rsi.isLegacyLink || rsi.description !== releaseVersion.title,
  );

  const latestReleaseSeriesItem = releaseVersion.publication.releaseSeries.find(
    rsi => !rsi.isLegacyLink,
  );

  // Re-order updates in descending order in-case the cached
  // release from the content API has not been updated to
  // have the updates in the correct order.
  const releaseUpdates = orderBy(releaseVersion.updates, 'on', 'desc');

  const showAllFilesButton = releaseVersion.downloadFiles.some(
    file =>
      file.type === 'Data' ||
      (file.type === 'Ancillary' && file.name !== 'All files'),
  );

  return (
    <Page
      title={releaseVersion.publication.title}
      caption={releaseVersion.title}
      description={
        releaseVersion.summarySection.content &&
        releaseVersion.summarySection.content.length > 0
          ? releaseVersion.summarySection.content[0].body
          : ''
      }
      breadcrumbs={[
        { name: 'Find statistics and data', link: '/find-statistics' },
      ]}
    >
      <div className={classNames('govuk-grid-row', styles.releaseIntro)}>
        {releaseVersion.publication?.isSuperseded && (
          <WarningMessage testId="superseded-warning">
            This publication has been superseded by{' '}
            <Link
              testId="superseded-by-link"
              to={`/find-statistics/${releaseVersion.publication.supersededBy?.slug}`}
            >
              {releaseVersion.publication.supersededBy?.title}
            </Link>
          </WarningMessage>
        )}

        <div className="govuk-grid-column-two-thirds">
          <ReleaseSummarySection
            lastUpdated={releaseUpdates[0]?.on}
            latestRelease={releaseVersion.latestRelease}
            nextReleaseDate={releaseVersion.nextReleaseDate}
            releaseDate={releaseVersion.published}
            releaseType={releaseVersion.type}
            renderReleaseNotes={
              <>
                {releaseUpdates.length > 0 && (
                  <Details
                    id="releaseLastUpdates"
                    summary={`See all updates (${releaseUpdates.length})`}
                    hiddenText={`for ${releaseVersion.title}`}
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
                      {releaseUpdates.map(update => (
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
                )}
              </>
            }
            renderStatusTags={
              <>
                {!releaseVersion.publication.isSuperseded &&
                  !releaseVersion.latestRelease && (
                    <Link
                      className="govuk-!-display-none-print govuk-!-display-block govuk-!-margin-bottom-3"
                      unvisited
                      to={`/find-statistics/${releaseVersion.publication.slug}/${latestReleaseSeriesItem?.releaseSlug}`}
                    >
                      View latest data:{' '}
                      <span className="govuk-!-font-weight-bold">
                        {latestReleaseSeriesItem?.description}
                      </span>
                    </Link>
                  )}
                {!releaseVersion.publication.isSuperseded && (
                  <>
                    {releaseVersion.latestRelease ? (
                      <Tag>This is the latest data</Tag>
                    ) : (
                      <Tag colour="orange">This is not the latest data</Tag>
                    )}
                  </>
                )}
              </>
            }
            renderSubscribeLink={
              <Link
                className="govuk-!-display-none-print govuk-!-font-weight-bold"
                unvisited
                to={`/subscriptions/new-subscription/${releaseVersion.publication.slug}`}
                data-testid={`subscription-${releaseVersion.publication.slug}`}
                onClick={() => {
                  logEvent({
                    category: 'Subscribe',
                    action: 'Email subscription',
                  });
                }}
              >
                Sign up for email alerts
              </Link>
            }
            renderProducerLink={
              releaseVersion.publishingOrganisations?.length ? (
                <span>
                  {releaseVersion.publishingOrganisations.map((org, index) => (
                    <Fragment key={org.id}>
                      {index > 0 && ' and '}
                      <Link unvisited to={org.url}>
                        {org.title}
                      </Link>
                    </Fragment>
                  ))}
                </span>
              ) : (
                <Link
                  unvisited
                  to="https://www.gov.uk/government/organisations/department-for-education"
                >
                  Department for Education
                </Link>
              )
            }
            onShowReleaseTypeModal={() =>
              logEvent({
                category: `${releaseVersion.publication.title} release page`,
                action: 'Release type clicked',
                label: window.location.pathname,
              })
            }
          />

          <VisuallyHidden as="h2">
            {/** 
              Visually hidden h2 as currently the release intro editor only starts from h3
              meaning that this breaks sequential heading order.
              @see {@link https://dfedigital.atlassian.net/browse/EES-3541}
              */}
            Introduction
          </VisuallyHidden>
          {releaseVersion.summarySection.content.map(block => (
            <ContentBlockRenderer
              key={block.id}
              block={block}
              getGlossaryEntry={glossaryService.getEntry}
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
          <RelatedContent testId="useful-information">
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
                    <Button
                      className="govuk-button  govuk-!-margin-bottom-3"
                      onClick={async () => {
                        await downloadService.downloadZip(
                          releaseVersion.id,
                          'ReleaseUsefulInfo',
                        );

                        logEvent({
                          category: `${releaseVersion.publication.title} release page - Useful information`,
                          action: 'Download all data button clicked',
                          label: `Publication: ${releaseVersion.publication.title}, Release: ${releaseVersion.title}, File: All files`,
                        });
                      }}
                    >
                      Download all data (zip)
                    </Button>
                  </li>
                )}
                {!!releaseVersion.relatedDashboardsSection?.content.length && (
                  <li>
                    <a href="#related-dashboards">View related dashboard(s)</a>
                  </li>
                )}
                {!!releaseVersion.content.length && (
                  <li>
                    <a
                      href="#content"
                      onClick={() => {
                        logEvent({
                          category: `${releaseVersion.publication.title} release page`,
                          action: `Release contents clicked`,
                          label: window.location.pathname,
                        });
                      }}
                    >
                      Release contents
                    </a>
                  </li>
                )}
                <li>
                  <a
                    href="#explore-data-and-files"
                    onClick={() => {
                      logEvent({
                        category: `${releaseVersion.publication.title} release page`,
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
                        category: `${releaseVersion.publication.title} release page`,
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
                  to={`/find-statistics/${releaseVersion.publication.slug}/${releaseVersion.slug}/data-guidance`}
                >
                  Data guidance
                </Link>
              </li>

              {releaseVersion.hasPreReleaseAccessList && (
                <li>
                  <Link
                    to={`/find-statistics/${releaseVersion.publication.slug}/${releaseVersion.slug}/prerelease-access-list`}
                  >
                    Pre-release access list
                  </Link>
                </li>
              )}
              <li>
                <a href="#contact-us">Contact us</a>
              </li>
            </ul>
            {!!releaseSeries.length && (
              <>
                <h2 className="govuk-heading-s" id="past-releases">
                  Releases in this series
                </h2>

                <Details
                  className="govuk-!-margin-bottom-4"
                  summary={`View releases (${releaseSeries.length})`}
                  hiddenText={`for ${releaseVersion.publication.title}`}
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
                        ...releaseSeries.map(
                          (
                            {
                              isLegacyLink,
                              description,
                              legacyLinkUrl,
                              releaseSlug,
                            },
                            index,
                          ) => (
                            <li
                              key={`release-${index.toString()}`}
                              data-testid="other-release-item"
                            >
                              {isLegacyLink ? (
                                <a href={legacyLinkUrl}>{description}</a>
                              ) : (
                                <Link
                                  to={`/find-statistics/${releaseVersion.publication.slug}/${releaseSlug}`}
                                >
                                  {description}
                                </Link>
                              )}
                            </li>
                          ),
                        ),
                      ]}
                    </ul>
                  </ScrollableContainer>
                </Details>
              </>
            )}

            {(releaseVersion.publication.methodologies.length > 0 ||
              releaseVersion.publication.externalMethodology) && (
              <>
                <h2
                  className="govuk-heading-s govuk-!-padding-top-0"
                  id="methodologies"
                >
                  Methodologies
                </h2>
                <ul className="govuk-list">
                  {releaseVersion.publication.methodologies.map(methodology => (
                    <li key={methodology.id}>
                      <Link to={`/methodology/${methodology.slug}`}>
                        {methodology.title}
                      </Link>
                    </li>
                  ))}
                  {releaseVersion.publication.externalMethodology && (
                    <li>
                      <Link
                        to={releaseVersion.publication.externalMethodology.url}
                        target="_blank"
                        rel="noopener noreferrer"
                      >
                        {releaseVersion.publication.externalMethodology.title}
                      </Link>
                    </li>
                  )}
                </ul>
              </>
            )}
            {releaseVersion.relatedInformation.length > 0 && (
              <>
                <h2 className="govuk-heading-s" id="related-pages">
                  Related pages
                </h2>
                <ul className="govuk-list">
                  {releaseVersion.relatedInformation &&
                    releaseVersion.relatedInformation.map(link => (
                      <li key={link.id}>
                        <a href={link.url}>{link.description}</a>
                      </li>
                    ))}
                </ul>
              </>
            )}
          </RelatedContent>
        </div>
      </div>
      <hr />

      <h2 className="dfe-print-break-before">
        Headline facts and figures - {releaseVersion.yearTitle}
      </h2>

      <PublicationReleaseHeadlinesSection releaseVersion={releaseVersion} />

      {(releaseVersion.downloadFiles ||
        !!releaseVersion.relatedDashboardsSection?.content.length) && (
        <ReleaseDataAndFiles
          downloadFiles={releaseVersion.downloadFiles}
          hasDataGuidance={releaseVersion.hasDataGuidance}
          renderAllFilesLink={
            <Button
              onClick={async () => {
                await downloadService.downloadZip(
                  releaseVersion.id,
                  'ReleaseDownloads',
                );

                logEvent({
                  category: 'Downloads',
                  action: `Release page all files, Release: ${releaseVersion.title}, File: All files`,
                });
              }}
            >
              Download all data (ZIP)
            </Button>
          }
          renderCreateTablesLink={
            <Link
              to={
                releaseVersion.latestRelease
                  ? `/data-tables/${releaseVersion.publication.slug}`
                  : `/data-tables/${releaseVersion.publication.slug}/${releaseVersion.slug}`
              }
            >
              View or create your own tables
            </Link>
          }
          renderDataCatalogueLink={
            <Link
              to={`/data-catalogue?themeId=${releaseVersion.publication.theme.id}&publicationId=${releaseVersion.publication.id}&releaseVersionId=${releaseVersion.id}`}
            >
              Data catalogue
            </Link>
          }
          renderDownloadLink={file => {
            return (
              <Link
                to={`${process.env.CONTENT_API_BASE_URL}/releases/${releaseVersion.id}/files/${file.id}`}
                onClick={() => {
                  logEvent({
                    category: 'Downloads',
                    action: 'Release page file downloaded',
                    label: `Publication: ${releaseVersion.publication.title}, Release: ${releaseVersion.title}, File: ${file.fileName}`,
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
                releaseVersion.latestRelease
                  ? `/find-statistics/${releaseVersion.publication.slug}/data-guidance`
                  : `/find-statistics/${releaseVersion.publication.slug}/${releaseVersion.slug}/data-guidance`
              }
            >
              Data guidance
            </Link>
          }
          onSectionOpen={accordionSection => {
            logEvent({
              category: `${releaseVersion.publication.title} release page`,
              action: `Data accordion opened`,
              label: accordionSection.title,
            });
          }}
          renderRelatedDashboards={
            releaseVersion.relatedDashboardsSection?.content.length
              ? releaseVersion.relatedDashboardsSection.content.map(block => (
                  <ContentBlockRenderer
                    key={block.id}
                    block={block}
                    getGlossaryEntry={glossaryService.getEntry}
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

      {releaseVersion.content.length > 0 && (
        <Accordion
          className="govuk-!-margin-top-9"
          id="content"
          onSectionOpen={accordionSection => {
            logEvent({
              category: `${releaseVersion.publication.title} release page`,
              action: `Content accordion opened`,
              label: `${accordionSection.title}`,
            });
          }}
        >
          {releaseVersion.content.map(
            ({ heading, caption, order, content }) => {
              return (
                <AccordionSection
                  heading={heading}
                  caption={caption}
                  key={order}
                >
                  {({ open }) => (
                    <PublicationSectionBlocks
                      blocks={content}
                      releaseVersion={releaseVersion}
                      visible={open}
                    />
                  )}
                </AccordionSection>
              );
            },
          )}
        </Accordion>
      )}

      <ReleaseHelpAndSupportSection
        publication={releaseVersion.publication}
        releaseType={releaseVersion.type}
        renderExternalMethodologyLink={externalMethodology => (
          <Link to={externalMethodology.url}>{externalMethodology.title}</Link>
        )}
        renderMethodologyLink={methodology => (
          <Link to={`/methodology/${methodology.slug}`}>
            {methodology.title}
          </Link>
        )}
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

export const getServerSideProps: GetServerSideProps = withAxiosHandler(
  async ({ query }) => {
    const { publication: publicationSlug, release: releaseSlug } =
      query as Dictionary<string>;

    const releaseVersion = await (releaseSlug
      ? publicationService.getPublicationRelease(publicationSlug, releaseSlug)
      : publicationService.getLatestPublicationRelease(publicationSlug));

    if (!releaseSlug) {
      return {
        redirect: {
          destination: `/find-statistics/${publicationSlug}/${releaseVersion.slug}`,
          permanent: true,
        },
      };
    }

    return {
      props: {
        releaseVersion,
      },
    };
  },
);

export default PublicationReleasePage;
