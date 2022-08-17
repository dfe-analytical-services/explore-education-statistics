import FormattedDate from '@common/components/FormattedDate';
import VisuallyHidden from '@common/components/VisuallyHidden';
import { PublicationSummaryWithRelease } from '@common/services/publicationService';
import { releaseTypes } from '@common/services/types/releaseType';
import styles from '@frontend/modules/find-statistics/components/PublicationSummary.module.scss';
import Link from '@frontend/components/Link';
import React from 'react';

interface Props {
  publication: PublicationSummaryWithRelease;
}

const PublicationSummary = ({ publication }: Props) => {
  const { legacyPublicationUrl, release, slug, summary, title } = publication;
  return (
    <div className={styles.container}>
      {legacyPublicationUrl ? (
        <>
          <h3 className="govuk-!-margin-bottom-2">{title}</h3>
          <p>
            Currently available via{' '}
            <a href={legacyPublicationUrl}>
              Statistics at DfE <VisuallyHidden>for {title}</VisuallyHidden>
            </a>
          </p>
        </>
      ) : (
        <>
          <h3 className="govuk-!-margin-bottom-2">
            <Link to={`/find-statistics/${slug}`}>{title}</Link>
          </h3>
          <p>{summary}</p>
        </>
      )}

      {release && (
        <ul className="govuk-list">
          <li>
            <strong>Release type:</strong> {releaseTypes[release.type]}
          </li>
          <li>
            <strong>Published:</strong>{' '}
            <FormattedDate format="d MMM yyyy">
              {release.published}
            </FormattedDate>
          </li>
          <li>
            <strong>Theme:</strong> {release.theme.title}
          </li>
        </ul>
      )}
    </div>
  );
};
export default PublicationSummary;
