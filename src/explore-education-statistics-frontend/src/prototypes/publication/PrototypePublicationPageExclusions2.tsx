import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import Details from '@common/components/Details';
import Link from '@frontend/components/Link';
import PrototypePage from '@frontend/prototypes/components/PrototypePage';
import React from 'react';
import PrototypeDataSample from './components/PrototypeDataSampleExclusions';

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
        Permanent and fixed-period exclusions statistics for schools in England
        - V2
      </h1>
      <dl className="dfe-meta-content">
        <dt className="govuk-caption-m">Published: </dt>
        <dd>
          <strong>22 March 2018</strong>
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
                <Link to="/prototypes/methodology-absence2">
                  Permanent and fixed-period exclusions statistics: methodology
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
                For academic year:{' '}
              </span>
              2016 to 2017 (latest data)
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
              <span className="govuk-caption-m">Last updated: </span>6 August
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
              <span className="govuk-caption-m">Next update: </span>19 July 2019
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
                    Permanent and fixed-period exclusions statistics:
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
        Headline facts and figures - 2016/2017 academic year
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
        <AccordionSection heading="Permanent exclusions">
          <div className="govuk-text">
            <p>
              The number of{' '}
              <a href="/glossary#permanent-exclusion">permanent exclusions</a>
              has increased across all state-funded primary, secondary and
              special schools to 7,720 - up from 6,685 in 2015/16.
            </p>
            <p>
              This works out to an average 40.6 permanent exclusions per day -
              up from an 35.2 per day in 2015/16.
            </p>
            <p>
              The permanent exclusion rate has also increased to 0.10% of pupils
              - up from from 0.08% in 2015/16 - which is equivalent to around 10
              pupils per 10,000.
            </p>
          </div>
          <PrototypeDataSample
            sectionId="permanentExclusions"
            chartTitle="permanent exclusions in England"
            xAxisLabel="School Year"
            yAxisLabel="Permanent exclusions"
            chartData={[
              {
                name: '2012/13',
                primary: 4.7,
                'primary and secondary': 5.9,
                secondary: 7.3,
              },
              {
                name: '2013/14',
                primary: 3.9,
                'primary and secondary': 4.3,
                secondary: 5.0,
              },
              {
                name: '2014/15',
                primary: 4.6,
                'primary and secondary': 5.8,
                secondary: 7.1,
              },
              {
                name: '2015/16',
                primary: 3.8,
                'primary and secondary': 4.0,
                secondary: 4.6,
              },
              {
                name: '2016/17',
                primary: 4.7,
                'primary and secondary': 5.8,
                secondary: 7.1,
              },
            ]}
            chartDataKeys={['primary', 'secondary', 'primary and secondary']}
          />

          <p>
            Most occurred in secondary schools which accounted for 83% of all
            permanent exclusions.
          </p>
          <p>
            The <a href="/glossary#permanent-exclusion">permanent exclusion</a>
            rate in secondary schools increased 0.20% - up from from 0.17% in
            2015/16 - which is equivalent to 20 pupils per 10,000.
          </p>
          <p>
            The rate also rose in primary schools to 0.03% but decreased in
            special schools to 0.07% - down from from 0.08% in 2015/16.
          </p>
          <p>
            The rate generally followed a downward trend after 2006/07 - when it
            stood at 0.12%.
          </p>
          <p>
            However, since 2012/13 it has been on the rise although rates are
            still lower now than in 2006/07.
          </p>
        </AccordionSection>

        <AccordionSection heading="Fixed-period exclusions">
          <div className="govuk-text">
            <p>
              The number of{' '}
              <a href="/glossary#fixed-period-exclusion">
                {' '}
                fixed-period exclusions
              </a>
              has increased across all state-funded primary, secondary and
              special schools to 381,865 - up from 339,360 in 2015/16.
            </p>
            <p>
              This works out to around 2,010 fixed-period exclusions per day -
              up from an 1,786 per day in 2015/16.
            </p>
          </div>
          <PrototypeDataSample
            sectionId="fixedPeriodExclusions"
            chartTitle="fixed-period exclusion rates in England"
            xAxisLabel="School Year"
            yAxisLabel="Fixed-period exclusion rate"
            chartData={[
              {
                name: '2012/13',
                primary: 14.7,
                'primary and secondary': 18.9,
                secondary: 23.3,
              },
              {
                name: '2013/14',
                primary: 13.9,
                'primary and secondary': 18.3,
                secondary: 22.0,
              },
              {
                name: '2014/15',
                primary: 14.6,
                'primary and secondary': 18.8,
                secondary: 24.1,
              },
              {
                name: '2015/16',
                primary: 13.8,
                'primary and secondary': 18.0,
                secondary: 22.6,
              },
              {
                name: '2016/17',
                primary: 14.7,
                'primary and secondary': 18.9,
                secondary: 24.1,
              },
            ]}
            chartDataKeys={['primary', 'secondary', 'primary and secondary']}
          />
          <h3 className="govuk-heading-s">Primary schools</h3>
          <ul className="govuk-list-bullet">
            <li>
              fixed-period exclusions numbers increased to 64,340 - up from
              55,740 in 2015/16
            </li>
            <li>
              fixed-period exclusions rate increased to 1.37% - up from 1.21% in
              2015/16
            </li>
          </ul>
          <h3 className="govuk-heading-s">Secondary schools</h3>
          <ul className="govuk-list-bullet">
            <li>
              fixed-period exclusions numbers increased to 302,890 - up from
              270,135 in 2015/16
            </li>
            <li>
              fixed-period exclusions rate increased to 9.4% - up from 8.46% in
              2015/16
            </li>
          </ul>
          <h3 className="govuk-heading-s">Special schools</h3>
          <ul className="govuk-list-bullet">
            <li>
              fixed-period exclusions numbers increased to 14,635 - up from
              13,485 in 2015/16
            </li>
            <li>
              fixed-period exclusions rate increased to 13.03% - up from 12.53%
              in 2015/16
            </li>
          </ul>
        </AccordionSection>

        <AccordionSection heading="Number and length of fixed-period exclusions">
          <div className="govuk-text">
            <h3 className="govuk-heading-s">
              Pupils with one or more fixed-period exclusion definition
            </h3>
            <p>
              The number of pupils with{' '}
              <a href="/glossary#one-or-more-fixed-period-exclusion">
                one or more fixed-period exclusion
              </a>
              has increased across state-funded primary, secondary and special
              schools to 183,475 (2.29% of pupils) up from 167,125 (2.11% of
              pupils) in 2015/16.
            </p>
            <p>
              Of these kinds of pupils, 59.1% excluded on only 1 occasion while
              1.5% received 10 or more fixed-period exclusions during the year.
            </p>
            <p>
              The percentage of pupils who went on to receive a{' '}
              <a href="/glossary#permanent-exclusion">permanent exclusion</a>
              was 3.5%.
            </p>
            <p>
              The average length of{' '}
              <a href="/glossary#fixed-period-exclusion">
                fixed-period exclusions
              </a>
              across schools decreased to 2.1 days - slightly shorter than in
              2015/16.
            </p>
            <p>
              The highest proportion of fixed-period exclusions (46.6%) lasted
              for only 1 day.
            </p>
            <p>
              Only 2.0% of fixed-period exclusions lasted for longer than 1 week
              and longer fixed-period exclusions were more prevalent in
              secondary schools.
            </p>
          </div>
        </AccordionSection>
        <AccordionSection heading="Reasons for exclusions">
          <p>
            All reasons (except bullying and theft) saw an increase in{' '}
            <a href="/glossary#permanent-exclusion">permanent exclusions</a>{' '}
            since 2015/16.
          </p>
          <p>The following most common reasons saw the largest increases:</p>
          <ul className="govuk-list-bullet">
            <li>physical assault against a pupil</li>
            <li>persistent disruptive behaviour</li>
            <li>other reasons</li>
          </ul>
          <h3 className="govuk-heading-s">Persistent disruptive behaviour</h3>
          <p>
            Remained the most common reason for permanent exclusions accounting
            for 2,755 (35.7%) of all permanent exclusions - which is equivalent
            to 3 permanent exclusions per 10,000 pupils.
          </p>
          <p>
            However, in special schools the most common reason for exclusion was
            physical assault against an adult - accounting for 37.8% of all
            permanent exclusions and 28.1% of all
            <a href="/glossary#fixed-period-exclusion">
              fixed-period exclusions
            </a>
            .
          </p>
          <p>
            Persistent disruptive behaviour is also the most common reason for
            fixed-period exclusions accounting for 108,640 %) of all
            fixed-period exclusions - up from 27.7% in 2015/16. This is
            equivalent to around 135 fixed-period exclusions per 10,000 pupils.
          </p>
          <p>
            All reasons saw an increase in fixed-period exclusions since
            2015/16. Persistent disruptive behaviour and other reasons saw the
            largest increases.
          </p>
        </AccordionSection>
        <AccordionSection heading="Exclusions by pupil characteristics">
          <p>
            We saw a similar pattern to previous years where the following
            groups (ones where we usually expect to see higher exclusion rates)
            showing an increase in exclusions since 2015/16:
          </p>
          <ul className="govuk-list-bullet">
            <li>boys</li>
            <li>national curriculum years 9 and 10</li>
            <li>pupils with special educational needs (SEN)</li>
            <li>
              pupils known to be eligible for and claiming free school meals
              (FSM)
            </li>
          </ul>
          <h3 className="govuk-heading-s">
            Age, national curriculum year group and gender
          </h3>
          <ul className="govuk-list-bullet">
            <li>
              more than half of all{' '}
              <a href="/glossary#permanent-exclusion">permanent exclusions</a>
              (57.2%) and{' '}
              <a href="/glossary#fixed-period-exclusion">
                fixed-period exclusions
              </a>{' '}
              (52.6 %) occur in national curriculum year 9 or above
            </li>
            <li>
              a quarter (25%) of all permanent exclusions were for pupils aged
              14 - who also had the highest rates for fixed-period exclusion and
              pupils receiving{' '}
              <a href="/glossary#one-or-more-fixed-period-exclusion">
                one or more fixed-period exclusion
              </a>
            </li>
            <li>
              the permanent exclusion rate for boys (0.15%) was more than 3
              times higher than for girls (0.04%)
            </li>
            <li>
              the fixed-period exclusion rate for boys (6.91%) was almost 3
              times higher than for girls (2.53%)
            </li>
          </ul>
          <h3 className="govuk-heading-s">
            Pupils eligible for and claiming free school meals (FSM)
          </h3>
          <ul className="govuk-list-bullet">
            <li>
              had a permanent exclusion rate of 0.28% and fixed period exclusion
              rate of 12.54% - around 4 times higher than those not eligible for
              FSM at 0.07% and 3.50% respectively
            </li>
            <li>
              accounted for 40% of all permanent exclusions and 36.7% of all
              fixed-period exclusions
            </li>
          </ul>
          <h3 className="govuk-heading-s">
            Special educational needs (SEN) pupils
          </h3>
          <ul className="govuk-list-bullet">
            <li>
              accounted for around half of all permanent exclusions (46.7%) and
              fixed-period exclusions (44.9%)
            </li>
            <li>
              had the highest permanent exclusion rate (0.35%0 - 6 times higher
              than the rate for pupils with no SEN (0.06%)
            </li>
            <li>
              pupils with a statement of SEN or education, health and care (EHC)
              plan had the highest fixed-period exclusion rate at 15.93% - more
              than 5 times higher than pupils with no SEN (3.06%)
            </li>
          </ul>
          <h3 className="govuk-heading-s">Ethnic group</h3>
          <ul className="govuk-list-bullet">
            <li>
              pupils of Gypsy/Roma and Traveller of Irish Heritage ethnic groups
              had the highest rates of permanent and fixed-period exclusions -
              but as the population is relatively small these figures should be
              treated with some caution
            </li>
            <li>
              pupils from a Black Caribbean background had a permanent exclusion
              rate nearly 3 times higher (0.28%) than the school population as a
              whole (0.10%)
            </li>
            <li>
              pupils of Asian ethnic groups had the lowest permanent and
              fixed-period exclusion rates
            </li>
          </ul>
        </AccordionSection>

        <AccordionSection heading="Independent exclusion reviews">
          <p>
            There were 560 reviews lodged with{' '}
            <a href="/glossary#independent-review-panels">
              independent review panels
            </a>
            in maintained primary, secondary and special schools and academies
            of which 525 (93.4%) were determined and 45 (8.0%) resulted in an
            offer of reinstatement.
          </p>
        </AccordionSection>
        <AccordionSection heading="Pupil referral unit exclusions">
          <h3 className="govuk-heading-s">Permanent exlcusion</h3>
          <p>
            The <a href="/glossary#permanent-exclusion">permanent exclusion</a>
            rate in{' '}
            <a href="/glossary#pupil-referral-unit">pupil referral units</a>
            decreased to 0.13 - down from 0.14% in 2015/16.
          </p>
          <p>
            Permanent exclusions rates have remained fairly steady following an
            increase between 2013/14 and 2014/15.
          </p>
          <h3 className="govuk-heading-s">Fixed-period exlcusion</h3>
          <p>
            The{' '}
            <a href="/glossary#fixed-period-exclusion">
              fixed period exclusion
            </a>{' '}
            rate has been steadily increasing since 2013/14.
          </p>
          <p>
            The percentage of pupils in pupil referral units who 1 or more
            fixed-period exclusion increased to 59.17% - up from 58.15% in
            2015/16.
          </p>
        </AccordionSection>
        <AccordionSection heading="Regional and local authority (LA) breakdown">
          <p>
            There's considerable variation in the{' '}
            <a href="/glossary#permanent-exclusion">permanent exclusion</a>
            and{' '}
            <a href="/glossary#fixed-period-exclusion">
              fixed period exclusion
            </a>{' '}
            rate at the LA level.
          </p>
          <h3 className="govuk-heading-s">Permanent exclusion</h3>
          <p>
            Similar to 2015/16, the regions with the joint-highest rates across
            all school types were:
          </p>
          <ul className="govuk-list-bullet">
            <li>North West - 0.14%</li>
            <li>North West - 0.14%</li>
          </ul>
          <p>Similar to 2015/16, the regions with the lowest rates were:</p>
          <ul className="govuk-list-bullet">
            <li>the South East - 0.06%</li>
            <li>Yorkshire and the Humber - 0.07%</li>
          </ul>
          <h3 className="govuk-heading-s">Fixed-period exclusion</h3>
          <p>
            Similar to 2015/16, the region with the highest rates across all
            school types was Yorkshire and the Humber at 7.22% while the lowest
            rate was in Outer London (3.49%).
          </p>
        </AccordionSection>
      </Accordion>

      <h2 className="govuk-heading-m govuk-!-margin-top-9">Help and support</h2>

      <Accordion id="extra-information-exclusions-sections">
        <AccordionSection
          heading="Permanent and fixed-period exclusions statistics: methodology"
          caption="Find out how and why we collect, process and publish these statistics"
          headingTag="h3"
        >
          <p>
            Read our{' '}
            <a href="/prototypes/methodology-absence2">
              Permanent and fixed-period exclusions statistics: methodology
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
        </AccordionSection>

        <AccordionSection heading="National Statistics" headingTag="h3">
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
              <a href="mailto:schools.statistics@education.gov.uk">
                schools.statistics@education.gov.uk
              </a>
            </p>

            <p>
              Telephone: Mark Pearson
              <br />
              0114 274 2585
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
            (DfE) or education
          </p>
          <p>
            Telephone
            <br />
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
