import WarningMessage from '@common/components/WarningMessage';
import logger from '@common/services/logger';
import isErrorLike from '@common/utils/error/isErrorLike';
import { FullTable } from '@common/modules/table-tool/types/fullTable';
import { TableHeadersConfig } from '@common/modules/table-tool/types/tableHeaders';
import tableBuilderService, {
  ReleaseTableDataQuery,
} from '@common/services/tableBuilderService';
import DataTableCaption from '@common/modules/table-tool/components/DataTableCaption';
import FixedMultiHeaderDataTable from '@common/modules/table-tool/components/FixedMultiHeaderDataTable';
import mapTableToJson from '@common/modules/table-tool/utils/mapTableToJson';
import TableExportMenu from '@common/modules/find-statistics/components/TableExportMenu';
import React, { forwardRef, memo, ReactNode, RefObject } from 'react';

interface Props {
  captionTitle?: string;
  dataBlockId?: string;
  footnotesClassName?: string;
  footnotesHeadingHiddenText?: string;
  fullTable: FullTable;
  query?: ReleaseTableDataQuery;
  releaseVersionId?: string;
  source?: string;
  tableHeadersConfig: TableHeadersConfig;
  tableHeadersForm?: ReactNode;
  onError?: (message: string) => void;
}

const TimePeriodDataTable = forwardRef<HTMLElement, Props>(
  function TimePeriodDataTable(
    {
      captionTitle,
      dataBlockId,
      footnotesClassName,
      footnotesHeadingHiddenText,
      fullTable,
      query,
      releaseVersionId,
      source,
      tableHeadersConfig,
      tableHeadersForm,
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
          {fullTable.subjectMeta.isCroppedTable && (
            <WarningMessage testId="missing-data-warning">
              The selected options return too many rows to be displayed here and
              so the table shows only a subset of the data provided by your
              selections. To get the full set of relevant data, use the download
              options below to download in ODT or CSV format.
            </WarningMessage>
          )}
          {dataBlockId && query && (
            <TableExportMenu
              fileName={captionTitle}
              fullTable={fullTable}
              title={captionTitle}
              tableRef={dataTableRef as RefObject<HTMLElement | null>}
              onCsvDownload={() =>
                tableBuilderService.getTableCsv({ releaseVersionId, ...query })
              }
            />
          )}
          <FixedMultiHeaderDataTable
            caption={
              <DataTableCaption
                title={captionTitle}
                meta={subjectMeta}
                id={captionId}
              />
            }
            captionId={captionId}
            captionTitle={captionTitle}
            tableJson={tableJson}
            footnotesClassName={footnotesClassName}
            footnotesId={
              dataBlockId
                ? `dataTableFootnotes-${dataBlockId}`
                : 'dataTableFootnotes'
            }
            footnotesHeadingHiddenText={footnotesHeadingHiddenText}
            ref={dataTableRef}
            footnotes={subjectMeta.footnotes}
            source={source}
            tableHeadersForm={tableHeadersForm}
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
