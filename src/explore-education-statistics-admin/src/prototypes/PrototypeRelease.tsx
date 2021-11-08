import Link from '@admin/components/Link';
import PrototypePage from '@admin/prototypes/components/PrototypePage';
import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import Details from '@common/components/Details';
import PageSearchForm from '@common/components/PageSearchForm';
import RelatedAside from '@common/components/RelatedAside';
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

const PrototypeRelease = () => {
  return (
    <div className={styles.prototypePublicPage}>
      <PrototypePage
        breadcrumbs={[
          {
            name: 'Childrens social care',
            link:
              'https://explore-education-statistics.service.gov.uk/find-statistics#themes-1',
          },
          {
            name: 'Children looked after in England including adoptions',
            link: '#',
          },
        ]}
        title="Children looked after in England"
        caption="Reporting year 2020"
        wide={false}
      >
        <div className="govuk-grid-row">
          <div className="govuk-grid-column-two-thirds">
            <div className="govuk-grid-row">
              <div className={styles.prototypeFlexContainer}>
                <div>
                  <strong className="govuk-tag">This is the latest data</strong>
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
                  <time>10 December 2020</time>
                </dd>
              </div>
              <div className="govuk-summary-list__row">
                <dt className="govuk-summary-list__key">Next update: </dt>
                <dd className="govuk-summary-list__value">
                  <time>01 December 2021</time>
                </dd>
              </div>
              <div className="govuk-summary-list__row">
                <dt className="govuk-summary-list__key">Last updated: </dt>
                <dd className="govuk-summary-list__value">
                  <time>18 December 2020</time>
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
                      href="/subscriptions?slug=children-looked-after-in-england-including-adoptions"
                      className="govuk-link govuk-link--no-visited-state dfe-print-hidden"
                    >
                      Sign up for email alerts
                    </a>
                  </strong>
                </dd>
              </div>
            </dl>
            <p className="govuk-!-margin-top-3">
              Information on children looked after in England, including numbers
              of looked after children adopted, care leavers and looked after
              children who are missing. Data is taken from the annual SSDA903
              data collection.
            </p>

            <PageSearchForm inputLabel="Search this page" />
          </div>
          <div className="govuk-grid-column-one-third">
            <RelatedAside>
              <h2 className="govuk-heading-m">Related infomation</h2>
              <ul className="govuk-list">
                <li>
                  <a
                    href="#data-1"
                    className="govuk-button govuk-!-margin-bottom-0"
                  >
                    Download data
                  </a>
                </li>
              </ul>
              <ul className="govuk-list govuk-list--spaced">
                <li>
                  <Link to="/methodology">Methodology</Link>
                </li>
                <li>
                  <Link to="/glossary">Glossary</Link>
                </li>
                <li>
                  <Link to="/glossary">Metadata guidance</Link>
                </li>
                <li>
                  <Link to="/glossary">Pre-release access list</Link>
                </li>
                <li>
                  <Details summary="View previous releases (10)">Test</Details>
                </li>
              </ul>
              <h3 className="govuk-heading-s govuk-!-margin-top-3">
                Related pages
              </h3>
              <ul className="govuk-list govuk-list--spaced">
                <li>
                  <a href="#">Children's social care</a>
                </li>
              </ul>
            </RelatedAside>
          </div>
        </div>
        <Tabs id="test">
          <TabsSection title="Summary">
            <div className={stylesKeyStat.container}>
              <div className={stylesKeyStat.column}>
                <div className={stylesKeyStatTile.tile}>
                  <h3 className="govuk-heading-s">
                    Children in need at 31 March
                  </h3>
                  <p className="govuk-heading-xl">389,260</p>
                  <p className="govuk-body-s">
                    a decrease of 2.6% from the same point in 2019
                  </p>
                </div>
              </div>
              <div className={stylesKeyStat.column}>
                <div className={stylesKeyStatTile.tile}>
                  <h3 className="govuk-heading-s">
                    Children in need at 31 March
                  </h3>
                  <p className="govuk-heading-xl">389,260</p>
                  <p className="govuk-body-s">
                    a decrease of 2.6% from the same point in 2019
                  </p>
                </div>
              </div>
              <div className={stylesKeyStat.column}>
                <div className={stylesKeyStatTile.tile}>
                  <h3 className="govuk-heading-s">
                    Children in need at 31 March
                  </h3>
                  <p className="govuk-heading-xl">389,260</p>
                  <p className="govuk-body-s">
                    a decrease of 2.6% from the same point in 2019
                  </p>
                </div>
              </div>
            </div>
            <div>
              <ul>
                <li>
                  there were 389,260 children in need at 31 March 2020, a
                  decrease of 2.6% from the same point in 2019
                </li>
                <li>
                  this was a rate of 323.7 per 10,000 children, down from 334.2
                  last year and the lowest rate in the last 8 years
                </li>
                <li>
                  there were 51,510 children in need on child protection plans,
                  a decrease of 1.4% from the same point in 2019
                </li>
                <li>
                  this was a rate of 42.8 per 10,000 children, down from 43.7
                  last year
                </li>
                <li>
                  there were 642,980 referrals during the year, a decrease of
                  1.0% compared to 2019
                </li>
                <li>
                  domestic violence by the parent was identified as a factor at
                  the end of assessment in 169,860 episodes of need and remains
                  the most common factor
                </li>
              </ul>
            </div>
          </TabsSection>
        </Tabs>
        <Accordion id="data">
          <AccordionSection
            heading="Download data and metadata"
            goToTop={false}
          >
            <p>
              Find and download files used in the production of this release.
            </p>
            <PrototypeDownloadPopular />
            <PrototypeDownloadUnderlying />
            <PrototypeDownloadAncillary />
            <PrototypeTableBuilder />
          </AccordionSection>
        </Accordion>
        <Accordion id="content">
          <AccordionSection
            heading="Children looked after on 31 March"
            goToTop={false}
          >
            This is a test
          </AccordionSection>
          <AccordionSection
            heading="Children starting to be looked after"
            goToTop={false}
          >
            This is a test
          </AccordionSection>
          <AccordionSection
            heading="Health outcomes for children looked after for at least 12 months on 31 March"
            goToTop={false}
          >
            This is a test
          </AccordionSection>
          <AccordionSection
            heading="Children looked after who were missing"
            goToTop={false}
          >
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
      </PrototypePage>
    </div>
  );
};

export default PrototypeRelease;
