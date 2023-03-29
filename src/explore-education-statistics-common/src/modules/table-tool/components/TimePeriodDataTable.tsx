import WarningMessage from '@common/components/WarningMessage';
import logger from '@common/services/logger';
import isErrorLike from '@common/utils/error/isErrorLike';
import { FullTable } from '@common/modules/table-tool/types/fullTable';
import { TableHeadersConfig } from '@common/modules/table-tool/types/tableHeaders';
import { ReleaseTableDataQuery } from '@common/services/tableBuilderService';
import React, { forwardRef, memo } from 'react';
import DataTableCaption from '@common/modules/table-tool/components/DataTableCaption';
import FixedMultiHeaderDataTable from '@common/modules/table-tool/components/FixedMultiHeaderDataTable';
import mapTableToJson from '@common/modules/table-tool/utils/mapTableToJson';

interface Props {
  captionTitle?: string;
  dataBlockId?: string;
  footnotesClassName?: string;
  fullTable: FullTable;
  query?: ReleaseTableDataQuery;
  source?: string;
  tableHeadersConfig: TableHeadersConfig;
  onError?: (message: string) => void;
}

const TimePeriodDataTable = forwardRef<HTMLElement, Props>(
  function TimePeriodDataTable(
    {
      captionTitle,
      dataBlockId,
      footnotesClassName,
      fullTable,
      query,
      source,
      tableHeadersConfig,
      onError,
    }: Props,
    dataTableRef,
  ) {
    try {
      const { subjectMeta, results } = fullTable;

      if (results.length === 0) {
        return (
          <WarningMessage>
            A table could not be returned. There is no data for the options
            selected.
          </WarningMessage>
        );
      }

      const { tableJson, hasMissingRowsOrColumns } = mapTableToJson({
        tableHeadersConfig,
        subjectMeta,
        results,
        query,
      });

      const captionId = dataBlockId
        ? `dataTableCaption-${dataBlockId}`
        : 'dataTableCaption';

      return (
        <>
          {hasMissingRowsOrColumns && (
            <WarningMessage testId="missing-data-warning">
              Some rows and columns are not shown in this table as the data does
              not exist in the underlying file.
            </WarningMessage>
          )}
          <FixedMultiHeaderDataTable
            caption={
              <DataTableCaption
                title={captionTitle}
                meta={subjectMeta}
                id={captionId}
              />
            }
            tableJson={tableJson}
            captionId={captionId}
            footnotesClassName={footnotesClassName}
            footnotesId={
              dataBlockId
                ? `dataTableFootnotes-${dataBlockId}`
                : 'dataTableFootnotes'
            }
            ref={dataTableRef}
            footnotes={subjectMeta.footnotes}
            source={source}
            footnotesHeadingHiddenText={`for ${captionTitle}`}
          />
        </>
      );
    } catch (error) {
      logger.error(error);

      onError?.(isErrorLike(error) ? error.message : 'Unknown error');

      return (
        <WarningMessage testId="table-error">
          There was a problem rendering the table.
        </WarningMessage>
      );
    }
  },
);

TimePeriodDataTable.displayName = 'TimePeriodDataTable';

export default memo(TimePeriodDataTable);
