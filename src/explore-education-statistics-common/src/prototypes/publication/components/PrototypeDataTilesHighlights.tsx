import Details from '@common/components/Details';
import styles from '@common/modules/find-statistics/components//SummaryRenderer.module.scss';
import React from 'react';

const PrototypeDataTileHighlights = () => (
  <div className={styles.keyStatsContainer}>
    <div className={styles.keyStatTile}>
      <div className={styles.keyStat}>
        <h3 className="govuk-heading-s">Overall absence</h3>
        <p className="govuk-heading-xl govuk-!-margin-bottom-2">4.7%</p>
      </div>
      <Details summary="What is overall absence?">
        Overall absence is the adipisicing elit. Dolorum hic nobis voluptas
        quidem fugiat enim ipsa reprehenderit nulla.
      </Details>
    </div>

    <div className={styles.keyStatTile}>
      <div className={styles.keyStat}>
        <h3 className="govuk-heading-s">Authorised absence</h3>
        <p className="govuk-heading-xl govuk-!-margin-bottom-2">3.4%</p>
      </div>
      <Details summary="What is authorised absence?">
        Authorised absence is the adipisicing elit. Dolorum hic nobis voluptas
        quidem fugiat enim ipsa reprehenderit nulla.
      </Details>
    </div>

    <div className={styles.keyStatTile}>
      <div className={styles.keyStat}>
        <h3 className="govuk-heading-s">Unauthorised absence</h3>
        <p className="govuk-heading-xl govuk-!-margin-bottom-2">1.3%</p>
      </div>
      <Details summary="What is unauthorised absence?">
        Unauthorised absence is the adipisicing elit. Dolorum hic nobis voluptas
        quidem fugiat enim ipsa reprehenderit nulla.
      </Details>
    </div>
  </div>
);

export default PrototypeDataTileHighlights;
