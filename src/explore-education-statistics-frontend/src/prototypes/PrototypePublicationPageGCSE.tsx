import React from 'react';
import Accordion from '../components/Accordion';
import AccordionSection from '../components/AccordionSection';
import Details from '../components/Details';
import GoToTopLink from '../components/GoToTopLink';
import Link from '../components/Link';
import PrototypeAbsenceData from './components/PrototypeAbsenceData';
import PrototypeDataSampleGCSE from './components/PrototypeDataSampleGCSE';
import PrototypeMap from './components/PrototypeMap';
import PrototypePage from './components/PrototypePage';
import PrototypeTableSampleGCSE from './components/PrototypeTableSampleGCSE';

const PublicationPage = () => {
  return (
    <PrototypePage
      breadcrumbs={[
        {
          link: '/prototypes/browse-releases-find',
          text: 'Find statistics and download data',
        },
        { text: 'GCSE and equivalent results in England', link: '#' },
      ]}
    >
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-two-thirds">
          <strong className="govuk-tag govuk-!-margin-bottom-2">
            {' '}
            This is the latest data{' '}
          </strong>
          <h1 className="govuk-heading-xl">
            GCSE and equivalent results in England, 2016 to 2017
          </h1>
          <p className="govuk-body-l">
            This statistical first release (SFR) provides information on the
            achievements in GCSE examinations and other qualifications of young
            people in academic year 2016 to 2017. This typically covers those
            starting the academic year aged 15.
          </p>
          <p className="govuk-body">
            You can also view a regional breakdown of statistics and data within
            the{' '}
            <a href="#contents-sections-heading-9">
              <strong>local authorities section</strong>
            </a>
          </p>

          <Details summary="Read more about our methodology">
            <p>
              To help you analyse and understand the statistics the following
              sections include:
            </p>

            <div className="govuk-inset-text">
              <Link to="#">
                Find out more about our pupil absence data and statistics
                methodology and terminology
              </Link>
            </div>
          </Details>
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

            <h3 className="govuk-heading-s">
              <span className="govuk-caption-m">Published: </span>22 March 2018
            </h3>

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

            <h3 className="govuk-heading-s govuk-!-margin-bottom-0">
              <span className="govuk-caption-m">Next update: </span>22 March
              2019
            </h3>
            <p className="govuk-caption-m govuk-!-margin-top-0">
              <a href="#">Notify me</a>
            </p>
          </aside>
        </div>
      </div>
      <hr />
      <h2 className="govuk-heading-l">
        Latest headline facts and figures - 2016/17
      </h2>
      <PrototypeDataSampleGCSE
        sectionId="headlines"
        chartTitle="average score per pupil in each element of Attainment 8"
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
      <h2 className="govuk-heading-l">Contents</h2>
      <Accordion id="contents-sections">
        <AccordionSection heading="About this release">
          <p className="govuk-body">
            This SFR provides revised GCSE and equivalent results of pupils at
            the end of key stage 4 in England. Figures are provided at national,
            regional and local authority level for the 2016-17 academic year.
            School level results for the headline measures are published in the
            {} <a href="#">revised school performance tables</a>.
          </p>

          <p className="govuk-body">
            This release provides an update to the provisional figures released
            in October 2017 in{' '}
            <a href="https://www.compare-school-performance.service.gov.uk/">
              SFR57/2017
            </a>
            . Amendments made during the schools checking exercise in September
            are included in this release, as are the majority of late results
            and reviews of marking received after the cut-off date for the
            provisional release in October.
          </p>

          <p className="govuk-body">
            This release also provides breakdowns by pupil characteristics and
            information on schools below the floor and those meeting the
            coasting definition, which was not included in the provisional
            update in October 2017.
          </p>

          <p className="govuk-body">
            Users should be cautious when comparing headline measures between
            2017 and 2016. In 2017, Attainment 8 scores were calculated using
            slightly different point score scales in comparison to 2016, in
            order to minimise change following the introduction of grade 9 to 1
            reformed GCSEs. This means that Attainment 8 scores are likely to
            look different in 2017, as a result of changes to the methodology.
            Where possible, for further context, 2017 Attainment 8 scores have
            been compared to 2016 shadow data, which mapped 2017 point scores
            onto 2016 results.
          </p>

          <p className="govuk-body">
            As explained below in the headline measures section, the threshold
            for the English and maths and EBacc attainment headline measures has
            risen in 2017 to include a grade 5 or above in English and maths,
            following the introduction of grade 9 to 1 reformed GCSEs in these
            subjects. In this release, pupils must achieve grades 5 or above for
            English and maths to achieve these threshold attainment measures.
            Additional measures are published alongside this where the threshold
            is set to achievement of grade 4 or above in English and maths in
            order to allow for comparisons to 2016. Since 2013, Universal Credit
            (UC) has been gradually rolling out nationwide replacing a number of
            income-related benefits, some of which provided families with
            entitlement to free school meals. Key stage 4 performance measures
            use pupils’ disadvantaged status at the end of key stage 4,
            therefore, the impact of Universal Credit on disadvantage measures
            is currently limited given the gradual roll out, but may increase in
            future years. A consultation which invited views on proposed
            approach to the eligibility for free school meals and the early
            year’s pupil premium under Universal Credit was closed in January
            2018, the response will be published later this year
          </p>
        </AccordionSection>

        <AccordionSection heading="Headline measures from 2017">
          <h3>Attainment 8 </h3>
          <p>
            Attainment 8 measures the average achievement of pupils in up to 8
            qualifications including English (double weighted if both language
            and literature are taken), maths (double weighted), three further
            qualifications that count in the English Baccalaureate (EBacc) and
            three further qualifications that can be GCSE qualifications
            (including EBacc subjects) or any other non-GCSE qualifications on
            the <a href="#">DfE approved list</a>.
          </p>
          <h3>Progress 8 </h3>
          <p>
            Progress 8 aims to capture the progress a pupil makes from the end
            of key stage 2 to the end of key stage 4. It compares pupils’
            achievement – their Attainment 8 score – with the average Attainment
            8 score of all pupils nationally who had a similar starting point
            (or ‘prior attainment’), calculated using assessment results from
            the end of primary school. Progress 8 is a relative measure,
            therefore the national average Progress 8 score for mainstream
            schools is very close to zero. When including pupils at special
            schools the national average is not zero as Progress 8 scores for
            special schools are calculated using Attainment 8 estimates based on
            pupils in mainstream schools.{' '}
            <a href="#">More information on Attainment 8 and Progress 8</a>.
          </p>
          <h3>The English Baccalaureate (EBacc) entry and achievement</h3>
          <p>
            The EBacc was first introduced into the performance tables in
            2009/10. It allows people to see how many pupils reach the
            attainment threshold in core academic subjects at key stage 4. The
            EBacc is made up of English, maths, science, a language, and history
            or geography. To count in the EBacc, qualifications must be on the{' '}
            <a href="#">English Baccalaureate list of qualifications</a>.
          </p>
          <p>
            In 2017, the headline EBacc achievement measure includes pupils who
            take exams in both English language and English literature, and
            achieve a grade 5 or above in at least one of these qualifications.
            Pupils must also achieve a grade 5 or above in mathematics and a
            grade C or above in the remaining subject areas.{' '}
          </p>
          <h3>
            Percentage of students staying in education or going into employment
            after key stage 4 (pupil destinations)
          </h3>
          <p>
            This measure is published here as part of a release including post
            key stage 4 and 16 to 18 destinations.
          </p>
          <h3>Additional measures</h3>
          <p>
            For transparency and to allow comparison to 2016, the threshold
            attainment measures are also published at grade 4 or above, as
            additional measures. These additional measures are:
          </p>
          <h3>Attainment in English and maths (grades 4 or above)</h3>
          <p>
            From 2017, this measure looks at the percentage of pupils achieving
            grade 4 or above in both English and maths. Pupils can achieve the
            English component of this with a grade 4 or above in English
            language or literature. There is no requirement to sit both exams.{' '}
          </p>
          <h3>English Baccalaureate (EBacc) achievement</h3>
          <p>
            This measure includes pupils who take exams in both English language
            and English literature, and achieve a grade 4 or above in at least
            one of these qualifications. Pupils also need to achieve a grade 4
            or above in maths and a grade C or above in the remaining subject
            areas.{' '}
          </p>
        </AccordionSection>

        <AccordionSection heading="Attainment in the headline measures">
          <p>
            When comparing 2017 headline measures to the equivalent revised data
            from 2016, it is important to note the changes in methodology
            underpinning the 2017 data. These changes are explained in the
            ‘About this release’ section above and expanded upon in following
            sections on specific headline measures.
          </p>
          <p>
            The tables below show decreases across the headline measures in 2017
            compared to 2016 revised data. However, these decreases are due to a
            number of methodological changes, including the move to a new point
            score scale for 2017 Attainment 8 scores, the introduction of
            reformed GCSEs in English and maths graded on the 9 to 1 scale and
            changes to the attainment threshold for the EBacc and the English
            and maths measure.
          </p>
          <p>
            For Attainment 8, 2016 shadow data is shown alongside the 2016
            revised results; this data is more comparable to 2017. For the
            threshold attainment measures, the equivalent measure using grade 4
            or above as a threshold for English and maths are given in the
            table, to aid comparability with 2016 data. The bottom of a C grade
            in unreformed GCSEs is mapped onto the bottom of a grade 4 in
            reformed GCSEs.
          </p>
          <PrototypeDataSampleGCSE
            chartTitle="average score per pupil in each element of Attainment 8"
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
          <p>
            The measures covered in this release include qualifications that
            count towards the secondary performance tables4 . Schools that offer
            unapproved qualifications, such as unregulated international GCSEs,
            will not have these qualifications counted in the performance
            tables, and pupils’ achievements in these qualifications are
            therefore not reflected in this release. This release is therefore
            representative of the performance of schools and pupils in
            qualifications which count in the performance tables, and not of all
            qualifications taken by pupils. The difference between the figures
            for all schools and statefunded schools is predominantly due to the
            impact of unregulated international GCSEs taken more commonly in
            independent schools. In 2017, over 30,000 pupils at the end{' '}
          </p>
          <p>
            In 2017, over 30,000 pupils at the end of key stage 4 were entered
            for either unreformed English or maths GCSEs, despite these
            qualifications not counting in 2017 performance tables. These pupils
            were not entered for the reformed GCSEs (graded on a 9 to 1 scale)
            in the same subject, which will have had an impact on Attainment 8,
            Progress 8, the EBacc entry measure and the attainment in English
            and maths measures. This is likely to have happened as a result of
            pupils taking these qualifications in 2016 before they reached the
            end of key stage 4. In addition, pupils have still entered
            unregulated international GCSEs and regulated international GCSEs
            (that counted in 2016 but no longer count in 2017) which will
            account for some of the remaining difference between the 2016 and
            2017 outcomes.
          </p>
          <p>
            an impact on Attainment 8, Progress 8, the EBacc entry measure and
            the attainment in English and maths measures. This is likely to have
            happened as a result of pupils taking these qualifications in 2016
            before they reached the end of key stage 4. In addition, pupils have
            still entered unregulated international GCSEs and regulated
            international GCSEs (that counted in 2016 but no longer count in
            2017) which will account for some of the remaining difference
            between the 2016 and 2017 outcomes.
          </p>
          <p>
            Figures for all schools typically change more than those for
            state-funded schools between the provisional and revised releases,
            due to the impact of results for independent schools and FE colleges
            with 14-16 provision. The level of change between provisional and
            revised data is higher for independent schools and FE colleges with
            14-16 provision as, under the current process, independent schools
            and FE colleges with 14-16 provision do not check their cohort
            figures until September, whereas state funded schools do this in
            June.
          </p>
          <p>
            The change between provisional and revised results in 2017 was
            slightly smaller than the equivalent change in 2016 for EBacc
            measures, and the same for English and maths achievement (at grades
            4 or above) to the change seen last year (at grades C or above), as
            shown in table 3. There was a slightly larger change in average
            Attainment 8 score.
          </p>
          <p>
            Possible reasons for this might include an increased number of
            amendments during the September checking exercise as schools
            continue to adapt to the new accountability system. There was also
            an increase in the number of GCSE grades challenged and grades
            changed in 2017, mainly in the reformed English and English
            literature GCSEs5 . These changes could have affected Attainment 8,
            more than the other headline measures, if the grade changes, as a
            result of reviews, were across the range of grades and not just the
            grade 3 and 4 boundary. The 2017 point score scale change could also
            have impacted Attainment 8, as the difference in points in 2017
            means that higher grades attract more points in comparison to 2016.
            Reviews of marking between provisional and revised data at higher
            grade boundaries would therefore have increased Attainment 8 scores
            more than an equivalent change in 2016.
          </p>
          <p>
            These changes are not substantially different to previous years and
            many of the patterns originally reported in the provisional release
            still stand.
          </p>

          <table className="govuk-table">
            <caption>
              {' '}
              Change between provisional and revised data in 2016 and 2017{' '}
            </caption>
            <thead>
              <tr>
                <th scope="col">Change between provisional and revised</th>
                <th scope="col" className="govuk-table__cell--numeric">
                  Average Attainment 8 score per pupil
                </th>
                <th scope="col" className="govuk-table__cell--numeric">
                  Attainment in English and maths (A*-C in 2016, 9-4 grades in
                  2017)
                </th>
                <th scope="col" className="govuk-table__cell--numeric">
                  Attainment in English and maths (9-5 grades)
                </th>
                <th scope="col" className="govuk-table__cell--numeric">
                  Percentage entering the EBacc
                </th>
                <th scope="col" className="govuk-table__cell--numeric">
                  Percentage achieving the EBacc (including 9-4 grades in
                  English and maths in 2017)
                </th>
                <th scope="col" className="govuk-table__cell--numeric">
                  Percentage achieving the EBacc (including 9-5 grades in
                  English and maths in 2017)
                </th>
              </tr>
            </thead>
            <tbody>
              <tr>
                <td colSpan={7}>All schools</td>
              </tr>
              <tr>
                <th scope="row">2016</th>
                <td className="govuk-table__cell--numeric">+0.3</td>
                <td className="govuk-table__cell--numeric">+0.6</td>
                <td className="govuk-table__cell--numeric">NA</td>
                <td className="govuk-table__cell--numeric">+0.2</td>
                <td className="govuk-table__cell--numeric">+0.3</td>
                <td className="govuk-table__cell--numeric">NA</td>
              </tr>
              <tr>
                <th scope="row">2017</th>
                <td className="govuk-table__cell--numeric">+0.4</td>
                <td className="govuk-table__cell--numeric">+0.6</td>
                <td className="govuk-table__cell--numeric">+0.5</td>
                <td className="govuk-table__cell--numeric">+0.1</td>
                <td className="govuk-table__cell--numeric">+0.2</td>
                <td className="govuk-table__cell--numeric">+0.2</td>
              </tr>
              <tr>
                <td colSpan={7}>State funded schools</td>
              </tr>
              <tr>
                <th scope="row">2016</th>
                <td className="govuk-table__cell--numeric">+0.3</td>
                <td className="govuk-table__cell--numeric">+0.6</td>
                <td className="govuk-table__cell--numeric">NA</td>
                <td className="govuk-table__cell--numeric">+0.2</td>
                <td className="govuk-table__cell--numeric">+0.3</td>
                <td className="govuk-table__cell--numeric">NA</td>
              </tr>
              <tr>
                <th scope="row">2017</th>
                <td className="govuk-table__cell--numeric">+0.4</td>
                <td className="govuk-table__cell--numeric">+0.6</td>
                <td className="govuk-table__cell--numeric">+0.5</td>
                <td className="govuk-table__cell--numeric">+0.1</td>
                <td className="govuk-table__cell--numeric">+0.2</td>
                <td className="govuk-table__cell--numeric">+0.2</td>
              </tr>
            </tbody>
          </table>

          <h3 className="govuk-heading-m">
            Schools continue to adapt their curricula to match the headline
            measures
          </h3>
          <p>
            Attainment 8 is made up of eight slots, which can be filled with
            English, maths, three qualifications which count towards the English
            Baccalaureate (EBacc), and three other qualifications from the DfE
            approved list, which can include additional EBacc qualifications. If
            a pupil has not taken the maximum number of qualifications that
            count in each group then they will receive a point score of zero
            where a slot is empty6
          </p>
          <p>
            In 2017, pupils in state-funded schools filled an average of 2.8
            EBacc slots. There was stability in the number of EBacc slots filled
            by pupils with average and high prior attainment (2.8 slots and 3.0
            slots respectively) but pupils with low prior attainment increased
            from 1.9 average EBacc slots filled in 2016 to 2.1 in 2017. This
            suggests that schools are continuing to enter pupils into
            qualifications that count towards the new headline measures. Whilst
            the average uptake for pupils with low prior attainment has
            increased, this is a smaller group of pupils, so this has not had
            much of an impact on the average for all pupils. The average number
            of EBacc slots filled is shown in Figure 1.
          </p>
        </AccordionSection>

        <AccordionSection heading="Attainment 8 and Progress 8">
          TEXT HERE
        </AccordionSection>

        <AccordionSection heading="Attainment in English and maths">
          TEXT HERE
        </AccordionSection>

        <AccordionSection heading="Subject analysis ">
          TEXT HERE
        </AccordionSection>

        <AccordionSection heading="Attainment 8 and Progress 8">
          TEXT HERE
        </AccordionSection>

        <AccordionSection heading="Attainment by pupil characteristics">
          TEXT HERE
        </AccordionSection>

        <AccordionSection heading="Floor standards">TEXT HERE</AccordionSection>

        <AccordionSection heading="Coasting schools">
          TEXT HERE
        </AccordionSection>

        <AccordionSection heading="Attainment by school type">
          TEXT HERE
        </AccordionSection>

        <AccordionSection heading="Attainment by admissions basis">
          TEXT HERE
        </AccordionSection>

        <AccordionSection heading="Attainment by religious character">
          TEXT HERE
        </AccordionSection>

        <AccordionSection heading="Attainment by local authority">
          TEXT HERE
        </AccordionSection>
      </Accordion>
      <h2 className="govuk-heading-m govuk-!-margin-top-9">
        Extra information
      </h2>
      <Accordion id="extra-information-sections">
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
          <h4 className="govuk-heading-">Media enquiries</h4>
          <address className="govuk-body dfe-font-style-normal">
            Press Office News Desk
            <br />
            Department for Education <br />
            Sanctuary Buildings <br />
            Great Smith Street <br />
            London
            <br />
            SW1P 3BT <br />
            Telephone: 020 7783 8300
          </address>

          <h4 className="govuk-heading-">Other enquiries</h4>
          <address className="govuk-body dfe-font-style-normal">
            Data Insight and Statistics Division
            <br />
            Level 1<br />
            Department for Education
            <br />
            Sanctuary Buildings <br />
            Great Smith Street
            <br />
            London
            <br />
            SW1P 3BT <br />
            Telephone: 020 7783 8300
            <br />
            Email: <a href="#">Schools.statistics@education.gov.uk</a>
          </address>
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
      <Link to="/prototypes/data-table-v3" className="govuk-button">
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

export default PublicationPage;
