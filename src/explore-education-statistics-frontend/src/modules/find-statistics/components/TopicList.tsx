import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import React, { Component } from 'react';
import PublicationList, { Publication } from '../components/PublicationList';

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

class TopicList extends Component<Props> {
  public render() {
    const { theme, topics } = this.props;

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
}

export default TopicList;
