import styles from '@admin/components/editable/EditableAccordionSection.module.scss';
import { useEditingContext } from '@admin/contexts/EditingContext';
import AccordionSection, {
  accordionSectionClasses,
  AccordionSectionProps,
} from '@common/components/AccordionSection';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
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

  const { editingMode } = useEditingContext();

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
    id,
    isEditingHeading,
    isReordering,
    newHeading,
    saveHeading,
    toggleEditingHeading,
  ]);

  if (editingMode !== 'edit') {
    return <AccordionSection {...props} />;
  }

  return (
    <Draggable draggableId={id} isDragDisabled={!isReordering} index={index}>
      {(draggableProvided, snapshot) => (
        <div
          // eslint-disable-next-line react/jsx-props-no-spreading
          {...draggableProvided.draggableProps}
          // eslint-disable-next-line react/jsx-props-no-spreading
          {...draggableProvided.dragHandleProps}
          ref={draggableProvided.innerRef}
          className={classNames({
            [styles.dragContainer]: isReordering,
            [styles.isDragging]: snapshot.isDragging,
          })}
          data-testid="editableAccordionSection"
        >
          <AccordionSection
            {...props}
            id={id}
            heading={heading}
            header={header}
          >
            {sectionProps => (
              <>
                <ButtonGroup>
                  {isEditingHeading ? (
                    <Button onClick={saveHeading}>Save section title</Button>
                  ) : (
                    <Button
                      type="button"
                      onClick={toggleEditingHeading}
                      variant="secondary"
                    >
                      Edit section title
                    </Button>
                  )}

                  {headerButtons}

                  {onRemoveSection && (
                    <>
                      <Button onClick={toggleRemoveModal.on} variant="warning">
                        Remove this section
                      </Button>

                      <ModalConfirm
                        title="Are you sure?"
                        open={showRemoveModal}
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
                </ButtonGroup>

                {typeof children === 'function'
                  ? children(sectionProps)
                  : children}
              </>
            )}
          </AccordionSection>
        </div>
      )}
    </Draggable>
  );
};

export default EditableAccordionSection;
