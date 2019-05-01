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
        This is the latest data{' '}
      </strong>
      <h1 className="govuk-heading-xl">
        Permanent and fixed-period exclusions statistics in England
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
                Read national statistical summaries and definitions, view charts
                and tables and download data files across a range of permanent
                and fixed-period exclusion subject areas.
              </p>
              <p className="govuk-body">
                You can also view a regional breakdown of statistics and data
                within the{' '}
                <a href="#contents-exclusions-sections-heading-9">
                  <strong>local authorities section</strong>
                </a>
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

          <p>
            <Link to="#">
              Find out more about our permanent and fixed-period exclusions data
              and statistics methodology and terminology
            </Link>
          </p>
          <Details summary="Download underlying data files">
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
                <a href="#" className="govuk-link">
                  Access API
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
                For school year:{' '}
              </span>
              2016/17 (latest data)
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
              Related content
            </h2>
            <nav role="navigation" aria-labelledby="related-content">
              <ul className="govuk-list">
                <li>
                  <Link to="/prototypes/methodology-absence">
                    Permanent and fixed-period exclusions: guidance and
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
        Latest headline facts and figures - 2016/17
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

      <h2 className="govuk-heading-l">Contents</h2>
      <Accordion id="contents-exclusions-sections">
        <AccordionSection heading="About this release">
          <p className="govuk-body">
            This National Statistics release reports on permanent and fixed
            period exclusions from state-funded primary, state-funded secondary
            and special schools during the 2016/17 academic year as reported in
            the School Census. This release also includes school level
            exclusions figures for state-funded primary, secondary and special
            schools and national level figures on permanent and fixed-period
            exclusions from pupil referral units. All figures in this release
            are based on unrounded data; therefore, constituent parts may not
            add up due to rounding.
          </p>
          <p className="govuk-body">
            An Exclusions statistics guide, which provides historical
            information on exclusion statistics, technical background
            information to the figures and data collection, and definitions of
            key terms should be referenced alongside this release.
          </p>
          <div className="govuk-bo">
            In this publication: The following tables are included in the
            statistical publication
          </div>
          <ul className="govuk-list-bullet">
            <li>national tables (Excel .xls and open format)</li>
            <li>local authority (LA) tables</li>
            <li>underlying data (open format .csv and metadata .txt)</li>
          </ul>
          <p className="govuk-body">
            The underlying data is accompanied by a metadata document that
            describes underlying data files.
          </p>
          <p className="govuk-body">
            We welcome feedback on any aspect of this document at{' '}
            <a href="#">schools.statistics@education.gov.uk</a>
          </p>
        </AccordionSection>

        <AccordionSection heading="Permanent exclusions">
          <div className="govuk-inset-text">
            <h3 className="govuk-heading-s">
              Permanent exclusion rate definition
            </h3>{' '}
            <p className="govuk-body">
              A permanent exclusion refers to a pupil who is excluded and who
              will not come back to that school (unless the exclusion is
              overturned). The number of permanent exclusions across all
              state-funded primary, secondary and special schools has increased
              from 6,685 in 2015/16 to 7,720 in 2016/17. This corresponds to
              around 40.6 permanent exclusions per day in 2016/17, up from an
              average of 35.2 per day in 2015/16.
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
            The rate of permanent exclusions across all state-funded primary,
            secondary and special schools has also increased from 0.08 per cent
            to 0.10 per cent of pupil enrolments, which is equivalent to around
            10 pupils per 10,000.
          </p>
          <p>
            Most (83 per cent) permanent exclusions occurred in secondary
            schools. The rate of permanent exclusions in secondary schools
            increased from 0.17 per cent in 2015/16 to 0.20 per cent in 2016/17,
            which is equivalent to around 20 pupils per 10,000.
          </p>
          <p>
            The rate of permanent exclusions also rose in primary schools, at
            0.03 per cent, but decreased in special schools from 0.08 per cent
            in 2015/16 to 0.07 per cent in 2016/17. Looking at longer-term
            trends, the rate of permanent exclusions across all state-funded
            primary, secondary and special schools followed a generally downward
            trend from 2006/07 when the rate was 0.12 per cent until 2012/13,
            and has been rising again since then, although rates are still lower
            now than in 2006/07.
          </p>
        </AccordionSection>

        <AccordionSection heading="Fixed-period exclusions">
          <div className="govuk-inset-text">
            <h3 className="govuk-heading-s">
              Fixed-period exclusion rate definition
            </h3>{' '}
            <p className="govuk-body">
              Fixed-period exclusion refers to a pupil who is excluded from a
              school for a set period of time. A fixed-period exclusion can
              involve a part of the school day and it does not have to be for a
              continuous period. A pupil may be excluded for one or more fixed
              periods up to a maximum of 45 school days in a single academic
              year. This total includes exclusions from previous schools covered
              by the exclusion legislation. A pupil may receive more than one
              fixed-period exclusion, so pupils with repeat exclusions can
              inflate fixed-period exclusion rates.
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
          <p>
            The number of fixed-period exclusions across all state-funded
            primary, secondary and special schools has increased from 339,360 in
            2015/16 to 381,865 in 2016/17. This corresponds to around 2,010
            fixed-period exclusions per day1 in 2016/17, up from an average of
            1,786 per day in 2015/16.
          </p>
          <p>
            There were increases in the number and rate of fixed-period
            exclusions for state-funded primary and secondary schools and
            special schools:
          </p>
          <ul className="govuk-list-bullet">
            <li>
              in state-funded primary schools, the number of fixed-period
              exclusions increased from 55,740 in 2015/16 to 64,340 in 2016/17,
              whilst the rate of fixed-period exclusions increased from 1.21 per
              cent to 1.37 per cent
            </li>
            <li>
              in state-funded secondary schools, the number of fixed-period
              exclusions increased from 270,135 in 2015/16 to 302,890 in
              2016/17, which corresponded to an increase in the rate of fixed
              period exclusions from 8.46 per cent to 9.4 per cent
            </li>
            <li>
              in special schools, the number of fixed-period exclusions
              increased in special schools from 13,485 in 2015/16 to 14,635 in
              2016/17, an increase in the rate of fixed-period exclusions from
              12.53 per cent to 13.03 per cent
            </li>
          </ul>
        </AccordionSection>

        <AccordionSection heading="Number and length of fixed-period exclusions">
          <div className="govuk-inset-text">
            <h3 className="govuk-heading-s">
              Enrolments with one or more fixed-period exclusion definition
            </h3>{' '}
            <p>
              Pupils with one or more fixed-period exclusion refer to pupil
              enrolments that had at least one fixed-period exclusion across the
              full academic year. It includes those with repeated fixed-period
              exclusions.
            </p>
          </div>
          <p>
            In state-funded primary, secondary and special schools, there were
            183,475 pupil enrolments, 2.29 per cent, with at least one fixed
            term exclusion in 2016/17, up from 167,125 pupil enrolments, 2.11
            per cent, in 2015/16.
          </p>
          <p>
            Of those pupils with at least one fixed-period exclusion, 59.1 per
            cent were excluded only on one occasion, and 1.5 per cent received
            10 or more fixed-period exclusions during the year. The percentage
            of pupils with at least one fixed-period exclusion that went on to
            receive a permanent one was 3.5 per cent.
          </p>
          <p>
            The average length of fixed-period exclusions across state-funded
            primary, secondary and special schools in 2016/17 was 2.1 days,
            slightly shorter than in 2015/16.
          </p>
          <p>
            The highest proportion of fixed-period exclusions (46.6 per cent)
            lasted for only one day. Only 2.0 per cent of fixed-period
            exclusions lasted for longer than one week and longer fixed-period
            exclusions were more prevalent in secondary schools.
          </p>
        </AccordionSection>
        <AccordionSection heading="Reasons for exclusions">
          <p>
            Persistent disruptive behaviour remained the most common reason for
            permanent exclusions in state-funded primary, secondary and special
            schools - accounting for 2,755 (35.7 per cent) of all permanent
            exclusions in 2016/17. This is equivalent to 3 permanent exclusions
            per 10,000 pupils. However, in special schools alone, the most
            common reason for exclusion was physical assault against and adult,
            which made up 37.8 per cent of all permanent exclusions and 28.1 per
            cent of all fixed-period exclusions.
          </p>
          <p>
            All reasons except bullying and theft saw an increase in permanent
            exclusions since last year. The most common reasons - persistent
            disruptive behaviour, physical assault against a pupil and other
            reasons had the largest increases.
          </p>
          <p>
            Persistent disruptive behaviour is also the most common reason for
            fixed-period exclusions. The 108,640 fixed-period exclusions for
            persistent disruptive behaviour in state-funded primary, secondary
            and special schools made up 28.4 per cent of all fixed-period
            exclusions, up from 27.7 per cent in 2015/16. This is equivalent to
            around 135 fixed-period exclusions per 10,000 pupils.
          </p>
          <p>
            All reasons saw an increase in fixed-period exclusions since last
            year. Persistent disruptive behaviour and other reasons saw the
            biggest increases.
          </p>
        </AccordionSection>
        <AccordionSection heading="Exclusions by pupil characteristics">
          <p>
            In 2016/17 we saw a similar pattern by pupil characteristics to
            previous years. The groups that we usually expect to have higher
            rates are the ones that have increased exclusions since last year
            e.g. boys, pupils with special educational needs, pupils known to be
            eligible for and claiming free school meals and national curriculum
            years 9 and 10.
          </p>
          <strong>Age, national curriculum year group and gender</strong>
          <ul className="govuk-list-bullet">
            <li>
              Over half of all permanent (57.2 per cent) and fixed-period (52.6
              per cent) exclusions occur in national curriculum year 9 or above.
            </li>
            <li>
              A quarter (25.0 per cent) of all permanent exclusions were for
              pupils aged 14, and pupils of this age group also had the highest
              rate of fixed-period exclusion, and the highest rate of pupils
              receiving one or more fixed-period exclusion.
            </li>
            <li>
              The permanent exclusion rate for boys (0.15 per cent) was over
              three times higher than that for girls (0.04 per cent) and the
              fixed-period exclusion rate was almost three times higher (6.91
              compared with 2.53 per cent).
            </li>
          </ul>
          <strong>Free school meals (FSM) eligibility</strong>
          <ul className="govuk-list-bullet">
            <li>
              Pupils known to be eligible for and claiming free school meals
              (FSM) had a permanent exclusion rate of 0.28 per cent and fixed
              period exclusion rate of 12.54 per cent - around four times higher
              than those who are not eligible (0.07 and 3.50 per cent
              respectively).
            </li>
            <li>
              Pupils known to be eligible for and claiming free school meals
              (FSM) accounted for 40.0 per cent of all permanent exclusions and
              36.7 per cent of all fixed-period exclusions. Special educational
              need (SEN)
            </li>
            <li>
              Pupils with identified special educational needs (SEN) accounted
              for around half of all permanent exclusions (46.7 per cent) and
              fixed-period exclusions (44.9 per cent).
            </li>
            <li>
              Pupils with SEN support had the highest permanent exclusion rate
              at 0.35 per cent. This was six times higher than the rate for
              pupils with no SEN (0.06 per cent).
            </li>
            <li>
              Pupils with an Education, Health and Care (EHC) plan or with a
              statement of SEN had the highest fixed-period exclusion rate at
              15.93 per cent - over five times higher than pupils with no SEN
              (3.06 per cent).
            </li>
          </ul>
          <strong>Ethnic group</strong>
          <ul className="govuk-list-bullet">
            <li>
              Pupils of Gypsy/Roma and Traveller of Irish Heritage ethnic groups
              had the highest rates of both permanent and fixed-period
              exclusions, but as the population is relatively small these
              figures should be treated with some caution.
            </li>
            <li>
              Black Caribbean pupils had a permanent exclusion rate nearly three
              times higher (0.28 per cent) than the school population as a whole
              (0.10 per cent). Pupils of Asian ethnic groups had the lowest
              rates of permanent and fixed-period exclusion.
            </li>
          </ul>
        </AccordionSection>
        <AccordionSection heading="Independent exclusion reviews">
          <p>
            Independent review Panel definition: Parents (and pupils if aged
            over 18) are able to request a review of a permanent exclusion. An
            independent review panel’s role is to review the decision of the
            governing body not to reinstate a permanently excluded pupil. The
            panel must consider the interests and circumstances of the excluded
            pupil, including the circumstances in which the pupil was excluded
            and have regard to the interests of other pupils and people working
            at the school.
          </p>
          <p>
            In 2016/17 in maintained primary, secondary and special schools and
            academies there were 560 reviews lodged with independent review
            panels of which 525 (93.4%) were determined and 45 (8.0%) resulted
            in an offer of reinstatement.
          </p>
        </AccordionSection>
        <AccordionSection heading="Exclusions from pupil referral units">
          <p>
            The rate of permanent exclusion in pupil referral units decreased
            from 0.14 per cent in 2015/16 to 0.13 in 2016/17. After an increase
            from 2013/14 to 2014/15, permanent exclusions rates have remained
            fairly steady. There were 25,815 fixed-period exclusions in pupil
            referral units in 2016/17, up from 23,400 in 2015/16. The fixed
            period exclusion rate has been steadily increasing since 2013/14.
          </p>
          <p>
            The percentage of pupil enrolments in pupil referral units who one
            or more fixed-period exclusion was 59.17 per cent in 2016/17, up
            from 58.15 per cent in 2015/16.
          </p>
        </AccordionSection>
        <AccordionSection heading="Exclusions by local authority">
          <p>
            There is considerable variation in the permanent and fixed-period
            exclusion rate at local authority level (see accompanying maps on
            the web page).
          </p>
          <p>
            The regions with the highest overall rates of permanent exclusion
            across state-funded primary, secondary and special schools are the
            West Midlands and the North West (at 0.14 per cent). The regions
            with the lowest rates are the South East (at 0.06 per cent) and
            Yorkshire and the Humber (at 0.07 per cent).
          </p>
          <p>
            The region with the highest fixed-period exclusion rate is Yorkshire
            and the Humber (at 7.22 per cent), whilst the lowest rate was seen
            in Outer London (3.49 per cent).
          </p>
          <p>
            These regions also had the highest and lowest rates of exclusion in
            the previous academic year.
          </p>
        </AccordionSection>
      </Accordion>

      <h2 className="govuk-heading-m govuk-!-margin-top-9">
        Extra information
      </h2>

      <Accordion id="extra-information-exclusions-sections">
        <AccordionSection
          heading="Where does this data come from?"
          caption="Our methodology, how we collect and process the data"
          headingTag="h3"
        >
          <ul className="govuk-list">
            <li>
              <a href="#" className="govuk-link">
                How do we collect it?
              </a>
            </li>
            <li>
              <a href="#" className="govuk-link">
                What do we do with it?
              </a>
            </li>
            <li>
              <a href="#" className="govuk-link">
                Related policies
              </a>
            </li>
          </ul>
        </AccordionSection>
        <AccordionSection heading="National statistics" headingTag="h3">
          <p className="govuk-body">
            The United Kingdom Statistics Authority designated these statistics
            as National Statistics in <a href="#">Month Year</a> in accordance
            with the Statistics and Registration Service Act 2007 and signifying
            compliance with the Code of Practice for Statistics.
          </p>
          <p className="govuk-body">
            Designation can be broadly interpreted to mean that the statistics:
          </p>
          <ul className="govuk-list govuk-list--bullet">
            <li>meet identified user needs;</li>
            <li>are well explained and readily accessible;</li>
            <li>are produced according to sound methods, and</li>
            <li>
              are managed impartially and objectively in the public interest
            </li>
          </ul>
          <p className="govuk-body">
            Once statistics have been designated as National Statistics it is a
            statutory requirement that the Code of Practice shall continue to be
            observed. Information on improvements made to these statistics to
            continue their compliance with the Code of Practice are provided in
            this <a href="#">accompanying document</a>
          </p>
        </AccordionSection>
        <AccordionSection heading="Feedback and questions" headingTag="h3">
          <ul className="govuk-list">
            <li>
              <a href="#" className="govuk-link">
                Feedback on this page
              </a>
            </li>
            <li>
              <a href="#" className="govuk-link">
                Make a suggestion
              </a>
            </li>
            <li>
              <a href="#" className="govuk-link">
                Ask a question
              </a>
            </li>
          </ul>
        </AccordionSection>
        <AccordionSection heading="Contact us" headingTag="h3">
          <p>
            If you have a specific enquiry about absence and exclusion
            statistics and data:
          </p>
          <p>
            <strong>School absence and exclusions team</strong>
          </p>
          <p>Email</p>
          <p>
            <a href="mailto:schools.statistics@education.gov.uk">
              schools.statistics@education.gov.uk
            </a>
          </p>
          <p>Telephone: Mark Pearson</p>
          <p>0114 274 2585</p>

          <h4>Press office</h4>
          <p>If you have a media enquiry:</p>
          <p>Telephone</p>
          <p>020 7925 6789 </p>
          <h4>Public enquiries</h4>
          <p>
            If you have a general enquiry about the Department for Education
            (DfE) or education:
          </p>
          <p>Telephone</p>
          <p>037 0000 2288</p>
        </AccordionSection>
      </Accordion>

      <h2 className="govuk-heading-m govuk-!-margin-top-9">
        Exploring the data
      </h2>
      <p>
        The statistics can be viewed as reports, or you can customise and
        download as excel or .csv files . The data can also be accessed via an
        API. <a href="#">What is an API?</a>
      </p>
      <Link to="/prototypes/table-tool" className="govuk-button">
        Create charts and tables
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
