import React, { Component } from 'react';
import Breadcrumbs from '../../components/Breadcrumbs';
import Link from '../../components/Link';
import { KeyIndicator } from './components/KeyIndicator';
import keyIndicatorTestData from './components/keyIndicatorTestData';
import { PublicationSummary } from './components/PublicationSummary';
import publicationSummaryTestData from './components/publicationSummaryTestData';
import styles from './LocalAuthorityPage.module.scss';

class LocalAuthorityPage extends Component<{}> {
  public render() {
    return (
      <>
        <Breadcrumbs current="Sheffield" />

        <div className="govuk-grid-row">
          <div className="govuk-grid-column-two-thirds">
            <span className="govuk-caption-xl">Metropolitan borough</span>
            <h1>Sheffield</h1>

            <h3>
              Find school, social care and 16+ education statistics for
              Sheffield
            </h3>

            <p>
              Here you can find DfE stats for Sheffield, and access them as
              reports, customise and download as excel files or csv files, and
              access them via an API. <Link to="#">(Find out more)</Link>
            </p>
          </div>

          <div className="govuk-grid-column-one-third">
            <strong>About Sheffield</strong>

            <dl className="govuk-body-s">
              <dt>Type:</dt>
              <dd>Metropolitan borough</dd>

              <dt>Region:</dt>
              <dd>Yorkshire and Humber</dd>
            </dl>
          </div>
        </div>

        <h4>What sort of stats are you looking for?</h4>

        <div className="govuk-grid-row">
          <div className="govuk-grid-column-one-third">
            <Link to="#">Schools</Link>
            <p>
              Absence and exclusions, School and pupil numbers, Capacity and
              admissions, Results, Teacher numbers
            </p>
          </div>
          <div className="govuk-grid-column-one-third">
            <Link to="#">Schools</Link>
            <p>Number of children, Vulnerable children</p>
          </div>
          <div className="govuk-grid-column-one-third">
            <Link to="#">16+ Education</Link>
            <p>Further education, Higher education, Apprenticeships</p>
          </div>
        </div>

        <hr />

        <h3>
          Latest publications for Sheffield (
          <Link to="#">see all publications</Link>)
        </h3>

        <p>
          These are the latest official statistics with figures for Sheffield.
          You can access the report and commentary, and also get the data for
          use in Excel and other tools. You can now customise the data to your
          requirements, and get a variety of formats. (
          <Link to="#">Find out more</Link>) (
          <Link to="#">Find more publications</Link>)
        </p>

        <div className={styles.wrappedRow}>
          {publicationSummaryTestData.map(publicationSummary => (
            <div className={styles.publicationSummary}>
              <PublicationSummary {...publicationSummary} />
            </div>
          ))}
        </div>

        <hr />

        <h3>
          Key indicators for Sheffield (<Link to="#">change</Link>)
        </h3>

        <p>
          These are some key indicators for Sheffield. You can change what you
          see here according your requirements. (
          <Link to="#">Find out more</Link>)
        </p>

        <div className={styles.wrappedRow}>
          {keyIndicatorTestData.map(keyIndicator => (
            <div className={styles.keyIndicator}>
              <KeyIndicator {...keyIndicator} />
            </div>
          ))}
        </div>
      </>
    );
  }
}

export default LocalAuthorityPage;
