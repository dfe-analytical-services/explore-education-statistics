import classNames from 'classnames';
import PrototypePage from '@admin/prototypes/components/PrototypePage';
import PrototypeDashboardContent from '@admin/prototypes/components/PrototypeDashboardContent';
import PrototypeChartExamples from '@admin/prototypes/components/PrototypeChartExamples';
import Details from '@common/components/Details';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import stylesKeyStat from '@common/modules/find-statistics/components/KeyStat.module.scss';
import stylesKeyStatTile from '@common/modules/find-statistics/components/KeyStatTile.module.scss';
import React, { useState } from 'react';
import styles from './PrototypePublicPage.module.scss';

const PrototypeReleaseData = () => {
  const [showContents, setShowContents] = useState(true);
  const [navSelected, setNavSelected] = useState('none');

  return (
    <div className={classNames(styles.prototypePublicPage1)}>
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

        <div>
          {!showContents && (
            <div className={styles.stickyNavToggle}>
              <a
                href="#"
                onClick={e => {
                  e.preventDefault();
                  setShowContents(true);
                }}
              >
                Contents
              </a>
            </div>
          )}

          <div
            className={classNames({ [styles.releaseContainer]: showContents })}
          >
            {showContents && (
              <div style={{ flex: '1 0 25%' }}>
                <div className={styles.stickyLinksContainer}>
                  <h2 id="contentsNoSideNav" className="govuk-body">
                    Contents
                    <span className="govuk-body-s govuk-!-margin-left-2">
                      <a
                        href="#"
                        onClick={e => {
                          e.preventDefault();
                          setShowContents(!showContents);
                        }}
                      >
                        [{showContents ? 'Hide' : 'Show'}]
                      </a>
                    </span>
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
                                'govuk-!-font-weight-bold':
                                  navSelected === 'about',
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
            )}

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
                                Local authority level data on placement
                                stability and local authority level data for
                                care leavers have been added.
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
                    <h4>October 2023 release</h4>
                    <p>
                      This transparency update adds additional data on monthly
                      apprenticeship starts in the ‘Latest Apprenticeships in
                      year data’ section to cover the period August 2022 to July
                      2023 (based on data returned by providers in September
                      2023).
                    </p>
                    <h4>July 2023 release</h4>
                    <p>
                      This release shows provisional in-year data for
                      apprenticeships and traineeships in England reported for
                      the academic year 2022/23 to date (August 2022 to April
                      2023) based on data returned by providers in June 2023.
                      This also includes apprenticeship service data (as of 09
                      June 2023) and Find an apprenticeship data (to June 2023).
                    </p>
                    <h4>Changes to the structure of the release</h4>
                    <p>
                      In January we changed the structure of the release to
                      improve user access to content and to allow for easier
                      maintenance
                      <strong>
                        . The same amount of data is still being published on a
                        quarterly basis.
                      </strong>
                      If you wish to provide feedback on these changes please
                      contact us at{' '}
                      <a href="mailto:FE.OFFICIALSTATISTICS@education.gov.uk">
                        FE.OFFICIALSTATISTICS@education.gov.uk
                      </a>
                      .
                    </p>
                    <p>
                      As announced in November we are changing the content of
                      the monthly updates in between the quarterly updates.
                      Specifically, this includes February, April, May, June,
                      August, September and October.
                    </p>
                    <p>
                      We will continue to update the two existing apprenticeship
                      starts files along with some narrative within the{' '}
                      <a href="https://explore-education-statistics.service.gov.uk/find-statistics/apprenticeships-and-traineeships/2022-23#content-5-heading">
                        latest apprenticeships in year data
                      </a>{' '}
                      accordion.
                    </p>
                    <p>
                      All other data previously published monthly, such as that
                      covering the apprenticeship service and find an
                      apprenticeship, will be updated in the quarterly releases
                      (January, March, July, and November).
                    </p>
                    <h4>
                      <strong>
                        Impact of COVID-19 on reporting of FE and apprenticeship
                        data
                      </strong>
                    </h4>
                    <p>
                      Historic data in this release covers periods affected by
                      varying COVID-19 restrictions, which impacted on
                      apprenticeship and traineeship learning and also provider
                      reporting behaviour via the Individualised Learner Record.
                      Therefore, extra care should be taken in comparing and
                      interpreting data presented in this release.
                    </p>
                    <p>
                      <strong>Please note that the ‘</strong>
                      <a href="https://explore-education-statistics.service.gov.uk/find-statistics/apprenticeships-and-traineeships#explore-data-and-files">
                        <strong>
                          Explore data and files used in this release
                        </strong>
                      </a>
                      ’ section contains the underlying files that underpin this
                      release and allows expert users to interrogate and analyse
                      the data for themselves. For pre-populated summary
                      statistics please see the relevant section underneath,
                      from which the data can be further explored using the
                      ‘Explore data’ functionality. You can also view featured
                      tables or create your own table using the ‘
                      <a href="https://explore-education-statistics.service.gov.uk/data-tables/apprenticeships-and-traineeships">
                        <strong>create your own tables</strong>
                      </a>
                      <strong>' functionality.</strong>
                    </p>
                  </div>
                </div>
              </div>
              <h2
                className="govuk-heading-l govuk-!-margin-top-6"
                id="headlines"
              >
                Headline facts and figures
              </h2>
              <Tabs id="test">
                <TabsSection title="Summary">
                  <div className={stylesKeyStat.container}>
                    <div className={stylesKeyStat.wrapper}>
                      <div className={stylesKeyStatTile.tile}>
                        <h3 className="govuk-heading-s">Starts (Aug - Apr)</h3>
                        <p className="govuk-heading-l">275,630</p>
                        <p className="govuk-body-s">
                          down by 4.6% from 2021/22
                        </p>
                      </div>
                    </div>
                    <div className={stylesKeyStat.wrapper}>
                      <div className={stylesKeyStatTile.tile}>
                        <h3 className="govuk-heading-s">
                          Participation (Aug - Apr)
                        </h3>
                        <p className="govuk-heading-l">703,670</p>
                        <p className="govuk-body-s">up by 1.6% from 2021/22</p>
                      </div>
                    </div>
                    <div className={stylesKeyStat.wrapper}>
                      <div className={stylesKeyStatTile.tile}>
                        <h3 className="govuk-heading-s">
                          Achievements (Aug - Apr)
                        </h3>
                        <p className="govuk-heading-l">105,600</p>
                        <p className="govuk-body-s">up by 20.1% from 2021/22</p>
                      </div>
                    </div>
                  </div>
                  <div className="dfe-content">
                    <h4>
                      <strong>
                        Figures for the 2022/23 academic year show:
                      </strong>
                    </h4>
                    <ul>
                      <li>
                        Apprenticeship starts were down by 4.6% to 275,630
                        compared to 288,800 reported for the same period in the
                        previous year.
                      </li>
                      <li>Under 19s accounted for 24.8% of starts (68,290).</li>
                      <li>
                        Advanced apprenticeships accounted for 43.2% of starts
                        (119,170) whilst higher apprenticeships accounted for a
                        34.0% of starts (93,970).
                      </li>
                      <li>
                        Higher apprenticeships continue to grow in 2022/23.
                        Higher apprenticeship starts increased by 6.1% to 93,670
                        compared to 88,240 in the same period last year.
                      </li>
                      <li>
                        Starts at Level 6 and 7 increased by 9.3% to 41,340 in
                        2022/23. This represents 15.0% of all starts reported to
                        date for 2022/23. There were 37,810 Level 6 and 7 starts
                        in the same period last year (13.1% of starts in the
                        same period).
                      </li>
                      <li>
                        Starts supported by Apprenticeship Service Account (ASA)
                        levy funds accounted for 67.0% (184,570).
                      </li>
                      <li>
                        Since May 2015 there have been 3,157,480 apprenticeship
                        starts. Since May 2010 this total stands at 5,535,020.
                      </li>
                      <li>
                        Apprenticeship achievements increased by 20.1% to
                        105,600 compared to 87,920 reported for the same period
                        in the previous year. Please note: COVID-19 restrictions
                        and assessment flexibilities affected the timing of
                        achievements, therefore care must be taken when
                        comparing achievements between years as some
                        achievements expected in a given academic year may have
                        been delayed to the subsequent year.
                      </li>
                      <li>
                        Learner participation increased by 1.6% to 703,670
                        compared to 692,920 reported for the same period in the
                        previous year.
                      </li>
                    </ul>
                  </div>
                </TabsSection>
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
                    Learn more about the data files used in this release using
                    our online guidance
                  </p>
                </div>
              </div>

              <section className="dfe-content">
                <h2 id="about">About these statistics</h2>
                <p>
                  This statistical release presents provisional information on
                  all age (16+) apprenticeships starts, achievements and
                  participation in England for the 2022/23 academic year.
                </p>
                <p>Also published are official statistics covering:</p>
                <ul>
                  <li>Apprenticeship service commitments</li>
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
                  ’. Please note that the FE and skills release includes the
                  adult apprenticeships and traineeships published here in its
                  headline figures.
                </p>
                <h4>Individualised Learner Record (ILR) administrative data</h4>
                <p>
                  The apprenticeship data in this release published in July 2023
                  are based on the tenth ILR data return from FE and
                  apprenticeship providers for the 2022/23 academic year, which
                  was taken in June 2023. The October monthly transparency
                  update is based on the thirteenth ILR data return (taken in
                  September 2023). The ILR is an administrative data collection
                  system designed primarily for operational use in order to fund
                  training providers for learners in FE and on apprenticeship
                  programmes.
                </p>
                <h4>National achievement rate tables data</h4>
                <p>
                  Figures in the ‘national achievement rate tables’ section are
                  as published in March 2023. These official statistics cover
                  achievement rates for apprenticeships in the 2021/22 academic
                  year and would have been previously released as part of the
                  standalone National achievement rate tables publication.
                </p>
                <h4>
                  <strong>
                    Provider reporting during the COVID-19 pandemic
                  </strong>
                </h4>
                <p>
                  Historic data in this publication covers periods affected by
                  varying COVID-19 restrictions, which will have impacted on
                  apprenticeship and traineeship learning. Therefore, extra care
                  should be taken in comparing and interpreting data presented
                  in this release.
                </p>
                <p>
                  The furlough scheme may also have impacted on how aspects of
                  ILR data were recorded, such as how the ‘learning status’ of a
                  learner was captured, e.g. whether a learner was recorded as a
                  continuing learner or whether they were recorded as being on a
                  break in learning while still being with an employer.
                </p>
              </section>

              <section className="dfe-content">
                <h2 id="supplement" className="govuk-!-margin-top-9">
                  How to find data and supplementary tables in this release
                </h2>
                <p>
                  The Apprenticeships and traineeships publication still
                  provides the same range of data it always did, but has
                  undergone some structural changes since the previous
                  publication in order to improve user’s experience.
                </p>
                <p>
                  We have also adopted a new naming convention for files to help
                  users find their data of interest. We have not changed the
                  content of these files except in a few cases where we have
                  merged some smaller files. You can find a look-up of the old
                  and new file names in the file called
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
                    it can be accessed.
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
                  '. These tables are created to provide the next level of
                  detail one might wish to find below the level of detail
                  provided by tables embedded within the release. They also
                  provide the user the opportunity to then amend content,
                  reorder and take away to meet their needs. Within the release
                  we list out the most relevant featured tables at the end of
                  each commentary section.
                </p>
                <p>
                  <strong>
                    In addition to featured tables you can also access
                    underlying data files
                  </strong>
                  and build your own tables using the table builder tool. For
                  example, the featured table showing enrolments by provider is
                  produced from an underlying data file which also contains
                  detail on the level of an aim, and it's sector subject area.
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
                  ', then switching to the 'Create your own table' tab and
                  selecting your file of interest.
                </p>
                <p>
                  Alternatively you can modify and existing featured table by
                  selecting it and then depending on the breakdowns available,
                  edit the location, time periods, indicators and/or filters
                  (Steps 3, 4 and 5).
                </p>
                <p>
                  <strong>There is a dashboard </strong>that provides
                  interactive presentation of our published data, with a number
                  of different views on to data and ‘drilldown’ capability to
                  allow users to investigate different types of FE provision. It
                  is particularly helpful in viewing data across different
                  geographical areas and providers.
                </p>
                <p>
                  <strong>
                    This release also provides ‘all supporting files’
                  </strong>
                  which can be found at the end of the '
                  <a href="https://explore-education-statistics.service.gov.uk/find-statistics/apprenticeships-and-traineeships#explore-data-and-files">
                    Explore data and files
                  </a>
                  ' accordion.<strong> </strong>These are mainly csv files which
                  can be downloaded, and provide some additional breakdowns
                  including unrounded data. They are provided for transparency
                  to enable analysts to re-use the data in this release. A
                  metadata document is available in the same location which
                  explains the content of these supporting files.
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
                  and develop. To provide feedback on this release, please email
                  us at
                  <a href="mailto:FE.OFFICIALSTATISTICS@education.gov.uk">
                    FE.OFFICIALSTATISTICS@education.gov.uk
                  </a>
                  .
                </p>
              </section>

              <section className="dfe-content">
                {' '}
                <h2
                  id="fullYearApprenticeships"
                  className="govuk-!-margin-top-9"
                >
                  Full year apprenticeship landscape
                </h2>
                <blockquote>
                  <p>
                    The figures in this section relate to full-year final data
                    up to and including the 2021/22 academic year and were
                    originally published in November 2022
                  </p>
                </blockquote>
              </section>
              <section className="dfe-content">
                <h3>The changing apprenticeship landscape</h3>
                <p>
                  Reform of the apprenticeships programme, along with the impact
                  of the COVID-19 pandemic have influenced the trends presented
                  in this section. Three main factors are set out in the graphic
                  below.
                </p>

                <PrototypeChartExamples chartType="barChart" />
                <PrototypeChartExamples chartType="map" />
                <PrototypeChartExamples chartType="lineChart" />
              </section>
              <PrototypeDashboardContent />
              <section>
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
              </section>
            </div>
          </div>
        </div>
      </PrototypePage>
    </div>
  );
};

export default PrototypeReleaseData;
