import React, { Component } from 'react';
import { match } from 'react-router';
import api from '../api';
import DataList from '../components/DataList';
import { H2, H3 } from '../components/Heading';
import Link from '../components/Link';
import Title from '../components/Title';

interface Props {
  match: match<{
    theme: string;
  }>;
}

interface State {
  data: {
    title: string;
  };
  topics: any[];
}

class ThemePage extends Component<Props, State> {
  public state = {
    data: {
      title: '',
    },
    topics: [],
  };

  public componentDidMount() {
    const { theme } = this.props.match.params;

    api
      .get(`theme/${theme}`)
      .then(({ data }) => this.setState({ data }))
      .catch(error => alert(error));
    api
      .get(`theme/${theme}/topics`)
      .then(({ data }) => this.setState({ topics: data }))
      .catch(error => alert(error));
  }

  public render() {
    const { data, topics } = this.state;

    return (
      <div>
        <div className="govuk-grid-row">
          <div className="govuk-grid-column-two-thirds">
            <Title label={data.title} />

            <p className="govuk-body">
              Here you can find DfE stats for Schools, and access them as
              reports, customise and download as excel files or csv files, and
              access them via an API. <Link to="#">(Find out more)</Link>
            </p>
            <p className="govuk-body">
              You can also see our statistics for 16+ education and social care.
            </p>

            <div className="govuk-form-group">
              <label className="govuk-label" htmlFor="find-for-schools">
                Find any DfE statistic, publication or indicator for schools:
              </label>
              <span id="find-for-schools" className="govuk-hint">
                Find publications, statistics and indicators for Schools.
              </span>
              <input
                className="govuk-input govuk-input--width-40"
                id="find-for-schools"
                name="find-for-schools"
                type="text"
                aria-describedby="find-for-schools-hint"
              />
              <button type="submit" className="govuk-button">
                Find
              </button>
            </div>
            <H3>
              What sort of stats are you looking for?
            </H3>
            <DataList data={topics} linkIdentifier={window.location.pathname} />
          </div>
        </div>
        <hr className="govuk-section-break--l govuk-section-break--visible" />

        <section id="latest-publications">
          <H2>
            Latest publications in {(data.title || '').toLowerCase()}
          </H2>

          <p className="govuk-body">
            These are the latest official statistics with figures in{' '}
            {(data.title || '').toLowerCase()}. You can access the report and
            commentary, and also get the data for use in Excel and other tools.
            You can now customise the data to your requirements, and get a
            variety of formats.
          </p>

          <hr className="govuk-section-break govuk-section-break--xl govuk-section-break--visible" />
        </section>

        <section id="key-indicators">
          <H2>
            Key indicators for {(data.title || '').toLowerCase()}
          </H2>

          <p className="govuk-body">
            These are some key indicators for {(data.title || '').toLowerCase()}
            . You can change what you see here according to your requirements.
          </p>
          <hr className="govuk-section-break govuk-section-break--xl govuk-section-break--visible" />
        </section>

        <section id="explore-statistics">
          <H2>
            Explore {(data.title || '').toLowerCase()} statistics
          </H2>
          <ul className="govuk-list govuk-list--bullet">
            <li>
              You can explore all the DfE statistics available for{' '}
              {(data.title || '').toLowerCase()} here. You can use our step by
              step guide, or dive straight in.
            </li>
            {/* <li>Once you've chosen your data you can view it by ###.</li> */}
            <li>
              You can also dowload it, visualise it or copy and paste it as you
              need.
            </li>
          </ul>

          {/* <div className="govuk-form-group">
                        <fieldset className="govuk-fieldset">
                            <legend className="govuk-fieldset__legend govuk-fieldset__legend--m">
                                <h1 className="govuk-fieldset__heading">
                                    Want to explore more school stats? What do you want to do?
                                </h1>
                            </legend>
                            <div className="govuk-radios">
                                <div className="govuk-radios__item">
                                    <input className="govuk-radios__input" id="what-to-do-1" name="what-to-do" type="radio" value="see-how-stats-have-changed" />
                                    <label className="govuk-label govuk-radios__label" for="what-to-do-1">
                                        See how school stats have changed over time for <Glink>(choose)</Glink>
                                    </label>
                                </div>
                                <div className="govuk-radios__item">
                                    <input className="govuk-radios__input" id="what-to-do-2" name="what-to-do" type="radio" value="compare-stats-over-time" />
                                    <label className="govuk-label govuk-radios__label" for="what-to-do-2">
                                        Compare school stats over time to <Glink>(choose)</Glink>
                                    </label>
                                </div>
                                <div className="govuk-radios__item">
                                    <input className="govuk-radios__input" id="what-to-do-3" name="what-to-do" type="radio" value="compare-current-stats-with" />
                                    <label className="govuk-label govuk-radios__label" for="what-to-do-3">
                                        Compare school current stats to <Glink>(choose)</Glink>
                                    </label>
                                </div>
                                <div className="govuk-radios__item">
                                    <input className="govuk-radios__input" id="what-to-do-4" name="what-to-do" type="radio" value="find-specific-stats" />
                                    <label className="govuk-label govuk-radios__label" for="what-to-do-4">
                                        Find specific stats for <Glink>(choose)</Glink>
                                    </label>
                                </div>
                            </div>
                        </fieldset>
                    </div> */}
        </section>

        {/* <section id="find-and-compare-statistics">
                    <h2 className="govuk-heading-l">Find and compare stats for {(data.title || '').toLowerCase()}</h2>
                </section> */}
      </div>
    );
  }
}

export default ThemePage;
