import React from 'react';
import Details from '../../components/Details';

const PrototypeDataTileHighlights = () => (
  <div className="dfe-dash-tiles dfe-dash-tiles--4-in-row">
    <div className="dfe-dash-tiles__tile">
      <h3 className="govuk-heading-s dfe-dash-tiles__heading">Attainment8</h3>
      <p className="govuk-heading-l govuk-!-margin-bottom-2">44.5</p>
      <Details summary="What is Attainment8?">
        Attainment8 measures the average achievement of pupils in up to 8
        qualifications (including English and Maths).
      </Details>
    </div>
    <div className="dfe-dash-tiles__tile">
      <h3 className="govuk-heading-s dfe-dash-tiles__heading">
        English / Maths 5+
      </h3>
      <p className="govuk-heading-l govuk-!-margin-bottom-2">43.3%</p>
      <Details summary="What is this?">
        measures the percentage of pupils achieving a grade 5 or above in both
        English and maths.
      </Details>
    </div>
    <div className="dfe-dash-tiles__tile">
      <h3 className="govuk-heading-s dfe-dash-tiles__heading">EBacc entries</h3>
      <p className="govuk-heading-l govuk-!-margin-bottom-2">38.4%</p>
      <Details summary="What are EBacc entries?">
        EBacc entries measure the percentage of pupils reaching the English
        Baccalaureate (EBacc) attainment threshold in core academic subjects at
        key stage 4. The EBacc is made up of English, maths, science, a
        language, and history or geography..
      </Details>
    </div>
    <div className="dfe-dash-tiles__tile">
      <h3 className="govuk-heading-s dfe-dash-tiles__heading">EBacc APS</h3>
      <p className="govuk-heading-l govuk-!-margin-bottom-2">4.04</p>
      <Details summary="What is Ebacc APS?">
        EBacc Average Point Score (APS) – measures pupils’ point scores across
        the five pillars of the EBacc, ensuring the attainment of all pupils is
        recognised. New measure from 2018, replacing the previous threshold
        EBacc attainment measure.
      </Details>
    </div>

    <ul className="govuk-list govuk-list--bullet">
      <li>average Attainment8 scores remained stable compared to 2017</li>
      <li>
        percentage of pupils achieving 5 or above in English and Maths increased
      </li>
      <li>EBacc entry increased slightly</li>
      <li>over 250 schools met the coasting definition in 2018</li>
    </ul>
  </div>
);

export default PrototypeDataTileHighlights;
