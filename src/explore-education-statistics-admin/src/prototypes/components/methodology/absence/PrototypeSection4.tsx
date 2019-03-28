import React from 'react';

const PrototypeMethodologySection = () => {
  return (
    <>
      <h3 id="section4-1"> The collection process 2005/06 to present</h3>
      <p>
        The school census collects information for a pupil’s absence in the term
        prior to the census i.e. one term in arrears. For example, the spring
        school census collects information on absence during the autumn term.
        Data is collected one term in arrears to ensure all absences are
        accounted for and recorded in each term.
      </p>
      <p>
        Within the autumn census all schools are required to provide the first
        and second halves of the summer term’s absence figures separately. This
        allows total absence levels to be calculated for both the full year and
        the first five half terms. See the{' '}
        <a href="#section2-4">
          school year (five half terms vs six half terms)
        </a>{' '}
        section for more information.
      </p>
      <table className="govuk-table">
        <caption className="govuk-table-caption">
          Table 6: School census pupil absence collection schedule
        </caption>
        <thead>
          <tr>
            <th scope="col">Phase</th>
            <th scope="col">Census</th>
            <th scope="col">Schedule</th>
          </tr>
        </thead>
        <tbody>
          <tr>
            <td rowSpan={3}>
              State-funded primary,secondary and special schools <sup>3</sup>
            </td>
            <td>Spring Census</td>
            <td>
              Collects autumn term absence - From 1st August to 31st December
            </td>
          </tr>
          <tr>
            <td>Summer Census</td>
            <td>
              Collects spring term absence - From 1st January to Easter Sunday
            </td>
          </tr>
          <tr>
            <td>Autumn Census</td>
            <td>
              Collects first half of summer term absence - From Easter Monday to
              Sunday before spring bank holiday Collects second half of summer
              term absence - From spring bank holiday to 31st July
            </td>
          </tr>
        </tbody>
        <tfoot>
          <tr>
            <td colSpan={3} className="govuk-body-s">
              <sup>3</sup>Prior to the 2016/17 academic year absence information
              from special schools was collected on an annual basis only, this
              was be collected each year via the autumn school census.
            </td>
          </tr>
        </tfoot>
      </table>
      <p>
        Schools submit their school census returns via the Department’s data
        collection software, COLLECT. Guidance on the school census, including
        absence, is available on the{' '}
        <a href="https://www.gov.uk/government/collections/school-census">
          DfE statistics website
        </a>
        .
      </p>
      <h3 id="section4-2">Background of absence data collection</h3>
      <p>
        The following sections outline how absence data collection has changed
        since it was first collected in 1993/94. A timeline is also available in
        Annex F.
      </p>
      <p>
        High level absence information showing rates from before and after the
        absence collection moved to the school census is available in Annex G
      </p>
      <h4>
        2005/06 to present: Pupil absence information is collected via the
        school census
      </h4>
      <p>
        In spring 2006 the school census started to collect enrolment level
        absence data on a termly basis (spring, summer and autumn collections)
        from maintained secondary schools, city technology colleges and
        academies relating to absence in the 2005/06 academic year.{' '}
      </p>
      <p>
        In the spring 2007 school census the scope of the collection was
        extended to maintained primary schools, collecting absence data for the
        2006/07 academic year.
      </p>
      <p>
        Reason for absence was also collected for the first time in the spring
        2007 school census, collecting absence by reason data for the autumn
        term 2006.{' '}
      </p>
      <p>
        Prior to the 2016/17 academic year special schools did not provide
        termly absence data; instead they provided annual enrolment level
        absence returns. Special school absence information was collected for
        the first time in the autumn 2007 school census, collecting absence data
        for the 2006/07 academic year.
      </p>
      <p>
        From September 2011 data collection systems were amended so that the
        national attendance code “D” (dual registered) was no longer counted in
        the school census as an attendance or a possible session. Following
        this, schools should only record the pupil’s attendance and absence for
        those sessions that a pupil was scheduled to attend at that school and
        the code “D” attendance code should be used to signify a session
        attended via the dual school. This means that any dual registered pupils
        attending full time at their dual school (school B) for the period would
        have zero possible sessions recorded for school A.{' '}
      </p>
      <p>
        Absence data for four year olds was collected for the first time in the
        spring 2013 school census, collecting absence information for the autumn
        term 2012.{' '}
      </p>
      <p>
        Absence data for the second half of the summer term (the sixth half
        term) was collected by the Department for the first time in the autumn
        2013 school census; previously absence data had only been collected for
        autumn term, spring term and the first half of the summer term. An
        amended persistent absentee threshold for the full year was also
        introduced to account for the additional half term of absence
      </p>
      <p>
        Whilst six half term data is more indicative of the academic year and
        therefore became the main measure for overall absence from this point,
        five half term data is still published alongside six half term data up
        to and including the 2013/14 academic year (until a meaningful time
        series for six half term data is established). Table 7 provides a
        comparison of absence figures based on five half terms and six half
        terms, for the full 2012/13 academic year, when data for the sixth half
        term was first included.
      </p>
      <table className="govuk-table">
        <caption className="govuk-table-caption">
          Table 7: Comparison of absence indicators for the 2012/13 academic
          year based on five half terms and six half terms
        </caption>
        <thead>
          <tr>
            <td />
            <th scope="col" className="govuk-table__header--numeric">
              Five half terms
            </th>
            <th scope="col" className="govuk-table__header--numeric">
              Six half terms
            </th>
          </tr>
        </thead>
        <tbody>
          <tr>
            <th colSpan={3}>
              State-funded primary, secondary and special schools
            </th>
          </tr>
          <tr>
            <td>Overall absence</td>
            <td className="govuk-table__cell--numeric">5.2</td>
            <td className="govuk-table__cell--numeric">5.3</td>
          </tr>
          <tr>
            <td>Authorised absence</td>
            <td className="govuk-table__cell--numeric">4.2</td>
            <td className="govuk-table__cell--numeric">4.2</td>
          </tr>
          <tr>
            <td>Unauthorised absence</td>
            <td className="govuk-table__cell--numeric">1.0</td>
            <td className="govuk-table__cell--numeric">1.1</td>
          </tr>
          <tr>
            <td>
              Percentage of pupil enrolments that are persistent absentees (1)
            </td>
            <td className="govuk-table__cell--numeric">4.6</td>
            <td className="govuk-table__cell--numeric">4.6</td>
          </tr>
          <tr>
            <th colSpan={3}>State-funded primary schools</th>
          </tr>
          <tr>
            <td>Overall absence</td>
            <td className="govuk-table__cell--numeric">5.2</td>
            <td className="govuk-table__cell--numeric">5.3</td>
          </tr>
          <tr>
            <td>Authorised absence</td>
            <td className="govuk-table__cell--numeric">4.2</td>
            <td className="govuk-table__cell--numeric">4.2</td>
          </tr>
          <tr>
            <td>Unauthorised absence</td>
            <td className="govuk-table__cell--numeric">1.0</td>
            <td className="govuk-table__cell--numeric">1.1</td>
          </tr>
          <tr>
            <td>
              Percentage of pupil enrolments that are persistent absentees (1)
            </td>
            <td className="govuk-table__cell--numeric">4.6</td>
            <td className="govuk-table__cell--numeric">4.6</td>
          </tr>
          <tr>
            <th colSpan={3}>State-funded secondary schools</th>
          </tr>
          <tr>
            <td>Overall absence</td>
            <td className="govuk-table__cell--numeric">5.2</td>
            <td className="govuk-table__cell--numeric">5.3</td>
          </tr>
          <tr>
            <td>Authorised absence</td>
            <td className="govuk-table__cell--numeric">4.2</td>
            <td className="govuk-table__cell--numeric">4.2</td>
          </tr>
          <tr>
            <td>Unauthorised absence</td>
            <td className="govuk-table__cell--numeric">1.0</td>
            <td className="govuk-table__cell--numeric">1.1</td>
          </tr>
          <tr>
            <td>
              Percentage of pupil enrolments that are persistent absentees (1)
            </td>
            <td className="govuk-table__cell--numeric">4.6</td>
            <td className="govuk-table__cell--numeric">4.6</td>
          </tr>
          <tr>
            <th colSpan={3}>Special schools</th>
          </tr>
          <tr>
            <td>Overall absence</td>
            <td className="govuk-table__cell--numeric">5.2</td>
            <td className="govuk-table__cell--numeric">5.3</td>
          </tr>
          <tr>
            <td>Authorised absence</td>
            <td className="govuk-table__cell--numeric">4.2</td>
            <td className="govuk-table__cell--numeric">4.2</td>
          </tr>
          <tr>
            <td>Unauthorised absence</td>
            <td className="govuk-table__cell--numeric">1.0</td>
            <td className="govuk-table__cell--numeric">1.1</td>
          </tr>
          <tr>
            <td>
              Percentage of pupil enrolments that are persistent absentees (1)
            </td>
            <td className="govuk-table__cell--numeric">4.6</td>
            <td className="govuk-table__cell--numeric">4.6</td>
          </tr>
        </tbody>
        <tfoot>
          <tr>
            <td colSpan={3} className="govuk-body-s">
              (1) Persistent absence defined with the 15% threshold methodology
              in place at the time of the change
            </td>
          </tr>
        </tfoot>
      </table>
      <p>
        In spring 2014, the scope of the collection was extended again to
        collect termly pupil referral unit (PRU) absence information, relating
        to absence in the 2013/14 academic year. Previously absence data for
        PRUs had been collected annually via the PRU Census, where the absence
        information related to the previous academic year. This Census has now
        been amalgamated into the school census. Pupil level PRU absence
        information was collected from all PRUs via the PRU Census for 2009/10
        to 2011/12; however no absence information for the 2012/13 academic year
        was collected for PRUs due to the move from PRU Census to school census.
      </p>
      <p>
        As of the spring 2014 school census, code “F” (authorised absence due to
        agreed extended family holiday) was discontinued following an amendment
        to the Education (Pupil Registration) (England) Regulations 2006. In the
        Pupil absence in schools in England: 2013 to 2014 release any extended
        family holiday absences (code “F”) recorded by schools have been
        combined with authorised family holiday absences (code “H”).
      </p>
      <p>
        Termly absence data for special schools was collected by the Department
        for the first time in the autumn 2016 school census; previously absence
        data had been collected from special schools on an annual basis only and
        therefore in year figures (the autumn and autumn/spring terms combined)
        could not be calculated.
      </p>
      <h4>
        1993/94 to 2004/05: Pupil absence information was collected via the
        Absence in Schools Survey
      </h4>
      <p>
        Prior to 2005/06 the Department’s main source of absence data was the
        Absence in Schools Survey which was conducted in May each year and
        collected, at school level, the number of day pupils of compulsory
        school age together with information on the number of sessions missed
        due to authorised and unauthorised absence. Absence information was
        collected from maintained primary, secondary, all special and
        independent schools, city technology colleges and academies in England.{' '}
      </p>
      <p>
        Data covering the 2005/06 academic year from both sources shows that the
        school census provided information on more pupil enrolments and tended
        to have higher rates of absence reported than the Absence in Schools
        Survey. Table 8 provides a comparison of absence figures based on the
        school census and the Absence in Schools Survey.
      </p>
      <p>
        For more information on the change from the Absence in Schools Survey to
        the school census, please see Pupil Absence in Secondary Schools in
        England, 2005/06.
      </p>
      <table className="govuk-table">
        <caption className="govuk-table-caption">
          Table 8: Comparison of absence indicators for the 2005/06 academic
          year based on the Absence in Schools Survey and the school census
        </caption>
        <thead>
          <tr>
            <td />
            <th scope="col" className="govuk-table__header--numeric">
              Absence in Schools survey
            </th>
            <th scope="col" className="govuk-table__header--numeric">
              School Census
            </th>
          </tr>
        </thead>
        <tbody>
          <tr>
            <th colSpan={3}>Maintained Secondary Schools</th>
          </tr>
          <tr>
            <td>Number of enrolments</td>
            <td className="govuk-table__cell--numeric">3,017,628</td>
            <td className="govuk-table__cell--numeric">3,024,728</td>
          </tr>
          <tr>
            <td>Overall absence</td>
            <td className="govuk-table__cell--numeric">7.94</td>
            <td className="govuk-table__cell--numeric">8.24</td>
          </tr>
          <tr>
            <td>Authorised absence</td>
            <td className="govuk-table__cell--numeric">6.74</td>
            <td className="govuk-table__cell--numeric">6.82</td>
          </tr>
          <tr>
            <td>Unauthorised absence</td>
            <td className="govuk-table__cell--numeric">1.20</td>
            <td className="govuk-table__cell--numeric">1.42</td>
          </tr>
          <tr>
            <th colSpan={3}>City Technology Colleges</th>
          </tr>
          <tr>
            <td>Number of enrolments</td>
            <td className="govuk-table__cell--numeric">3,017,628</td>
            <td className="govuk-table__cell--numeric">3,024,728</td>
          </tr>
          <tr>
            <td>Overall absence</td>
            <td className="govuk-table__cell--numeric">7.94</td>
            <td className="govuk-table__cell--numeric">8.24</td>
          </tr>
          <tr>
            <td>Authorised absence</td>
            <td className="govuk-table__cell--numeric">6.74</td>
            <td className="govuk-table__cell--numeric">6.82</td>
          </tr>
          <tr>
            <td>Unauthorised absence</td>
            <td className="govuk-table__cell--numeric">1.20</td>
            <td className="govuk-table__cell--numeric">1.42</td>
          </tr>
          <tr>
            <th colSpan={3}>Academies</th>
          </tr>
          <tr>
            <td>Number of enrolments</td>
            <td className="govuk-table__cell--numeric">3,017,628</td>
            <td className="govuk-table__cell--numeric">3,024,728</td>
          </tr>
          <tr>
            <td>Overall absence</td>
            <td className="govuk-table__cell--numeric">7.94</td>
            <td className="govuk-table__cell--numeric">8.24</td>
          </tr>
          <tr>
            <td>Authorised absence</td>
            <td className="govuk-table__cell--numeric">6.74</td>
            <td className="govuk-table__cell--numeric">6.82</td>
          </tr>
          <tr>
            <td>Unauthorised absence</td>
            <td className="govuk-table__cell--numeric">1.20</td>
            <td className="govuk-table__cell--numeric">1.42</td>
          </tr>
        </tbody>
      </table>
      <h3 id="section4-3">Data coverage</h3>
      <h4>Coverage for 2006/07 to present</h4>
      <p>
        Schools provide, via the school census, individual level attendance data
        for pupils of compulsory school age (ages 5 to 15 at the start of the
        school year) and, as of September 2012, pupils aged 4 (at the start of
        the school year) who are nonboarders.{' '}
      </p>
      <p>
        Departmental guidance states it is important that schools are able to
        work with parents of four year olds to develop good patterns of school
        attendance before they reach compulsory school age, and avoid it
        becoming a problem later on in their academic career.{' '}
      </p>
      <p>
        Within published absence statistics, schools are categorised into the
        following phases.
      </p>
      <table className="govuk-table">
        <caption className="govuk-table-caption">
          Table 9: School types included in published absence statistics
        </caption>
        <thead>
          <tr>
            <th>Phase</th>
            <th>Types of school</th>
          </tr>
        </thead>
        <tbody>
          <tr>
            <td>State-funded primary</td>
            <td>
              <ul className="govuk-list">
                <li>Local authority maintained schools</li>
                <li>Middle schools as deemed</li>
                <li>Sponsored academies</li>
                <li>Converter academies</li>
                <li>Free schools</li>
              </ul>
            </td>
          </tr>
          <tr>
            <td>State-funded secondary </td>
            <td>
              <ul className="govuk-list">
                <li>Local authority maintained schools</li>
                <li>Middle schools as deemed</li>
                <li>City technology colleges</li>
                <li>Sponsored academies</li>
                <li>Converter academies</li>
                <li>Free schools</li>
                <li>University technical colleges</li>
                <li>Studio schools</li>
              </ul>
            </td>
          </tr>
          <tr>
            <td>Special </td>
            <td>
              <ul className="govuk-list">
                <li>Local authority maintained special schools</li>
                <li>Non-maintained special schools</li>
                <li>Sponsored academies</li>
                <li>Converter academies</li>
                <li>Free schools</li>
              </ul>
            </td>
          </tr>
          <tr>
            <td>Pupil referral units </td>
            <td>
              <ul className="govuk-list">
                <li>Pupil referral units</li>
                <li>Alternative provision sponsored academies</li>
                <li>Alternative provision converter academies</li>
                <li>Alternative provision free schools</li>
              </ul>
            </td>
          </tr>
        </tbody>
      </table>
      <p>
        Prior to the 2016/17 academic year special school information was
        published in full year absence releases only as termly data was not
        collected.
      </p>

      <p>
        In the spring 2014 school census, the Department introduced a new
        “all-through” school phase. Schools with a statutory low age of below
        seven and a statutory high age of above fourteen have been flagged as
        all-through schools in pupil absence National Statistics releases. For
        recent releases an approximation of all-through absence can be estimated
        using the school level underlying data.{' '}
      </p>

      <h4>Coverage for 2005/06</h4>
      <p>
        Absence information for 2005/06 was the first years’ worth of absence
        data collected termly, at enrolment level, via the school census. In
        this first year of collection, information was collected for secondary
        schools only.
      </p>

      <h4>Coverage for 1993/94 to 2004/05</h4>
      <p>
        The absence in Schools Survey collected annual absence data, at school
        level, for compulsory school aged pupils in primary, secondary, special
        and independent schools. Information on academy absence was first
        collected for the 2002/03 academic year.
      </p>
      <h3 id="section4-4">What absence information is collected</h3>
      <p>
        Schools are expected to supply the following via their school census
        returns:
      </p>
      <h4>The number of sessions possible </h4>
      <p>
        Schools must record the number of sessions possible for each enrolment
        for the attendance period (term or half term). There are two sessions
        for each school day (morning and afternoon).{' '}
      </p>
      <p>
        Every pupil aged 4 to 15 (excluding boarders) as at the start of the
        academic year (31 August) who was on the school roll for at least one
        session during the specified attendance period should have an entry for
        the number of possible sessions.
      </p>
      <p>
        Enrolments with zero possible sessions are not included in the
        Department’s absence releases.
      </p>
      <p>Special cases when recording possible sessions:</p>

      <ul className="govuk-list govuk-list--bullet">
        <li>
          <p>Dual registered pupils</p>
          <p>
            To avoid the double counting absence for pupils who are registered
            at more than one school (referred to as ‘dual registered’). Each
            school should:
          </p>
          <ul>
            <li>
              only record the attendance and absence for the sessions the pupil
              is required to attend at their school{' '}
            </li>
            <li>
              use code ‘D’ (dual registered at another educational
              establishment) to record all of the sessions that the pupil is due
              to attend at the other school
            </li>
          </ul>
        </li>
        <li>
          <p>Zero sessions possible</p>
          <p>
            Zero sessions possible can be recorded where a dual registered pupil
            has spent all of the attendance period (term or half term) at their
            other school. For example, if a dually registered pupil spent all of
            the term/ half term at his/her subsidiary registration then the main
            registration would have zero sessions possible recorded for that
            term in the Census.{' '}
          </p>
        </li>
        <li>
          <p>Summer half term </p>
          <p>
            The official school leaving date for a pupil who ceases to be of
            compulsory school age is the last Friday in June of the academic
            year following the pupil’s fifteenth birthday. This means that year
            11 pupils must remain on the school roll until this date and their
            attendance must be recorded.{' '}
          </p>
          <p>
            Schools remain responsible for year 11 pupils up to the leaving
            date, even when they have finished exams. It is up to schools to
            consider how they might seek to widen the range of learning
            opportunities during this time to meet the needs of their pupils.{' '}
          </p>
          <p>
            Note: Absence data for the second half of the summer term is
            collected for such pupils however any possible sessions and/or
            absence information for pupils aged 15 years old in this term will
            not be published in any National Statistics
          </p>
        </li>
        <li>
          <p>Pupils aged four</p>
          <p>
            Pupils aged four are not of compulsory school age, their absence
            information is collected for indicative purposes only. The
            Department collects this data because good patterns of regular and
            punctual attendance can be set from the early years and schools will
            want to be aware of their performance in this respect.
          </p>
          <p>
            For pupils aged four, the number of sessions that they are expected
            to attend will vary from pupil to pupil and from school to school.
            Attendance code ‘X’ (non-compulsory school age absence - not counted
            in possible attendances) should be used for those sessions when a
            four year old is not expected to attend.
          </p>
          <p>
            Within National Statistics, four year old absences are presented
            separately to those for pupils of compulsory school age and only
            overall absence rates are published.
          </p>
        </li>
      </ul>
      <h4>Sessions missed due to authorised absence </h4>
      <p>
        Schools are required to report the number of authorised absence sessions
        accrued by each enrolment. This can either be reported by a reason for
        absence breakdown, or by an aggregated total. See the number of sessions
        missed for each specified reason for absence section for more
        information.
      </p>

      <p>
        Authorised absence is absence which has been authorised by a teacher or
        other authorised representative of the school. See key absence measures
        section for more information.
      </p>

      <h4>Sessions missed due to unauthorised absence</h4>

      <p>
        Schools are required to report the number of unauthorised absence
        sessions accrued by each enrolment. This can either be reported by a
        reason for absence breakdown, or by an aggregated total. See the number
        of sessions missed for each specified reason for absence section for
        more information
      </p>

      <p>
        Unauthorised absence is absence without permission from a teacher or
        other authorised representative of the school. See key absence measures
        section for more information.
      </p>

      <p>
        Unauthorised absence does not apply to pupils of non-compulsory school
        age i.e. those aged four. Any absence for four year olds should be
        recorded as authorised.{' '}
      </p>

      <h4>
        The number of sessions missed for each specified reason for absence
      </h4>

      <p>
        Schools are able to provide their absence data using a reason code
        breakdown or by using total figures for the number of sessions missed
        due to authorised or unauthorised absence.{' '}
      </p>

      <p>
        Some schools do not have the required software to provide absence data
        broken down by reason, and therefore are only able to provide overall
        totals. In instances where no reason breakdown is provided absence is
        categorised under “unclassified”. The majority of schools are able to
        and do provide absence information broken down by reason for absence,
        but an estimated 1 per cent of schools do not.{' '}
      </p>

      <p>
        When deriving absence levels for each enrolment, in the first instance,
        the sum of their absence by reason has been used, if this is missing or
        is less than the total provided, their overall totals have been used.{' '}
      </p>

      <table className="govuk-table">
        <caption className="govuk-table-caption">
          Table 10: In the first instance, absences provided by reason are used
          to create absence totals
        </caption>
        <thead>
          <tr>
            <td />
            <th>Authorised absence total (as provided)</th>
            <th>Unauthorised absence total (as provided)</th>
            <th>Sum of provided reason breakdown (calculated)</th>
            <th>Outcome</th>
          </tr>
        </thead>
        <tbody>
          <tr>
            <td>Pupil A</td>
            <td>15 sessions</td>
            <td>35 sessions</td>
            <td>42 sessions</td>
            <td>
              The sum of enrolment’s reason for absence breakdown is lower than
              the sum of authorised and unauthorised totals. Therefore, the
              reason for absence breakdown is not used.
            </td>
          </tr>
          <tr>
            <td>Pupil B</td>
            <td>30 sessions</td>
            <td>10 sessions</td>
            <td>50 sessions</td>
            <td>
              The sum of enrolment’s reason for absence breakdown is higher than
              provided totals. Therefore, the reason for absence breakdown is
              used.
            </td>
          </tr>
          <tr>
            <td>Pupil C</td>
            <td>20 sessions</td>
            <td>5 sessions</td>
            <td>25 sessions</td>
            <td>
              Reason breakdown total equals the sum of provided authorised and
              unauthorised totals. Reason breakdown figures are used.
            </td>
          </tr>
        </tbody>
      </table>
      <h4>Authorised absence reasons</h4>
      <p>
        Only special circumstances should warrant an authorised leave of
        absence. Schools should consider each application individually and take
        into account the specific circumstances and relevant background context
        behind the request before authorising.{' '}
      </p>
      <p>
        The authorised reasons schools can use to record absences via the school
        census are as follows:
      </p>
      <ul className="govuk-list govuk-list--bullet">
        <li>
          <p>Illness (not medical or dental appointments) </p>
          <p>
            Schools should advise parents to notify them on the first day the
            child is unable to attend due to illness. Schools should authorise
            absences due to illness unless they have genuine cause for concern
            about the veracity of an illness. If the authenticity of illness is
            in doubt, schools can request parents to provide medical evidence to
            support illness. Schools can record the absence as unauthorised if
            not satisfied of the authenticity of the illness but should advise
            parents of their intention. Schools are advised not to request
            medical evidence unnecessarily. Medical evidence can take the form
            of prescriptions, appointment cards, etc. rather than doctors’
            notes.{' '}
          </p>
        </li>
        <li>
          <p>Medical or dental appointments </p>
          <p>
            Missing registration for a medical or dental appointment is counted
            as an authorised absence. Schools should, however, encourage parents
            to make appointments out of school hours. Where this is not
            possible, the pupil should only be out of school for the minimum
            amount of time necessary for the appointment.
          </p>
        </li>
        <li>
          <p>Holiday authorised by the school </p>
          <p>
            Headteachers should not grant leave of absence unless there are
            exceptional circumstances. The application must be made in advance
            and the headteacher must be satisfied that there are exceptional
            circumstances based on the individual facts and circumstances of the
            case which warrant the leave. Where a leave of absence is granted,
            the headteacher will determine the number of days a pupil can be
            away from school. A leave of absence is granted entirely at the
            headteacher’s discretion.
          </p>
        </li>
        <li>
          <p>Religious observance </p>
          <p>
            Schools must treat absence as authorised when it is due to religious
            observance. The day must be exclusively set apart for religious
            observance by the religious body to which the parents belong. Where
            necessary, schools should seek advice from the parents’ religious
            body about whether it has set the day apart for religious
            observance.
          </p>
        </li>
        <li>
          <p>Study leave </p>
          <p>
            Schools must record study leave as authorised absence. Study leave
            should be used sparingly and only granted to year 11 pupils during
            public examinations. Provision should still be made available for
            those pupils who want to continue to come into school to revise.
          </p>
        </li>
        <li>
          <p>Gypsy, Roma and Traveller absence </p>
          <p>
            A number of different groups are covered by the generic term
            Traveller – Roma, English and Welsh Gypsies, Irish and Scottish
            Travellers, Showmen (fairground people) and Circus people, Bargees
            (occupational boat dwellers) and New Travellers.
          </p>
          <p>
            This code should be used when Traveller families are known to be
            travelling for occupational purposes and have agreed this with the
            school but it is not known whether the pupil is attending
            educational provision. It should not be used for any other types of
            absence by these groups.{' '}
          </p>
          <p>
            To help ensure continuity of education for Traveller children it is
            expected that the child should attend school elsewhere when their
            family is travelling and be dual registered at that school and the
            main school. Children from these groups whose families do not travel
            are expected to register at a school and attend as normal. They are
            subject to the same rules as other children in terms of the
            requirement to attend school regularly once registered at a school.{' '}
          </p>
        </li>
        <li>
          <p>Excluded but no alternative provision made</p>
          <p>
            If no alternative provision is made for a pupil to continue their
            education whilst they are excluded but still on the admission
            register, they should be marked absent in the attendance register
            using code “E”. Alternative provision must be arranged for each
            excluded pupil from the sixth consecutive day of any fixed period or
            permanent exclusion. Where alternative provision is made they should
            be marked using the appropriate attendance code.{' '}
          </p>
        </li>
        <li>
          <p>Other authorised absences</p>
          <p>
            Any authorised absences not covered by the groups above, this code
            should only be used in exceptional circumstances.
          </p>
        </li>
      </ul>
      <h4>Unable to attend due to exceptional circumstances </h4>
      <p>
        In 2012, for the 2011/12 academic year, the school census started to
        collect absence information for pupils who are unable to attend school
        due to exceptional circumstances, or attendance code “Y”
      </p>
      <p>
        Absences due to exceptional circumstances do not count as a possible
        session and are not included in National Statistics.
      </p>
      <p>This code can be used where a pupil is unable to attend because: </p>
      <ul className="govuk-list govuk-list--bullet">
        <li>
          The school site, or part of it, is closed due to an unavoidable cause
        </li>
        <li>
          The transport provided by the school or a local authority is not
          available and where the pupil’s home is not within walking distance
        </li>
        <li>
          A local or national emergency has resulted in widespread disruption to
          travel which has prevented the pupil from attending school
        </li>
      </ul>
      <p>
        This code can also be used where a pupil is unable to attend because:{' '}
      </p>
      <ul className="govuk-list govuk-list--bullet">
        <li>
          The pupil is in custody; detained for a period of less than four
          months. If the school has evidence from the place of custody that the
          pupil is attending 31 educational activities then they can record
          those sessions as code “B” (present at approved educational activity).{' '}
        </li>
      </ul>
      <h4>Absence by reason for four year olds</h4>
      <p>
        Schools are not obliged to use individual absence and attendance codes
        for pupils aged four. However they are encouraged to use these codes
        and, if they do so, the appropriate absences will be returned in the
        school census. If schools do not wish to use these codes then the total
        number of absences for the attendance period will be recorded as
        sessions missed due to authorised absence.{' '}
      </p>
      <p>
        Absences recorded for four year olds will not be treated as ‘authorised’
        or ‘unauthorised’ and will instead be reported, and published, as
        overall absence only.{' '}
      </p>

      <h3 id="section4-5"> No longer collected but available historically</h3>
      <p>
        The authorised absence code “extended family holiday” was discontinued
        as of September 2014 and should not be used by schools. As of the Pupil
        absence in schools in England: 2013 to 2014 Statistical First Release
        any extended family holiday absence returned will be combined with
        “authorised family holiday” absence. Separate absence figures for
        “extended family holiday” are published historically.
      </p>
      <p>
        For 2009/10 to 2011/12, absence information for pupils attending pupil
        referral units (PRUs) was collected annually via the PRU Census and
        published as an additional table to the full year absence release in May
        each year. As of January 2014 PRU census information, including absence
        data, is now collected termly via the school census and published as an
        additional table to each termly SFR. For the 2012/13 academic year, due
        to the move from PRU Census to school census, absence data was not
        collected from PRUs.
      </p>

      <h3 id="section4-6"> What absence information is not collected</h3>
      <p>
        The following section outlines information the Department does not
        collect, including areas regularly queried by users.
      </p>
      <h4>Boarding school absence</h4>
      <p>
        Boarding schools without day-pupils are not required to keep an
        attendance register. Schools with a mixture of day-pupils and boarders
        must keep an attendance register for the day-pupils but absence
        information will not be collected for boarders.{' '}
      </p>

      <h4>Closed school absence</h4>
      <p>
        Data are collected a term in arrears, meaning that where a school
        closes, data are not collected for the last term the school was open.
        For schools which close at the end of a term, data for that term will
        not be collected.
      </p>
      <h4> Internal absence</h4>
      <p>
        The Department is unable to identify “internal absence”. This is defined
        as any absence by pupils between the school’s twice-daily registrations,
        i.e. a pupil is recorded as attending during morning or afternoon
        registration but is physically not present at another part of the
        relevant session. The Department’s current data systems are limited to
        only record and measure the registrations required by law, not any
        subsequent absence.
      </p>
      <h4>Those not registered at a school</h4>
      <p>
        The Department only collects absence data for pupils on roll of a
        state-funded primary, state-funded secondary, special schools or pupil
        referral units (including alternative provision academies) during the
        absence period. It does not include those children who are not
        registered at a school.{' '}
      </p>
      <h4>Daily absence and periods of absence</h4>
      <p>
        Absence information is collected termly for primary and secondary
        schools and pupil referral units and is collected annually for special
        schools. The Department does not collect dates of absence and is
        therefore unable to provide absence figures for specific days, weeks or
        months.
      </p>
      <p>
        In addition, the Department is also unable to identify the lengths of
        individual absences as only aggregated absence totals, either overall or
        broken down by reason, are collected for each enrolment.
      </p>
      <h4>
        {' '}
        Individual reason absence, for example chronic illness or snow days
      </h4>
      <p>
        The Department collects pupil absence information broken down by reason,
        however these groupings are broad and often cover a range of potential
        reasons grouped under one relevant category.
      </p>
      <p>
        Breakdowns often requested by users are absences due to specific types
        of illness or days lost due to snow or flooding, both of which we are
        unable to provide individually. Specific illnesses would be covered
        under the “illness” reason for 33 absence and sessions missed due to
        snow or flooding would be covered under the “exceptional circumstances”
        reason for absence, or attendance code “Y”.{' '}
      </p>
      <h4>Post 16 and nursery pupil’s absence</h4>
      <p>
        Absence information is collected and published for pupils of compulsory
        school age, aged between five and fifteen as of the start of the
        academic year (31st August). Absence information for four year olds is
        also collected for indicative purposes only. The Department does not
        report on absence for pupils aged three and below or aged sixteen and
        above and schools should not provide this data, therefore such
        breakdowns are unavailable.
      </p>
      <h4>Pupil attendance</h4>
      <p>
        The Department collects pupil absence information only - attendance
        codes are not collected.{' '}
      </p>
      <p>Attendance codes used by schools are as follows:</p>
      <ul className="govuk-list govuk-list--bullet">
        <li>
          <p>Present at School </p>
          <p>
            Pupils must not be marked present if they were not in school during
            registration. If a pupil were to leave the school premises after
            registration they would still be counted as present for statistical
            purposes.{' '}
          </p>
        </li>
        <li>
          <p>Late arrival before the register has closed </p>
          <p>
            Schools should have a policy on how long registers should be kept
            open; this should be for a reasonable length of time but not that
            registers are to be kept open for the whole session. A pupil
            arriving after the register has closed should be marked absent with
            code “U”, or with another absence code if that is more appropriate.{' '}
          </p>
        </li>
      </ul>
      <p>
        Attendance codes for when pupils are present at an approved off-site
        educational activity are as follows:
      </p>
      <ul className="govuk-list govuk-list--bullet">
        <li>
          <p>Off-site educational activity </p>
          <p>
            This code should be used when pupils are present at an off-site
            educational activity that has been approved by the school.
            Ultimately schools are responsible for the safeguarding and welfare
            of pupils educated off-site. Therefore by using code B, schools are
            certifying that the education is supervised and measures have been
            taken to safeguard pupils. This code should not be used for any
            unsupervised educational activity or where a pupil 34 is at home
            doing school work. Schools should ensure that they have in place
            arrangements whereby the provider of the alternative activity
            notifies the school of any absences by individual pupils. The school
            should record the pupil’s absence using the relevant absence code.
          </p>
        </li>
        <li>
          <p>Consortia Schools </p>
          <p>
            Pupils attending consortia schools as part of their course only need
            to be placed on the registers of their ‘main’ school rather than on
            all of the schools they attend. They should be treated as guest
            pupils at the other consortia schools. The consortia schools
            however, must ensure they have suitable systems in place for
            monitoring and reporting the attendance and absence of the pupils
            involved, which must be shared with the ‘main’ school.{' '}
          </p>
        </li>
        <li>
          <p>Dual Registered - at another educational establishment</p>
          <p>
            This code is not counted as a possible attendance in the school
            census. The law allows for dual registration of pupils at more than
            one school. This code is used to indicate that the pupil was not
            expected to attend the session in question because they were
            scheduled to attend the other school at which they are registered.{' '}
          </p>
          <p>
            The main examples of dual registration are pupils who are attending
            a pupil referral unit, a hospital school or a special school on a
            temporary basis. It can also be used when the pupil is known to be
            registered at another school during the session in question.{' '}
          </p>
          <p>
            Each school should only record the pupil’s attendance and absence
            for those sessions that the pupil is scheduled to attend their
            school. Schools should ensure that they have in place arrangements
            whereby all unexplained and unexpected absence is followed up in a
            timely manner.{' '}
          </p>
        </li>
        <li>
          <p>
            At an interview with prospective employers, or another educational
            establishment
          </p>
          <p>
            This code should be used to record time spent in interviews with
            prospective employers or another educational establishment. Schools
            should be satisfied that the interview is linked to employment
            prospects, further education or transfer to another educational
            establishment.{' '}
          </p>
        </li>
        <li>
          <p>Participating in a supervised sporting activity </p>
          <p>
            This code should be used to record the sessions when a pupil is
            taking part in a sporting activity that has been approved by the
            school and supervised by someone authorised by the school.{' '}
          </p>
        </li>
        <li>
          <p>Educational visit or trip </p>
          <p>
            This code should be used for attendance at an organised trip or
            visit, including residential trips organised by the school, or
            attendance at a supervised trip of a strictly educational nature
            arranged by an organisation approved by the school.{' '}
          </p>
        </li>
        <li>
          <p>Work experience </p>
          <p>
            Work experience is for pupils in the final two years of compulsory
            education. Schools should ensure that they have in place
            arrangements whereby the work experience placement provider notifies
            the school of any absences by individual pupils. Any absence should
            be recorded using the relevant code.{' '}
          </p>
        </li>
      </ul>
      <p>
        The following codes are used as administrative codes and are not counted
        as a possible attendance in the school census:{' '}
      </p>
      <ul className="govuk-list govuk-list--bullet">
        <li>
          <p>Not required to be in school </p>
          <p>
            This code is used to record sessions that non-compulsory school age
            children are not expected to attend.{' '}
          </p>
        </li>
        <li>
          <p>Pupil not on admission register</p>
          <p>
            This code is available to enable schools to set up registers in
            advance of pupils joining the school to ease administration burdens.
            Schools must put pupils on the admission register from the first day
            that the school has agreed, or been notified, that the pupil will
            attend the school.
          </p>
        </li>
        <li>
          <p>Planned whole or partial school closure </p>
          <p>
            This code should be used for whole or partial school closures that
            are known or planned in advance such as: between terms; half terms;
            occasional days (for example, bank holidays); weekends (where it is
            required by the management information system); up to five
            non-educational days to be used for curriculum planning/training;
            and use of schools as polling stations.
          </p>
        </li>
        <li>
          <p>Different term dates for different pupils:</p>
          <p>
            Schools and local authorities can agree to set different term dates
            for different year groups – e.g. for ‘staggered starts’ or
            ‘induction days’. A code “#” can be 36 used to record the year
            group(s) that is not due to attend. This is only acceptable where
            the school ensures that those pupils not attending on that day are
            still offered a full education over the school year.
          </p>
        </li>
      </ul>
    </>
  );
};

export default PrototypeMethodologySection;
