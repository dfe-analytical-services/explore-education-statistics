import {
  useEditingContext,
  EditingMode,
} from '@admin/contexts/editing/EditingContext';
import useEditingActions from '@admin/contexts/editing/useEditingActions';
import FormRadioGroup from '@common/components/form/FormRadioGroup';
import useToggle from '@common/hooks/useToggle';
import React from 'react';
import classNames from 'classnames';
import styles from './EditablePageModeToggle.module.scss';

interface Props {
  previewLabel?: string;
  showTablePreviewOption?: boolean;
}
const EditablePageModeToggle = ({
  previewLabel = 'Preview content',
  showTablePreviewOption = false,
}: Props) => {
  const { editingMode } = useEditingContext();
  const actions = useEditingActions();
  const [isOpen, toggleOpen] = useToggle(true);

  const options = [
    {
      label: 'Edit content',
      value: 'edit',
    },
    {
      label: previewLabel,
      value: 'preview',
    },
  ];
  if (showTablePreviewOption) {
    options.push({
      label: 'Preview table tool',
      value: 'table-preview',
    });
  }

  return (
    <div
      className={classNames(styles.toggle, {
        [styles.open]: isOpen,
      })}
    >
      <button
        type="button"
        id="pageViewToggleButton"
        onClick={toggleOpen}
        className={styles.button}
        aria-expanded={isOpen}
      >
        Set page view
        <span className="govuk-accordion__icon" aria-hidden />
      </button>
      <div aria-labelledby="pageViewToggleButton" className={styles.content}>
        <FormRadioGroup
          id="editingMode"
          name="editingMode"
          className={styles.fieldset}
          value={editingMode}
          legend="Set page view"
          legendHidden
          small
          options={options}
          onChange={event => {
            actions.setEditingMode(event.target.value as EditingMode);
          }}
        />
      </div>
    </div>
  );
};

export default EditablePageModeToggle;
