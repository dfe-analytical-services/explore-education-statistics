import React, { Component } from 'react';
import { Helmet } from 'react-helmet';
import { RouteComponentProps } from 'react-router';
import { contentApi } from '../../services/api';
import ThemeList from './components/ThemeList';

interface Props extends RouteComponentProps<{}> {}

interface State {
  items: any[];
}

class FindStatisticsPage extends Component<Props, State> {
  public state = {
    items: [],
  };

  public componentDidMount() {
    contentApi
      .get('theme')
      .then(({ data }) => this.setState({ items: data }))
      .catch(error => alert(error));
  }

  public render() {
    const { items } = this.state;

    return (
        <>
        <Helmet>
          <title>Find statistics and data - GOV.UK</title>
        </Helmet>
          <h1 className="govuk-heading-xl">Find statistics and data</h1>
          <p className="govuk-body-l">
            Browse to find the statistics and data you’re looking for and open
            the section to get links to:
          </p>
          <ul className="govuk-bulllet-list govuk-!-margin-bottom-9">
            <li>
              up-to-date national statistical headlines, breakdowns and
              explanations
            </li>
            <li>
              charts and tables to help you compare, contrast and view national
              and regional statistical data and trends
            </li>
          </ul>
          <ThemeList
            items={items}
            linkIdentifier={this.props.match.url}
          />
        </>
    );
  }
}

export default FindStatisticsPage;
