import EditableLink from '@admin/components/editable/EditableLink';
import Link from '@admin/components/Link';
import { useEditingContext } from '@admin/contexts/EditingContext';
import releaseContentRelatedInformationService from '@admin/services/releaseContentRelatedInformationService';
import { EditableRelease } from '@admin/services/releaseContentService';
import Button from '@common/components/Button';
import ButtonText from '@common/components/ButtonText';
import {
  Form,
  FormFieldset,
  FormFieldTextInput,
} from '@common/components/form';
import { BasicLink } from '@common/services/publicationService';
import Yup from '@common/validation/yup';
import { Formik } from 'formik';
import React, { useState } from 'react';

interface Props {
  release: EditableRelease;
}

const RelatedInformationSection = ({ release }: Props) => {
  const [links, setLinks] = useState<BasicLink[]>(release.relatedInformation);
  const [formOpen, setFormOpen] = useState<boolean>(false);

  const { isEditing } = useEditingContext();

  const addLink = (link: Omit<BasicLink, 'id'>) => {
    return new Promise(resolve => {
      releaseContentRelatedInformationService
        .create(release.id, link)
        .then(newLinks => {
          setLinks(newLinks);
          resolve();
        });
    });
  };

  const removeLink = (linkId: string) => {
    releaseContentRelatedInformationService
      .delete(release.id, linkId)
      .then(setLinks);
  };

  const renderLinkForm = () => {
    return !formOpen ? (
      <Button onClick={() => setFormOpen(true)}>Add related information</Button>
    ) : (
      <Formik<Omit<BasicLink, 'id'>>
        initialValues={{ description: '', url: '' }}
        validationSchema={Yup.object({
          description: Yup.string().required('Enter a link title'),
          url: Yup.string()
            .url('Enter a valid link url')
            .required('Enter a link url'),
        })}
        onSubmit={link =>
          addLink(link).then(() => {
            setFormOpen(false);
          })
        }
      >
        {form => {
          return (
            <Form {...form} id="relatedInformationForm">
              <FormFieldset
                id="relatedLink"
                legend="Add related information"
                legendSize="m"
              >
                <FormFieldTextInput label="Title" name="description" />
                <FormFieldTextInput label="Link url" name="url" />
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
      </Formik>
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
            {release.publication.methodology &&
              (isEditing ? (
                <a>{release.publication.methodology.title}</a>
              ) : (
                <Link
                  to={`/methodologies/${release.publication.methodology.id}`}
                >
                  {release.publication.methodology.title}
                </Link>
              ))}
            {release.publication.externalMethodology &&
              (isEditing ? (
                <a>{release.publication.externalMethodology.title}</a>
              ) : (
                <Link to={release.publication.externalMethodology.url}>
                  {release.publication.externalMethodology.title}
                </Link>
              ))}
            {!release.publication.externalMethodology &&
              !release.publication.methodology &&
              'No methodology added.'}
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

export default RelatedInformationSection;
