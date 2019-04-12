import classNames from 'classnames';
import React from 'react';
import Link from '../../../../components/Link';

const PrototypeMethodologySection = () => {
  return (
    <>
      <p>
        The sections below outline how pupil absence data goes from raw school
        census returns to a finalised dataset used to prepare National
        Statistics releases.
      </p>
      <h3 id="section5-1">5.1 Data linking</h3>
      <p>
        Annual and combined termly absence data are derived by linking pupil
        records collected in each relevant school census to calculate overall
        totals for the number of possible sessions; the number of sessions
        missed by reason for absence and the number of sessions missed due to
        authorised and unauthorised absence for each enrolment. Derived absence
        totals are then linked to a pupil’s characteristics data, also collected
        via the school census, at the time of the absence. Where characteristics
        data are missing, information collected in the following census are
        used, except for ethnicity as this information is only collected in the
        spring school census, see table 11.
      </p>
      <p>
        For pupil enrolments joining mid academic year, characteristic
        information is taken from the census relating to the term of absence, if
        no data is available the following census is used.
      </p>
      <p>
        Characteristics data is only linked once, on the earliest term of data,
        so ‘in-year’ changes in characteristic information will not be picked up
        until the following academic year
      </p>
      <table className="govuk-table">
        <caption className="govuk-table-caption">
          Table 11: Characteristics linking for absence data
        </caption>
        <thead>
          <tr>
            <th scope="col">Absence period</th>
            <th scope="col">Source of absence data</th>
            <th scope="col">
              First choice for characteristic (same as absence period)
            </th>
            <th scope="col">
              Second choice for characteristic (following Census)
            </th>
          </tr>
        </thead>
        <tbody>
          <tr>
            <td>Autumn term</td>
            <td>Spring school census</td>
            <td>Autumn school census</td>
            <td>Spring school census</td>
          </tr>
          <tr>
            <td>Spring term</td>
            <td>Summer school census</td>
            <td>Spring school census</td>
            <td>Summer school census</td>
          </tr>
          <tr>
            <td>Summer term</td>
            <td>Autumn school census</td>
            <td>Summer school census</td>
            <td>Autumn school census</td>
          </tr>
        </tbody>
      </table>
      <h3 id="section5-2">5.2 Data removed</h3>
      <p>
        To prepare the absence data for publication routine checks are applied
        to the linked data and where necessary data is removed as follows.
      </p>
      <p>
        Schools with fewer than 6 pupil enrolments aged between five and fifteen
        are removed from the analysis. This usually amounts to a negligible
        number of schools; most commonly post 16 establishments with a small
        number of 15 year old pupils.
      </p>
      <p>
        For all pupil enrolments, if the number of possible sessions in a term
        is zero/missing or is less than the total number of sessions missed due
        to overall absence, then the enrolment is removed from the absence data
        and is not included in National Statistics.
      </p>
      <p>
        Any sponsored academies which opened part way through a term, and
        provided predecessor school absence data via the school census will have
        the term of data which includes predecessor information removed before
        any analysis. This absence data is removed because when a sponsored
        academy opens it is a new school, and therefore should not be held
        accountable for absence levels accrued within the predecessor school.
      </p>
      <p>
        To identify these academies we look at their average number of possible
        sessions (mean and mode) supplied via the school census. If a sponsored
        academy has a higher average possible sessions than we would expect,
        estimated using their open date, we assume they have supplied
        information for their predecessor school.{' '}
      </p>
      <p>
        Table 12 provides examples of sponsored academies which would be
        reviewed when compiling autumn and spring terms absence data. We would
        expect schools to provide around 250 possible sessions across the autumn
        and spring term.
      </p>
      <table className="govuk-table">
        <caption className="govuk-table-caption">
          Table 12: Removing predecessor school absence from sponsored academies
        </caption>
        <thead>
          <tr>
            <th scope="col">Open date</th>
            <th scope="col">Average possible sessions</th>
            <th scope="col">Outcome</th>
          </tr>
        </thead>
        <tbody>
          <tr>
            <td>January</td>
            <td>125 sessions (as expected)</td>
            <td>No data removed</td>
          </tr>
          <tr>
            <td>January</td>
            <td>200 sessions (too high, includes predecessor information)</td>
            <td>Autumn term adsence removed</td>
          </tr>
          <tr>
            <td>March</td>
            <td>30 sessions (as expected)</td>
            <td>No data removed</td>
          </tr>
          <tr>
            <td>March</td>
            <td>130 sessions (too high, includes predecessor information)</td>
            <td>Academy removed completely</td>
          </tr>
        </tbody>
      </table>
      <p>
        Year on year comparisons of local authority data may be affected by
        schools converting to academies.
      </p>{' '}
      <h3 id="section5-3">5.3 Variables added</h3>
      <p>
        Most of the variables needed to produce national level absence
        statistics can be calculated directly from the fields collected via the
        school census. However, to prepare the absence data for publication the
        following variables are also added to the dataset.
      </p>
      <h4>Authorised, unauthorised and overall absence totals</h4>
      <p>
        Termly authorised and unauthorised absence totals are derived by taking
        whichever is highest – either the total authorised or unauthorised
        absence sessions provided by the school or the sum of the authorised or
        unauthorised absence reasons provided by the school. These fields are
        set to zero if the absence totals greater than the number of sessions
        possible or if the number of sessions possible is equal to zero.
      </p>
      <p>
        The termly overall absence total is calculated by taking the sum of the
        authorised and unauthorised absence totals. Again this is set to zero if
        the absence total is greater than the number of sessions possible or if
        the number of sessions possible is equal to zero.
      </p>
      <p>
        Full year totals are derived by summing the termly totals together.{' '}
      </p>
      <h4> Persistent absentee indicators</h4>
      <p>
        An indicator variable for the published persistent absence measure is
        added to each enrolment before producing any analysis. This is derived
        by comparing each enrolment’s overall absence total to their own
        possible sessions to establish if they have missed 10 per cent or more
        of the sessions available to them (see persistent absence methodology).
      </p>
      <h4>School type, academy type</h4>
      <p>
        School type information, including school type, academy type and academy
        open date, are added to our underlying data prior to producing any
        analysis to allow us to produce the school/academy type breakdowns in
        our statistical releases.
      </p>
      <p>
        These variables are derived using a combination of Edubase and the Open
        academies and academy projects in development data. Within absence
        National Statistics, academies are only indicated as academies if they
        were open as of the 12th September.
      </p>
      <h3 id="section5-4">5.4 Consistency checks</h3>
      <p>
        After the data is processed as set out above, consistency checks are
        performed against the Schools, pupils and their characteristics National
        Statistics release to check that the numbers of schools and enrolments
        are as expected.
      </p>
      <p>
        Further checks are carried out on the consistency of figures compared
        with previous years, both nationally and at local authority level.
      </p>
      <h3 id="section5-5">5.5 Data quality</h3>
      <p>
        The following should be taken into account when reviewing published
        pupil absence statistics.
      </p>
      <p>
        The absence information reported in published releases is based on data
        returned by schools as part of the school census. This might include
        duplicates if schools have recorded duplicated pupils.
      </p>
      <p>
        It does not include data which has been submitted by local authorities
        or schools outside of the school census.{' '}
      </p>
      <p>
        It is a school’s responsibility to record absence data correctly in
        their school census return and the parent’s responsibility to truthfully
        report the reason for a child’s absence from school.
      </p>
      <p>
        Only full year absence statistics give a definitive view of pupil
        absence, so figures presented in the interim termly publications should
        be treated with caution.
      </p>
      <p>
        Caution is recommended when interpreting the data for Traveller of Irish
        Heritage and Gypsy/Roma children due to potential under-reporting for
        these ethnic classifications
      </p>
    </>
  );
};

export default PrototypeMethodologySection;
