import React, { Component } from 'react';
import { RouteComponentProps } from 'react-router';
import api from '../api';
import ContentItemList from '../components/ContentItemList';
import PageHeading from '../components/PageHeading';

interface Props extends RouteComponentProps<{}> {}

interface State {
  items: any[];
}

class ThemesPage extends Component<Props, State> {
  public state = {
    items: [],
  };

  public componentDidMount() {
    api
      .get('theme')
      .then(({ data }) => this.setState({ items: data }))
      .catch(error => alert(error));
  }

  public render() {
    const { items } = this.state;

    return (
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-two-thirds">
          <PageHeading caption="Themes" heading="Find themes" />

          <ContentItemList
            items={items}
            linkIdentifier={this.props.match.url}
          />
        </div>
      </div>
    );
  }
}

export default ThemesPage;
