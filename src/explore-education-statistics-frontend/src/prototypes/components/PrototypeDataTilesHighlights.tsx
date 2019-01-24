import React from 'react';
import Details from '../../components/Details';

const PrototypeDataTileHighlights = () => (
  <div className="dfe-dash-tiles dfe-dash-tiles--3-in-row">
    <div className="dfe-dash-tiles__tile">
      <h3 className="govuk-heading-m dfe-dash-tiles__heading">
        Overall absence
        <span className="govuk-caption-m date-range govuk-tag">2016/17</span>
      </h3>
      <div>
        <span className="govuk-heading-xl govuk-!-margin-bottom-2 govuk-caption-increase-negative">
          4.7%
        </span>
        <p className="govuk-body dfe-dash-tiles__tile--hidden">
          <strong className="increase">
            +0.4
            <abbr aria-label="Percentage points" title="Percentage points">
              ppt
            </abbr>
          </strong>
          more than 2015/16
        </p>
      </div>
      <Details summary="What does this mean?">
        Overall absence is the adipisicing elit. Dolorum hic nobis voluptas
        quidem fugiat enim ipsa reprehenderit nulla.
      </Details>
    </div>

    <div className="dfe-dash-tiles__tile">
      <h3 className="govuk-heading-m dfe-dash-tiles__heading">
        Authorised absence
        <span className="govuk-caption-m date-range govuk-tag">2016/17</span>
      </h3>
      <span className="govuk-heading-xl govuk-!-margin-bottom-2 govuk-caption-increase-negative">
        3.4%
      </span>
      <p className="govuk-body dfe-dash-tiles__tile--hidden">
        <strong className="level">
          0
          <abbr aria-label="Percentage points" title="Percentage points">
            ppt
          </abbr>
        </strong>
        the same as 2015/16
      </p>
      <Details summary="What does this mean?">
        Overall absence is the adipisicing elit. Dolorum hic nobis voluptas
        quidem fugiat enim ipsa reprehenderit nulla.
      </Details>
    </div>

    <div className="dfe-dash-tiles__tile">
      <h3 className="govuk-heading-m dfe-dash-tiles__heading">
        Unauthorised absence
        <span className="govuk-caption-m date-range govuk-tag">2016/17</span>
      </h3>
      <span className="govuk-heading-xl govuk-!-margin-bottom-2 govuk-caption-increase-negative">
        1.3%
      </span>
      <p className="govuk-body dfe-dash-tiles__tile--hidden">
        <strong className="decrease">
          -0.4
          <abbr aria-label="Percentage points" title="Percentage points">
            ppt
          </abbr>
        </strong>
        less than 2015/16
      </p>
      <Details summary="What does this mean?">
        Overall absence is the adipisicing elit. Dolorum hic nobis voluptas
        quidem fugiat enim ipsa reprehenderit nulla.
      </Details>
    </div>
  </div>
);

export default PrototypeDataTileHighlights;
