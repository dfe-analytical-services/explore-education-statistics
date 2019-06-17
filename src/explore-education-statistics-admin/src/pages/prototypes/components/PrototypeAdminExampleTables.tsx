import React from 'react';
import { FormGroup, FormTextInput } from '@common/components/form';
import Button from '@common/components/Button';
import ModalConfirm from '@common/components/ModalConfirm';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import useToggle from '@common/hooks/useToggle';
import PrototypeTableContent from '@admin/pages/prototypes/components/PrototypeTableContent';
import PrototypeChartEditor from '@admin/pages/prototypes/components/PrototypeChartEditor';
import Link from '../../../components/Link';

interface Props {
  // tableId?: string;
  task?: string;
}

const PrototypeExampleTable = ({ task }: Props) => {
  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  const [showEditModal, toggleEditModal] = useToggle(false);
  const [showDeleteModal, toggleDeleteModal] = useToggle(false);

  return (
    <div className="govuk-width-container">
      <Tabs>
        <TabsSection id="table-preview" title="Table preview">
          <PrototypeTableContent task={task} />
        </TabsSection>
        <TabsSection id="add-chart" title="Add a chart">
          <PrototypeChartEditor />
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
