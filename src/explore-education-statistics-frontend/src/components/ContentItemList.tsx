import React from 'react';
import { Link } from 'react-router-dom';

interface ContentItem {
  id: string;
  slug: string;
  summary: string;
  title: string;
}

interface Props {
  items: ContentItem[];
  linkIdentifier: string;
}

const ContentItemList = ({ linkIdentifier = '', items = [] }: Props) => (
  <div>
    {items.length > 0 ? (
      <div className="govuk-grid-row">
        {items.map(({ id, slug, title, summary }) => (
          <div className="govuk-grid-column-one-half" key={id}>
            <h4>
              <Link to={`${linkIdentifier}/${slug}`} className="govuk-link">
                {title}
              </Link>
            </h4>
            <p>{summary}</p>
          </div>
        ))}
      </div>
    ) : (
      <div className="govuk-inset-text">None currently published.</div>
    )}
  </div>
);

export default ContentItemList;
