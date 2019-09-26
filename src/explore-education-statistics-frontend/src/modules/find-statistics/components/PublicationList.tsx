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
          <li key={id} className="govuk-!-margin-bottom-0">
            <h3 className="govuk-heading-s govuk-!-margin-bottom-0">{title}</h3>
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
                    className="govuk-link"
                    to={`/find-statistics/publication?publication=${slug}`}
                    as={`/find-statistics/${slug}`}
                    data-testid={`view-stats-${slug}`}
                  >
                    View statistics and data{' '}
                    <span className="govuk-visually-hidden">for {title}</span>
                  </Link>
                </div>
                <div className="govuk-grid-column-one-third">
                  <Link
                    className="govuk-link"
                    to={`/data-tables/${slug}`}
                    data-testid={`create-table-${slug}`}
                  >
                    Create your own tables online{' '}
                    <span className="govuk-visually-hidden">for {title}</span>
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
