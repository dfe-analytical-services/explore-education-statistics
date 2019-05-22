import { contentApi } from '@common/services/api';
import Link from '@frontend/components/Link';
import Page from '@frontend/components/Page';
import PageTitle from '@frontend/components/PageTitle';
import React, { Component } from 'react';
import { Topic } from '@common/components/TopicList';

interface Props {
  themes: {
    id: string;
    slug: string;
    title: string;
    topics: Topic[];
  }[];
}

class DownloadIndexPage extends Component<Props> {
  public static defaultProps = {
    themes: [],
  };

  public static async getInitialProps() {
    const themes = await contentApi.get('/Content/tree');
    return { themes };
  }

  public render() {
    const { themes } = this.props;

    return (
      <Page
        breadcrumbs={[
          {
            link: '/download',
            name: 'Download',
          },
        ]}
      >
        <PageTitle title="Download data files" />
        <div className="govuk-grid-row">
          <div className="govuk-grid-column-two-thirds">
            <p className="govuk-body-l">
              Find the data files behind our range of national and regional
              statistics for your own analysis.
            </p>
          </div>
          <div className="govuk-grid-column-one-third">
            <aside className="app-related-items">
              <h2 className="govuk-heading-m" id="releated-content">
                Related content
              </h2>
              <nav role="navigation" aria-labelledby="subsection-title">
                <ul className="govuk-list">
                  <li>
                    <Link to="/statistics">Find statistics and data</Link>
                  </li>
                  <li>
                    <Link to="/glossary">Education statistics: glossary</Link>
                  </li>
                </ul>
              </nav>
            </aside>
          </div>
        </div>

        <h2 className="govuk-heading-l">Choose a theme</h2>

        {themes.length > 0 ? (
          <>
            {themes.map(({ id, title }) => (
              <div key={id}>
                <p>
                  <a href="#">{title}</a>
                </p>
              </div>
            ))}
          </>
        ) : (
          <div className="govuk-inset-text">No data currently published</div>
        )}

        {/* Hard code the below links temporarily, until they're put in the Content API */}
        <p>
          <Link to="/download/pupils-schools">Pupils and schools</Link>
        </p>
        <p>
          <Link to="/download/school-college-performance">
            Schools and college performance - including GCSE and key stage
            performance
          </Link>
        </p>
      </Page>
    );
  }
}

export default DownloadIndexPage;
