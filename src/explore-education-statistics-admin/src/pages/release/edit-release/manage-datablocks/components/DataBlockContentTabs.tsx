import ChartBuilder from '@admin/pages/release/edit-release/manage-datablocks/components/ChartBuilder';
import { ReleaseDataBlock } from '@admin/services/release/edit-release/datablocks/service';
import LoadingSpinner from '@common/components/LoadingSpinner';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import TableHeadersForm from '@common/modules/table-tool/components/TableHeadersForm';
import TimePeriodDataTable from '@common/modules/table-tool/components/TimePeriodDataTable';
import { TableHeadersConfig } from '@common/modules/table-tool/types/tableHeaders';
import tableBuilderService, {
  TableDataResponse,
} from '@common/services/tableBuilderService';
import { FullTable } from '@common/modules/table-tool/types/fullTable';
import mapFullTable from '@common/modules/table-tool/utils/mapFullTable';
import mapTableHeadersConfig from '@common/modules/table-tool/utils/mapTableHeadersConfig';
import getDefaultTableHeaderConfig from '@common/modules/table-tool/utils/getDefaultTableHeadersConfig';
import { DataBlockRerequest } from '@common/services/dataBlockService';
import { Chart, DataBlockRequest } from '@common/services/types/blocks';
import React, { useEffect, useRef, useState } from 'react';

interface Props {
  dataBlock: ReleaseDataBlock;
  dataBlockResponse: TableDataResponse;
  releaseId: string;
  onDataBlockSave: (dataBlock: ReleaseDataBlock) => void;
}

const DataBlockContentTabs = ({
  dataBlock,
  dataBlockResponse,
  releaseId,
  onDataBlockSave,
}: Props) => {
  const dataTableRef = useRef<HTMLElement>(null);

  const [activeTab, setActiveTab] = useState<string>('');
  // we want to modify this internally as our own data, copying it
  const [chartBuilderData, setChartBuilderData] = useState<TableDataResponse>(
    () => {
      return { ...dataBlockResponse };
    },
  );

  // only update it if the external reference changes
  useEffect(() => {
    setChartBuilderData({ ...dataBlockResponse });
  }, [dataBlockResponse]);

  const [initialConfiguration, setInitialConfiguration] = useState<
    Chart | undefined
  >();

  useEffect(() => {
    if (dataBlock && dataBlock.charts) {
      setInitialConfiguration({
        ...dataBlock.charts[0],
      });
    }
  }, [dataBlock]);

  const [tableData, setTableData] = useState<{
    fullTable: FullTable;
    tableHeadersConfig: TableHeadersConfig;
  }>();

  useEffect(() => {
    const table = dataBlock.tables;
    const fullTable = mapFullTable(chartBuilderData);
    const tableHeadersConfig =
      (table?.length && table[0].tableHeaders) ||
      getDefaultTableHeaderConfig(fullTable.subjectMeta);

    setTableData({
      fullTable,
      tableHeadersConfig: mapTableHeadersConfig(
        tableHeadersConfig,
        fullTable.subjectMeta,
      ),
    });
  }, [dataBlock.tables, chartBuilderData]);

  const onChartSave = (chart: Chart) => {
    const newDataBlock: ReleaseDataBlock = {
      ...dataBlock,
      charts: [chart],
    };

    onDataBlockSave(newDataBlock);
  };

  const reRequestdata = (reRequest: DataBlockRerequest) => {
    const newRequest: DataBlockRequest = {
      ...dataBlock.dataBlockRequest,
      ...reRequest,
      includeGeoJson: false,
    };

    tableBuilderService.getTableData(newRequest).then(response => {
      if (response) {
        setChartBuilderData({ ...response });
      }
    });
  };

  return (
    <Tabs
      openId={activeTab}
      onToggle={tab => {
        setActiveTab(tab.id);
      }}
      id="editDataBlockSections"
    >
      {tableData && (
        <TabsSection title="Table">
          <TableHeadersForm
            initialValues={tableData?.tableHeadersConfig}
            id="dataBlockContentTabs-tableHeadersForm"
            onSubmit={async nextTableHeaders => {
              setTableData({
                ...tableData,
                tableHeadersConfig: nextTableHeaders,
              });

              if (dataBlock) {
                await onDataBlockSave({
                  ...dataBlock,
                  tables: [
                    {
                      tableHeaders: nextTableHeaders,
                      indicators: [],
                    },
                  ],
                });
              }

              if (dataTableRef.current) {
                dataTableRef.current.scrollIntoView({
                  behavior: 'smooth',
                  block: 'start',
                });
              }
            }}
          />

          <TimePeriodDataTable {...tableData} ref={dataTableRef} />
        </TabsSection>
      )}
      <TabsSection title="Create chart">
        {tableData?.fullTable ? (
          <div style={{ position: 'relative' }}>
            <ChartBuilder
              data={tableData?.fullTable.results}
              meta={{
                ...tableData?.fullTable.subjectMeta,
                // Don't render footnotes as they take
                // up too much screen space.
                footnotes: [],
              }}
              releaseId={releaseId}
              onChartSave={onChartSave}
              initialConfiguration={initialConfiguration}
              onRequiresDataUpdate={reRequestdata}
            />
          </div>
        ) : (
          <LoadingSpinner text="Creating chart" />
        )}
      </TabsSection>
    </Tabs>
  );
};

export default DataBlockContentTabs;
