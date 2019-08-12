import classNames from 'classnames';
import React from 'react';
import Link from '@admin/components/Link';
import Details from '@common/components/Details';

const PrototypeFootnotes = () => {
  return (
    <div className="govuk-grid-row">
      <div className="govuk-grid-column-one-third">
        <h3 className="govuk-heading-s">Indicators</h3>
        <Details summary="Indicator" tag="3 selected">
          <div className="dfe-filter-overflow">
            <img src="/static/images/prototype/indicator-filters.png" alt="" />
          </div>
        </Details>
      </div>
      <div className="govuk-grid-column-one-third">
        <h3 className="govuk-heading-s">Filters</h3>
        <Details
          summary="Characteristic"
          tag="1 selected"
          className="govuk-!-margin-bottom-2"
        >
          <div className="dfe-filter-overflow">
            <img
              src="/static/images/prototype/characteristic-filter.png"
              alt=""
            />
          </div>
        </Details>
        <Details summary="School type" tag="1 selected">
          <div className="dfe-filter-overflow">
            <img src="/static/images/prototype/school-filter.png" alt="" />
          </div>
        </Details>
      </div>
      <div className="govuk-grid-column-one-third">
        Footnote
        <textarea
          className="govuk-textarea govuk-!-margin-bottom-3"
          name="footnote-1"
          rows={5}
        >
          The number of sessions missed in each band expressed as a percentage
          of the total number of sessions missed for that category of absence
          overall.
        </textarea>
        <div>
          <button className="govuk-button" type="submit">
            Save
          </button>
        </div>
      </div>
    </div>
  );
};

export default PrototypeFootnotes;
