import DetailsMenu from '@common/components/DetailsMenu';
import {
  Form,
  FormFieldRadioGroup,
  FormFieldset,
  FormGroup,
  Formik,
  FormTextSearchInput,
} from '@common/components/form';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import createErrorHelper from '@common/lib/validation/createErrorHelper';
import Yup from '@common/lib/validation/yup';
import { ThemeMeta } from '@common/services/tableBuilderService';
import { FormikProps } from 'formik';
import camelCase from 'lodash';
import React, { useState } from 'react';
import { InjectedWizardProps } from './Wizard';
import WizardStepFormActions from './WizardStepFormActions';
import WizardStepHeading from './WizardStepHeading';

interface FormValues {
  publicationId: string;
}

export type PublicationFormSubmitHandler = (values: FormValues) => void;

interface Props {
  onSubmit: PublicationFormSubmitHandler;
  options: ThemeMeta[];
  publicationId?: string;
}

const PublicationForm = (props: Props & InjectedWizardProps) => {
  const {
    options,
    onSubmit,
    isActive,
    goToNextStep,
    publicationId = '',
  } = props;
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
      enableReinitialize
      initialValues={{
        publicationId,
      }}
      onSubmit={async values => {
        await onSubmit(values);
        goToNextStep();
      }}
      validationSchema={Yup.object<FormValues>({
        publicationId: Yup.string().required('Choose publication'),
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
                      onKeyPress={event => {
                        if (event.key === 'Enter') {
                          event.preventDefault();
                        }
                      }}
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
                              publication.title.search(
                                new RegExp(searchTerm, 'i'),
                              ) > -1,
                          ),
                        );
                      })
                      .map(group => {
                        return (
                          <DetailsMenu
                            summary={group.title}
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
                                    publication.title.search(
                                      new RegExp(searchTerm, 'i'),
                                    ) > -1,
                                );
                              })
                              .map(topic => (
                                <DetailsMenu
                                  summary={topic.title}
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
                                    legend={`Choose option from ${topic.title}`}
                                    legendHidden
                                    small
                                    showError={false}
                                    name="publicationId"
                                    id={`${formId}-publicationId-${camelCase(
                                      topic.title,
                                    )}`}
                                    onChange={(event, option) => {
                                      setPublicationName(option.label);
                                    }}
                                    options={topic.publications
                                      .filter(
                                        publication =>
                                          publication.id ===
                                            values.publicationId ||
                                          publication.title.search(
                                            new RegExp(searchTerm, 'i'),
                                          ) > -1,
                                      )
                                      .map(publication => ({
                                        id: publication.id,
                                        label: publication.title,
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
