import FormattedDate from '@common/components/FormattedDate';
import { PublicationListSummary } from '@common/services/publicationService';
import { releaseTypes } from '@common/services/types/releaseType';
import Link from '@frontend/components/Link';
import styles from '@frontend/modules/home/components/HomeReleaseLink.module.scss';
import React from 'react';

interface Props {
  release: PublicationListSummary;
  onClick?: (label: string) => void;
}

const HomeReleaseLink = ({ release, onClick }: Props) => {
  const { slug, summary, title, published, latestReleaseSlug, theme, type } =
    release;

  return (
    <div className={styles.linkWrapper}>
      <p className="dfe-colour--dark-grey govuk-!-font-size-16 govuk-!-margin-bottom-0">
        <FormattedDate>{published}</FormattedDate>, {releaseTypes[type]}
      </p>
      <h3 className="govuk-heading-m govuk-!-margin-bottom-2 govuk-!-padding-top-0">
        <Link
          to={`/find-statistics/${slug}/${latestReleaseSlug}`}
          onClick={() => onClick?.(title)}
        >
          {title}
        </Link>
      </h3>
      {summary && <p className="govuk-!-margin-bottom-2">{summary}</p>}
      <p className="govuk-caption-m">{theme}</p>
    </div>
  );
};

export default HomeReleaseLink;
