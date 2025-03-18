import { useEditingContext } from '@admin/contexts/EditingContext';
import AccordionSection, {
  AccordionSectionProps,
} from '@common/components/AccordionSection';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import { FormTextInput } from '@common/components/form';
import ModalConfirm from '@common/components/ModalConfirm';
import Tooltip from '@common/components/Tooltip';
import useToggle from '@common/hooks/useToggle';
import React, { ReactNode, useCallback, useMemo, useState } from 'react';

export interface EditableAccordionSectionProps extends AccordionSectionProps {
  id: string;
  headerButtons?: ReactNode;
  disabledHeadingChangeTooltip?: string;
  disabledRemoveSectionTooltip?: string;
  isReordering?: boolean;
  onHeadingChange: (heading: string) => void;
  onRemoveSection?: () => void;
}

const EditableAccordionSection = (props: EditableAccordionSectionProps) => {
  const {
    children,
    disabledHeadingChangeTooltip,
    disabledRemoveSectionTooltip,
    heading,
    headerButtons,
    id,
    isReordering,
    onHeadingChange,
    onRemoveSection,
  } = props;

  const { editingMode } = useEditingContext();

  const [isEditingHeading, toggleEditingHeading] = useToggle(false);

  const [newHeading, setNewHeading] = useState(heading);

  const saveHeading = useCallback(async () => {
    if (isEditingHeading && onHeadingChange && newHeading !== heading) {
      await onHeadingChange(newHeading);
    }

    toggleEditingHeading.off();
  }, [
    heading,
    isEditingHeading,
    newHeading,
    onHeadingChange,
    toggleEditingHeading,
  ]);

  const header: ReactNode = useMemo(() => {
    if (isEditingHeading) {
      return (
        <FormTextInput
          id={`${id}-editHeading`}
          name="heading"
          label="Edit Heading"
          autoFocus
          value={newHeading}
          onChange={e => {
            setNewHeading(e.target.value);
          }}
          onClick={e => {
            e.stopPropagation();
          }}
          onKeyPress={async e => {
            switch (e.key) {
              case 'Enter':
                await saveHeading();
                break;
              case 'Esc':
                toggleEditingHeading.off();
                break;
              default:
                break;
            }
          }}
        />
      );
    }

    return undefined;
  }, [id, isEditingHeading, newHeading, saveHeading, toggleEditingHeading]);

  if (isReordering) {
    return heading;
  }

  return (
    <div data-testid="editableAccordionSection">
      <AccordionSection
        {...props}
        id={id}
        heading={heading}
        header={header}
        trackScroll
      >
        {sectionProps => (
          <>
            {editingMode === 'edit' && (
              <ButtonGroup>
                {isEditingHeading ? (
                  <Button onClick={saveHeading}>Save section title</Button>
                ) : (
                  <Tooltip
                    text={disabledHeadingChangeTooltip}
                    enabled={!!disabledHeadingChangeTooltip}
                  >
                    {({ ref }) => (
                      <Button
                        ariaDisabled={!!disabledHeadingChangeTooltip}
                        type="button"
                        ref={ref}
                        variant="secondary"
                        onClick={toggleEditingHeading}
                      >
                        Edit section title
                      </Button>
                    )}
                  </Tooltip>
                )}

                {headerButtons}

                {onRemoveSection && (
                  <Tooltip
                    text={disabledRemoveSectionTooltip}
                    enabled={!!disabledRemoveSectionTooltip}
                  >
                    {({ ref }) => (
                      <ModalConfirm
                        title="Removing section"
                        triggerButton={
                          <Button
                            ariaDisabled={!!disabledRemoveSectionTooltip}
                            ref={ref}
                            variant="warning"
                          >
                            Remove this section
                          </Button>
                        }
                        onConfirm={onRemoveSection}
                      >
                        <p>
                          Are you sure you want to remove the following section?
                          <br />
                          <strong>"{heading}"</strong>
                        </p>
                      </ModalConfirm>
                    )}
                  </Tooltip>
                )}
              </ButtonGroup>
            )}

            {typeof children === 'function' ? children(sectionProps) : children}
          </>
        )}
      </AccordionSection>
    </div>
  );
};

export default EditableAccordionSection;
