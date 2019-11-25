import React, { useState, useContext } from 'react';
import { FormikProps } from 'formik';
import {
  ManageContentPageViewModel,
  BasicLink,
} from '@admin/services/release/edit-release/content/types';
import Link from '@admin/components/Link';
import releaseContentService from '@admin/services/release/edit-release/content/service';
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
  relatedInformation: ManageContentPageViewModel['relatedInformation'];
}

const RelatedInformationSection = ({ relatedInformation, release }: Props) => {
  const [links, setLinks] = useState<BasicLink[]>(relatedInformation);
  const [formOpen, setFormOpen] = useState<boolean>(false);

  const { isEditing } = useContext(EditingContext);

  const addLink = (link: Omit<BasicLink, 'id'>) => {
    return new Promise(resolve => {
      releaseContentService.relatedInfo
        .create(release.id, link)
        .then((createdLink: BasicLink) => {
          setLinks([...links, createdLink]);
          resolve();
        });
    });
  };

  const removeLink = (linkId: string) => {
    releaseContentService.relatedInfo
      .delete(release.id, linkId)
      .then(() => setLinks(links.filter(({ id }) => id !== linkId)));
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
              <Link
                to="#"
                className="govuk-button govuk-button--secondary"
                onClick={() => {
                  form.resetForm();
                  setFormOpen(false);
                }}
              >
                Cancel
              </Link>
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
            <Link to={`/methodology/${release.publication.slug}`}>
              {`${release.publication.title}: methodology`}
            </Link>
          </li>
          {isEditing && <hr />}

          {relatedInformation.map(({ id, description, url }) => (
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

export default RelatedInformationSection;
