import React from 'react';
import Accordion from '../components/Accordion';
import AccordionSection from '../components/AccordionSection';
import Details from '../components/Details';
import GoToTopLink from '../components/GoToTopLink';
import Link from '../components/Link';
import PrototypeAbsenceData from './components/PrototypeAbsenceData';
import PrototypeDataSample from './components/PrototypeDataSample';
import PrototypeMap from './components/PrototypeMap';
import PrototypePage from './components/PrototypePage';

const PublicationPage = () => {
  return (
    <PrototypePage
      breadcrumbs={[
        {
          link: '/prototypes/browse-releases-find',
          text: 'Methodology',
        },
        { text: 'Specific methodology', link: '#' },
      ]}
    >
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-two-thirds">
          <h1 className="govuk-heading-xl">A guide to absence statistics</h1>
          <h2 className="govuk-heading-s">
            <span className="govuk-caption-m">Published: </span>March 2018
          </h2>
          <p className="govuk-body-l">
            This document provides a comprehensive guide to the pupil absence in
            schools in England statistics published by the Department for
            Education.
          </p>
          <p>The key areas covered in this guide are:</p>
          <ul className="govuk-list govuk-list--bullet">
            <li>background to published statistics and methodology</li>
            <li>data collection and coverage</li>
            <li>data processing</li>
          </ul>
        </div>

        <div className="govuk-grid-column-one-third">
          <aside className="app-related-items">
            <h2 className="govuk-heading-m" id="subsection-title">
              Related content
            </h2>
            <ul className="govuk-list">
              <li>
                <a href="/prototypes/publication">
                  Pupil absence statistics and data for schools in England
                </a>
              </li>
            </ul>
          </aside>
        </div>
      </div>
      <h2 className="govuk-heading-l govuk-!-margin-top-6">Contents</h2>
      <Accordion id="contents-sections">
        <AccordionSection heading="Introduction">
          <div className="govuk-grid-row">
            <div className="govuk-grid-column-one-quarter">
              <h3 className="govuk-heading-s">In this section</h3>
              <ul className="govuk-body-s">
                <li>
                  <a href="#1.1">
                    Requirements of schools in ensuring pupil attendance
                  </a>
                </li>
                <li>
                  <a href="#1.2">Uses and Users</a>
                </li>
              </ul>
            </div>
            <div className="govuk-grid-column-three-quarters">
              <h3 id="1.1">
                Requirements of schools in ensuring pupil attendance
              </h3>
              <p>
                All maintained schools are required to provide two possible
                sessions per day, morning and afternoon, to all pupils. The
                length of each session, break and the school day is determined
                by the school’s governing body. Schools must meet for at least
                380 sessions or 190 days during any school year to educate their
                pupils. If a school is prevented from meeting for one or more
                sessions because of an unavoidable event, it should find a
                practical way of holding extra sessions. However, if it cannot
                find a practical way of doing this then it is not required to
                make up the lost sessions. Academy and free school funding
                agreements state that the duration of the school day/sessions is
                the responsibility of the academy trust. Schools are required to
                take attendance registers twice a day: once at the start of the
                first/morning session of each school day and once during the
                second/afternoon session. In their register, schools are
                required to record whether pupils are:{' '}
              </p>
              <ul className="govuk-list govuk-list--bullet">
                <li>present</li>
                <li>attending an approved educational activity</li>
                <li>absent</li>
                <li>unable to attend due to exceptional circumstances</li>
              </ul>
              <p>
                Where a pupil of compulsory school age is absent, schools have a
                responsibility to:{' '}
              </p>
              <ul className="govuk-list govuk-list--bullet">
                <li>ascertain the reason</li>
                <li>ensure the proper safeguarding action is taken</li>
                <li>
                  indicate in their register whether the absence is authorised
                  by the school or unauthorised
                </li>
                <li>
                  identify the correct code to use before entering it on to the
                  school’s electronic register, or management information system
                  which is then used to download data to the school census. A
                  code set of these is available in <a href="#">Annex C</a>.
                </li>
              </ul>
              <p>
                The Parent of every child of compulsory school age is required
                to ensure that the child receive a suitable full time education
                to the child’s ability, age, aptitude and any special education
                needs the child may have either by regular attendance at school
                or otherwise. Failure of a parent to secure regular attendance
                of their school registered child of compulsory school age can
                lead to a penalty notice or prosecution. Local authorities (LAs)
                and schools have legal responsibilities regarding accurate
                recording of a pupil’s attendance. Further information is
                available in the Departmental advice on school attendance.{' '}
              </p>
              <h3 id="1.2">Uses and users</h3>
              <p>
                Data used to derive published absence statistics is collected
                via the school census. There is widespread use of data from the
                schools census. In addition to mainstream and specialist media
                coverage of our statistical publications that data are used by a
                range of companies. These include housing websites such as
                Rightmove and Zoopla, specialist publications such as the good
                schools guide, organisations providing data analysis services to
                schools such as Fischer Family Trust. The data is well used by
                the academic research community (e.g. Durham University),
                education think tanks (Education Policy Institute). It is also
                used by central government (DfE, Ofsted, other government
                departments).
              </p>
              <p>
                The published data are used frequently in answers to
                parliamentary questions and public enquiries, including those
                made under the Freedom of Information Act.
              </p>
            </div>
          </div>
        </AccordionSection>

        <AccordionSection heading="Background">
          <div className="govuk-grid-row">
            <div className="govuk-grid-column-one-quarter">
              <h3 className="govuk-heading-s">In this section</h3>
              <ul className="govuk-body-s">
                <li>
                  <a href="#2.1">Current termly publications</a>
                </li>
                <li>
                  <a href="#2.2">Key absence measures</a>
                </li>
                <li>
                  <a href="#2.3">Cohort used in absence measures</a>
                </li>
                <li>
                  <a href="#2.4">
                    The school year (five half terms vs six half terms)
                  </a>
                </li>
                <li>
                  <a href="#2.5">
                    Published geographical and characteristics breakdowns
                  </a>
                </li>
                <li>
                  <a href="#2.6">
                    Underlying data provided alongside publications
                  </a>
                </li>
                <li>
                  <a href="#2.7">Suppression of absence data</a>
                </li>
                <li>
                  <a href="#2.8">Other related publications</a>
                </li>
                <li>
                  <a href="#2.9">
                    Devolved administration statistics on absence
                  </a>
                </li>
              </ul>
            </div>
            <div className="govuk-grid-column-three-quarters">
              <h3 id="2.1">Current termly publications</h3>
              <p>
                The Department publishes termly pupil absence data via three
                National Statistics releases each year. These are:
              </p>
              <ul className="govuk-list govuk-list--bullet">
                <li>autumn term, published in May</li>
                <li>autumn and spring terms, published in October</li>
                <li>full year, published in March</li>
              </ul>
              <p>
                Only the full year absence release gives a definitive view of
                pupil absence. Termly publications can be affected significantly
                by term length and therefore findings from these releases are
                for indicative purposes only and the results should be treated
                with caution.
              </p>
              <p>
                The Department’s annual absence statistical releases have been
                badged as National Statistics since the 1999/00 academic year
                publication. The termly and two term combined releases were
                badged as National Statistics slightly later. The combined
                autumn and spring term release was badged as national statistics
                from the autumn 2006 and spring 2007 publication and the single
                term releases were badged as National Statistics as of the
                autumn term 2009 publication.
              </p>
              <h4>Historical publications</h4>
              <p>
                Prior to the 2012/13 academic year the Department also published
                spring term only absence data. However, this was discontinued as
                it was deemed no longer necessary and of the least importance to
                users. The last published spring term release was{' '}
                <a href="https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-spring-term-2012">
                  Pupil absence in schools in England: spring term 2012
                </a>{' '}
                published on 30th August 2012.
              </p>
              <p>
                For the 2005/06 academic year, due to the transition of absence
                collection between the Absence in Schools Survey and the school
                census, absence information was published for secondary schools
                only as a National Statistics release. Absence data for 2005/06
                were not released on a termly basis as this was the first year
                of collection.
              </p>
              <p>
                For academic years 1999/00 to 2004/05, annual pupil absence
                information was collected via the Absence in Schools Survey and
                published as an annual National Statistics release.
              </p>
              <p>
                For academic years 1993/94 to 1998/99, annual pupil absence
                information was collected via the Absence in Schools Survey and
                published via Statistical bulletins. Links to all absence
                publications can be found in <a href="#">Annex D</a>.
              </p>
              <h3 id="2.2">Key absence measures</h3>
              <p>
                The Department monitors pupil absence levels using two key
                measures - overall absence rate and persistent absence (PA)
                rate. The key measures are calculated for pupils who are of
                compulsory school age i.e. aged between 5 and 15 as at the start
                of the academic year (31st August).
              </p>
              <p>
                Absence information is reported as a totals or rates across a
                period, usually by school term or academic year.
              </p>
              <p>
                Overall absence is the aggregated total of all authorised and
                unauthorised absences.
              </p>
              <p>
                Authorised absence is absence with permission from a teacher or
                other authorised representative of the schools. This includes
                instances of absence for which a satisfactory explanation has
                been provided e.g. illness.
              </p>
              <p>
                Unauthorised absence is absence without permission from the
                school. This includes all unexplained or unjustified absences
                and arrivals after registration has closed. See overall absence
                methodology section for further information.
              </p>
              <p>
                Persistent absence is when a pupil enrolment’s overall absence
                equates to 10 per cent or more of their possible sessions.
              </p>
              <p>
                See persistent absence methodology section for further
                information.
              </p>
              <h3 id="2.3">Cohort used in absence measures</h3>
              <p>
                Absence information is collected and disseminated at enrolment
                level rather than pupil level. This means that where a pupil has
                moved school throughout the year, they will be counted more than
                once as they have recorded attendance and absence at more than
                one school. This allows for schools to be held accountable for
                pupil absences, as the absence is attached to enrolments at a
                particular school, not the individual pupil. All the enrolments
                at a school over the period in question are included in the
                absence measures, not just the pupils on roll at a particular
                date. Schools only record absence for the period a pupil is on
                roll at their school. The number of pupil enrolments is
                approximately 4 per cent higher than the number of pupils.
              </p>
              <h3 id="2.4">
                The school year (five half terms vs six half terms)
              </h3>
              <p>
                Generally, the academic year is made up of three terms, autumn,
                spring and summer. Each term has two parts (half terms) which
                are usually separated by a half term break.
              </p>
              <p>
                Since the 2012/13 academic year, pupil absence information has
                been collected for the full academic year i.e. all six half
                terms. However, prior to this absence information was collected
                for the first five half terms only, meaning absences in the
                second half of the summer term were not collected.
              </p>
              <p>
                Since the 2012/13 academic year, the Department’s key absence
                indicators have been based on the full academic year’s (six half
                term) data. However, as we are unable to rework time series
                tables or provide any historical six half term absence levels
                the Department continued to publish a full set of absence
                information for the first five half terms up to and including
                the 2013/14 academic year. Following this, a single csv file
                based on data for five half terms has been published alongside
                the annual absence publications to enable users to still make
                longer term time comparisons on this basis if they wish to.
              </p>
              <p>
                To account for high levels of study leave and other authorised
                absences for pupils aged 15 in the second half of the summer
                term, all possible sessions and absences relating to this period
                for 15 year olds (as at the start of the academic year) are
                removed prior to any analysis being undertaken and are not
                included in any published statistics.
              </p>
              <table className="govuk-table">
                <caption className="govuk-table__caption">
                  Table 1: State-funded primary, secondary and special schools -
                  pupils of compulsory school age pupil and enrolment numbers
                  comparison
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
                      <sup>1</sup> Pupils with a sole or dual main registration,
                      aged between 5 and 15 who are not boarders as of the
                      January school census each year.
                    </td>
                  </tr>
                </tfoot>
              </table>
              <p>
                In published absence statistics, pupil enrolments who first
                enrolled at a school within the second half of the summer term
                are not included. This is to ensure the same cohorts of
                enrolments are included in both the five and six half term
                absence measures. Screen reader support enabled.
              </p>
              <h3 id="2.5">
                Published geographical and characteristics breakdowns
              </h3>
              <p>
                The Department routinely publishes pupil absence information at
                national, local authority and school level including breakdowns
                by pupil characteristics.
              </p>
              <p>
                The autumn term absence publication provides high level
                information designed to give an early indication on absence
                levels and the effect of winter illness. Information provided
                includes authorised, unauthorised and overall absence rates,
                absence broken down by reason, the number of pupils with one or
                more sessions of absence for different reasons and information
                on persistent absence.
              </p>
              <p>
                The combined autumn and spring term publication includes similar
                information to that of the autumn term. However, it also
                includes absence levels broken down by pupils’ gender, free
                school meal eligibility, national curriculum year group, first
                language, special educational need and ethnic group.
              </p>
              <p>
                The full academic year's absence publication includes combined
                absence information for the autumn, spring and summer terms. It
                is the largest publication and includes similar breakdowns to
                that of the combined autumn and spring term publication (as
                outlined above) as well as persistent absence broken down by
                reason for absence and pupil characteristic. Additional
                breakdowns included in this full year release relate to the
                distribution of enrolments by length of overall absence,
                percentage of enrolments by their overall absence and number of
                schools by the percentage of persistent absentees. In this
                publication, information is also provided at district level,
                based on Income Deprivation Affecting Children Index (IDACI) and
                by degree of rurality. In addition, from 2015/16 onwards,
                characteristics include free school meal eligibility in the last
                6 years.
              </p>
              <p>
                The Income Deprivation Affecting Children Index (IDACI) is
                provided by the Department for Communities and Local Government
                (CLG). The index measures the proportion of all children aged 0
                to 15 living in income deprived families and is based on
                Lower-layer Super Output Areas (LSOAs) in England. Each LSOA is
                given a rank between 1 and 32,844 where the LSOA with the rank
                of 1 is the most deprived LSOA and the LSOA with the rank of
                32,844 is the most deprived. IDACI is a subset of the Income
                Deprivation Domain of the Index of Multiple Deprivation 2015,
                which which measures the proportion of the population in an area
                experiencing deprivation relating to low income. The definition
                of low income used includes both those people that are
                out-of-work, and those that are in work but who have low
                earnings (and who satisfy the respective means tests). Further
                information about IDACI can be found on the CLG site.
              </p>
              <p>
                IDACI bands from 2014/15 are based on 2015 IDACI scores. IDACI
                bands for 2010/11 to 2013/14 are based on 2010 IDACI scores and
                those for 2007/08 to 2009/10 are based on 2007 IDACI scores.
                Care should be taken when comparing IDACI tables based on
                different IDACI scores.
              </p>
              <p>
                The Rural and Urban Area Classification is a product of a joint
                project to produce a single and consistent classification of
                urban and rural areas. The project was sponsored by a number of
                government departments. The rural and urban definitions classify
                output areas, wards and super output areas by aggregating the
                underlying hectare grid squares classifications for the measures
                of settlement size and sparsity. Up to eight classes of output
                areas could be distinguished; four settlement types (urban, town
                and fringe, village, hamlet and isolated dwelling) in either a
                sparse or less sparse regional setting.
              </p>
              <p>
                Absence data by degree of rurality from 2014/15 has been
                analysed based on the 2011 Rural and Urban Area Classification,
                whereas equivalent data for previous years was analysed based on
                the 2004 Rural and Urban Area Classification. Further
                information about the Rural and Urban Area Classification 2011
                can be found on the Office for National Statistics website
              </p>
              <p>
                A full list of published absence breakdowns (as of the latest
                academic year’s releases) is available in Annex E.
              </p>
              <p>
                From 2015/16 onwards, published tables on characteristics
                breakdowns include figures for pupils with unclassified or
                missing characteristics information. This represents a small
                proportion of all pupils and the figures should be interpreted
                with caution. For some characteristics, like free school meals
                eligibility, pupils with unclassified or missing characteristics
                information have been found to have a low average number of
                sessions possible, which might explain more variability in
                absence rates which use the number of possible sessions as a
                denominator.
              </p>
              <h3 id="2.6">Underlying data provided alongside publications</h3>
              <p>
                From the 2009/10 academic year, each National Statistics release
                has been accompanied by underlying data, including national,
                local authority and school level information. Alongside the
                underlying data there is an accompanying document (metadata)
                which provides further information on the contents of these
                files. This data is released under the terms of the Open
                Government License and is intended to meet at least 3 stars for
                Open Data.
              </p>
              <p>
                Following the ‘
                <a href="https://assets.publishing.service.gov.uk/government/uploads/system/uploads/attachment_data/file/467936/Absence_statistics_changes_-_consultation_response.pdf">
                  Consultation on improvements to pupil absence statistics
                </a>
                ’, results published in October 2015, releases are now
                accompanied by time series underlying data, containing
                additional breakdowns and data from 2006/07 to the latest year.
                This additional data is intended to provide users with all
                information in one place and give them the option of producing
                their own analysis.
              </p>
              <h3 id="2.7">Suppression of absence data</h3>
              <p>
                The Code of Practice for Official Statistics requires that
                reasonable steps should be taken to ensure that all published or
                disseminated statistics produced by the Department for Education
                protects confidentiality.{' '}
              </p>
              <p>
                To do this totals are rounded and small numbers are suppressed
                according to the following rules:{' '}
              </p>
              <ul className="govuk-list govuk-list--bullet">
                <li>
                  enrolment numbers at national and regional levels are rounded
                  to the nearest 5. Local authority totals across school types
                  are also rounded to the nearest 5 to prevent disclosure of any
                  supressed values
                </li>
                <li>
                  enrolment numbers of 1 or 2 are suppressed to protect pupil
                  confidentiality
                </li>
                <li>
                  where the numerator or denominator of any percentage
                  calculated on enrolment numbers of 1 or 2, the percentage is
                  suppressed. This suppression is consistent with the
                  <a href="http://media.education.gov.uk/assets/files/policy%20statement%20on%20confidentiality.pdf">
                    Departmental statistical policy
                  </a>
                </li>
                <li>
                  where any number is shown as 0, the original figure was also 0
                </li>
              </ul>

              <table className="govuk-table">
                <thead>
                  <tr>
                    <th colSpan={2}>
                      Symbols used to identify this in published tables are as
                      follows:
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
                    <td>
                      Small number suppressed to preserve confidentiality{' '}
                    </td>
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
              <h3 id="2.8">Other related publications</h3>
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
                    <li>
                      the link between absence and attainment at KS2 and KS4
                    </li>
                  </ul>
                </li>
              </ul>
              <p>
                In addition historical pupil absence data is available in the
                following publications which have been discontinued:
              </p>
              <ul className="govuk-list govuk-list--bullet">
                <li>
                  special Educational Needs: an analysis (Department for
                  Education) - data up to and including 2013/14
                </li>
                <li>
                  neighbourhood statistics (ONS small area tables) - 2006/07 to
                  2012/13 inclusive
                </li>
              </ul>
              <h3 id="2.9">Devolved administration statistics on absence</h3>
              <p>
                The Department collects and reports on absence information from
                schools in England. For information for Wales, Scotland and
                Northern Ireland, contact the departments below or access their
                statistics at the following links:
              </p>
              <p>Wales: school.stats@wales.gsi.gov.uk or</p>
              <p>Welsh Government – Statistics and Research</p>
              <p>Scotland: school.stats@wales.gsi.gov.uk or</p>
              <p>Scottish Government – School Education Statistics</p>
              <p>Northern Ireland: statistics@deni.gov.uk or</p>
              <p>Department of Education – Education Statistics</p>
            </div>
          </div>
        </AccordionSection>

        <AccordionSection heading="Methodology">
          <div className="govuk-grid-row">
            <div className="govuk-grid-column-one-quarter">
              <h3 className="govuk-heading-s">In this section</h3>
              <ul className="govuk-body-s">
                <li>
                  <a href="#3.1">Overall absence methodology</a>
                </li>
                <li>
                  <a href="#3.2">Persistent absence methodology</a>
                </li>
              </ul>
            </div>
            <div className="govuk-grid-column-three-quarters">
              <h3 id="3.1">Overall absence methodology</h3>
              <p>
                An enrolment’s overall absence rate is the total number of
                overall absence sessions as a percentage of the total number of
                possible sessions available to that enrolment, where overall
                absence is the sum of authorised and unauthorised absence and
                one session is equal to half a day
              </p>
              <p>
                To calculate school, local authority and national level overall
                absence rates, the total absences and possible sessions for all
                enrolments within the relevant areas are included.
              </p>
              <p className="govuk-body-s">
                Overall absence rate = (Total overall absence sessions) / Total
                sessions possible) X 100
              </p>
              <h3 id="3.2">Persistent absence methodology</h3>
              <p>
                The persistent absence measure was introduced in 2005/06. The
                sections below outline how the measure has changed since it was
                implemented.
              </p>
              <p>
                In published releases, to allow users to compare over time, time
                series information is recalculated following any methodology
                change.
              </p>
              <h4>
                Overview of persistent absence measures used since 2005/06
              </h4>
              <table className="govuk-table">
                <caption className="govuk-table-caption">
                  Table 2: Persistent absence measures since 2005/06
                </caption>
                <thead>
                  <tr>
                    <th>Description of persistent absence measure</th>
                    <th>Academic years</th>
                  </tr>
                </thead>
                <tbody>
                  <tr>
                    <td>
                      10% or more of sessions missed (based on each pupil’s
                      possible sessions)
                    </td>
                    <td>2015/16 onwards</td>
                  </tr>
                  <tr>
                    <td>
                      Around 15% or more of sessions missed (based on a standard
                      threshold)
                    </td>
                    <td>2010/11 to 2014/15</td>
                  </tr>
                  <tr>
                    <td>
                      Around 20% or more of sessions missed (based on a standard
                      threshold)
                    </td>
                    <td>2005/06 to 2009/10</td>
                  </tr>
                </tbody>
              </table>
              <h4>
                2015/16 onwards (10 per cent, based on an exact methodology)
              </h4>
              <p>
                Since the start of the 2015/16 academic year schools, a pupil
                has been classified as a persistent absentee if they miss 10 per
                cent or more of their own possible sessions, rather than if they
                reach a standard threshold of absence sessions. Meaning, that if
                an enrolment’s overall absence rate 10 per cent or higher2 they
                will be classified as persistently absent. See{' '}
                <a href="#3.1">overall absence methodology</a> section for
                further information.
              </p>
              <p>
                To calculate school, local authority and national level
                persistent absence rates, all persistently absent enrolments
                within the relevant areas are included.
              </p>
              <p className="govuk-body-s">
                Persistent absence rate = (Number of enrolments classed as
                persistent absentees / Number of enrolments) X 100
              </p>
              <p>
                Note that, although the measure was only in place from September
                2015, figures based on the 10 per cent exact methodology were
                published alongside the 2014/15 Pupil absence in schools in
                England releases for information purposes only.
              </p>
              <p>
                Table 3 provides a comparison of figures at the 10% and 15%
                level using the previous threshold methodology and the current
                exact methodology, based on figures for the full 2014/15
                academic year, when the change became effective.
              </p>
              <table className="govuk-table">
                <caption className="govuk-table-caption">
                  Table 3: Comparison of the number and percentage of persistent
                  absentees for the 2014/15 academic year based on the threshold
                  (previous) and exact (current) methodology
                </caption>
                <thead>
                  <tr>
                    <td rowSpan={2} />
                    <th
                      colSpan={2}
                      scope="col"
                      className="govuk-table__cell--numeric"
                    >
                      Threshold methodology
                    </th>
                    <th
                      colSpan={2}
                      scope="col"
                      className="govuk-table__cell--numeric"
                    >
                      Exact methodology
                    </th>
                  </tr>
                  <tr>
                    <th className="govuk-table__cell--numeric">Number</th>
                    <th className="govuk-table__cell--numeric">Percentage</th>
                    <th className="govuk-table__cell--numeric">Number</th>
                    <th className="govuk-table__cell--numeric">Percentage</th>
                  </tr>
                </thead>
                <tbody>
                  <tr>
                    <th colSpan={5}>Total</th>
                  </tr>
                  <tr>
                    <td>10 per cent</td>
                    <td className="govuk-table__cell--numeric">327,070</td>
                    <td className="govuk-table__cell--numeric">9.4</td>
                    <td className="govuk-table__cell--numeric">728,080</td>
                    <td className="govuk-table__cell--numeric">11.0</td>
                  </tr>
                  <tr>
                    <td>15 per cent</td>
                    <td className="govuk-table__cell--numeric">245,840</td>
                    <td className="govuk-table__cell--numeric">3.7</td>
                    <td className="govuk-table__cell--numeric">308,100</td>
                    <td className="govuk-table__cell--numeric">4.6</td>
                  </tr>

                  <tr>
                    <th colSpan={5}>Primary</th>
                  </tr>
                  <tr>
                    <td>10 per cent</td>
                    <td className="govuk-table__cell--numeric">257,945</td>
                    <td className="govuk-table__cell--numeric">6.9</td>
                    <td className="govuk-table__cell--numeric">314,440</td>
                    <td className="govuk-table__cell--numeric">8.4</td>
                  </tr>
                  <tr>
                    <td>15 per cent</td>
                    <td className="govuk-table__cell--numeric">79,955</td>
                    <td className="govuk-table__cell--numeric">2.1</td>
                    <td className="govuk-table__cell--numeric">113,160</td>
                    <td className="govuk-table__cell--numeric">3.0</td>
                  </tr>

                  <tr>
                    <th colSpan={5}>Secondary</th>
                  </tr>
                  <tr>
                    <td>10 per cent</td>
                    <td className="govuk-table__cell--numeric">347,425</td>
                    <td className="govuk-table__cell--numeric">12.3</td>
                    <td className="govuk-table__cell--numeric">390,185</td>
                    <td className="govuk-table__cell--numeric">13.8</td>
                  </tr>
                  <tr>
                    <td>15 per cent</td>
                    <td className="govuk-table__cell--numeric">152,775</td>
                    <td className="govuk-table__cell--numeric">5.4</td>
                    <td className="govuk-table__cell--numeric">180,610</td>
                    <td className="govuk-table__cell--numeric">6.4</td>
                  </tr>

                  <tr>
                    <th colSpan={5}>Special</th>
                  </tr>
                  <tr>
                    <td>10 per cent</td>
                    <td className="govuk-table__cell--numeric">21,700</td>
                    <td className="govuk-table__cell--numeric">25.4</td>
                    <td className="govuk-table__cell--numeric">23,460</td>
                    <td className="govuk-table__cell--numeric">27.5</td>
                  </tr>
                  <tr>
                    <td>15 per cent</td>
                    <td className="govuk-table__cell--numeric">13,110</td>
                    <td className="govuk-table__cell--numeric">15.4</td>
                    <td className="govuk-table__cell--numeric">14,330</td>
                    <td className="govuk-table__cell--numeric">16.8</td>
                  </tr>
                </tbody>
              </table>

              <p>
                Further details on the rationale for the methodology change can
                be found in the original consultation document on proposed
                changes to absence statistics which was published in October
                2014 in Section 9 of the Pupil absence in schools in England:
                autumn 2013 and spring 2014 Statistical First Release. A
                consultation response summarising feedback received from users
                on the proposed change to the methodology and next steps,
                published in October 2015, can also be found on the same
                webpage.
              </p>

              <h4>2010/11 to 2014/15 methodology (15 per cent PA threshold)</h4>

              <p>
                Pupils were identified as persistent absentees by comparing the
                number of overall absence sessions they had against a standard
                threshold of around 15 per cent of possible sessions, equating
                to 56 or more sessions across the full academic year for pupils
                aged 5 to 14 and 46 or more sessions across the full academic
                year for pupils aged 15 (whose absence information is based on
                the first five half terms only, as any sixth half term absence
                is removed prior to analysis as set out in the section on the
                <a href="#2.4">
                  school year (five half terms vs six half terms)
                </a>
                .
              </p>
              <p>
                Prior to 2012/13 (when absence data was first collected for the
                second half of the summer term), the threshold was 46 sessions
                across the first five half terms of the year for all pupils.
              </p>
              <p>
                For the autumn term, figures showed how many enrolments had
                already become persistent absentees, as well as those who may
                become persistent absentees based on the standard threshold of
                22 or more sessions of absence.
              </p>
              <p>
                For the autumn and spring terms, figures showed how many
                enrolments had already become persistent absentees, as well as
                those who may become persistent absentees based on the standard
                threshold of 38 or more sessions of absence.
              </p>
              <p>
                Standard termly persistent absentee thresholds were calculated
                by taking 15 per cent of the mode (most common number of)
                possible sessions for all enrolments. This meant that, in some
                cases, the standard threshold may be more or less than 15 per
                cent of an individual pupil’s sessions. The termly persistent
                absence thresholds were reviewed each term to account for any
                changes to the mode possible sessions, but the thresholds did
                not tend to change from year to year. See table 4 for standard
                thresholds.
              </p>
              <table className="govuk-table">
                <caption className="govuk-table-cap">
                  Table 4: Standard cumulative persistent absence thresholds for
                  missing 15 per cent of possible sessions
                </caption>
                <thead>
                  <tr>
                    <th scope="col">Term</th>
                    <th scope="col">Threshold</th>
                  </tr>
                </thead>
                <tbody>
                  <tr>
                    <td>Autumn term</td>
                    <td>22 sessions</td>
                  </tr>
                  <tr>
                    <td>
                      Spring term (as of 2012/13, no longer published
                      individually)
                    </td>
                    <td>16 sessions</td>
                  </tr>
                  <tr>
                    <td>Autumn and spring terms</td>
                    <td>38 sessions</td>
                  </tr>
                  <tr>
                    <td>First five half terms</td>
                    <td>46 sessions</td>
                  </tr>
                  <tr>
                    <td>Full academic year – six half terms</td>
                    <td>56 sessions</td>
                  </tr>
                </tbody>
              </table>
              <h4>2005/06 to 2009/10 methodology (20 per cent PA threshold)</h4>
              <p>
                The persistent absence measure was first introduced in 2005/06,
                where a pupil was identified as a persistent absentee if they
                missed around 20 per cent or more of possible sessions.{' '}
              </p>

              <p>
                Persistent absence figures were published in a similar way to
                that of the 15 per cent threshold figures that were in place
                between 2010/11 and 2013/14. With both the autumn term and
                autumn and spring term releases showing how many enrolments 17
                have already become persistent absentees, as well as those who
                may become, persistent absentees based on the standard
                threshold. See table 5 for standard thresholds to which missing
                20 per cent of sessions equated to.{' '}
              </p>
              <p>
                This threshold was reduced to 15 per cent as of September 2010.
              </p>
              <table className="govuk-table">
                <caption className="govuk-table-caption">
                  Table 5: Standard absence thresholds for missing 20 per cent
                  of possible sessions, between 2005/06 and 2009/10
                </caption>
                <thead>
                  <tr>
                    <th scope="col">Term</th>
                    <th scope="col">Threshold</th>
                  </tr>
                </thead>
                <tbody>
                  <tr>
                    <td>Autumn term</td>
                    <td>28 sessions</td>
                  </tr>
                  <tr>
                    <td>Spring term</td>
                    <td>52 sessions</td>
                  </tr>
                  <tr>
                    <td>Autumn and Spring term</td>
                    <td>52 sessions</td>
                  </tr>
                  <tr>
                    <td>First five half terms</td>
                    <td>64 sessions</td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>
        </AccordionSection>

        <AccordionSection heading="Data collection">
          <p>DATA COLLECTION</p>
        </AccordionSection>

        <AccordionSection heading="Data processing">
          <p>DATA PROCESSING</p>
        </AccordionSection>

        <AccordionSection heading="Research relating to pupil absence">
          <p>PUPIL ABSENCE</p>
        </AccordionSection>

        <AccordionSection heading="Contacts">
          <p>Contacts</p>
        </AccordionSection>
      </Accordion>

      <div className="govuk-!-margin-top-9">
        <a href="#print" className="govuk-link">
          Print this page
        </a>
      </div>
    </PrototypePage>
  );
};

export default PublicationPage;
