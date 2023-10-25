import classNames from 'classnames';
import PrototypePage from '@admin/prototypes/components/PrototypePage';
import PrototypeDashboardContent from '@admin/prototypes/components/PrototypeDashboardContent';
import PrototypeChartExamples from '@admin/prototypes/components/PrototypeChartExamples';
import Details from '@common/components/Details';
import PageSearchForm from '@common/components/PageSearchForm';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import stylesKeyStat from '@common/modules/find-statistics/components/KeyStat.module.scss';
import stylesKeyStatTile from '@common/modules/find-statistics/components/KeyStatTile.module.scss';
import React, { useState } from 'react';
import styles from './PrototypePublicPage.module.scss';

const PrototypeReleaseData = () => {
  // const [showContents, setShowContents] = useState(true);
  const [navSelected, setNavSelected] = useState('none');

  return (
    <div>
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
        wide={false}
      >
        <span className="govuk-caption-xl" data-testid="page-title-caption">
          Academic Year 2021/22
        </span>
        <h1 className="govuk-heading-xl" data-testid="page-title">
          Apprenticeships and traineeships
        </h1>
        <span className="govuk-tag govuk-!-margin-bottom-3">Latest data</span>
        <div className="dfe-flex">
          <div>
            <div className="govuk-body-l">
              Apprenticeship and traineeship starts, achievements and
              participation. Includes breakdowns by age, sex, ethnicity,
              subject, provider, geography etc.
            </div>
            <a href="#" className="govuk-button">
              Download all data from this release (zip)
            </a>
          </div>

          <img
            src="/assets/images/UKSA-quality-mark.jpg"
            className="govuk-!-margin-right-3"
            alt="UK statistics authority quality mark"
            height="60"
            width="60"
          />
        </div>
        <hr className="govuk-!-margin-bottom-6" />

        <div className={styles.releaseContainer}>
          <div>
            <div className={styles.stickyLinksContainer}>
              <h2 id="contentsNoSideNav" className="govuk-body">
                Contents
              </h2>
              <div
                className={classNames(styles.stickyLinks)}
                style={{ border: 'none' }}
              >
                <div
                  className="dfe-flex dfe-justify-content--space-between"
                  style={{
                    flexDirection: 'column',
                  }}
                >
                  <ul
                    className={classNames(
                      'govuk-list',
                      'govuk-list--spaced',
                      styles.prototypeInPageNavSpacing,
                    )}
                  >
                    <li>
                      <a
                        className={classNames(
                          'govuk-link--no-visited-state',
                          styles.prototypeLinkNoUnderline,
                          'govuk-body-s',
                          {
                            'govuk-!-font-weight-bold':
                              navSelected === 'summary',
                          },
                        )}
                        href="#summary"
                        onClick={() => {
                          setNavSelected('summary');
                        }}
                      >
                        Release details
                      </a>
                    </li>
                    <li>
                      <a
                        className={classNames(
                          'govuk-link--no-visited-state',
                          styles.prototypeLinkNoUnderline,
                          'govuk-body-s',
                          {
                            'govuk-!-font-weight-bold':
                              navSelected === 'headlines',
                          },
                        )}
                        href="#headlines"
                        onClick={() => {
                          setNavSelected('headlines');
                        }}
                      >
                        Headline facts and figures test
                      </a>
                    </li>
                    <li>
                      <a
                        className={classNames(
                          'govuk-link--no-visited-state',
                          styles.prototypeLinkNoUnderline,
                          'govuk-body-s',
                          {
                            'govuk-!-font-weight-bold':
                              navSelected === 'exploreData',
                          },
                        )}
                        href="#exploreData"
                        onClick={() => {
                          setNavSelected('exploreData');
                        }}
                      >
                        Explore data used in this release
                      </a>
                    </li>
                    <li>
                      <a
                        className={classNames(
                          'govuk-link--no-visited-state',
                          styles.prototypeLinkNoUnderline,
                          'govuk-body-s',
                          {
                            'govuk-!-font-weight-bold': navSelected === 'about',
                          },
                        )}
                        href="#about"
                        onClick={() => {
                          setNavSelected('about');
                        }}
                      >
                        About these statistics
                      </a>
                    </li>
                    <li>
                      <a
                        className={classNames(
                          'govuk-link--no-visited-state',
                          styles.prototypeLinkNoUnderline,
                          'govuk-body-s',
                          {
                            'govuk-!-font-weight-bold':
                              navSelected === 'supplement',
                          },
                        )}
                        href="#supplement"
                        onClick={() => {
                          setNavSelected('supplement');
                        }}
                      >
                        How to find data and supplementary tables in this
                        release
                      </a>
                    </li>
                    <li>
                      <a
                        className={classNames(
                          'govuk-link--no-visited-state',
                          styles.prototypeLinkNoUnderline,
                          'govuk-body-s',
                        )}
                        href="#fullYearApprenticeships"
                      >
                        Full year apprenticeships data
                      </a>
                    </li>
                    <li>
                      <a
                        className={classNames(
                          'govuk-link--no-visited-state',
                          styles.prototypeLinkNoUnderline,
                          'govuk-body-s',
                        )}
                        href="#fullYearTraineeships"
                      >
                        Full year traineeships data
                      </a>
                    </li>
                    <li>
                      <a
                        className={classNames(
                          'govuk-link--no-visited-state',
                          styles.prototypeLinkNoUnderline,
                          'govuk-body-s',
                        )}
                        href="#latestApprenticeships"
                      >
                        Latest Apprenticeships in year data
                      </a>
                    </li>
                    <li>
                      <a
                        className={classNames(
                          'govuk-link--no-visited-state',
                          styles.prototypeLinkNoUnderline,
                          'govuk-body-s',
                        )}
                        href="#latestTraineeships"
                      >
                        Latest Traineeships in year data
                      </a>
                    </li>
                    <li>
                      <a
                        className={classNames(
                          'govuk-link--no-visited-state',
                          styles.prototypeLinkNoUnderline,
                          'govuk-body-s',
                        )}
                        href="#visualisationTool"
                      >
                        Interactive data visualisation tool
                      </a>
                    </li>
                    <li>
                      <a
                        className={classNames(
                          'govuk-link--no-visited-state',
                          styles.prototypeLinkNoUnderline,
                          'govuk-body-s',
                        )}
                        href="#pubSectorApprenrticeships"
                      >
                        Public sector apprenticeships 2022-23
                      </a>
                    </li>
                    <li>
                      <a
                        className={classNames(
                          'govuk-link--no-visited-state',
                          styles.prototypeLinkNoUnderline,
                          'govuk-body-s',
                        )}
                        href="#additionalData"
                      >
                        Additional analysis and transparency data
                      </a>
                    </li>
                    <li>
                      <a
                        className={classNames(
                          'govuk-link--no-visited-state',
                          styles.prototypeLinkNoUnderline,
                          'govuk-body-s',
                        )}
                        href="#help"
                      >
                        Help and support
                      </a>
                    </li>
                  </ul>
                  <hr />
                  <div className="govuk-!-margin-bottom-9 govuk-body-s">
                    <a href="#">Back to top</a>
                  </div>
                </div>
              </div>
            </div>
          </div>
          <div className={styles.releaseMainContent}>
            <div className="govuk-grid-row">
              <div className="govuk-grid-column-full">
                <h2 className="govuk-heading-l" id="summary">
                  Release details
                </h2>
                <dl className="govuk-summary-list govuk-summary-list--no-border">
                  <div className="govuk-summary-list__row">
                    <dt className="govuk-summary-list__key">Published</dt>
                    <dd className="govuk-summary-list__value">
                      <time>27 January 2022 </time>
                    </dd>
                  </div>
                  <div className="govuk-summary-list__row">
                    <dt className="govuk-summary-list__key">Release type</dt>
                    <dd className="govuk-summary-list__value">
                      <a href="#exploreData">National data</a>
                    </dd>
                  </div>
                  <div className="govuk-summary-list__row">
                    <dt className="govuk-summary-list__key">Methodologies</dt>
                    <dd className="govuk-summary-list__value">
                      <a href="/methodology/further-education-and-skills-statistics-methodology">
                        Further education and skills statistics: methodology
                      </a>
                    </dd>
                  </div>
                  <div className="govuk-summary-list__row">
                    <dt className="govuk-summary-list__key">
                      Related information
                    </dt>
                    <dd className="govuk-summary-list__value">
                      <ul className="govuk-list">
                        <li>
                          <a href="#">Data guidance</a>
                        </li>
                        <li>
                          <a href="#">Pre-release access list</a>
                        </li>
                      </ul>
                    </dd>
                  </div>
                  <div className="govuk-summary-list__row">
                    <dt className="govuk-summary-list__key">Last updated </dt>
                    <dd className="govuk-summary-list__value">
                      <Details
                        className="govuk-!-margin-0"
                        summary="22 December 2022 (18 updates)"
                      >
                        <ol className="govuk-list">
                          <li>
                            <time className="govuk-body govuk-!-font-weight-bold">
                              18 December 2020
                            </time>
                            <p>
                              Local authority level data on placement stability
                              and local authority level data for care leavers
                              have been added.
                            </p>
                          </li>
                          <li>
                            <time className="govuk-body govuk-!-font-weight-bold">
                              11 December 2020
                            </time>
                            <p>
                              Correction to metadata for LA level 'children
                              ceasing to be looked after' file
                            </p>
                          </li>
                        </ol>
                      </Details>
                    </dd>
                  </div>
                  <div className="govuk-summary-list__row">
                    <dt className="govuk-summary-list__key">
                      Releases in this series{' '}
                    </dt>
                    <dd className="govuk-summary-list__value">
                      <Details
                        className="govuk-!-margin-0"
                        summary="View releases (8)"
                      >
                        <ol className="govuk-list">
                          <li>Test</li>
                        </ol>
                      </Details>
                    </dd>
                  </div>
                  <div className="govuk-summary-list__row">
                    <dt className="govuk-summary-list__key">
                      Receive updates{' '}
                    </dt>
                    <dd className="govuk-summary-list__value">
                      <a
                        data-testid="subscription-children-looked-after-in-england-including-adoptions"
                        href="/subscriptions?slug=children-looked-after-in-england-including-adoptions"
                        className="govuk-link govuk-link--no-visited-state dfe-print-hidden"
                      >
                        <strong>Sign up for email alerts</strong>
                      </a>
                    </dd>
                  </div>
                </dl>{' '}
                <hr />
                <div className="dfe-content">
                  <h4>
                    <strong>October 2023 release</strong>
                  </h4>
                  <p>
                    This transparency update adds additional data on monthly
                    apprenticeship starts in the ‘Latest Apprenticeships in year
                    data’ section to cover the period August 2022 to July 2023
                    (based on data returned by providers in September
                    2023).&nbsp;
                  </p>
                </div>
              </div>
            </div>
            <h2 className="govuk-heading-l govuk-!-margin-top-6" id="headlines">
              Headline facts and figures
            </h2>
            <Tabs id="test">
              <TabsSection title="Summary">
                <div className={stylesKeyStat.container}>
                  <div className={stylesKeyStat.wrapper}>
                    <div className={stylesKeyStatTile.tile}>
                      <h3 className="govuk-heading-s">Starts</h3>
                      <p className="govuk-heading-l">9,999,999</p>
                      <p className="govuk-body-s">up by 8.6% from 2020/21</p>
                    </div>
                  </div>
                  <div className={stylesKeyStat.wrapper}>
                    <div className={stylesKeyStatTile.tile}>
                      <h3 className="govuk-heading-s">Learner Participation</h3>
                      <p className="govuk-heading-l">740,350</p>
                      <p className="govuk-body-s">up by 3.8% from 2020/21</p>
                    </div>
                  </div>
                  <div className={stylesKeyStat.wrapper}>
                    <div className={stylesKeyStatTile.tile}>
                      <h3 className="govuk-heading-s">Achievements</h3>
                      <p className="govuk-heading-l">137,220</p>
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
                      Advanced apprenticeships accounted for nearly a half of
                      starts (43.3% or 151,300 starts).
                    </li>
                    <li>
                      Higher apprenticeships accounted for nearly a third of
                      starts (30.5% or 106,400 starts).
                    </li>
                    <li>Under 19s accounted for 22.2% of starts (77,500).</li>
                    <li>
                      Starts supported by Apprenticeship Service Account (ASA)
                      levy funds accounted for 64.6% (225,600) – please see the
                      'Further education and skills statistics: methodology'
                      document for more information about ASA levy funds.
                    </li>
                    <li>
                      Apprenticeship standards made up 99.5% of starts
                      (347,500).&nbsp;
                      <i>
                        Note: There are still a small number of starts on
                        frameworks. All remaining apprenticeship frameworks were
                        withdrawn to new learners on 31 July 2020. Learners who
                        started on frameworks are where it has been agreed a
                        learner can return to a previous framework they have
                        been on after an extensive break.
                      </i>
                    </li>
                    <li>
                      Since May 2015 there have been 2,881,900 apprenticeship
                      starts and since May 2010 this total stands at 5,259,400.
                    </li>
                  </ul>
                </div>
              </TabsSection>
              <TabsSection title="Table">This is the table section</TabsSection>
            </Tabs>
            <h2 className="govuk-heading-l" id="exploreData">
              Explore data and files used in this release
            </h2>
            <div
              className={classNames(
                styles.prototypeCardContainerGrid,
                styles.prototypeCardBg,
                'govuk-!-margin-bottom-9',
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
                  View featured tables that we have built for you, or create
                  your own tables from open data using our table tool
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
                  Browse and download individual open data files from this
                  release in our data catalogue
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
                  Download all data available in this release as a compressed
                  ZIP file
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
            <h2 id="about">About these statistics</h2>
            <div className="dfe-content">
              <p>
                This statistical release presents provisional information on all
                age (16+) apprenticeships starts, achievements and participation
                in England for the first quarter of the 2022/23 academic year.
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
                A separate release covers overall further education and skills
                data, please see ‘
                <a href="https://explore-education-statistics.service.gov.uk/find-statistics/further-education-and-skills">
                  Further education and skills
                </a>
                ’. Please note that the FE and skills release includes the adult
                apprenticeships and traineeships published here in its headline
                figures.
              </p>
              <h4>
                <strong>
                  Individualised Learner Record (ILR) administrative data
                </strong>
              </h4>
              <p>
                The apprenticeship data in this release are based on the fourth
                ILR data return from FE and apprenticeship providers for the
                2022/23 academic year, which was taken in December 2022. The ILR
                is an administrative data collection system designed primarily
                for operational use in order to fund training providers for
                learners in FE and on apprenticeship programmes.
              </p>
              <h4>
                <strong>National achievement rate tables data</strong>
              </h4>
              <p>
                Figures in the ‘national achievement rate tables’ section are as
                published in March 2022.&nbsp;These official statistics cover
                achievement rates for apprenticeships in the 2020/21 academic
                year and would have been previously released as part of the
                standalone National achievement rate tables publication. As
                confirmed in our&nbsp;
                <a href="https://www.gov.uk/government/publications/coronavirus-covid-19-school-and-college-performance-measures">
                  guidance
                </a>
                , due to Coronavirus (COVID-19), we have not published
                institution-level qualification achievement rates (QARs) in the
                national achievement rate tables for 2019/20 or 2020/21 academic
                years. We have published high level summaries of QARs for
                statistical purposes.&nbsp;
              </p>
              <p>
                Achievement rates covering the 2021/22 academic year are planned
                to be published as part of our March 2023 statistics update.
              </p>
              <h4>
                <strong>Provider reporting during the COVID-19 pandemic</strong>
              </h4>
              <p>
                Historic data in this publication covers periods affected by
                varying COVID-19 restrictions,&nbsp;which will have impacted on
                apprenticeship and traineeship learning.&nbsp;Therefore, extra
                care should be taken in comparing and interpreting data
                presented in this release.
              </p>
              <p>
                The furlough scheme may also have impacted on how aspects of ILR
                data were recorded, such as how the ‘learning status’ of a
                learner was captured, e.g. whether a learner was recorded as a
                continuing learner or whether they were recorded as being on a
                break in learning while still being with an employer.
              </p>
            </div>
            <h2 id="supplement" className="govuk-!-margin-top-9">
              How to find data and supplementary tables in this release
            </h2>
            <div className="dfe-content">
              <p>
                The Apprenticeships and traineeships publication still provides
                the same range of data it always did, but has undergone some
                structural changes since the previous publication in order to
                improve user’s experience.&nbsp;
              </p>
              <p>
                We have also adopted a new naming convention for files to help
                users find their data of interest. We have not changed the
                content of these files except in a few cases where we have
                merged some smaller files. You can find a look-up of the old and
                new file names in the file called
                <strong> “New Release Layout - Names Lookup” </strong>that can
                be found by clicking '
                <a href="https://explore-education-statistics.service.gov.uk/find-statistics/apprenticeships-and-traineeships#explore-data-and-files">
                  Explore data and files
                </a>
                ' and opening the ‘all supporting files’ section.
              </p>
              <p>
                <strong>
                  This section serves to signpost users to the data most
                  relevant to their uses by detailing the routes through which
                  it can be accessed.&nbsp;
                </strong>
              </p>
              <p>
                <strong>
                  The content of the publication below contains charts and
                  tables which highlight key figures
                </strong>{' '}
                and trends that give an overview of the national picture of the
                apprenticeship and traineeship landscape.
              </p>
              <p>
                <strong>
                  'Featured tables' provide further detail with figures broken
                  down by common areas of interest.{' '}
                </strong>
                These can be found by expanding the '
                <a href="https://explore-education-statistics.service.gov.uk/find-statistics/apprenticeships-and-traineeships#explore-data-and-files">
                  Explore data and files
                </a>
                ' accordion and clicking '
                <a href="https://explore-education-statistics.service.gov.uk/data-tables/apprenticeships-and-traineeships">
                  Create tables
                </a>
                '. These tables are created to provide the next level of detail
                one might wish to find below the level of detail provided by
                tables embedded within the release.&nbsp; They also provide the
                user the opportunity to then amend content, reorder and take
                away to meet their needs.&nbsp; Within the release we list out
                the most relevant featured tables at the end of each commentary
                section.
              </p>
              <p>
                <strong>
                  In addition to featured tables you can also access underlying
                  data files&nbsp;
                </strong>
                and build your own tables using the table builder tool. For
                example, the featured table showing enrolments by provider is
                produced from an underlying data file which also contains detail
                on the level of an aim, and it's sector subject area.
              </p>
              <p>
                The list of files available can be accessed by expanding the '
                <a href="https://explore-education-statistics.service.gov.uk/find-statistics/apprenticeships-and-traineeships#explore-data-and-files">
                  Explore data and files
                </a>
                ' accordion and clicking either ‘browse data files’ or '
                <a href="https://explore-education-statistics.service.gov.uk/data-tables/apprenticeships-and-traineeships">
                  Create tables
                </a>
                ', then switching to the&nbsp;'Create your own table' tab and
                selecting your file of interest.
              </p>
              <p>
                Alternatively you can modify and existing featured table by
                selecting it and then depending on the breakdowns available,
                edit the location, time periods, indicators and/or filters
                (Steps 3, 4 and 5).
              </p>
              <p>
                <strong>There is a dashboard&nbsp;</strong>that provides
                interactive presentation of our published data, with a number of
                different views on to data and ‘drilldown’ capability to allow
                users to investigate different types of FE provision.&nbsp; It
                is particularly helpful in viewing data across different
                geographical areas and providers.
              </p>
              <p>
                <strong>
                  This release also provides ‘all supporting files’&nbsp;
                </strong>
                which can be found at the end of the '
                <a href="https://explore-education-statistics.service.gov.uk/find-statistics/apprenticeships-and-traineeships#explore-data-and-files">
                  Explore data and files
                </a>
                ' accordion.<strong> </strong>These are mainly csv files which
                can be downloaded, and provide some additional breakdowns
                including unrounded data.&nbsp; They are provided for
                transparency to enable analysts to re-use the data in this
                release. A metadata document is available in the same location
                which explains the content of these supporting files.
              </p>
              <p>
                All of the data available in this release can be downloaded
                using the 'Download all data (zip)' button at the top right of
                this page.
              </p>
              <p>
                <strong>Feedback</strong>
              </p>
              <p>
                This release is a structural change to how we publish our data
                and statistics, which we continually look to improve. As a
                result, your feedback is important to help us further improve
                and develop. To provide feedback on this release, please
                email&nbsp;us at&nbsp;
                <a href="mailto:FE.OFFICIALSTATISTICS@education.gov.uk">
                  FE.OFFICIALSTATISTICS@education.gov.uk
                </a>
                .&nbsp;
              </p>
            </div>
            <h2 id="fullYearApprenticeships" className="govuk-!-margin-top-9">
              Full year apprenticeship landscape
            </h2>
            <div className="dfe-content">
              <blockquote>
                <p>
                  The figures in this section relate to full-year final data up
                  to and including the 2021/22 academic year&nbsp;and were
                  originally published in November 2022
                </p>
              </blockquote>
            </div>
            <div className="dfe-content">
              <h3>The changing apprenticeship landscape</h3>
              <p>
                Reform of the apprenticeships programme, along with the impact
                of the COVID-19 pandemic have influenced the trends presented in
                this section. Three main factors are set out in the graphic
                below.
              </p>

              <PrototypeChartExamples chartType="barChart" />
              <PrototypeChartExamples chartType="map" />
              <PrototypeChartExamples chartType="lineChart" />
            </div>
            <PrototypeDashboardContent />
            <h2 className="govuk-heading-l" id="help">
              Help and support
            </h2>
            <h3 className="govuk-heading-m">Methodology</h3>
            <p>
              Find out how and why we collect, process and publish these
              statistics.
            </p>
            <p>
              <a
                href="/methodology/further-education-and-skills-statistics-methodology"
                className="govuk-link"
              >
                Further education and skills statistics: methodology
              </a>
            </p>
          </div>
        </div>
      </PrototypePage>
    </div>
  );
};

export default PrototypeReleaseData;
