import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';
import generateTableTitle from '@common/modules/table-tool/utils/generateTableTitle';
import React, { useMemo } from 'react';

interface Props {
  id?: string;
  title?: string;
  meta: FullTableMeta;
}

const DataTableCaption = ({ id, title, meta }: Props) => {
  const generatedTitle = useMemo<string>(
    () => (title ? '' : generateTableTitle(meta)),
    [meta, title],
  );

  return (
    <strong id={id} data-testid="dataTableCaption">
      {title || generatedTitle}
    </strong>
  );
};

export default DataTableCaption;
