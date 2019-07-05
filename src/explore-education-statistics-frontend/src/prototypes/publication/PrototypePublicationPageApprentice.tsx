import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import Details from '@common/components/Details';
import PrototypeMap from '@common/prototypes/publication/components/PrototypeMap';
import Link from '@frontend/components/Link';
import PrototypePage from '@frontend/prototypes/components/PrototypePage';
import React from 'react';
import PrototypeAbsenceData from './components/PrototypeAbsenceData';
import PrototypeDataSample from './components/PrototypeDataSample-app';

const PublicationPage = () => {
  let mapRef: PrototypeMap | null = null;

  return (
    <PrototypePage
      breadcrumbs={[
        {
          link: '/prototypes/browse-releases-find',
          text: 'Find statistics and download data',
        },
        { text: 'Absence statistics for schools in England', link: '#' },
      ]}
    >
      <strong className="govuk-tag govuk-!-margin-bottom-2">
        {' '}
        Latest statistics and data{' '}
      </strong>
      <h1 className="govuk-heading-xl">Apprenticeship statistics</h1>
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
                For academic year:{' '}
              </span>
              2017/18
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
                  <Link to="#">Apprenticeship statistics: methodology</Link>
                </li>
              </ul>
            </nav>
          </aside>
        </div>
      </div>
      <hr />
      <h2 className="govuk-heading-l">
        Headline facts and figures - 2017/18 academic year
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
        <AccordionSection heading="Overview">
          <p>
            General information in here about apprenticeships to give an
            overview of what this publication contains.
          </p>
          <p>
            We'd generally suggest including about 3 to 4 lines to break up the
            beginning of this section ahead of the following table, chart and
            data section.
          </p>
          <p>
            Please note the following table is not accurate and even though it
            has an 'apprenticeships' heading the table and charts do not contain
            this kind of data.
          </p>
          <PrototypeDataSample
            sectionId="absenceRates"
            chartTitle="Apprenticeships in England"
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
          <p>
            You could add in more information underneath the table if you wanted
            to.
          </p>
          <p>Or even add in an extra couple of lines - if you liked.</p>
        </AccordionSection>

        <AccordionSection heading="Annual comparisons">
          <div className="govuk-text">
            <p>
              The latest figures in this section relate to the 2017/18 academic
              year with all figures and trends based on full-year final data.
            </p>
            <h3 className="govuk-heading-s">Apprenticeship standards</h3>
            <p>
              Apprenticeship standards are high-quality employer-designed
              apprenticeships.
            </p>
            <p>
              The government is committed to all apprenticeship starts being on
              standards by the start of 2020/21.
            </p>
            <p>
              The apprenticeship frameworks these standards are replacing are to
              be withdrawn at this point.
            </p>
            <p>Latest figures show:</p>
            <ul className="govuk-list-bullet">
              <li>
                standards accounted for 43.6% of starts – up from just 5.0% in
                2016/17
              </li>
              <li>
                since September 2014 there have been 193,100 starts and 3,000
                achievements on standards
              </li>
            </ul>
            <h3 className="govuk-heading-s">Detailed level</h3>
            <p>Latest figures show:</p>
            <ul className="govuk-list-bullet">
              <li>
                higher apprenticeship (level 4+) starts reached their highest
                volume and increased to 48,150 - up from just 3,700 in 2011/12
              </li>
              <li>
                higher apprenticeship starts have increased by 31.7% – up from
                36,570 in 2016/17
              </li>
              <li>
                higher apprenticeship starts also increased by 34.7% between
                2015/16 and 2016/17 – up to 36,570 from 27,200
              </li>
              <li>
                intermediate apprenticeship (level 2) starts decreased by 38.1%
                161,400 – down from 260,700 in 2016/17
              </li>
              <li>
                advanced apprenticeships (level 3) starts fell by 15.9% from
                2016.17 - down from 197,700 to 166,200
              </li>
              <li>
                degree and postgraduate apprenticeship (level 6 and 7) starts
                stood at 10,880 of which 58.5% (6,360) were degree
                apprenticeships – up from 1,700 in 2016/17 when they accounted
                for 96.3% (1,630) of starts
              </li>
              <li>
                compared to 2016/17 - higher level starts increased by 31.7%
                (xx,xxx) - up from 36,750
              </li>
            </ul>
            <p>
              In comparison - higher level starts increased by 34.7% (xx,xxx)
              between 2015/16 and 2016/17.
            </p>
            <h3 className="govuk-heading-s">Length of employment</h3>
            <p>
              The rate of starts by the length of time apprentices have been
              with their employer prior to starting their apprenticeships has
              changed over time.
            </p>
            <p>
              Those who had been with their employer for more than 12 months
              prior to starting an apprenticeship accounted for:
            </p>
            <ul className="govuk-list-bullet">
              <li>34.2% of all starts in 2013/14</li>
              <li>(rising steadily to) 41.6% of all starts in 2016/17</li>
              <li>(before falling to) 38.2% of all starts in 2017/18</li>
            </ul>
            <p>
              Those had been with their employer for up to 3 months prior to
              starting their apprenticeship accounted for:
            </p>
            <ul className="govuk-list-bullet">
              <li>44.1% of all starts in 2017/18 - up from 39.5% in 2016/17</li>
            </ul>
            <h3 className="govuk-heading-s">
              Expected duration and off-the-job training
            </h3>
            <p>
              The expected duration of an apprenticeship is the difference
              between the associated start date and planned end date as recorded
              in the ILR.{' '}
            </p>
            <p>
              Latest figures for 2017/18 show the average expected duration of
              an apprenticeship:
            </p>
            <ul className="govuk-list-bullet">
              <li>increased to 581 days - up from 406 days in 2011/12</li>
              <li>
                increased by 13.7% - up to 581 days from 511 days in 2016/17
              </li>
            </ul>
            <p>
              Reported average hours of formal training per week (taken from the
              ‘Apprenticeship Evaluation Learner Survey 2017’) are combined with
              apprenticeship starts and expected duration data (taken from the
              ILR) to estimate off-the-job training hours.
            </p>
            <p>Latest estimates suggest:</p>
            <ul className="govuk-list-bullet">
              <li>
                the average expected off-the-job training hours increased by
                26.3% to 630 hours - up from 490 hours in 2016/17
              </li>
              <li>
                despite a 26.0% decrease in apprenticeship starts since 2016/17,
                the total number of off-the job training hours is estimated to
                only have decreased by 6.6%% over the same period – down to 212
                million hours from 227 million hours
              </li>
            </ul>
          </div>
        </AccordionSection>

        <AccordionSection heading="Apprenticeship service: transparency">
          <div className="govuk-text">
            <h3 className="govuk-heading-s">
              Account registrations and commitments
            </h3>
            <p>As at 30 April 2019 the number of:</p>
            <ul className="govuk-list-bullet">
              <li>registered ASAs was 17,300</li>
              <li>
                commitments recorded for 2018/19 was 174,400 - 166,400 fully
                agreed and 8,000 pending approval
              </li>
            </ul>
            <p>
              Compared to the equivalent point last year this is an increase of
              X% - up from 140,900 in 2017/18 (132,600 fully agreed and 8,300
              pending approval).
            </p>
            <h3 className="govuk-heading-s">Commitments by age and level</h3>
            <p>Of the 174,400 commitments recorded so far for 2018/19:</p>
            <ul className="govuk-list-bullet">
              <li>aged 25 and over – 91,700</li>
              <li>intermediate apprenticeships – 53,900</li>
              <li>advanced – 74,100</li>
            </ul>
            <h3 className="govuk-heading-s">Transfers</h3>
            <p>
              In April 2018 it became possible for levy-paying organisations to
              transfer up to 10% of the annual value of funds entering their
              apprenticeship service account to other organisations in the
              apprenticeship service. This increased to 25% from April 2019.
            </p>
            <p>
              As at 30 April 2019 there were 480 commitments where the transfer
              of funds between ASAs has been approved. Of these, 270
              materialised into apprenticeship starts.
            </p>
            <h3 className="govuk-heading-s">Monthly starts</h3>
            <p>
              As of May 2017 significant structural changes were made to the
              apprenticeship funding system including the introduction of the
              apprenticeship levy and the apprenticeship service.
            </p>
            <p>
              These changes have had a significant impact on apprenticeship
              starts.
            </p>
            <p>
              Quarterly apprenticeship starts data [link off elsewhere] are the
              most robust basis for interpreting how starts relate to historical
              trends.
            </p>
            <p>
              Apprenticeship starts figures are provided on a monthly basis for
              transparency purposes.
            </p>
            <p>
              Reported to date for the period August 2018 to March 2019 there
              have been 285,000 starts.
            </p>
            <p>Compared to the equivalent period in previous years this is:</p>
            <ul className="govuk-list-bullet">
              <li>up by x% from 261,200 in 2017/18</li>
              <li>down by y% from 362,400 in 2016/17</li>
              <li>down by z% from 346,300 in 2015/16</li>
            </ul>
          </div>
        </AccordionSection>
        <AccordionSection heading="Apprenticeship starts: headlines">
          <p>
            The figures in this section relate to the first 2 quarters of
            2018/19 (ie August 2018 to January 2019) unless otherwise stated.
          </p>
          <p>
            During the first two quarters of 2018/19, the number of
            apprenticeship starts reported to date was 214,200. Compared to:
          </p>
          <ul className="govuk-list-bullet">
            <li>
              the first two quarters of 2017/18 this was up 10.3% from 194,100
            </li>
            <li>
              the first two quarters of 2016/17 this down 17.2% from 258,800
            </li>
          </ul>
          <p>
            There have now been 1,709,500 starts since May 2015 and 4,087,100
            starts since May 2010.
          </p>
          <h3 className="govuk-heading-s">Starts by apprenticeship level</h3>
          <p>Compared to the first two quarters of 2017/18:</p>
          <ul className="govuk-list-bullet">
            <li>intermediate level starts down 7.4% (include volume?)</li>
            <li>advanced level starts up 8.9% (include volume?)</li>
            <li>higher level starts up 92.6% (include volume?)</li>
          </ul>
          <p>Compared to the first two quarters of 2016/17:</p>
          <ul className="govuk-list-bullet">
            <li>intermediate level starts down 42.8% (include volume?)</li>
            <li>advanced level starts down 7.0% (include volume?)</li>
            <li>higher level starts up 142.6% (include volume?)</li>
          </ul>
          <p>During the first two quarters of 2018/19:</p>
          <ul className="govuk-list-bullet">
            <li>
              intermediate and advanced levels as a proportion of all starts
              decreased to 81.7% - down from 89.5% in the first two quarters of
              2017/18
            </li>
            <li>
              the number of starts at level 6 and 7 increased to 14,010 - up
              from 4,050 in the first two quarters of 2017/18
            </li>
          </ul>
          <h3 className="govuk-heading-s">Levy supported starts</h3>
          <p>
            During the first two quarters of 2018/19 the number of reported
            levy-supported starts was 105,700. This was broken down by level as:
          </p>
          <ul className="govuk-list-bullet">
            <li>intermediate level - 34,000</li>
            <li>advanced level - 44,700</li>
            <li>higher level - 27,000</li>
          </ul>
          <p>
            In order to be counted as a levy supported start, an apprenticeship
            must have been supported through levy funds.
          </p>
          <p>
            Since the introduction of the apprenticeship levy in April 2017, a
            total of 312,900 levy supported apprenticeship starts have been
            recorded.
          </p>
          <h3 className="govuk-heading-s">New apprenticeship standards</h3>
          <p>
            The number of apprenticeship starts reported on apprenticeship
            standards increased to 128,100 - up from 71,600 in the first two
            quarters of 2017/18.
          </p>
          <p>
            There have now been 321,200 starts on apprenticeship standards since
            their introduction in September 2014.
          </p>
          <h3 className="govuk-heading-s">Participation numbers</h3>
          <p>The number of reported apprenticeship participants was 602,400:</p>
          <ul className="govuk-list-bullet">
            <li>
              in the first two quarters of 2017/18 this down 11.1% from 677,300
            </li>
            <li>
              in the first two quarters of 2016/17 this down 17.7% from 731,600
            </li>
          </ul>
          <h3 className="govuk-heading-s">
            Proportion of participation by apprenticeship level
          </h3>
          <p>Compared to the first two quarters of 2017/18:</p>
          <ul className="govuk-list-bullet">
            <li>intermediate level participation decreased to 28.8%</li>
            <li>advanced level participation decreased to 6.0%</li>
            <li>higher level participation increased to 55.7%</li>
          </ul>
          <p>Compared to the first two quarters of 2016/17:</p>
          <ul className="govuk-list-bullet">
            <li>intermediate level participation decreased to 42.3%</li>
            <li>advanced level participation decreased to 6.2%</li>
            <li>higher level participation increased to 108.8%</li>
          </ul>
          <h3 className="govuk-heading-s">Participation by age group</h3>
          <p>Compared to the first two quarters of 2017/18:</p>
          <ul className="govuk-list-bullet">
            <li>under-19s - down 7.8%</li>
            <li>19 to 24s - down 6.8%</li>
            <li>25-plus - down 15.9%</li>
          </ul>
          <p>Compared to the first two quarters of 2016/17:</p>
          <ul className="govuk-list-bullet">
            <li>under-19s - down 17%</li>
            <li>19 to 24s - down 14.6%</li>
            <li>25-plus - down 20.5%</li>
          </ul>
        </AccordionSection>
        <AccordionSection heading="Apprenticeship starts: learner charactertistics">
          <p>
            The latest figures in this section relate to the 2018/19 academic
            year (August 2018 to January 2019) unless otherwise stated.
          </p>
          <h3 className="govuk-heading-s">Key headlines</h3>
          <p>Out of the latest 214,200 reported starts:</p>
          <ul className="govuk-list-bullet">
            <li>males represent 52.9% (130,400)</li>
            <li>females represent 47.1% (100,800)</li>
            <li>under-19s represent 30.8% (xx,xxx)</li>
            <li>19 to 24s represent 29.6% (xx,xxx)</li>
            <li>25 and overs represent 39.5% (xx,xxx)</li>
            <li>
              Black, Asian and other ethnic minorities (BAME) represent 11.1%
              (23,700)
            </li>
            <li>
              those declaring a learning difficulties or disabilities (LLDD)
              represent 11.9% (25,500)
            </li>
          </ul>
          <p>Comparing latest start rates against those for earlier years:</p>
          <ul className="govuk-list-bullet">
            <li>males decreased from 53.4% n 2016/17</li>
            <li>
              BAMEs remain stable increasing slightly from 11.1% to 11.2% in
              2016/17
            </li>
            <li>
              LLDDs showed a year-on-year increase to 11.9% from 7.7% in 2011/12
            </li>
            <li>
              the proportion of younger apprentices is growing - the under-19s
              rate increased to 30.8% from 24.8% in 2016/17
            </li>
          </ul>
          <p>
            Participation in apprenticeships has fallen in each age-group -
            particularly among 25s and over.
          </p>
          <h3 className="govuk-heading-s">Gender</h3>
          <p>
            The proportion of apprenticeship starts between males and females
            has shifted towards males in the last two years:
          </p>
          <ul className="govuk-list-bullet">
            <li>
              females accounted for around 53% of starts between 2011/12 and
              2016/17 (with the exception of 2012/13 where the female share was
              nearer 55%)
            </li>
            <li>
              the female rate decreased to 49% in 2017/18 down from 47.1% in the
              latest reported figures
            </li>
          </ul>
          <p>Investigating further:</p>
          <ul className="govuk-list-bullet">
            <li>
              the decrease in female starts is mostly within the aged 25 and
              over age group – in contrast there has been a rise in male
              apprentices aged under-19
            </li>
            <li>
              intermediate (level 2) apprenticeships for females have decreased
              at a greater rate than for males – x% vs y%
            </li>
            <li>
              health, public Services and care sector starts for females
              accounted for almost a quarter (23%) of all starts in 2015/16 -
              this has fallen to 17% in 2018/19
            </li>
          </ul>
          <h3 className="govuk-heading-s">Age</h3>
          <p>
            The 25 and over age group has the highest share of starts. However,
            since 2016/17:
          </p>
          <ul className="govuk-list-bullet">
            <li>
              aged 25s and over starts have decreased from 46% to 40% so far in
              2018/19
            </li>
            <li>
              under-19s starts have increased from 25% to 31% so far in 2018/1
            </li>
            <li>
              the decline in starts for 25s and over seems to predominantly
              coming from those aged 45-59
            </li>
          </ul>
          <h3 className="govuk-heading-s">Ethnicity</h3>
          <p>
            Apprenticeship starts by those in minority ethnic groups have
            changed little over the last few years with 11.1% of latest starts
            within the BAME ethnic group compared to 11.2% in the previous two
            academic years - their peak level.
          </p>
          <p>Within the BAME group:</p>
          <ul className="govuk-list-bullet">
            <li>
              Black apprentices have seen their proportion of starts decrease to
              2.9% from 3.7% in 2015/16
            </li>
            <li>
              Asian apprentices and those with mixed-race backgrounds have seen
              their proportion of starts rise from xx% to yy%
            </li>
          </ul>
          <h3 className="govuk-heading-s">
            Learners with learning difficulties or disabilities (LLDD)
          </h3>
          <p>
            The share of LLDD apprentices has increased year-on-year from 7.7%
            in 2011/12 to 11.9% reported so far in 2018/19.
          </p>
        </AccordionSection>
        <AccordionSection heading="Apprenticeship starts: subjects">
          <p>
            The latest figures in this section relate to the 2018/19 academic
            year (August 2018 to January 2019) unless otherwise stated.
          </p>
          <p>Out of the 214,200 latest reported starts:</p>
          <ul className="govuk-list-bullet">
            <li>starts on standards represent 59.8% (xx,xxxx)</li>
            <li>levy-supported starts represent 49.4% (xx,xxx)</li>
            <li>level 6 and above starts represent 6.5% (xx,xxx)</li>
            <li>
              starts with science, technology, engineering and manufacturing
              (STEM) related subjects represent 31% (xx,xxx)
            </li>
          </ul>
          <p>Compared to this point in the 2017/18 academic year:</p>
          <ul className="govuk-list-bullet">
            <li>starts on standards have increased by 78.9% to 128,100</li>
            <li>level 6 and 7 starts have increased by more than 3 times</li>
          </ul>
          <p>
            Starts in business, admin and law related subjects continue to be
            most popular - accounting for x% of all starts.
          </p>
          <p>The proportion of STEM starts has increased year-on-year.</p>
          <p>
            Data shows there' ha's been a shift towards starts on higher level
            apprenticeships in recent years (ie those at level 4 and above).
          </p>
          <p>Between 2016/17 and 2017/18:</p>
          <ul className="govuk-list-bullet">
            <li>higher level starts increased by 31.7% to 48,150</li>
            <li>level 6 and above starts increased by x% to 10,880</li>
            <li>intermediate starts decreased by 38.1%</li>
            <li>advanced starts decreased by 15.9%</li>
          </ul>
          <p>Latest quarterly data for 2018/19 shows:</p>
          <ul className="govuk-list-bullet">
            <li>higher level starts increased by 31.7% to 48,150</li>
            <li>level 6 and above starts increased by x% to 10,880</li>
          </ul>
          <p>
            In 2011/12 the proportion of intermediate and advanced level starts
            out of all starts was 99.3%.
          </p>
          <p>
            Since their introduction in September 2014, the number of starts on
            standards now stands at 321,200.
          </p>
        </AccordionSection>

        <AccordionSection heading="Apprenticeship starts: public sector">
          <p>
            Public sector bodies in England with 250 or more staff have a target
            to employ an average of at least 2.3% of their staff as new
            apprentice starts over the period 1 April 2017 to 31 March 2021.
          </p>
          <p>
            This means when making workforce planning decisions bodies in scope
            should actively consider apprenticeships either for new recruits or
            as part of career development for existing staff.
          </p>
          <p>
            The target is for new apprenticeship starts (including both existing
            staff that start an apprenticeship and new recruits) and measures
            these as a percentage of the total headcount of public sector bodies
            at the beginning of the reporting period.
          </p>
          <p>
            The target is an average over four years, split into the following
            individual reporting periods covering each financial year:
          </p>
          <h3 className="govuk-heading-s">
            Period covering 1 April 2017 to 31 March 2018
          </h3>
          <p>Figures submitted to DfE by public sector bodies show:</p>
          <ul className="govuk-list-bullet">
            <li>
              the proportion of workers in the public sector who started an
              apprenticeship was 1.4% - equating to over 45,000 new apprentices
            </li>
            <li>
              the total number of employees has fallen over the reporting period
              (from 3.21 to 3.18 million) but the number of apprentices has
              risen (from 46,000 to more than 60,000)
            </li>
            <li>
              at the start of the reporting period - 1.4% of employees were
              apprentices rising to 1.9% by the end
            </li>
            <li>
              staff starting an apprenticeship account for one in ten (10%) of
              all new appointments
            </li>
          </ul>
          <h3 className="govuk-heading-s">Sub-sectors</h3>
          <p>
            There was variation in the take-up rate of apprentices in different
            parts of the public sector during 2017/18:
          </p>
          <ul className="govuk-list-bullet">
            <li>
              armed forces recruitment reported the proportion of employees
              starting with an apprenticeship at 9.1%
            </li>
            <li>
              civil service recruitment was slightly behind the national average
              reporting 1.3% of employees starting with an apprenticeship
            </li>
            <li>
              NHS recruitment was also reported slightly behind the national
              average with 1.2% of employees starting with an apprenticeship
            </li>
            <li>
              police recruitment had the lowest reported rate of apprenticeship
              recruitment at 0.2%
            </li>
            <li>
              all sub-sectors (apart from the police) saw a reported increase in
              the percentage of employees starting with an apprenticeship
            </li>
          </ul>
          <p>
            Most sub-sectors saw a modest rise in headcount over the period.
            However, local authorities (LAs) declared a 5% fall in employees.
          </p>
          <p>
            Despite this, the the number of apprentices in LAs has risen has
            risen by more than 70% from 7,300 to 12,500.
          </p>
        </AccordionSection>
        <AccordionSection
          heading="A N Other section"
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
            Overall and persistent absence rates vary across primary, secondary
            and special schools by region LA.
          </p>
          <p>
            Similar to last year, the 3 regions with the highest overall absence
            rate across all school types are the North East (4.9%), Yorkshire
            and the Humber (4.9%) and the South West (4.8%) with Inner and Outer
            London having the lowest overall absence rate (4.4%).
          </p>
          <p>
            The region with the highest persistent absence rate is Yorkshire and
            the Humber (11.9%) with Outer London having the lowest rate of
            persistent absence (10%).
          </p>
          <p>
            For LA-level absence statistics and data download our data files.
          </p>
          <p>
            You can customise and download data as Excel or .csv files. Our data
            can also be accessed via an API.
          </p>
          <ul className="govuk-list">
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
              <a href="#" className="govuk-link">
                What is an API?
              </a>
            </li>
          </ul>
        </AccordionSection>
      </Accordion>
      <h2 className="govuk-heading-m govuk-!-margin-top-9">Help and support</h2>
      <Accordion id="extra-information-sections">
        <AccordionSection
          heading="Apprenticeship statistics: methodology"
          caption="How we collect and process statistics and data"
          headingTag="h3"
        >
          <p>
            Read our{' '}
            <a href="/prototypes/methodology-absence">
              Apprenticeships statistics: methodology
            </a>{' '}
            guidance.
          </p>
        </AccordionSection>
        <AccordionSection heading="About these statistics">
          <p className="govuk-body">
            The statistics and data show the absence of pupils of compulsory
            school age during the 2016/17 academic year in the following
            state-funded school types:
          </p>
          <ul className="govuk-list-bullet">
            <li>primary schools</li>
            <li>secondary schools</li>
            <li>special schools</li>
          </ul>
          <p>
            The statistics and data also includes information on absence in
            pupil referral units and for pupils aged 4.
          </p>
          <p>
            We use the key measures of 'overall' and 'persistent' absence to
            monitor pupil absence while 'absence by reason' and 'pupil
            characteristics' are also included.
          </p>
          <p>
            The statistics and data are available at national, regional, local
            authority (LA) and school level and are used by LAs and schools to
            compare their local absence rates to regional and national averages
            for different pupil groups.
          </p>
          <p>
            The statistics and data are also used for policy development as key
            indicators in behaviour and school attendance policy.
          </p>
        </AccordionSection>

        <AccordionSection heading="National Statistics" headingTag="h3">
          <p className="govuk-body">
            The United Kingdom Statistics Authority designated these statistics
            as National Statistics in 2011 in accordance with the Statistics and
            Registration Service Act 2007 and signifying compliance with the
            Code of Practice for Statistics.
          </p>
          <p className="govuk-body">
            Designation can be broadly interpreted to mean that the statistics:
          </p>
          <ul className="govuk-list govuk-list--bullet">
            <li>meet identified user needs</li>
            <li>are well explained and readily accessible</li>
            <li>are produced according to sound methods</li>
            <li>
              are managed impartially and objectively in the public interest
            </li>
          </ul>
          <p className="govuk-body">
            Once statistics have been designated as National Statistics it's a
            statutory requirement that the Code of Practice shall continue to be
            observed.
          </p>
          <p>
            Information on improvements made to these statistics to continue
            their compliance with the Code of Practice are provided in this{' '}
            <a href="#">accompanying document</a>
          </p>
        </AccordionSection>
        <AccordionSection heading="Contact us" headingTag="h3">
          <p>
            If you have a specific enquiry about absence and exclusion
            statistics and data:
          </p>
          <h4 className="govuk-heading-s govuk-!-margin-bottom-0">
            School absence and exclusions team
          </h4>

          <p className="govuk-!-margin-top-0">
            Email <br />
            <a href="mailto:schools.statistics@education.gov.uk">
              schools.statistics@education.gov.uk
            </a>
          </p>

          <p>
            Telephone: Mark Pearson <br /> 0114 274 2585
          </p>

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
            037 0000 2288
          </p>
        </AccordionSection>
      </Accordion>
      <h2 className="govuk-heading-m govuk-!-margin-top-9">
        Create your own tables online
      </h2>
      <p>
        Use our tool to build tables using our range of national and regional
        data.
      </p>
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
