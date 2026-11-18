import styles from '@admin/pages/release/data/components/BackToDataFilesLink.module.scss';
import ModalConfirm from '@common/components/ModalConfirm';
import React from 'react';

interface Props {
  hasUnsavedChanges?: boolean;
  onBack: () => void;
}

/**
 * Back link styled like the GOV.UK one, for leaving the Manage filters and
 * indicators screens. These switch views using state rather than routes, so
 * it's a button. Asks to confirm first if there are unsaved changes.
 */
export default function BackToDataFilesLink({
  hasUnsavedChanges = false,
  onBack,
}: Props) {
  return (
    <ModalConfirm
      title="You have unsaved changes"
      confirmText="Discard changes"
      cancelText="Keep editing"
      // As the warning's trigger, the button gets focus back when the warning
      // is dismissed, however it's dismissed
      triggerButton={
        <button
          type="button"
          className={styles.backLink}
          onClick={event => {
            if (!hasUnsavedChanges) {
              // Stops the trigger opening the warning
              event.preventDefault();
              onBack();
            }
          }}
        >
          Back to data files
        </button>
      }
      onConfirm={onBack}
    >
      <p>If you go back to the data files, your changes will be lost.</p>
    </ModalConfirm>
  );
}
