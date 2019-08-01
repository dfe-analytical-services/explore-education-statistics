import Details from '@common/components/Details';
import styles from '@common/modules/find-statistics/components//SummaryRenderer.module.scss';
import React from 'react';

interface Props {
  editing?: boolean;
}

const PrototypeDataTileHighlights = ({ editing }: Props) => (
  <div className={styles.keyStatsContainer}>
    <div className={styles.keyStatTile}>
      <div className={styles.keyStat}>
        {!editing && <h3 className="govuk-heading-s">Overall absence</h3>}
        {editing && (
          <input
            type="text"
            className="govuk-input govuk-!-width-full"
            placeholder="Key indicator"
            value="Overall absence"
            id="key-indicator-1"
            name="key-indicator-1"
          />
        )}
        {!editing && (
          <p className="govuk-heading-xl govuk-!-margin-bottom-2">4.7%</p>
        )}
        {editing && (
          <input
            type="text"
            className="govuk-!-margin-top-3 govuk-input govuk-!-width-full"
            value="4.7%"
            id="value-1"
            name="value-1"
          />
        )}
        {!editing && <p className="govuk-body-s">Up from 4.6% in 2015/16</p>}
        {editing && (
          <input
            type="text"
            className="govuk-!-margin-top-3 govuk-input govuk-!-width-full"
            value="Up from 4.6% in 2015/16"
            id="trend-1"
            name="trend-1"
          />
        )}
      </div>
      <Details summary="What is overall absence?">
        Overall absence is the adipisicing elit. Dolorum hic nobis voluptas
        quidem fugiat enim ipsa reprehenderit nulla.
      </Details>
    </div>

    <div className={styles.keyStatTile}>
      <div className={styles.keyStat}>
        {!editing && (
          <h3 className="govuk-heading-s">Authorised absence rate</h3>
        )}
        {editing && (
          <input
            type="text"
            className="govuk-input govuk-!-width-full"
            placeholder="Key indicator"
            value="Authorised absence rate"
            id="key-indicator-2"
            name="key-indicator-2"
          />
        )}
        {!editing && (
          <p className="govuk-heading-xl govuk-!-margin-bottom-2">3.4%</p>
        )}
        {editing && (
          <input
            type="text"
            className="govuk-!-margin-top-3 govuk-input govuk-!-width-full"
            value="3.4%"
            id="value-2"
            name="value-2"
          />
        )}
        {!editing && <p className="govuk-body-s">Similar to previous years</p>}
        {editing && (
          <input
            type="text"
            className="govuk-!-margin-top-3 govuk-input govuk-!-width-full"
            value="Similar to previous years"
            id="trend-2"
            name="trend-2"
          />
        )}
      </div>
      <Details summary="What is authorised absence?">
        Authorised absence is the adipisicing elit. Dolorum hic nobis voluptas
        quidem fugiat enim ipsa reprehenderit nulla.
      </Details>
    </div>

    <div className={styles.keyStatTile}>
      <div className={styles.keyStat}>
        {!editing && (
          <h3 className="govuk-heading-s">Unauthorised absence rate</h3>
        )}
        {editing && (
          <input
            type="text"
            className="govuk-input govuk-!-width-full"
            placeholder="Key indicator"
            value="Unauthorised absence rate"
            id="key-indicator-3"
            name="key-indicator-3"
          />
        )}
        {!editing && (
          <p className="govuk-heading-xl govuk-!-margin-bottom-2">1.3%</p>
        )}
        {editing && (
          <input
            type="text"
            className="govuk-!-margin-top-3 govuk-input govuk-!-width-full"
            value="1.3%"
            id="value-3"
            name="value-3"
          />
        )}
        {!editing && <p className="govuk-body-s">Up from 1.1% in 2015/16</p>}
        {editing && (
          <input
            type="text"
            className="govuk-!-margin-top-3 govuk-input govuk-!-width-full"
            value="Up from 1.1% in 2015/16"
            id="trend-3"
            name="trend-3"
          />
        )}
      </div>
      <Details summary="What is unauthorised absence?">
        Unauthorised absence is the adipisicing elit. Dolorum hic nobis voluptas
        quidem fugiat enim ipsa reprehenderit nulla.
      </Details>
    </div>
  </div>
);

export default PrototypeDataTileHighlights;
