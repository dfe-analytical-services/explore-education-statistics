import Link from '@frontend/components/Link';
import Page from '@frontend/components/Page';
import PageTitle from '@frontend/components/PageTitle';
import React from 'react';

function DownloadIndexPage() {
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

      <p>
        <a href="#">Children and early years - including social care</a>
      </p>
      <p>
        <a href="#">Destinations of pupils and students - including NEET</a>
      </p>
      <p>
        <a href="#">Finance and funding</a>
      </p>
      <p>
        <a href="#">Further education</a>
      </p>
      <p>
        <a href="#">Higher education</a>
      </p>
      <p>
        <Link to="/download/pupils-schools">Pupils and schools</Link>
      </p>
      <p>
        <a href="/download/school-college-performance">
          Schools and college performance - including GCSE and key stage
          performance
        </a>
      </p>
      <p>
        <a href="#">Teachers and school workforce</a>
      </p>
      <p>
        <a href="#">UK education and training statistics</a>
      </p>
    </Page>
  );
}

export default DownloadIndexPage;
