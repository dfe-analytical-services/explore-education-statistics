import AccordionSection from '@common/components/AccordionSection';
import PageSearchFormWithAnalytics from '@frontend/components/PageSearchFormWithAnalytics';
import RelatedInformation from '@common/components/RelatedInformation';
import Link from '@frontend/components/Link';
import Page from '@frontend/components/Page';
import React from 'react';
import Accordion from '@common/components/Accordion';
import { logEvent } from '@frontend/services/googleAnalyticsService';

function GlossaryIndexPage() {
  return (
    <Page title="Glossary">
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-two-thirds">
          <p className="govuk-body-l">
            Browse our A to Z list of definitions for terms used across
            education statistics and data.
          </p>
          <p className="govuk-body">
            The glossary is intended to grow over time as the service is
            populated.
          </p>

          <PageSearchFormWithAnalytics
            inputLabel="Search our A to Z list of definitions for terms used across
            education statistics and data."
          />
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
      <Accordion
        id="a-z"
        onSectionOpen={accordionSection => {
          logEvent('Glossary', 'Accordion opened', accordionSection.title);
        }}
      >
        <AccordionSection heading="A">
          <h3 id="absence">Absence</h3>
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
          <h3 id="academic-year">Academic year</h3>
          <p>
            Lasts from 31 August to 31 July. Generally broken into 3 terms -
            autumn, spring and summer.
          </p>
          <h3 id="ad-hoc-statistics">Ad hoc statistics</h3>
          <p>
            Releases of statistics which are not part of DfE's regular annual
            official statistical release calendar.
          </p>
          <h3 id="authorised-absence">Authorised absence</h3>
          <p>
            When a pupil misses (or is absent from) at least 1 possible school
            session with the permission of a teacher or other authorised school
            representative.
          </p>
          <p>
            Counted in sessions, where 1 session is equivalent to half-a-day.
          </p>
        </AccordionSection>
        <AccordionSection heading="B">
          <p className="govuk-inset-text">
            There are currently no entries under this section
          </p>
        </AccordionSection>
        <AccordionSection heading="C">
          <p className="govuk-inset-text">
            There are currently no entries under this section
          </p>
        </AccordionSection>
        <AccordionSection heading="D">
          <h3 id="dual-main-registered-pupils">Dual main registered pupils</h3>
          <p>
            Dual registered pupils who are enrolled at more than 1 school have a
            dual main registration (at their main school) and 1 or more
            subsidiary registrations (at their additional schools).
          </p>
          <p>
            See also{' '}
            <a href="/glossary#dual-registered-pupils">
              Dual registered pupils
            </a>
            .
          </p>
          <h3 id="dual-registered-pupils">Dual registered pupils</h3>
          <p>Pupils who are enrolled at more than 1 school.</p>
          <p>
            See also{' '}
            <a href="/glossary#dual-main-registered-pupils">
              dual main registered pupils
            </a>
            .
          </p>
        </AccordionSection>
        <AccordionSection heading="E">
          <h3 id="exclusion">Exclusion</h3>
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
          <h3 id="exclusion-rate">Exclusion rate</h3>
          <p>
            The number of exclusions as a percentage of the overall school
            population.
          </p>
          <h3 id="exclusion-review-panel">Exclusion review panel</h3>
          <p>
            The process by which parents (and pupils aged over 18 years) can
            request a review of a permanent exclusion.
          </p>
        </AccordionSection>
        <AccordionSection heading="F">
          <h3 id="fixed-period-exclusion">Fixed-period exclusion</h3>
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
        </AccordionSection>
        <AccordionSection heading="G">
          <p className="govuk-inset-text">
            There are currently no entries under this section
          </p>
        </AccordionSection>
        <AccordionSection heading="H">
          <p className="govuk-inset-text">
            There are currently no entries under this section
          </p>
        </AccordionSection>
        <AccordionSection heading="I">
          <h3 id="independent-review-panel">Independent review panel</h3>
          <p>
            Parents (and pupils if aged over 18 years) can request a review of a
            permanent exclusion.
          </p>
          <p>
            An independent review panel’s role is to review the decision of the
            governing body not to reinstate a permanently excluded pupil.
          </p>
          <p>
            It must consider the interests and circumstances of the excluded
            pupil, including the circumstances in which the pupil was excluded,
            and have regard to the interests of other pupils and people working
            at the school.
          </p>
        </AccordionSection>
        <AccordionSection heading="J">
          <p className="govuk-inset-text">
            There are currently no entries under this section
          </p>
        </AccordionSection>
        <AccordionSection heading="K">
          <p className="govuk-inset-text">
            There are currently no entries under this section
          </p>
        </AccordionSection>
        <AccordionSection heading="L">
          <h3 id="lunchtime-exclusion">Lunchtime exclusion</h3>
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
          <p className="govuk-inset-text">
            There are currently no entries under this section
          </p>
        </AccordionSection>
        <AccordionSection heading="N">
          <h3 id="national-offer-day">National Offer Day</h3>
          <p>
            The day when councils send parents confirmations of their child's
            place at school.
          </p>
          <p>
            Primary school places are confirmed on 16 April each year - known as
            National Primary Offer Day.
          </p>
          <p>
            Secondary school places are confirmed on 1 March each year - known
            as National Secondary Offer Day.
          </p>
          <p>
            If either date falls on a weekend, confirmations are sent the next
            working day.
          </p>
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
          </p>
        </AccordionSection>
        <AccordionSection heading="O">
          <h3 id="one-or-more-fixed-period-exclusion">
            One or more fixed-period exclusion
          </h3>
          <p>
            Pupils who have had at least 1 fixed-period excluions across a full
            academic year.
          </p>
          <p>Includes those with repeated fixed-period exclusions.</p>
          <h3 id="overall-absence">Overall absence</h3>
          <p>
            The total number of all authorised and unauthorised absences from
            possible school sessions for all pupils.
          </p>
          <p>
            Expressed as a percentage of the total number of possible school
            sessions for all pupils.
          </p>
          <p>
            Counted in sessions, where 1 session is equivalent to half-a-day.
          </p>
        </AccordionSection>
        <AccordionSection heading="P">
          <h3 id="permanent-exclusion">Permanent exclusion</h3>
          <p>
            When a pupil is not allowed to attend (or is excluded from) a school
            and cannot go back to that specific school unless their exclusion is
            overturned.
          </p>
          <h3 id="persistent-absence">Persistent absence</h3>
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
            <Link
              to="/methodology/[methodology]"
              as="/methodology/pupil-absence-in-schools-in-england"
            >
              Pupil absence statistics: methodology
            </Link>{' '}
            guidance.
          </p>
          <h3 id="possible-school-session">Possible school session</h3>
          <p>
            Schools are required to provide 2 possible sessions per day - 1
            session in the morning and 1 in the afternoon.
          </p>
          <p>This is why a session is equivalent to half-a-day.</p>
          <h3 id="pupil-enrolment">Pupil enrolment</h3>
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
          <h3 id="pupil-referral-unit">Pupil referral unit (PRUs)</h3>
          <p>
            An alternative education provision specifically organised to provide
            education for children who are not able to attend school and may not
            otherwise receive a suitable education.
          </p>
          <p>
            This could be because they have a short- or long-term illness, have
            been excluded from school or are a new starter waiting for a
            mainstream school place.
          </p>
          <p>
            Under section 19 of the Education Act 1996, each local education
            authority (LEA) has a duty to provide suitable education for
            children of compulsory school age who cannot attend school.
          </p>
          <p>
            Placing pupils in PRUs is one of the ways in which LEAs can make
            sure they can comply with this duty.
          </p>
          <p>
            PRUs are a mixture of public units and privately managed companies.
          </p>
          <h3 id="pupils-with-1-or-more-fixed-period-exclusion">
            Pupils with one or more fixed-period exclusion
          </h3>
          <p>
            Pupil who have had at least one fixed-period exclusion across a full
            academic year.
          </p>
        </AccordionSection>
        <AccordionSection heading="Q">
          <p className="govuk-inset-text">
            There are currently no entries under this section
          </p>
        </AccordionSection>
        <AccordionSection heading="R">
          <p className="govuk-inset-text">
            There are currently no entries under this section
          </p>
        </AccordionSection>
        <AccordionSection heading="S">
          <h3 id="school-census">School census</h3>
          <p>
            Statutory termly data collection for all of the following
            educational organisations in England:
          </p>
          <ul>
            <li>academies - including free schools and studio schools</li>
            <li>
              colleges - including city technology and university technical
              colleges
            </li>
            <li>
              maintained schools - including nurseries, middle-deemed primary
              and secondary schools, primary schools and secondary schools
            </li>
            <li>
              special schools - including local authority maintained and
              non-maintained special schools
            </li>
          </ul>
          <h3 id="school-session">School session</h3>
          <p>
            See{' '}
            <a href="/glossary#possible-school-session">
              possible school session
            </a>
          </p>
          <h3 id="school-year">School year</h3>
          <p>
            See <a href="/glossary#academic-year">academic year</a>
          </p>
          <h3 id="sole-registered-pupils">Sole registered pupils</h3>
          <p>Pupils who are on the roll of only 1 school.</p>
        </AccordionSection>
        <AccordionSection heading="T">
          <p className="govuk-inset-text">
            There are currently no entries under this section
          </p>
        </AccordionSection>
        <AccordionSection heading="U">
          <h3 id="unauthorised-absence">Unauthorised absence</h3>
          <p>
            When a pupil misses at least 1 possible school session without the
            permission of a teacher or other authorised school representative.
          </p>
          <p>
            Includes all unexplained or unjustified absences and late arrivals.
          </p>
          <p>
            Counted in sessions, where 1 session is equivalent to half-a-day.
          </p>
        </AccordionSection>
        <AccordionSection heading="V">
          <p className="govuk-inset-text">
            There are currently no entries under this section
          </p>
        </AccordionSection>
        <AccordionSection heading="W">
          <p className="govuk-inset-text">
            There are currently no entries under this section
          </p>
        </AccordionSection>
        <AccordionSection heading="X">
          <p className="govuk-inset-text">
            There are currently no entries under this section
          </p>
        </AccordionSection>
        <AccordionSection heading="Y">
          <p className="govuk-inset-text">
            There are currently no entries under this section
          </p>
        </AccordionSection>
        <AccordionSection heading="Z">
          <p className="govuk-inset-text">
            There are currently no entries under this section
          </p>
        </AccordionSection>
      </Accordion>
    </Page>
  );
}

export default GlossaryIndexPage;
