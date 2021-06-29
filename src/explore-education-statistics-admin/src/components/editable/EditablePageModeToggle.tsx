import { useEditingContext } from '@admin/contexts/EditingContext';
import FormRadioGroup from '@common/components/form/FormRadioGroup';
import useToggle from '@common/hooks/useToggle';
import React from 'react';
import classNames from 'classnames';
import styles from './EditablePageModeToggle.module.scss';

const EditablePageModeToggle = () => {
  const { isEditing, setEditing } = useEditingContext();
  const [isOpen, toggleOpen] = useToggle(true);

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
          id="pageMode"
          name="pageMode"
          className={styles.fieldset}
          value={isEditing ? 'edit' : 'preview'}
          legend="Set page view"
          legendHidden
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
    </div>
  );
};

export default EditablePageModeToggle;
