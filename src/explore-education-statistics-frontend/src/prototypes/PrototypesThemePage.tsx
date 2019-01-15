import React from 'react';
// import CollapsibleSection from '../components/CollapsibleSection';
// import Details from '../components/Details';
// import PrototypeDataSample from './components/PrototypeDataSample';
import Link from '../components/Link';
import PrototypePage from './components/PrototypePage';
import PrototypeSearchForm from './components/PrototypeSearchForm';

const ThemePage = () => {
  return (
    <PrototypePage
      breadcrumbs={[
        { text: 'Schools', link: 'topic' },
        { text: 'Absence and exclusions', link: '#' },
      ]}
    >
      <h1 className="govuk-heading-l">
        Explore pupil absence statistics for schools in England
      </h1>
      <p className="govuk-body">
        Here you can find DfE stats for absence and exlusions, and access them
        as reports, customise and download as excel files, and access them via
        an API. <a href="#">Find out more</a>
      </p>
      <PrototypeSearchForm />
      <h2 className="govuk-heading-m">
        The following publications are available in absence and exclusions
      </h2>
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-one-half">
          <a
            href="publication"
            className="govuk-heading-s govuk-!-margin-bottom-0"
          >
            Pupil absence
          </a>
          <p className="govuk-caption-m govuk-!-margin-top-0">
            Overall absence, authorised absence, unauthorised absence,
            persisitent absence
          </p>
        </div>
        <div className="govuk-grid-column-one-half">
          <a
            href="publication"
            className="govuk-heading-s govuk-!-margin-bottom-0"
          >
            Permananent and fixed period exclusions
          </a>
          <p className="govuk-caption-m govuk-!-margin-top-0">
            Permanent exclusions, and fixed period exclusions
          </p>
        </div>
      </div>
      <hr />
      <h2 className="govuk-heading-m">
        Latest publications in absence and exclusions{' '}
        <Link to="#" className="govuk-body">
          (see all publications)
        </Link>
      </h2>
      <p>
        These are the latest official statistics with figures for Sheffield. You
        can access the report and commentary, and also get the data for use in
        Excel and other tools. You can now customise the data to your
        requirements, and get a variety of formats.
      </p>
      <div className="dfe-dash-tiles dfe-dash-tiles--2-in-row govuk-!-margin-top-0 govuk-!-padding-top-0">
        <div className="dfe-dash-tiles__tile">
          <h2 className="govuk-heading-m">
            <Link to="publication">Pupil absence in schools in England</Link>
          </h2>

          <div className="govuk-grid-row">
            <div className="govuk-grid-column-one-half">
              <h2 className="govuk-heading-s">Sheffield overall absence</h2>
              <span className="govuk-heading-xl govuk-!-margin-bottom-0 govuk-caption-increase-negative">
                4.7%
              </span>
              <p className="govuk-body">
                <strong className="increase">
                  +0.4
                  <abbr
                    aria-label="Percentage points"
                    title="Percentage points"
                  >
                    ppt
                  </abbr>
                </strong>
                more than 2015/16
              </p>
            </div>
            <div className="govuk-grid-column-one-half">
              <div className="govuk-body-s">
                <ul>
                  <li>
                    On average in 2016/17,{' '}
                    <strong>pupils missed 8.2 school days</strong>
                  </li>
                  <li>
                    Overall and unauthorised absence rates have{' '}
                    <strong>increased</strong> since last year
                  </li>
                  <li>
                    <strong>One in ten</strong> pupils was persistently absent
                    during the academic year 2016/17
                  </li>
                </ul>
              </div>
            </div>
          </div>
          <details className="govuk-details">
            <summary className="govuk-details__summary">
              <span className="govuk-details__summary-text">
                What does this mean?
              </span>
            </summary>
            <div className="govuk-details__text">
              Permanent exclusion rate is the adipisicing elit. Dolorum hic
              nobis voluptas quidem fugiat enim ipsa reprehenderit nulla.
            </div>
          </details>
          <hr />
          <div className="govuk-grid-row">
            <div className="govuk-grid-column-one-half">
              <ul className="govuk-list govuk-body-s">
                <li>
                  <a className="govuk-link" href="/local-authorities/sheffield">
                    View report
                  </a>
                </li>
                <li>
                  <a className="govuk-link" href="/local-authorities/sheffield">
                    Download data
                  </a>
                </li>
              </ul>
            </div>
            <div className="govuk-grid-column-one-half">
              <ul className="govuk-list govuk-body-s">
                <li>
                  <a className="govuk-link" href="/local-authorities/sheffield">
                    Visualise data
                  </a>
                </li>
                <li>
                  <a className="govuk-link" href="/local-authorities/sheffield">
                    Create your own dataset
                  </a>
                </li>
              </ul>
            </div>
          </div>
        </div>
        <div className="dfe-dash-tiles__tile">
          <h2 className="govuk-heading-m">
            <Link to="publication">Permanent and fixed period exlusions</Link>
          </h2>

          <div className="govuk-grid-row">
            <div className="govuk-grid-column-one-half">
              <h2 className="govuk-heading-s">
                Overall rate of fixed period exclusions
              </h2>
              <span className="govuk-heading-xl govuk-!-margin-bottom-0 govuk-caption-increase-negative">
                0.10%
              </span>
              <p className="govuk-body">
                <strong className="increase">
                  +0.2
                  <abbr
                    aria-label="Percentage points"
                    title="Percentage points"
                  >
                    ppt
                  </abbr>
                </strong>
                more than 2015/16
              </p>
            </div>
            <div className="govuk-grid-column-one-half">
              <div className="govuk-body-s">
                <ul>
                  <li>
                    On average in 2016/17,{' '}
                    <strong>pupils missed 8.2 school days</strong>
                  </li>
                  <li>
                    Overall and unauthorised absence rates have{' '}
                    <strong>increased</strong> since last year
                  </li>
                  <li>
                    <strong>One in ten</strong> pupils was persistently absent
                    during the academic year 2016/17
                  </li>
                </ul>
              </div>
            </div>
          </div>
          <details className="govuk-details">
            <summary className="govuk-details__summary">
              <span className="govuk-details__summary-text">
                What does this mean?
              </span>
            </summary>
            <div className="govuk-details__text">
              Permanent exclusion rate is the adipisicing elit. Dolorum hic
              nobis voluptas quidem fugiat enim ipsa reprehenderit nulla.
            </div>
          </details>
          <hr />
          <div className="govuk-grid-row">
            <div className="govuk-grid-column-one-half">
              <ul className="govuk-list govuk-body-s">
                <li>
                  <a className="govuk-link" href="/local-authorities/sheffield">
                    View report
                  </a>
                </li>
                <li>
                  <a className="govuk-link" href="/local-authorities/sheffield">
                    Download data
                  </a>
                </li>
              </ul>
            </div>
            <div className="govuk-grid-column-one-half">
              <ul className="govuk-list govuk-body-s">
                <li>
                  <a className="govuk-link" href="/local-authorities/sheffield">
                    Visualise data
                  </a>
                </li>
                <li>
                  <a className="govuk-link" href="/local-authorities/sheffield">
                    Create your own dataset
                  </a>
                </li>
              </ul>
            </div>
          </div>
        </div>
      </div>
      <hr />
      <h2 className="govuk-heading-m">
        Key indicators for Sheffield{' '}
        <Link to="#" className="govuk-body">
          (change key indicators)
        </Link>
      </h2>
      <p>
        These are some key indicators for Sheffield. You can change what you see
        here according your requirements. <Link to="#">Find out more</Link>
      </p>

      <h3 className="govuk-heading-s">
        <Link to="#">Find an indicator &#x25BC;</Link>
      </h3>
      <div className="dfe-dash-tiles dfe-dash-tiles--3-in-row">
        <div className="dfe-dash-tiles__tile">
          <h3 className="govuk-heading-m">
            Permanent exclusion rate
            <span className="govuk-caption-m date-range govuk-tag">
              2016/17
            </span>
          </h3>
          <span className="govuk-heading-xl govuk-!-margin-bottom-0 govuk-caption-increase-negative">
            4.7%
          </span>
          <p className="govuk-body">
            <strong className="increase">
              +0.4
              <abbr aria-label="Percentage points" title="Percentage points">
                ppt
              </abbr>
            </strong>
            more than 2015/16
            <br />
            <a className="referenceLink" href="/local-authorities/sheffield">
              From: Early years foundation state profile results
            </a>
          </p>
          <details className="govuk-details">
            <summary className="govuk-details__summary">
              <span className="govuk-details__summary-text">
                What does this mean?
              </span>
            </summary>
            <div className="govuk-details__text">
              Permanent exclusion rate is the adipisicing elit. Dolorum hic
              nobis voluptas quidem fugiat enim ipsa reprehenderit nulla.
            </div>
          </details>
        </div>

        <div className="dfe-dash-tiles__tile">
          <h3 className="govuk-heading-m">
            Fixed period exlusion rate
            <span className="govuk-caption-m date-range govuk-tag">
              2016/17
            </span>
          </h3>
          <div>
            <span className="govuk-heading-xl govuk-!-margin-bottom-0 govuk-caption-increase-negative">
              3.4%
            </span>
            <p className="govuk-body">
              <strong className="level">
                0
                <abbr aria-label="Percentage points" title="Percentage points">
                  ppt
                </abbr>
              </strong>
              the same as 2015/16
              <br />
              <a className="referenceLink" href="/local-authorities/sheffield">
                From: National curriculum assessments at key stage 2 in England
              </a>
            </p>
          </div>
          <details className="govuk-details">
            <summary className="govuk-details__summary">
              <span className="govuk-details__summary-text">
                What does this mean?
              </span>
            </summary>
            <div className="govuk-details__text">
              Fixed period exlusion rateis the adipisicing elit. Dolorum hic
              nobis voluptas quidem fugiat enim ipsa reprehenderit nulla.
            </div>
          </details>
        </div>

        <div className="dfe-dash-tiles__tile">
          <h3 className="govuk-heading-m">
            One or more fixed period exclusion
            <span className="govuk-caption-m date-range govuk-tag">
              2016/17
            </span>
          </h3>
          <div>
            <span className="govuk-heading-xl govuk-!-margin-bottom-0 govuk-caption-increase-negative">
              1.3%
            </span>
            <p className="govuk-body">
              <strong className="decrease">
                -0.4
                <abbr aria-label="Percentage points" title="Percentage points">
                  ppt
                </abbr>
              </strong>
              more than 2015/16 <br />
              <a className="referenceLink" href="/local-authorities/sheffield">
                From: GCSE and equivalent results: 2017 to 2018 (provisional)
              </a>
            </p>
          </div>
          <details className="govuk-details">
            <summary className="govuk-details__summary">
              <span className="govuk-details__summary-text">
                What does this mean?
              </span>
            </summary>
            <div className="govuk-details__text">
              One or more fixed period exclusionis the adipisicing elit. Dolorum
              hic nobis voluptas quidem fugiat enim ipsa reprehenderit nulla.
            </div>
          </details>
        </div>
      </div>
      <a href="#" className="govuk-button">
        Explore statistics
      </a>
    </PrototypePage>
  );
};

export default ThemePage;
