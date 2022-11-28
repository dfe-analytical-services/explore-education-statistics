import FormattedDate from '@common/components/FormattedDate';
import { PublicationListSummary } from '@common/services/publicationService';
import { releaseTypes } from '@common/services/types/releaseType';
import styles from '@frontend/modules/find-statistics/components/PublicationSummary.module.scss';
import Link from '@frontend/components/Link';
import React from 'react';

interface Props {
  publication: PublicationListSummary;
}

const PublicationSummary = ({ publication }: Props) => {
  const { published, slug, summary, theme, title, type } = publication;
  return (
    <li className={`${styles.container} govuk-!-margin-top-4`}>
      <h3 className="govuk-!-margin-bottom-2">
        <Link to={`/find-statistics/${slug}`}>{title}</Link>
      </h3>
      <p>{summary}</p>

      <dl>
        <div className="dfe-flex">
          <dt>Release type:</dt>
          <dd className="govuk-!-margin-left-2" data-testid="release-type">
            {releaseTypes[type]}
          </dd>
        </div>
        <div className="dfe-flex">
          <dt>Published:</dt>
          <dd className="govuk-!-margin-left-2" data-testid="published">
            <FormattedDate format="d MMM yyyy">{published}</FormattedDate>
          </dd>
        </div>
        <div className="dfe-flex">
          <dt>Theme:</dt>
          <dd className="govuk-!-margin-left-2" data-testid="theme">
            {theme}
          </dd>
        </div>
      </dl>
    </li>
  );
};
export default PublicationSummary;
