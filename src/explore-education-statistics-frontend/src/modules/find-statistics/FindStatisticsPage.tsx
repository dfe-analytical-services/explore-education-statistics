import React, { Component } from 'react';
import Page from 'src/components/Page';
import PageTitle from 'src/components/PageTitle';
import { contentApi } from 'src/services/api';
import TopicList, { Topic } from './components/TopicList';

interface Props {
  themes: {
    id: string;
    slug: string;
    title: string;
    topics: Topic[];
  }[];
}

class FindStatisticsPage extends Component<Props> {
  public static defaultProps = {
    themes: [],
  };

  public static async getInitialProps(): Promise<Props> {
    const themes = await contentApi.get('/Content/tree');

    return {
      themes,
    };
  }

  public render() {
    const { themes } = this.props;

    return (
      <Page breadcrumbs={[{ name: 'Find statistics and data' }]}>
        <PageTitle title="Find statistics and data" />

        <p className="govuk-body-l">
          Browse to find the statistics and data you’re looking for and open the
          section to get links to:
        </p>

        <ul className="govuk-!-margin-bottom-9">
          <li>
            up-to-date national statistical headlines, breakdowns and
            explanations
          </li>
          <li>
            charts and tables to help you compare, contrast and view national
            and regional statistical data and trends
          </li>
          <li>
            links to underlying data so you can download files and carry out
            your own statistical analysis
          </li>
        </ul>

        {themes.length > 0 ? (
          <>
            {themes.map(({ id, slug, title, topics }) => (
              <div key={id}>
                <h2 className="govuk-heading-l">{title}</h2>

                <TopicList topics={topics} theme={id} />
              </div>
            ))}
          </>
        ) : (
          <div className="govuk-inset-text">No data currently published.</div>
        )}
      </Page>
    );
  }
}

export default FindStatisticsPage;
