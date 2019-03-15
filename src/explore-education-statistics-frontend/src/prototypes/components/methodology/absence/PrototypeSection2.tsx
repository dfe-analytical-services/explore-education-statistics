import classNames from 'classnames';
import React from 'react';
import Link from '../../../../components/Link';

const PrototypeMethodologySection = () => {
  return (
    <>
      <h3 id="section1-1">
        Requirements of schools in ensuring pupil attendance
      </h3>
      <p>
        All maintained schools are required to provide two possible sessions per
        day, morning and afternoon, to all pupils. The length of each session,
        break and the school day is determined by the school’s governing body.
        Schools must meet for at least 380 sessions or 190 days during any
        school year to educate their pupils. If a school is prevented from
        meeting for one or more sessions because of an unavoidable event, it
        should find a practical way of holding extra sessions. However, if it
        cannot find a practical way of doing this then it is not required to
        make up the lost sessions. Academy and free school funding agreements
        state that the duration of the school day/sessions is the responsibility
        of the academy trust. Schools are required to take attendance registers
        twice a day: once at the start of the first/morning session of each
        school day and once during the second/afternoon session. In their
        register, schools are required to record whether pupils are:{' '}
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
          indicate in their register whether the absence is authorised by the
          school or unauthorised
        </li>
        <li>
          identify the correct code to use before entering it on to the school’s
          electronic register, or management information system which is then
          used to download data to the school census. A code set of these is
          available in <a href="#">Annex C</a>.
        </li>
      </ul>
      <p>
        The Parent of every child of compulsory school age is required to ensure
        that the child receive a suitable full time education to the child’s
        ability, age, aptitude and any special education needs the child may
        have either by regular attendance at school or otherwise. Failure of a
        parent to secure regular attendance of their school registered child of
        compulsory school age can lead to a penalty notice or prosecution. Local
        authorities (LAs) and schools have legal responsibilities regarding
        accurate recording of a pupil’s attendance. Further information is
        available in the Departmental advice on school attendance.{' '}
      </p>
      <h3 id="section1-2">Uses and users</h3>
      <p>
        Data used to derive published absence statistics is collected via the
        school census. There is widespread use of data from the schools census.
        In addition to mainstream and specialist media coverage of our
        statistical publications that data are used by a range of companies.
        These include housing websites such as Rightmove and Zoopla, specialist
        publications such as the good schools guide, organisations providing
        data analysis services to schools such as Fischer Family Trust. The data
        is well used by the academic research community (e.g. Durham
        University), education think tanks (Education Policy Institute). It is
        also used by central government (DfE, Ofsted, other government
        departments).
      </p>
      <p>
        The published data are used frequently in answers to parliamentary
        questions and public enquiries, including those made under the Freedom
        of Information Act.
      </p>
    </>
  );
};

export default PrototypeMethodologySection;
