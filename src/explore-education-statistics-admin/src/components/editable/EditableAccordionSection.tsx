import styles from '@admin/components/editable/EditableAccordionSection.module.scss';
import { useEditingContext } from '@admin/contexts/EditingContext';
import AccordionSection, {
  accordionSectionClasses,
  AccordionSectionProps,
} from '@common/components/AccordionSection';
import Button from '@common/components/Button';
import { FormTextInput } from '@common/components/form';
import ModalConfirm from '@common/components/ModalConfirm';
import useToggle from '@common/hooks/useToggle';
import classNames from 'classnames';
import React, {
  createElement,
  ReactNode,
  useCallback,
  useMemo,
  useState,
} from 'react';
import { Draggable } from 'react-beautiful-dnd';

export interface DraggableAccordionSectionProps {
  index: number;
  isReordering: boolean;
}

export interface EditableAccordionSectionProps extends AccordionSectionProps {
  id: string;
  headerButtons?: ReactNode;
  onHeadingChange: (heading: string) => void;
  onRemoveSection?: () => void;
}

const EditableAccordionSection = (props: EditableAccordionSectionProps) => {
  const {
    children,
    heading,
    id,
    onHeadingChange,
    onRemoveSection,
    headerButtons,
    headingTag = 'h2',
    ...restProps
  } = props;

  const { index, isReordering } = restProps as DraggableAccordionSectionProps;

  const { isEditing } = useEditingContext();

  const [showRemoveModal, toggleRemoveModal] = useToggle(false);
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

  const header = useMemo(() => {
    if (isEditingHeading) {
      return (
        <FormTextInput
          id="heading"
          name="heading"
          label="Edit Heading"
          defaultValue={newHeading}
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

    if (isReordering) {
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
    heading,
    headingTag,
    isEditingHeading,
    isReordering,
    newHeading,
    saveHeading,
    toggleEditingHeading,
  ]);

  if (!isEditing) {
    return <AccordionSection {...props} />;
  }

  return (
    <Draggable draggableId={id} isDragDisabled={!isReordering} index={index}>
      {(draggableProvided, snapshot) => (
        <div
          {...draggableProvided.draggableProps}
          {...draggableProvided.dragHandleProps}
          ref={draggableProvided.innerRef}
          className={classNames({
            [styles.dragContainer]: isReordering,
            [styles.isDragging]: snapshot.isDragging,
          })}
          data-testid="EditableAccordionSection"
        >
          <AccordionSection
            {...props}
            id={id}
            heading={heading}
            header={header}
          >
            <div>
              {onHeadingChange &&
                (isEditingHeading ? (
                  <Button onClick={saveHeading}>Save section title</Button>
                ) : (
                  <Button
                    type="button"
                    onClick={toggleEditingHeading}
                    variant="secondary"
                  >
                    Edit section title
                  </Button>
                ))}

              {headerButtons}

              {onRemoveSection && (
                <>
                  <Button onClick={toggleRemoveModal.on} variant="warning">
                    Remove this section
                  </Button>

                  <ModalConfirm
                    title="Are you sure?"
                    mounted={showRemoveModal}
                    onConfirm={onRemoveSection}
                    onExit={toggleRemoveModal.off}
                    onCancel={toggleRemoveModal.off}
                  >
                    <p>
                      Are you sure you want to remove the following section?
                      <br />
                      <strong>"{heading}"</strong>
                    </p>
                  </ModalConfirm>
                </>
              )}
            </div>

            {children}
          </AccordionSection>
        </div>
      )}
    </Draggable>
  );
};

export default EditableAccordionSection;
