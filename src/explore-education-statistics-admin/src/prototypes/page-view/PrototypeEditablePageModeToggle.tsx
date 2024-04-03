import { EditingMode, useEditingContext } from '@admin/contexts/EditingContext';
import FormRadioGroup from '@common/components/form/FormRadioGroup';
import useToggle from '@common/hooks/useToggle';
import { useMobileMedia } from '@common/hooks/useMedia';
import React from 'react';
import classNames from 'classnames';
import styles from './PrototypeEditablePageModeToggle.module.scss';

interface Props {
  canUpdateRelease?: boolean;
  previewLabel?: string;
  showTablePreviewOption?: boolean;
}
const PrototypeEditablePageModeToggle = ({
  canUpdateRelease = true,
  previewLabel = 'Preview content',
  showTablePreviewOption = false,
}: Props) => {
  const { editingMode, setEditingMode } = useEditingContext();
  const [isOpen, toggleOpen] = useToggle(true);
  const { isMedia: isMobileMedia } = useMobileMedia();

  const previewOption = {
    label: previewLabel,
    value: 'preview',
  };

  const options = [previewOption];

  if (canUpdateRelease) {
    options.push({
      label: 'Edit content',
      value: 'edit',
    });
  }

  if (showTablePreviewOption) {
    options.push({
      label: 'Preview table tool',
      value: 'table-preview',
    });
  }

  return (
    <div
      className={classNames(styles.container, {
        [styles.open]: !isMobileMedia || isOpen,
      })}
    >
      {isMobileMedia && (
        <button
          type="button"
          id="pageViewToggleButton"
          onClick={toggleOpen}
          className={styles.button}
          aria-expanded={isOpen}
        >
          Set page view
          <span
            className={classNames('govuk-accordion-nav__chevron', {
              'govuk-accordion-nav__chevron--down': isOpen,
            })}
            aria-hidden
          />
        </button>
      )}

      <div aria-labelledby="pageViewToggleButton" className={styles.content}>
        <FormRadioGroup
          id="editingMode"
          inline
          name="editingMode"
          className={styles.fieldset}
          value={editingMode}
          legend="Page view"
          legendHidden={isMobileMedia}
          legendSize="s"
          small
          options={options}
          onChange={event => {
            setEditingMode(event.target.value as EditingMode);
          }}
        />
      </div>
    </div>
  );
};

export default PrototypeEditablePageModeToggle;
