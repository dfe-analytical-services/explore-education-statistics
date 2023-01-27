import classNames from 'classnames';
import Link from '@admin/components/Link';
import PrototypePage from '@admin/prototypes/components/PrototypePage';
import Details from '@common/components/Details';
import PageSearchForm from '@common/components/PageSearchForm';
import RelatedAside from '@common/components/RelatedAside';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import stylesKeyStat from '@common/modules/find-statistics/components/KeyStat.module.scss';
import stylesKeyStatTile from '@common/modules/find-statistics/components/KeyStatTile.module.scss';
import React from 'react';
import Accordion from './components/PrototypeAccordion';
import AccordionSection from './components/PrototypeAccordionSection';
import PrototypeDownloadAncillary from './components/PrototypeDownloadAncillary';
import PrototypeDownloadPopular from './components/PrototypeDownloadPopular';
import PrototypeDownloadUnderlying from './components/PrototypeDownloadUnderlying';
import PrototypeTableBuilder from './components/PrototypeTableBuilder';
import styles from './PrototypePublicPage.module.scss';

const PrototypeReleaseData = () => {
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
                  src="/assets/images/UKSA-quality-mark.jpg"
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
                    href="/subscriptions?slug=children-looked-after-in-england-including-adoptions"
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
            <RelatedAside>
              <h2 className="govuk-heading-m">Quick links</h2>
              <ul className="govuk-list">
                <li>
                  <a
                    href="#data-1"
                    className="govuk-button govuk-!-margin-bottom-3"
                  >
                    Download all data (zip)
                  </a>
                </li>
                <li>
                  <Link to="#contents">Release contents</Link>
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
            </RelatedAside>
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
            styles.prototypeCardContainer,
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
                href="#"
                className={classNames(
                  styles.prototypeCardChevronLink,
                  'govuk-link--no-visited-state',
                )}
              >
                Download all data (.zip)
              </a>
            </h2>
            <p className="govuk-body govuk-!-margin-bottom-0">
              All data used in this release is available as open data for
              download
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
        </div>
        <div
          className={classNames(
            styles.prototypeCardContainer,
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
              You can view featured tables that we have built for you, or create
              your own tables from the open data using our table tool
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
          <AccordionSection heading="All supporting files" goToTop={false}>
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
        <div>
          <div className={styles.stickyLinksContainer}>
            <div className={classNames(styles.stickyLinks, 'govuk-inset-text')}>
              <h3>Contents quick links</h3>
              <ul className="govuk-list govuk-list--spaced">
                <li>
                  <a href="#about-these-statistics">About these statistics</a>
                </li>
                <li>
                  <a href="#how-to-use-this-release-and-find-data">
                    How to use this release and find datas
                  </a>
                </li>
                <li>
                  <a href="#interactive-data-visualisation-tool">
                    Interactive data visualisation tool
                  </a>
                </li>
                <li>
                  <a href="#latest-headline-statistics">
                    Latest headline statistics
                  </a>
                </li>
                <li>
                  <a href="#annual-time-series">Annual time series</a>
                </li>
                <li>
                  <a href="#learner-characteristics">Learner characteristics</a>
                </li>
                <li>
                  <a href="#subjects-and-levels">Subjects and levels</a>
                </li>
                <li>
                  <a href="#geographical-breakdowns">Geographical breakdowns</a>
                </li>
                <li>
                  <a href="#providers">Providers</a>
                </li>
                <li>
                  <a href="#public-sector-apprenticeships">
                    Public sector apprenticeships
                  </a>
                </li>
                <li>
                  <a href="#Traineeships">Traineeships</a>
                </li>
                <li>
                  <a href="#national-achievement-rate-tables">
                    National achievement rate tables
                  </a>
                </li>
                <li>
                  <a href="#apprenticeship-service-and-monthly-transparency-data">
                    Apprenticeship Service and monthly transparency data
                  </a>
                </li>
                <li>
                  <a href="#additional-analysis">Additional analysis</a>
                </li>
                <li>
                  <a href="#future-changes">Future changes</a>
                </li>
              </ul>
            </div>
          </div>
          <h2 className="govuk-heading-l" id="contents">
            Release contents
          </h2>
          <Accordion id="content">
            <AccordionSection heading="About these statistics" goToTop={false}>
              This is a test
            </AccordionSection>
            <AccordionSection
              heading="How to use this release and find data"
              goToTop={false}
            >
              This is a test
            </AccordionSection>
            <AccordionSection
              heading="Interactive data visualisation tool"
              goToTop={false}
            >
              This is a test
            </AccordionSection>
            <AccordionSection
              heading="Latest headline statistics"
              goToTop={false}
            >
              This is a test
            </AccordionSection>
            <AccordionSection heading="Annual time series" goToTop={false}>
              This is a test
            </AccordionSection>
            <AccordionSection heading="Learner characteristics" goToTop={false}>
              This is a test
            </AccordionSection>
            <AccordionSection heading="Subjects and levels" goToTop={false}>
              This is a test
            </AccordionSection>
            <AccordionSection heading="Geographical breakdowns" goToTop={false}>
              This is a test
            </AccordionSection>
            <AccordionSection heading="Providers" goToTop={false}>
              This is a test
            </AccordionSection>
            <AccordionSection
              heading="Public sector apprenticeships"
              goToTop={false}
            >
              This is a test
            </AccordionSection>
            <AccordionSection heading="Traineeships" goToTop={false}>
              This is a test
            </AccordionSection>
            <AccordionSection
              heading="National achievement rate tables"
              goToTop={false}
            >
              This is a test
            </AccordionSection>
            <AccordionSection
              heading="Apprenticeship Service and monthly transparency data"
              goToTop={false}
            >
              This is a test
            </AccordionSection>
            <AccordionSection heading="Additional analysis" goToTop={false}>
              This is a test
            </AccordionSection>
            <AccordionSection heading="Future changes" goToTop={false}>
              This is a test
            </AccordionSection>
          </Accordion>
        </div>
        <h2
          className="govuk-heading-l govuk-!-margin-top-9"
          id="extra-information"
          data-testid="extra-information"
        >
          Help and support
        </h2>
        <Accordion id="downloads">
          <AccordionSection heading="Download associated files" goToTop={false}>
            <PrototypeDownloadPopular />
            <PrototypeDownloadUnderlying />
            <PrototypeDownloadAncillary />
            <PrototypeTableBuilder />
          </AccordionSection>
          <AccordionSection
            heading="Download associated files (Example 2)"
            goToTop={false}
          >
            <PrototypeDownloadPopular viewAsList />
            <PrototypeDownloadUnderlying viewAsList />
            <PrototypeDownloadAncillary />
            <PrototypeTableBuilder />
          </AccordionSection>
          <AccordionSection
            heading="Download associated files (Example 3)"
            goToTop={false}
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
          <AccordionSection heading="Methodology" goToTop={false}>
            Methodology section
          </AccordionSection>
          <AccordionSection heading="National statistics" goToTop={false}>
            National statistics
          </AccordionSection>
          <AccordionSection heading="Contact us" goToTop={false}>
            Contact us
          </AccordionSection>
        </Accordion>
        <div style={{ height: '1000px' }}>test</div>
      </PrototypePage>
    </div>
  );
};

export default PrototypeReleaseData;
