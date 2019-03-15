import React from 'react';
import Details from '../../components/Details';

const PrototypeDataTileHighlights = () => (
  <div className="dfe-dash-tiles dfe-dash-tiles--2-in-row">
    <div className="dfe-dash-tiles__tile">
      <h3 className="govuk-heading-s dfe-dash-tiles__heading">
        Average attainment 8 score per pupil
      </h3>

      <p className="govuk-body ">
        <span className="govuk-body govuk-!-font-weight-bold govuk-!-font-size-36">
          44.6 {}
        </span>
        (average score for all schools). In comparison to 2016, the average
        Attainment 8 score per pupil has decreased by <strong>3.9</strong>{' '}
        points.
      </p>

      <p className="govuk-body ">
        <span className="govuk-body govuk-!-font-weight-bold govuk-!-font-size-36">
          46.3 {}
        </span>
        (average score state funded schools). In comparison to 2016, the average
        Attainment 8 score per pupil has decreased by <strong>3.6</strong>{' '}
        points.
      </p>

      <Details summary="What is the attainment 8 score?">
        Attainment 8 measures the average achievement of pupils in up to 8
        qualifications including English (double weighted if both language and
        literature are taken), maths (double weighted), three further
        qualifications that count in the English Baccalaureate (EBacc) and three
        further qualifications that can be GCSE qualifications (including EBacc
        subjects) or any other non-GCSE qualifications on the{' '}
        <a href="#">DfE approved list</a>.
      </Details>
    </div>

    <div className="dfe-dash-tiles__tile">
      <h3 className="govuk-heading-s dfe-dash-tiles__heading">
        Floor standard and coasting definition
      </h3>

      <p className="govuk-body">
        <span className="govuk-body govuk-!-font-weight-bold govuk-!-font-size-36">
          365 {}
        </span>
        schools are below the secondary school floor standard (
        <strong>12%</strong> of total state funded mainstream schools).
      </p>

      <Details summary="What is the floor standard and coasting definition?">
        <h3 className="govuk-heading-s">The floor standard</h3>
        <p>
          The 2017 floor standard is the same as in 2016. A school is below the
          floor if:
        </p>
        <ol className="govuk-list govuk-list--number">
          <li>Its Progress 8 score is below -0.5; and</li>
          <li>The upper band of the 95% confidence interval is below zero.</li>
        </ol>
        <p>Schools are also excluded from the floor standards where:</p>
        <ul className="govuk-list govuk-list--bullet">
          <li>
            there are fewer than six pupils in the year 11 cohort, or included
            in the Progress 8 measure; or
          </li>
          <li>
            fewer than 50% of pupils have key stage 2 assessments that can be
            used as prior attainment in the calculation of Progress 8
          </li>
        </ul>
      </Details>

      <p className="govuk-body">
        <span className="govuk-body govuk-!-font-weight-bold govuk-!-font-size-36">
          171 {}
        </span>
        schools meet the coasting definition (<strong>9.6%</strong> of total
        schools). <strong>169</strong> schools are both below and meet the
        coasting definition.
      </p>

      <Details summary="What is the coasting definition?">
        <h3 className="govuk-heading-s">Coasting schools</h3>
        <p>
          A school will fall within the coasting definition if data shows that
          over time, it has not supported its pupils to fulfil their potential.
          A secondary school will meet the coasting definition if:
        </p>
        <ol className="govuk-list govuk-list--number">
          <li>
            In 2015, fewer than 60% of pupils achieved 5+ A* to C grades
            including English and maths, and the school has less than the
            national median percentage of pupils who achieved expected progress
            in English and in mathematics<sup>24</sup>; and
          </li>
          <li>
            2016 and 2017, the school has a Progress 8 score below -0.25 and the
            upper band of the 95% confidence interval is below zero.
          </li>
        </ol>
      </Details>
    </div>

    <div className="dfe-dash-tiles__tile">
      <h3 className="govuk-heading-s dfe-dash-tiles__heading">
        EBacc entry and achievement
      </h3>

      <p className="govuk-body">
        <span className="govuk-body govuk-!-font-weight-bold govuk-!-font-size-36">
          38.2% {}
        </span>
        of pupils in state funded schools entered the EBacc and{' '}
        <strong>21.3%</strong> achieved the EBacc.
      </p>

      <p className="govuk-body">
        <span className="govuk-body govuk-!-font-weight-bold govuk-!-font-size-36">
          23.7% {}
        </span>
        of pupils achieved the EBacc by gaining grades 4 or above in English and
        maths GCSEs and grades C or above in unreformed subject areas.
      </p>

      <Details summary="What is EBacc entry and achievement?">
        <h3 className="govuk-heading-s">
          The English Baccalaureate (EBacc) entry and achievement
        </h3>
        <p>
          The EBacc was first introduced into the performance tables in 2009/10.
          It allows people to see how many pupils reach the attainment threshold
          in core academic subjects at key stage 4. The EBacc is made up of
          English, maths, science, a language, and history or geography. To
          count in the EBacc, qualifications must be on the{' '}
          <a href="https://www.gov.uk/government/publications/english-baccalaureate-eligible-qualifications">
            English Baccalaureate list of qualifications
          </a>
          .
        </p>
      </Details>
    </div>

    <div className="dfe-dash-tiles__tile">
      <h3 className="govuk-heading-s dfe-dash-tiles__heading">
        Percentage achieving the threshold of a grade 4 or above in English and
        maths
      </h3>

      <p className="govuk-body">
        <span className="govuk-body govuk-!-font-weight-bold govuk-!-font-size-36">
          59.1% {}
        </span>
        of pupils in all schools achieved grade 4 or above in English and maths.
      </p>

      <p className="govuk-body">
        <span className="govuk-body govuk-!-font-weight-bold govuk-!-font-size-36">
          63.9% {}
        </span>
        of pupils in state funded schools achieved grade 4 or above in English
        and maths.
      </p>

      <Details summary="What is a threshold of grade 4?">
        This figure is comparable to 2016 data because the bottom of a grade 4
        in reformed GCSEs maps onto the bottom of a grade C of unreformed GCSEs
      </Details>
    </div>

    <ul className="govuk-list govuk-list--bullet">
      <li>
        the average Attainment 8 score per pupil has decreased in comparison to
        2016 but this change is as expected when compared to 2016 data with the
        2017 point score scale applied{' '}
      </li>
      <li>
        365 schools are below the floor standard in 2017, and 271 meet the
        coasting definition
      </li>
      <li>
        the gap between disadvantaged pupils and others continues to narrow
      </li>
      <li>EBacc entry and achievement have both decreased</li>
      <li>
        percentage achieving the threshold of a grade 4 or above in English and
        maths is stable compared to equivalent 2016 data
      </li>
    </ul>
  </div>
);

export default PrototypeDataTileHighlights;
