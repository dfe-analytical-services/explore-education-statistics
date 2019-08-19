import Details from '@common/components/Details';
import styles from '@common/modules/find-statistics/components//SummaryRenderer.module.scss';
import React from 'react';

interface Props {
  editing?: boolean;
  indicatorOrder?: string;
  indicator?: string;
  indicatorValue?: string;
  trend?: string;
  guidanceTitle?: string;
  guidanceText?: string;
}

const KeyIndicator = ({
  editing,
  indicatorOrder,
  indicator,
  indicatorValue,
  trend,
  guidanceTitle,
  guidanceText,
}: Props) => {
  return (
    <div className={styles.keyStatTile}>
      {!editing && (
        <>
          <div className={styles.keyStat}>
            <h3 className="govuk-heading-s">{indicator}</h3>{' '}
            <p className="govuk-heading-xl govuk-!-margin-bottom-2">
              {indicatorValue}
            </p>
            <p className="govuk-body-s">{trend}</p>
          </div>
          <Details summary={guidanceTitle}>{guidanceText}</Details>
        </>
      )}
      {editing && (
        <>
          <form>
            <legend className="govuk-heading-s">
              Key indicator {indicatorOrder}
            </legend>
            <div className={styles.keyStat}>
              <label htmlFor={`key-indicator-${indicatorOrder}`}>
                Indicator
              </label>
              <input
                type="text"
                className="govuk-!-margin-bottom-2 govuk-input govuk-!-width-full"
                placeholder={indicator}
                name={`key-indicator-${indicatorOrder}`}
                id={`key-indicator-${indicatorOrder}`}
              />
              <label htmlFor={`value-1-${indicatorOrder}`}>Value</label>
              <input
                type="text"
                className="govuk-!-margin-bottom-2 govuk-input"
                placeholder={indicatorValue}
                id={`value-1-${indicatorOrder}`}
                name={`value-1-${indicatorOrder}`}
              />
              <label htmlFor={`trend-1-${indicatorOrder}`}>Trend</label>
              <input
                type="text"
                className="govuk-!-margin-bottom-3 govuk-input govuk-!-width-full"
                placeholder={trend}
                id={`trend-1-${indicatorOrder}`}
                name={`trend-1-${indicatorOrder}`}
              />
            </div>
            <Details summary="Guidance text" open>
              <label htmlFor={`help-title-${indicatorOrder}`}>
                Guidance title
              </label>
              <input
                type="text"
                className="govuk-!-margin-bottom-2 govuk-input govuk-!-width-full"
                placeholder={guidanceTitle}
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
                placeholder={`${guidanceText} is the adipisicing elit. Dolorum hic nobis voluptas quidem fugiat enim ipsa reprehenderit nulla.`}
              />
            </Details>
            <button
              type="button"
              className="govuk-button govuk-!-margin-top-3 govuk-!-margin-right-3"
            >
              Save
            </button>
            <button
              type="button"
              className="govuk-button govuk-button--secondary govuk-!-margin-top-3"
            >
              Remove
            </button>
          </form>
        </>
      )}
    </div>
  );
};

const PrototypeDataTileHighlights = ({ editing }: Props) => (
  <div className={styles.keyStatsContainer}>
    <KeyIndicator
      editing={editing}
      indicatorOrder="1"
      indicator="Overall absence"
      indicatorValue="4.7%"
      trend="Up from 4.7% in 2015/16"
      guidanceTitle="What is overall absence?"
      guidanceText="Overall absence is"
    />
    <KeyIndicator
      editing={editing}
      indicatorOrder="2"
      indicator="Authorised absence rate"
      indicatorValue="3.4%"
      trend="Similar to previous years"
      guidanceTitle="What is authorised absence?"
      guidanceText="Authorised absence is"
    />
    <KeyIndicator
      editing={editing}
      indicatorOrder="3"
      indicator="Unauthorised absence rate"
      indicatorValue="1.3%"
      trend="Up from 1.1% in 2015/16"
      guidanceTitle="What is unauthorised absence?"
      guidanceText="Unauthorised absence is"
    />
  </div>
);

export default PrototypeDataTileHighlights;
