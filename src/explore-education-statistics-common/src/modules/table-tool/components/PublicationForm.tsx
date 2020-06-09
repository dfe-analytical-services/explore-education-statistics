import DetailsMenu from '@common/components/DetailsMenu';
import {
  Form,
  FormFieldRadioGroup,
  FormFieldset,
  FormGroup,
  FormTextSearchInput,
} from '@common/components/form';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import { ThemeMeta } from '@common/services/tableBuilderService';
import createErrorHelper from '@common/validation/createErrorHelper';
import Yup from '@common/validation/yup';
import { Formik } from 'formik';
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
  publicationTitle?: string;
}

const PublicationForm = (props: Props & InjectedWizardProps) => {
  const {
    options,
    onSubmit,
    isActive,
    goToNextStep,
    publicationId = '',
    publicationTitle = '',
  } = props;

  const [searchTerm, setSearchTerm] = useState('');
  const lowercaseSearchTerm = searchTerm.toLowerCase();

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
      validateOnBlur={false}
      validateOnChange={false}
      validationSchema={Yup.object<FormValues>({
        publicationId: Yup.string().required('Choose publication'),
      })}
      onSubmit={async values => {
        await onSubmit(values);
        goToNextStep();
      }}
    >
      {form => {
        const { values } = form;
        const { getError } = createErrorHelper(form);

        const filteredOptions: ThemeMeta[] = options
          .filter(theme =>
            theme.topics.some(topic =>
              topic.publications.some(
                publication =>
                  publication.id === values.publicationId ||
                  publication.title.toLowerCase().includes(lowercaseSearchTerm),
              ),
            ),
          )
          .map(group => ({
            ...group,
            topics: group.topics
              .filter(topic =>
                topic.publications.some(
                  publication =>
                    publication.id === values.publicationId ||
                    publication.title
                      .toLowerCase()
                      .includes(lowercaseSearchTerm),
                ),
              )
              .map(topic => ({
                ...topic,
                publications: topic.publications.filter(
                  publication =>
                    publication.id === values.publicationId ||
                    publication.title
                      .toLowerCase()
                      .includes(lowercaseSearchTerm),
                ),
              })),
          }));

        return (
          <>
            {isActive ? (
              <Form {...form} id={formId} showSubmitError>
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
                    <div aria-live="assertive">
                      {filteredOptions.length > 0 ? (
                        filteredOptions.map(theme => (
                          <DetailsMenu
                            jsRequired
                            summary={theme.title}
                            key={theme.id}
                            id={`${formId}-theme-${theme.id}`}
                            open={
                              searchTerm !== '' ||
                              theme.topics.some(topic =>
                                topic.publications.some(
                                  publication =>
                                    publication.id === values.publicationId,
                                ),
                              )
                            }
                          >
                            {theme.topics.map(topic => (
                              <DetailsMenu
                                summary={topic.title}
                                key={topic.id}
                                id={`${formId}-topic-${topic.id}`}
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
                                  disabled={form.isSubmitting}
                                  options={topic.publications.map(
                                    publication => ({
                                      label: publication.title,
                                      value: publication.id,
                                    }),
                                  )}
                                />
                              </DetailsMenu>
                            ))}
                          </DetailsMenu>
                        ))
                      ) : (
                        <p>No publications found</p>
                      )}
                    </div>
                  </FormGroup>
                </FormFieldset>

                <WizardStepFormActions {...props} form={form} formId={formId} />
              </Form>
            ) : (
              <>
                {stepHeading}
                <SummaryList noBorder>
                  <SummaryListItem term="Publication">
                    {publicationTitle}
                  </SummaryListItem>
                </SummaryList>
              </>
            )}
          </>
        );
      }}
    </Formik>
  );
};

export default PublicationForm;
