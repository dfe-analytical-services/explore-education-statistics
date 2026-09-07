import FormattedDate from '@common/components/FormattedDate';
import VisuallyHidden from '@common/components/VisuallyHidden';
import { releaseTypes } from '@common/services/types/releaseType';
import Link from '@frontend/components/Link';
import styles from '@frontend/modules/home/components/HomeReleaseLink.module.scss';
import { HomeRelease } from '@frontend/modules/home/data/prototypeHomeReleasesData';
import React from 'react';

interface Props {
  release: HomeRelease;
  onClick?: (label: string) => void;
}

const HomeReleaseLink = ({ release, onClick }: Props) => {
  const {
    publicationSlug,
    publicationSummary,
    publicationTitle,
    publishedDate,
    releaseSlug,
    releaseTitle,
    theme,
    type,
  } = release;

  return (
    <div className={styles.linkWrapper}>
      <p className="dfe-colour--dark-grey govuk-!-font-size-16 govuk-!-margin-bottom-0">
        <FormattedDate>{publishedDate}</FormattedDate>, {releaseTypes[type]}
      </p>
      <h3 className="govuk-heading-m govuk-!-margin-bottom-2 govuk-!-padding-top-0">
        <Link
          to={`/find-statistics/${publicationSlug}/${releaseSlug}`}
          onClick={() => onClick?.(publicationTitle)}
        >
          {publicationTitle}
          <VisuallyHidden>, {releaseTitle}</VisuallyHidden>
        </Link>
      </h3>
      <p className="govuk-!-margin-bottom-2">{publicationSummary}</p>
      <p className="govuk-caption-m">{theme}</p>
    </div>
  );
};

export default HomeReleaseLink;
