import withErrorControl, {
  ErrorControlProps,
} from '@admin/validation/withErrorControl';
import ButtonText from '@common/components/ButtonText';
import { BasicLink } from '@common/services/publicationService';
import React, { useState, useContext } from 'react';
import { FormikProps } from 'formik';
import { ManageContentPageViewModel } from '@admin/services/release/edit-release/content/types';
import Link from '@admin/components/Link';
import { relatedInformationService } from '@admin/services/release/edit-release/content/service';
import { EditingContext } from '@common/modules/find-statistics/util/wrapEditableComponent';
import Button from '@common/components/Button';
import {
  Formik,
  FormFieldset,
  Form,
  FormFieldTextInput,
} from '@common/components/form';
import Yup from '@common/lib/validation/yup';
import EditableLink from './EditableLink';

interface Props {
  release: ManageContentPageViewModel['release'];
}

const RelatedInformationSection = ({
  release,
  handleApiErrors,
}: Props & ErrorControlProps) => {
  const [links, setLinks] = useState<BasicLink[]>(release.relatedInformation);
  const [formOpen, setFormOpen] = useState<boolean>(false);

  const { isEditing } = useContext(EditingContext);

  const addLink = (link: Omit<BasicLink, 'id'>) => {
    return new Promise(resolve => {
      relatedInformationService
        .create(release.id, link)
        .then(newLinks => {
          setLinks(newLinks);
          resolve();
        })
        .catch(handleApiErrors);
    });
  };

  const removeLink = (linkId: string) => {
    relatedInformationService
      .delete(release.id, linkId)
      .then(setLinks)
      .catch(handleApiErrors);
  };

  const renderLinkForm = () => {
    return !formOpen ? (
      <Button onClick={() => setFormOpen(true)}>Add related information</Button>
    ) : (
      <Formik<Omit<BasicLink, 'id'>>
        initialValues={{ description: '', url: '' }}
        validationSchema={Yup.object({
          description: Yup.string().required('Link title must be provided'),
          url: Yup.string()
            .url()
            .required('Link url must be provided'),
        })}
        onSubmit={link =>
          addLink(link).then(() => {
            setFormOpen(false);
          })
        }
        render={(form: FormikProps<Omit<BasicLink, 'id'>>) => {
          return (
            <Form {...form} id="create-new-link-form">
              <FormFieldset
                id="allFieldsFieldset"
                legend="Add related information"
                legendSize="m"
              >
                <FormFieldTextInput
                  id="title"
                  label="Title"
                  name="description"
                />
                <FormFieldTextInput id="link-url" label="Link url" name="url" />
              </FormFieldset>
              <Button
                type="submit"
                className="govuk-button govuk-!-margin-right-1"
              >
                Create link
              </Button>
              <ButtonText
                className="govuk-button govuk-button--secondary"
                onClick={() => {
                  form.resetForm();
                  setFormOpen(false);
                }}
              >
                Cancel
              </ButtonText>
            </Form>
          );
        }}
      />
    );
  };

  return (
    <>
      <h2 className="govuk-heading-m govuk-!-margin-top-6" id="related-content">
        Related guidance
      </h2>
      <nav role="navigation" aria-labelledby="related-content">
        <ul className="govuk-list">
          <li>
            <Link to="#">
              {release.publication.methodology
                ? release.publication.methodology.title
                : 'Methodology title'}
            </Link>
          </li>
          {isEditing && <hr />}

          {links.map(({ id, description, url }) => (
            <li key={id}>
              <EditableLink removeOnClick={() => removeLink(id)} to={url}>
                {description}
              </EditableLink>
            </li>
          ))}
        </ul>
      </nav>
      {isEditing && renderLinkForm()}
    </>
  );
};

export default withErrorControl(RelatedInformationSection);
