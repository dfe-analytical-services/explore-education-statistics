import React from 'react';
import Details from '../../components/Details';

const PrototypeDataTileHighlights = () => (
  <div className="dfe-dash-tiles dfe-dash-tiles--3-in-row">
    <div className="dfe-dash-tiles__tile">
      <h3 className="govuk-heading-m dfe-dash-tiles__heading">
        Overall absence
      </h3>
      <div>
        <span className="govuk-heading-xl govuk-!-margin-bottom-2">4.7%</span>
      </div>
      <Details summary="What is overall absence?">
        Overall absence is the adipisicing elit. Dolorum hic nobis voluptas
        quidem fugiat enim ipsa reprehenderit nulla.
      </Details>
    </div>

    <div className="dfe-dash-tiles__tile">
      <h3 className="govuk-heading-m dfe-dash-tiles__heading">
        Authorised absence
      </h3>
      <span className="govuk-heading-xl govuk-!-margin-bottom-2">3.4%</span>
      <Details summary="What is authorised absence?">
        Authorised absence is the adipisicing elit. Dolorum hic nobis voluptas
        quidem fugiat enim ipsa reprehenderit nulla.
      </Details>
    </div>

    <div className="dfe-dash-tiles__tile">
      <h3 className="govuk-heading-m dfe-dash-tiles__heading">
        Unauthorised absence
      </h3>
      <span className="govuk-heading-xl govuk-!-margin-bottom-2">1.3%</span>
      <Details summary="What is unauthorised absence?">
        Unauthorised absence is the adipisicing elit. Dolorum hic nobis voluptas
        quidem fugiat enim ipsa reprehenderit nulla.
      </Details>
    </div>
  </div>
);

export default PrototypeDataTileHighlights;
