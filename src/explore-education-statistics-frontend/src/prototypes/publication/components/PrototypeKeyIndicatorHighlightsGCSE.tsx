import Details from '@common/components/Details';
import React from 'react';
import styles from './PrototypeKeyIndicator.module.scss';

const PrototypeDataTileHighlights = () => (
  <>
    <div className={styles.keyStatsContainer}>
      <div className={styles.keyStatTile}>
        <h3 className={styles.keyStatHeading}>Attainment 8</h3>
        <p className={styles.keyStatLarge}>44.5</p>
        <Details summary="What is Attainment 8?">
          <p className="govuk-body-s">
            Attainment 8 measures the average achievement of pupils in up to 8
            qualifications (including English and Maths).
          </p>
        </Details>
      </div>
      <div className={styles.keyStatTile}>
        <h3 className={styles.keyStatHeading}>Progress 8</h3>
        <p className={styles.keyStatLarge}>1.0</p>
        <Details summary="What is Progress 8?">
          <p className="govuk-body-s">
            Progress 8 Lorem ipsum dolor sit amet consectetur adipisicing elit.
            Unde voluptates, eaque aliquid quos corporis tempora voluptate eum,
            est ea quidem ex omnis labore consequatur doloremque molestiae nulla
            deleniti ad autem?
          </p>
        </Details>
      </div>
      <div className={styles.keyStatTile}>
        <h3 className={styles.keyStatHeading}>English / Maths 5+</h3>
        <p className={styles.keyStatLarge}>43.3%</p>
        <Details summary="What is this?">
          <p className="govuk-body-s">
            measures the percentage of pupils achieving a grade 5 or above in
            both English and maths.
          </p>
        </Details>
      </div>
      <div className={styles.keyStatTile}>
        <h3 className={styles.keyStatHeading}>EBacc entries</h3>
        <p className={styles.keyStatLarge}>38.4%</p>
        <Details summary="What are EBacc entries?">
          <p className="govuk-body-s">
            EBacc entries measure the percentage of pupils reaching the English
            Baccalaureate (EBacc) attainment threshold in core academic subjects
            at key stage 4. The EBacc is made up of English, maths, science, a
            language, and history or geography.
          </p>
        </Details>
      </div>
      <div className={styles.keyStatTile}>
        <h3 className={styles.keyStatHeading}>EBacc APS</h3>
        <p className={styles.keyStatLarge}>4.04</p>
        <Details summary="What is Ebacc APS?">
          <p className="govuk-body-s">
            EBacc Average Point Score (APS) – measures pupils’ point scores
            across the five pillars of the EBacc, ensuring the attainment of all
            pupils is recognised. New measure from 2018, replacing the previous
            threshold EBacc attainment measure.
          </p>
        </Details>
      </div>
      <div className={styles.keyStatTile}>
        <h3 className={styles.keyStatHeading}>Schools below floor standard</h3>
        <p className={styles.keyStatLarge}>365</p>
        <Details summary="What is the floor standard?">
          <p className="govuk-body-s">
            The floor standard - Lorem ipsum dolor sit amet consectetur
            adipisicing elit. Soluta dignissimos neque recusandae fuga fugit
            asperiores error aperiam libero quod, incidunt voluptatem, nulla est
            quos pariatur officia temporibus enim ipsa quasi?
          </p>
        </Details>
      </div>
    </div>
    <ul className="govuk-list govuk-list--bullet">
      <li>average Attainment8 scores remained stable compared to 2017</li>
      <li>
        percentage of pupils achieving 5 or above in English and Maths increased
      </li>
      <li>EBacc entry increased slightly</li>
      <li>over 250 schools met the coasting definition in 2018</li>
    </ul>
  </>
);

export default PrototypeDataTileHighlights;
