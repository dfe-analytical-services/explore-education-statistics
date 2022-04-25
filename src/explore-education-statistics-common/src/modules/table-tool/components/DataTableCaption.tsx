import ButtonText from '@common/components/ButtonText';
import useToggle from '@common/hooks/useToggle';
import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';
import generateTableTitle from '@common/modules/table-tool/utils/generateTableTitle';
import React from 'react';

interface Props extends FullTableMeta {
  id?: string;
  title?: string;
}

const DataTableCaption = ({ id, title, ...meta }: Props) => {
  const { locations, filters } = meta;

  const [expanded, toggleExpanded] = useToggle(false);

  const caption = generateTableTitle({
    ...meta,
    expanded,
  });

  return (
    <>
      <strong id={id} data-testid="dataTableCaption">
        {title || caption}
      </strong>
      {(locations.length > 5 ||
        Object.values(filters).flatMap(group => group.options).length > 5) && (
        <ButtonText
          className="govuk-!-display-block govuk-!-margin-top-2"
          onClick={toggleExpanded}
        >
          {`${expanded ? 'Hide' : 'View'} full table title`}
        </ButtonText>
      )}
    </>
  );
};

export default DataTableCaption;
