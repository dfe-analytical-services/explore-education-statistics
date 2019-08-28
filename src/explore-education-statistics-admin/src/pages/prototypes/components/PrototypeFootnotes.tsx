import classNames from 'classnames';
import React, { useState } from 'react';
import Link from '@admin/components/Link';
import Details from '@common/components/Details';

interface Props {
  subject?: string;
  titleRow?: boolean;
}

const PrototypeFootnotes = ({ subject, titleRow }: Props) => {
  const [valueIndicator, setValueIndicator] = useState('');
  const [valueFilter, setValueFilter] = useState('');
  const [valueFootnote, setValueFootnote] = useState('');
  const [addIndicatorBlock, setIndicatorBlock] = useState(false);
  const [addFilterBlock, setFilterBlock] = useState(false);
  const [editFootnoteBlock, setEditFootnoteBlock] = useState(false);

  return (
    <>
      {titleRow && (
        <>
          <div className="govuk-grid-row govuk-heading-s govuk-!-margin-bottom-0">
            <div className="govuk-grid-column-one-third">Subject</div>
            <div className="govuk-grid-column-one-third">Indicator</div>
            <div className="govuk-grid-column-one-third">Filter</div>
          </div>
          <hr className="govuk-!-margin-top-1 govuk-!-margin-bottom-2" />
        </>
      )}
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
                  defaultChecked
                  onChange={event => {
                    setValueIndicator(event.target.value);
                    setEditFootnoteBlock(false);
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
          {!addIndicatorBlock && (
            <div className="govuk-form-group govuk-!-margin-bottom-2">
              <div className="govuk-checkboxes govuk-checkboxes--small">
                <div className="govuk-checkboxes__item">
                  <input
                    className="govuk-checkboxes__input"
                    type="checkbox"
                    id="selectAllIndicators"
                    name="selectAllIndicators"
                    value="selectAllIndicators"
                    defaultChecked
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
          )}
          {addIndicatorBlock && (
            <>
              <span className="govuk-hint govuk-!-margin-top-2">
                Select at least one indicator
              </span>
              <Details
                summary="Indicator"
                className="govuk-!-margin-bottom-1"
                tag="3 selected"
              >
                <div className="dfe-filter-overflow">
                  <img
                    src="/static/images/prototype/indicator-filters.png"
                    alt=""
                  />
                </div>
              </Details>
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
            </>
          )}
        </div>
        <div className="govuk-grid-column-one-third">
          {!addFilterBlock && (
            <div className="govuk-form-group govuk-!-margin-bottom-2">
              <div className="govuk-checkboxes govuk-checkboxes--small">
                <div className="govuk-checkboxes__item">
                  <input
                    className="govuk-checkboxes__input"
                    type="checkbox"
                    id="selectAllFilters"
                    name="selectAllFilters"
                    value="selectAllFilters"
                    defaultChecked
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
          )}
          {addFilterBlock && (
            <>
              <span className="govuk-hint govuk-!-margin-top-2">
                Select at least one filter
              </span>
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
            </>
          )}
        </div>
      </div>
      <hr className="govuk-!-margin-0 govuk-!-margin-bottom-2" />
    </>
  );
};

export default PrototypeFootnotes;
