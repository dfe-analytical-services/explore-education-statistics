import Page from '@frontend/components/Page';
import PageTitle from '@frontend/components/PageTitle';
import React, { Component } from 'react';

class GlossaryIndexPage extends Component {
  public render() {
    return (
      <Page breadcrumbs={[{ name: 'Glossary' }]}>
        <PageTitle title="Education statistics: glossary" />
      </Page>
    );
  }
}

export default GlossaryIndexPage;
