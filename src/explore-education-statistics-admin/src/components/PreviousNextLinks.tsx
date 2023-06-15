import React from 'react';
import Link from '@admin/components/Link';
import styles from './PreviousNextLinks.module.scss';

export interface PreviousNextLink {
  label: string;
  linkTo: string;
}

export interface Props {
  previousSection?: PreviousNextLink;
  nextSection?: PreviousNextLink;
}

/**
 * This component represents Previous / Next links for traversing between linked pages or sections, however the
 * client needs to use it.
 *
 * @param previousSection
 * @param nextSection
 * @constructor
 */
const PreviousNextLinks = ({ previousSection, nextSection }: Props) => {
  if (previousSection && nextSection) {
    return (
      <div className="govuk-grid-row govuk-!-margin-top-9">
        <div className="govuk-grid-column-one-half ">
          <Link to={previousSection.linkTo} className={styles.nextPrevious}>
            <span className="govuk-heading-m govuk-!-margin-bottom-0">
              Previous step
            </span>
            <span className={styles.nextPreviousSmall}>
              {previousSection.label}
            </span>
          </Link>
        </div>
        <div className="govuk-grid-column-one-half dfe-align--right">
          <Link to={nextSection.linkTo} className={styles.nextPrevious}>
            <span className="govuk-heading-m govuk-!-margin-bottom-0">
              Next step
            </span>{' '}
            <span className={styles.nextPreviousSmall}>
              {nextSection.label}
            </span>
          </Link>
        </div>
      </div>
    );
  }

  if (previousSection) {
    return (
      <div className="govuk-!-margin-top-9">
        <Link to={previousSection.linkTo} className={styles.nextPrevious}>
          <span className="govuk-heading-m govuk-!-margin-bottom-0">
            Previous step
          </span>
          <span className={styles.nextPreviousSmall}>
            {previousSection.label}
          </span>
        </Link>
      </div>
    );
  }

  if (nextSection) {
    return (
      <div className="govuk-!-margin-top-9 dfe-align--right">
        <Link to={nextSection.linkTo} className={styles.nextPrevious}>
          <span className="govuk-heading-m govuk-!-margin-bottom-0">
            Next step
          </span>{' '}
          <span className={styles.nextPreviousSmall}>{nextSection.label}</span>
        </Link>
      </div>
    );
  }

  return null;
};

export default PreviousNextLinks;
