import React from 'react';
import styles from './InfoIcon.module.scss';

interface Props {
  description: string;
}

const InfoIcon = ({ description }: Props) => {
  return (
    <>
      <span className={styles.infoIcon} aria-hidden>
        ?
      </span>
      <span className="govuk-visually-hidden">{description}</span>
    </>
  );
};

export default InfoIcon;
