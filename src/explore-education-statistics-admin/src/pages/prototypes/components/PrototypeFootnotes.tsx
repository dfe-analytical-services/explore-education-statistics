import classNames from 'classnames';
import React, { useState } from 'react';
import Link from '@admin/components/Link';
import Details from '@common/components/Details';

const PrototypeFootnotes = () => {
  const [valueIndicator, setValueIndicator] = useState('');
  const [valueFilter, setValueFilter] = useState('');
  const [valueFootnote, setValueFootnote] = useState('');
  const [addIndicatorBlock, setIndicatorBlock] = useState(true);
  const [addFilterBlock, setFilterBlock] = useState(true);
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
            <Details summary="Indicator" tag="3 selected">
              <div className="dfe-filter-overflow">
                <img
                  src="/static/images/prototype/indicator-filters.png"
                  alt=""
                />
              </div>
            </Details>
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
        <div className="govuk-grid-column-one-third">
          <h3 className="govuk-heading-s">Footnote</h3>
          {!editFootnoteBlock && (
            <>
              <textarea
                className="govuk-textarea govuk-!-margin-bottom-3"
                name="footnote-1"
                rows={5}
                value={valueFootnote}
                onChange={event => {
                  setValueFootnote(event.target.value);
                }}
              >
                test
              </textarea>
              <button
                className="govuk-button"
                type="submit"
                onClick={() => setEditFootnoteBlock(true)}
              >
                Save
              </button>
            </>
          )}
          {editFootnoteBlock && (
            <div>
              <div>
                <p>{valueFootnote}</p>
              </div>
              <button
                className="govuk-button govuk-!-margin-right-3"
                type="submit"
                onClick={() => setEditFootnoteBlock(false)}
              >
                Edit
              </button>
              <button
                className="govuk-button govuk-button--secondary"
                type="submit"
              >
                Delete footnote
              </button>
            </div>
          )}
        </div>
      </div>
    </>
  );
};

export default PrototypeFootnotes;
