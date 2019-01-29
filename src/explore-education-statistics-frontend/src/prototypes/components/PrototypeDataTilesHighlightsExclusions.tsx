import React from 'react';
import Details from '../../components/Details';

const PrototypeDataTileHighlights = () => (
  <div className="dfe-dash-tiles dfe-dash-tiles--3-in-row">
    <div className="dfe-dash-tiles__tile">
      <h3 className="govuk-heading-m dfe-dash-tiles__heading">
        Overall permanent exclusions
      </h3>
      <div>
        <span className="govuk-heading-xl govuk-!-margin-bottom-2 govuk-caption-increase-positive">
          0.10%
        </span>
      </div>
      <Details summary="What does this mean?">
        Overall permanent exclusions is the adipisicing elit. Dolorum hic nobis
        voluptas quidem fugiat enim ipsa reprehenderit nulla.
      </Details>
    </div>

    <div className="dfe-dash-tiles__tile">
      <h3 className="govuk-heading-m dfe-dash-tiles__heading">
        Number of exclusions
      </h3>
      <span className="govuk-heading-xl govuk-!-margin-bottom-2 govuk-caption-increase-negative">
        7,720
      </span>
      <Details summary="What does this mean?">
        Number of exclusions is the adipisicing elit. Dolorum hic nobis voluptas
        quidem fugiat enim ipsa reprehenderit nulla.
      </Details>
    </div>

    <div className="dfe-dash-tiles__tile">
      <h3 className="govuk-heading-m dfe-dash-tiles__heading">
        Overall rate of fixed-period exclusions
      </h3>
      <span className="govuk-heading-xl govuk-!-margin-bottom-2 govuk-caption-increase-negative">
        4.76%
      </span>
      <Details summary="What does this mean?">
        Unauthorised absence is the adipisicing elit. Dolorum hic nobis voluptas
        quidem fugiat enim ipsa reprehenderit nulla.
      </Details>
    </div>
  </div>
);

export default PrototypeDataTileHighlights;
