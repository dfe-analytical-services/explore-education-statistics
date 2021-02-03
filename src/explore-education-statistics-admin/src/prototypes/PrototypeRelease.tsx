import PrototypePage from '@admin/prototypes/components/PrototypePage';
import React from 'react';
import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import Details from '@common/components/Details';
import Link from '@admin/components/Link';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import RelatedAside from '@common/components/RelatedAside';
import RelatedInformation from '@common/components/RelatedInformation';
import classNames from 'classnames';
import PageSearchForm from '@common/components/PageSearchForm';
import styles from './PrototypePublicPage.module.scss';
import PrototypeDownloadPopular from './components/PrototypeDownloadPopular';
import PrototypeDownloadUnderlying from './components/PrototypeDownloadUnderlying';
import PrototypeDownloadAncillary from './components/PrototypeDownloadAncillary';

const PrototypeRelease = () => {
  return (
    <div className={styles.prototypePublicPage}>
      <PrototypePage
        breadcrumbs={[
          {
            name: 'Childrens social care',
            link:
              'https://explore-education-statistics.service.gov.uk/find-statistics#publications-1',
          },
          {
            name: 'Children looked after in England including adoptions',
            link: '#',
          },
        ]}
        title="Children looked after in England including adoptions"
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
                  src="/assets/images/UKSA-quality-mark2.jpg"
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
              {/*<div className="govuk-summary-list__row">
                <dt className="govuk-summary-list__key">Next update: </dt>
                <dd className="govuk-summary-list__value">
                  <time>01 December 2021</time>
                </dd>
              </div>*/}
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
            <p>
              Although the majority of this data relates to before the
              coronavirus (COVID-19) pandemic, there could be a small effect on
              these figures due to the impact the pandemic had on social work
              practice in the second half of March 2020. The{' '}
              <a href="https://www.gov.uk/government/publications/vulnerable-children-and-young-people-survey">
                vulnerable children and young people survey
              </a>{' '}
              has been collecting information from local authorities in England
              to help understand the how the coronavirus (COVID-19) outbreak
              affected children’s social care.
            </p>
            <PageSearchForm inputLabel="Search this page" />
          </div>
          <div className="govuk-grid-column-one-third">
            <RelatedAside>
              <h2 className="govuk-heading-m">Related infomation</h2>
              <ul className="govuk-list">
                <li>
                  <a
                    href="#downloads-1"
                    className="govuk-button govuk-!-margin-bottom-0"
                  >
                    Download files
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
          <TabsSection title="Summary">HIGHLIGHTS</TabsSection>
        </Tabs>
        <Accordion id="content">
          <AccordionSection heading="Section 1" goToTop={false}>
            This is a test
          </AccordionSection>
          <AccordionSection heading="Section 2" goToTop={false}>
            This is a test
          </AccordionSection>
          <AccordionSection heading="Section 3" goToTop={false}>
            This is a test
          </AccordionSection>
          <AccordionSection heading="Section 4" goToTop={false}>
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
            <div className="govuk-grid-row">
              <div className="govuk-grid-column-three-quarters">
                <PrototypeDownloadPopular />
                <PrototypeDownloadUnderlying />
                <PrototypeDownloadAncillary />
                <h3 className="govuk-heading-m">
                  Create your own tables online
                </h3>
                <p>
                  Use our tool to build tables using our range of national and
                  regional data
                </p>
                <a href="#" className="govuk-button">
                  Create table
                </a>
              </div>
            </div>
          </AccordionSection>
          <AccordionSection
            heading="Download associated files (Example 2)"
            goToTop={false}
          >
            <Tabs id="download">
              <TabsSection title="Popular tables">
                <PrototypeDownloadPopular />
              </TabsSection>
              <TabsSection title="Underlying data">
                <PrototypeDownloadUnderlying />
              </TabsSection>
              <TabsSection title="Ancillary files">
                <PrototypeDownloadAncillary />
              </TabsSection>
            </Tabs>
            <h3 className="govuk-heading-m">Create your own tables online</h3>
            <p>
              Use our tool to build tables using our range of national and
              regional data
            </p>
            <a href="#" className="govuk-button">
              Create table
            </a>
          </AccordionSection>
        </Accordion>
      </PrototypePage>
    </div>
  );
};

export default PrototypeRelease;
