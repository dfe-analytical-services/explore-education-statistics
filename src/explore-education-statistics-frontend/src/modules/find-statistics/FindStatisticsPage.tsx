import { contentApi } from '@common/services/api';
import Link from '@frontend/components/Link';
import Page from '@frontend/components/Page';
import PageTitle from '@frontend/components/PageTitle';
import React, { Component } from 'react';
import TopicList, { Topic } from '@common/components/TopicList';

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

  public static async getInitialProps() {
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

        <div className="govuk-grid-row">
          <div className="govuk-grid-column-two-thirds">
            <p className="govuk-body-l">
              Browse to find the statistics and data you’re looking for and open
              the section to get links to:
            </p>

            <ul className="govuk-!-margin-bottom-9">
              <li>
                up-to-date national statistical headlines, breakdowns and
                explanations
              </li>
              <li>
                charts and tables to help you compare, contrast and view
                national and regional statistical data and trends
              </li>
              <li>
                our table tool to build your own tables online and explore our
                range of national and regional data
              </li>
              <li>
                links to underlying data so you can download files and carry out
                your own statistical analysis
              </li>
            </ul>
          </div>
          <div className="govuk-grid-column-one-third">
            <aside className="app-related-items">
              <h2 className="govuk-heading-m" id="releated-content">
                Related content
              </h2>
              <nav role="navigation" aria-labelledby="subsection-title">
                <ul className="govuk-list">
                  <li>
                    <Link to="/methodology">
                      Education statistics: methodology
                    </Link>
                  </li>
                  <li>
                    <Link to="/glossary">Education statistics: glossary</Link>
                  </li>
                </ul>
              </nav>
            </aside>
          </div>
        </div>

        {themes.length > 0 ? (
          <>
            {themes.map(({ id, title, topics }) => (
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
