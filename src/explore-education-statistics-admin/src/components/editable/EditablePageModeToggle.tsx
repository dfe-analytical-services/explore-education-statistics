import { EditingMode, useEditingContext } from '@admin/contexts/EditingContext';
import styles from '@admin/components/editable/EditablePageModeToggle.module.scss';
import FormRadioGroup from '@common/components/form/FormRadioGroup';
import { useMobileMedia } from '@common/hooks/useMedia';
import useToggle from '@common/hooks/useToggle';
import React from 'react';
import classNames from 'classnames';

interface Props {
  canUpdateRelease?: boolean;
  previewLabel?: string;
  showTablePreviewOption?: boolean;
}

export default function EditablePageModeToggle({
  canUpdateRelease = true,
  previewLabel = 'Preview content',
  showTablePreviewOption = false,
}: Props) {
  const { editingMode, setEditingMode } = useEditingContext();
  const [isOpen, toggleOpen] = useToggle(true);
  const { isMedia: isMobileMedia } = useMobileMedia();

  const options = [
    ...(canUpdateRelease
      ? [
          {
            label: 'Edit content',
            value: 'edit',
          },
        ]
      : []),
    {
      label: previewLabel,
      value: 'preview',
    },

    ...(showTablePreviewOption
      ? [
          {
            label: 'Preview table tool',
            value: 'table-preview',
          },
        ]
      : []),
  ];

  return (
    <div className={styles.container}>
      {isMobileMedia && (
        <button
          type="button"
          id="pageViewToggleButton"
          onClick={toggleOpen}
          className={styles.button}
          aria-expanded={isOpen}
        >
          Change page view
          <span
            className={classNames('govuk-accordion-nav__chevron', {
              'govuk-accordion-nav__chevron--down': isOpen,
            })}
            aria-hidden
          />
        </button>
      )}

      <div
        aria-labelledby="pageViewToggleButton"
        className={classNames({
          'dfe-js-hidden': isMobileMedia && !isOpen,
        })}
      >
        <FormRadioGroup
          id="editingMode"
          inline
          name="editingMode"
          className={styles.fieldset}
          value={editingMode}
          legend="Change page view"
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
}
