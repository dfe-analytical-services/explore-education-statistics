import Link from '@frontend/components/Link';
import React from 'react';

export interface Publication {
  id: string;
  legacyPublicationUrl: string | null;
  slug: string;
  summary: string;
  title: string;
}

interface Props {
  publications: Publication[];
}

function PublicationList({ publications }: Props) {
  return (
    <>
      {publications.length > 0 ? (
        publications.map(({ id, legacyPublicationUrl, slug, title }) => (
          <li key={id} className="govuk-!-margin-bottom-6">
            <strong>{title}</strong>
            {legacyPublicationUrl ? (
              <span>
                {' '}
                - currently available via{' '}
                <a href={legacyPublicationUrl}>Statistics at DfE</a>
              </span>
            ) : (
              <div className="govuk-grid-row govuk-!-margin-top-2 govuk-!-margin-bottom-4">
                <div className="govuk-grid-column-one-third govuk-!-margin-bottom-2">
                  <Link
                    className="govuk-link"
                    to={`/find-statistics/publication?publication=${slug}`}
                    as={`/find-statistics/${slug}`}
                    data-testid={`view-stats-${slug}`}
                  >
                    View statistics and data
                  </Link>
                </div>
                <div className="govuk-grid-column-one-third">
                  <Link
                    className="govuk-link"
                    to={`/data-tables/${slug}`}
                    data-testid={`create-table-${slug}`}
                  >
                    Create your own tables online
                  </Link>
                </div>
              </div>
            )}
          </li>
        ))
      ) : (
        <></>
      )}
    </>
  );
}

export default PublicationList;
