import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import RelatedInformation from '@common/components/RelatedInformation';
import Link from '@frontend/components/Link';
import Page from '@frontend/components/Page';
import PageTitle from '@frontend/components/PageTitle';
import React from 'react';

function GlossaryIndexPage() {
  return (
    <Page breadcrumbs={[{ name: 'Glossary' }]}>
      <PageTitle title="Education statistics: glossary" />
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-two-thirds">
          <p className="govuk-body-l">
            Browse our A to Z list of definitions for terms used across
            education statistics and data.
          </p>
        </div>
        <div className="govuk-grid-column-one-third">
          <RelatedInformation>
            <ul className="govuk-list">
              <li>
                <Link to="/methodology">Education statistics: methodology</Link>
              </li>
            </ul>
          </RelatedInformation>
        </div>
      </div>
      <Accordion id="a-z">
        <AccordionSection heading="A">
          <h3 id="absence">absence</h3>
          <p>
            When a pupil misses (or is absent from) at least 1 possible school
            session.
          </p>
          <p>
            Counted in sessions, where 1 session is equivalent to half-a-day.
          </p>
          <p>There are 4 types of absence:</p>
          <ul>
            <li>
              <a href="/glossary#authorised-absence">authorised absence</a>
            </li>
            <li>
              <a href="/glossary#overall-absence">overall absence</a>
            </li>
            <li>
              <a href="/glossary#persistent-absence">persistent absence</a>
            </li>
            <li>
              <a href="/glossary#unauthorised-absence">unauthorised absence</a>
            </li>
          </ul>
          <h3 id="academic-year">academic year</h3>
          <p>
            Lasts from 31 August to 31 July. Generally broken into 3 terms -
            autumn, spring and summer.
          </p>
          <h3 id="academic-year">ad hoc statistics</h3>
          <p>
            Releases of statistics which are not part of DfE's regular annual
            official statistical release calendar.
          </p>
          <h3 id="authorised-absence">authorised absence</h3>
          <p>
            When a pupil misses (or is absent from) at least 1 possible school
            sessionf with the permission of a teacher or other authorised school
            representative.
          </p>
          <p>
            Counted in sessions, where 1 session is equivalent to half-a-day.
          </p>
        </AccordionSection>
        <AccordionSection heading="B">
          <h3 id="btec-national-diploma">BTEC National Diploma</h3>
          <p>INSERT DEFINITION - TBC</p>
        </AccordionSection>
        <AccordionSection heading="C">
          <h3 id="chair-of-governors">chair of governors</h3>
          <p>INSERT DEFINITION - TBC.</p>
          <h3 id="children-in-need">Children in Need</h3>
          <p>INSERT DEFINITION - TBC.</p>
        </AccordionSection>
        <AccordionSection heading="D">
          <h3 id="dedicated-schools-grant">dedicated schools grant</h3>
          <p>INSERT DEFINITION - TBC.</p>
          <h3 id="dual-main-registered-pupils">dual main registered pupils</h3>
          <p>
            Dual registered pupils who are enrolled at more than 1 school have a
            dual main registration (at their main school) and 1 or more
            subsidiary registrations (at their additional schools).
          </p>
          <p>
            See also <a href="dual-registered-pupils">dual registered pupils</a>
            .
          </p>
          <h3 id="dual-registered-pupils">dual registered pupils</h3>
          <p>Pupils who are enrolled at more than 1 school.</p>
          <p>
            See also{' '}
            <a href="dual-main-registered-pupils">
              dual main registered pupils
            </a>
            .
          </p>
        </AccordionSection>
        <AccordionSection heading="E">
          <h3 id="early-years">early years</h3>
          <p>INSERT DEFINITION - TBC.</p>
          <h3 id="early-years-foundation-stage-eyfs">
            early years foundation stage (EYFS)
          </h3>
          <p>INSERT DEFINITION - TBC.</p>
          <h3 id="exclusion">exclusion</h3>
          <p>
            When a pupil is not allowed to attend (or is excluded from) a
            school.
          </p>
          <p>There are 2 types of exclusion:</p>
          <ul>
            <li>
              <a href="/glossary#fixed-period-exclusion">
                fixed-period exclusion
              </a>
            </li>
            <li>
              <a href="/glossary#permanent-exclusion">permanent exclusion</a>
            </li>
          </ul>
          <h3 id="exclusion-rate">exclusion rate</h3>
          <p>
            The number of exclusions as a percentage of the overall school
            population.
          </p>
          <h3 id="exclusion-review-panel">exclusion review panel</h3>
          <p>
            The process by which parents (and pupils aged over 18 years) can
            request a review of a permanent exclusion.
          </p>
        </AccordionSection>
        <AccordionSection heading="F">
          <h3 id="fixed-period-exclusion">fixed-period exclusion</h3>
          <p>
            When a pupil is not allowed to attend (or is excluded from) a school
            for a set period of time.
          </p>
          <p>
            This can be for part of a school day and does not have to be for a
            continuous period.
          </p>
          <p>
            A pupil can be excluded for 1 or more fixed periods up to a maximum
            of 45 school days in a single academic year.
          </p>
          <p>
            This total includes exclusions from previous schools covered by the
            exclusion legislation so pupils with repeat exclusions can inflate
            fixed-period exclusion rates.
          </p>
          <h3 id="foundation-degrees">foundation degrees</h3>
          <p>INSERT DEFINITION - TBC.</p>
          <h3 id="foundation-schools">foundation schools</h3>
          <p>INSERT DEFINITION - TBC.</p>
          <h3 id="foundation-stage--foundation-subjects">
            foundation stage / foundation subjects
          </h3>
          <p>INSERT DEFINITION - TBC.</p>
          <h3 id="free-school">free school</h3>
          <p>INSERT DEFINITION - TBC.</p>
          <h3 id="free-school-meals">free school meals</h3>
          <p>INSERT DEFINITION - TBC.</p>
          <h3 id="further-education-fe">further education (FE)</h3>
          <p>INSERT DEFINITION - TBC.</p>
        </AccordionSection>
        <AccordionSection heading="G">
          <h3 id="gcse-gcses">GCSE, GCSEs</h3>
          <p>INSERT DEFINITION - TBC.</p>
          <h3 id="grammar-school">grammar school</h3>
          <p>INSERT DEFINITION - TBC.</p>
        </AccordionSection>
        <AccordionSection heading="H">
          <h3 id="headteacher">headteacher</h3>
          <p>INSERT DEFINITION - TBC.</p>
          <h3 id="higher-education-he">higher education (HE)</h3>
          <p>INSERT DEFINITION - TBC.</p>
        </AccordionSection>
        <AccordionSection heading="I">
          <h3 id="individual-schools-budget">individual schools budget</h3>
          <p>INSERT DEFINITION - TBC.</p>
          <h3 id="initial-teacher-training">initial teacher training</h3>
          <p>INSERT DEFINITION - TBC.</p>
          <h3 id="inset-day">inset day</h3>
          <p>INSERT DEFINITION - TBC.</p>
          <h3 id="international-baccalaureate">International Baccalaureate</h3>
          <p>INSERT DEFINITION - TBC.</p>
        </AccordionSection>
        <AccordionSection heading="J">
          <h3 id="jjj-jjj">SOMETHING BEGINNING WITH J</h3>
          <p>INSERT DEFINITION - TBC.</p>
        </AccordionSection>
        <AccordionSection heading="K">
          <h3 id="key-stage">key stage</h3>
          <p>INSERT DEFINITION - TBC.</p>
        </AccordionSection>
        <AccordionSection heading="L">
          <h3 id="local-authority">local authority</h3>
          <p>INSERT DEFINITION - TBC.</p>
          <h3 id="looked-after-children">looked-after children</h3>
          <p>INSERT DEFINITION - TBC.</p>
          <h3 id="lunchtime-exclusion">lunchtime exclusion</h3>
          <p>
            When a pupil is not allowed to attend (or is excluded from) a school
            for the duration of the school's lunchtime period.
          </p>
          <p>
            A pupil is excluded in this way when their lunchtime behaviour has
            been disruptive.
          </p>
        </AccordionSection>
        <AccordionSection heading="M">
          <h3 id="mainstream-schools">mainstream schools</h3>
          <p>INSERT DEFINITION - TBC.</p>
          <h3 id="maintained-schools-maintained-nursery-schools">
            maintained schools, maintained nursery schools
          </h3>
          <p>INSERT DEFINITION - TBC.</p>
          <h3 id="middle-deemed-primary-school-middle-deemed-secondary-school">
            middle-deemed primary school, middle-deemed secondary school
          </h3>
          <p>INSERT DEFINITION - TBC.</p>
          <h3 id="mixed-sex-schools">mixed-sex schools</h3>
          <p>INSERT DEFINITION - TBC.</p>
          <h3 id="multi-academy-trust">multi-academy trust</h3>
          <p>INSERT DEFINITION - TBC.</p>
          <h3 id="multi-ethnic">multi-ethnic</h3>
          <p>INSERT DEFINITION - TBC.</p>
        </AccordionSection>
        <AccordionSection heading="N">
          <h3 id="national-curriculum">national curriculum</h3>
          <p>INSERT DEFINITION - TBC.</p>
          <h3 id="national-curriculum-tests">national curriculum tests</h3>
          <p>Not SATs - INSERT DEFINITION - TBC.</p>
          <h3 id="national-pupil-database">national pupil database</h3>
          <p>INSERT DEFINITION - TBC.</p>
          <h3 id="national-scholarship-fund">national scholarship fund</h3>
          <p>INSERT DEFINITION - TBC.</p>
          <h3 id="national-scholarship-fund">National Statistician</h3>
          <p>
            Chief Executive of the UK Statistics Authority (UKSA) and the Head
            of the UK Government Statistical Service (GSS).
          </p>
          <p>
            The current National Statistician is{' '}
            <a href="https://www.gov.uk/government/people/john-pullinger">
              John Pullinger
            </a>
            .
          </p>
          <h3 id="newly-qualified-teacher">newly qualified teacher</h3>
          <p>INSERT DEFINITION - TBC.</p>
          <h3 id="nursery-school">nursery school</h3>
          <p>INSERT DEFINITION - TBC.</p>
        </AccordionSection>
        <AccordionSection heading="O">
          <h3 id="one-or-more-fixed-period-exclusion">
            one or more fixed-period exclusion
          </h3>
          <p>
            Pupils who have had at least 1 fixed-period excluions across a full
            academic year.
          </p>
          <p>Includes those with repeated fixed-period exclusions.</p>
          <h3 id="overall-absence">overall absence</h3>
          <p>
            The total number of all authorised and unauthorised absences from
            possible school sessions for all pupils.
          </p>
          <p>
            Expressed as a percentage of the total number of possible sessions
            for all pupils.
          </p>
          <p>
            Counted in sessions, where 1 session is equivalent to half-a-day.
          </p>
          <h3 id="ofsted-judgements">Ofsted judgements</h3>
          <p>INCLUDE DEFINITION IN HERE....TBC</p>
          <p>There are 4 Ofsted grades:</p>
          <ul>
            <li>outstanding (or grade 1)</li>
            <li>good (or grade 2)</li>
            <li>requires improvement (or grade 3)</li>
            <li>inadequate (or grade 4)</li>
          </ul>
          <h3 id="one-year-on">one-year-on</h3>
          <p>If used adjectivally, hyphenate and use one rather than 1.</p>
        </AccordionSection>
        <AccordionSection heading="P">
          <h3 id="permanent-exclusion">permanent exclusion</h3>
          <p>
            When a pupil is not allowed to attend (or is excluded from) a school
            and cannot go back to that specific school unless their exclusion is
            overturned.
          </p>
          <h3 id="persistent-absence">persistent absence</h3>
          <p>
            When a pupil misses (or is absent from) 10% or more possible school
            sessions.
          </p>
          <p>
            Counted in sessions, where 1 session is equivalent to half-a-day.
          </p>
          <p>
            This definition changed at the start of the 2015/16 academic year.
          </p>
          <p>
            For further information on this change read our{' '}
            <a href="/prototypes/methodology-absence">
              Pupil absence statistics: methodology
            </a>{' '}
            guidance.
          </p>
          <h3 id="possible-school-session">possible school session</h3>
          <p>
            Schools are required to provide 2 possible sessions per day - 1
            session in the morning and 1 in the afternoon.
          </p>
          <p>This is why a session is equivalent to half-a-day.</p>
          <h3 id="pupil-enrolment">pupil enrolment</h3>
          <p>
            A way to refer to a 'pupil' at a school. The number of pupil
            enrolments presented includes pupils on the school roll for at least
            1 session who are aged between 5 and 15 years, excluding boarders.
          </p>
          <p>
            Some pupils may be counted more than once. For example, if they
            moved schools during the academic year or are registered in more
            than 1 school.
          </p>
          <h3 id="performance-tables">performance tables</h3>
          <p>INSERT DEFINITION - TBC.</p>
          <h3 id="pupil-referral-unit">pupil referral unit</h3>
          <p>INSERT DEFINITION - TBC</p>
          <h3 id="pupils-with-1-or-more-fixed-period-exclusion">
            pupils with 1 or more fixed-period exclusion
          </h3>
          <p>
            Pupil who have had at least 1 fixed period exclusion across a full
            academic year.
          </p>
        </AccordionSection>
        <AccordionSection heading="Q">
          <h3 id="qualified-teacher-status">qualified teacher status</h3>
          <p>INSERT DEFINITION - TBC.</p>
        </AccordionSection>
        <AccordionSection heading="R">
          <h3 id="rrr-rrr">SOMETHING BEGINNING WITH R</h3>
          <p>INSERT DEFINITION - TBC.</p>
        </AccordionSection>
        <AccordionSection heading="S">
          <h3 id="same-sex-schools">same-sex schools</h3>
          <p>INSERT DEFINITION - TBC.</p>
          <h3 id="sats">SATs</h3>
          <p>
            <a href="#national-curriculum-tests">
              See national curriculum tests
            </a>
            .
          </p>
          <h3 id="school-admissions-code">School Admissions Code</h3>
          <p>INSERT DEFINITION - TBC.</p>
          <h3 id="school-and-college-performance-tables">
            school and college performance tables
          </h3>
          <p>INSERT DEFINITION - TBC.</p>
          <h3 id="school-census">school census</h3>
          <p>
            Statutory termly data collection for all maintained nursery,
            primary, secondary, middle-deemed primary, middle-deemed secondary,
            local authority maintained special and non-maintained special
            schools, academies including free schools, studio schools and
            university technical colleges and city technology colleges in
            England.
          </p>
          <h3 id="school-improvement-plan">school improvement plan</h3>
          <p>INSERT DEFINITION - TBC.</p>
          <h3 id="school-session">school session</h3>
          <p>
            See{' '}
            <a href="/glossary#possible-school-session">
              possible school session
            </a>
            .
          </p>
          <h3 id="schools-workforce">schools workforce</h3>
          <p>INSERT DEFINITION - TBC.</p>
          <h3 id="sixth-form-college">sixth-form college</h3>
          <p>INSERT DEFINITION - TBC.</p>
          <h3 id="sole-registered-pupils">sole registered pupils</h3>
          <p>Pupils who are on the roll of 1 school only.</p>
          <h3 id="special-educational-needs-code-of-practice">
            Special Educational Needs Code of Practice
          </h3>
          <p>INSERT DEFINITION - TBC.</p>
          <h3 id="special-measures">special measures</h3>
          <p>INSERT DEFINITION - TBC.</p>
          <h3 id="summer-school">summer school</h3>
          <p>INSERT DEFINITION - TBC.</p>
        </AccordionSection>
        <AccordionSection heading="T">
          <h3 id="techbacc">TechBacc</h3>
          <p>INSERT DEFINITION - TBC.</p>
          <h3 id="trust-school">trust school</h3>
          <p>INSERT DEFINITION - TBC.</p>
        </AccordionSection>
        <AccordionSection heading="U">
          <h3 id="unauthorised-absence">unauthorised absence</h3>
          <p>
            When a pupil misses (or is from) at least 1 possible school session
            without the permission of a teacher or other authorised school
            representative.
          </p>
          <p>
            Includes all unexplained or unjustified absences and late arrivals.
          </p>
          <p>
            Counted in sessions, where 1 session is equivalent to half-a-day.
          </p>
          <h3 id="university-technical-college">
            university technical college
          </h3>
          <p>INSERT DEFINITION - TBC.</p>
        </AccordionSection>
        <AccordionSection heading="V">
          <h3 id="voluntary-aided-schools-voluntary-controlled-schools">
            voluntary-aided schools, voluntary-controlled schools
          </h3>
          <p>INSERT DEFINITION - TBC.</p>
        </AccordionSection>
        <AccordionSection heading="W">
          <h3 id="what-is-an-api">What is an API?</h3>
          <p>INSERT DEFINITION - TBC.</p>
        </AccordionSection>
        <AccordionSection heading="X">
          <h3 id="xxx-xxx">SOMETHING BEGINNIG WITH X</h3>
          <p>INSERT DEFINITION - TBC.</p>
        </AccordionSection>
        <AccordionSection heading="Y">
          <h3 id="year-1-year-2">year 1, year 2</h3>
          <p>INSERT DEFINITION - TBC.</p>
        </AccordionSection>
        <AccordionSection heading="Z">
          <div>
            <h3 id="zzz-zzz">SOMETHING BEGINNING WITH Z</h3>
            <p>INSERT DEFINITION - TBC.</p>
          </div>
        </AccordionSection>
      </Accordion>
    </Page>
  );
}

export default GlossaryIndexPage;
