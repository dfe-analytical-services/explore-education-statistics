import FormattedDate from '@common/components/FormattedDate';
import React, { useState } from 'react';

interface Props {
  title: string;
  summary: string;
  published: string;
  type: string;
  slug: string;
  theme: string;
  searchType?: string;
  methodologyTitle2?: string;
  data?: string;
}

const PrototypeSearchResult = ({
  title,
  summary,
  published,
  type,
  slug,
  theme,
  searchType,
  methodologyTitle2,
  data,
}: Props) => {
  const [protoSlug] = useState(true);

  return (
    <>
      {searchType === 'data' && (
        <>
          <p>This is the data catalog</p>
          {data}
        </>
      )}
      {searchType === 'methodology' && (
        <>
          <h3 className="govuk-heading-m govuk-!-margin-bottom-2">{title}</h3>
          <p>{summary}</p>
          <dl className="govuk-!-mar.gin-top-0">
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
          <ul>
            <li>
              <a href="#">Methodology for {title}</a>
            </li>
            {methodologyTitle2 && (
              <li>
                <a href="#">{methodologyTitle2}</a>
              </li>
            )}
          </ul>
          <hr />
        </>
      )}
      {!searchType && (
        <>
          <h3 className="govuk-heading-m govuk-!-margin-bottom-2">
            <a
              href={
                protoSlug
                  ? '/prototypes/releaseData'
                  : `https://explore-education-statistics.service.gov.uk/find-statistics/${slug}`
              }
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
      )}
    </>
  );
};

export default PrototypeSearchResult;
