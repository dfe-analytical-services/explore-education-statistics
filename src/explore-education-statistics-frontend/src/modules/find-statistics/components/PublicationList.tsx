import { PublicationSummary } from '@common/services/themeService';
import Link from '@frontend/components/Link';
import React from 'react';

interface Props {
  publications: PublicationSummary[];
  testId?: string;
}

function PublicationList({ publications, testId }: Props) {
  return publications.length > 0 ? (
    <ul className="govuk-!-margin-top-0" data-testid={testId}>
      {publications.map(({ id, legacyPublicationUrl, slug, title }) => (
        <li key={id} className="govuk-!-margin-bottom-0">
          <h4
            id={`publication-heading-${id}`}
            className="govuk-heading-s govuk-!-font-weight-regular govuk-!-margin-bottom-0"
          >
            {title}
          </h4>
          {legacyPublicationUrl ? (
            <div className="govuk-!-margin-bottom-3">
              {' '}
              Currently available via{' '}
              <a href={legacyPublicationUrl}>
                Statistics at DfE{' '}
                <span className="govuk-visually-hidden">for {title}</span>
              </a>
            </div>
          ) : (
            <div className="govuk-grid-row govuk-!-margin-bottom-3">
              <div className="govuk-grid-column-one-third govuk-!-margin-bottom-1">
                <Link
                  to={`/find-statistics/${slug}`}
                  testId={`View stats link for ${title}`}
                >
                  View statistics and data{' '}
                  <span className="govuk-visually-hidden">for {title}</span>
                </Link>
              </div>

              <div className="govuk-grid-column-one-third">
                <Link
                  to={`/data-tables/${slug}`}
                  data-testid={`Create table link for ${title}`}
                >
                  Create your own tables{' '}
                  <span className="govuk-visually-hidden">for {title}</span>
                </Link>
              </div>
            </div>
          )}
        </li>
      ))}
    </ul>
  ) : null;
}

export default PublicationList;
