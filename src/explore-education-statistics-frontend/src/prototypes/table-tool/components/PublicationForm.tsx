import Button from '@common/components/Button';
import DetailsMenu from '@common/components/DetailsMenu';
import {
  FormFieldRadioGroup,
  FormFieldset,
  FormGroup,
} from '@common/components/form';
import Form from '@common/components/form/Form';
import FormTextSearchInput from '@common/components/form/FormTextSearchInput';
import createErrorHelper from '@common/lib/validation/createErrorHelper';
import Yup from '@common/lib/validation/yup';
import { InjectedWizardProps } from '@frontend/prototypes/table-tool/components/Wizard';
import { Formik, FormikProps } from 'formik';
import camelCase from 'lodash';
import React, { useState } from 'react';

interface FormValues {
  publicationId: string;
}

interface Props {
  onSubmit: (values: { publicationId: string }) => void;
  options: {
    id: string;
    name: string;
    topics: {
      id: string;
      name: string;
      publications: {
        id: string;
        name: string;
      }[];
    }[];
  }[];
}

const PublicationForm = ({
  options,
  onSubmit,
  goToNextStep,
}: Props & InjectedWizardProps) => {
  const [searchTerm, setSearchTerm] = useState('');

  return (
    <Formik<FormValues>
      initialValues={{
        publicationId: '',
      }}
      onSubmit={values => {
        onSubmit(values);
        goToNextStep();
      }}
      validationSchema={Yup.object<FormValues>({
        publicationId: Yup.string().required('Choose a publication'),
      })}
      render={(form: FormikProps<FormValues>) => {
        const { values } = form;
        const { getError } = createErrorHelper(form);

        return (
          <Form {...form} id="publicationForm">
            <FormGroup>
              <FormTextSearchInput
                id="publicationForm-publicationIdSearch"
                label="Search publications"
                name="publicationSearch"
                onChange={event => setSearchTerm(event.target.value)}
                width={20}
              />
            </FormGroup>

            <FormFieldset
              error={getError('publicationId')}
              id="publicationForm-publicationId"
              legend="Choose a publication"
              legendHidden
            >
              {options
                .filter(option => {
                  return option.topics.some(topic =>
                    topic.publications.some(
                      publication =>
                        publication.id === values.publicationId ||
                        publication.name.search(new RegExp(searchTerm, 'i')) >
                          -1,
                    ),
                  );
                })
                .map(option => {
                  return (
                    <DetailsMenu
                      summary={option.name}
                      key={option.id}
                      open={
                        searchTerm !== '' ||
                        option.topics.some(topic =>
                          topic.publications.some(
                            publication =>
                              publication.id === values.publicationId,
                          ),
                        )
                      }
                    >
                      {option.topics
                        .filter(topic => {
                          return topic.publications.find(
                            publication =>
                              publication.id === values.publicationId ||
                              publication.name.search(
                                new RegExp(searchTerm, 'i'),
                              ) > -1,
                          );
                        })
                        .map(topic => (
                          <DetailsMenu
                            summary={topic.name}
                            key={topic.id}
                            open={
                              searchTerm !== '' ||
                              topic.publications.some(
                                publication =>
                                  publication.id === values.publicationId,
                              )
                            }
                          >
                            <FormFieldRadioGroup
                              legend={`Choose option from ${topic.name}`}
                              legendHidden
                              small
                              showError={false}
                              name="publicationId"
                              id={`publicationForm-publicationId-${camelCase(
                                topic.name,
                              )}`}
                              options={topic.publications
                                .filter(
                                  publication =>
                                    publication.id === values.publicationId ||
                                    publication.name.search(
                                      new RegExp(searchTerm, 'i'),
                                    ) > -1,
                                )
                                .map(publication => ({
                                  id: publication.id,
                                  label: publication.name,
                                  value: publication.id,
                                }))}
                            />
                          </DetailsMenu>
                        ))}
                    </DetailsMenu>
                  );
                })}
            </FormFieldset>

            <FormGroup>
              <Button type="submit">Next step</Button>
            </FormGroup>
          </Form>
        );
      }}
    />
  );
};

export default PublicationForm;
