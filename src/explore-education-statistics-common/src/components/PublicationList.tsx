import Details from '@common/components/Details';
import Link from '@frontend/components/Link';
import React from 'react';

export interface Publication {
  id: string;
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
        publications.map(({ id, slug, summary, title }) => (
          <li key={id}>
            <strong>{title}</strong>
            <div className="govuk-!-margin-top-0 govuk-!-margin-bottom-4">
              <div className="govuk-grid-column-one-third">
                <Link
                  className="govuk-link govuk-!-margin-right-9"
                  to={`/statistics/publication?publication=${slug}`}
                  as={`/statistics/${slug}`}
                  data-testid={`view-stats-${slug}`}
                >
                  View statistics and data
                </Link>
              </div>
              <div className="govuk-grid-column-one-third">
                <Link
                  className="govuk-link"
                  to={`/table-tool/${slug}`}
                  data-testid={`create-table-${slug}`}
                >
                  Create your own tables online
                </Link>
              </div>
            </div>
          </li>
        ))
      ) : (
        <></>
      )}
    </>
  );
}

export default PublicationList;
