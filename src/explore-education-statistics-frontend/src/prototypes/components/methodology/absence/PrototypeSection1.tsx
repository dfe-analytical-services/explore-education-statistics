import classNames from 'classnames';
import React from 'react';
import Link from '../../../../components/Link';

const PrototypeMethodologySection = () => {
  return (
    <>
      <h3 id="section1-1">Pupil attendance requirements for schools</h3>
      <p>
        All maintained schools are required to provide 2 possible sessions per
        day, morning and afternoon, to all pupils.
      </p>
      <p>
        The length of each session, break and the school day is determined by
        the school’s governing body.
      </p>
      <p>
        Schools must meet for at least 380 sessions or 190 days during any
        school year to educate their pupils.
      </p>
      <p>
        If a school is prevented from meeting for 1 or more sessions because of
        an unavoidable event, it should find a practical way of holding extra
        sessions.
      </p>
      <p>
        However, if it cannot find a practical way of doing this then it’s not
        required to make up the lost sessions.
      </p>
      <p>
        Academy and free school funding agreements state that the duration of
        the school day and sessions are the responsibility of the academy trust.
      </p>
      <p>
        Schools are required to take attendance registers twice a day - once at
        the start of the first morning session and once during the second
        afternoon session.
      </p>
      <p>
        In their register, schools are required to record whether pupils are:{' '}
      </p>
      <ul className="govuk-list govuk-list--bullet">
        <li>absent</li>
        <li>attending an approved educational activity</li>
        <li>present</li>
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
          available in <a href="#">Annex C</a>
        </li>
      </ul>
      <p>
        The parent of every child of compulsory school age is required to ensure
        their child receives a suitable full-time education for their ability,
        age, aptitude and any special education needs they may have either by
        regular attendance at school or otherwise.
      </p>
      <p>
        Failure of a parent to secure regular attendance of their school
        registered child of compulsory school age can lead to a penalty notice
        or prosecution.
      </p>
      <p>
        Local authorities (LAs) and schools have legal responsibilities
        regarding accurate recording of a pupil’s attendance.
      </p>
      <p>
        Further information is available in DfE's advice on school attendance.{' '}
      </p>
      <h3 id="section1-2">Uses and users of absence data and statistics</h3>
      <p>
        The data used to publish absence statistics is collected via the school
        census which is used by a variety of companies and organisations
        including:{' '}
      </p>
      <ul className="govuk-list govuk-list--bullet">
        <li>
          mainstream and specialist media companies such as Rightmove and Zoopla
        </li>
        <li>specialist publications such as the good schools guide</li>
        <li>data analysis organisations such the Fischer Family Trust</li>
        <li>
          academic research and think tank organisations such as Durham
          University and the Education Policy Institute
        </li>
        <li>
          central government organisations such as DfE, Ofsted other government
          departments
        </li>
      </ul>
      <p>
        The published data is also used in answers to parliamentary questions
        and public enquiries - including those made under the Freedom of
        Information Act.
      </p>
    </>
  );
};

export default PrototypeMethodologySection;
