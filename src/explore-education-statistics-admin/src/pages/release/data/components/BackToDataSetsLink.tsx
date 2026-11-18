import styles from '@admin/pages/release/data/components/BackToDataSetsLink.module.scss';
import ModalConfirm from '@common/components/ModalConfirm';
import classNames from 'classnames';
import React, { useState } from 'react';

interface Props {
  hasUnsavedChanges?: boolean;
  onBack: () => void;
}

/**
 * GOV.UK back link for leaving the Manage filters and indicators screens,
 * which switch views using state rather than routes. Asks to confirm first
 * if there are unsaved changes.
 */
export default function BackToDataSetsLink({
  hasUnsavedChanges = false,
  onBack,
}: Props) {
  const [showConfirm, setShowConfirm] = useState(false);

  return (
    <>
      <button
        type="button"
        className={classNames('govuk-back-link', styles.backLink)}
        onClick={() => {
          if (hasUnsavedChanges) {
            setShowConfirm(true);
          } else {
            onBack();
          }
        }}
      >
        Back to data sets
      </button>

      <ModalConfirm
        open={showConfirm}
        title="You have unsaved changes"
        confirmText="Discard changes"
        cancelText="Keep editing"
        onConfirm={onBack}
        onCancel={() => setShowConfirm(false)}
        onExit={() => setShowConfirm(false)}
      >
        <p>If you go back to the data sets, your changes will be lost.</p>
      </ModalConfirm>
    </>
  );
}
