import React from 'react';

const PrototypeMethodologySection = () => {
  return (
    <>
      <p>
        The following are key terms used in published absence statistics and
        their definitions:
      </p>
      <dl className="govuk-list">
        <dt>Academic year</dt>
        <dd>
          31st August to the 31st July.
          <br />
          The academic year is generally broken into three terms, autumn spring
          and summer.
        </dd>
        <dt>Authorised absence</dt>
        <dd>
          Absence with permission from a teacher or other authorised
          representative of the schools. Counted in sessions, where each session
          is equivalent to half a day.
        </dd>
        <dt>Overall absence</dt>
        <dd>
          The aggregated total of all authorised and unauthorised absences,
          counted in sessions where each session is equivalent to half a day.
        </dd>
        <dt>Persistent absence</dt>
        <dd>
          A pupil enrolment is identified as a persistent absentee if they miss
          10 per cent or more of their own possible sessions.
        </dd>
        <dt>Possible session</dt>
        <dd>
          Schools are required to provide two possible sessions per day, where
          one session is the equivalent to half a day i.e. one session in the
          morning and one in the afternoon.
        </dd>
        <dt>Pupil enrolment</dt>
        <dd>
          The number of pupil enrolments presented includes pupils on the school
          roll for at least one session who are aged between 5 and 15, excluding
          boarders. Some pupils may be counted more than once (if they moved
          schools during the academic year or are registered in more than one
          school).
        </dd>
        <dt>School census</dt>
        <dd>
          Statutory termly data collection for all maintained nursery, primary,
          secondary, middle-deemed primary, middle-deemed secondary, local
          authority maintained special and non-maintained special schools,
          academies including free schools, studio schools and university
          technical colleges and city technology colleges in England.
        </dd>
        <dt>Unauthorised absence</dt>
        <dd>
          Absence without permission from a teacher or other authorised
          representative of the school. This includes all unexplained or
          unjustified absences and late arrivals. Counted in sessions, where
          each session is equivalent to half a day.
        </dd>
      </dl>
    </>
  );
};

export default PrototypeMethodologySection;
