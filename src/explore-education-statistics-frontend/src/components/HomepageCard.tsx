import React from 'react';
import { logEvent } from '@frontend/services/googleAnalyticsService';
import ButtonLink from './ButtonLink';
import styles from './HomepageCard.module.scss';

interface Props {
  buttonText: string;
  destination: string;
  text: string;
  title: string;
}

const HomepageCard = ({ buttonText, destination, text, title }: Props) => {
  return (
    <div className="govuk-grid-column-one-third dfe-card__item">
      <div className="dfe-card">
        <h2 className={styles.cardTitle}>{title}</h2>
        <p className="govuk-!-margin-top-2">{text}</p>
        <ButtonLink
          to={destination}
          className="govuk-button--start"
          onClick={() =>
            logEvent({
              category: 'Homepage',
              action: 'Homepage link clicked',
              label: title,
            })
          }
        >
          {buttonText}
          <svg
            className="govuk-button__start-icon"
            xmlns="http://www.w3.org/2000/svg"
            width="17.5"
            height="19"
            viewBox="0 0 33 40"
            aria-hidden="true"
            focusable="false"
          >
            <path fill="currentColor" d="M0 0h13l20 20-20 20H0l20-20z" />
          </svg>
        </ButtonLink>
      </div>
    </div>
  );
};

export default HomepageCard;
