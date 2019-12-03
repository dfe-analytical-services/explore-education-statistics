/* eslint-disable */
import PrototypePage from '@admin/pages/prototypes/components/PrototypePage';
import React from 'react';
import tableBuilderService, {
  ThemeMeta,
} from '@common/modules/full-table/services/tableBuilderService';
import TableToolWizard from '@common/modules/table-tool/components/TableToolWizard';

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
        <TableToolWizard publicationId={publicationId} themeMeta={themeMeta} />
      )}
    </PrototypePage>
  );
};

export default PrototypeTableTool;
