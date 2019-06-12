import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import Details from '@common/components/Details';
import Link from '@frontend/components/Link';
import PrototypePage from '@frontend/prototypes/components/PrototypePage';
import React from 'react';
import PrototypeDataSample from './components/PrototypeDataSampleApplications';

const PublicationPageExclusions = () => {
  return (
    <PrototypePage
      breadcrumbs={[
        {
          link: '/prototypes/browse-releases-find',
          text: 'Find statistics and download data',
        },
        { text: 'Permanent and fixed-period exclusions statistics', link: '#' },
      ]}
    >
      <strong className="govuk-tag govuk-!-margin-bottom-2">
        {' '}
        Latest statistics and data{' '}
      </strong>
      <h1 className="govuk-heading-xl">
        Secondary and primary school applications and offers in England
      </h1>
      <dl className="dfe-meta-content">
        <dt className="govuk-caption-m">Published: </dt>
        <dd>
          <strong>14 June 2018</strong>
        </dd>
      </dl>
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-two-thirds">
          <div className="govuk-grid-row">
            <div className="govuk-grid-column-three-quarters">
              <p className="govuk-body">
                Read national statistical summaries, view charts and tables and
                download data files.
              </p>
              <p>
                All figures refer to the <strong>March and April 2018</strong> -
                unless otherwise stated.
              </p>
              <p className="govuk-inset-text">
                <a href="#contents-exclusions-sections-heading-8">
                  View regional and local authority (LA) breakdowns
                </a>
              </p>

              <p>
                Find out how and why these statistics are collected and
                published -{' '}
                <Link to="#">
                  Secondary and primary school applications and offers:
                  methodology
                </Link>
              </p>
            </div>
            <div className="govuk-grid-column-one-quarter">
              <img
                src="/static/images/UKSA-quality-mark.jpg"
                alt="UK statistics authority quality mark"
                height="130"
                width="130"
              />
            </div>
          </div>

          <Details summary="Download data files">
            <p>
              Download data in the following formats or access our data via our
              API:
            </p>
            <ul className="govuk-list">
              <li>
                <a href="#" className="govuk-link">
                  Download pdf files
                </a>
              </li>
              <li>
                <a href="#" className="govuk-link">
                  Download Excel files
                </a>
              </li>
              <li>
                <a href="#" className="govuk-link">
                  Download .csv files
                </a>
              </li>
              <li>
                <a href="/glossary#what-is-an-api" className="govuk-link">
                  Access API
                </a>{' '}
                -{' '}
                <a href="/glossary#what-is-an-api" className="govuk-link">
                  What is an API?
                </a>
              </li>
            </ul>
          </Details>
        </div>

        <div className="govuk-grid-column-one-third">
          <aside className="app-related-items">
            <h2 className="govuk-heading-m" id="subsection-title">
              About these statistics
            </h2>

            <h3 className="govuk-heading-s govuk-!-margin-bottom-0">
              <span className="govuk-caption-m govuk-caption-inline">
                For:{' '}
              </span>
              March and April 2018 (latest data)
            </h3>

            <Details summary="See previous 7 releases">
              <ul className="govuk-list">
                <li>
                  <a href="https://www.gov.uk/government/statistics/permanent-and-fixed-period-exclusions-in-england-2015-to-2016">
                    2015 to 2016
                  </a>
                </li>
                <li>
                  <a href="https://www.gov.uk/government/statistics/permanent-and-fixed-period-exclusions-in-england-2014-to-2015">
                    2014 to 2015
                  </a>
                </li>
                <li>
                  <a href="https://www.gov.uk/government/statistics/permanent-and-fixed-period-exclusions-in-england-2013-to-2014">
                    2013 to 2014
                  </a>
                </li>
                <li>
                  <a href="https://www.gov.uk/government/statistics/permanent-and-fixed-period-exclusions-in-england-2012-to-2013">
                    2012 to 2013
                  </a>
                </li>
                <li>
                  <a href="https://www.gov.uk/government/statistics/permanent-and-fixed-period-exclusions-from-schools-in-england-2011-to-2012-academic-year">
                    2011 to 2012
                  </a>
                </li>
                <li>
                  <a href="https://www.gov.uk/government/statistics/permanent-and-fixed-period-exclusions-from-schools-in-england-academic-year-2010-to-2011">
                    2010 to 2011
                  </a>
                </li>
                <li>
                  <a href="https://www.gov.uk/government/statistics/permanent-and-fixed-period-exclusions-from-schools-in-england-academic-year-2009-to-2010">
                    2009 to 2010
                  </a>
                </li>
                <li>
                  <a href="https://www.gov.uk/government/statistics/permanent-and-fixed-period-exclusions-in-england-academic-year-2008-to-2009">
                    2008 to 2009
                  </a>
                </li>
              </ul>
            </Details>

            <h3 className="govuk-heading-s govuk-!-margin-bottom-0">
              <span className="govuk-caption-m">Last updated: </span>11 June
              2018
            </h3>

            <Details summary="See all 2 updates">
              <div data-testid="publication-page--update-element">
                <h3 className="govuk-heading-s">6 August 2018</h3>
                <p>
                  Updated exclusion rates for Gypsy/Roma pupils, to include
                  extended ethnicity categories within the headcount (Gypsy,
                  Roma and other Gypsy/Roma).
                </p>
              </div>
              <div data-testid="publication-page--update-element">
                <h3 className="govuk-heading-s">19 July 2018</h3>
                <p>First published.</p>
              </div>
            </Details>

            <h3 className="govuk-heading-s govuk-!-margin-bottom-0">
              <span className="govuk-caption-m">Next update: </span>June 2019
            </h3>
            <p className="govuk-caption-m govuk-!-margin-top-0">
              <a href="#">Sign up for updates</a>
            </p>

            <h2
              className="govuk-heading-m govuk-!-margin-top-6"
              id="related-content"
            >
              Related guidance
            </h2>
            <nav role="navigation" aria-labelledby="related-content">
              <ul className="govuk-list">
                <li>
                  <Link to="#">
                    Secondary and primary school applications and offers:
                    methodology
                  </Link>
                </li>
              </ul>
            </nav>
          </aside>
        </div>
      </div>

      <hr />

      <h2 className="govuk-heading-l">
        Headline facts and figures - March and April 2018
      </h2>

      <PrototypeDataSample
        sectionId="headlines"
        chartTitle="change in permanent exclusion percentage in England"
        xAxisLabel="School Year"
        yAxisLabel="Permanent Exclusion %"
        chartData={[
          {
            name: '2012/13',
            primary: 0.02,
            secondary: 0.23,
            special: 0.18,
            total: 0.12,
          },
          {
            name: '2013/14',
            primary: 0.02,
            secondary: 0.23,
            special: 0.18,
            total: 0.12,
          },
          {
            name: '2014/15',
            primary: 0.03,
            secondary: 0.2,
            special: 0.07,
            total: 0.1,
          },
          {
            name: '2015/16',
            primary: 0.03,
            secondary: 0.21,
            special: 0.04,
            total: 0.09,
          },
          {
            name: '2016/17',
            primary: 0.03,
            secondary: 0.2,
            special: 0.07,
            total: 0.1,
          },
        ]}
        chartDataKeys={['total', 'primary', 'secondary', 'special']}
      />

      <Accordion id="contents-exclusions-sections">
        <AccordionSection heading="Secondary applications and offers">
          <h3 className="govuk-heading-s">Secondary applications</h3>
          <p className="govuk-body">
            The number of applications received for secondary school places
            increased to 582,761 - up 3.6% since 2017. This follows a 2.6%
            increase between 2016 and 2017.
          </p>
          <p>
            This continues the increase in secondary applications seen since
            2013 which came on the back of a rise in births which began in the
            previous decade.
          </p>
          <p>
            Since 2013, when secondary applications were at their lowest, there
            has been a 16.6% increase in the number of applications.
          </p>
          <h3 className="govuk-heading-s">Secondary offers</h3>
          <p>
            The proportion of secondary applicants receiving an offer of their
            first-choice school has decreased to 82.1% - down from 83.5% in
            2017.
          </p>
          <p>
            The proportion of applicants who received an offer of any of their
            preferred schools also decreased slightly to 95.5% - down from 96.1%
            in 2017.
          </p>
          <h3 className="govuk-heading-s">Secondary National Offer Day</h3>
          <p>
            These statistics come from the process undertaken by local
            authorities (LAs) which enabled them to send out offers of secondary
            school places to all applicants on the{' '}
            <a href="/glossary#national-offer-day">
              Secondary National Offer Day
            </a>{' '}
            of 1 March 2018.
          </p>
          <p>
            The secondary figures have been collected since 2008 and can be
            viewed as a time series in the following table.
          </p>
          <p>
            INSERT - Table A: Timeseries of key secondary preference rates,
            England
          </p>
        </AccordionSection>

        <AccordionSection heading="Secondary geographical variation">
          <h3 className="govuk-heading-s">First preference rates</h3>
          <p>
            At local authority (LA) level, the 3 highest first preference rates
            were achieved by the following local authorities:
          </p>
          <ul className="govuk-list-bullet">
            <li>Northumberland - 98.1%</li>
            <li>East Riding of Yorkshire - 96.7%</li>
            <li>Bedford - 96.4%</li>
          </ul>
          <p>
            Northumberland has been the top performer in this measure since
            2015.
          </p>
          <p>
            As in previous years, the lowest first preference rates were all in
            London.
          </p>
          <ul className="govuk-list-bullet">
            <li>Hammersmith and Fulham - 51.4%</li>
            <li>Kensington and Chelsea - 54.3%</li>
            <li>Lambeth - 55.2%</li>
          </ul>
          <p>
            These figures do not include City of London which has a tiny number
            of applications and no secondary schools.
          </p>
          <p>
            Hammersmith and Fulham has had the lowest first preference rate
            since 2015.
          </p>
          <p>
            The higher number of practical options available to London
            applicants and ability to name 6 preferences may encourage parents
            to make more speculative choices for their top preferences.
          </p>
          <h3 className="govuk-heading-s">Regional variation</h3>
          <p>
            There's much less regional variation in the proportions receiving
            any preferred offer compared to those for receiving a first
            preference as shown in the following chart.
          </p>
          <p>INSERT CHART IN HERE!!!</p>
          <p>
            An applicant can apply for any school, including those situated in
            another local authority (LA).
          </p>
          <p>
            Their authority liaises with the requested school (to make sure the
            applicant is considered under the admissions criteria) and makes the
            offer.
          </p>
          <h3 className="govuk-heading-s">Secondary offers</h3>
          <p>
            In 2018, 91.6% of secondary offers made were from schools inside the
            home authority. This figure has been stable for the past few years.
          </p>
          <p>
            This release concentrates on the headline figures for the proportion
            of children receiving their first preference or a preferred offer.
          </p>
          <p>However, the main tables provide more information including:</p>
          <ul className="govuk-list-bullet">
            <li>the number of places available</li>
            <li>
              the proportion of children for whom a preferred offer was not
              received
            </li>
            <li>
              whether applicants were provided with offers inside or outside
              their home authority
            </li>
          </ul>
        </AccordionSection>

        <AccordionSection heading="Primary applications and offers ">
          <h3 className="govuk-heading-s">Primary applications</h3>
          <p>
            The number of applications received for primary school places
            decreased to 608,180 - down 2% on 2017 (620,330).
          </p>
          <p>
            This is the result of a notable fall in births since 2013 which is
            now feeding into primary school applications.
          </p>
          <p>
            The number of primary applications is the lowest seen since 2013 -
            when this data was first collected.
          </p>
          <h3 className="govuk-heading-s">Primary offers</h3>
          <p>
            The proportion of primary applicants receiving an offer of their
            first-choice school has increased to 91% - up from 90% in 2017.
          </p>
          <p>
            The proportion of applicants who received an offer of any of their
            offer of any of their preferences has also increased slightly to
            98.1% - up from 97.7% in 2017.
          </p>
          <h3 className="govuk-heading-s">Primary National Offer Day</h3>
          <p>
            These statistics come from the process undertaken by local
            authorities (LAs) which enabled them to send out offers of primary
            school places to all applicants on the{' '}
            <a href="/glossary#national-offer-day">
              Primary National Offer Day
            </a>{' '}
            of 16 April 2018.
          </p>
          <p>
            The primary figures have been collected and published since 2014 and
            can be viewed as a time series in the following table.
          </p>
          <p>
            INSERT - Table B: Timeseries of key primary preference rates,
            England Entry into academic year
          </p>
        </AccordionSection>
        <AccordionSection heading="Primary geographical variation ">
          <h3 className="govuk-heading-s">First preference rates</h3>
          <p>
            At local authority (LA) level, the 3 highest first preference rates
            were achieved by the following local authorities:
          </p>
          <ul className="govuk-list-bullet">
            <li>East Riding of Yorkshire - 97.6%</li>
            <li>Northumberland - 97.4%</li>
            <li>Rutland - 97.4%</li>
          </ul>
          <p>
            These authorities are in the top 3 for the first time since 2015.
          </p>
          <p>The lowest first preference rates were all in London.</p>
          <ul className="govuk-list-bullet">
            <li>Kensington and Chelsea - 68.4%</li>
            <li>Camden - 76.5%</li>
            <li>Hammersmith and Fulham - 76.6%</li>
          </ul>
          <p>
            Hammersmith and Fulham and Kensington and Chelsea have both been in
            the bottom 3 since 2015.
          </p>
          <p>
            Although overall results are better at primary level than at
            secondary, for London as a whole the improvement is much more
            marked:
          </p>
          <ul className="govuk-list-bullet">
            <li>
              primary first preference rate increased to 86.6% - up from 85.9%
              in 2017
            </li>
            <li>
              secondary first preference rate decreased to 66% - down from 68.%
              in 2017
            </li>
          </ul>
          <p>INSERT CHART HERE!!!</p>
          <h3 className="govuk-heading-s">Primary offers</h3>
          <p>
            In 2018, 97.1% of pimary offers made were from schools inside the
            home authority. This figure has been stable since 2014 when this
            data was first collected and published.
          </p>
          <p>
            As in previous years, at primary level a smaller proportion of
            offers were made of schools outside the applicant’s home authority
            compared to secondary level.
          </p>
        </AccordionSection>
      </Accordion>

      <h2 className="govuk-heading-m govuk-!-margin-top-9">Help and support</h2>

      <Accordion id="extra-information-exclusions-sections">
        <AccordionSection
          heading="Secondary and primary school applications and offers: methodology"
          caption="Find out how and why we collect, process and publish these statistics"
          headingTag="h3"
        >
          <p>
            Read our{' '}
            <a href="#">
              Secondary and primary school applications and offers: methodology
            </a>{' '}
            guidance.
          </p>
        </AccordionSection>

        <AccordionSection heading="About these statistics">
          <p className="govuk-body">
            The statistics and data cover the number of offers made to
            applicants for primary and secondary school places and the
            proportion which have received their preferred offers.
          </p>
          <p className="govuk-body">
            The data was collected from local authorities (LAs) where it was
            produced as part of the annual applications and offers process for
            applicants requiring a primary or secondary school place in
            September 2018.
          </p>
          <p>
            The offers were made, and data collected, based on the National
            Offer Days of 1 March 2018 for secondary schools and 16 April 2018
            for primary schools.
          </p>
        </AccordionSection>

        <AccordionSection
          heading="Official OR National Statistics"
          headingTag="h3"
        >
          <p className="govuk-body">
            The{' '}
            <a href="https://www.statisticsauthority.gov.u">
              UK Statistics Authority
            </a>{' '}
            designated these statistics as National Statistics in [INSERT MONTH
            YEAR] in accordance with the{' '}
            <a href="https://www.legislation.gov.uk/ukpga/2007/18/contents">
              Statistics and Registration Service Act 2007
            </a>
            .
          </p>
          <p className="govuk-body">
            Designation signifies their compliance with the authority's{' '}
            <a href="https://www.statisticsauthority.gov.uk/code-of-practice/the-code/">
              Code of Practice for Statistics
            </a>{' '}
            which broadly means these statistics are:
          </p>
          <ul className="govuk-list govuk-list--bullet">
            <li>managed impartially and objectively in the public interest</li>
            <li>meeting identified user needs</li>
            <li>produced according to sound methods</li>
            <li>well-explained and readily accessible</li>
          </ul>
          <p className="govuk-body">
            Once designated as National Statistics it's a statutory requirement
            for statistics to ffollow and comply with the Code of Practice for
            Statistics.
          </p>
          <p>
            Find out more about the standards we follow to produce these
            statistics through our{' '}
            <a href="https://www.gov.uk/government/publications/standards-for-official-statistics-published-by-the-department-for-education">
              Standards for official statistics published by DfE
            </a>{' '}
            guidance.
          </p>
        </AccordionSection>

        <AccordionSection heading="Contact us" headingTag="h3">
          <h4 className="govuk-heading-s govuk-!-margin-bottom-0">
            School absence and exclusions team
          </h4>
          <p className="govuk-!-margin-top-0">
            If you have a specific enquiry about absence and exclusion
            statistics and data
          </p>
          <div className="govuk-inset-text">
            <p className="govuk-!-margin-top-0">
              Email:{' '}
              <a href="mailto:school.preference@education.gov.uk">
                school.preference@education.gov.uk
              </a>
            </p>

            <p>
              Telephone: [INSERT NAME OF STATISTICIAN]
              <br />
              020 7783 8553
            </p>
          </div>

          <h4 className="govuk-heading-s govuk-!-margin-bottom-0">
            Press office
          </h4>
          <p className="govuk-!-margin-top-0">If you have a media enquiry:</p>
          <p>
            Telephone <br />
            020 7925 6789
          </p>
          <h4 className="govuk-heading-s govuk-!-margin-bottom-0">
            Public enquiries
          </h4>
          <p className="govuk-!-margin-top-0">
            If you have a general enquiry about the Department for Education
            (DfE) or education:
          </p>
          <p>
            Telephone <br />
            0370 000 2288
          </p>
        </AccordionSection>
      </Accordion>

      <h2 className="govuk-heading-m govuk-!-margin-top-9">
        Create your own tables online
      </h2>
      <p>Use our tool to build tables using national and regional data.</p>
      <Link to="/prototypes/table-tool" className="govuk-button">
        Create tables
      </Link>
      <div className="govuk-!-margin-top-9">
        <a href="#print" className="govuk-link">
          Print this page
        </a>
      </div>
    </PrototypePage>
  );
};

export default PublicationPageExclusions;
