import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import Details from '@common/components/Details';
import Link from '@frontend/components/Link';

import PrototypePage from '@frontend/prototypes/components/PrototypePage';
import PrototypeSearchForm from '@frontend/prototypes/components/PrototypeSearchForm';
import PrototypeDataSampleGCSE from '@frontend/prototypes/publication/components/PrototypeDataSampleGCSE';
import React from 'react';

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
      <strong className="govuk-tag govuk-!-margin-bottom-2">
        {' '}
        Latest statistics and data{' '}
      </strong>
      <h1 className="govuk-heading-xl">
        GCSE and equivalent result statistics and data for schools in England
      </h1>
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-two-thirds">
          <dl className="dfe-meta-content govuk-!-margin-0">
            <dt className="govuk-caption-m">Published:</dt>
            <dd>
              <strong>22 March 2018</strong>
            </dd>
          </dl>
        </div>
        <div className="govuk-grid-column-one-third">
          <PrototypeSearchForm />
        </div>
      </div>

      <hr />

      <div className="govuk-grid-row">
        <div className="govuk-grid-column-two-thirds">
          <div className="govuk-grid-row">
            <div className="govuk-grid-column-three-quarters">
              <p className="govuk-body">
                Read national statistical summaries, view charts and tables and
                download data files.
              </p>
              <p className="govuk-inset-text">
                View a regional breakdown of statistics and data under the{' '}
                <a href="#contents-sections-heading-6">
                  Regional and local authority (LA) breakdown
                </a>{' '}
                section
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
            Find out how and why these statistics are collected and published -{' '}
            <Link to="https://assets.publishing.service.gov.uk/government/uploads/system/uploads/attachment_data/file/772862/2018_Key_stage_4_Methodology.pdf">
              GCSE and equivalent results: methodology
            </Link>
          </p>

          <Details summary="Download data files">
            <p>
              You can customise and download data as Excel or .csv files. Our
              data can also be accessed via an API.
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
              2016 to 2017 (latest data)
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

            <h3 className="govuk-heading-s govuk-!-margin-bottom-0">
              <span className="govuk-caption-m">Next update: </span>22 March
              2019
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
                  <Link to="https://assets.publishing.service.gov.uk/government/uploads/system/uploads/attachment_data/file/772862/2018_Key_stage_4_Methodology.pdf">
                    GCSE and equivalent results statistics: methodology
                  </Link>
                </li>
              </ul>
            </nav>
          </aside>
        </div>
      </div>
      <hr />
      <h2 className="govuk-heading-l">
        Headline facts and figures - 2016 to 2017
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
        <AccordionSection heading="About these statistics">
          <p>
            This release provides information on the achievements in GCSE
            examinations and other qualifications of young people in academic
            year 2016 to 2017. This typically covers those starting the academic
            year aged 15.
          </p>
          <p>
            It shows results for GCSE and equivalent Key Stage 4 (KS4)
            qualifications in 2018 across a range of measures, broken down by
            pupil characteristics and education institutions. Results are also
            provided on schools below the floor standards and meeting the
            coasting definition.{' '}
          </p>

          <p>
            This is an update to Provisional figures released in October 2018.
            Users should be careful when comparing headline measures to results
            in previous years given recent methodological changes
          </p>
          <p>
            Figures are available at national, regional, local authority, and
            school level. Figures held in this release are used for policy
            development and count towards the secondary performance tables.
            Schools and local authorities also use the statistics to compare
            their local performance to regional and national averages for
            different pupil groups.
          </p>
        </AccordionSection>

        <AccordionSection heading="School performance">
          <p>
            Results for 2018 show an increases across all headline measures
            compared to 2017.{' '}
            <strong>
              When drawing comparison over time, however, it is very important
              to note any changes to methodology or data changes underpinning
              these measures.
            </strong>{' '}
            For example, changes in Attainment 8 may have been affected by the
            introduction of further reformed GCSEs graded on the 9-1 scale which
            have a higher maximum score than unreformed GCSEs. Similarly, in
            2016 there were significant changes to the Attainment in English and
            Maths measure.
          </p>
          <p>
            These results cover state-funded schools but results for all schools
            are available in the supporting tables and show slightly lower
            performance across all headline measures on average. Differences
            between the figures for all schools and state-funded schools are
            primarily due to the impact of unapproved and unregulated
            qualifications such as international GCSEs taken more commonly in
            independent schools. These qualification are not included in school
            performance tables.
          </p>
          <p>
            There are five primary headline measures used throughout this
            report:
          </p>
          <ul className="govuk-list govuk-list--bullet">
            <li>
              <strong>Attainment8</strong> - measures the average achievement of
              pupils in up to 8 qualifications (including English and Maths).{' '}
            </li>
            <li>
              <strong>Attainment in English &amp; Maths (9-5)</strong> -
              measures the percentage of pupils achieving a grade 5 or above in
              both English and maths.
            </li>
            <li>
              <strong>EBacc Entries</strong> – measure the percentage of pupils
              reaching the English Baccalaureate (EBacc) attainment threshold in
              core academic subjects at key stage 4. The EBacc is made up of
              English, maths, science, a language, and history or geography.
            </li>
            <li>
              <strong>EBacc Average Point Score (APS)</strong> – measures
              pupils’ point scores across the five pillars of the EBacc,
              ensuring the attainment of all pupils is recognised. New measure
              from 2018, replacing the previous threshold EBacc attainment
              measure.
            </li>
            <li>
              <strong>Progress</strong> - measures the progress a pupil makes
              from the end of key stage 2 to the end of key stage 4. It compares
              pupils’ Attainment 8 score with the average for all pupils
              nationally who had a similar starting point. Progress 8 is a
              relative measure, therefore the national average Progress 8 score
              for mainstream schools is very close to zero.
            </li>
          </ul>
        </AccordionSection>
        <AccordionSection heading="Schools meeting the coasting and floor standard">
          <p>
            <strong>
              There is wide variation in the percentage of schools meeting the
              coasting and floor standard by region
            </strong>
          </p>
          <p>
            The floor and coasting standards give measures of whether schools
            are helping pupils to fulfil their potential based on progress
            measures. The floor standard is based on results in the most recent
            year, whereas the Coasting definition looks at slightly different
            measures over the past three years. Only state-funded mainstream
            schools are covered by these measures, subject to certain
            eligibility criteria.
          </p>
          <ul className="govuk-list govuk-list--bullet">
            <li>
              <strong>11.6%</strong> of eligible schools were below the floor
              standard in 2018. This represents 346 schools
            </li>
            <li>
              <strong>9.2%</strong> of eligible schools met the coasting
              definition in 2018. This represents 257 schools
            </li>
            <li>
              <strong>161</strong> schools were both coating and below the floor
              standard
            </li>
            <li>
              due to methodological changes no directly comparable measures
              exist for previous years
            </li>
          </ul>
        </AccordionSection>
        <AccordionSection heading="Pupil characteristics">
          <p>
            Breakdowns by pupil characteristics show that across all headline
            measures:
          </p>
          <ul className="govuk-list govuk-list--bullet">
            <li>girls continue to do better than boys</li>
            <li>
              non-disadvantaged pupils continue to do better than disadvantaged
              pupils
            </li>
            <li>
              pupils with no identified Special Educational Needs (SEN) continue
              to do better perform than SEN pupils
            </li>
          </ul>
          <p>
            In general the pattern of attainment gaps for Attainment 8 in 2018
            remained the same as in 2017 although differences in Attainment 8
            scores widened slightly across all groups. This is to be expected
            due to changes to reformed GCSEs in 2018, meaning more points are
            available for higher scores.{' '}
          </p>
          <p>
            Due to changes in performance measures over time, comparability over
            time is complicated. As such, for disadvantaged pupils is
            recommended to use to disadvantage gap index instead with is more
            resilient to changes in grading systems over time. The gap between
            disadvantaged pupils and others, measured using the gap index, has
            remained broadly stable, widening by 0.6% in 2018, and narrowing by
            9.5% since 2011.
          </p>
        </AccordionSection>
        <AccordionSection heading="Headline performance">
          <p>
            Results across headline measures differ by ethnicity with Chinese
            pupils in particular achieving scores above the national average.
          </p>
          <p>
            Performance across headline measures increased for all major ethnic
            groups from 2017 to 2018, with the exception of EBacc entries for
            white pupils were there was a small decrease.
          </p>
          <p>
            Within the more detailed ethnic groupings, pupils from an Indian
            background are the highest performing group in key stage 4 headline
            measures other than Chinese pupils. Gypsy/Roma pupils and traveller
            of Irish heritage pupils are the lowest performing groups.
          </p>
          <p>
            For context, White pupils made up 75.8% of pupils at the end of key
            stage 4 in 2018, 10.6% were Asian, 5.5% were black, 4.7% were mixed,
            0.4% were Chinese. The remainder are in smaller breakdowns or
            unclassified.
          </p>
        </AccordionSection>
        <AccordionSection heading="Regional and local authority (LA) breakdown">
          <p>
            Performance varies considerably across the country – for Attainment
            8 score per pupil there is nearly a 23 point gap between the poorest
            and highest performing areas. The highest performing local
            authorities are concentrated in London and the south with the
            majority of the lowest performing local authorities are located in
            the northern and midland regions with average Attainment 8 score per
            pupil show that. This is similar to patterns seen in recent years
            and against other performance measures.{' '}
          </p>
        </AccordionSection>
        <AccordionSection heading="Pupil subject areas">
          <p>
            It is compulsory for pupils to study English and Maths at key stage
            4 in state-funded schools.{' '}
          </p>
          <h3>Science</h3>
          <p>
            It is compulsory for schools to teach Science at Key Stage 4. For
            these subjects, the proportion of pupils entering continues to
            increase.{' '}
          </p>
          <p>
            In 2018, 68.0% of the cohort entered the new combined science
            pathway rather than the individual science subjects like Chemistry,
            Biology, Physics or Computer Science. The general pattern is for
            pupils with higher prior attainment tend to take single sciences;
            those with lower prior attainment to opt for the combined science
            pathway; and those with the lowest prior attainment to take no
            science qualifications.
          </p>
          <h3>Humanities</h3>
          <p>
            The proportion of pupils entering EBacc humanities continued to
            increase in 2018, to 78.3% in state-funded schools, a rise of 1.5
            percentage points since 2017. This was driven by small increases in
            entries across the majority of prior attainment groups for
            geography, and small increases in entries for pupils with low and
            average prior attainment for history. In history, the slight
            increase in entries from pupils with low and average prior
            attainment groups was counter-balanced by continued decreases in
            proportion of entries for high prior attainers. This trend has
            continued since 2016.
          </p>
          <h3>Languages</h3>
          <p>
            Entries to EBacc languages continued to decrease in 2018 to 46.1%, a
            fall of 1.3 percentage points compared to 2017. This was the fourth
            year in a row that entries have fallen. There were decreases across
            the majority of prior attainment bands but the largest drop occurred
            for pupils with higher prior attainment.. This decrease in entries
            for pupils with high prior attainment between 2018 and 2017 is much
            smaller than the drop that occurred between 2016 and 2017. Some of
            this drop can be explained by pupils who entered a language
            qualification early in a subject that was subsequently reformed in
            2018. This was the case for over 3,500 pupils, whose language result
            did not count in 2018 performance tables.{' '}
          </p>
          <h3>Art and design subjects</h3>
          <p>
            The percentage of pupils entering at least one arts subject
            decreased in 2018, by 2.2 percentage points compared to equivalent
            data in 2017. 44.3% of pupils in state-funded schools entered at
            least one arts subject. This is the third consecutive year that a
            fall in entries has occurred.{' '}
          </p>
        </AccordionSection>
        <AccordionSection heading="Schools performance">
          [TABLE - Key stage 4 revised attainment data ]
          <p>
            Schools in England can be divided into state-funded and independent
            schools (funded by fees paid by attendees). Independent schools are
            considered separately, because the department holds state-funded
            schools accountable for their performance.{' '}
          </p>
          <p>
            The vast majority of pupils in state-funded schools are in either
            academies (68%) or LA maintained schools (29%).{' '}
            <strong>Converter academies </strong>
            were high performing schools that chose to convert to academies and
            have on average higher attainment across the headline measures.
            <strong>Sponsored academies </strong> were schools that were low
            performing prior to conversion and tend to perform below the average
            for state-funded schools.{' '}
          </p>
          <p>
            Between 2017 and 2018 EBacc entry remained stable for sponsored
            academies, with an increase of 0.1 percentage points to 30.1%. EBacc
            entry fell marginally for converter academies by 0.3 percentage
            points (from 44.2% to 43.8%). Over the same period, EBacc entry in
            local authority maintained schools increased by 0.2 percentage
            points to 37.0%.
          </p>
        </AccordionSection>
        <AccordionSection heading="Attainment">
          <p>
            Academies are state schools directly funded by the government, each
            belonging to a trust. Multi-Academy Trusts (MATs) can be responsible
            for a group of academies and cover around 13.6% of state-funded
            mainstream pupils. Most MATs are responsible for between 3 and 5
            schools but just over 10% cover 11 or more schools.{' '}
          </p>
          <p>
            Generally speaking MATs are typically more likely to cover
            previously poor-performing schools and pupils are more likely to
            have lower prior attainment, be disadvantaged, have special
            educational needs (SEN) or have English as an additional language
            (EAL) than the national average.
          </p>
          <p>
            The number of eligible MATs included in Key Stage 4 measures
            increased from 62 in 2017 to 85 in 2018. This is an increase from
            384 to 494 schools, and from 54,356 to 69,169 pupils.{' '}
          </p>
          <p>
            On Progress8 measures, in 2018, 32.9% of MATs were below the
            national average and 7.1% well below average. 29.4% were not above
            or below the national average by a statistically significant amount.{' '}
          </p>
          <p>
            Entry rate in EBacc is lower in MATs compared to the national
            average – in 2018 43.5% of MATs had an entry rate higher than the
            national average of 39.1%. The EBacc average point score is also
            lower in MATs – 32.9% of MATs had an APS higher than the national
            average.
          </p>
          <p>
            Analysis by characteristics shows that in 2018 disadvantaged pupils
            in MATs made more progress than the national average for
            disadvantaged. However, non-disadvantaged pupils, SEN and non-SEN
            pupils, pupils with English as a first language and high prior
            attainment pupils made less progress than the national average for
            their respective group.
          </p>
        </AccordionSection>
      </Accordion>
      <h2 className="govuk-heading-m govuk-!-margin-top-9">Help and support</h2>
      <Accordion id="extra-information-sections">
        <AccordionSection
          heading="GCSE and equivalent results statistics: methodology"
          caption="How we collect and process statistics and data"
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
          <h4 className="govuk-heading-s govuk-!-margin-bottom-0">
            Attainment statistics team
          </h4>
          <p className="govuk-!-margin-top-0">
            Email
            <br />
            <a href="mailto:attainment.statistics@education.gov.uk">
              attainment.statistics@education.gov.uk
            </a>
          </p>
          <p>
            Telephone: Kathryn Kenney <br />
            01325 340 620
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
