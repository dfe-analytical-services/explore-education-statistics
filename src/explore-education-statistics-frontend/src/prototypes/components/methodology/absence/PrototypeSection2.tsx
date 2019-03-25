import classNames from 'classnames';
import React from 'react';
import Link from '../../../../components/Link';

const PrototypeMethodologySection = () => {
  return (
    <>
      <h3 id="section2-1">Current termly publications</h3>
      <p>
        DfE publishes termly pupil absence data and statistics via the following
        3 National Statistics releases each year:
      </p>
      <ul className="govuk-list govuk-list--bullet">
        <li>autumn term data and statistics - published in May</li>
        <li>
          autumn and spring terms data and statistics - published in October
        </li>
        <li>full year data and statistics - published in March</li>
      </ul>
      <p>
        Only the full year absence release gives a definitive view of pupil
        absence.
      </p>
      <p>
        Termly publications can be affected significantly by term length with
        findings from these releases for indicative purposes only and the
        results should be treated with caution.
      </p>
      <p>
        DfE’s annual absence releases have been badged as National Statistics
        since the 1999/00 academic year publication.
      </p>
      <p>
        The termly and 2 term combined releases were badged as National
        Statistics slightly later.
      </p>
      <p>
        The combined autumn and spring term release was badged as national
        statistics from the autumn 2006 and spring 2007 publication and the
        single term releases were badged as National Statistics as of the autumn
        term 2009 publication.
      </p>
      <h4>Historical publications</h4>
      <p>
        Prior to the 2012/13 academic year DfE also published spring term only
        absence data. However, this was discontinued as it was deemed no longer
        necessary and of the least importance to users.
      </p>
      <p>The last spring term release wass published on 30 August 2012: </p>
      <ul className="govuk-list govuk-list--bullet">
        <li>
          <a href="https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-spring-term-2012">
            Pupil absence in schools in England: spring term 2012
          </a>{' '}
        </li>
      </ul>
      <p>
        For the 2005/06 academic year, due to the transition of absence
        collection between the Absence in Schools Survey and the school census,
        absence information was published for secondary schools only as a
        National Statistics release.
      </p>
      <p>
        Absence data for 2005/06 were not released on a termly basis as this was
        the first year of collection.
      </p>
      <p>
        For academic years 1999/00 to 2004/05, annual pupil absence information
        was collected via the Absence in Schools Survey and published as an
        annual National Statistics release.
      </p>
      <p>
        For academic years 1993/94 to 1998/99, annual pupil absence information
        was collected via the Absence in Schools Survey and published via
        Statistical bulletins. Links to all absence publications can be found in{' '}
        <a href="#">Annex D</a>.
      </p>
      <h3 id="section2-2">Key absence measures</h3>
      <p>
        DfE monitors pupil absence levels using 2 key measures - overall absence
        rate and persistent absence (PA) rate.
      </p>
      <p>
        These key measures are calculated for pupils who are of compulsory s
        chool age - ie aged between 5 and 15 as at the start of the academic
        year (31 August).
      </p>
      <p>
        Absence information is reported as totals or rates across a period,
        usually by school term or academic year.
      </p>
      <p>
        Overall absence is the aggregated total of all authorised and
        unauthorised absences.
      </p>
      <p>
        Authorised absence is absence with permission from a teacher or other
        authorised school representative - including absences where a
        satisfactory explanation has been provided. For example, through
        illness.
      </p>
      <p>
        Unauthorised absence is absence without permission from the school. This
        includes all unexplained or unjustified absences and arrivals after
        registration has closed. See overall absence methodology section for
        further information.
      </p>
      <p>
        Persistent absence is when a pupil enrolment’s overall absence equates
        to 10% or more of their possible sessions.
      </p>
      <p>See persistent absence methodology section for further information.</p>
      <h3 id="section2-3">Cohort used in absence measures</h3>
      <p>
        Absence information is collected and disseminated at enrolment level
        rather than pupil level.
      </p>
      <p>
        This means where a pupil has moved school throughout the year, theyll be
        counted more than once as they have recorded attendance and absence at
        more than 1 school.
      </p>
      <p>
        This allows for schools to be held accountable for pupil absences, as
        the absence is attached to enrolments at a particular school, not the
        individual pupil.
      </p>
      <p>
        All the enrolments at a school over the period in question are included
        in the absence measures, not just the pupils on roll at a particular
        date.
      </p>
      <p>
        Schools only record absence for the period a pupil is on roll at their
        school.
      </p>
      <p>
        The number of pupil enrolments is approximately 4% higher than the
        number of pupils.
      </p>
      <h3 id="section2-4">The school year (5 half terms vs 6 half terms)</h3>
      <p>
        Generally, the academic year is made up of 3 terms - autumn, spring and
        summer.
      </p>
      <p>
        Each term has 2 parts (half terms) which are usually separated by a half
        term break.
      </p>
      <p>
        Since the 2012/13 academic year, pupil absence information has been
        collected for the full academic year (ie all 6 half terms).
      </p>
      <p>
        However, prior to this absence information was collected for the first 5
        half terms only, meaning absences in the second half of the summer term
        were not collected.
      </p>
      <p>
        Since the 2012/13 academic year, DfE's key absence indicators have been
        based on the full academic year’s (ie 6 half terms) data.
      </p>
      <p>
        However, as we're unable to rework time series tables or provide any
        historical 6 half term absence levels DfE continued to publish a full
        set of absence information for the first 5 half terms up to and
        including the 2013/14 academic year.
      </p>
      <p>
        Following this, a single csv file based on data for 5 half terms has
        been published alongside the annual absence publications so longer
        term-time comparisons can still be made.
      </p>
      <p>
        To account for high levels of study leave and other authorised absences
        for pupils aged 15 in the second half of the summer term, all possible
        sessions and absences relating to this period for 15 year olds (as at
        the start of the academic year) are removed prior to any analysis being
        undertaken and are not included in any published statistics.
      </p>
      <table className="govuk-table">
        <caption className="govuk-table__caption">
          Table 1: State-funded primary, secondary and special schools - pupils
          of compulsory school age pupil and enrolment numbers comparison
        </caption>
        <thead>
          <tr>
            <th
              scope="col"
              className="govuk-table__header govuk-table__header--numeric"
            >
              Academic year
            </th>
            <th
              scope="col"
              className="govuk-table__header govuk-table__header--numeric"
            >
              Pupil numbers as at January each year<sup>1</sup>
            </th>
            <th
              scope="col"
              className="govuk-table__header govuk-table__header--numeric"
            >
              Enrolment numbers across full academic year
            </th>
            <th
              scope="col"
              className="govuk-table__header govuk-table__header--numeric"
            >
              Percentage difference
            </th>
          </tr>
        </thead>
        <tbody>
          <tr>
            <td className="govuk-table__cell govuk-table__cell--numeric">
              2012/13
            </td>
            <td className="govuk-table__cell govuk-table__cell--numeric">
              6,230,420
            </td>
            <td className="govuk-table__cell govuk-table__cell--numeric">
              6,477,725
            </td>
            <td className="govuk-table__cell govuk-table__cell--numeric">
              4.0
            </td>
          </tr>
          <tr>
            <td className="govuk-table__cell govuk-table__cell--numeric">
              2013/14
            </td>
            <td className="govuk-table__cell govuk-table__cell--numeric">
              6,300,105
            </td>
            <td className="govuk-table__cell govuk-table__cell--numeric">
              6,554,005
            </td>
            <td className="govuk-table__cell govuk-table__cell--numeric">
              4.0
            </td>
          </tr>
          <tr>
            <td className="govuk-table__cell govuk-table__cell--numeric">
              2014/15
            </td>
            <td className="govuk-table__cell govuk-table__cell--numeric">
              6,381,940
            </td>
            <td className="govuk-table__cell govuk-table__cell--numeric">
              6,642,755
            </td>
            <td className="govuk-table__cell govuk-table__cell--numeric">
              4.0
            </td>
          </tr>
          <tr>
            <td className="govuk-table__cell govuk-table__cell--numeric">
              2015/16
            </td>
            <td className="govuk-table__cell govuk-table__cell--numeric">
              6,484,725
            </td>
            <td className="govuk-table__cell govuk-table__cell--numeric">
              6,737,190
            </td>
            <td className="govuk-table__cell govuk-table__cell--numeric">
              3.9
            </td>
          </tr>
        </tbody>
        <tfoot>
          <tr>
            <td colSpan={4}>
              <sup>1</sup> Pupils with a sole or dual main registration, aged
              between 5 and 15 who are not boarders as of the January school
              census each year.
            </td>
          </tr>
        </tfoot>
      </table>
      <p>
        In published absence statistics, pupil enrolments who first enrolled at
        a school within the second half of the summer term are not included.
      </p>
      <p>
        This is to ensure the same cohorts of enrolments are included in both
        the 5 and 6 half term absence measures.
      </p>
      <h3 id="section2-5">
        Published geographical and characteristics breakdowns
      </h3>
      <p>
        DfE routinely publishes pupil absence information at national, local
        authority and school level - including breakdowns by pupil
        characteristics.
      </p>
      <p>
        The autumn term absence publication provides high level information
        designed to give an early indication on absence levels and the effect of
        winter illness. This includes:
      </p>
      <ul className="govuk-list govuk-list--bullet">
        <li>authorised, unauthorised and overall absence rates</li>
        <li>absence broken down by reason</li>
        <li>
          the number of pupils with 1 or more sessions of absence for different
          reasons
        </li>
        <li>information on persistent absence</li>
      </ul>
      <p>
        The combined autumn and spring term publication includes similar
        information to that of the autumn term. However, it also includes
        absence levels broken down by pupil:
      </p>
      <ul className="govuk-list govuk-list--bullet">
        <li>ethnic group</li>
        <li>first language</li>
        <li>free school meal eligibility</li>
        <li>gender</li>
        <li>national curriculum year group</li>
        <li>special educational need</li>
      </ul>
      <p>
        The full academic year's absence publication includes combined absence #
        information for the autumn, spring and summer terms.
      </p>
      <p>
        It’s the largest publication and includes similar breakdowns to that of
        the combined autumn and spring term publication (as outlined above) as
        well as persistent absence broken down by reason for absence and pupil
        characteristic.
      </p>
      <p>
        Additional breakdowns included in this full year release relate to the:
      </p>
      <ul className="govuk-list govuk-list--bullet">
        <li>distribution of enrolments by length of overall absence</li>
        <li>percentage of enrolments by their overall absence </li>
        <li>number of schools by the percentage of persistent absentees</li>
      </ul>
      <p>
        In this publication, information is also provided at district level,
        based on Income Deprivation Affecting Children Index (IDACI) and by
        degree of rurality.
      </p>
      <p>
        In addition, from 2015/16 onwards, characteristics include free school
        meal eligibility in the last 6 years.
      </p>
      <p>
        The Income Deprivation Affecting Children Index (IDACI) is provided by
        the Department for Communities and Local Government (CLG).
      </p>
      <p>
        The index measures the proportion of all children aged 0 to 15 living in
        income deprived families and is based on Lower-layer Super Output Areas
        (LSOAs) in England.
      </p>
      <p>
        Each LSOA is given a rank between 1 and 32,844 where the LSOA with the
        rank of 1 is the most deprived LSOA and the LSOA with the rank of 32,844
        is the most deprived.
      </p>
      <p>
        IDACI is a subset of the Income Deprivation Domain of the Index of
        Multiple Deprivation 2015 which measures the proportion of the
        population in an area experiencing deprivation relating to low income.
      </p>
      <p>
        The definition of low income used includes both those people that are
        out-of-work and those that are in work but who have low earnings (and
        who satisfy the respective means tests).
      </p>
      <p>Further information about IDACI can be found on the CLG site.</p>
      <p>IDACI bands from 2014/15 are based on 2015 IDACI scores.</p>
      <p>
        IDACI bands for 2010/11 to 2013/14 are based on 2010 IDACI scores and
        those for 2007/08 to 2009/10 are based on 2007 IDACI scores.
      </p>
      <p>
        Care should be taken when comparing IDACI tables based on different
        IDACI scores.
      </p>
      <p>
        The Rural and Urban Area Classification is a product of a joint project
        to produce a single and consistent classification of urban and rural
        areas. The project was sponsored by a number of government departments.
      </p>
      <p>
        The rural and urban definitions classify output areas, wards and super
        output areas by aggregating the underlying hectare grid squares
        classifications for the measures of settlement size and sparsity.
      </p>
      <p>
        Up to 8 classes of output areas could be distinguished - 4 settlement
        types (urban, town and fringe, village, hamlet and isolated dwelling) in
        either a sparse or less sparse regional setting.
      </p>
      <p>
        Absence data by degree of rurality from 2014/15 has been analysed based
        on the 2011 Rural and Urban Area Classification, whereas equivalent data
        for previous years was analysed based on the 2004 Rural and Urban Area
        Classification.
      </p>
      <p>
        Further information about the Rural and Urban Area Classification 2011
        can be found on the Office for National Statistics website.
      </p>
      <p>
        A full list of published absence breakdowns (as of the latest academic
        year’s releases) is available in Annex E.
      </p>
      <p>
        From 2015/16 onwards, published tables on characteristics breakdowns
        include figures for pupils with unclassified or missing characteristics
        information.
      </p>
      <p>
        This represents a small proportion of all pupils and the figures should
        be interpreted with caution.
      </p>
      <p>
        For some characteristics, like free school meals eligibility, pupils
        with unclassified or missing characteristics information have been found
        to have a low average number of sessions possible, which might explain
        more variability in absence rates which use the number of possible
        sessions as a denominator.
      </p>
      <h3 id="section2-6">Underlying data provided alongside publications</h3>
      <p>
        From the 2009/10 academic year, each National Statistics release has
        been accompanied by underlying data, including national, local authority
        and school level information.
      </p>
      <p>
        Alongside the underlying data there is an accompanying document
        (metadata) which provides further information on the contents of these
        files.
      </p>
      <p>
        This data is released under the terms of the Open Government License and
        is intended to meet at least 3 stars for Open Data.
      </p>

      <p>
        Following the ‘
        <a href="https://assets.publishing.service.gov.uk/government/uploads/system/uploads/attachment_data/file/467936/Absence_statistics_changes_-_consultation_response.pdf">
          Consultation on improvements to pupil absence statistics
        </a>
        ’, results published in October 2015, releases are now accompanied by
        time series underlying data, containing additional breakdowns and data
        from 2006/07 to the latest year.
      </p>
      <p>
        This additional data is intended to provide users with all information
        in one place and give them the option of producing their own analysis.
      </p>
      <h3 id="section2-7">Suppression of absence data</h3>
      <p>
        The Code of Practice for Official Statistics requires reasonable steps
        are taken to ensure all published or disseminated statistics produced by
        DfE protects confidentiality.{' '}
      </p>
      <p>
        To do this totals are rounded and small numbers are suppressed according
        to the following rules:{' '}
      </p>
      <ul className="govuk-list govuk-list--bullet">
        <li>
          enrolment numbers at national and regional levels are rounded to the
          nearest 5. Local authority totals across school types are also rounded
          to the nearest 5 to prevent disclosure of any supressed values
        </li>
        <li>
          enrolment numbers of 1 or 2 are suppressed to protect pupil
          confidentiality
        </li>
        <li>
          where the numerator or denominator of any percentage calculated on
          enrolment numbers of 1 or 2, the percentage is suppressed. This
          suppression is consistent with{' '}
          <a href="http://media.education.gov.uk/assets/files/policy%20statement%20on%20confidentiality.pdf">
            DfE's statistical policy
          </a>
        </li>
        <li>where any number is shown as 0 - the original figure was also 0</li>
      </ul>

      <table className="govuk-table">
        <thead>
          <tr>
            <th colSpan={2}>
              Symbols used to identify this in published tables are as follows:
            </th>
          </tr>
        </thead>
        <tbody>
          <tr>
            <td>0</td>
            <td>Zero</td>
          </tr>
          <tr>
            <td>x</td>
            <td>Small number suppressed to preserve confidentiality </td>
          </tr>
          <tr>
            <td>.</td>
            <td>Not applicable </td>
          </tr>
          <tr>
            <td>..</td>
            <td>Not available</td>
          </tr>
        </tbody>
      </table>
      <h3 id="section2-8">Other related publications</h3>
      <p>
        Pupil absence information is also available in the following
        publications:
      </p>
      <ul className="govuk-list govuk-list--bullet">
        <li>
          other National Statistics releases published by the Department
          <ul className="govuk-list govuk-list--bullet">
            <li>children in Need</li>
            <li>children looked after</li>
            <li>school and college performance tables</li>
          </ul>
        </li>
        <li>
          other reports published by the Department
          <ul className="govuk-list govuk-list--bullet">
            <li>the link between absence and attainment at KS2 and KS4</li>
          </ul>
        </li>
      </ul>
      <p>
        In addition historical pupil absence data is available in the following
        publications which have been discontinued:
      </p>
      <ul className="govuk-list govuk-list--bullet">
        <li>
          special Educational Needs: an analysis (Department for Education) -
          data up to and including 2013/14
        </li>
        <li>
          neighbourhood statistics (ONS small area tables) - 2006/07 to 2012/13
          inclusive
        </li>
      </ul>
      <h3 id="section2-9">Devolved administration statistics on absence</h3>
      <p>
        The Department collects and reports on absence information from schools
        in England. For information for Wales, Scotland and Northern Ireland,
        contact the departments below or access their statistics at the
        following links:
      </p>
      <p>Wales: school.stats@wales.gsi.gov.uk or</p>
      <a href="https://gov.wales/statistics-and-research">
        Welsh Government – Statistics and Research
      </a>
      <p>Scotland: school.stats@wales.gsi.gov.uk or</p>
      <a href="https://www2.gov.scot/Topics/Statistics/Browse/School-Education">
        Scottish Government – School Education Statistics
      </a>
      <p>Northern Ireland: statistics@deni.gov.uk or</p>
      <a href="https://www.education-ni.gov.uk/topics/statistics-and-research/statistics">
        Department of Education – Education Statistics
      </a>
    </>
  );
};

export default PrototypeMethodologySection;
