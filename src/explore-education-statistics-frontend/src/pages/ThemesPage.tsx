import React, { Component } from 'react';
import api from '../api';
import DataList from '../components/DataList';
import PageHeading from '../components/PageHeading';

interface State {
  data: any[];
}

class ThemesPage extends Component<{}, State> {
  public state = {
    data: [],
  };

  public componentDidMount() {
    api
      .get('theme')
      .then(({ data }) => this.setState({ data }))
      .catch(error => alert(error));
  }

  public render() {
    const { data } = this.state;

    return (
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-two-thirds">
          <PageHeading caption="Themes" heading="Find themes" />

          <DataList data={data} linkIdentifier={window.location.pathname} />
        </div>
      </div>
    );
  }
}

export default ThemesPage;
