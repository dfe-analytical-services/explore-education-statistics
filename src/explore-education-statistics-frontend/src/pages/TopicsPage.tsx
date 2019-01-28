import React, { Component } from 'react';
import { Helmet } from 'react-helmet';
import { RouteComponentProps } from 'react-router';
import ContentItemList from '../components/ContentItemList';
import PageHeading from '../components/PageHeading';
import { contentApi } from '../services/api';

interface Props extends RouteComponentProps<{}> {}

interface State {
  items: any[];
}

class TopicsPage extends Component<Props, State> {
  public state = {
    items: [],
  };

  public componentDidMount() {
    contentApi
      .get('topic')
      .then(({ data }) => this.setState({ items: data }))
      .catch(error => alert(error));
  }

  public render() {
    const { items } = this.state;

    return (
      <div className="govuk-grid-row">
        <Helmet>
          <title>Topics - GOV.UK</title>
        </Helmet>
        <div className="govuk-grid-column-two-thirds">
          <PageHeading caption="Topics" heading="Find topics" />

          <ContentItemList
            items={items}
            linkIdentifier={this.props.match.url}
          />
        </div>
      </div>
    );
  }
}

export default TopicsPage;
