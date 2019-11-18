import Details from '@common/components/Details';
import styles from '@common/modules/find-statistics/components//SummaryRenderer.module.scss';
import React, { useState } from 'react';

interface Props {
  editing?: boolean;
  indicatorOrder?: string;
  indicator?: string;
  indicatorValue?: string;
  trend?: string;
}

const KeyIndicator = ({
  editing,
  indicatorOrder,
  indicator,
  indicatorValue,
  trend,
}: Props) => {
  const [selectedKeyIndicator, setSelectedKeyIndicator] = useState(false);
  const [previewIndicator, setPreviewIndicator] = useState(false);
  const [removeIndicator, setRemoveIndicator] = useState(false);

  return (
    <>
      {!removeIndicator && (
        <div className={styles.keyStatTile}>
          {!editing && (
            <>
              <div className={styles.keyStat}>
                <h3 className="govuk-heading-s">{selectedKeyIndicator}</h3>{' '}
                <p className="govuk-heading-xl govuk-!-margin-bottom-2">
                  {indicatorValue}
                </p>
                <p className="govuk-body-s">{trend}</p>
              </div>
              <Details summary={`What is ${selectedKeyIndicator}?`}>
                {`${selectedKeyIndicator} is ...`}
              </Details>
            </>
          )}
          {editing && previewIndicator && (
            <>
              <button
                type="button"
                className="govuk-button govuk-!-margin-bottom-1"
                onClick={() => setPreviewIndicator(false)}
              >
                Edit key indicator
              </button>

              <div className={styles.keyStat} style={{ flexGrow: 0 }}>
                <h3 className="govuk-heading-s">{selectedKeyIndicator}</h3>{' '}
                <p className="govuk-heading-xl govuk-!-margin-bottom-2">
                  {indicatorValue}
                </p>
                <p className="govuk-body-s">{trend}</p>
              </div>
              <Details summary={`What is ${selectedKeyIndicator}?`}>
                {`${selectedKeyIndicator} ...`}
              </Details>
            </>
          )}
          {editing && !previewIndicator && (
            <>
              <form>
                <legend className="govuk-heading-s">
                  Key indicator {indicatorOrder}
                </legend>
                <div className={styles.keyStat}>
                  <label htmlFor={`data-keystat-${indicatorOrder}`}>
                    Select data
                  </label>
                  <select
                    className="govuk-select govuk-!-margin-bottom-2 govuk-!-width-full"
                    name={`data-keystat-${indicatorOrder}`}
                    id={`data-keystat-${indicatorOrder}`}
                    onBlur={e => {
                      setSelectedKeyIndicator(e.target.value);
                    }}
                  >
                    <option value={indicator} selected>
                      {`key-${indicator}`}
                    </option>
                    <option value="Authorised absence">
                      Key - Authorised absence
                    </option>
                    <option value="Unauthorised absence">
                      Key - Unauthorised abence
                    </option>
                    <option value="Overall absence">
                      Key - Overall absence
                    </option>
                  </select>

                  {selectedKeyIndicator && (
                    <>
                      <h3 className="govuk-heading-s">
                        {selectedKeyIndicator}
                      </h3>

                      <p className="govuk-heading-xl">{indicatorValue}</p>

                      <label htmlFor={`trend-1-${indicatorOrder}`}>Trend</label>
                      <input
                        type="text"
                        className="govuk-!-margin-bottom-3 govuk-input govuk-!-width-full"
                        placeholder={trend}
                        id={`trend-1-${indicatorOrder}`}
                        name={`trend-1-${indicatorOrder}`}
                      />
                    </>
                  )}
                </div>
                {selectedKeyIndicator && (
                  <>
                    <Details summary="Guidance text" open>
                      <label htmlFor={`help-title-${indicatorOrder}`}>
                        Guidance title
                      </label>
                      <input
                        type="text"
                        className="govuk-!-margin-bottom-2 govuk-input govuk-!-width-full"
                        placeholder={`What is ${selectedKeyIndicator}`}
                        id={`help-title-${indicatorOrder}`}
                        name={`help-title-${indicatorOrder}`}
                      />
                      <label htmlFor={`help-text-1${indicatorOrder}`}>
                        Guidance text
                      </label>
                      <textarea
                        id={`help-text-1${indicatorOrder}`}
                        className="govuk-!-margin-bottom-2 govuk-body-s govuk-textarea govuk-!-width-full"
                        rows={5}
                        placeholder={`${selectedKeyIndicator} ...`}
                      />
                    </Details>
                    <button
                      type="button"
                      className="govuk-button govuk-!-margin-top-3 govuk-!-margin-right-3"
                      onClick={() => setPreviewIndicator(true)}
                    >
                      Save
                    </button>
                    <button
                      type="button"
                      className="govuk-button govuk-button--secondary govuk-!-margin-top-3"
                      onClick={() => setRemoveIndicator(true)}
                    >
                      Remove
                    </button>
                  </>
                )}
              </form>
            </>
          )}
        </div>
      )}
    </>
  );
};

const PrototypeDataTileHighlights = ({ editing }: Props) => {
  const [addKeyIndicator1, setAddKeyIndicator1] = useState(false);
  const [addKeyIndicator2, setAddKeyIndicator2] = useState(false);
  const [addKeyIndicator3, setAddKeyIndicator3] = useState(false);

  return (
    <>
      <div className={styles.keyStatsContainer}>
        {addKeyIndicator1 && (
          <KeyIndicator
            editing={editing}
            indicatorOrder="1"
            indicatorValue="4.7%"
            trend="Up from 4.7% in 2015/16"
          />
        )}
        {addKeyIndicator2 && (
          <KeyIndicator
            editing={editing}
            indicatorOrder="2"
            indicatorValue="3.4%"
            trend="Similar to previous years"
          />
        )}
        {addKeyIndicator3 && (
          <KeyIndicator
            editing={editing}
            indicatorOrder="3"
            indicatorValue="1.3%"
            trend="Up from 1.1% in 2015/16"
            remove="tester"
          />
        )}
      </div>

      {editing && (
        <>
          {!addKeyIndicator1 && !addKeyIndicator2 && !addKeyIndicator3 && (
            <button
              type="button"
              className="govuk-button"
              onClick={() => setAddKeyIndicator1(true)}
            >
              Add a key indicator
            </button>
          )}
          {addKeyIndicator1 && !addKeyIndicator2 && !addKeyIndicator3 && (
            <button
              type="button"
              className="govuk-button"
              onClick={() => setAddKeyIndicator2(true)}
            >
              Add another key indicator
            </button>
          )}
          {addKeyIndicator1 && addKeyIndicator2 && !addKeyIndicator3 && (
            <button
              type="button"
              className="govuk-button"
              onClick={() => setAddKeyIndicator3(true)}
            >
              Add another key indicator
            </button>
          )}
          {addKeyIndicator1 && addKeyIndicator2 && addKeyIndicator3 && (
            <button
              type="button"
              className="govuk-button"
              onClick={() => setAddKeyIndicator3(true)}
            >
              Add another key indicator
            </button>
          )}
        </>
      )}
    </>
  );
};

export default PrototypeDataTileHighlights;
