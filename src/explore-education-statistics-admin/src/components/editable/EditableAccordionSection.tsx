import styles from '@admin/components/editable/EditableAccordionSection.module.scss';
import DraggableItem from '@admin/components/DraggableItem';
import { useEditingContext } from '@admin/contexts/EditingContext';
import AccordionSection, {
  accordionSectionClasses,
  AccordionSectionProps,
} from '@common/components/AccordionSection';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import { FormTextInput } from '@common/components/form';
import ModalConfirm from '@common/components/ModalConfirm';
import Tooltip from '@common/components/Tooltip';
import useToggle from '@common/hooks/useToggle';
import classNames from 'classnames';
import React, {
  createElement,
  ReactNode,
  useCallback,
  useMemo,
  useState,
} from 'react';

export interface DraggableAccordionSectionProps {
  index: number;
  isReordering: boolean;
}

export interface EditableAccordionSectionProps extends AccordionSectionProps {
  id: string;
  headerButtons?: ReactNode;
  disabledHeadingChangeTooltip?: string;
  disabledRemoveSectionTooltip?: string;
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
    headingTag = 'h2',
    id,
    onHeadingChange,
    onRemoveSection,
    ...restProps
  } = props;

  const { index, isReordering } = restProps as DraggableAccordionSectionProps;

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

    if (isReordering && editingMode === 'edit') {
      return createElement(
        headingTag,
        {
          className: accordionSectionClasses.sectionHeading,
        },
        heading,
      );
    }

    return undefined;
  }, [
    editingMode,
    heading,
    headingTag,
    id,
    isEditingHeading,
    isReordering,
    newHeading,
    saveHeading,
    toggleEditingHeading,
  ]);

  return (
    <DraggableItem
      className={classNames({
        [styles.draggableItem]: isReordering && editingMode === 'edit',
      })}
      id={id}
      index={index}
      isDisabled={editingMode !== 'edit'}
      isReordering={isReordering && editingMode === 'edit'}
      testId="editableAccordionSection"
    >
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
    </DraggableItem>
  );
};

export default EditableAccordionSection;
