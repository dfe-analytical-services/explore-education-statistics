import DetailsMenu from '@common/components/DetailsMenu';
import {
  FormFieldRadioGroup,
  FormFieldset,
  FormGroup,
} from '@common/components/form';
import Form from '@common/components/form/Form';
import FormTextSearchInput from '@common/components/form/FormTextSearchInput';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import createErrorHelper from '@common/lib/validation/createErrorHelper';
import Yup from '@common/lib/validation/yup';
import { Formik, FormikProps } from 'formik';
import camelCase from 'lodash';
import React, { useState } from 'react';
import { InjectedWizardProps } from './Wizard';
import WizardStepFormActions from './WizardStepFormActions';
import WizardStepHeading from './WizardStepHeading';

interface FormValues {
  publicationId: string;
}

interface Props {
  onSubmit: (values: {
    publicationId: string;
    publicationName: string;
  }) => void;
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

const PublicationForm = (props: Props & InjectedWizardProps) => {
  const { options, onSubmit, isActive, goToNextStep } = props;
  const [publicationName, setPublicationName] = useState('');
  const [searchTerm, setSearchTerm] = useState('');

  const formId = 'publicationForm';

  const stepHeading = (
    <WizardStepHeading {...props} fieldsetHeading>
      Choose a publication
    </WizardStepHeading>
  );

  return (
    <Formik<FormValues>
      initialValues={{
        publicationId: '',
      }}
      onSubmit={async ({ publicationId }) => {
        await onSubmit({
          publicationId,
          publicationName,
        });
        goToNextStep();
      }}
      validationSchema={Yup.object<FormValues>({
        publicationId: Yup.string().required('Choose a publication'),
      })}
      render={(form: FormikProps<FormValues>) => {
        const { values } = form;
        const { getError } = createErrorHelper(form);

        return (
          <>
            {isActive ? (
              <Form {...form} id={formId}>
                <FormFieldset
                  error={getError('publicationId')}
                  id={`${formId}-publicationId`}
                  legend={stepHeading}
                >
                  <FormGroup>
                    <FormTextSearchInput
                      id={`${formId}-publicationIdSearch`}
                      label="Search publications"
                      name="publicationSearch"
                      onChange={event => setSearchTerm(event.target.value)}
                      width={20}
                    />
                  </FormGroup>

                  <FormGroup>
                    {options
                      .filter(group => {
                        return group.topics.some(topic =>
                          topic.publications.some(
                            publication =>
                              publication.id === values.publicationId ||
                              publication.name.search(
                                new RegExp(searchTerm, 'i'),
                              ) > -1,
                          ),
                        );
                      })
                      .map(group => {
                        return (
                          <DetailsMenu
                            summary={group.name}
                            key={group.id}
                            open={
                              searchTerm !== '' ||
                              group.topics.some(topic =>
                                topic.publications.some(
                                  publication =>
                                    publication.id === values.publicationId,
                                ),
                              )
                            }
                          >
                            {group.topics
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
                                    id={`${formId}-publicationId-${camelCase(
                                      topic.name,
                                    )}`}
                                    onChange={(event, option) => {
                                      setPublicationName(option.label);
                                    }}
                                    options={topic.publications
                                      .filter(
                                        publication =>
                                          publication.id ===
                                            values.publicationId ||
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
                  </FormGroup>
                </FormFieldset>

                <WizardStepFormActions {...props} form={form} formId={formId} />
              </Form>
            ) : (
              <>
                {stepHeading}
                <SummaryList noBorder>
                  <SummaryListItem term="Publication">
                    {publicationName}
                  </SummaryListItem>
                </SummaryList>
              </>
            )}
          </>
        );
      }}
    />
  );
};

export default PublicationForm;
