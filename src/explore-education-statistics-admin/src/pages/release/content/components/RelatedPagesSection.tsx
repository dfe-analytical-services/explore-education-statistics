import styles from '@admin/components/editable/EditableAccordionSection.module.scss';
import EditableLink from '@admin/components/editable/EditableLink';
import { useEditingContext } from '@admin/contexts/EditingContext';
import releaseContentRelatedInformationService from '@admin/services/releaseContentRelatedInformationService';
import { EditableRelease } from '@admin/services/releaseContentService';
import Button from '@common/components/Button';
import { FormFieldset } from '@common/components/form';
import FormFieldTextInput from '@common/components/form/FormFieldTextInput';
import FormProvider from '@common/components/form/FormProvider';
import Form from '@common/components/form/Form';
import useToggle from '@common/hooks/useToggle';
import ButtonGroup from '@common/components/ButtonGroup';
import { BasicLink } from '@common/services/publicationService';
import Yup from '@common/validation/yup';
import React, { useState } from 'react';
import { DragDropContext, Droppable } from '@hello-pangea/dnd';
import DraggableItem from '@admin/components/DraggableItem';
import classNames from 'classnames';

type FormValues = Omit<BasicLink, 'id'>;

interface Props {
  release: EditableRelease;
}

export default function RelatedPagesSection({ release }: Props) {
  const [links, setLinks] = useState<BasicLink[]>(release.relatedInformation);
  const [formOpen, toggleFormOpen] = useToggle(false);

  const { editingMode } = useEditingContext();

  const removeLink = (linkId: string) => {
    releaseContentRelatedInformationService
      .delete(release.id, linkId)
      .then(setLinks);
  };

  const handleSubmit = async (values: FormValues) => {
    const newLinks = await releaseContentRelatedInformationService.create(
      release.id,
      values,
    );
    setLinks(newLinks);
    toggleFormOpen.off();
  };

  const [reorderingMode, setReorderingMode] = useState<boolean>(false);

  // https://github.com/hello-pangea/dnd?tab=readme-ov-file

  return (
    <>
      {(editingMode === 'edit' || links.length > 0) && (
        <>
          <h3 className="govuk-heading-s" id="related-pages">
            Related pages
          </h3>
          <nav role="navigation" aria-labelledby="related-content">
            <DragDropContext dragHandleUsageInstructions="test" onDragEnd={(a) => console.log(a)}>
              <Droppable droppableId="droppable-1">
                {(provided, snapshot) => (
                  <div
                    ref={provided.innerRef}
                    style={{ backgroundColor: snapshot.isDraggingOver ? 'blue' : 'grey' }}
                    // eslint-disable-next-line react/jsx-props-no-spreading
                    {...provided.droppableProps}
                  >
                    <ul className="govuk-list">
                      {links.map(({ id, description, url }, index) => (
                        <DraggableItem
                          className={classNames({
                            [styles.draggableItem]: editingMode === 'edit',
                          })}
                          id={id}
                          index={index}
                          isDisabled={editingMode !== 'edit'}
                          isReordering={true}
                        >
                          <li>
                            <EditableLink removeOnClick={() => removeLink(id)} to={url}>
                              {description}
                            </EditableLink>
                          </li>
                        </DraggableItem>
                      ))}
                    </ul>
                    {provided.placeholder}
                  </div>
                )}
              </Droppable>
            </DragDropContext>
          </nav>
        </>
      )}
      {editingMode === 'edit' && (
        <>
          {formOpen ? (
            <FormProvider
              initialValues={{ description: '', url: '' }}
              validationSchema={Yup.object({
                description: Yup.string().required('Enter a link title'),
                url: Yup.string()
                  .url('Enter a valid link URL')
                  .required('Enter a link URL'),
              })}
            >
              <Form id="relatedPageForm" onSubmit={handleSubmit}>
                <FormFieldset
                  id="relatedLink"
                  legend="Add related page link"
                  legendSize="m"
                >
                  <FormFieldTextInput<FormValues>
                    label="Title"
                    name="description"
                  />
                  <FormFieldTextInput<FormValues> label="Link URL" name="url" />
                </FormFieldset>
                <ButtonGroup>
                  <Button
                    type="submit"
                    className="govuk-button govuk-!-margin-right-1"
                  >
                    Create link
                  </Button>
                  <Button
                    className="govuk-button govuk-button--secondary"
                    variant="secondary"
                    onClick={toggleFormOpen.off}
                  >
                    Cancel
                  </Button>
                </ButtonGroup>
              </Form>
            </FormProvider>
          ) : (
            <Button onClick={toggleFormOpen.on}>Add related page link</Button>
          )}
        </>
      )}
    </>
  );
}
