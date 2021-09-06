import ModalConfirm from '@common/components/ModalConfirm';
import React from 'react';
import { IdTitlePair } from '@admin/services/types/common';
import Link from '@admin/components/Link';
import { generatePath } from 'react-router';
import {
  MethodologyRouteParams,
  methodologyStatusRoute,
} from '@admin/routes/methodologyRoutes';

interface Props {
  onConfirm: () => void;
  onCancel: () => void;
  methodologiesScheduledWithRelease: IdTitlePair[];
}

const CancelAmendmentModal = ({
  onConfirm,
  onCancel,
  methodologiesScheduledWithRelease,
}: Props) => {
  return (
    <ModalConfirm
      title="Confirm you want to cancel this amended release"
      onConfirm={onConfirm}
      onExit={onCancel}
      onCancel={onCancel}
      open
    >
      <p>
        By cancelling the amendments you will lose any changes made, and the
        original release will remain unchanged.
      </p>
      {methodologiesScheduledWithRelease.length > 0 && (
        <>
          <p>
            The following methodologies are scheduled to be published with this
            amended release:
          </p>
          <ul>
            {methodologiesScheduledWithRelease.map(m => (
              <li key={m.id}>{m.title}</li>
            ))}
          </ul>
        </>
      )}
    </ModalConfirm>
  );
};

export default CancelAmendmentModal;
