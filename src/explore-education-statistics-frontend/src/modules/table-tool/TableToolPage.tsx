import tableBuilderService, {
  ThemeMeta,
} from '@common/modules/full-table/services/tableBuilderService';
import Page from '@frontend/components/Page';
import TableTool from '@frontend/components/TableTool';
import { NextContext } from 'next';
import React, { Component } from 'react';

export interface PublicationOptions {
  id: string;
  title: string;
  topics: {
    id: string;
    title: string;
    publications: {
      id: string;
      title: string;
      slug: string;
    }[];
  }[];
}

interface Props {
  themeMeta: ThemeMeta[];
  publicationId: string;
}

class TableToolPage extends Component<Props> {
  public static async getInitialProps({ query }: NextContext) {
    const themeMeta = await tableBuilderService.getThemes();

    const publication = themeMeta
      .flatMap(option => option.topics)
      .flatMap(option => option.publications)
      .find(option => option.slug === query.publicationSlug);

    return {
      themeMeta,
      publicationId: publication ? publication.id : '',
    };
  }

  public render() {
    const { themeMeta, publicationId } = this.props;
    return (
      <Page title="Create your own tables online" caption="Table Tool" wide>
        <p>
          Choose the data and area of interest you want to explore and then use
          filters to create your table.
        </p>

        <p>
          Once you've created your table, you can download the data it contains
          for your own offline analysis.
        </p>

        <TableTool themeMeta={themeMeta} publicationId={publicationId} />
      </Page>
    );
  }
}

export default TableToolPage;
