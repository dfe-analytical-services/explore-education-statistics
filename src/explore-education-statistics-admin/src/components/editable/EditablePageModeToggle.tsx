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
  const { activeSection, editingMode, setEditingMode } = useEditingContext();
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
        aria-labelledby={isMobileMedia ? 'pageViewToggleButton' : undefined}
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

            // Small timeout to ensure it's moved on to the next
            // event in the loop before scrolling. Otherwise it might
            // scroll while still switching mode.
            setTimeout(() => {
              // Add some spacing at the top so it's not covered by the
              // editable mode bar and there's a bit of space below it.
              const spacing = 150;
              const section = activeSection
                ? document.querySelector(
                    `#${activeSection}, [data-scroll="${activeSection}"]`,
                  )
                : undefined;

              if (section) {
                // Check if the target section is within an accordion
                const accordionSection = section.closest(
                  '.govuk-accordion__section-content',
                );
                const button =
                  accordionSection?.previousElementSibling?.querySelector(
                    'button',
                  );

                if (
                  button &&
                  !(button.getAttribute('aria-expanded') === 'true')
                ) {
                  // Expand the accordion section if it's not already open
                  button.dispatchEvent(
                    new MouseEvent('click', { bubbles: true }),
                  );
                }

                // Small timeout to allow accordion to expand before measuring position
                setTimeout(() => {
                  const top =
                    section.getBoundingClientRect().top +
                    document.documentElement.scrollTop;

                  window.scrollTo(0, top - spacing);
                }, 20);
              }
            }, 80);
          }}
        />
      </div>
    </div>
  );
}
