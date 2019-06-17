import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import Details from '@common/components/Details';
import PrototypeMap from '@common/prototypes/publication/components/PrototypeMap';
import Link from '@frontend/components/Link';
import PrototypePage from '@frontend/prototypes/components/PrototypePage';
import React from 'react';
import PrototypeAbsenceData from './components/PrototypeAbsenceData';
import PrototypeDataSample from './components/PrototypeDataSample';

const PublicationPage = () => {
  let mapRef: PrototypeMap | null = null;

  return (
    <PrototypePage
      breadcrumbs={[
        {
          link: '/prototypes/browse-releases-find',
          text: 'Find statistics and download data',
        },
        { text: 'Pupil absence statistics for schools in England', link: '#' },
      ]}
    >
      <strong className="govuk-tag govuk-!-margin-bottom-2">
        {' '}
        Latest statistics and data{' '}
      </strong>
      <h1 className="govuk-heading-xl">
        Pupil absence statistics for schools in England - V2
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
            </div>
          </div>

          <Details summary="Download data files">
            <ul className="govuk-list">
              <li>
                <a href="#" className="govuk-link">
                  Download data file 1 (.csv)
                </a>
              </li>
              <li>
                <a href="#" className="govuk-link">
                  Download data file 2 (.csv)
                </a>
              </li>
              <li>
                <a href="#" className="govuk-link">
                  Download data file 3 (.csv)
                </a>
              </li>
              <li>
                <a href="#" className="govuk-link">
                  Download data file 4 (.csv)
                </a>
              </li>
              <li>
                <a href="#" className="govuk-link">
                  Download data file 5 (.csv)
                </a>
              </li>
              <li>
                <a href="#" className="govuk-link">
                  Download data file 6 (.csv)
                </a>
              </li>
              <li>
                <a href="#" className="govuk-link">
                  Download data file 7 (.csv)
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
              </span>{' '}
              2016 to 2017
            </h3>

            <Details summary="See previous 7 releases">
              <ul className="govuk-list">
                <li>
                  <a
                    className="govuk-link"
                    href="/themes/schools/absence-and-exclusions/pupil-absence-in-schools-in-england/2015-16"
                  >
                    2015 to 2016
                  </a>
                </li>
                <li>
                  <a href="https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-2014-to-2015">
                    2014 to 2015
                  </a>
                </li>
                <li>
                  <a href="https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-2013-to-2014">
                    2013 to 2014
                  </a>
                </li>
                <li>
                  <a href="https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-2012-to-2013">
                    2012 to 2013
                  </a>
                </li>
                <li>
                  <a href="https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-including-pupil-characteristics">
                    2011 to 2012
                  </a>
                </li>
                <li>
                  <a href="https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-including-pupil-characteristics-academic-year-2010-to-2011">
                    2010 to 2011
                  </a>
                </li>
                <li>
                  <a href="https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-including-pupil-characteristics-academic-year-2009-to-2010">
                    2009 to 2010
                  </a>
                </li>
              </ul>
            </Details>

            <h3 className="govuk-heading-s govuk-!-margin-bottom-0">
              <span className="govuk-caption-m">Last updated: </span>20 June
              2018
            </h3>

            <Details summary="See all 2 updates">
              <div data-testid="publication-page--update-element">
                <h3 className="govuk-heading-s">19 April 2017</h3>
                <p>
                  Underlying data file updated to include absence data by pupil
                  residency and school location, andupdated metadata document.
                </p>
              </div>
              <div data-testid="publication-page--update-element">
                <h3 className="govuk-heading-s">22 March 2017</h3>
                <p>First published.</p>
              </div>
            </Details>

            <h2
              className="govuk-heading-m govuk-!-margin-top-6"
              id="related-content"
            >
              Related guidance
            </h2>
            <nav role="navigation" aria-labelledby="related-content">
              <ul className="govuk-list">
                <li>
                  <Link to="/prototypes/methodology-absence">
                    Pupil absence statistics: methodology
                  </Link>
                </li>
              </ul>
            </nav>
          </aside>
        </div>
      </div>
      <hr />
      <h2 className="govuk-heading-l">
        Headline facts and figures - 2016 to 2017 school year
      </h2>
      <PrototypeDataSample
        sectionId="headlines"
        chartTitle="change in absence types in England"
        xAxisLabel="School Year"
        yAxisLabel="Absence Rate"
        chartData={[
          {
            authorised: 4.2,
            name: '2012/13',
            overall: 5.3,
            unauthorised: 1.1,
          },
          {
            authorised: 3.5,
            name: '2013/14',
            overall: 4.5,
            unauthorised: 1.1,
          },
          {
            authorised: 3.5,
            name: '2014/15',
            overall: 4.6,
            unauthorised: 1.1,
          },
          {
            authorised: 3.4,
            name: '2015/16',
            overall: 4.6,
            unauthorised: 1.1,
          },
          {
            authorised: 3.4,
            name: '2016/17',
            overall: 4.7,
            unauthorised: 1.3,
          },
        ]}
        chartDataKeys={['unauthorised', 'authorised', 'overall']}
      />

      <Accordion id="contents-sections">
        <AccordionSection heading="Pupil absence rates">
          <div className="govuk-text">
            <h3 className="govuk-heading-s">Overall absence</h3>
            <p>
              The <a href="/glossary#overall-absence">overall absence</a> rate
              has increased across state-funded primary, secondary and special
              schools between 2015/16 and 2016/17 driven by an increase in the
              unauthorised absence rate.
            </p>
            <p>
              It increased from 4.6% to 4.7% over this period while the{' '}
              <a href="/glossary#unauthorised-absence">unauthorised absence</a>{' '}
              rate increased from 1.1% to 1.3%.
            </p>
            <p>
              The rate stayed the same at 4% in primary schools but increased
              from 5.2% to 5.4% for secondary schools. However, in special
              schools it was much higher and rose to 9.7%.
            </p>
            <p>
              The overall and{' '}
              <a href="/glossary#authorised-absence">authorised absence</a>{' '}
              rates have been fairly stable over recent years after gradually
              decreasing between 2006/07 and 2013/14.
            </p>
          </div>
          <PrototypeDataSample
            sectionId="absenceRates"
            chartTitle="absence rates in England"
            xAxisLabel="School Year"
            yAxisLabel="Absence Rate"
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

          <h3 className="govuk-heading-s">Unauthorised absence</h3>
          <p>
            The{' '}
            <a href="/glossary#unauthorised-absence">unauthorised absence</a>{' '}
            rate has not varied much since 2006/07 but at is at its highest
            since records began - 1.3%.
          </p>
          <p>
            This is due to an increase in absence due to family holidays not
            agreed by schools.
          </p>
          <h3 className="govuk-heading-s">Authorised absence</h3>
          <p>
            The <a href="/glossary#authorised-absence">authorised absence</a>{' '}
            rate has stayed at 3.4% since 2015/16 but has been decreasing in
            recent years within primary schools.
          </p>
          <h3 className="govuk-heading-s">Total number of days missed</h3>
          <p>
            The total number of days missed for{' '}
            <a href="/glossary#overall-absence">overall absence</a> across
            state-funded primary, secondary and special schools has increased to
            56.7 million from 54.8 million in 2015/16.
          </p>
          <p>
            This partly reflects a rise in the total number of pupils with the
            average number of days missed per pupil slightly increased to 8.2
            days from 8.1 days in 2015/16.
          </p>
          <p>
            In 2016/17, 91.8% of primary, secondary and special school pupils
            missed at least 1 session during the school year - similar to the
            91.7% figure from 2015/16.
          </p>
        </AccordionSection>

        <AccordionSection heading="Persistent absence">
          <div className="govuk-text">
            <p>
              The <a href="/glossary#persistent-absence">persistent absence</a>{' '}
              rate increased to and accounted for 37.6% of all absence - up from
              36.6% in 2015 to 16 but still down from 43.3% in 2011 to 12.
            </p>
            <p>
              It also accounted for almost a third (31.6%) of all{' '}
              <a href="/glossary#authorised-absence">authorised absence</a> and
              more than half (53.8%) of all{' '}
              <a href="/glossary#unauthorised-absence">unauthorised absence</a>.
            </p>
            <p>
              Overall, it's increased across primary and secondary schools to
              10.8% - up from 10.5% in 2015 to 16.
            </p>
          </div>
          <PrototypeDataSample
            sectionId="persistentAbsence"
            chartTitle="persistent absence rates in England"
            xAxisLabel="School Year"
            yAxisLabel="Persistent Absence Rate"
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
          <h3 className="govuk-heading-s">Persistent absentees</h3>
          <p>
            The <a href="/glossary#overall-absence">overall absence</a> rate for
            persistent absentees across all schools increased to 18.1% - nearly
            4 times higher than the rate for all pupils. This is slightly up
            from 17.6% in 2015/16.
          </p>
          <h3 className="govuk-heading-s">Illness absence rate</h3>
          <p>
            The illness absence rate is almost 4 times higher for persistent
            absentees at 7.6% compared to 2% for other pupils.
          </p>
        </AccordionSection>

        <AccordionSection
          id="reasons-for-absence"
          heading="Reasons for absence"
        >
          <div className="govuk-text">
            <p>These have been broken down into the following:</p>
            <ul className="govuk-list-bullet">
              <li>
                distribution of absence by reason - the proportion of absence
                for each reason, calculated by taking the number of absences for
                a specific reason as a percentage of the total number of
                absences
              </li>
              <li>
                rate of absence by reason - the rate of absence for each reason,
                calculated by taking the number of absences for a specific
                reason as a percentage of the total number of possible sessions
              </li>
              <li>
                one or more sessions missed due to each reason - the number of
                pupils missing at least 1 session due to each reason
              </li>
            </ul>
          </div>
          <PrototypeDataSample
            sectionId="reasonAbsence"
            chartTitle="reason for absence in England"
            xAxisLabel="School Year"
            yAxisLabel="Absence Rate"
            chartData={[
              {
                'family holiday': 0.7,
                illness: 3.2,
                name: '2012/13',
                overall: 3.9,
              },
              {
                'family holiday': 0.7,
                illness: 3.5,
                name: '2013/14',
                overall: 4.2,
              },
              {
                'family holiday': 0.7,
                illness: 3.4,
                name: '2014/15',
                overall: 4.1,
              },
              {
                'family holiday': 0.7,
                illness: 3.3,
                name: '2015/16',
                overall: 4.0,
              },
              {
                'family holiday': 0.7,
                illness: 3.7,
                name: '2016/17',
                overall: 4.4,
              },
            ]}
            chartDataKeys={['overall', 'illness', 'family holiday']}
          />
          <h3 className="govuk-heading-s">Illness</h3>
          <p>
            This is the main driver behind{' '}
            <a href="/glossary#overall-absence">overall absence</a> and
            accounted for 55.3% of all absence - down from 57.3% in 2015/16 and
            60.1% in 2014/15.
          </p>
          <p>
            While the overall absence rate has slightly increased since 2015/16
            the illness rate has stayed the same at 2.6%.
          </p>
          <p>
            The absence rate due to other unauthorised circumstances has also
            stayed the same since 2015/16 at 0.7%.
          </p>
          <h3 className="govuk-heading-s">Absence due to family holiday</h3>
          <p>
            The unauthorised holiday absence rate has increased gradually since
            2006/07 while authorised holiday absence rates are much lower than
            in 2006/07 and remained steady over recent years.
          </p>
          <p>
            The percentage of pupils who missed at least 1 session due to family
            holiday increased to 16.9% - up from 14.7% in 2015/16.
          </p>
          <p>
            The absence rate due to family holidays agreed by the school stayed
            at 0.1%.
          </p>
          <p>
            Meanwhile, the percentage of all possible sessions missed due to
            unauthorised family holidays increased to 0.4% - up from 0.3% in
            2015/16.
          </p>
          <h3 className="govuk-heading-s">Regulation amendment</h3>
          <p>
            A regulation amendment in September 2013 stated that term-time leave
            could only be granted in exceptional circumstances which explains
            the sharp fall in authorised holiday absence between 2012/13 and
            2013/14.
          </p>
          <p>
            These statistics and data relate to the period after the{' '}
            <a href="https://commonslibrary.parliament.uk/insights/term-time-holidays-supreme-court-judgment/">
              Isle of Wight Council v Jon Platt High Court judgment (May 2016)
            </a>{' '}
            where the High Court supported a local magistrates’ ruling that
            there was no case to answer.
          </p>
          <p>
            They also partially relate to the period after the April 2017
            Supreme Court judgment where it unanimously agreed that no children
            should be taken out of school without good reason and clarified that
            'regularly' means 'in accordance with the rules prescribed by the
            school'.
          </p>
        </AccordionSection>

        <AccordionSection heading="Distribution of absence">
          <p>
            Nearly half of all pupils (48.9%) were absent for 5 days or less
            across primary, secondary and special schools - down from 49.1% in
            2015/16.
          </p>
          <p>
            The average total absence for primary school pupils was 7.2 days
            compared to 16.9 days for special school and 9.3 day for secondary
            school pupils.
          </p>
          <p>
            The rate of pupils who had more than 25 days of absence stayed the
            same as in 2015/16 at 4.3%.
          </p>
          <p>
            These pupils accounted for 23.5% of days missed while 8.2% of pupils
            had no absence.
          </p>
          <h3 className="govuk-heading-s">Absence by term</h3>
          <p>Across all schools:</p>
          <ul className="govuk-list-bullet">
            <li>
              <a href="/glossary#overall-absence">overall absence</a> - highest
              in summer and lowest in autumn
            </li>
            <li>
              <a href="/glossary#authorised-absence">authorised absence</a> -
              highest in spring and lowest in summer
            </li>
            <li>
              <a href="/glossary#unauthorised-absence">unauthorised absence</a>{' '}
              - highest in summer
            </li>
          </ul>
        </AccordionSection>

        <AccordionSection heading="Absence by pupil characteristics">
          <p>
            The <a href="/glossary#overall-absence">overall absence</a> and{' '}
            <a href="/glossary#persistent-absence">persistent absence</a>{' '}
            patterns for pupils with different characteristics have been
            consistent over recent years.
          </p>
          <h3 className="govuk-heading-s">Ethnic groups</h3>
          <p>Overall absence rate:</p>
          <ul className="govuk-list-bullet">
            <li>
              Travellers of Irish heritage and Gypsy / Roma pupils - highest at
              18.1% and 12.9% respectively
            </li>
            <li>
              Chinese and Black African ethnicity pupils - substantially lower
              than the national average of 4.7% at 2.4% and 2.9% respectively
            </li>
          </ul>
          <p>Persistent absence rate:</p>
          <ul className="govuk-list-bullet">
            <li>Travellers of Irish heritage pupils - highest at 64%</li>
            <li>Chinese pupils - lowest at 3.1%</li>
          </ul>
          <h3 className="govuk-heading-s">
            Free school meals (FSM) eligibility
          </h3>
          <p>Overall absence rate:</p>
          <ul className="govuk-list-bullet">
            <li>
              pupils known to be eligible for and claiming FSM - higher at 7.3%
              compared to 4.2% for non-FSM pupils
            </li>
          </ul>
          <p>Persistent absence rate:</p>
          <ul className="govuk-list-bullet">
            <li>
              pupils known to be eligible for and claiming FSM - more than
              double the rate of non-FSM pupils
            </li>
          </ul>
          <h3 className="govuk-heading-s">Gender</h3>
          <p>Overall absence rate:</p>
          <ul className="govuk-list-bullet">
            <li>boys and girls - very similar at 4.7% and 4.6% respectively</li>
          </ul>
          <p>Persistent absence rate:</p>
          <ul className="govuk-list-bullet">
            <li>boys and girls - similar at 10.9% and 10.6% respectively</li>
          </ul>
          <h3 className="govuk-heading-s">National curriculum year group</h3>
          <p>Overall absence rate:</p>
          <ul className="govuk-list-bullet">
            <li>
              pupils in national curriculum year groups 3 and 4 - lowest at 3.9%
              and 4% respectively
            </li>
            <li>
              pupils in national curriculum year groups 10 and 11 - highest at
              6.1% and 6.2% respectively
            </li>
          </ul>
          <p>This trend is repeated for the persistent absence rate.</p>
          <h3 className="govuk-heading-s">Special educational need (SEN)</h3>
          <p>Overall absence rate:</p>
          <ul className="govuk-list-bullet">
            <li>
              pupils with a SEN statement or education healthcare (EHC) plan -
              8.2% compared to 4.3% for those with no identified SEN
            </li>
          </ul>
          <p>Persistent absence rate:</p>
          <ul className="govuk-list-bullet">
            <li>
              pupils with a SEN statement or education healthcare (EHC) plan -
              more than 2 times higher than pupils with no identified SEN
            </li>
          </ul>
        </AccordionSection>

        <AccordionSection heading="Absence for 4-year-olds">
          <p>
            The <a href="/glossary#overall-absence">overall absence</a> rate
            decreased to 5.1% - down from 5.2% for the previous 2 years.
          </p>
          <p>
            Absence recorded for 4-year-olds is not treated as authorised or
            unauthorised and only reported as overall absence.
          </p>
        </AccordionSection>

        <AccordionSection heading="Pupil referral unit absence">
          <p>
            The <a href="/glossary#overall-absence">overall absence</a> rate
            increased to 33.9% - up from 32.6% in 2015/16.
          </p>
          <p>
            The <a href="/glossary#persistent-absence">persistent absence</a>{' '}
            rate increased to 73.9% - up from 72.5% in 2015/16.
          </p>
        </AccordionSection>

        <AccordionSection
          heading="Regional and local authority (LA) breakdown"
          onToggle={() => mapRef && mapRef.refresh()}
        >
          <PrototypeAbsenceData
            ref={el => {
              if (el) {
                // eslint-disable-next-line prefer-destructuring
                mapRef = el.mapRef;
              }
            }}
          />

          <p>
            <a href="/glossary#overall-absence">Overall absence</a> and{' '}
            <a href="/glossary#persistent-absence">persistent absence</a> rates
            vary across primary, secondary and special schools by region and
            local authority (LA).
          </p>
          <h3 className="govuk-heading-s">Overall absence</h3>
          <p>
            Similar to 2015/16, the 3 regions with the highest rates across all
            school types were:
          </p>
          <ul className="govuk-list-bullet">
            <li>North East - 4.9%</li>
            <li>Yorkshire and the Humber - 4.9%</li>
            <li>South West - 4.8%</li>
          </ul>
          <p>Meanwhile, Inner and Outer London had the lowest rates at 4.4%.</p>
          <h3 className="govuk-heading-s">Persistent absence</h3>
          <p>
            The region with the highest persistent absence rate was Yorkshire
            and the Humber with 11.9% while Outer London had the lowest rate at
            10%.
          </p>
          <h3 className="govuk-heading-s">Local authority (LA) level data</h3>
          <p>
            Download data in the following formats or access our data via our
            API:
          </p>
          <ul className="govuk-list-bullet">
            <li>
              <a href="#" className="govuk-link">
                Download .csv files
              </a>
            </li>
            <li>
              <a href="#" className="govuk-link">
                Download Excel files
              </a>
            </li>
            <li>
              <a href="#" className="govuk-link">
                Download pdf files
              </a>
            </li>
            <li>
              <a href="#" className="govuk-link">
                Access API
              </a>{' '}
              -{' '}
              <a href="/glossary#what-is-an-api" className="govuk-link">
                What is an API?
              </a>
            </li>
          </ul>
        </AccordionSection>
      </Accordion>
      <h2 className="govuk-heading-m govuk-!-margin-top-9">Help and support</h2>
      <Accordion id="extra-information-sections">
        <AccordionSection
          heading="Pupil absence statistics: methodology"
          caption="Find out how and why we collect, process and publish these statistics"
          headingTag="h3"
        >
          <p>
            Read our{' '}
            <a href="/prototypes/methodology-absence">
              Pupil absence statistics: methodology
            </a>{' '}
            guidance.
          </p>
        </AccordionSection>

        <AccordionSection heading="About these statistics">
          <p className="govuk-body">
            The statistics and data cover the absence of pupils of compulsory
            school age during the 2016/17 academic year in the following
            state-funded school types:
          </p>
          <ul className="govuk-list-bullet">
            <li>primary schools</li>
            <li>secondary schools</li>
            <li>special schools</li>
          </ul>
          <p>
            They also include information for{' '}
            <a href="/glossary#pupil-referral-unit">pupil referral units</a> and
            pupils aged 4 years.
          </p>
          <p>
            We use the key measures of{' '}
            <a href="/glossary#overall-absence">overall absence</a> and{' '}
            <a href="/glossary#persistent-absence">persistent absence</a> to
            monitor pupil absence and also include{' '}
            <a href="#contents-sections-heading-3">absence by reason</a> and{' '}
            <a href="#contents-sections-heading-5">pupil characteristics</a>.
          </p>
          <p>
            The statistics and data are available at national, regional, local
            authority (LA) and school level and are used by LAs and schools to
            compare their local absence rates to regional and national averages
            for different pupil groups.
          </p>
          <p>
            They're also used for policy development as key indicators in
            behaviour and school attendance policy.
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
          <p className="govuk-!-margin-top-0">If you have a media enquiry</p>
          <p>
            Telephone
            <br />
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

export default PublicationPage;
