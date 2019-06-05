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
                All figures refer to the <strong>2016/17 academic year</strong>{' '}
                - unless otherwise stated.
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
              <a href="#">Notify me</a>
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
          <h3 className="govuk-heading-s">SOME SORT OF HEADLINE???</h3>{' '}
          <p className="govuk-body">
            These figures come from the application and offer process undertaken
            by local authorities to enable them to send out offers of a place in
            a secondary school to all applicants on the national offer day of 1
            March 2018. The secondary figures have been collected since 2008 and
            a time series of the key figures has been provided below. 582,761
            applications were received for a place at secondary school in 2018,
            a 3.6% increase on 2017. This continues the increase first seen
            since 2013, when the rise in births which began in the previous
            decade started to reach secondary school age, and follows a 2.6%
            increase between 2016 and 2017. Since 2013, when secondary
            applications were at their lowest, there has been a 16.6% increase
            in the number of parents applying for a secondary school place for
            their child. The proportion of secondary applicants receiving an
            offer of their first choice school has dropped from 83.5% in 2017 to
            82.1% in 2018. The proportion of applicants who received an offer of
            any of their preferred schools also dropped slightly to 95.5% in
            2018 (from 96.1%).
          </p>
          <p>
            INSERT - Table A: Timeseries of key secondary preference rates,
            England
          </p>
        </AccordionSection>

        <AccordionSection heading="Secondary geographical variation">
          <p className="govuk-body">
            At local authority level Northumberland (98.1%), East Riding of
            Yorkshire (96.7%) and Bedford (96.4%) achieved the three best first
            preference rates in 2018. Northumberland has been the top performer
            in this measure for the last three years. As in previous years, the
            lowest first preference rates at secondary level are all in London.
            Omitting City of London (which has a tiny number of application and
            has no secondary schools), Hammersmith & Fulham (51.4%), Kensington
            & Chelsea (54.3%) and Lambeth (55.2%) achieved the lowest rates in
            2018. Hammersmith & Fulham has had the lowest first preference rate
            for the last three years. The higher number of practical options
            available to London applicants and ability to name six preferences
            may encourage parents to make more speculative choices for their top
            preferences. There is much less regional variation in the
            proportions receiving any preferred offer compared to those for
            receiving a first preference (see chart below).
          </p>
          <p>INSERT CHART IN HERE!!!</p>
          <p>
            An applicant can apply for any school, including those situated in
            another local authority. Their authority liaises with the requested
            school, to ensure the applicant is considered under the admissions
            criteria, and makes the offer. In 2018 91.6% of secondary offers
            made were of schools inside the home authority. This figure has been
            stable for the past few years. This release concentrates on the
            headline figures for the proportion of children receiving their
            first preference or a preferred offer. However, the main tables
            provide more information such as the number of places available, the
            proportion of children for whom a preferred offer was not received
            and whether the applicants were provided with offers within or
            outside their home authority.
          </p>
        </AccordionSection>

        <AccordionSection heading="Primary applications and offers ">
          <h3 className="govuk-heading-s">
            Enrolments with one or more fixed-period exclusion definition
          </h3>{' '}
          <p>
            The primary table is based on the offers made by local authorities
            on the primary national offer day of 16 April 2018. This national
            offer day was introduced in 2014, which was the first year that
            primary application and offer data were collected and published. A
            timeseries of the key figures has been provided below. The number of
            applications for a primary school place in 2018 was 608,180, 2.0%
            lower than in 2017 (620,330). This decrease is a result of the
            notable drop in births in 2013, which is now feeding into primary
            school applications. The number of applications is the lowest seen
            in the five years that this data has been collected. The proportion
            of applicants receiving their first preference offer has risen to
            91.0%, up 1.0 percentage point on 2017. The proportion receiving an
            offer of any of their preferences has also increased slightly, from
            97.7% (2017) to 98.1%.
          </p>
          <p>
            INSERT - Table B: Timeseries of key primary preference rates,
            England Entry into academic year
          </p>
        </AccordionSection>
        <AccordionSection heading="Primary geographical variation ">
          <p>
            At local authority level East Riding of Yorkshire (97.6%),
            Northumberland (97.4%) and Rutland (97.4%) achieved the best first
            preference rates in 2018. All these authorities are in the top three
            for the first time in the last three years. The local authorities
            with the worst first preference rates in 2018 were Kensington &
            Chelsea with 68.4%, Camden (76.5%) and Hammersmith & Fulham (76.6%).
            Both Kensington & Chelsea and Hammersmith & Fulham have been in the
            bottom three performers in this measure for the last three years.
            Although overall results are better at primary level than at
            secondary, for London local authorities the improvement is much more
            marked. In 2018, for London as a whole, the first preference rate at
            primary level was 86.6% (85.9% in 2017), compared to 66.0% at
            secondary level (68.2% in 2017).
          </p>
          <p>INSERT CHART HERE!!!</p>
          <p>
            As in previous years, at primary level a smaller proportion of
            offers were made of schools outside the applicant’s local authority
            compared to secondary level. In 2018 97.1% of offers were inside the
            home authority. This has remained stable for the five years for
            which primary data is available.
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
            The statistics and data cover permanent and fixed period exclusions
            and school-level exclusions during the 2016/17 academic year in the
            following state-funded school types as reported in the school
            census:
          </p>
          <ul className="govuk-list-bullet">
            <li>primary schools</li>
            <li>secondary schools</li>
            <li>special schools</li>
          </ul>
          <p className="govuk-body">
            They also include national-level information on permanent and
            fixed-period exclusions for{' '}
            <a href="/glossary#pupil-referral-unit">pupil referral units</a>.
          </p>
          <p>
            All figures are based on unrounded data so constituent parts may not
            add up due to rounding.
          </p>
          <p>
            This statistical release provides the number of offers made to
            applicants for both secondary and primary school places and the
            proportion which have received preferred offers (1st, 2nd, 3rd
            preference etc). The data is collected from local authorities, where
            it is produced as part of the annual application and offer process
            for applicants requiring a place to start at secondary or at primary
            school in September 2018. The offers were made, and data collected,
            based on the national offer days of 1 March 2018 (secondary) and 16
            April 2018 (primary).
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
