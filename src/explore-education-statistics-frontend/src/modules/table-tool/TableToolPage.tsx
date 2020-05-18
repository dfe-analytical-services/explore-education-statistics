import tableBuilderService, {
  ThemeMeta,
} from '@common/services/tableBuilderService';
import Page from '@frontend/components/Page';
import TableTool from '@frontend/modules/table-tool/components/TableTool';
import { GetServerSideProps, NextPage } from 'next';
import React from 'react';

interface Props {
  themeMeta: ThemeMeta[];
  publicationId: string;
}

const TableToolPage: NextPage<Props> = ({ themeMeta, publicationId }) => {
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
};

export const getServerSideProps: GetServerSideProps<Props> = async ({
  query,
}) => {
  const themeMeta = await tableBuilderService.getThemes();

  const publication = themeMeta
    .flatMap(option => option.topics)
    .flatMap(option => option.publications)
    .find(option => option.slug === query.publicationSlug);

  return {
    props: {
      themeMeta,
      publicationId: publication ? publication.id : '',
    },
  };
};

export default TableToolPage;
