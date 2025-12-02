import classNames from 'classnames';
import Link from '@admin/components/Link';
import PrototypePage from '@admin/prototypes/components/PrototypePage';
import Details from '@common/components/Details';
import PageSearchForm from '@common/components/PageSearchForm';
import RelatedContent from '@common/components/RelatedContent';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import stylesKeyStat from '@common/modules/find-statistics/components/KeyStat.module.scss';
import stylesKeyStatTile from '@common/modules/find-statistics/components/KeyStatTile.module.scss';
import React, { useState } from 'react';
import Accordion from './components/PrototypeAccordion';
import AccordionSection from './components/PrototypeAccordionSection';
import PrototypeDownloadAncillary from './components/PrototypeDownloadAncillary';
import PrototypeDownloadPopular from './components/PrototypeDownloadPopular';
import PrototypeDownloadUnderlying from './components/PrototypeDownloadUnderlying';
import PrototypeTableBuilder from './components/PrototypeTableBuilder';
import styles from './PrototypePublicPage.module.scss';

const PrototypeReleaseData = () => {
  const [showContents, setShowContents] = useState(true);

  return (
    <div className={styles.prototypePublicPage}>
      <PrototypePage
        breadcrumbs={[
          {
            name: 'Further education',
            link: '#',
          },
          {
            name: 'Apprenticeships and traineeships',
            link: '#',
          },
        ]}
        title="Apprenticeships and traineeships"
        caption="Academic Year 2021/22"
        wide={false}
      >
        <div className="govuk-grid-row">
          <div className="govuk-grid-column-two-thirds">
            <div className="govuk-grid-row">
              <div className={styles.prototypeFlexContainer}>
                <div>
                  <span className="govuk-tag">This is the latest data</span>{' '}
                  <span className="govuk-tag">National statistics</span>
                </div>

                <img
                  src="/assets/images/accredited-official-statistics-logo.svg"
                  className="govuk-!-margin-right-3"
                  alt="UK statistics authority quality mark"
                  height="60"
                  width="60"
                />
              </div>
            </div>
            <dl className="govuk-summary-list">
              <div className="govuk-summary-list__row">
                <dt className="govuk-summary-list__key">Published: </dt>
                <dd className="govuk-summary-list__value">
                  <time>27 January 2022</time>
                </dd>
              </div>
              <div className="govuk-summary-list__row">
                <dt className="govuk-summary-list__key">Last updated: </dt>
                <dd className="govuk-summary-list__value">
                  <time>22 December 2022</time>
                  <Details
                    className="govuk-!-margin-top-2"
                    summary="See all updates (18)"
                  >
                    <ol className="govuk-list">
                      <li>
                        <time className="govuk-body govuk-!-font-weight-bold">
                          18 December 2020
                        </time>
                        <p>
                          Local authority level data on placement stability and
                          local authority level data for care leavers have been
                          added.
                        </p>
                      </li>
                      <li>
                        <time className="govuk-body govuk-!-font-weight-bold">
                          11 December 2020
                        </time>
                        <p>
                          Correction to metadata for LA level 'children ceasing
                          to be looked after' file
                        </p>
                      </li>
                    </ol>
                  </Details>
                </dd>
              </div>
              <div className="govuk-summary-list__row">
                <dt className="govuk-summary-list__key">Underlying data: </dt>
                <dd className="govuk-summary-list__value">
                  <a href="#exploreData">
                    Explore data and files used in this release
                  </a>
                </dd>
              </div>
              <div className="govuk-summary-list__row">
                <dt className="govuk-summary-list__key">Receive updates: </dt>
                <dd className="govuk-summary-list__value">
                  <a
                    data-testid="subscription-children-looked-after-in-england-including-adoptions"
                    href="/subscriptions/new-subscription/children-looked-after-in-england-including-adoptions"
                    className="govuk-link govuk-link--no-visited-state dfe-print-hidden"
                  >
                    Sign up for email alerts
                  </a>
                </dd>
              </div>
            </dl>
            <div className="dfe-content">
              <h4>December 2022 update</h4>
              <p>
                This transparency update makes changes to the ‘Apprenticeship
                Service and monthly transparency data’ and ‘Additional analysis’
                sections. The latest available apprenticeship service and Find
                an apprenticeship data is provided.{' '}
              </p>
              <p>
                Please note: the first in-year apprenticeship starts data for
                the 2022/23 academic year, covering August to October 2022 will
                be published in January 2023.
              </p>
              <h4>
                Impact of COVID-19 on reporting of FE and apprenticeship data
              </h4>
              <p>
                Data in this release covers a period affected by varying
                COVID-19 restrictions, which will have impacted on
                apprenticeship and traineeship learning and also provider
                reporting behaviour via the Individualised Learner Record.
                Therefore, extra care should be taken in comparing and
                interpreting data presented in this release.
              </p>
            </div>

            <PageSearchForm inputLabel="Search this page" />
          </div>
          <div className="govuk-grid-column-one-third">
            <RelatedContent>
              <h2 className="govuk-heading-m">Quick links</h2>
              <ul className="govuk-list">
                <li>
                  <a
                    href="#data-1"
                    className="govuk-button govuk-!-margin-bottom-3"
                  >
                    Download all data (ZIP)
                  </a>
                </li>
                <li>
                  <Link to="#content">Release contents</Link>
                </li>
                <li>
                  <Link to="#exploreData">Explore data</Link>
                </li>
                <li>
                  <Link to="#extra-information">Help and support</Link>
                </li>
              </ul>
              <h3 className="govuk-heading-s">Related information</h3>
              <ul className="govuk-list">
                <li>
                  <Link to="./table-highlights-2?source=publicationPage">
                    View or create tables
                  </Link>
                </li>
                <li>
                  <Link to="./data-catalog?theme=fe&amp;publication=traineeships&amp;source=publicationPage">
                    Data catalogue
                  </Link>
                </li>
                <li>
                  <Link to="#">Data guidance</Link>
                </li>
                <li>
                  <Link to="#">Pre-release access list</Link>
                </li>
                <li>
                  <Link to="#">Contact us</Link>
                </li>
              </ul>
              <h2 className="govuk-heading-s">Past releases</h2>
              <ul className="govuk-list govuk-list--spaced">
                <li>
                  <Details summary="View previous releases (10)">Test</Details>
                </li>
              </ul>
              <h3 className="govuk-heading-s govuk-!-margin-top-3">
                Methodologies
              </h3>
              <ul className="govuk-list govuk-list--spaced">
                <li>
                  <a href="#">
                    Further education and skills statistics: methodology
                  </a>
                </li>
              </ul>
            </RelatedContent>
          </div>
        </div>

        <h2 className="govuk-heading-l govuk-!-margin-top-6">
          Headlines and data downloads
        </h2>
        <Tabs id="test">
          <TabsSection title="Summary">
            <div className={stylesKeyStat.container}>
              <div className={stylesKeyStat.column}>
                <div className={stylesKeyStatTile.tile}>
                  <h3 className="govuk-heading-s">Starts</h3>
                  <p className="govuk-heading-xl">349,190</p>
                  <p className="govuk-body-s">up by 8.6% from 2020/21</p>
                </div>
              </div>
              <div className={stylesKeyStat.column}>
                <div className={stylesKeyStatTile.tile}>
                  <h3 className="govuk-heading-s">Learner Participation</h3>
                  <p className="govuk-heading-xl">740,350</p>
                  <p className="govuk-body-s">up by 3.8% from 2020/21</p>
                </div>
              </div>
              <div className={stylesKeyStat.column}>
                <div className={stylesKeyStatTile.tile}>
                  <h3 className="govuk-heading-s">Achievements</h3>
                  <p className="govuk-heading-xl">137,220</p>
                  <p className="govuk-body-s">down by 12.3% from 2020/21</p>
                </div>
              </div>
            </div>
            <div>
              <h3 className="govuk-heading-s">
                Overall sustained destination rate declined in 2020/21 with
                lower numbers going into apprenticeships and employment.
              </h3>
              <ul>
                <li>
                  Advanced apprenticeships accounted for nearly a half of starts
                  (43.3% or 151,300 starts).
                </li>
                <li>
                  Higher apprenticeships accounted for nearly a third of starts
                  (30.5% or 106,400 starts).
                </li>
                <li>Under 19s accounted for 22.2% of starts (77,500).</li>
                <li>
                  Starts supported by Apprenticeship Service Account (ASA) levy
                  funds accounted for 64.6% (225,600) – please see the 'Further
                  education and skills statistics: methodology' document for
                  more information about ASA levy funds.
                </li>
                <li>
                  Apprenticeship standards made up 99.5% of starts
                  (347,500).&nbsp;
                  <i>
                    Note: There are still a small number of starts on
                    frameworks. All remaining apprenticeship frameworks were
                    withdrawn to new learners on 31 July 2020. Learners who
                    started on frameworks are where it has been agreed a learner
                    can return to a previous framework they have been on after
                    an extensive break.
                  </i>
                </li>
                <li>
                  Since May 2015 there have been 2,881,900 apprenticeship starts
                  and since May 2010 this total stands at 5,259,400.
                </li>
              </ul>
            </div>
          </TabsSection>
          <TabsSection title="Table">This is the table section</TabsSection>
        </Tabs>
        <h2 className="govuk-heading-m" id="exploreData">
          Explore data and files used in this release
        </h2>
        <div
          className={classNames(
            styles.prototypeCardContainerGrid,
            styles.prototypeCardBg,
          )}
        >
          <div className={classNames(styles.prototypeCardChevron)}>
            <h2
              className={classNames(
                'govuk-heading-s',
                'govuk-!-margin-bottom-2',
              )}
            >
              <a
                href="./table-highlights-2?source=publicationPage"
                className={classNames(
                  styles.prototypeCardChevronLink,
                  'govuk-link--no-visited-state',
                )}
              >
                View or create your own tables
              </a>
            </h2>
            <p className="govuk-body govuk-!-margin-bottom-0">
              View featured tables that we have built for you, or create your
              own tables from open data using our table tool
            </p>
          </div>

          <div className={classNames(styles.prototypeCardChevron)}>
            <h2
              className={classNames(
                'govuk-heading-s',
                'govuk-!-margin-bottom-2',
              )}
            >
              <a
                href="/prototypes/data-catalog?theme=fe&publication=traineeships&source=publicationPage"
                className={classNames(
                  styles.prototypeCardChevronLink,
                  'govuk-link--no-visited-state',
                )}
              >
                Data catalogue
              </a>
            </h2>
            <p className="govuk-body govuk-!-margin-bottom-0">
              Browse and download individual open data files from this release
              in our data catalogue
            </p>
          </div>

          <div className={classNames(styles.prototypeCardChevron)}>
            <h2
              className={classNames(
                'govuk-heading-s',
                'govuk-!-margin-bottom-2',
              )}
            >
              <a
                href="#"
                className={classNames(
                  styles.prototypeCardChevronLink,
                  'govuk-link--no-visited-state',
                )}
              >
                Download all data (ZIP)
              </a>
            </h2>
            <p className="govuk-body govuk-!-margin-bottom-0">
              Download all data available in this release as a compressed ZIP
              file
            </p>
          </div>
          <div className={classNames(styles.prototypeCardChevron)}>
            <h2
              className={classNames(
                'govuk-heading-s',
                'govuk-!-margin-bottom-2',
              )}
            >
              <a
                href="https://explore-education-statistics.service.gov.uk/find-statistics/apprenticeships-and-traineeships/data-guidance"
                className={classNames(
                  styles.prototypeCardChevronLink,
                  'govuk-link--no-visited-state',
                )}
              >
                Data guidance
              </a>
            </h2>
            <p className="govuk-body govuk-!-margin-bottom-0">
              Learn more about the data files used in this release using our
              online guidance
            </p>
          </div>
        </div>
        <Accordion id="data">
          <AccordionSection heading="Other supporting files" backToTop={false}>
            <p>
              All supporting files from this release are listed for individual
              download below:
            </p>
            <ul className="govuk-list">
              <li>
                <h3 className="govuk-heading-s govuk-!-margin-bottom-2">
                  <a href="#">
                    Apprenticeship Achievement Rates Standards and Frameworks
                    (csv, 90 Kb)
                  </a>
                </h3>
                <p>
                  Apprenticeship achievement rates for individual standards and
                  frameworks from the 2018/19 to 2020/21 academic years. For
                  information on the standards and frameworks that are redacted
                  please see the ‘Apprenticeship Achievement Rates Standards and
                  Frameworks redactions’ file.
                </p>
                <hr />
              </li>
              <li>
                <h3 className="govuk-heading-s govuk-!-margin-bottom-2">
                  <a href="#">
                    Apprenticeship Achievement Rates Standards and Frameworks
                    redactions (csv, 41 Kb)
                  </a>
                </h3>
                <p>
                  A list of the standards and frameworks for which achievement
                  rates are redacted to preserve provider anonymity. Any
                  framework/standard with less than 30 leavers, or with 10 or
                  less providers, or where one provider accounts for more than
                  60% of all leavers, has been removed.
                </p>
                <hr />
              </li>
              <li>
                <h3 className="govuk-heading-s govuk-!-margin-bottom-2">
                  <a href="#">
                    Apprenticeship Infographic November 2022 (pdf, 1 Mb)
                  </a>
                </h3>
                <p>Apprenticeships infographic outlining trends</p>
                <hr />
              </li>
              <li>
                <h3 className="govuk-heading-s govuk-!-margin-bottom-2">
                  <a href="#">
                    Explanatory Note On Apprenticeship Achievement Rates For
                    Standards (pdf, 1 Mb)
                  </a>
                </h3>
                <p>
                  Describes apprenticeship achievement rates for frameworks and
                  standards for 2018/19 to 2020/21
                </p>
                <hr />
              </li>
              <li>
                <h3 className="govuk-heading-s govuk-!-margin-bottom-2">
                  <a href="#">
                    Metadata for underlying data files (pdf, 230 Kb)
                  </a>
                </h3>
                <p>
                  Explanatory information for using the underlying data files
                  that support the Apprenticeships and traineeships 2021/22
                  statistics publication.
                </p>
                <hr />
              </li>
              <li>
                <h3 className="govuk-heading-s govuk-!-margin-bottom-2">
                  <a href="#">
                    Skills toolkit course registrations and completions as at
                    27-02-2022 (csv, 3 Kb)
                  </a>
                </h3>
                <p>
                  Skills toolkit course registrations and completions as at 27
                  February 2022
                </p>
                <hr />
              </li>
              <li>
                <h3 className="govuk-heading-s govuk-!-margin-bottom-2">
                  <a href="#">
                    Supporting tables - Apprenticeship starts since May 2010 and
                    May 2015 by region, local authority and parliamentary
                    constituency (xlsx, 284 Kb)
                  </a>
                </h3>
                <p>
                  Supporting tables - Apprenticeship starts since May 2010 and
                  May 2015 by region, local authority and parliamentary
                  constituency
                </p>
                <hr />
              </li>
              <li>
                <h3 className="govuk-heading-s govuk-!-margin-bottom-2">
                  <a href="#">
                    Traineeship incentive payments by November 2022 (csv, 6 Kb)
                  </a>
                </h3>
                <p>
                  Traineeship incentive payments made up to and including 10
                  November 2022 by local authority district
                </p>
                <hr />
              </li>
            </ul>
          </AccordionSection>
        </Accordion>

        <h2 className={styles.contentsMobile}>Contents</h2>

        {!showContents && (
          <>
            <h2 id="contentsNoSideNav">Contents</h2>
            <p>
              <a
                href="#"
                onClick={e => {
                  setShowContents(true);
                  e.preventDefault();
                }}
              >
                Show contents in side panel
              </a>
            </p>
          </>
        )}
        <div className={styles.releaseContainer}>
          <div>
            {showContents && (
              <div className={styles.stickyLinksContainer}>
                <div className={classNames(styles.stickyLinks)}>
                  <h2 id="contents">Contents</h2>
                  <ul className="govuk-list govuk-list--spaced">
                    <li>
                      <a
                        className={classNames(
                          'govuk-link--no-visited-state',
                          styles.prototypeLinkNoUnderline,
                        )}
                        href="#about-these-statistics"
                      >
                        About these statistics
                      </a>
                    </li>
                    <li>
                      <a
                        className={classNames(
                          'govuk-link--no-visited-state',
                          styles.prototypeLinkNoUnderline,
                        )}
                        href="#how-to-use-this-release-and-find-data"
                      >
                        How to use this release and find data
                      </a>
                    </li>
                    <li>
                      <a
                        className={classNames(
                          'govuk-link--no-visited-state',
                          styles.prototypeLinkNoUnderline,
                        )}
                        href="#interactive-data-visualisation-tool"
                      >
                        Interactive data visualisation tool
                      </a>
                    </li>
                    <li>
                      <a
                        className={classNames(
                          'govuk-link--no-visited-state',
                          styles.prototypeLinkNoUnderline,
                        )}
                        href="#latest-headline-statistics"
                      >
                        Latest headline statistics
                      </a>
                    </li>
                    <li>
                      <a
                        className={classNames(
                          'govuk-link--no-visited-state',
                          styles.prototypeLinkNoUnderline,
                        )}
                        href="#annual-time-series"
                      >
                        Annual time series
                      </a>
                    </li>
                    <li>
                      <a
                        className={classNames(
                          'govuk-link--no-visited-state',
                          styles.prototypeLinkNoUnderline,
                        )}
                        href="#learner-characteristics"
                      >
                        Learner characteristics
                      </a>
                    </li>
                    <li>
                      <a
                        className={classNames(
                          'govuk-link--no-visited-state',
                          styles.prototypeLinkNoUnderline,
                        )}
                        href="#subjects-and-levels"
                      >
                        Subjects and levels
                      </a>
                    </li>
                    <li>
                      <a
                        className={classNames(
                          'govuk-link--no-visited-state',
                          styles.prototypeLinkNoUnderline,
                        )}
                        href="#geographical-breakdowns"
                      >
                        Geographical breakdowns
                      </a>
                    </li>
                    <li>
                      <a
                        className={classNames(
                          'govuk-link--no-visited-state',
                          styles.prototypeLinkNoUnderline,
                        )}
                        href="#providers"
                      >
                        Providers
                      </a>
                    </li>
                    <li>
                      <a
                        className={classNames(
                          'govuk-link--no-visited-state',
                          styles.prototypeLinkNoUnderline,
                        )}
                        href="#public-sector-apprenticeships"
                      >
                        Public sector apprenticeships
                      </a>
                    </li>
                    <li>
                      <a
                        className={classNames(
                          'govuk-link--no-visited-state',
                          styles.prototypeLinkNoUnderline,
                        )}
                        href="#Traineeships"
                      >
                        Traineeships
                      </a>
                    </li>
                    <li>
                      <a
                        className={classNames(
                          'govuk-link--no-visited-state',
                          styles.prototypeLinkNoUnderline,
                        )}
                        href="#national-achievement-rate-tables"
                      >
                        National achievement rate tables
                      </a>
                    </li>
                    <li>
                      <a
                        className={classNames(
                          'govuk-link--no-visited-state',
                          styles.prototypeLinkNoUnderline,
                        )}
                        href="#apprenticeship-service-and-monthly-transparency-data"
                      >
                        Apprenticeship Service and monthly transparency data
                      </a>
                    </li>
                    <li>
                      <a
                        className={classNames(
                          'govuk-link--no-visited-state',
                          styles.prototypeLinkNoUnderline,
                        )}
                        href="#additional-analysis"
                      >
                        Additional analysis
                      </a>
                    </li>
                    <li>
                      <a
                        className={classNames(
                          'govuk-link--no-visited-state',
                          styles.prototypeLinkNoUnderline,
                        )}
                        href="#future-changes"
                      >
                        Future changes
                      </a>
                    </li>
                  </ul>
                  <hr />
                  <ul className="govuk-list">
                    <li>
                      <a
                        className={classNames(
                          'govuk-link--no-visited-state',
                          styles.prototypeLinkNoUnderline,
                        )}
                        href="#contentsNoSideNav"
                        onClick={_ => {
                          setShowContents(false);
                        }}
                      >
                        Hide contents side panel
                      </a>
                    </li>
                    <li>
                      <a
                        className={classNames(
                          'govuk-link--no-visited-state',
                          styles.prototypeLinkNoUnderline,
                        )}
                        href="#"
                      >
                        Back to top
                      </a>
                    </li>
                  </ul>
                </div>
              </div>
            )}
          </div>
          <div className={styles.releaseMainContent}>
            <Accordion id="content">
              <AccordionSection
                open
                heading="About these statistics"
                backToTop={false}
              >
                <div className="dfe-content">
                  <p>
                    This statistical release presents provisional information on
                    all age (16+) apprenticeships starts, achievements and
                    participation in England for the first quarter of the
                    2022/23 academic year.
                  </p>
                  <p>Also published are official statistics covering:</p>
                  <ul>
                    <li>
                      Apprenticeship service commitments, registrations, and
                      reservations
                    </li>
                    <li>
                      Employers reporting the withdrawal of apprentices due to
                      redundancy
                    </li>
                    <li>
                      Adverts and vacancies as reported on the Find an
                      apprenticeship website
                    </li>
                  </ul>
                  <p>
                    A separate release covers overall further education and
                    skills data, please see ‘
                    <a href="https://explore-education-statistics.service.gov.uk/find-statistics/further-education-and-skills">
                      Further education and skills
                    </a>
                    ’. Please note that the FE and skills release includes the
                    adult apprenticeships and traineeships published here in its
                    headline figures.
                  </p>
                  <h4>
                    <strong>
                      Individualised Learner Record (ILR) administrative data
                    </strong>
                  </h4>
                  <p>
                    The apprenticeship data in this release are based on the
                    fourth ILR data return from FE and apprenticeship providers
                    for the 2022/23 academic year, which was taken in December
                    2022. The ILR is an administrative data collection system
                    designed primarily for operational use in order to fund
                    training providers for learners in FE and on apprenticeship
                    programmes.
                  </p>
                  <h4>
                    <strong>National achievement rate tables data</strong>
                  </h4>
                  <p>
                    Figures in the ‘national achievement rate tables’ section
                    are as published in March 2022.&nbsp;These official
                    statistics cover achievement rates for apprenticeships in
                    the 2020/21 academic year and would have been previously
                    released as part of the standalone National achievement rate
                    tables publication. As confirmed in our&nbsp;
                    <a href="https://www.gov.uk/government/publications/coronavirus-covid-19-school-and-college-performance-measures">
                      guidance
                    </a>
                    , due to Coronavirus (COVID-19), we have not published
                    institution-level qualification achievement rates (QARs) in
                    the national achievement rate tables for 2019/20 or 2020/21
                    academic years. We have published high level summaries of
                    QARs for statistical purposes.&nbsp;
                  </p>
                  <p>
                    Achievement rates covering the 2021/22 academic year are
                    planned to be published as part of our March 2023 statistics
                    update.
                  </p>
                  <h4>
                    <strong>
                      Provider reporting during the COVID-19 pandemic
                    </strong>
                  </h4>
                  <p>
                    Historic data in this publication covers periods affected by
                    varying COVID-19 restrictions,&nbsp;which will have impacted
                    on apprenticeship and traineeship learning.&nbsp;Therefore,
                    extra care should be taken in comparing and interpreting
                    data presented in this release.
                  </p>
                  <p>
                    The furlough scheme may also have impacted on how aspects of
                    ILR data were recorded, such as how the ‘learning status’ of
                    a learner was captured, e.g. whether a learner was recorded
                    as a continuing learner or whether they were recorded as
                    being on a break in learning while still being with an
                    employer.
                  </p>
                </div>
              </AccordionSection>
              <AccordionSection
                heading="How to use this release and find data"
                backToTop={false}
              >
                <div className="dfe-content">
                  <p>
                    The Apprenticeships and traineeships publication still
                    provides the same range of data it always did, but has
                    undergone some structural changes since the previous
                    publication in order to improve user’s experience.&nbsp;
                  </p>
                  <p>
                    We have also adopted a new naming convention for files to
                    help users find their data of interest. We have not changed
                    the content of these files except in a few cases where we
                    have merged some smaller files. You can find a look-up of
                    the old and new file names in the file called
                    <strong> “New Release Layout - Names Lookup” </strong>that
                    can be found by clicking '
                    <a href="https://explore-education-statistics.service.gov.uk/find-statistics/apprenticeships-and-traineeships#explore-data-and-files">
                      Explore data and files
                    </a>
                    ' and opening the ‘all supporting files’ section.
                  </p>
                  <p>
                    <strong>
                      This section serves to signpost users to the data most
                      relevant to their uses by detailing the routes through
                      which it can be accessed.&nbsp;
                    </strong>
                  </p>
                  <p>
                    <strong>
                      The content of the publication below contains charts and
                      tables which highlight key figures
                    </strong>{' '}
                    and trends that give an overview of the national picture of
                    the apprenticeship and traineeship landscape.
                  </p>
                  <p>
                    <strong>
                      'Featured tables' provide further detail with figures
                      broken down by common areas of interest.{' '}
                    </strong>
                    These can be found by expanding the '
                    <a href="https://explore-education-statistics.service.gov.uk/find-statistics/apprenticeships-and-traineeships#explore-data-and-files">
                      Explore data and files
                    </a>
                    ' accordion and clicking '
                    <a href="https://explore-education-statistics.service.gov.uk/data-tables/apprenticeships-and-traineeships">
                      Create tables
                    </a>
                    '. These tables are created to provide the next level of
                    detail one might wish to find below the level of detail
                    provided by tables embedded within the release.&nbsp; They
                    also provide the user the opportunity to then amend content,
                    reorder and take away to meet their needs.&nbsp; Within the
                    release we list out the most relevant featured tables at the
                    end of each commentary section.
                  </p>
                  <p>
                    <strong>
                      In addition to featured tables you can also access
                      underlying data files&nbsp;
                    </strong>
                    and build your own tables using the table builder tool. For
                    example, the featured table showing enrolments by provider
                    is produced from an underlying data file which also contains
                    detail on the level of an aim, and it's sector subject area.
                  </p>
                  <p>
                    The list of files available can be accessed by expanding the
                    '
                    <a href="https://explore-education-statistics.service.gov.uk/find-statistics/apprenticeships-and-traineeships#explore-data-and-files">
                      Explore data and files
                    </a>
                    ' accordion and clicking either ‘browse data files’ or '
                    <a href="https://explore-education-statistics.service.gov.uk/data-tables/apprenticeships-and-traineeships">
                      Create tables
                    </a>
                    ', then switching to the&nbsp;'Create your own table' tab
                    and selecting your file of interest.
                  </p>
                  <p>
                    Alternatively you can modify and existing featured table by
                    selecting it and then depending on the breakdowns available,
                    edit the location, time periods, indicators and/or filters
                    (Steps 3, 4 and 5).
                  </p>
                  <p>
                    <strong>There is a dashboard&nbsp;</strong>that provides
                    interactive presentation of our published data, with a
                    number of different views on to data and ‘drilldown’
                    capability to allow users to investigate different types of
                    FE provision.&nbsp; It is particularly helpful in viewing
                    data across different geographical areas and providers.
                  </p>
                  <p>
                    <strong>
                      This release also provides ‘all supporting files’&nbsp;
                    </strong>
                    which can be found at the end of the '
                    <a href="https://explore-education-statistics.service.gov.uk/find-statistics/apprenticeships-and-traineeships#explore-data-and-files">
                      Explore data and files
                    </a>
                    ' accordion.<strong> </strong>These are mainly csv files
                    which can be downloaded, and provide some additional
                    breakdowns including unrounded data.&nbsp; They are provided
                    for transparency to enable analysts to re-use the data in
                    this release. A metadata document is available in the same
                    location which explains the content of these supporting
                    files.
                  </p>
                  <p>
                    All of the data available in this release can be downloaded
                    using the 'Download all data (zip)' button at the top right
                    of this page.
                  </p>
                  <p>
                    <strong>Feedback</strong>
                  </p>
                  <p>
                    This release is a structural change to how we publish our
                    data and statistics, which we continually look to improve.
                    As a result, your feedback is important to help us further
                    improve and develop. To provide feedback on this release,
                    please email&nbsp;us at&nbsp;
                    <a href="mailto:FE.OFFICIALSTATISTICS@education.gov.uk">
                      FE.OFFICIALSTATISTICS@education.gov.uk
                    </a>
                    .&nbsp;
                  </p>
                </div>
              </AccordionSection>
              <AccordionSection
                heading="Interactive data visualisation tool"
                backToTop={false}
              >
                This is a test
              </AccordionSection>
              <AccordionSection
                heading="Latest headline statistics"
                backToTop={false}
              >
                This is a test
              </AccordionSection>
              <AccordionSection heading="Annual time series" backToTop={false}>
                This is a test
              </AccordionSection>
              <AccordionSection
                heading="Learner characteristics"
                backToTop={false}
              >
                This is a test
              </AccordionSection>
              <AccordionSection heading="Subjects and levels" backToTop={false}>
                This is a test
              </AccordionSection>
              <AccordionSection
                heading="Geographical breakdowns"
                backToTop={false}
              >
                This is a test
              </AccordionSection>
              <AccordionSection heading="Providers" backToTop={false}>
                This is a test
              </AccordionSection>
              <AccordionSection
                heading="Public sector apprenticeships"
                backToTop={false}
              >
                This is a test
              </AccordionSection>
              <AccordionSection heading="Traineeships" backToTop={false}>
                This is a test
              </AccordionSection>
              <AccordionSection
                heading="National achievement rate tables"
                backToTop={false}
              >
                This is a test
              </AccordionSection>
              <AccordionSection
                heading="Apprenticeship Service and monthly transparency data"
                backToTop={false}
              >
                This is a test
              </AccordionSection>
              <AccordionSection heading="Additional analysis" backToTop={false}>
                This is a test
              </AccordionSection>
              <AccordionSection heading="Future changes" backToTop={false}>
                This is a test
              </AccordionSection>
            </Accordion>
          </div>
        </div>
        <h2
          className="govuk-heading-l govuk-!-margin-top-9"
          id="extra-information"
          data-testid="extra-information"
        >
          Help and support
        </h2>
        <Accordion id="downloads">
          <AccordionSection
            heading="Download associated files"
            backToTop={false}
          >
            <PrototypeDownloadPopular />
            <PrototypeDownloadUnderlying />
            <PrototypeDownloadAncillary />
            <PrototypeTableBuilder />
          </AccordionSection>
          <AccordionSection
            heading="Download associated files (Example 2)"
            backToTop={false}
          >
            <PrototypeDownloadPopular viewAsList />
            <PrototypeDownloadUnderlying viewAsList />
            <PrototypeDownloadAncillary />
            <PrototypeTableBuilder />
          </AccordionSection>
          <AccordionSection
            heading="Download associated files (Example 3)"
            backToTop={false}
          >
            <Tabs id="download">
              <TabsSection title="Featured tables">
                <PrototypeDownloadPopular viewAsList />
              </TabsSection>
              <TabsSection title="Underlying data">
                <PrototypeDownloadUnderlying viewAsList />
              </TabsSection>
              <TabsSection title="Ancillary files">
                <PrototypeDownloadAncillary viewAsList />
              </TabsSection>
            </Tabs>
            <h3 className="govuk-heading-m">Create your own tables</h3>
            <p>
              Use our tool to build tables using our range of national and
              regional data
            </p>
            <a href="#" className="govuk-button">
              Create table
            </a>
          </AccordionSection>
          <AccordionSection heading="Methodology" backToTop={false}>
            Methodology section
          </AccordionSection>
          <AccordionSection heading="National statistics" backToTop={false}>
            National statistics
          </AccordionSection>
          <AccordionSection heading="Contact us" backToTop={false}>
            Contact us
          </AccordionSection>
        </Accordion>
      </PrototypePage>
    </div>
  );
};

export default PrototypeReleaseData;
