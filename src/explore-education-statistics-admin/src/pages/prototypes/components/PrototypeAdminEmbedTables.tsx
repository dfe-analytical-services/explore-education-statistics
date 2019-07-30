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
    <>
      <Tabs id="tablePreviewTab">
        <TabsSection id="table-preview" title="Table">
          <div className="govuk-width-container">
            <PrototypeTableContent table={table} task={task} />
          </div>
        </TabsSection>
        <TabsSection id="add-chart" title="Chart">
          {Data && <ChartBuilder data={Data} />}
        </TabsSection>
      </Tabs>
      {task === 'selectTable' && (
        <a className="govuk-button" href="#">
          Embed selected data block in this section
        </a>
      )}
      {task === 'view' && (
        <>
          <FormGroup>
            <FormTextInput
              id="permalink"
              name="permalink"
              label="Permalink"
              hint="Copy this link to view a standalone version of this table. You can use this link to refer to your table within your commentary.
              "
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
          <a
            className="govuk-button govuk-!-margin-right-3"
            href="/prototypes/publication-create-new-absence-table?status=step5#table-builder"
          >
            Edit table
          </a>

          <Button variant="warning" onClick={() => toggleDeleteModal(true)}>
            Delete table
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
    </>
  );
};

export default PrototypeExampleTable;
