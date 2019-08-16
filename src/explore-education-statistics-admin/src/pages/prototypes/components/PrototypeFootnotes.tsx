import classNames from 'classnames';
import React, { useState } from 'react';
import Link from '@admin/components/Link';
import Details from '@common/components/Details';

interface Props {
  subject?: string;
}

const PrototypeFootnotes = ({ subject }: Props) => {
  const [valueIndicator, setValueIndicator] = useState('');
  const [valueFilter, setValueFilter] = useState('');
  const [valueFootnote, setValueFootnote] = useState('');
  const [addIndicatorBlock, setIndicatorBlock] = useState(false);
  const [addFilterBlock, setFilterBlock] = useState(false);
  const [editFootnoteBlock, setEditFootnoteBlock] = useState(false);

  return (
    <>
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-one-third">
          <div className="govuk-form-group govuk-!-margin-bottom-2">
            <div className="govuk-checkboxes govuk-checkboxes--small">
              <div className="govuk-checkboxes__item">
                <input
                  className="govuk-checkboxes__input"
                  type="checkbox"
                  id="selectSubject"
                  name="selectSubject"
                  value="selectSubject"
                  onChange={event => {
                    setValueIndicator(event.target.value);
                    setEditFootnoteBlock(false);

                    if (addIndicatorBlock) {
                      setIndicatorBlock(false);
                    } else {
                      setIndicatorBlock(true);
                    }
                    if (addFilterBlock) {
                      setFilterBlock(false);
                    } else {
                      setFilterBlock(true);
                    }
                  }}
                />
                <label
                  className="govuk-label govuk-checkboxes__label"
                  htmlFor="selectSubject"
                >
                  {subject}
                </label>
              </div>
            </div>
          </div>
        </div>
        <div className="govuk-grid-column-one-third">
          <div className="govuk-form-group govuk-!-margin-bottom-2">
            <div className="govuk-checkboxes govuk-checkboxes--small">
              <div className="govuk-checkboxes__item">
                <input
                  className="govuk-checkboxes__input"
                  type="checkbox"
                  id="selectAllIndicators"
                  name="selectAllIndicators"
                  value="selectAllIndicators"
                  onChange={event => {
                    setValueIndicator(event.target.value);
                    setEditFootnoteBlock(false);

                    if (addIndicatorBlock) {
                      setIndicatorBlock(false);
                    } else {
                      setIndicatorBlock(true);
                    }
                  }}
                />
                <label
                  className="govuk-label govuk-checkboxes__label"
                  htmlFor="selectAllIndicators"
                >
                  Select all indicators
                </label>
              </div>
            </div>
          </div>
          {addIndicatorBlock && (
            <>
              <Details summary="Indicator" tag="3 selected">
                <div className="dfe-filter-overflow">
                  <img
                    src="/static/images/prototype/indicator-filters.png"
                    alt=""
                  />
                </div>
              </Details>
            </>
          )}
        </div>
        <div className="govuk-grid-column-one-third">
          <div className="govuk-form-group govuk-!-margin-bottom-2">
            <div className="govuk-checkboxes govuk-checkboxes--small">
              <div className="govuk-checkboxes__item">
                <input
                  className="govuk-checkboxes__input"
                  type="checkbox"
                  id="selectAllFilters"
                  name="selectAllFilters"
                  value="selectAllFilters"
                  onChange={event => {
                    setValueIndicator(event.target.value);
                    setEditFootnoteBlock(false);

                    if (addFilterBlock) {
                      setFilterBlock(false);
                    } else {
                      setFilterBlock(true);
                    }
                  }}
                />
                <label
                  className="govuk-label govuk-checkboxes__label"
                  htmlFor="selectAllFilters"
                >
                  Select all filters
                </label>
              </div>
            </div>
          </div>
          {addFilterBlock && (
            <>
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
                  <img
                    src="/static/images/prototype/school-filter.png"
                    alt=""
                  />
                </div>
              </Details>
            </>
          )}
        </div>
      </div>
      <hr className="govuk-!-margin-0 govuk-!-margin-bottom-2" />
    </>
  );
};

export default PrototypeFootnotes;
