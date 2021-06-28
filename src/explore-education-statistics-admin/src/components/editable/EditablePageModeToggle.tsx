import { useEditingContext } from '@admin/contexts/EditingContext';
import FormRadioGroup from '@common/components/form/FormRadioGroup';
import useToggle from '@common/hooks/useToggle';
import React, { useState } from 'react';
import classNames from 'classnames';
import styles from './EditablePageModeToggle.module.scss';

const EditablePageModeToggle = () => {
  const { setEditing } = useEditingContext();
  const { setTablePreview } = useEditingContext();
  const [isOpen, toggleOpen] = useToggle(true);
  const [pageMode, setPageMode] = useState<string>('edit');

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
          value={pageMode}
          legend="Set page view"
          legendHidden
          small
          options={[
            {
              label: 'Add / view comments and edit content',
              value: 'edit',
            },
            {
              label: 'Preview release page',
              value: 'preview',
            },
            {
              label: 'Preview table tool',
              value: 'table',
            },
          ]}
          onChange={event => {
            setPageMode(event.target.value);
            setEditing(event.target.value === 'edit');
            setTablePreview(event.target.value === 'table');
          }}
        />
      </div>
    </div>
  );
};

export default EditablePageModeToggle;
