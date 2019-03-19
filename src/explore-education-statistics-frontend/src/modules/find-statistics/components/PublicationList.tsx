import React, { Component } from 'react';
import Link from 'src/components/Link';
import { contentApi } from 'src/services/api';

interface Props {
  topic: string;
}

interface State {
  publications: {
    id: string;
    slug: string;
    summary: string;
    title: string;
  }[];
}

class PublicationList extends Component<Props, State> {
  public state = {
    publications: [],
  };

  public componentDidMount() {
    const { topic } = this.props;

    contentApi
      .get(`topic/${topic}/publications`)
      .then(publications => this.setState({ publications }));
  }

  public render() {
    const { publications } = this.state;

    return (
      <>
        {publications.length > 0 ? (
          publications.map(({ id, slug, summary, title }) => (
            <li key={id}>
              <h3 className="govuk-heading-m govuk-!-margin-bottom-0">
                {title}
              </h3>
              <p className="govuk-caption-m govuk-!-margin-top-0 govuk-!-margin-bottom-1">
                {summary}
              </p>
              <div className="govuk-!-margin-top-0 govuk-!-margin-bottom-9">
                <div className="govuk-grid-row">
                  <div className="govuk-grid-column-one-third">
                    <Link
                      className="govuk-link govuk-!-margin-right-9"
                      to={`/statistics/publication?publication=${slug}`}
                      as={`/statistics/${slug}`}
                    >
                      View statistics
                    </Link>
                  </div>
                </div>
              </div>
            </li>
          ))
        ) : (
          <div className="govuk-inset-text">
            No publications currently released.
          </div>
        )}
      </>
    );
  }
}

export default PublicationList;
