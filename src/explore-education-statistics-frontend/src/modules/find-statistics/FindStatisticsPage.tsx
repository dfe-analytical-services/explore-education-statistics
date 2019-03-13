import React, { Component } from 'react';
import { Helmet } from 'react-helmet';
import { contentApi } from 'src/services/api';
import Page from '../../components/Page';
import TopicList from './components/TopicList';

interface Props {
  themes: {
    id: string;
    slug: string;
    title: string;
  }[];
}

class FindStatisticsPage extends Component<Props> {
  public static defaultProps = {
    themes: [],
  };

  public static async getInitialProps(): Promise<Props> {
    const { data } = await contentApi.get('theme');

    return {
      themes: data,
    };
  }

  public render() {
    const { themes } = this.props;

    return (
      <Page breadcrumbs={[{ name: 'Find statistics and data' }]}>
        <Helmet>
          <title>Find statistics and data - GOV.UK</title>
        </Helmet>

        <h1 className="govuk-heading-xl">Find statistics and data</h1>

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
        </ul>

        {themes.length > 0 ? (
          <>
            {themes.map(({ id, slug, title }) => (
              <div key={id}>
                <h2 className="govuk-heading-l">{title}</h2>

                <TopicList theme={slug} />
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
