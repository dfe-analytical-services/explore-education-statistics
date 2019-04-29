import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import React from 'react';
import PublicationList, { Publication } from './PublicationList';

export interface Topic {
  id: string;
  slug: string;
  summary: string;
  title: string;
  publications: Publication[];
}

interface Props {
  theme: string;
  topics: Topic[];
}

function TopicList({ theme, topics }: Props) {
  return (
    <>
      {topics.length > 0 ? (
        <Accordion id={theme}>
          {topics.map(({ id, title, summary, publications }) => (
            <AccordionSection heading={title} caption={summary} key={id}>
              <ul className="govuk-!-margin-top-0 govuk-!-padding-top-0">
                <PublicationList publications={publications} />
              </ul>
            </AccordionSection>
          ))}
        </Accordion>
      ) : (
        <div className="govuk-inset-text">No data currently published.</div>
      )}
    </>
  );
}

export default TopicList;
