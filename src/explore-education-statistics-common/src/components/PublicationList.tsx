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
            <h3 className="govuk-heading-m govuk-!-margin-bottom-0">{title}</h3>
            <p className="govuk-caption-m govuk-!-margin-top-0 govuk-!-margin-bottom-1">
              {summary}
            </p>
            <div className="govuk-!-margin-top-0 govuk-!-margin-bottom-5">
              <div className="govuk-grid-row">
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
                    className="govuk-link govuk-!-margin-right-9"
                    to={`/table-tool/${slug}`}
                    data-testid={`create-table-${slug}`}
                  >
                    Create your own tables online
                  </Link>
                </div>
              </div>
            </div>
          </li>
        ))
      ) : (
        <div className="govuk-inset-text">
          These statistics and data are not yet available on the explore
          education statistics service. To find and download these statistics
          and data browse{' '}
          <a href="https://www.gov.uk/government/organisations/department-for-education/about/statistics#statistical-collections">
            Statistics at DfE
          </a>
        </div>
      )}
    </>
  );
}

export default PublicationList;
