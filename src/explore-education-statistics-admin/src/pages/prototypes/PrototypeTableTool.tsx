/* eslint-disable */
import PrototypePage from '@admin/pages/prototypes/components/PrototypePage';
import React from 'react';
import TableTool from '@common/modules/table-tool/components/TableTool';
import tableBuilderService, {
  TableDataQuery,
  ThemeMeta,
} from '@common/modules/full-table/services/tableBuilderService';

const PrototypeTableTool = () => {
  const publicationSlug = undefined; //"pupil-absence-in-schools-in-england";

  const [themeMeta, setThemeMeta] = React.useState<ThemeMeta[]>();
  const [publicationId, setPublicationId] = React.useState<string>();

  React.useEffect(() => {
    tableBuilderService.getThemes().then(themeMeta => {
      const publication = themeMeta
        .flatMap(option => option.topics)
        .flatMap(option => option.publications)
        .find(option => option.slug === publicationSlug);

      setThemeMeta(themeMeta);
      setPublicationId(publication ? publication.id : '');
    });
  }, []);

  return (
    <PrototypePage wide>
      {themeMeta && publicationId !== undefined && (
        <TableTool publicationId={publicationId} themeMeta={themeMeta} />
      )}
    </PrototypePage>
  );
};

export default PrototypeTableTool;
