/* eslint-disable @typescript-eslint/no-unused-vars */
import React from 'react';
import { FormGroup, FormTextInput } from '@common/components/form';
import Button from '@common/components/Button';
import ModalConfirm from '@common/components/ModalConfirm';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import useToggle from '@common/hooks/useToggle';
import PrototypeTableContent from '@admin/pages/prototypes/components/PrototypeTableContent';
import ChartBuilder from '@admin/modules/chart-builder/ChartBuilder';
import DataBlockService, {
  DataBlockResponse,
  GeographicLevel,
} from '@common/services/dataBlockService';
import Link from '@admin/components/Link';
import PrototypeData, { PrototypeTable } from '../PrototypeData';

interface Props {
  // tableId?: string;
  task?: string;
  table?: PrototypeTable;
}

const PrototypeExampleTable = ({ task, table }: Props) => {
  const [showEditModal, toggleEditModal] = useToggle(false);
  const [showDeleteModal, toggleDeleteModal] = useToggle(false);

  const [Data, updateData] = React.useState<DataBlockResponse>(
    PrototypeData.testResponse,
  );

  /*
  React.useEffect(() => {
    // Temporary for now
    const fetchData = async () => {
      const newData = await DataBlockService.getDataBlockForSubject({
        subjectId: 1,
        startYear: '2012',
        endYear: '2016',
        filters: ['1', '71', '72', '73'],
        geographicLevel: GeographicLevel.National,
        indicators: ['23', '26', '28'],
      });
      updateData(newData);
    };

    fetchData();
  }, []);
   */

  return (
    <div className="govuk-width-container">
      <Tabs id="tablePreviewTab">
        <TabsSection id="table-preview" title="Table preview">
          <PrototypeTableContent table={table} task={task} />
        </TabsSection>
        <TabsSection id="add-chart" title="Add a chart">
          {Data && <ChartBuilder data={Data} />}
        </TabsSection>
      </Tabs>
      {task === 'view' && (
        <>
          <FormGroup>
            <FormTextInput
              id="permalink"
              name="permalink"
              label="Permalink"
              hint="Copy this URL to view a standalone verion of this table"
              defaultValue="http://dfe-url.gov.uk/example-permalink"
              width={20}
              onClick={e =>
                e.currentTarget.setSelectionRange(
                  0,
                  e.currentTarget.value.length,
                )
              }
            />
          </FormGroup>
          <Link
            className="govuk-button govuk-!-margin-right-3"
            to="/prototypes/publication-create-new-absence-table?status=step5"
          >
            Edit this table
          </Link>

          <Button variant="warning" onClick={() => toggleDeleteModal(true)}>
            Delete this table
          </Button>

          <ModalConfirm
            mounted={showDeleteModal}
            title="Confirm delete table"
            onExit={() => toggleDeleteModal(false)}
            onConfirm={() => toggleDeleteModal(false)}
            onCancel={() => toggleDeleteModal(false)}
          >
            <p>Are you sure you want to delete the table?</p>
          </ModalConfirm>
        </>
      )}
    </div>
  );
};

export default PrototypeExampleTable;
