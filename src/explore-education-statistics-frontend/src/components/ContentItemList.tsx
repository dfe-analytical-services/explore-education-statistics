import React from 'react';
import { Link } from 'react-router-dom';
import Accordion from '../components/Accordion';
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
  <>
    {items.length > 0 ? (
      <>
        {items.map(({ id, slug, title, summary }) => (
          <>
          <h2 className="govuk-heading-l">{title}</h2>
          <Accordion id="{slug}">
            <p>TODO: fetch publications</p>
          </Accordion>
          </>
        ))}
      </>
    ) : (
      <div className="govuk-inset-text">None data currently published.</div>
    )}
  </>
);

export default ContentItemList;
