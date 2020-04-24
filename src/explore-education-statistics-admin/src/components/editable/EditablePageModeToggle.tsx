import { useEditingContext } from '@admin/contexts/EditingContext';
import FormRadioGroup from '@common/components/form/FormRadioGroup';
import React from 'react';
import styles from './EditablePageModeToggle.module.scss';

const EditablePageModeToggle = () => {
  const { isEditing, setEditing } = useEditingContext();

  return (
    <div className={styles.toggle}>
      <FormRadioGroup
        id="pageMode"
        name="pageMode"
        className={styles.fieldset}
        value={isEditing ? 'edit' : 'preview'}
        legend="Set page view"
        small
        options={[
          {
            label: 'Add / view comments and edit content',
            value: 'edit',
          },
          {
            label: 'Preview content',
            value: 'preview',
          },
        ]}
        onChange={event => {
          setEditing(event.target.value === 'edit');
        }}
      />
    </div>
  );
};

export default EditablePageModeToggle;
