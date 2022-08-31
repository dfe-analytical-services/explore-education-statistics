import FormattedDate from '@common/components/FormattedDate';
import { PublicationSummaryWithRelease } from '@common/services/publicationService';
import { releaseTypes } from '@common/services/types/releaseType';
import styles from '@frontend/modules/find-statistics/components/PublicationSummary.module.scss';
import Link from '@frontend/components/Link';
import React from 'react';

interface Props {
  publication: PublicationSummaryWithRelease;
}

const PublicationSummary = ({ publication }: Props) => {
  const {
    legacyPublicationUrl,
    latestRelease,
    slug,
    summary,
    theme,
    title,
  } = publication;
  return (
    <li className={`${styles.container} govuk-!-margin-top-4`}>
      <h3 className="govuk-!-margin-bottom-2">
        <Link to={legacyPublicationUrl ?? `/find-statistics/${slug}`}>
          {title}
        </Link>
      </h3>
      <p>
        {legacyPublicationUrl
          ? 'Not yet on this service. Currently available via Statistics at DfE.'
          : summary}
      </p>

      {latestRelease && (
        <dl>
          <div className="dfe-flex">
            <dt>Release type:</dt>
            <dd className="govuk-!-margin-left-2" data-testid="release-type">
              {releaseTypes[latestRelease.type]}
            </dd>
          </div>
          <div className="dfe-flex">
            <dt>Published:</dt>
            <dd className="govuk-!-margin-left-2" data-testid="published">
              <FormattedDate format="d MMM yyyy">
                {latestRelease.published}
              </FormattedDate>
            </dd>
          </div>
          <div className="dfe-flex">
            <dt>Theme:</dt>
            <dd className="govuk-!-margin-left-2" data-testid="theme">
              {theme?.title}
            </dd>
          </div>
        </dl>
      )}
    </li>
  );
};
export default PublicationSummary;
