import React from 'react';
import Accordion from '../../../components/Accordion';
import TopicList from './TopicList';
interface ThemeItem {
  id: string;
  slug: string;
  summary: string;
  title: string;
}

interface Props {
  items: ThemeItem[];
  linkIdentifier: string;
}

const ThemeList = ({ items = [] }: Props) => (
  <>
    {items.length > 0 ? (
      <>
        {items.map(({ id, slug, title, summary }) => (
          <>
          <h2 className="govuk-heading-l">{title}</h2>
          <Accordion id="{slug}">
            <TopicList theme={slug} />
          </Accordion>
          </>
        ))}
      </>
    ) : (
      <div className="govuk-inset-text">No data currently published.</div>
    )}
  </>
);

export default ThemeList;
