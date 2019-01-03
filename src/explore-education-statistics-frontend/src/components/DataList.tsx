import React from 'react';
import { Link } from 'react-router-dom';

interface Props {
  data: Array<{
    id: string,
    slug: string,
    summary: string,
    title: string
  }>,
  linkIdentifier: string,
}

const DataList = ({ linkIdentifier = '', data = [] }: Props) => (
  <div>
    {data.length > 0 ? (
      <div className="govuk-grid-row">
        {data.map((elem => (
          <div className="govuk-grid-column-one-half" key={elem.id}>
            <h4 className="govuk-heading-s">
              <Link
                className="govuk-link"
                to={`${linkIdentifier}/${elem.slug}`}
              >
                {elem.title}
              </Link>
            </h4>
            <p className="govuk-body">{elem.summary}</p>

            <p className="govuk-body">link description</p>
          </div>
        )))}
      </div>
    ) : (
      <div className="govuk-inset-text">
        None currently published.
      </div>
    )}
  </div>
);

export default DataList;
