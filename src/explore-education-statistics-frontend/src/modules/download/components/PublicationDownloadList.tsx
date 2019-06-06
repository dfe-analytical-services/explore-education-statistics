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
        publications.map(({ id, slug, title }) => (
          <div className="govuk-!-margin-bottom-9" key={id}>
            <h3 className="govuk-heading-s">Download files for: {title}</h3>
            <p>
              <Link
                to={`/statistics/downloads?publication=${slug}`}
                as={`/statistics/downloads/${slug}`}
                data-testid={`download-stats-${slug}`}
              >
                Example download file 1
              </Link>{' '}
              (csv, 100mb)
            </p>
            <p>
              <Link
                to={`/statistics/downloads?publication=${slug}`}
                as={`/statistics/downloads/${slug}`}
                data-testid={`download-stats-${slug}`}
              >
                Example download file 2
              </Link>{' '}
              (csv, 100mb)
            </p>
          </div>
        ))
      ) : (
        <></>
      )}
    </>
  );
}

export default PublicationList;
