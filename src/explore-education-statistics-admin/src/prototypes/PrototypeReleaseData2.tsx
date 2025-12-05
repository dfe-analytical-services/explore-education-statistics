import classNames from 'classnames';
import Link from '@admin/components/Link';
import PrototypePage from '@admin/prototypes/components/PrototypePage';
import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import Details from '@common/components/Details';
import PageSearchForm from '@common/components/PageSearchForm';
import RelatedContent from '@common/components/RelatedContent';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import stylesKeyStat from '@common/modules/find-statistics/components/KeyStat.module.scss';
import stylesKeyStatTile from '@common/modules/find-statistics/components/KeyStatTile.module.scss';
import React from 'react';
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
            name: 'Destination of pupils and students',
            link: '#',
          },
          {
            name: '16-18 destination measures',
            link: '#',
          },
        ]}
        title="16-18 destination measures"
        caption="Academic Year 2020/21"
        wide={false}
      >
        <div className="govuk-grid-row">
          <div className="govuk-grid-column-two-thirds">
            <div className="govuk-grid-row">
              <div className={styles.prototypeFlexContainer}>
                <div>
                  <span className="govuk-tag">This is the latest data</span>{' '}
                  <span className="govuk-tag">Official statistics</span>
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
                  <time>10 January 2023</time>
                </dd>
              </div>
              <div className="govuk-summary-list__row">
                <dt className="govuk-summary-list__key">Next update: </dt>
                <dd className="govuk-summary-list__value">
                  <time>11 January 2024</time>
                </dd>
              </div>
              <div className="govuk-summary-list__row">
                <dt className="govuk-summary-list__key">Last updated: </dt>
                <dd className="govuk-summary-list__value">
                  <time>18 January 2023</time>
                  <Details
                    className="govuk-!-margin-top-2"
                    summary="See all updates (2)"
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
                <dt className="govuk-summary-list__key">Receive updates: </dt>
                <dd className="govuk-summary-list__value">
                  <strong>
                    <a
                      data-testid="subscription-children-looked-after-in-england-including-adoptions"
                      href="/subscriptions/new-subscription/children-looked-after-in-england-including-adoptions"
                      className="govuk-link govuk-link--no-visited-state dfe-print-hidden"
                    >
                      Sign up for email alerts
                    </a>
                  </strong>
                </dd>
              </div>
            </dl>
            <p className="govuk-!-margin-top-3">
              These official statistics show students continuing to education,
              apprenticeship or employment destinations in the year after
              completing 16 to 18 study in schools and colleges in England.
            </p>
            <p>
              The release also provides information on destination outcomes for
              students based on a range of individual characteristics, and
              geographical location and type of education provider.
            </p>
            <p>
              The release focuses on outcomes for state-funded mainstream
              schools and colleges. See institution type section for outcomes
              for independent mainstream schools and special schools.
            </p>

            <PageSearchForm inputLabel="Search this page" />
          </div>
          <div className="govuk-grid-column-one-third">
            <RelatedContent>
              <h2 className="govuk-heading-m">Data downloads</h2>
              <ul className="govuk-list govuk-list--spaced">
                <li>
                  <a href="#">Explore data and files</a>
                </li>
                <li>
                  <a href="#">View data guidance</a>
                </li>
                <li>
                  <a
                    href="#data-1"
                    className="govuk-button govuk-!-margin-bottom-0"
                  >
                    Download all data (zip)
                  </a>
                </li>
              </ul>
              <h2 className="govuk-heading-m">Supporting information</h2>
              <ul className="govuk-list govuk-list--spaced">
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
                  <a href="#">16-18 destination measures</a>
                </li>
              </ul>
            </RelatedContent>
          </div>
        </div>
        <h2 className="govuk-heading-l">Key statistics and data downloads</h2>
        <Tabs id="test">
          <TabsSection title="Summary">
            <div className={stylesKeyStat.container}>
              <div className={stylesKeyStat.column}>
                <div className={stylesKeyStatTile.tile}>
                  <h3 className="govuk-heading-s">
                    Sustained education, apprenticeship or employment
                  </h3>
                  <p className="govuk-heading-xl">79.1%</p>
                  <p className="govuk-body-s">
                    1.6 percentage point decrease since previous year
                  </p>
                </div>
              </div>
              <div className={stylesKeyStat.column}>
                <div className={stylesKeyStatTile.tile}>
                  <h3 className="govuk-heading-s">
                    Sustained education destination
                  </h3>
                  <p className="govuk-heading-xl">51.9%</p>
                  <p className="govuk-body-s">
                    4.5 percentage point increase since previous year
                  </p>
                </div>
              </div>
              <div className={stylesKeyStat.column}>
                <div className={stylesKeyStatTile.tile}>
                  <h3 className="govuk-heading-s">Sustained apprenticeships</h3>
                  <p className="govuk-heading-xl">6.4%</p>
                  <p className="govuk-body-s">
                    2.0 percentage point decrease since previous year
                  </p>
                </div>
              </div>
              <div className={stylesKeyStat.column}>
                <div className={stylesKeyStatTile.tile}>
                  <h3 className="govuk-heading-s">
                    Sustained employment destination
                  </h3>
                  <p className="govuk-heading-xl">20.8%</p>
                  <p className="govuk-body-s">
                    4.1 percentage point decrease since previous year
                  </p>
                </div>
              </div>
            </div>
            <div>
              <h3 className="govuk-heading-s">
                {' '}
                Overall sustained destination rate declined in 2020/21 with
                lower numbers going into apprenticeships and employment.
              </h3>
              <p>
                The headline destination rate was 79.1% for students that were
                deemed to have reached the end of 16 to 18 study in 2020, this
                is 1.6 percentage points less when compared to the previous
                academic year. This cohort shows a higher rate of students
                continuing in education (51.9%) and lower rates of
                apprenticeship and employment destinations compared to previous
                years. The decreases in apprenticeship and employment
                destinations are likely due to the disruption caused by the
                coronavirus (COVID-19) pandemic. The increase in the proportion
                of students progressing to further education is mainly due to a
                change in the underlying cohort.
              </p>
              <h3 className="govuk-heading-s">
                {' '}
                The rise in overall education destinations was mainly driven by
                an increase in further education. The number of students going
                to higher education was also up.
              </h3>
              <p>
                Further education destinations went up by 3.4 percentage points
                compared to the previous year. This is in large part due to a
                change in the underlying cohort of 16 to 18 students, with more
                students that stayed at their 16 to 18 provider for a third year
                of study included in the 2020 cohort. The rate of progression to
                higher education was also higher, up by 0.2 percentage points.
              </p>
            </div>
          </TabsSection>
          <TabsSection title="Table">This is the table section</TabsSection>
        </Tabs>
        <h2 className="govuk-heading-m">Data and files used in this release</h2>
        <div
          className={classNames(
            styles.prototypeCardContainer,
            styles.prototypeCardBg,
          )}
        >
          <div className={classNames(styles.prototypeCardChevron)}>
            <h2
              className={classNames(
                'govuk-heading-m',
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
            <p className="govuk-body-l govuk-!-margin-bottom-0">
              All data used in this release is available as open data for
              download
            </p>
          </div>
          <div className={classNames(styles.prototypeCardChevron)}>
            <h2
              className={classNames(
                'govuk-heading-m',
                'govuk-!-margin-bottom-2',
              )}
            >
              <a
                href="https://explore-education-statistics.service.gov.uk/data-catalogue/16-18-destination-measures/2020-21"
                className={classNames(
                  styles.prototypeCardChevronLink,
                  'govuk-link--no-visited-state',
                )}
              >
                Open data catalogue
              </a>
            </h2>
            <p className="govuk-body-l govuk-!-margin-bottom-0">
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
                'govuk-heading-m',
                'govuk-!-margin-bottom-2',
              )}
            >
              <a
                href="https://explore-education-statistics.service.gov.uk/data-tables/16-18-destination-measures"
                className={classNames(
                  styles.prototypeCardChevronLink,
                  'govuk-link--no-visited-state',
                )}
              >
                View or create your own tables
              </a>
            </h2>
            <p className="govuk-body-l govuk-!-margin-bottom-0">
              You can view featured tables that we have built for you, or create
              your own tables from the open data using our table tool
            </p>
          </div>
          <div className={classNames(styles.prototypeCardChevron)}>
            <h2
              className={classNames(
                'govuk-heading-m',
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
                Data guidance
              </a>
            </h2>
            <p className="govuk-body-l govuk-!-margin-bottom-0">
              Learn more about the data files used in this release using our
              online guidance
            </p>
          </div>
        </div>
        <Accordion id="data">
          <AccordionSection heading="All supporting files" backToTop={false}>
            <p>
              All supporting files from this release are listed for individual
              download below:
            </p>
            <ul className="govuk-list">
              <li>
                <h3 className="govuk-heading-s govuk-!-margin-bottom-2">
                  <a href="#">
                    Example 1, 16-18 Institution level destinations (csv, 39 Mb)
                  </a>
                </h3>
                <p>
                  Example 1, Institution level data showing the sustained
                  destination of students in the year after completing 16-18
                  study (2019/20 cohort).
                </p>
                <hr />
              </li>
              <li>
                <h3 className="govuk-heading-s govuk-!-margin-bottom-2">
                  <a href="#">
                    Example 2, 16-18 Institution level destinations (csv, 45 Mb)
                  </a>
                </h3>
                <p>
                  Example 2, Institution level data showing the sustained
                  destination of students in the year after completing 16-18
                  study (2019/20 cohort).
                </p>
                <hr />
              </li>
              <li>
                <h3 className="govuk-heading-s govuk-!-margin-bottom-2">
                  <a href="#">
                    Example 3, 16-18 Institution level destinations (csv, 36 Mb)
                  </a>
                </h3>
                <p>
                  Example 3, Institution level data showing the sustained
                  destination of students in the year after completing 16-18
                  study (2019/20 cohort).
                </p>
              </li>
            </ul>
          </AccordionSection>
        </Accordion>
        <h2 className="govuk-heading-l">Release content</h2>
        <Accordion id="content">
          <AccordionSection
            heading="What are destination measures?"
            backToTop={false}
          >
            This is a test
          </AccordionSection>
          <AccordionSection heading="Student characteristics" backToTop={false}>
            This is a test
          </AccordionSection>
          <AccordionSection
            heading="Qualification level studied and prior attainment"
            backToTop={false}
          >
            This is a test
          </AccordionSection>
          <AccordionSection heading="Chamge across the years" backToTop={false}>
            This is a test
          </AccordionSection>
          <AccordionSection heading="Institution type" backToTop={false}>
            This is a test
          </AccordionSection>
        </Accordion>
        <h2
          className="govuk-heading-m govuk-!-margin-top-9"
          data-testid="extra-information"
        >
          Downloads, help and support
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
