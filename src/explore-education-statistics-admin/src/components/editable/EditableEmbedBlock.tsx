import EditableBlockWrapper from '@admin/components/editable/EditableBlockWrapper';
import EditableEmbedForm, {
  EditableEmbedFormValues,
} from '@admin/components/editable/EditableEmbedForm';
import Gate from '@common/components/Gate';
import EmbedBlock from '@common/modules/find-statistics/components/EmbedBlock';
import React from 'react';
import { EmbedBlock as EmbedBlockType } from '@common/services/types/blocks';
import Modal from '@common/components/Modal';
import useToggle from '@common/hooks/useToggle';
import { useConfig } from '@admin/contexts/ConfigContext';

interface Props {
  block: EmbedBlockType;
  editable: boolean;
  visible?: boolean;
  onDelete: () => void;
  onSubmit: (values: EditableEmbedFormValues) => void;
}

const EditableEmbedBlock = ({
  block,
  editable,
  visible = true,
  onDelete,
  onSubmit,
}: Props) => {
  const [showEmbedDashboardForm, toggleEmbedDashboardForm] = useToggle(false);

  const { PermittedEmbedUrlDomains } = useConfig();

  return (
    <>
      <EditableBlockWrapper
        onEmbedBlockEdit={editable ? toggleEmbedDashboardForm.on : undefined}
        onDelete={editable ? onDelete : undefined}
      >
        <Gate condition={!!visible}>
          <EmbedBlock
            title={block.title}
            url={block.url}
            permittedEmbedDomains={PermittedEmbedUrlDomains}
          />
        </Gate>
      </EditableBlockWrapper>

      <Modal
        title="Edit embedded URL"
        open={showEmbedDashboardForm}
        onExit={toggleEmbedDashboardForm.off}
      >
        <EditableEmbedForm
          initialValues={block}
          onCancel={toggleEmbedDashboardForm.off}
          onSubmit={async embedBlock => {
            await onSubmit(embedBlock);
            toggleEmbedDashboardForm.off();
          }}
        />
      </Modal>
    </>
  );
};

export default EditableEmbedBlock;
