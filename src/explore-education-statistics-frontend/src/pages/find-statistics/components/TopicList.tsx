import React, { Component, HTMLAttributes, ReactNode } from 'react';
import AccordionSection from '../../../components/AccordionSection';
import { contentApi } from '../../../services/api';
import PublicationList from '../components/PublicationList';

interface Props {
  theme: string;
}

interface State {
  publications: object[];
  theme: string;
}

class TopicList extends Component<Props> {
  public state = {
    theme: '',
    topics: [],
  };

  public componentDidMount() {
    const { theme } = this.props;
    contentApi
      .get(`theme/${theme}/topics`)
      .then(json => this.setState({ topics: json.data }))
      // tslint:disable-next-line:no-console
      .catch(error => console.log(error));
  }

  public render() {
    const { theme, topics } = this.state;

    return (
      <>
        {topics.length > 0 ? (
          <>
            {topics.map(({ id, slug, title, summary }) => (
              <AccordionSection id={id} heading={title} caption={summary}>
                <div className="govuk-!-margin-top-0 govuk-!-padding-top-0">
                  <ul className="govuk-list-bullet">
                    <PublicationList topic={slug}/>
                  </ul>
                </div>
              </AccordionSection>
            ))}
          </>
        ) : (
          <div className="govuk-inset-text">No data currently published.</div>
        )}
      </>
    );
  }
}

export default TopicList;
