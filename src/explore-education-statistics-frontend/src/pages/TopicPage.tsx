import React, { Component } from 'react';
import { match } from 'react-router';
import api from '../api';
import DataList from '../components/DataList';
import { H2, H3 } from '../components/Heading';
import Link from '../components/Link';
import Title from '../components/Title';

interface Props {
  match: match<{
    topic: string;
  }>;
}

interface State {
  data: {
    title: string;
  };
  publications: object[];
}

class TopicPage extends Component<Props, State> {
  public state = {
    data: {
      title: '',
    },
    publications: [],
  };

  public componentDidMount() {
    const { topic } = this.props.match.params;

    api
      .get(`topic/${topic}`)
      .then(json => this.setState({ data: json.data }))
      .catch(error => alert(error));
    api
      .get(`topic/${topic}/publications`)
      .then(json => this.setState({ publications: json.data }))
      .catch(error => alert(error));
  }

  public render() {
    const { data, publications } = this.state;

    return (
      <div>
        <div className="govuk-grid-row">
          <div className="govuk-grid-column-two-thirds">
            <Title label={data.title} />

            <p className="govuk-body">
              Here you can find DfE stats for {(data.title || '').toLowerCase()}
              , and access them as reports, customise and download as excel
              files or csv files, and access them via an API.{' '}
              <Link to="#">(Find out more)</Link>
            </p>
            <p className="govuk-body">
              You can also see our statistics for 16+ education and social care.
            </p>

            <H3>
              The following publications are available in{' '}
              {(data.title || '').toLowerCase()}
            </H3>
            <DataList
              data={publications}
              linkIdentifier={window.location.pathname}
            />
          </div>
        </div>
        <hr className="govuk-section-break govuk-section-break--xl govuk-section-break--visible" />

        <section id="latest-publications">
          <H2>
            Latest publications in {(data.title || '').toLowerCase()}
          </H2>
          <p className="govuk-body">
            These are the latest official statistics with figures in
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
              You can explore all the DfE statistics available for
              {(data.title || '').toLowerCase()} here. You can use our step by
              step guide, or dive straight in.
            </li>
            {/* <li>Once you've chosen your data you can view it by ###.</li> */}
            <li>
              You can also download it, visualise it or copy and paste it as you
              need.
            </li>
          </ul>
        </section>
      </div>
    );
  }
}

export default TopicPage;
