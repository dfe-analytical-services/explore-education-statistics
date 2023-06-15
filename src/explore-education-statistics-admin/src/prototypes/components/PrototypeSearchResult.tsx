import React from 'react';
import FormattedDate from '@common/components/FormattedDate';

interface Props {
  title: string;
  summary: string;
  published: string;
  type: string;
  slug: string;
  theme: string;
}

const PrototypeSearchResult = ({
  title,
  summary,
  published,
  type,
  slug,
  theme,
}: Props) => {
  return (
    <>
      <h3 className="govuk-heading-m govuk-!-margin-bottom-2">
        <a
          href={`https://explore-education-statistics.service.gov.uk/find-statistics/${slug}`}
        >
          {title}
        </a>
      </h3>
      <p>{summary}</p>

      <dl className="govuk-!-margin-top-0">
        <div className="dfe-flex">
          <dt>Release type:</dt>
          <dd className="govuk-!-margin-left-2">{type}</dd>
        </div>
        <div className="dfe-flex">
          <dt>Published:</dt>
          <dd className="govuk-!-margin-left-2">
            <FormattedDate format="d MMM yyyy">{published}</FormattedDate>
          </dd>
        </div>
        <div className="dfe-flex">
          <dt>Theme:</dt>
          <dd className="govuk-!-margin-left-2">{theme}</dd>
        </div>
      </dl>

      <hr />
    </>
  );
};

export default PrototypeSearchResult;
