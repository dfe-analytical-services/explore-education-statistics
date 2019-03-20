import React, { Component } from 'react';
import Accordion from 'src/components/Accordion';
import AccordionSection from 'src/components/AccordionSection';
import { contentApi } from 'src/services/api';
import PublicationList from '../components/PublicationList';

interface Props {
  theme: string;
}

interface State {
  topics: {
    id: string;
    slug: string;
    summary: string;
    title: string;
  }[];
}

class TopicList extends Component<Props, State> {
  public state = {
    topics: [],
  };

  public componentDidMount() {
    const { theme } = this.props;

    contentApi
      .get(`theme/${theme}/topics`)
      .then(topics => this.setState({ topics }));
  }

  public render() {
    const { theme } = this.props;
    const { topics } = this.state;

    return (
      <>
        {topics.length > 0 ? (
          <Accordion id={theme}>
            {topics.map(({ id, slug, title, summary }) => (
              <AccordionSection heading={title} caption={summary} key={id}>
                <ul className="govuk-!-margin-top-0 govuk-!-padding-top-0">
                  <PublicationList topic={slug} />
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
